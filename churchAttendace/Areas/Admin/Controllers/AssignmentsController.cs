using churchAttendace.Data;
using churchAttendace.Models.Entities;
using churchAttendace.Models.ViewModels;
using churchAttendace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public AssignmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var stageManagers = await _context.StageManagerStages
                .Include(s => s.Stage)
                .Include(s => s.User)
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.AssignedAt)
                .Select(s => new StageManagerAssignmentRow
                {
                    Id = s.Id,
                    StageName = s.Stage!.Name,
                    ManagerEmail = s.User!.Email ?? s.User.UserName ?? string.Empty,
                    AssignedAt = s.AssignedAt
                })
                .ToListAsync();

            var classServants = await _context.ClassServants
                .Include(c => c.Class)
                .Include(c => c.User)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.AssignedAt)
                .Select(c => new ClassServantAssignmentRow
                {
                    Id = c.Id,
                    ClassName = c.Class!.Name,
                    ServantEmail = c.User!.Email ?? c.User.UserName ?? string.Empty,
                    AssignedAt = c.AssignedAt
                })
                .ToListAsync();

            var stageManagerUsers = (await _userManager.GetUsersInRoleAsync("StageManager"))
                .Where(u => u.IsActive)
                .OrderBy(u => u.Email)
                .ToList();
            
            var servantUsers = (await _userManager.GetUsersInRoleAsync("Servant"))
                .Where(u => u.IsActive)
                .OrderBy(u => u.Email)
                .ToList();

            var model = new AssignmentViewModel
            {
                StageManagers = stageManagers,
                ClassServants = classServants,
                StageOptions = new SelectList(await _context.Stages.OrderBy(s => s.Name).ToListAsync(), "Id", "Name"),
                ClassOptions = new SelectList(await _context.Classes.OrderBy(c => c.Name).ToListAsync(), "Id", "Name"),
                ManagerOptions = new SelectList(stageManagerUsers, "Id", "Email"),
                ServantOptions = new SelectList(servantUsers, "Id", "Email")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStageManager(string userId, int stageId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "يرجى اختيار المشرف";
                return RedirectToAction(nameof(Index));
            }

            if (stageId == 0)
            {
                TempData["ErrorMessage"] = "يرجى اختيار المرحلة";
                return RedirectToAction(nameof(Index));
            }

            var exists = await _context.StageManagerStages.AnyAsync(s => s.UserId == userId && s.StageId == stageId && s.IsActive);
            if (exists)
            {
                TempData["ErrorMessage"] = "تم تعيين المشرف لهذه المرحلة مسبقاً";
                return RedirectToAction(nameof(Index));
            }

            var assignment = new StageManagerStage
            {
                UserId = userId,
                StageId = stageId,
                AssignedByUserId = _userManager.GetUserId(User),
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.StageManagerStages.Add(assignment);
            await _context.SaveChangesAsync();
            await _auditService.LogCreateAsync(assignment);
            TempData["SuccessMessage"] = "تم تعيين المشرف بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClassServant(string userId, int classId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "يرجى اختيار الخادم";
                return RedirectToAction(nameof(Index));
            }

            if (classId == 0)
            {
                TempData["ErrorMessage"] = "يرجى اختيار الفصل";
                return RedirectToAction(nameof(Index));
            }

            var exists = await _context.ClassServants.AnyAsync(c => c.UserId == userId && c.ClassId == classId && c.IsActive);
            if (exists)
            {
                TempData["ErrorMessage"] = "تم تعيين الخادم لهذا الفصل مسبقاً";
                return RedirectToAction(nameof(Index));
            }

            var assignment = new ClassServant
            {
                UserId = userId,
                ClassId = classId,
                AssignedByUserId = _userManager.GetUserId(User),
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ClassServants.Add(assignment);
            await _context.SaveChangesAsync();
            await _auditService.LogCreateAsync(assignment);
            TempData["SuccessMessage"] = "تم تعيين الخادم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStageManagerUser(string newManagerEmail, string newManagerPassword)
        {
            if (string.IsNullOrWhiteSpace(newManagerEmail) || string.IsNullOrWhiteSpace(newManagerPassword))
            {
                TempData["ErrorMessage"] = "يرجى إدخال البريد الإلكتروني وكلمة المرور للمشرف الجديد";
                return RedirectToAction(nameof(Index));
            }

            var existingUser = await _userManager.FindByEmailAsync(newManagerEmail);
            ApplicationUser managerUser;

            if (existingUser != null)
            {
                managerUser = existingUser;
            }
            else
            {
                managerUser = new ApplicationUser
                {
                    UserName = newManagerEmail,
                    Email = newManagerEmail,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(managerUser, newManagerPassword);
                if (!createResult.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(" | ", createResult.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!await _userManager.IsInRoleAsync(managerUser, "StageManager"))
            {
                await _userManager.AddToRoleAsync(managerUser, "StageManager");
            }

            TempData["SuccessMessage"] = "تم حفظ المشرف الجديد، ويمكنك الآن اختياره من القائمة.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateServantUser(string newServantEmail, string newServantPassword)
        {
            if (string.IsNullOrWhiteSpace(newServantEmail) || string.IsNullOrWhiteSpace(newServantPassword))
            {
                TempData["ErrorMessage"] = "يرجى إدخال البريد الإلكتروني وكلمة المرور للخادم الجديد";
                return RedirectToAction(nameof(Index));
            }

            var existingUser = await _userManager.FindByEmailAsync(newServantEmail);
            ApplicationUser servantUser;

            if (existingUser != null)
            {
                servantUser = existingUser;
            }
            else
            {
                servantUser = new ApplicationUser
                {
                    UserName = newServantEmail,
                    Email = newServantEmail,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(servantUser, newServantPassword);
                if (!createResult.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join(" | ", createResult.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!await _userManager.IsInRoleAsync(servantUser, "Servant"))
            {
                await _userManager.AddToRoleAsync(servantUser, "Servant");
            }

            TempData["SuccessMessage"] = "تم حفظ الخادم الجديد، ويمكنك الآن اختياره من القائمة.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateStageManager(int id)
        {
            var item = await _context.StageManagerStages.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            item.IsActive = false;
            await _context.SaveChangesAsync();
            await _auditService.LogSoftDeleteAsync(item);
            TempData["SuccessMessage"] = "تم إلغاء تعيين المشرف";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateClassServant(int id)
        {
            var item = await _context.ClassServants.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            item.IsActive = false;
            await _context.SaveChangesAsync();
            await _auditService.LogSoftDeleteAsync(item);
            TempData["SuccessMessage"] = "تم إلغاء تعيين الخادم";
            return RedirectToAction(nameof(Index));
        }
    }
}
