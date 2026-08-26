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

public interface IRataService
{
    Task<PageResult<RataResponse>> GetAllAsync(RataSearchObject? search = null);
    Task<RataResponse> InsertAsync(RataInsertRequest request);
}

public class RataService : IRataService
{
    private readonly SvadbeniSalonDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<RataInsertRequest> _insertValidator;
    private readonly IAuthenticatedUserAccessor _userAccessor;

    public RataService(
        SvadbeniSalonDbContext dbContext,
        IMapper mapper,
        IValidator<RataInsertRequest> insertValidator,
        IAuthenticatedUserAccessor userAccessor)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _insertValidator = insertValidator;
        _userAccessor = userAccessor;
    }

    public async Task<PageResult<RataResponse>> GetAllAsync(RataSearchObject? search = null)
    {
        search ??= new RataSearchObject();
        search.NormalizePaging();
        search.IncludeTotalCount ??= true;

        if (!_userAccessor.IsSalonStaff())
        {
            return new PageResult<RataResponse> { Items = [], TotalCount = 0 };
        }

        var query = _dbContext.Rate.AsQueryable();

        if (search.SvadbaId.HasValue)
        {
            query = query.Where(r => r.SvadbaId == search.SvadbaId.Value);
        }

        query = query.OrderByDescending(r => r.DatumUplate);

        int? totalCount = null;
        if (search.IncludeTotalCount == true)
        {
            totalCount = await query.CountAsync();
        }

        var items = await query
            .Skip((search.Page!.Value - 1) * search.PageSize!.Value)
            .Take(search.PageSize!.Value)
            .Select(r => new RataResponse
            {
                Id = r.Id,
                SvadbaId = r.SvadbaId,
                Iznos = r.Iznos,
                DatumUplate = r.DatumUplate,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();

        return new PageResult<RataResponse>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    public async Task<RataResponse> InsertAsync(RataInsertRequest request)
    {
        if (!_userAccessor.IsSalonStaff())
        {
            throw new ClientException("Samo zaposlenici mogu dodavati uplate.");
        }

        var validationResult = await _insertValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
            throw new FluentValidation.ValidationException(errors);
        }

        var svadba = await _dbContext.Svadbe
            .Include(s => s.Ponuda)
            .FirstOrDefaultAsync(s => s.Id == request.SvadbaId);

        if (svadba == null)
        {
            throw new ClientException("Svadba nije pronađena.");
        }

        if (svadba.Status is TerminStatus.Cancelled or TerminStatus.Completed)
        {
            throw new ClientException(
                "Uplata nije moguća za otkazanu ili završenu rezervaciju.");
        }

        var uplaceno = await _dbContext.Rate
            .Where(r => r.SvadbaId == request.SvadbaId)
            .SumAsync(r => (decimal?)r.Iznos) ?? 0;

        if (svadba.Ponuda == null)
        {
            throw new ClientException("Svadba nema povezanu ponudu.");
        }

        var katalogCijena = svadba.Ponuda.Cijena;
        var preostalo = katalogCijena - uplaceno;

        if (preostalo <= 0)
        {
            throw new ClientException(
                "Svadba je već u potpunosti uplaćena. Nova uplata nije dozvoljena.");
        }

        if (request.Iznos > preostalo)
        {
            throw new ClientException(
                $"Uplata premašuje preostali iznos. Preostalo: {preostalo:0.00} KM.");
        }

        var entity = new Rata
        {
            SvadbaId = request.SvadbaId,
            Iznos = request.Iznos,
            DatumUplate = request.DatumUplate.Date,
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Rate.Add(entity);
        await _dbContext.SaveChangesAsync();

        return new RataResponse
        {
            Id = entity.Id,
            SvadbaId = entity.SvadbaId,
            Iznos = entity.Iznos,
            DatumUplate = entity.DatumUplate,
            CreatedAt = entity.CreatedAt,
        };
    }
}
