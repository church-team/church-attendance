using churchAttendace.Data;
using churchAttendace.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class ComplaintsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ComplaintsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Complaints
                .Include(c => c.RepliedByUser)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Reply(int id)
        {
            var item = await _context.Complaints.FindAsync(id);
            if (item == null)
                return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, [FromForm] string? replyText)
        {
            var item = await _context.Complaints.FindAsync(id);
            if (item == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(replyText))
            {
                TempData["ErrorMessage"] = "يرجى كتابة نص الرد.";
                return View(item);
            }

            item.Reply = replyText.Trim();
            item.RepliedAt = DateTime.UtcNow;
            item.RepliedByUserId = _userManager.GetUserId(User);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حفظ الرد بنجاح. سيظهر الرد على الصفحة الرئيسية للزوار.";
            return RedirectToAction(nameof(Index));
        }
    }
}
