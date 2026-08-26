using FluentValidation;
using FluentValidation.Results;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Enums;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services.Database;

namespace SvadbeniSalon.Services;

public interface IRacunService
{
    Task<PageResult<RacunResponse>> GetAllAsync(RacunSearchObject? search = null);
    Task<RacunResponse> CreateAsync(RacunInsertRequest request);
    Task<byte[]> GetPdfAsync(int id);
}

public class RacunService : IRacunService
{
    private readonly SvadbeniSalonDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<RacunInsertRequest> _insertValidator;
    private readonly IAuthenticatedUserAccessor _userAccessor;

    public RacunService(
        SvadbeniSalonDbContext dbContext,
        IMapper mapper,
        IValidator<RacunInsertRequest> insertValidator,
        IAuthenticatedUserAccessor userAccessor)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _insertValidator = insertValidator;
        _userAccessor = userAccessor;
    }

    public async Task<PageResult<RacunResponse>> GetAllAsync(RacunSearchObject? search = null)
    {
        search ??= new RacunSearchObject();
        search.NormalizePaging();
        search.IncludeTotalCount ??= true;

        if (!_userAccessor.IsSalonStaff())
        {
            return new PageResult<RacunResponse> { Items = [], TotalCount = 0 };
        }

        var query = _dbContext.Racuni
            .Include(r => r.Svadba)
            .ThenInclude(s => s.User)
            .Include(r => r.Svadba)
            .ThenInclude(s => s.Ponuda)
            .AsQueryable();

        if (search.SvadbaId.HasValue)
        {
            query = query.Where(r => r.SvadbaId == search.SvadbaId.Value);
        }

        query = query.OrderByDescending(r => r.DatumIzdavanja);

        int? totalCount = null;
        if (search.IncludeTotalCount == true)
        {
            totalCount = await query.CountAsync();
        }

        var entities = await query
            .Skip((search.Page!.Value - 1) * search.PageSize!.Value)
            .Take(search.PageSize!.Value)
            .ToListAsync();

        return new PageResult<RacunResponse>
        {
            Items = entities.Select(MapResponse).ToList(),
            TotalCount = totalCount,
        };
    }

    public async Task<RacunResponse> CreateAsync(RacunInsertRequest request)
    {
        if (!_userAccessor.IsSalonStaff())
        {
            throw new ClientException("Samo zaposlenici mogu izdavati račune.");
        }

        var validationResult = await _insertValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
            throw new FluentValidation.ValidationException(errors);
        }

        var svadba = await _dbContext.Svadbe
            .Include(s => s.Ponuda)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == request.SvadbaId);

        if (svadba == null)
        {
            throw new ClientException("Svadba nije pronađena.");
        }

        if (svadba.Status == TerminStatus.Cancelled)
        {
            throw new ClientException("Račun se ne može izdati za otkazanu svadbu.");
        }

        if (svadba.Ponuda == null)
        {
            throw new ClientException("Svadba nema povezanu ponudu.");
        }

        var uplate = await _dbContext.Rate
            .Where(r => r.SvadbaId == request.SvadbaId)
            .OrderBy(r => r.DatumUplate)
            .ToListAsync();

        if (uplate.Count == 0)
        {
            throw new ClientException("Prije izdavanja računa evidentirajte barem jednu uplatu.");
        }

        var uplaceno = uplate.Sum(r => r.Iznos);
        var now = DateTime.UtcNow;
        var brojRacuna = await GenerateBrojRacunaAsync(now.Year);

        var entity = new Racun
        {
            SvadbaId = request.SvadbaId,
            BrojRacuna = brojRacuna,
            DatumIzdavanja = now,
            UkupanIznos = svadba.Ponuda.Cijena,
            UplaceniIznos = uplaceno,
            KreiraoUserId = _userAccessor.GetUserId(),
            CreatedAt = now,
        };

        _dbContext.Racuni.Add(entity);
        await _dbContext.SaveChangesAsync();

        entity.Svadba = svadba;
        return MapResponse(entity);
    }

    public async Task<byte[]> GetPdfAsync(int id)
    {
        if (!_userAccessor.IsSalonStaff())
        {
            throw new ClientException("Samo zaposlenici mogu preuzimati račune.");
        }

        var racun = await _dbContext.Racuni
            .Include(r => r.Svadba)
            .ThenInclude(s => s.User)
            .Include(r => r.Svadba)
            .ThenInclude(s => s.Ponuda)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (racun == null)
        {
            throw new ClientException("Račun nije pronađen.");
        }

        var uplate = await _dbContext.Rate
            .Where(r => r.SvadbaId == racun.SvadbaId && r.CreatedAt <= racun.CreatedAt)
            .OrderBy(r => r.DatumUplate)
            .ToListAsync();

        return RacunPdfGenerator.Generate(racun, racun.Svadba, uplate);
    }

    private async Task<string> GenerateBrojRacunaAsync(int year)
    {
        var count = await _dbContext.Racuni.CountAsync(r => r.DatumIzdavanja.Year == year);
        return $"R-{year}-{(count + 1):D5}";
    }

    private static RacunResponse MapResponse(Racun entity)
    {
        var svadba = entity.Svadba;
        return new RacunResponse
        {
            Id = entity.Id,
            SvadbaId = entity.SvadbaId,
            BrojRacuna = entity.BrojRacuna,
            DatumIzdavanja = entity.DatumIzdavanja,
            UkupanIznos = entity.UkupanIznos,
            UplaceniIznos = entity.UplaceniIznos,
            KorisnikIme = svadba?.User != null
                ? $"{svadba.User.FirstName} {svadba.User.LastName}".Trim()
                : string.Empty,
            PonudaNaziv = svadba?.Ponuda?.Naziv ?? string.Empty,
            DatumSvadbe = svadba?.DatumSvadbe ?? entity.DatumIzdavanja,
            CreatedAt = entity.CreatedAt,
        };
    }
}
