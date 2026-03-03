using churchAttendace.Data;
using churchAttendace.Models.Entities;
using churchAttendace.Services.Interfaces;
using churchAttendace.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "CanManageClasses")]
    public class ChildrenController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IUserScopeService _scope;

        public ChildrenController(ApplicationDbContext context, IAuditService auditService, IUserScopeService scope)
        {
            _context = context;
            _auditService = auditService;
            _scope = scope;
        }

        public async Task<IActionResult> Index(string? search, int? classId, int? stageId)
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var allowedClassIds = await _scope.GetAllowedClassIdsAsync();
            classId = NormalizeClassSelection(classId, allowedClassIds);
            stageId = NormalizeStageSelection(stageId, allowedStageIds);

            var query = _context.Children
                .Where(c=>c.IsActive)
                .Include(c => c.Class)
                .ThenInclude(c => c.Stage)
                .ApplyChildScope(_scope, allowedStageIds, allowedClassIds);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.FullName.Contains(search) || (c.ParentName != null && c.ParentName.Contains(search)));
            }

            if (classId.HasValue)
            {
                query = query.Where(c => c.ClassId == classId);
            }

            if (stageId.HasValue)
            {
                query = query.Where(c => c.Class!.StageId == stageId);
            }

            ViewData["Search"] = search;
            ViewData["ClassId"] = classId;
            ViewData["StageId"] = stageId;
            ViewData["Stages"] = new SelectList(await _context.Stages.ApplyStageScope(_scope, allowedStageIds).OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
            ViewData["Classes"] = new SelectList(await _context.Classes.ApplyClassScope(_scope, allowedStageIds, allowedClassIds).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");

            var items = await query.OrderBy(c => c.FullName).ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            await LoadClassesAsync();
            return View(new Child { BirthDate = DateTime.Today.AddYears(-10) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Child model)
        {
            if (!await _scope.CanAccessClassAsync(model.ClassId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadClassesAsync();
                return View(model);
            }

            _context.Children.Add(model);
            await _context.SaveChangesAsync();
            await _auditService.LogCreateAsync(model);
            TempData["SuccessMessage"] = "تمت إضافة الطفل بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _scope.CanAccessChildAsync(id))
            {
                return Forbid();
            }

            var item = await _context.Children.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            await LoadClassesAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Child model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!await _scope.CanAccessChildAsync(id) || !await _scope.CanAccessClassAsync(model.ClassId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadClassesAsync();
                return View(model);
            }

            var original = await _context.Children.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (original == null)
            {
                return NotFound();
            }

            model.CreatedAt = original.CreatedAt;
            model.UpdatedAt = DateTime.UtcNow;
            _context.Update(model);
            await _context.SaveChangesAsync();
            await _auditService.LogUpdateAsync(original, model);
            TempData["SuccessMessage"] = "تم تحديث بيانات الطفل بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!await _scope.CanAccessChildAsync(id))
            {
                return Forbid();
            }

            var item = await _context.Children
                .Include(c => c.Class)
                .ThenInclude(c => c.Stage)
                .FirstOrDefaultAsync(c => c.Id == id);

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
            if (!await _scope.CanAccessChildAsync(id))
            {
                return Forbid();
            }

            var item = await _context.Children.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _auditService.LogSoftDeleteAsync(item);
            TempData["SuccessMessage"] = "تم تعطيل الطفل بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadClassesAsync()
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var allowedClassIds = await _scope.GetAllowedClassIdsAsync();
            ViewData["ClassId"] = new SelectList(await _context.Classes.ApplyClassScope(_scope, allowedStageIds, allowedClassIds).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
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

        private int? NormalizeStageSelection(int? stageId, IReadOnlyCollection<int> allowedStageIds)
        {
            if (!_scope.IsAdmin() && stageId.HasValue && !allowedStageIds.Contains(stageId.Value))
            {
                var fallback = allowedStageIds.FirstOrDefault();
                TempData["ErrorMessage"] = "تم تحديث المرحلة المختارة لأنها لم تعد ضمن صلاحياتك.";
                return fallback == 0 ? null : fallback;
            }

            return stageId;
        }
    }
}
