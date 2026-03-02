using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Models.Entities
{
    public class Class
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الفصل مطلوب")]
        [StringLength(100, ErrorMessage = "اسم الفصل يجب ألا يتجاوز 100 حرف")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int StageId { get; set; }

        [StringLength(500, ErrorMessage = "الوصف يجب ألا يتجاوز 500 حرف")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual Stage? Stage { get; set; }
        public virtual ICollection<ClassServant> Servants { get; set; } = new List<ClassServant>();
        public virtual ICollection<Child> Children { get; set; } = new List<Child>();
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
