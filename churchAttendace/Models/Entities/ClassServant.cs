using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Models.Entities
{
    public class ClassServant
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int ClassId { get; set; }

        public DateTime AssignedAt { get; set; }

        public string? AssignedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ApplicationUser? User { get; set; }
        public virtual Class? Class { get; set; }
        public virtual ApplicationUser? AssignedByUser { get; set; }
    }
}
