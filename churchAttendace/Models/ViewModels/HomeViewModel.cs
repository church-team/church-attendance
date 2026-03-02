using System.ComponentModel.DataAnnotations;

namespace churchAttendace.Models.ViewModels
{
    public class HomeViewModel
    {
        public string Box1Text { get; set; } = @"تأسست الكنيسة
في حبرية مثلث الرحمات الانبا مينا
مطران كرسي جرجا وبهجورة وفرشوط وتوابعها سنه 1970 ميلادية
وتم اعادة تدشينها بعد تجديدها
في حبرية صاحب النيافة الانبا مرقوريوس اسقف جرجا وتوابعها ورئيس دير الملاك ميخائيل الشرقي
يوم الاثنين الموافق 17 /9 /2012 ميلادية";
        public string Box2Text { get; set; } = @"في اثناء القداس الالهي
راه حاضرون القداس
نورا يشع من ستر الهيكل
وعند اختفاء النور وجدوا صورة القديس العظيم مارمينا قد طبعت على الستر";
        /// <summary>شكاوى ومقترحات التي تم الرد عليها (للعرض على الصفحة الرئيسية).</summary>
        public List<ComplaintWithReplyViewModel> ComplaintsWithReplies { get; set; } = new();
    }

    public class ComplaintWithReplyViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Reply { get; set; } = string.Empty;
        public DateTime RepliedAt { get; set; }
    }

    public class ComplaintViewModel
    {
        [Display(Name = "شكاوى أو مقترحات")]
        [StringLength(2000, ErrorMessage = "النص يجب ألا يتجاوز 2000 حرف")]
        public string? Text { get; set; }
    }
}
