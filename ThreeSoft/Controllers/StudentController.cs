using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreeSoft.Entities;
using System.Linq;
using System.Threading.Tasks;
using ThreeSoft.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ThreeSoft.Controllers
{ 
    public class StudentController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public StudentController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager; 
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
                .Where(n => n.UserId == userId && n.IsLocked == false)
                .ToListAsync();

            var checklists = await _context.Checklists
                .Where(c => c.UserId == userId)
                .Include(c => c.Tasks)
                .ToListAsync();

            var ParentPinViewModel = new ParentPinViewModel
            {
                ParentPin = student.ParentPin,
                ParentUnlocked = false
            };

            var model = new InteractViewModel
            {
                Student = student,
                Notes = notes,
                Checklists = checklists,
                Parent = ParentPinViewModel
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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var parentNote = await _context.Notes
                .Include(n => n.Replies)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (parentNote == null)
            {
                return NotFound();
            }

            var newReply = new Note
            {
                Content = reply,
                UserId = userId,
                IsLocked = false,
                ParentNoteId = noteId,
                CreatedAt = DateTime.UtcNow
            };

            parentNote.Replies.Add(newReply);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SetParentPin(InteractViewModel model)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var student = await _userManager.FindByIdAsync(userId);

            if (student == null)
            {
                return RedirectToAction("Index", "Home");
            }

            student.ParentPin = model.Parent.ParentPin;

            var result = await _userManager.UpdateAsync(student);
            if (result.Succeeded)
            {
                model.Student = student;
                return RedirectToAction("Index", "Student");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return RedirectToAction("Index", "Student");
        }

        [HttpPost]
        public async Task<IActionResult> ViewParentMessages (InteractViewModel model)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var student = await _userManager.FindByIdAsync(userId);

            if (model.Parent.ParentPin == student.ParentPin)
            {
                model.Student = student;

                model.Notes = await _context.Notes
                          .Where(n => n.UserId == userId)
                          .ToListAsync();

                model.Checklists = await _context.Checklists
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Tasks)
                    .ToListAsync();

                model.Parent.ParentUnlocked = true;

                model.Parent.ParentPin = null;

                return View("Index", model);
            }

            return RedirectToAction("Index", "Student");
        }
    }
}