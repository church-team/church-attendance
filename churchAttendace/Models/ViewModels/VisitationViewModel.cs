using System;
using System.Collections.Generic;

namespace churchAttendace.Models.ViewModels
{
    public class VisitationChildRowViewModel
    {
        public int ChildId { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ChildName { get; set; } = string.Empty;
        public string ParentPhoneNumber { get; set; } = string.Empty;
        public string? ChildPhoneNumber { get; set; }
        public DateTime? LastAbsentDate { get; set; }
        public bool IsContacted { get; set; }
    }

    public class VisitationViewModel
    {
        public List<VisitationChildRowViewModel> Children { get; set; } = new();
    }
}

