using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Models.Entities
{
    public class StageManagerStage
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int StageId { get; set; }

        public DateTime AssignedAt { get; set; }

        public string? AssignedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ApplicationUser? User { get; set; }
        public virtual Stage? Stage { get; set; }
        public virtual ApplicationUser? AssignedByUser { get; set; }
    }
}
