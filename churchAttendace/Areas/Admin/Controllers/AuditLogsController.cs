using churchAttendace.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? actionType)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.TableName.Contains(search) || a.RecordId.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(actionType))
            {
                query = query.Where(a => a.Action == actionType);
            }

            ViewData["Search"] = search;
            ViewData["ActionType"] = actionType;
            var items = await query.Take(200).ToListAsync();
            return View(items);
        }
    }
}
