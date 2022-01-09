using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Shared.Models
{
    public class PagedRequestBase<T>
    {
        public PagedRequestBase()
        {
        }

        public PagedRequestBase(int pageSize, int page)
        {
            PageSize = pageSize;
            Page = page;
        }

        public string Includes { get; set; }

        public int PageSize { get; set; }

        public int Page { get; set; }

        public long? FromId { get; set; }

        public DateTime? UpdatedAfter { get; set; }

        public string OrderByKey { get; set; }

        public bool IsDescending { get; set; } = false;

        public bool IsFullSize { get; set; } = false;
        public T Query { get; set; }

        // json string because Swashbuckle does not support Dictionary in FromQuery yet
        public string Filter { get; set; }
        //When working with graph api
        public string NextPage { get; set; }
        private void EnsureValidPagination()
        {
            if (PageSize <= 0)
                PageSize = 10;

            if (PageSize > 100)
                PageSize = 100;

            if (Page <= 0)
                Page = 1;

            if (Page > 1000)
                Page = 1000;
        }

        public int ItemsToSkip()
        {
            EnsureValidPagination();
            return (Page - 1) * PageSize;
        }
    }
}
