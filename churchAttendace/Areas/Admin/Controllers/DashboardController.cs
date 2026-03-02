using churchAttendace.Data;
using churchAttendace.Models.ViewModels;
using churchAttendace.Services.Interfaces;
using churchAttendace.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserScopeService _scope;

        public DashboardController(ApplicationDbContext context, IUserScopeService scope)
        {
            _context = context;
            _scope = scope;
        }

        public async Task<IActionResult> Index()
        {
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var allowedClassIds = await _scope.GetAllowedClassIdsAsync();
            var attendanceQuery = _context.Attendance
                .ApplyAttendanceScope(_scope, allowedStageIds, allowedClassIds)
                .Where(a => a.Session!.SessionDate >= startOfMonth);
            var totalAttendance = await attendanceQuery.CountAsync();
            var presentAttendance = await attendanceQuery.CountAsync(a => a.IsPresent);
            var rate = totalAttendance == 0 ? 0 : (double)presentAttendance / totalAttendance * 100;

            var model = new DashboardViewModel
            {
                StagesCount = await _context.Stages.ApplyStageScope(_scope, allowedStageIds).CountAsync(s => s.IsActive),
                ClassesCount = await _context.Classes.ApplyClassScope(_scope, allowedStageIds, allowedClassIds).CountAsync(c => c.IsActive),
                ChildrenCount = await _context.Children.ApplyChildScope(_scope, allowedStageIds, allowedClassIds).CountAsync(c => c.IsActive),
                SessionsCount = await _context.Sessions.ApplySessionScope(_scope, allowedStageIds, allowedClassIds).CountAsync(s => s.IsActive),
                AttendanceThisMonth = presentAttendance,
                AttendanceRate = Math.Round(rate, 2)
            };

            return View(model);
        }
    }
}
