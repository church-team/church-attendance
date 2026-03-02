namespace churchAttendace.Models.ViewModels
{
    public class GiftsChildRowViewModel
    {
        public int ChildId { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ChildName { get; set; } = string.Empty;
        public int ConsecutiveWeeks { get; set; }
        public DateTime? LastSessionDate { get; set; }
    }

    public class GiftsViewModel
    {
        public List<GiftsChildRowViewModel> Children { get; set; } = new();
    }
}
