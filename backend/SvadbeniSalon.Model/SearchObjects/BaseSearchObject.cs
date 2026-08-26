using SvadbeniSalon.Model.Exceptions;

namespace SvadbeniSalon.Model.SearchObjects
{
    public class BaseSearchObject
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;

        public int? Page { get; set; } = 1;

        public int? PageSize { get; set; } = DefaultPageSize;

        public bool? IncludeTotalCount { get; set; } = false;

        public string? SortBy { get; set; }

        public void NormalizePaging()
        {
            Page ??= 1;
            PageSize ??= DefaultPageSize;

            if (Page < 1)
            {
                throw new ClientException("Page mora biti najmanje 1.");
            }

            if (PageSize < 1)
            {
                throw new ClientException("PageSize mora biti najmanje 1.");
            }

            if (PageSize > MaxPageSize)
            {
                throw new ClientException(
                    $"PageSize ne smije biti veći od {MaxPageSize}.");
            }
        }
    }
}
