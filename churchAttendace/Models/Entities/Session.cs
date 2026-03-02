using System.ComponentModel.DataAnnotations;
using churchAttendace.Utilities;

namespace churchAttendace.Models.Entities
{
    public class Session
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الفصل مطلوب")]
        public int ClassId { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الجلسة مطلوب")]
        [DataType(DataType.Date)]
        [NotFutureDate(ErrorMessage = "تاريخ الجلسة لا يمكن أن يكون في المستقبل")]
        public DateTime SessionDate { get; set; }

        [StringLength(200, ErrorMessage = "اسم الجلسة يجب ألا يتجاوز 200 حرف")]
        public string? SessionName { get; set; }

        [StringLength(1000, ErrorMessage = "الملاحظات يجب ألا تتجاوز 1000 حرف")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual Class? Class { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }
        public virtual ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
    }
}
