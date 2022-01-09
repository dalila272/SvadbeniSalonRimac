using AutoMapper;
using SvadbeniSalon.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvadbeniSalon.Database.Extensions
{
    public static class QueryableExtension
    {

        public static PagedResult<TModel> GetPaginatedResult<TEntity, TModel, TRequestBase>(this IQueryable<TEntity> query, IMapper mapper, PagedRequestBase<TRequestBase> pagedRequest)
        {
            if (!(pagedRequest?.IsFullSize ?? false))
                query = query.Skip(pagedRequest.ItemsToSkip()).Take(pagedRequest.PageSize);

            return new PagedResult<TModel>
            {
                PageSize = pagedRequest.PageSize,
                Results = mapper.Map<IList<TModel>>(query?.ToList()),
                TotalResults = query?.Count() ?? 0
            };
        }
    }
}
