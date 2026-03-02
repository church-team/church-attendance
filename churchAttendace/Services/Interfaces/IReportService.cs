using churchAttendace.Models.ViewModels;

namespace churchAttendace.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<SessionsReportRowViewModel>> GetSessionsReportAsync(ReportFilterViewModel filters);
        Task<List<AttendanceGridReportRowViewModel>> GetAttendanceGridReportAsync(ReportFilterViewModel filters);
        Task<List<AbsentChildrenReportRowViewModel>> GetAbsentChildrenReportAsync(ReportFilterViewModel filters);
        Task<List<ConsecutiveAttendanceReportRowViewModel>> GetConsecutiveAttendanceReportAsync(ReportFilterViewModel filters);
    }
}
