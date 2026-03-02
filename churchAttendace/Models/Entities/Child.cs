using System.ComponentModel.DataAnnotations;
using churchAttendace.Utilities;

namespace churchAttendace.Models.Entities
{
    public class Child
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الطفل مطلوب")]
        [StringLength(200, ErrorMessage = "الاسم يجب ألا يتجاوز 200 حرف")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "رقم هاتف غير صحيح")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "اسم ولي الأمر مطلوب")]
        [StringLength(200, ErrorMessage = "اسم ولي الأمر يجب ألا يتجاوز 200 حرف")]
        public string ParentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "هاتف ولي الأمر مطلوب")]
        [Phone(ErrorMessage = "رقم هاتف غير صحيح")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "رقم الهاتف يجب أن يكون رقم مصري صحيح")]
        [StringLength(20)]
        public string ParentPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        [DataType(DataType.Date)]
        [PastDate(ErrorMessage = "تاريخ الميلاد يجب أن يكون في الماضي")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "الفصل مطلوب")]
        public int ClassId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "عدد النقاط يجب أن يكون رقماً صحيحاً غير سالب")]
        public int Points { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual Class? Class { get; set; }

        public string Address { get; set; } = string.Empty;

        /// <summary>آخر تاريخ تم فيه تسليم هدية الأربع أسابيع؛ العد يبدأ من بعد هذا التاريخ.</summary>
        public DateTime? LastGiftDeliveredAt { get; set; }
    }
}
