using churchAttendace.Data;
using churchAttendace.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(items);
        }

        public IActionResult Create()
        {
            return View(new Announcement());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Announcements.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم إضافة الإعلان بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Announcements.FindAsync(id);
            if (item == null)
                return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Announcement model)
        {
            if (id != model.Id)
                return NotFound();
            if (!ModelState.IsValid)
                return View(model);

            var item = await _context.Announcements.FindAsync(id);
            if (item == null)
                return NotFound();

            item.Title = model.Title;
            item.Body = model.Body;
            item.IsActive = model.IsActive;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم تحديث الإعلان بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Announcements.FindAsync(id);
            if (item == null)
                return NotFound();
            _context.Announcements.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حذف الإعلان";
            return RedirectToAction(nameof(Index));
        }
    }
}
