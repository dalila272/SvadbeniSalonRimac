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
using SvadbeniSalon.Model.Constants;

namespace SvadbeniSalon.Services;

public interface IRecenzijaService
    : IBaseCRUDService<RecenzijaResponse, RecenzijaSearchObject, RecenzijaInsertRequest, RecenzijaUpdateRequest>
{
}

public class RecenzijaService
    : BaseCRUDService<Recenzija, RecenzijaResponse, RecenzijaSearchObject, RecenzijaInsertRequest, RecenzijaUpdateRequest>,
        IRecenzijaService
{

    private static readonly TerminStatus[] ReviewEligibleStatuses =
        [TerminStatus.Completed];

    private readonly IAuthenticatedUserAccessor _userAccessor;

    public RecenzijaService(
        SvadbeniSalonDbContext dbContext,
        IMapper mapper,
        IValidator<RecenzijaInsertRequest> insertValidator,
        IValidator<RecenzijaUpdateRequest> updateValidator,
        IAuthenticatedUserAccessor userAccessor)
        : base(dbContext, mapper, insertValidator, updateValidator)
    {
        _userAccessor = userAccessor;
    }

    public override async Task<PageResult<RecenzijaResponse>> GetAllAsync(RecenzijaSearchObject? search = null)
    {
        search ??= new RecenzijaSearchObject();
        if (string.IsNullOrWhiteSpace(search.SortBy))
        {
            search.SortBy = "CreatedAt desc";
        }

        search.IncludeTotalCount ??= true;
        return await base.GetAllAsync(search);
    }

    protected override async Task<IQueryable<Recenzija>> IncludeRelatedEntitiesAsync(
        RecenzijaSearchObject? search,
        IQueryable<Recenzija> query = null!)
    {
        query ??= _dbContext.Set<Recenzija>();
        return await Task.FromResult(query
            .Include(r => r.Ponuda)
            .Include(r => r.User)
            .Include(r => r.Svadba));
    }

    protected override IQueryable<Recenzija> ApplyFilters(IQueryable<Recenzija> query, RecenzijaSearchObject? search)
    {
        if (_userAccessor.IsInRole(RoleNames.Admin))
        {
            if (search?.UserId.HasValue == true)
            {
                query = query.Where(r => r.UserId == search.UserId.Value);
            }
        }
        else
        {
            var userId = _userAccessor.GetUserId();
            if (!userId.HasValue)
            {
                return query.Where(_ => false);
            }

            query = query.Where(r => r.UserId == userId.Value);
        }

        if (search?.PonudaId.HasValue == true)
        {
            query = query.Where(r => r.PonudaId == search.PonudaId.Value);
        }

        if (search?.SvadbaId.HasValue == true)
        {
            query = query.Where(r => r.SvadbaId == search.SvadbaId.Value);
        }

        if (search?.Ocjena.HasValue == true)
        {
            query = query.Where(r => r.Ocjena == search.Ocjena.Value);
        }

        return query;
    }

    public override async Task<RecenzijaResponse> GetByIdAsync(int id)
    {
        var entity = await LoadEntityAsync(id);
        if (entity == null || !CanAccess(entity))
        {
            throw new NotFoundException($"{nameof(Recenzija)} with id {id} not found.");
        }

        return MapRecenzija(entity);
    }

    public override async Task<RecenzijaResponse> InsertAsync(RecenzijaInsertRequest request)
    {
        var userId = _userAccessor.GetUserId()
                     ?? throw new InvalidOperationException("User id claim is missing.");

        var validationResult = await _insertValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
            throw new FluentValidation.ValidationException(errors);
        }

        var svadba = await _dbContext.Svadbe
            .FirstOrDefaultAsync(s => s.Id == request.SvadbaId && s.UserId == userId);

        if (svadba == null)
        {
            throw new ClientException("Svadba nije pronađena.");
        }

        if (svadba.PonudaId != request.PonudaId)
        {
            throw new ClientException("Odabrani paket ne odgovara svadbi.");
        }

        if (!ReviewEligibleStatuses.Contains(svadba.Status))
        {
            throw new ClientException("Recenziju možete ostaviti tek nakon što je svadba završena.");
        }

        var duplicate = await _dbContext.Recenzije.AnyAsync(r => r.SvadbaId == request.SvadbaId);
        if (duplicate)
        {
            throw new ClientException("Već ste ocijenili ovu svadbu.");
        }

        var entity = new Recenzija
        {
            UserId = userId,
            PonudaId = request.PonudaId,
            SvadbaId = request.SvadbaId,
            Ocjena = request.Ocjena,
            Komentar = request.Komentar?.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Recenzije.Add(entity);
        await _dbContext.SaveChangesAsync();

        return MapRecenzija(await LoadEntityAsync(entity.Id) ?? entity);
    }

    public override async Task<RecenzijaResponse> UpdateAsync(int id, RecenzijaUpdateRequest request)
    {
        var userId = _userAccessor.GetUserId()
                     ?? throw new InvalidOperationException("User id claim is missing.");

        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
            throw new FluentValidation.ValidationException(errors);
        }

        var entity = await _dbContext.Recenzije.FindAsync(id);
        if (entity == null || entity.UserId != userId)
        {
            throw new NotFoundException($"{nameof(Recenzija)} with id {id} not found.");
        }

        entity.Ocjena = request.Ocjena;
        entity.Komentar = request.Komentar?.Trim();

        await _dbContext.SaveChangesAsync();

        return MapRecenzija(await LoadEntityAsync(id) ?? entity);
    }

    public override async Task DeleteAsync(int id)
    {
        var userId = _userAccessor.GetUserId()
                     ?? throw new InvalidOperationException("User id claim is missing.");

        var entity = await _dbContext.Recenzije.FindAsync(id);
        if (entity == null || entity.UserId != userId)
        {
            throw new NotFoundException($"{nameof(Recenzija)} with id {id} not found.");
        }

        _dbContext.Recenzije.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    private bool CanAccess(Recenzija entity)
    {
        if (_userAccessor.IsInRole(RoleNames.Admin))
        {
            return true;
        }

        var userId = _userAccessor.GetUserId();
        return userId.HasValue && entity.UserId == userId.Value;
    }

    private async Task<Recenzija?> LoadEntityAsync(int id)
    {
        return await _dbContext.Recenzije
            .Include(r => r.Ponuda)
            .Include(r => r.User)
            .Include(r => r.Svadba)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    private static RecenzijaResponse MapRecenzija(Recenzija entity)
    {
        var clientName = entity.User != null
            ? $"{entity.User.FirstName} {entity.User.LastName}".Trim()
            : string.Empty;

        return new RecenzijaResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            KorisnikIme = clientName,
            PonudaId = entity.PonudaId,
            PonudaNaziv = entity.Ponuda?.Naziv ?? string.Empty,
            SvadbaId = entity.SvadbaId,
            SvadbaDatum = entity.Svadba?.DatumSvadbe,
            Ocjena = entity.Ocjena,
            Komentar = entity.Komentar,
            CreatedAt = entity.CreatedAt,
        };
    }
}
