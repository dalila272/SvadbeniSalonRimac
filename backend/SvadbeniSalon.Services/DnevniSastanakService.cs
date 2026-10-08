using FluentValidation;
using FluentValidation.Results;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Messages;
using SvadbeniSalon.Model.Enums;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services.Database;
using System.Linq.Dynamic.Core;
using SvadbeniSalon.Model.Constants;

namespace SvadbeniSalon.Services;

public interface IDnevniSastanakService
    : IBaseCRUDService<DnevniSastanakResponse, DnevniSastanakSearchObject, DnevniSastanakInsertRequest, DnevniSastanakUpdateRequest>
{
    Task<List<DateTime>> GetZauzetiDatumiAsync();
    Task<DnevniSastanakResponse> ChangeStatusAsync(int id, TerminStatusChangeRequest request);
}

public class DnevniSastanakService
    : BaseCRUDService<DnevniSastanak, DnevniSastanakResponse, DnevniSastanakSearchObject, DnevniSastanakInsertRequest, DnevniSastanakUpdateRequest>,
        IDnevniSastanakService
{
    private readonly IAuthenticatedUserAccessor _userAccessor;
    private readonly INotificationPublisher _notificationPublisher;

    public DnevniSastanakService(
        SvadbeniSalonDbContext dbContext,
        IMapper mapper,
        IValidator<DnevniSastanakInsertRequest> insertValidator,
        IValidator<DnevniSastanakUpdateRequest> updateValidator,
        IAuthenticatedUserAccessor userAccessor,
        INotificationPublisher notificationPublisher)
        : base(dbContext, mapper, insertValidator, updateValidator)
    {
        _userAccessor = userAccessor;
        _notificationPublisher = notificationPublisher;
    }

    public override async Task<PageResult<DnevniSastanakResponse>> GetAllAsync(DnevniSastanakSearchObject? search = null)
    {
        search ??= new DnevniSastanakSearchObject();
        if (string.IsNullOrWhiteSpace(search.SortBy))
        {
            search.SortBy = "DatumSastanka asc";
        }

        search.IncludeTotalCount ??= true;
        var query = _dbContext.DnevniSastanci
            .Include(s => s.User)
            .AsQueryable();

        return await ExecutePagedAsync(query, search, MapEntity);
    }

    protected override IQueryable<DnevniSastanak> ApplyFilters(
        IQueryable<DnevniSastanak> query,
        DnevniSastanakSearchObject? search)
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

        if (search?.Status.HasValue == true)
        {
            query = query.Where(s => (int)s.Status == search.Status.Value);
        }

        if (search?.DatumOd.HasValue == true)
        {
            query = query.Where(s => s.DatumSastanka >= search.DatumOd.Value);
        }

        if (search?.DatumDo.HasValue == true)
        {
            query = query.Where(s => s.DatumSastanka <= search.DatumDo.Value);
        }

        return query;
    }

    public override async Task<DnevniSastanakResponse> GetByIdAsync(int id)
    {
        var entity = await LoadEntityAsync(id);
        if (entity == null || !CanAccess(entity))
        {
            throw new NotFoundException($"{nameof(DnevniSastanak)} with id {id} not found.");
        }

        return MapEntity(entity);
    }

    public override async Task<DnevniSastanakResponse> InsertAsync(DnevniSastanakInsertRequest request)
    {
        var (userId, kontaktIme) = await ResolveInsertContactAsync(request);

        var validationResult = await _insertValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
            throw new FluentValidation.ValidationException(errors);
        }

        await EnsureSlotAvailableAsync(request.DatumSastanka, null);

        var entity = new DnevniSastanak
        {
            UserId = userId,
            KontaktIme = kontaktIme,
            DatumSastanka = request.DatumSastanka,
            Napomena = request.Napomena?.Trim(),
            Status = TerminStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.DnevniSastanci.Add(entity);
        await _dbContext.SaveChangesAsync();

        var loaded = await LoadEntityAsync(entity.Id) ?? entity;
        var mapped = MapEntity(loaded);

        if (mapped.UserId.HasValue)
        {
            await _notificationPublisher.PublishAsync(new NotificationMessage
            {
                UserId = mapped.UserId.Value,
                RecipientEmail = loaded.User?.Email?.Trim(),
                Kind = "DnevniSastanak",
                Title = "Dnevni sastanak rezervisan",
                Body = $"Vaš termin {mapped.DatumSastanka:dd.MM.yyyy HH:mm} je na čekanju. Salon će vam uskoro potvrditi.",
                CreatedAt = DateTime.UtcNow,
            });
        }

        return mapped;
    }

    public override async Task<DnevniSastanakResponse> UpdateAsync(int id, DnevniSastanakUpdateRequest request)
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

        var entity = await _dbContext.DnevniSastanci.FindAsync(id);
        if (entity == null || (!isStaff && entity.UserId != userId))
        {
            throw new NotFoundException($"{nameof(DnevniSastanak)} with id {id} not found.");
        }

        if (entity.Status != TerminStatus.Pending)
        {
            throw new ClientException("Sastanak možete mijenjati samo dok je na čekanju.");
        }

        await EnsureSlotAvailableAsync(request.DatumSastanka, id);

        entity.DatumSastanka = request.DatumSastanka;
        entity.Napomena = request.Napomena?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var loaded = await LoadEntityAsync(id) ?? entity;
        var mapped = MapEntity(loaded);
        if (mapped.UserId.HasValue)
        {
            await _notificationPublisher.PublishAsync(new NotificationMessage
            {
                UserId = mapped.UserId.Value,
                RecipientEmail = loaded.User?.Email?.Trim(),
                Kind = "DnevniSastanakUpdated",
                Title = "Dnevni sastanak ažuriran",
                Body =
                    $"Vaš dnevni sastanak je izmijenjen. Novi termin: {mapped.DatumSastanka:dd.MM.yyyy HH:mm}.",
                CreatedAt = DateTime.UtcNow,
            });
        }

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

    public async Task<List<DateTime>> GetZauzetiDatumiAsync()
    {
        return await _dbContext.DnevniSastanci
            .Where(s => TerminStatusMachine.ActiveStatuses.Contains(s.Status))
            .Select(s => s.DatumSastanka)
            .ToListAsync();
    }

    public async Task<DnevniSastanakResponse> ChangeStatusAsync(int id, TerminStatusChangeRequest request)
    {
        var actorId = _userAccessor.GetUserId()
                      ?? throw new UnauthorizedAccessException("Niste prijavljeni.");

        var entity = await _dbContext.DnevniSastanci.FindAsync(id);
        if (entity == null || !CanAccess(entity))
        {
            throw new NotFoundException($"{nameof(DnevniSastanak)} with id {id} not found.");
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

        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.StatusChangedByUserId = actorId;
        entity.StatusChangedAt = DateTime.UtcNow;
        entity.StatusChangeReason = string.IsNullOrWhiteSpace(razlog) ? null : razlog;

        await _dbContext.SaveChangesAsync();

        var loaded = await LoadEntityAsync(id) ?? entity;
        var mapped = MapEntity(loaded);
        var customerEmail = loaded.User?.Email?.Trim();

        if (mapped.UserId.HasValue)
        {
            NotificationMessage? notification = request.Status switch
            {
                TerminStatus.Confirmed => new NotificationMessage
                {
                    UserId = mapped.UserId.Value,
                    RecipientEmail = customerEmail,
                    Kind = "DnevniSastanakStatus",
                    Title = "Sastanak potvrđen",
                    Body =
                        $"Vaš dnevni sastanak {mapped.DatumSastanka:dd.MM.yyyy u HH:mm} je potvrđen. Vidimo se u salonu!",
                    CreatedAt = DateTime.UtcNow,
                },
                TerminStatus.Cancelled => new NotificationMessage
                {
                    UserId = mapped.UserId.Value,
                    RecipientEmail = customerEmail,
                    Kind = "DnevniSastanakStatus",
                    Title = "Sastanak otkazan",
                    Body =
                        $"Termin {mapped.DatumSastanka:dd.MM.yyyy u HH:mm} je otkazan." +
                        (string.IsNullOrWhiteSpace(razlog) ? "" : $" Razlog: {razlog}"),
                    CreatedAt = DateTime.UtcNow,
                },
                TerminStatus.Completed => new NotificationMessage
                {
                    UserId = mapped.UserId.Value,
                    RecipientEmail = customerEmail,
                    Kind = "DnevniSastanakStatus",
                    Title = "Sastanak završen",
                    Body =
                        $"Vaš sastanak {mapped.DatumSastanka:dd.MM.yyyy u HH:mm} je označen kao završen.",
                    CreatedAt = DateTime.UtcNow,
                },
                _ => null,
            };

            if (notification != null)
            {
                await _notificationPublisher.PublishAsync(notification);
            }
        }

        return mapped;
    }

    private async Task EnsureSlotAvailableAsync(DateTime datumSastanka, int? excludeId)
    {
        var activeMeetings = await _dbContext.DnevniSastanci
            .Where(s =>
                s.DatumSastanka.Date == datumSastanka.Date
                && TerminStatusMachine.ActiveStatuses.Contains(s.Status)
                && (!excludeId.HasValue || s.Id != excludeId.Value))
            .Select(s => s.DatumSastanka)
            .ToListAsync();

        if (activeMeetings.Any(existing =>
                DnevniSastanakSlotRules.Overlaps(existing, datumSastanka)))
        {
            throw new ClientException("Odabrani termin se preklapa s postojećim sastankom.");
        }
    }

    private bool CanAccess(DnevniSastanak entity)
    {
        if (_userAccessor.IsSalonStaff())
        {
            return true;
        }

        var userId = _userAccessor.GetUserId();
        return userId.HasValue && entity.UserId == userId.Value;
    }

    private async Task<DnevniSastanak?> LoadEntityAsync(int id)
    {
        return await _dbContext.DnevniSastanci
            .Include(s => s.User)
            .Include(s => s.StatusChangedByUser)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    private async Task<(int? UserId, string? KontaktIme)> ResolveInsertContactAsync(
        DnevniSastanakInsertRequest request)
    {
        if (!_userAccessor.IsSalonStaff())
        {
            var userId = _userAccessor.GetUserId()
                         ?? throw new InvalidOperationException("User id claim is missing.");
            return (userId, null);
        }

        var hasUser = request.UserId.HasValue && request.UserId.Value > 0;
        var kontaktIme = request.KontaktIme?.Trim();

        if (hasUser && !string.IsNullOrWhiteSpace(kontaktIme))
        {
            throw new ClientException("Odaberite klijenta iz aplikacije ili unesite ime gosta, ne oba.");
        }

        if (!hasUser && string.IsNullOrWhiteSpace(kontaktIme))
        {
            throw new ClientException("Odaberite klijenta ili unesite ime gosta.");
        }

        if (hasUser)
        {
            var isCustomer = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AnyAsync(u =>
                    u.Id == request.UserId!.Value
                    && u.IsActive
                    && u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Customer));

            if (!isCustomer)
            {
                throw new ClientException("Odabrani korisnik nije klijent.");
            }

            return (request.UserId!.Value, null);
        }

        return (null, kontaktIme);
    }

    private static string GetDisplayName(DnevniSastanak entity)
    {
        if (entity.User != null)
        {
            var name = $"{entity.User.FirstName} {entity.User.LastName}".Trim();
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
        }

        return entity.KontaktIme?.Trim() ?? string.Empty;
    }

    private static DnevniSastanakResponse MapEntity(DnevniSastanak entity)
    {
        return new DnevniSastanakResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            KorisnikIme = GetDisplayName(entity),
            KontaktIme = entity.KontaktIme,
            DatumSastanka = entity.DatumSastanka,
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
