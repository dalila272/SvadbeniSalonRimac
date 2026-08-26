using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services.Database;
using FluentValidation;
using SvadbeniSalon.Model.Exceptions;

namespace SvadbeniSalon.Services;

public interface IPonudaService
    : IBaseCRUDService<PonudaResponse, PonudaSearchObject, PonudaInsertRequest, PonudaUpdateRequest>
{
}

public class PonudaService
    : BaseCRUDService<Ponuda, PonudaResponse, PonudaSearchObject, PonudaInsertRequest, PonudaUpdateRequest>,
        IPonudaService
{
    public PonudaService(
        SvadbeniSalonDbContext dbContext,
        MapsterMapper.IMapper mapper,
        IValidator<PonudaInsertRequest> insertValidator,
        IValidator<PonudaUpdateRequest> updateValidator)
        : base(dbContext, mapper, insertValidator, updateValidator)
    {
    }

    protected override async Task<IQueryable<Ponuda>> IncludeRelatedEntitiesAsync(
        PonudaSearchObject? search,
        IQueryable<Ponuda> query = null!)
    {
        query ??= _dbContext.Set<Ponuda>();
        query = query
            .Include(p => p.Meni)
            .Include(p => p.MuzicariPonuda).ThenInclude(mp => mp.Muzicar)
            .Include(p => p.DekoracijePonuda).ThenInclude(dp => dp.Dekoracija);

        return await base.IncludeRelatedEntitiesAsync(search, query);
    }

    protected override IQueryable<Ponuda> ApplyFilters(IQueryable<Ponuda> query, PonudaSearchObject? search)
    {
        if (search != null)
        {
            if (!string.IsNullOrWhiteSpace(search.Naziv))
            {
                var term = search.Naziv.Trim().ToLower();
                query = query.Where(p => p.Naziv.ToLower().Contains(term));
            }

            if (search.MeniId.HasValue)
            {
                query = query.Where(p => p.MeniId == search.MeniId.Value);
            }

            if (search.MuzicarId.HasValue)
            {
                query = query.Where(p =>
                    p.MuzicariPonuda.Any(mp => mp.MuzicarId == search.MuzicarId.Value));
            }

            if (search.DekoracijaId.HasValue)
            {
                query = query.Where(p =>
                    p.DekoracijePonuda.Any(dp => dp.DekoracijaId == search.DekoracijaId.Value));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == search.IsActive.Value);
            }
        }

        return query;
    }

    protected PonudaResponse MapPonuda(Ponuda entity)
    {
        var response = _mapper.Map<PonudaResponse>(entity);
        response.Muzicari = entity.MuzicariPonuda
            .Select(mp => _mapper.Map<MuzicarResponse>(mp.Muzicar))
            .ToList();
        response.Dekoracije = entity.DekoracijePonuda
            .Select(dp => _mapper.Map<DekoracijaResponse>(dp.Dekoracija))
            .ToList();
        return response;
    }

    public override async Task<PonudaResponse> GetByIdAsync(int id)
    {
        var entity = await _dbContext.Set<Ponuda>()
            .Include(p => p.Meni)
            .Include(p => p.MuzicariPonuda).ThenInclude(mp => mp.Muzicar)
            .Include(p => p.DekoracijePonuda).ThenInclude(dp => dp.Dekoracija)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null)
        {
            throw new NotFoundException($"Ponuda with id {id} not found.");
        }

        return MapPonuda(entity);
    }

    public override async Task<PageResult<PonudaResponse>> GetAllAsync(PonudaSearchObject? search = null)
    {
        search ??= new PonudaSearchObject();
        if (!(search.IncludeInactive ?? false))
        {
            search.IsActive ??= true;
        }

        search.IncludeTotalCount ??= true;
        if (string.IsNullOrWhiteSpace(search.SortBy))
        {
            search.SortBy = "Id desc";
        }

        var query = _dbContext.Set<Ponuda>()
            .Include(p => p.Meni)
            .AsQueryable();

        return await ExecutePagedAsync(query, search, MapPonudaSummary);
    }

    private PonudaResponse MapPonudaSummary(Ponuda entity)
    {
        var response = _mapper.Map<PonudaResponse>(entity);
        response.Muzicari = [];
        response.Dekoracije = [];
        return response;
    }

    public override async Task<PonudaResponse> InsertAsync(PonudaInsertRequest request)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var response = await base.InsertAsync(request);
            await SyncRelationsAsync(response.Id, request.MuzicarIds, request.DekoracijaIds);
            await transaction.CommitAsync();
            return (await GetByIdAsync(response.Id))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public override async Task<PonudaResponse> UpdateAsync(int id, PonudaUpdateRequest request)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var response = await base.UpdateAsync(id, request);
            await SyncRelationsAsync(id, request.MuzicarIds, request.DekoracijaIds);
            await transaction.CommitAsync();
            return (await GetByIdAsync(id))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task SyncRelationsAsync(int ponudaId, List<int> muzicarIds, List<int> dekoracijaIds)
    {
        var existingMuzicari = _dbContext.Set<MuzicarPonuda>().Where(x => x.PonudaId == ponudaId);
        _dbContext.RemoveRange(existingMuzicari);

        var existingDekoracije = _dbContext.Set<DekoracijaPonuda>().Where(x => x.PonudaId == ponudaId);
        _dbContext.RemoveRange(existingDekoracije);

        foreach (var muzicarId in muzicarIds.Distinct())
        {
            _dbContext.Add(new MuzicarPonuda { PonudaId = ponudaId, MuzicarId = muzicarId });
        }

        foreach (var dekoracijaId in dekoracijaIds.Distinct())
        {
            _dbContext.Add(new DekoracijaPonuda { PonudaId = ponudaId, DekoracijaId = dekoracijaId });
        }

        await _dbContext.SaveChangesAsync();
    }
}
