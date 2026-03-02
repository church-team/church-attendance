namespace churchAttendace.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int StagesCount { get; set; }
        public int ClassesCount { get; set; }
        public int ChildrenCount { get; set; }
        public int SessionsCount { get; set; }
        public int AttendanceThisMonth { get; set; }
        public double AttendanceRate { get; set; }
    }
}
