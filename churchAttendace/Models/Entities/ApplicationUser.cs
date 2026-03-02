using Microsoft.AspNetCore.Identity;

namespace churchAttendace.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<StageManagerStage> ManagedStages { get; set; } = new List<StageManagerStage>();
        public virtual ICollection<ClassServant> TaughtClasses { get; set; } = new List<ClassServant>();
        public virtual ICollection<Session> CreatedSessions { get; set; } = new List<Session>();
        public virtual ICollection<Attendance> RecordedAttendances { get; set; } = new List<Attendance>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
