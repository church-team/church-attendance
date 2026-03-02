using churchAttendace.Data;
using churchAttendace.Models.Entities;
using churchAttendace.Services.Interfaces;
using churchAttendace.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "CanManageStages")]
    public class StagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IUserScopeService _scope;

        public StagesController(ApplicationDbContext context, IAuditService auditService, IUserScopeService scope)
        {
            _context = context;
            _auditService = auditService;
            _scope = scope;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var allowedStageIds = await _scope.GetAllowedStageIdsAsync();
            var query = _context.Stages.Where(s => s.IsActive).ApplyStageScope(_scope, allowedStageIds);
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.Name.Contains(search));
            }

            ViewData["Search"] = search;
            var items = await query.OrderBy(s => s.Name).ToListAsync();
            return View(items);
        }

        public IActionResult Create()
        {
            if (!_scope.IsAdmin())
            {
                return Forbid();
            }

            return View(new Stage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Stage stage)
        {
            if (!_scope.IsAdmin())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(stage);
            }

            _context.Stages.Add(stage);
            await _context.SaveChangesAsync();
            await _auditService.LogCreateAsync(stage);
            TempData["SuccessMessage"] = "تمت إضافة المرحلة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _scope.CanAccessStageAsync(id))
            {
                return Forbid();
            }

            var stage = await _context.Stages.FindAsync(id);
            if (stage == null)
            {
                return NotFound();
            }

            return View(stage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Stage stage)
        {
            if (id != stage.Id)
            {
                return NotFound();
            }

            if (!await _scope.CanAccessStageAsync(id))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(stage);
            }

            var original = await _context.Stages.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (original == null)
            {
                return NotFound();
            }

            stage.CreatedAt = original.CreatedAt;
            stage.UpdatedAt = DateTime.UtcNow;
            _context.Update(stage);
            await _context.SaveChangesAsync();
            await _auditService.LogUpdateAsync(original, stage);
            TempData["SuccessMessage"] = "تم تحديث المرحلة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!await _scope.CanAccessStageAsync(id))
            {
                return Forbid();
            }

            var stage = await _context.Stages
                .Include(s => s.Classes)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stage == null)
            {
                return NotFound();
            }

            return View(stage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            if (!await _scope.CanAccessStageAsync(id))
            {
                return Forbid();
            }

            var stage = await _context.Stages.FindAsync(id);
            if (stage == null)
            {
                return NotFound();
            }

            stage.IsActive = false;
            stage.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _auditService.LogSoftDeleteAsync(stage);
            TempData["SuccessMessage"] = "تم تعطيل المرحلة بنجاح";
            return RedirectToAction(nameof(Index));
        }
    }
}
