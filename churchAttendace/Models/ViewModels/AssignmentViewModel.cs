using Microsoft.AspNetCore.Mvc.Rendering;

namespace churchAttendace.Models.ViewModels
{
    public class AssignmentViewModel
    {
        public List<StageManagerAssignmentRow> StageManagers { get; set; } = new();
        public List<ClassServantAssignmentRow> ClassServants { get; set; } = new();
        public SelectList? StageOptions { get; set; }
        public SelectList? ClassOptions { get; set; }
        public SelectList? ManagerOptions { get; set; }
        public SelectList? ServantOptions { get; set; }
    }

    public class StageManagerAssignmentRow
    {
        public int Id { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }

    public class ClassServantAssignmentRow
    {
        public int Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ServantEmail { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }
}
