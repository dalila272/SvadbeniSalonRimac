using FluentValidation;
using FluentValidation.Results;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services.Database;

namespace SvadbeniSalon.Services;

public interface IMuzicarService
    : IBaseCRUDService<MuzicarResponse, MuzicarSearchObject, MuzicarInsertRequest, MuzicarUpdateRequest> { }

public class MuzicarService
    : BaseCRUDService<Muzicar, MuzicarResponse, MuzicarSearchObject, MuzicarInsertRequest, MuzicarUpdateRequest>,
        IMuzicarService
{
    public MuzicarService(
        SvadbeniSalonDbContext dbContext,
        IMapper mapper,
        IValidator<MuzicarInsertRequest> insertValidator,
        IValidator<MuzicarUpdateRequest> updateValidator)
        : base(dbContext, mapper, insertValidator, updateValidator)
    {
    }

    protected override Muzicar MapInsertRequestToEntity(MuzicarInsertRequest request)
    {
        return new Muzicar
        {
            Naziv = request.Naziv.Trim(),
            Opis = request.Opis?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
        };
    }

    protected override void MapUpdateRequestToEntity(MuzicarUpdateRequest request, Muzicar entity)
    {
        entity.Naziv = request.Naziv.Trim();
        entity.Opis = request.Opis?.Trim() ?? string.Empty;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    protected override IQueryable<Muzicar> ApplyFilters(
        IQueryable<Muzicar> query,
        MuzicarSearchObject? search)
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

    public override async Task<MuzicarResponse> GetByIdAsync(int id)
    {
        var entity = await LoadMuzicarAsync(id);
        return MapMuzicar(entity);
    }

    public override async Task<PageResult<MuzicarResponse>> GetAllAsync(MuzicarSearchObject? search = null)
    {
        search ??= new MuzicarSearchObject();
        search.IncludeTotalCount ??= true;
        if (string.IsNullOrWhiteSpace(search.SortBy))
        {
            search.SortBy = "Id desc";
        }

        var query = _dbContext.Muzicari
            .Include(m => m.MuzicarZanrovi)
            .ThenInclude(mz => mz.Zanr)
            .AsQueryable();

        return await ExecutePagedAsync(query, search, MapMuzicar);
    }

    public override async Task<MuzicarResponse> InsertAsync(MuzicarInsertRequest request)
    {
        await ValidateZanroviAsync(request.ZanrIds);

        var validationResult = await _insertValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
            throw new ValidationException(errors);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var entity = MapInsertRequestToEntity(request);
            _dbContext.Muzicari.Add(entity);
            await _dbContext.SaveChangesAsync();

            await SyncZanroviAsync(entity.Id, request.ZanrIds);
            await transaction.CommitAsync();
            return await GetByIdAsync(entity.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public override async Task<MuzicarResponse> UpdateAsync(int id, MuzicarUpdateRequest request)
    {
        await ValidateZanroviAsync(request.ZanrIds);

        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => _mapper.Map<ValidationFailure>(e));
            throw new ValidationException(errors);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var entity = await LoadMuzicarAsync(id);
            MapUpdateRequestToEntity(request, entity);
            await SyncZanroviAsync(id, request.ZanrIds);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapMuzicar(await LoadMuzicarAsync(id));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public override async Task DeleteAsync(int id)
    {
        var inUse = await _dbContext.MuzicarPonuda.AnyAsync(mp => mp.MuzicarId == id);
        if (inUse)
        {
            throw new ClientException("Muzičar se koristi u ponudama i ne može se obrisati.");
        }

        var links = _dbContext.MuzicarZanrovi.Where(x => x.MuzicarId == id);
        _dbContext.RemoveRange(links);

        await base.DeleteAsync(id);
    }

    private async Task<Muzicar> LoadMuzicarAsync(int id)
    {
        var entity = await _dbContext.Muzicari
            .Include(m => m.MuzicarZanrovi)
            .ThenInclude(mz => mz.Zanr)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null)
        {
            throw new NotFoundException($"Muzicar with id {id} not found.");
        }

        return entity;
    }

    private static MuzicarResponse MapMuzicar(Muzicar entity)
    {
        return new MuzicarResponse
        {
            Id = entity.Id,
            Naziv = entity.Naziv,
            Opis = entity.Opis,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ZanrIds = entity.MuzicarZanrovi.Select(mz => mz.ZanrId).ToList(),
            Zanrovi = entity.MuzicarZanrovi
                .Where(mz => mz.Zanr != null)
                .Select(mz => new ZanrResponse
                {
                    Id = mz.Zanr!.Id,
                    Naziv = mz.Zanr.Naziv,
                    IsActive = mz.Zanr.IsActive,
                    CreatedAt = mz.Zanr.CreatedAt,
                })
                .ToList(),
        };
    }

    private async Task ValidateZanroviAsync(IEnumerable<int> zanrIds)
    {
        var ids = zanrIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var count = await _dbContext.Zanrovi.CountAsync(z => ids.Contains(z.Id) && z.IsActive);
        if (count != ids.Count)
        {
            throw new ClientException("Jedan ili više odabranih žanrova nije validan.");
        }
    }

    private async Task SyncZanroviAsync(int muzicarId, IEnumerable<int> zanrIds)
    {
        var existing = _dbContext.MuzicarZanrovi.Where(x => x.MuzicarId == muzicarId);
        _dbContext.RemoveRange(existing);

        foreach (var zanrId in zanrIds.Distinct())
        {
            _dbContext.MuzicarZanrovi.Add(new MuzicarZanr
            {
                MuzicarId = muzicarId,
                ZanrId = zanrId,
            });
        }

        await _dbContext.SaveChangesAsync();
    }
}

public interface IDekoracijaService
    : IBaseCRUDService<DekoracijaResponse, DekoracijaSearchObject, DekoracijaInsertRequest, DekoracijaUpdateRequest> { }

public class DekoracijaService
    : BaseCRUDService<Dekoracija, DekoracijaResponse, DekoracijaSearchObject, DekoracijaInsertRequest, DekoracijaUpdateRequest>,
        IDekoracijaService
{
    public DekoracijaService(
        SvadbeniSalonDbContext dbContext,
        IMapper mapper,
        IValidator<DekoracijaInsertRequest> insertValidator,
        IValidator<DekoracijaUpdateRequest> updateValidator)
        : base(dbContext, mapper, insertValidator, updateValidator)
    {
    }

    protected override IQueryable<Dekoracija> ApplyFilters(
        IQueryable<Dekoracija> query,
        DekoracijaSearchObject? search)
    {
        if (!string.IsNullOrWhiteSpace(search?.Naziv))
        {
            var term = search.Naziv.Trim().ToLower();
            query = query.Where(d => d.Naziv.ToLower().Contains(term));
        }

        if (search?.IsActive.HasValue == true)
        {
            query = query.Where(d => d.IsActive == search.IsActive.Value);
        }

        return query;
    }

    public override async Task DeleteAsync(int id)
    {
        var inUse = await _dbContext.DekoracijaPonuda.AnyAsync(dp => dp.DekoracijaId == id);
        if (inUse)
        {
            throw new ClientException("Dekoracija se koristi u ponudama i ne može se obrisati.");
        }

        await base.DeleteAsync(id);
    }
}

public interface IZanrService
    : IBaseCRUDService<ZanrResponse, ZanrSearchObject, ZanrInsertRequest, ZanrUpdateRequest> { }

public class ZanrService
    : BaseCRUDService<Zanr, ZanrResponse, ZanrSearchObject, ZanrInsertRequest, ZanrUpdateRequest>,
        IZanrService
{
    private const string CacheVersionKey = "zanrovi:version";
    private readonly IMemoryCache _cache;

    public ZanrService(
        SvadbeniSalonDbContext dbContext,
        IMapper mapper,
        IValidator<ZanrInsertRequest> insertValidator,
        IValidator<ZanrUpdateRequest> updateValidator,
        IMemoryCache cache)
        : base(dbContext, mapper, insertValidator, updateValidator)
    {
        _cache = cache;
    }

    protected override IQueryable<Zanr> ApplyFilters(IQueryable<Zanr> query, ZanrSearchObject? search)
    {
        if (!string.IsNullOrWhiteSpace(search?.Naziv))
        {
            var term = search.Naziv.Trim().ToLower();
            query = query.Where(z => z.Naziv.ToLower().Contains(term));
        }

        if (search?.IsActive.HasValue == true)
        {
            query = query.Where(z => z.IsActive == search.IsActive.Value);
        }

        return query;
    }

    public override async Task<PageResult<ZanrResponse>> GetAllAsync(ZanrSearchObject? search = null)
    {
        search ??= new ZanrSearchObject();
        search.IncludeTotalCount ??= true;
        search.NormalizePaging();

        var version = _cache.GetOrCreate(CacheVersionKey, _ => 0);
        var cacheKey =
            $"zanrovi:v{version}:{search.Page}:{search.PageSize}:{search.IsActive}:{search.Naziv}:{search.SortBy}";

        if (_cache.TryGetValue(cacheKey, out PageResult<ZanrResponse>? cached) && cached != null)
        {
            return cached;
        }

        var result = await base.GetAllAsync(search);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public override async Task<ZanrResponse> InsertAsync(ZanrInsertRequest request)
    {
        request.Naziv = request.Naziv.Trim();
        await EnsureUniqueNazivAsync(request.Naziv);
        var result = await base.InsertAsync(request);
        InvalidateCache();
        return result;
    }

    public override async Task<ZanrResponse> UpdateAsync(int id, ZanrUpdateRequest request)
    {
        request.Naziv = request.Naziv.Trim();
        await EnsureUniqueNazivAsync(request.Naziv, excludeId: id);
        var result = await base.UpdateAsync(id, request);
        InvalidateCache();
        return result;
    }

    public override async Task DeleteAsync(int id)
    {
        var inUse = await _dbContext.MuzicarZanrovi.AnyAsync(mz => mz.ZanrId == id);
        if (inUse)
        {
            throw new ClientException("Žanr se koristi kod muzičara i ne može se obrisati.");
        }

        await base.DeleteAsync(id);
        InvalidateCache();
    }

    private void InvalidateCache()
    {
        var version = _cache.GetOrCreate(CacheVersionKey, _ => 0);
        _cache.Set(CacheVersionKey, version + 1);
    }

    private async Task EnsureUniqueNazivAsync(string naziv, int? excludeId = null)
    {
        var query = _dbContext.Zanrovi.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(z => z.Id != excludeId.Value);
        }

        var exists = await query.AnyAsync(z => z.Naziv.ToLower() == naziv.ToLower());
        if (exists)
        {
            throw new ClientException($"Žanr '{naziv}' već postoji.");
        }
    }
}
