using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Models.ViewModels
{
    public class AttendanceViewModel
    {
        public int SessionId { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public string ClassName { get; set; } = string.Empty;

        [Required]
        public List<AttendanceChildViewModel> Children { get; set; } = new();
    }

    public class AttendanceChildViewModel
    {
        public int ChildId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
        public string? Notes { get; set; }
    }
}
