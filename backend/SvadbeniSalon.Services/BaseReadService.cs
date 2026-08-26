using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services.Database;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace SvadbeniSalon.Services
{
    public abstract class BaseReadService<TEntity, TResponse, TSearch> : IBaseReadService<TResponse, TSearch>
        where TEntity : class
        where TSearch : BaseSearchObject, new()
    {
        protected readonly MapsterMapper.IMapper _mapper;
        protected readonly SvadbeniSalonDbContext _dbContext;

        protected BaseReadService(MapsterMapper.IMapper mapper, SvadbeniSalonDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        protected abstract IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, TSearch? search);

        public virtual async Task<PageResult<TResponse>> GetAllAsync(TSearch? search = null)
        {
            search ??= new TSearch();
            var query = await IncludeRelatedEntitiesAsync(search, _dbContext.Set<TEntity>().AsQueryable());
            return await ExecutePagedAsync(query, search, MapEntityToResponse);
        }

        protected virtual TResponse MapEntityToResponse(TEntity entity)
            => _mapper.Map<TResponse>(entity);

        protected async Task<PageResult<TResponse>> ExecutePagedAsync(
            IQueryable<TEntity> query,
            TSearch search,
            Func<TEntity, TResponse> map)
        {
            search.NormalizePaging();
            query = ApplyFilters(query, search);

            int? totalCount = null;
            if (search.IncludeTotalCount == true)
            {
                totalCount = await query.CountAsync();
            }

            if (!string.IsNullOrWhiteSpace(search.SortBy)
                && TryNormalizeSortBy(search.SortBy, out var safeSort))
            {
                query = query.OrderBy(safeSort);
            }

            var entities = await query
                .Skip((search.Page!.Value - 1) * search.PageSize!.Value)
                .Take(search.PageSize!.Value)
                .ToListAsync();

            return new PageResult<TResponse>
            {
                Items = entities.Select(map).ToList(),
                TotalCount = totalCount,
            };
        }

        protected static bool TryNormalizeSortBy(string sortBy, out string normalized)
        {
            normalized = string.Empty;
            var parts = sortBy.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length is < 1 or > 2)
            {
                return false;
            }

            var propertyName = parts[0];
            var direction = parts.Length == 2 ? parts[1].ToLowerInvariant() : "asc";
            if (direction is "descending") direction = "desc";
            if (direction is "ascending") direction = "asc";
            if (direction is not ("asc" or "desc"))
            {
                return false;
            }

            var prop = typeof(TEntity).GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null)
            {
                return false;
            }

            normalized = $"{prop.Name} {direction}";
            return true;
        }

        protected virtual Task<IQueryable<TEntity>> IncludeRelatedEntitiesAsync(
            TSearch? search,
            IQueryable<TEntity> query)
        {
            return Task.FromResult(query);
        }

        public virtual async Task<TResponse> GetByIdAsync(int id)
        {
            var query = await IncludeRelatedEntitiesAsync(null, _dbContext.Set<TEntity>().AsQueryable());
            var entity = await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
            if (entity == null)
            {
                throw new NotFoundException($"{typeof(TEntity).Name} with id {id} not found.");
            }

            return MapEntityToResponse(entity);
        }
    }
}
