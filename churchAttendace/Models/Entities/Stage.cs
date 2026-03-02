using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Models.Entities
{
    public class Stage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المرحلة مطلوب")]
        [StringLength(100, ErrorMessage = "اسم المرحلة يجب ألا يتجاوز 100 حرف")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "الوصف يجب ألا يتجاوز 500 حرف")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
        public virtual ICollection<StageManagerStage> StageManagers { get; set; } = new List<StageManagerStage>();
    }
}
