using churchAttendace.Data;
using churchAttendace.Models.Entities;
using churchAttendace.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel();
            var withReplies = await _context.Complaints
                .Where(c => c.Reply != null && c.RepliedAt != null)
                .OrderByDescending(c => c.RepliedAt)
                .Select(c => new ComplaintWithReplyViewModel
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    Reply = c.Reply!,
                    RepliedAt = c.RepliedAt!.Value
                })
                .ToListAsync();
            model.ComplaintsWithReplies = withReplies;
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Announcements()
        {
            var items = await _context.Announcements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(items);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitComplaint([FromForm] string? complaintText)
        {
            if (string.IsNullOrWhiteSpace(complaintText))
            {
                TempData["ErrorMessage"] = "يرجى كتابة الشكوى أو المقترح.";
                return RedirectToAction(nameof(Index));
            }

            var complaint = new Complaint
            {
                Text = complaintText.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم استلام شكواك/مقترحك بنجاح. شكراً لك.";
            return RedirectToAction(nameof(Index));
        }
    }
}
