using FluentValidation;
using FluentValidation.Results;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Enums;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Messages;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services.Database;
using System.Linq.Dynamic.Core;
using SvadbeniSalon.Model.Constants;

namespace SvadbeniSalon.Services;

public interface ISvadbaService
    : IBaseCRUDService<SvadbaResponse, SvadbaSearchObject, SvadbaInsertRequest, SvadbaUpdateRequest>
{
    Task<SvadbaResponse> ChangeStatusAsync(int id, TerminStatusChangeRequest request);
    Task<List<DateTime>> GetZauzetiDatumiAsync();
}

public class SvadbaService
    : BaseCRUDService<Svadba, SvadbaResponse, SvadbaSearchObject, SvadbaInsertRequest, SvadbaUpdateRequest>,
        ISvadbaService
{

    private readonly IAuthenticatedUserAccessor _userAccessor;
    private readonly INotificationPublisher _notificationPublisher;

    public SvadbaService(
        SvadbeniSalonDbContext dbContext,
        IMapper mapper,
        IValidator<SvadbaInsertRequest> insertValidator,
        IValidator<SvadbaUpdateRequest> updateValidator,
        IAuthenticatedUserAccessor userAccessor,
        INotificationPublisher notificationPublisher)
        : base(dbContext, mapper, insertValidator, updateValidator)
    {
        _userAccessor = userAccessor;
        _notificationPublisher = notificationPublisher;
    }

    public override async Task<PageResult<SvadbaResponse>> GetAllAsync(SvadbaSearchObject? search = null)
    {
        search ??= new SvadbaSearchObject();
        if (string.IsNullOrWhiteSpace(search.SortBy))
        {
            search.SortBy = "DatumSvadbe desc";
        }

        var query = await IncludeRelatedEntitiesAsync(search, _dbContext.Set<Svadba>().AsQueryable());
        var page = await ExecutePagedAsync(query, search, s => MapSvadba(s));

        var ids = page.Items.Select(i => i.Id).ToList();
        if (ids.Count > 0)
        {
            var stats = await _dbContext.Rate
                .Where(r => ids.Contains(r.SvadbaId))
                .GroupBy(r => r.SvadbaId)
                .Select(g => new { SvadbaId = g.Key, Sum = g.Sum(x => x.Iznos), Count = g.Count() })
                .ToDictionaryAsync(x => x.SvadbaId, x => x);

            foreach (var item in page.Items)
            {
                if (stats.TryGetValue(item.Id, out var s))
                {
                    item.UplaceniIznos = s.Sum;
                    item.BrojEvidentiranihUplata = s.Count;
                }

                item.PreostaliIznos = Math.Max(0, item.CijenaPonude - item.UplaceniIznos);
                item.IsFullyPaid = item.CijenaPonude > 0 && item.UplaceniIznos >= item.CijenaPonude;
            }
        }

        return page;
    }

    protected override async Task<IQueryable<Svadba>> IncludeRelatedEntitiesAsync(
        SvadbaSearchObject? search,
        IQueryable<Svadba> query = null!)
    {
        query ??= _dbContext.Set<Svadba>();
        return await Task.FromResult(query.Include(s => s.Ponuda).Include(s => s.User));
    }

    protected override IQueryable<Svadba> ApplyFilters(IQueryable<Svadba> query, SvadbaSearchObject? search)
    {
        if (_userAccessor.IsSalonStaff())
        {
            if (search?.UserId.HasValue == true)
            {
                query = query.Where(s => s.UserId == search.UserId.Value);
            }
        }
        else
        {
            var userId = _userAccessor.GetUserId();
            if (!userId.HasValue)
            {
                return query.Where(_ => false);
            }

            query = query.Where(s => s.UserId == userId.Value);
        }

        if (search?.PonudaId.HasValue == true)
        {
            query = query.Where(s => s.PonudaId == search.PonudaId.Value);
        }

        if (search?.Status.HasValue == true)
        {
            query = query.Where(s => (int)s.Status == search.Status.Value);
        }

        if (search?.DatumOd.HasValue == true)
        {
            query = query.Where(s => s.DatumSvadbe >= search.DatumOd.Value.Date);
        }

        if (search?.DatumDo.HasValue == true)
        {
            query = query.Where(s => s.DatumSvadbe <= search.DatumDo.Value.Date);
        }

        return query;
    }

    public override async Task<SvadbaResponse> GetByIdAsync(int id)
    {
        var entity = await LoadEntityAsync(id);
        if (entity == null || !CanAccess(entity))
        {
            throw new NotFoundException($"{nameof(Svadba)} with id {id} not found.");
        }

        return await MapSvadbaAsync(entity);
    }

    public override async Task<SvadbaResponse> InsertAsync(SvadbaInsertRequest request)
    {
        var targetUserId = await ResolveTargetUserIdAsync(request.UserId);

        var validationResult = await _insertValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
            throw new FluentValidation.ValidationException(errors);
        }

        await ValidateBusinessRulesAsync(
            request.PonudaId,
            request.DatumSvadbe,
            request.Vrijeme,
            targetUserId,
            excludeId: null,
            previousWeddingDate: null);

        var dogovorenaCijena = await ResolveOfferPriceAsync(request.PonudaId);

        var entity = new Svadba
        {
            UserId = targetUserId,
            PonudaId = request.PonudaId,
            DogovorenaCijena = dogovorenaCijena,
            DatumSvadbe = request.DatumSvadbe.Date,
            Vrijeme = request.Vrijeme,
            BrojGostiju = request.BrojGostiju,
            BrojRata = request.BrojRata,
            Napomena = request.Napomena?.Trim(),
            Status = TerminStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Svadbe.Add(entity);
        await _dbContext.SaveChangesAsync();

        var loaded = await LoadEntityAsync(entity.Id) ?? entity;
        var mapped = await MapSvadbaAsync(loaded);

        var customerEmail = loaded.User?.Email?.Trim();
        var vrijemeText = $"{loaded.Vrijeme.Hours:D2}:{loaded.Vrijeme.Minutes:D2}";
        await _notificationPublisher.PublishAsync(new NotificationMessage
        {
            UserId = targetUserId,
            RecipientEmail = customerEmail,
            Kind = "NewSvadba",
            Title = "Rezervacija svadbe primljena",
            Body =
                $"Poštovani {mapped.KorisnikIme},\n\n" +
                $"Vaša rezervacija svadbe za {mapped.DatumSvadbe:dd.MM.yyyy} u {vrijemeText} " +
                $"(ponuda: {mapped.PonudaNaziv}, {mapped.BrojGostiju} gostiju) je na čekanju. " +
                "Salon će vam uskoro potvrditi termin.\n\n" +
                "Svadbeni Salon Rimac",
            AdminBody =
                $"Nova rezervacija svadbe (status: na čekanju).\n\n" +
                $"Klijent: {mapped.KorisnikIme} (#{targetUserId})\n" +
                $"Email: {customerEmail ?? "—"}\n" +
                $"Datum: {mapped.DatumSvadbe:dd.MM.yyyy} u {vrijemeText}\n" +
                $"Ponuda: {mapped.PonudaNaziv}\n" +
                $"Broj gostiju: {mapped.BrojGostiju}",
            CreatedAt = DateTime.UtcNow,
        });

        return mapped;
    }

    public override async Task<SvadbaResponse> UpdateAsync(int id, SvadbaUpdateRequest request)
    {
        var userId = _userAccessor.GetUserId()
                     ?? throw new InvalidOperationException("User id claim is missing.");
        var isStaff = _userAccessor.IsSalonStaff();

        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
            throw new FluentValidation.ValidationException(errors);
        }

        var entity = await _dbContext.Svadbe.FindAsync(id);
        if (entity == null || (!isStaff && entity.UserId != userId))
        {
            throw new NotFoundException($"{nameof(Svadba)} with id {id} not found.");
        }

        if (entity.Status != TerminStatus.Pending)
        {
            throw new ClientException("Rezervaciju možete mijenjati samo dok je na čekanju.");
        }

        await ValidateBusinessRulesAsync(
            request.PonudaId,
            request.DatumSvadbe,
            request.Vrijeme,
            entity.UserId,
            excludeId: id,
            previousWeddingDate: entity.DatumSvadbe);

        var existingPayments = await _dbContext.Rate.CountAsync(r => r.SvadbaId == id);
        if (request.BrojRata < existingPayments)
        {
            throw new ClientException(
                $"Broj rata ne može biti manji od već evidentiranih uplata ({existingPayments}).");
        }

        if (entity.PonudaId != request.PonudaId)
        {
            entity.DogovorenaCijena = await ResolveOfferPriceAsync(request.PonudaId);
        }

        entity.PonudaId = request.PonudaId;
        entity.DatumSvadbe = request.DatumSvadbe.Date;
        entity.Vrijeme = request.Vrijeme;
        entity.BrojGostiju = request.BrojGostiju;
        entity.BrojRata = request.BrojRata;
        entity.Napomena = request.Napomena?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var loaded = await LoadEntityAsync(id) ?? entity;
        var mapped = await MapSvadbaAsync(loaded);
        var customerEmail = loaded.User?.Email?.Trim();
        var vrijemeText = $"{loaded.Vrijeme.Hours:D2}:{loaded.Vrijeme.Minutes:D2}";
        await _notificationPublisher.PublishAsync(new NotificationMessage
        {
            UserId = loaded.UserId,
            RecipientEmail = customerEmail,
            Kind = "SvadbaUpdated",
            Title = "Rezervacija svadbe ažurirana",
            Body =
                $"Poštovani {mapped.KorisnikIme},\n\n" +
                $"Vaša rezervacija svadbe je izmijenjena.\n" +
                $"Novi termin: {mapped.DatumSvadbe:dd.MM.yyyy} u {vrijemeText}\n" +
                $"Ponuda: {mapped.PonudaNaziv}, {mapped.BrojGostiju} gostiju.\n\n" +
                "Svadbeni Salon Rimac",
            AdminBody =
                $"Rezervacija svadbe je ažurirana (status: na čekanju).\n\n" +
                $"Klijent: {mapped.KorisnikIme} (#{loaded.UserId})\n" +
                $"Email: {customerEmail ?? "—"}\n" +
                $"Datum: {mapped.DatumSvadbe:dd.MM.yyyy} u {vrijemeText}\n" +
                $"Ponuda: {mapped.PonudaNaziv}\n" +
                $"Broj gostiju: {mapped.BrojGostiju}",
            CreatedAt = DateTime.UtcNow,
        });

        return mapped;
    }

    public override async Task DeleteAsync(int id)
    {
        await ChangeStatusAsync(id, new TerminStatusChangeRequest
        {
            Status = TerminStatus.Cancelled,
            Razlog = "Otkazano na zahtjev korisnika.",
        });
    }

    public async Task<SvadbaResponse> ChangeStatusAsync(int id, TerminStatusChangeRequest request)
    {
        var actorId = _userAccessor.GetUserId()
                      ?? throw new UnauthorizedAccessException("Niste prijavljeni.");

        var entity = await _dbContext.Svadbe
            .Include(s => s.Rate)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null || !CanAccess(entity))
        {
            throw new NotFoundException($"{nameof(Svadba)} with id {id} not found.");
        }

        var isStaff = _userAccessor.IsSalonStaff();
        if (!TerminStatusMachine.CanTransition(entity.Status, request.Status, isStaff))
        {
            throw new ClientException(
                $"Prijelaz statusa {TerminStatusMachine.StatusLabel(entity.Status)} → " +
                $"{TerminStatusMachine.StatusLabel(request.Status)} nije dozvoljen.");
        }

        var razlog = request.Razlog?.Trim();
        if (TerminStatusMachine.RequiresReason(request.Status))
        {
            if (string.IsNullOrWhiteSpace(razlog) || razlog.Length < 3)
            {
                throw new ClientException(
                    "Razlog otkazivanja je obavezan (najmanje 3 karaktera).");
            }
        }

        var paid = entity.Rate.Sum(r => r.Iznos);
        if (request.Status == TerminStatus.Cancelled && paid > 0)
        {
            if (!isStaff)
            {
                throw new ClientException(
                    "Rezervaciju s uplatama može otkazati samo zaposlenik, uz evidentirani povrat.");
            }

            if (razlog == null
                || !razlog.Contains("povrat", StringComparison.OrdinalIgnoreCase))
            {
                throw new ClientException(
                    "Za otkazivanje plaćene rezervacije u razlogu navedite tok povrata " +
                    "(npr. „povrat uplate dogovoren s klijentom“).");
            }
        }

        if (request.Status == TerminStatus.Completed)
        {
            EnsureCanComplete(entity, paid);
        }

        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.StatusChangedByUserId = actorId;
        entity.StatusChangedAt = DateTime.UtcNow;
        entity.StatusChangeReason = string.IsNullOrWhiteSpace(razlog) ? null : razlog;

        await _dbContext.SaveChangesAsync();

        var loaded = await LoadEntityAsync(id) ?? entity;
        var mapped = await MapSvadbaAsync(loaded);
        var customerEmail = loaded.User?.Email?.Trim();

        NotificationMessage? notification = request.Status switch
        {
            TerminStatus.Confirmed => new NotificationMessage
            {
                UserId = mapped.UserId,
                RecipientEmail = customerEmail,
                Kind = "SvadbaStatus",
                Title = "Rezervacija svadbe potvrđena",
                Body =
                    $"Vaša rezervacija za {mapped.DatumSvadbe:dd.MM.yyyy.} ({mapped.PonudaNaziv}) je potvrđena.",
                CreatedAt = DateTime.UtcNow,
            },
            TerminStatus.Cancelled => new NotificationMessage
            {
                UserId = mapped.UserId,
                RecipientEmail = customerEmail,
                Kind = "SvadbaStatus",
                Title = "Rezervacija svadbe otkazana",
                Body =
                    $"Rezervacija za {mapped.DatumSvadbe:dd.MM.yyyy.} je otkazana." +
                    (string.IsNullOrWhiteSpace(razlog) ? "" : $" Razlog: {razlog}"),
                CreatedAt = DateTime.UtcNow,
            },
            TerminStatus.Completed => new NotificationMessage
            {
                UserId = mapped.UserId,
                RecipientEmail = customerEmail,
                Kind = "SvadbaStatus",
                Title = "Svadba završena",
                Body =
                    $"Vaša svadba ({mapped.PonudaNaziv}) je označena kao završena. Možete ostaviti recenziju.",
                CreatedAt = DateTime.UtcNow,
            },
            _ => null,
        };

        if (notification != null)
        {
            await _notificationPublisher.PublishAsync(notification);
        }

        return mapped;
    }

    public async Task<List<DateTime>> GetZauzetiDatumiAsync()
    {
        return await _dbContext.Svadbe
            .Where(s => TerminStatusMachine.ActiveStatuses.Contains(s.Status))
            .Select(s => s.DatumSvadbe.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }

    private async Task<int> ResolveTargetUserIdAsync(int? requestedUserId)
    {
        if (_userAccessor.IsSalonStaff())
        {
            if (!requestedUserId.HasValue || requestedUserId.Value <= 0)
            {
                throw new ClientException("Klijent je obavezan.");
            }

            var isCustomer = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AnyAsync(u =>
                    u.Id == requestedUserId.Value
                    && u.IsActive
                    && u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Customer));

            if (!isCustomer)
            {
                throw new ClientException("Odabrani korisnik nije klijent.");
            }

            return requestedUserId.Value;
        }

        return _userAccessor.GetUserId()
               ?? throw new InvalidOperationException("User id claim is missing.");
    }

    private async Task ValidateBusinessRulesAsync(
        int ponudaId,
        DateTime datumSvadbe,
        TimeSpan vrijeme,
        int userId,
        int? excludeId,
        DateTime? previousWeddingDate)
    {
        EnsureWeddingNotInPast(datumSvadbe, vrijeme);
        EnsureWeddingLeadTime(datumSvadbe, previousWeddingDate);

        var ponudaExists = await _dbContext.Ponude.AnyAsync(p => p.Id == ponudaId && p.IsActive);
        if (!ponudaExists)
        {
            throw new ClientException("Odabrani paket nije dostupan.");
        }

        var userHasActive = await _dbContext.Svadbe.AnyAsync(s =>
            s.UserId == userId
            && TerminStatusMachine.ActiveStatuses.Contains(s.Status)
            && (!excludeId.HasValue || s.Id != excludeId.Value));

        if (userHasActive)
        {
            throw new ClientException("Već imate aktivnu rezervaciju svadbe.");
        }

        var dateTaken = await _dbContext.Svadbe.AnyAsync(s =>
            s.DatumSvadbe == datumSvadbe.Date
            && TerminStatusMachine.ActiveStatuses.Contains(s.Status)
            && (!excludeId.HasValue || s.Id != excludeId.Value));

        if (dateTaken)
        {
            throw new ClientException("Termin za odabrani datum je već zauzet.");
        }
    }

    private static void EnsureWeddingNotInPast(DateTime datumSvadbe, TimeSpan vrijeme)
    {
        if (SalonClock.IsInPast(datumSvadbe, vrijeme))
        {
            throw new ClientException(
                "Termin svadbe (datum i vrijeme) ne smije biti u prošlosti.");
        }
    }

    private static void EnsureWeddingLeadTime(DateTime datumSvadbe, DateTime? previousWeddingDate)
    {
        if (previousWeddingDate.HasValue
            && previousWeddingDate.Value.Date == datumSvadbe.Date)
        {
            return;
        }

        if (datumSvadbe.Date < SalonClock.MinWeddingBookingDate)
        {
            throw new ClientException(
                $"Rezervacija mora biti najmanje {SalonClock.MinWeddingLeadDays} dana unaprijed.");
        }
    }

    private static void EnsureCanComplete(Svadba entity, decimal paid)
    {
        if (SalonClock.IsInFuture(entity.DatumSvadbe, entity.Vrijeme))
        {
            throw new ClientException(
                "Svadbu možete označiti završenom tek nakon što je termin (datum i vrijeme) prošao.");
        }

        var obaveza = entity.DogovorenaCijena;
        if (obaveza <= 0)
        {
            throw new ClientException(
                "Svadba nema dogovorenu cijenu. Provjerite rezervaciju prije završetka.");
        }

        if (paid < obaveza)
        {
            throw new ClientException(
                $"Svadbu možete završiti tek kada je u potpunosti plaćena. " +
                $"Uplaćeno: {paid:0.00} KM, obaveza: {obaveza:0.00} KM.");
        }
    }

    private bool CanAccess(Svadba entity)
    {
        if (_userAccessor.IsSalonStaff())
        {
            return true;
        }

        var userId = _userAccessor.GetUserId();
        return userId.HasValue && entity.UserId == userId.Value;
    }

    private async Task<decimal> ResolveOfferPriceAsync(int ponudaId)
    {
        var cijena = await _dbContext.Ponude
            .Where(p => p.Id == ponudaId && p.IsActive)
            .Select(p => (decimal?)p.Cijena)
            .FirstOrDefaultAsync();

        if (!cijena.HasValue)
        {
            throw new ClientException("Odabrana ponuda ne postoji ili nije aktivna.");
        }

        return cijena.Value;
    }

    private static decimal AgreedPrice(Svadba entity)
        => entity.DogovorenaCijena > 0
            ? entity.DogovorenaCijena
            : entity.Ponuda?.Cijena ?? 0;

    private async Task<Svadba?> LoadEntityAsync(int id)
    {
        return await _dbContext.Svadbe
            .Include(s => s.Ponuda)
            .Include(s => s.User)
            .Include(s => s.StatusChangedByUser)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    private async Task<SvadbaResponse> MapSvadbaAsync(Svadba entity)
    {
        var uplate = await _dbContext.Rate
            .Where(r => r.SvadbaId == entity.Id)
            .GroupBy(_ => 1)
            .Select(g => new { Sum = g.Sum(x => x.Iznos), Count = g.Count() })
            .FirstOrDefaultAsync();

        return MapSvadba(entity, uplate?.Sum ?? 0, uplate?.Count ?? 0);
    }

    private static SvadbaResponse MapSvadba(
        Svadba entity,
        decimal uplaceniIznos = 0,
        int brojEvidentiranihUplata = 0)
    {
        var cijena = AgreedPrice(entity);
        return new SvadbaResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            KorisnikIme = entity.User != null
                ? $"{entity.User.FirstName} {entity.User.LastName}".Trim()
                : string.Empty,
            PonudaId = entity.PonudaId,
            PonudaNaziv = entity.Ponuda?.Naziv ?? string.Empty,
            CijenaPonude = cijena,
            UplaceniIznos = uplaceniIznos,
            PreostaliIznos = Math.Max(0, cijena - uplaceniIznos),
            IsFullyPaid = cijena > 0 && uplaceniIznos >= cijena,
            BrojEvidentiranihUplata = brojEvidentiranihUplata,
            DatumSvadbe = entity.DatumSvadbe,
            Vrijeme = entity.Vrijeme,
            BrojGostiju = entity.BrojGostiju,
            BrojRata = entity.BrojRata,
            Status = (int)entity.Status,
            Napomena = entity.Napomena,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            StatusChangedByUserId = entity.StatusChangedByUserId,
            StatusChangedByName = entity.StatusChangedByUser != null
                ? $"{entity.StatusChangedByUser.FirstName} {entity.StatusChangedByUser.LastName}".Trim()
                : null,
            StatusChangedAt = entity.StatusChangedAt,
            StatusChangeReason = entity.StatusChangeReason,
        };
    }
}
