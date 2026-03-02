namespace churchAttendace.Models.ViewModels
{
    public class BreadcrumbItem
    {
        public string Title { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool IsActive { get; set; }
    }
}
