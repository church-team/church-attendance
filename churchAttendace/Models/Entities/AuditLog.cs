using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Models.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string RecordId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Action { get; set; } = string.Empty;

        public string? ChangesJson { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; }

        public virtual ApplicationUser? User { get; set; }
    }
}
