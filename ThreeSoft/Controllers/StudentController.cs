using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreeSoft.Entities;
using System.Linq;
using System.Threading.Tasks;
using ThreeSoft.Models;

namespace ThreeSoft.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var student = await _context.Users.FindAsync(userId);
            if (student == null)
            {
                return NotFound();
            }

            var notes = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsLocked)
                .ToListAsync();

            var checklists = await _context.Checklists
                .Where(c => c.UserId == userId)
                .Include(c => c.Tasks)
                .ToListAsync();

            var model = new InteractViewModel
            {
                Student = student,
                Notes = notes,
                Checklists = checklists
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleChecklistTaskCompletion(int taskId)
        {
            var task = await _context.ChecklistTasks.FindAsync(taskId);
            if (task != null)
            {
                task.IsCompleted = !task.IsCompleted;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ReplyToNote(int noteId, string reply)
        {
            // Implement note reply logic here.
            return RedirectToAction("Index");
        }
    }
}
