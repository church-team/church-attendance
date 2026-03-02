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
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IUserScopeService _scope;

        public ClassesController(ApplicationDbContext context, IAuditService auditService, IUserScopeService scope)
        {
            _context = context;
            _auditService = auditService;
            _scope = scope;
        }

        public async Task<IActionResult> Index(string? search, int? stageId)
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var allowedClassIds = await _scope.GetAllowedClassIdsAsync();
            stageId = NormalizeStageSelection(stageId, allowedStageIds);

            var query = _context.Classes
                 .Where(s => s.IsActive)
                .Include(c => c.Stage)
                .ApplyClassScope(_scope, allowedStageIds, allowedClassIds);
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search));
            }

            if (stageId.HasValue)
            {
                query = query.Where(c => c.StageId == stageId);
            }

            ViewData["Search"] = search;
            ViewData["StageId"] = stageId;
            ViewData["Stages"] = new SelectList(await _context.Stages.ApplyStageScope(_scope, allowedStageIds).OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
            var items = await query.OrderBy(c => c.Name).ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            await LoadStagesAsync();
            return View(new Class());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Class model)
        {
            if (!await _scope.CanAccessStageAsync(model.StageId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadStagesAsync();
                return View(model);
            }

            _context.Classes.Add(model);
            await _context.SaveChangesAsync();
            await _auditService.LogCreateAsync(model);
            TempData["SuccessMessage"] = "تمت إضافة الفصل بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _scope.CanAccessClassAsync(id))
            {
                return Forbid();
            }

            var item = await _context.Classes.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            await LoadStagesAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Class model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!await _scope.CanAccessClassAsync(id) || !await _scope.CanAccessStageAsync(model.StageId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadStagesAsync();
                return View(model);
            }

            var original = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (original == null)
            {
                return NotFound();
            }

            model.CreatedAt = original.CreatedAt;
            model.UpdatedAt = DateTime.UtcNow;
            _context.Update(model);
            await _context.SaveChangesAsync();
            await _auditService.LogUpdateAsync(original, model);
            TempData["SuccessMessage"] = "تم تحديث الفصل بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!await _scope.CanAccessClassAsync(id))
            {
                return Forbid();
            }

            var item = await _context.Classes
                .Include(c => c.Stage)
                .Include(c => c.Children)
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
            if (!await _scope.CanAccessClassAsync(id))
            {
                return Forbid();
            }

            var item = await _context.Classes.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _auditService.LogSoftDeleteAsync(item);
            TempData["SuccessMessage"] = "تم تعطيل الفصل بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadStagesAsync()
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            ViewData["StageId"] = new SelectList(await _context.Stages.ApplyStageScope(_scope, allowedStageIds).OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
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
