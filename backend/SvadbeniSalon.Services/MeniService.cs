using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services.Database;
using FluentValidation;

namespace SvadbeniSalon.Services;

public interface IMeniService
    : IBaseCRUDService<MeniResponse, MeniSearchObject, MeniInsertRequest, MeniUpdateRequest> { }

public class MeniService
    : BaseCRUDService<Meni, MeniResponse, MeniSearchObject, MeniInsertRequest, MeniUpdateRequest>,
        IMeniService
{
    public MeniService(
        SvadbeniSalonDbContext dbContext,
        MapsterMapper.IMapper mapper,
        IValidator<MeniInsertRequest> insertValidator,
        IValidator<MeniUpdateRequest> updateValidator)
        : base(dbContext, mapper, insertValidator, updateValidator)
    {
    }

    protected override Meni MapInsertRequestToEntity(MeniInsertRequest request)
    {
        return new Meni
        {
            Naziv = request.Naziv,
            Opis = request.Opis,
            Cijena = request.Cijena,
            IsActive = request.IsActive,
        };
    }

    protected override void MapUpdateRequestToEntity(MeniUpdateRequest request, Meni entity)
    {
        entity.Naziv = request.Naziv;
        entity.Opis = request.Opis;
        entity.Cijena = request.Cijena;
        entity.IsActive = request.IsActive;
    }

    protected override IQueryable<Meni> ApplyFilters(IQueryable<Meni> query, MeniSearchObject? search)
    {
        if (!string.IsNullOrWhiteSpace(search?.Naziv))
        {
            var term = search.Naziv.Trim().ToLower();
            query = query.Where(m => m.Naziv.ToLower().Contains(term));
        }

        if (search?.IsActive.HasValue == true)
        {
            query = query.Where(m => m.IsActive == search.IsActive.Value);
        }

        return query;
    }

    public override async Task<MeniResponse> GetByIdAsync(int id)
    {
        var entity = await LoadMeniAsync(id);
        return MapMeni(entity);
    }

    public override async Task<PageResult<MeniResponse>> GetAllAsync(MeniSearchObject? search = null)
    {
        search ??= new MeniSearchObject();
        search.IncludeTotalCount ??= true;
        if (string.IsNullOrWhiteSpace(search.SortBy))
        {
            search.SortBy = "Id desc";
        }

        var query = _dbContext.Set<Meni>().AsQueryable();
        return await ExecutePagedAsync(query, search, MapMeniSummary);
    }

    private MeniResponse MapMeniSummary(Meni entity)
    {
        var response = _mapper.Map<MeniResponse>(entity);
        response.Artikli = [];
        return response;
    }

    public override async Task<MeniResponse> InsertAsync(MeniInsertRequest request)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var response = await base.InsertAsync(request);
            await SyncArtikliAsync(response.Id, request.HranaIds, request.PiceIds);
            await transaction.CommitAsync();
            return (await GetByIdAsync(response.Id))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public override async Task<MeniResponse> UpdateAsync(int id, MeniUpdateRequest request)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var response = await base.UpdateAsync(id, request);
            await SyncArtikliAsync(id, request.HranaIds, request.PiceIds);
            await transaction.CommitAsync();
            return (await GetByIdAsync(id))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public override async Task DeleteAsync(int id)
    {
        var inUse = await _dbContext.Set<Ponuda>().AnyAsync(p => p.MeniId == id);
        if (inUse)
        {
            throw new ClientException("Meni se koristi u ponudama i ne može se obrisati.");
        }

        var links = _dbContext.Set<MeniArtikal>().Where(x => x.MeniId == id);
        _dbContext.RemoveRange(links);

        await base.DeleteAsync(id);
    }

    private async Task<Meni> LoadMeniAsync(int id)
    {
        var entity = await _dbContext.Set<Meni>()
            .Include(m => m.MeniArtikli)
            .ThenInclude(ma => ma.Artikal)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null)
        {
            throw new NotFoundException($"Meni with id {id} not found.");
        }

        return entity;
    }

    private MeniResponse MapMeni(Meni entity)
    {
        var response = _mapper.Map<MeniResponse>(entity);
        response.Artikli = entity.MeniArtikli
            .Select(ma => _mapper.Map<ArtikalResponse>(ma.Artikal))
            .ToList();
        return response;
    }

    private async Task SyncArtikliAsync(int meniId, List<int> hranaIds, List<int> piceIds)
    {
        var artikalIds = hranaIds.Concat(piceIds).Distinct().ToList();

        var existing = _dbContext.Set<MeniArtikal>().Where(x => x.MeniId == meniId);
        _dbContext.RemoveRange(existing);

        foreach (var artikalId in artikalIds)
        {
            _dbContext.Add(new MeniArtikal { MeniId = meniId, ArtikalId = artikalId });
        }

        await _dbContext.SaveChangesAsync();
    }
}
