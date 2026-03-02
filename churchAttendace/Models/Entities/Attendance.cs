using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Models.Entities
{
    public class Attendance
    {
        public int SessionId { get; set; }
        public int ChildId { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        [StringLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
        public string? Notes { get; set; }

        public DateTime RecordedAt { get; set; }

        [Required]
        public string RecordedByUserId { get; set; } = string.Empty;

        public virtual Session? Session { get; set; }
        public virtual Child? Child { get; set; }
        public virtual ApplicationUser? RecordedByUser { get; set; }
    }
}
