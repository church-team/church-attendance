namespace churchAttendace.Utilities
{
    public class QueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? Sort { get; set; } = "Id";
        public string? Dir { get; set; } = "asc";
        public int? StageId { get; set; }
        public int? ClassId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
