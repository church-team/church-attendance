using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Models.Entities
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الإعلان مطلوب")]
        [StringLength(300, ErrorMessage = "العنوان يجب ألا يتجاوز 300 حرف")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "محتوى الإعلان مطلوب")]
        [StringLength(4000, ErrorMessage = "المحتوى يجب ألا يتجاوز 4000 حرف")]
        public string Body { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
