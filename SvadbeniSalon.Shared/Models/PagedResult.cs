using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Shared.Models
{
    public class PagedResult<T>
    {
        public PagedResult() { }
        public PagedResult(ICollection<T> results, int totalResults = 0, int page = 0, int pageSize = 0)
        {
            Results = results;
            TotalResults = totalResults;
            Page = page;
            PageSize = pageSize;
        }

        public PagedResult(PagedResult<T> baseInfo)
        {
            Page = baseInfo.Page;
            PageSize = baseInfo.PageSize;
            TotalResults = baseInfo.TotalResults;
            Results = baseInfo.Results;
        }

        public ICollection<T> Results { get; set; }
        public int TotalResults { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        //When working with graph api
        public string NextPage { get; set; }

        public int TotalPages => (TotalResults - 1) / (PageSize < 1 ? 1 : PageSize) + 1;
    }
}
