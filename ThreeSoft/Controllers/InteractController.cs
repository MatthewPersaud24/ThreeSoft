using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreeSoft.Entities;
using ThreeSoft.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ThreeSoft.Controllers
{
    public class InteractController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InteractController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string studentId)
        {
            var student = await _context.Users.FindAsync(studentId);
            if (student == null)
            {
                return NotFound();
            }

            var notes = await _context.Notes
                .Where(n => n.UserId == studentId)
                .Include(n => n.Replies)
                .ToListAsync();

            var checklists = await _context.Checklists
                .Where(c => c.UserId == studentId)
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
        public async Task<IActionResult> AddNote(string studentId, string content, bool isLocked)
        {
            if (content != null) {
                var note = new Note
                {
                    UserId = studentId,
                    Content = content,
                    IsLocked = isLocked,
                };

                _context.Notes.Add(note);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { studentId });
        }

        [HttpPost]
        public async Task<IActionResult> AddChecklist(string studentId, string title)
        {
            if (title != null) {
                var checklist = new Checklist
                {
                    UserId = studentId,
                    Title = title
                };

                _context.Checklists.Add(checklist);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { studentId });
        }

        [HttpPost]
        public async Task<IActionResult> AddChecklistTask(string studentId, int checklistId, string task)
        {
            if (task != null)
            {
                var checklistTask = new ChecklistTask
                {
                    ChecklistId = checklistId,
                    Task = task
                };

                _context.ChecklistTasks.Add(checklistTask);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { studentId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChecklistTask(int taskId)
        {
            var task = await _context.ChecklistTasks.FindAsync(taskId);
            if (task != null)
            {
                _context.ChecklistTasks.Remove(task);
                await _context.SaveChangesAsync();
            }

            var checklist = await _context.Checklists.FindAsync(task.ChecklistId);
            return RedirectToAction("Index", new { studentId = checklist.UserId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChecklist(int checklistId)
        {
            var checklist = await _context.Checklists.FindAsync(checklistId);
            if (checklist != null)
            {
                _context.Checklists.Remove(checklist);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { studentId = checklist.UserId });
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

            var checklist = await _context.Checklists.FindAsync(task.ChecklistId);
            return RedirectToAction("Index", new { studentId = checklist.UserId });
        }
    }
}
