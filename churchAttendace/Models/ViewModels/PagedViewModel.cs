using churchAttendace.Utilities;

namespace churchAttendace.Models.ViewModels
{
    public class PagedViewModel<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public QueryParameters Query { get; set; } = new();

        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}
