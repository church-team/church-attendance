using churchAttendace.Data;
using churchAttendace.Models.ViewModels;
using churchAttendace.Services.Interfaces;
using churchAttendace.Utilities;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserScopeService _scope;

        public ReportService(ApplicationDbContext context, IUserScopeService scope)
        {
            _context = context;
            _scope = scope;
        }

        public async Task<List<SessionsReportRowViewModel>> GetSessionsReportAsync(ReportFilterViewModel filters)
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var allowedClassIds = await _scope.GetAllowedClassIdsAsync();

            var query = _context.Sessions
                .Include(s => s.Class)
                .ThenInclude(c => c!.Stage)
                .Include(s => s.AttendanceRecords)
                .ApplySessionScope(_scope, allowedStageIds, allowedClassIds)
                .AsQueryable();

            if (filters.ClassId.HasValue)
            {
                query = query.Where(s => s.ClassId == filters.ClassId.Value);
            }

            if (filters.StageId.HasValue)
            {
                query = query.Where(s => s.Class!.StageId == filters.StageId.Value);
            }

            if (filters.From.HasValue)
            {
                query = query.Where(s => s.SessionDate >= filters.From.Value);
            }

            if (filters.To.HasValue)
            {
                query = query.Where(s => s.SessionDate <= filters.To.Value);
            }

            var rows = await query
                .OrderByDescending(s => s.SessionDate)
                .Select(s => new SessionsReportRowViewModel
                {
                    StageName = s.Class!.Stage!.Name,
                    ClassName = s.Class!.Name,
                    SessionName = s.SessionName ?? $"جلسة {s.SessionDate:yyyy/MM/dd}",
                    SessionDate = s.SessionDate,
                    TotalChildren = s.Class!.Children.Count(c => c.IsActive),
                    PresentCount = s.AttendanceRecords.Count(a => a.IsPresent),
                    AbsentCount = s.AttendanceRecords.Count(a => !a.IsPresent)
                })
                .ToListAsync();

            foreach (var row in rows)
            {
                row.AttendanceRate = row.TotalChildren == 0 ? 0 : Math.Round((double)row.PresentCount / row.TotalChildren * 100, 2);
            }

            return rows;
        }

        public async Task<List<AttendanceGridReportRowViewModel>> GetAttendanceGridReportAsync(ReportFilterViewModel filters)
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var allowedClassIds = await _scope.GetAllowedClassIdsAsync();

            var query = _context.Attendance
                .Include(a => a.Child)
                .Include(a => a.Session)
                .ThenInclude(s => s!.Class)
                .ThenInclude(c => c!.Stage)
                .ApplyAttendanceScope(_scope, allowedStageIds, allowedClassIds)
                .AsQueryable();

            if (filters.ClassId.HasValue)
            {
                query = query.Where(a => a.Session!.ClassId == filters.ClassId.Value);
            }

            if (filters.StageId.HasValue)
            {
                query = query.Where(a => a.Session!.Class!.StageId == filters.StageId.Value);
            }

            if (filters.From.HasValue)
            {
                query = query.Where(a => a.Session!.SessionDate >= filters.From.Value);
            }

            if (filters.To.HasValue)
            {
                query = query.Where(a => a.Session!.SessionDate <= filters.To.Value);
            }

            return await query
                .OrderByDescending(a => a.Session!.SessionDate)
                .ThenBy(a => a.Child!.FullName)
                .Select(a => new AttendanceGridReportRowViewModel
                {
                    StageName = a.Session!.Class!.Stage!.Name,
                    ClassName = a.Session!.Class!.Name,
                    SessionName = a.Session!.SessionName ?? $"جلسة {a.Session.SessionDate:yyyy/MM/dd}",
                    SessionDate = a.Session!.SessionDate,
                    ChildName = a.Child!.FullName,
                    IsPresent = a.IsPresent,
                    Notes = a.Notes
                })
                .ToListAsync();
        }

        public async Task<List<AbsentChildrenReportRowViewModel>> GetAbsentChildrenReportAsync(ReportFilterViewModel filters)
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var allowedClassIds = await _scope.GetAllowedClassIdsAsync();

            var query = _context.Attendance
                .Include(a => a.Child)
                .ThenInclude(c => c!.Class)
                .ThenInclude(c => c!.Stage)
                .Include(a => a.Session)
                .ApplyAttendanceScope(_scope, allowedStageIds, allowedClassIds)
                .Where(a => !a.IsPresent)
                .AsQueryable();

            if (filters.ClassId.HasValue)
            {
                query = query.Where(a => a.Session!.ClassId == filters.ClassId.Value);
            }

            if (filters.StageId.HasValue)
            {
                query = query.Where(a => a.Session!.Class!.StageId == filters.StageId.Value);
            }

            if (filters.From.HasValue)
            {
                query = query.Where(a => a.Session!.SessionDate >= filters.From.Value);
            }

            if (filters.To.HasValue)
            {
                query = query.Where(a => a.Session!.SessionDate <= filters.To.Value);
            }

            return await query
                .GroupBy(a => new
                {
                    a.ChildId,
                    ChildName = a.Child!.FullName,
                    ClassName = a.Child!.Class!.Name,
                    StageName = a.Child!.Class!.Stage!.Name
                })
                .Select(g => new AbsentChildrenReportRowViewModel
                {
                    StageName = g.Key.StageName,
                    ClassName = g.Key.ClassName,
                    ChildName = g.Key.ChildName,
                    AbsentCount = g.Count(),
                    LastAbsentDate = g.Max(a => a.Session!.SessionDate)
                })
                .OrderByDescending(r => r.AbsentCount)
                .ThenBy(r => r.ChildName)
                .ToListAsync();
        }

        public async Task<List<ConsecutiveAttendanceReportRowViewModel>> GetConsecutiveAttendanceReportAsync(ReportFilterViewModel filters)
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var allowedClassIds = await _scope.GetAllowedClassIdsAsync();

            var query = _context.Attendance
                .Include(a => a.Child)
                .ThenInclude(c => c!.Class)
                .ThenInclude(c => c!.Stage)
                .Include(a => a.Session)
                .ApplyAttendanceScope(_scope, allowedStageIds, allowedClassIds)
                .AsQueryable();

            if (filters.ClassId.HasValue)
            {
                query = query.Where(a => a.Session!.ClassId == filters.ClassId.Value);
            }

            if (filters.StageId.HasValue)
            {
                query = query.Where(a => a.Session!.Class!.StageId == filters.StageId.Value);
            }

            if (filters.From.HasValue)
            {
                query = query.Where(a => a.Session!.SessionDate >= filters.From.Value);
            }

            if (filters.To.HasValue)
            {
                query = query.Where(a => a.Session!.SessionDate <= filters.To.Value);
            }

            var attendance = await query
                .OrderBy(a => a.ChildId)
                .ThenBy(a => a.Session!.SessionDate)
                .Select(a => new
                {
                    a.ChildId,
                    a.IsPresent,
                    a.Session!.SessionDate,
                    ChildName = a.Child!.FullName,
                    ClassName = a.Child!.Class!.Name,
                    StageName = a.Child!.Class!.Stage!.Name
                })
                .ToListAsync();

            var results = new List<ConsecutiveAttendanceReportRowViewModel>();
            foreach (var group in attendance.GroupBy(a => new { a.ChildId, a.ChildName, a.ClassName, a.StageName }))
            {
                var longest = 0;
                var current = 0;
                DateTime? lastDate = null;

                foreach (var record in group.OrderBy(r => r.SessionDate))
                {
                    lastDate = record.SessionDate;
                    if (record.IsPresent)
                    {
                        current++;
                        if (current > longest)
                        {
                            longest = current;
                        }
                    }
                    else
                    {
                        current = 0;
                    }
                }

                results.Add(new ConsecutiveAttendanceReportRowViewModel
                {
                    StageName = group.Key.StageName,
                    ClassName = group.Key.ClassName,
                    ChildName = group.Key.ChildName,
                    LongestStreak = longest,
                    CurrentStreak = current,
                    LastSessionDate = lastDate
                });
            }

            return results.OrderByDescending(r => r.LongestStreak).ThenBy(r => r.ChildName).ToList();
        }
    }
}
