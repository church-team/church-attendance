using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Models.Entities
{
    public class Complaint
    {
        public int Id { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "النص يجب ألا يتجاوز 2000 حرف")]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        [StringLength(2000, ErrorMessage = "الرد يجب ألا يتجاوز 2000 حرف")]
        public string? Reply { get; set; }

        public DateTime? RepliedAt { get; set; }
        public string? RepliedByUserId { get; set; }

        public virtual ApplicationUser? RepliedByUser { get; set; }
    }
}
