using churchAttendace.Data;
using churchAttendace.Models.Entities;
using churchAttendace.Services.Interfaces;
using churchAttendace.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "CanTakeAttendance")]
    public class SessionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly IUserScopeService _scope;

        public SessionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuditService auditService, IUserScopeService scope)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
            _scope = scope;
        }

        public async Task<IActionResult> Index(int? classId, DateTime? from, DateTime? to)
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var accessibleClassIds = await _scope.GetAllowedClassIdsAsync();
            classId = NormalizeClassSelection(classId, accessibleClassIds);
            var query = _context.Sessions
                 .Where(s => s.IsActive)
                 .Include(s => s.Class)
                .ThenInclude(c => c!.Stage)
                .ApplySessionScope(_scope, allowedStageIds, accessibleClassIds);

            if (classId.HasValue)
            {
                query = query.Where(s => s.ClassId == classId);
            }

            if (from.HasValue)
            {
                query = query.Where(s => s.SessionDate >= from);
            }

            if (to.HasValue)
            {
                query = query.Where(s => s.SessionDate <= to);
            }

            ViewData["ClassId"] = classId;
            ViewData["From"] = from?.ToString("yyyy-MM-dd");
            ViewData["To"] = to?.ToString("yyyy-MM-dd");
            ViewData["Classes"] = new SelectList(await _context.Classes.ApplyClassScope(_scope, allowedStageIds, accessibleClassIds).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");

            var items = await query.OrderByDescending(s => s.SessionDate).ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            await LoadClassesAsync();
            return View(new Session { SessionDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Session model)
        {
            ModelState.Remove(nameof(Session.CreatedByUserId));
            if (!ModelState.IsValid)
            {
                await LoadClassesAsync();
                return View(model);
            }

            if (!await _scope.CanAccessClassAsync(model.ClassId))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(model.SessionName))
            {
                model.SessionName = $"جلسة {model.SessionDate:yyyy/MM/dd}";
            }

            model.CreatedByUserId = _userManager.GetUserId(User);
            _context.Sessions.Add(model);
            await _context.SaveChangesAsync();
            await _auditService.LogCreateAsync(model);
            TempData["SuccessMessage"] = "تمت إضافة الجلسة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _scope.CanAccessSessionAsync(id))
            {
                return Forbid();
            }

            var item = await _context.Sessions
                .Include(s => s.AttendanceRecords)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            if (_scope.IsServant() && !CanEditSession(item))
            {
                TempData["ErrorMessage"] = "لا يمكن تعديل الجلسة بعد مرور ٢٤ ساعة على موعدها عند تسجيل حضور.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await LoadClassesAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Session model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Session.CreatedByUserId));
            if (!ModelState.IsValid)
            {
                await LoadClassesAsync();
                return View(model);
            }

            var original = await _context.Sessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (original == null)
            {
                return NotFound();
            }

            if (!await _scope.CanAccessSessionAsync(id) || !await _scope.CanAccessClassAsync(model.ClassId))
            {
                return Forbid();
            }

            if (_scope.IsServant() && !CanEditSession(original))
            {
                TempData["ErrorMessage"] = "لا يمكن تعديل الجلسة بعد مرور ٢٤ ساعة على موعدها عند تسجيل حضور.";
                return RedirectToAction(nameof(Details), new { id });
            }

            model.CreatedByUserId = original.CreatedByUserId;
            model.CreatedAt = original.CreatedAt;

            if (string.IsNullOrWhiteSpace(model.SessionName))
            {
                model.SessionName = $"جلسة {model.SessionDate:yyyy/MM/dd}";
            }

            model.UpdatedAt = DateTime.UtcNow;
            _context.Update(model);
            await _context.SaveChangesAsync();
            await _auditService.LogUpdateAsync(original, model);
            TempData["SuccessMessage"] = "تم تحديث الجلسة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!await _scope.CanAccessSessionAsync(id))
            {
                return Forbid();
            }

            var item = await _context.Sessions
                .Include(s => s.Class)
                .ThenInclude(c => c!.Stage)
                .Include(s => s.AttendanceRecords)
                .ThenInclude(a => a.Child)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            if (!await _scope.CanAccessSessionAsync(id))
            {
                return Forbid();
            }

            var item = await _context.Sessions.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _auditService.LogSoftDeleteAsync(item);
            TempData["SuccessMessage"] = "تم تعطيل الجلسة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadClassesAsync()
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var accessibleClassIds = await _scope.GetAllowedClassIdsAsync();
            ViewData["ClassId"] = new SelectList(await _context.Classes.ApplyClassScope(_scope, allowedStageIds, accessibleClassIds).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        }

        private int? NormalizeClassSelection(int? classId, IReadOnlyCollection<int> allowedClassIds)
        {
            if (!_scope.IsAdmin() && classId.HasValue && !allowedClassIds.Contains(classId.Value))
            {
                var fallback = allowedClassIds.FirstOrDefault();
                TempData["ErrorMessage"] = "تم تحديث الفصل المختار لأنه لم يعد ضمن صلاحياتك.";
                return fallback == 0 ? null : fallback;
            }

            return classId;
        }

        private static bool CanEditSession(Session session)
        {
            var editDeadline = session.SessionDate.Date.AddDays(1);
            if (DateTime.Now <= editDeadline)
            {
                return true;
            }

            return !session.AttendanceRecords.Any();
        }
    }
}
