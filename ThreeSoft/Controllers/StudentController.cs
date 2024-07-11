using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreeSoft.Entities;
using System.Linq;
using System.Threading.Tasks;
using ThreeSoft.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

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
        public async Task<IActionResult> ViewParentMessages(InteractViewModel model)
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

        public async Task<IActionResult> ExportPlan()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var student = await _userManager.FindByIdAsync(userId);

            var notes = await _context.Notes
                .Where(n => n.UserId == userId)
                .ToListAsync();

            var checklists = await _context.Checklists
                .Where(c => c.UserId == userId)
                .Include(c => c.Tasks)
                .ToListAsync();

            StringBuilder csvContent = new StringBuilder();

            csvContent.AppendLine("Notes:");
            csvContent.AppendLine("Id,Content,UserId,IsLocked,ParentNoteId,CreatedAt");
            foreach (var note in notes)
            {
                csvContent.AppendLine($"{note.Id},{note.Content},{note.UserId},{note.IsLocked},{note.ParentNoteId},{note.CreatedAt}");
            }

            csvContent.AppendLine();
            csvContent.AppendLine("Checklists:");
            csvContent.AppendLine("Id,Title,UserId");
            csvContent.AppendLine("Tasks:");
            csvContent.AppendLine("Id,Task,IsCompleted,ChecklistId");
            foreach (var checklist in checklists)
            {
                csvContent.AppendLine($"{checklist.Id},{checklist.Title},{checklist.UserId}");
                foreach (var task in checklist.Tasks)
                {
                    csvContent.AppendLine($"{task.Id},{task.Task},{task.IsCompleted},{task.ChecklistId}");
                }
            }

            var filename = $"LearningPlan_{student.UserName}_{DateTime.Now:yyyy-MM-dd}.csv";
            System.IO.File.WriteAllText(filename, csvContent.ToString());

            byte[] fileBytes = System.IO.File.ReadAllBytes(filename);
            return File(fileBytes, "text/csv", filename);
        }

        public async Task<IActionResult> ImportPlan(IFormFile file)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var student = await _userManager.FindByIdAsync(userId); 
            
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                // Skip headers
                var headers = reader.ReadLine();

                // Process each line
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var values = line.Split(',');
                    
                    if (values[0] == "Notes:")
                    {
                        while (!reader.EndOfStream)
                        {
                            var noteLine = await reader.ReadLineAsync();
                            if (noteLine == "Checklists:")
                                break;

                            var noteValues = noteLine.Split(',');
                            var note = new Note
                            {
                                Id = int.Parse(noteValues[0]),
                                Content = noteValues[1],
                                UserId = noteValues[2],
                                IsLocked = bool.Parse(noteValues[3]),
                                ParentNoteId = int.Parse(noteValues[4]),
                                CreatedAt = DateTime.Parse(noteValues[5])
                            };
                            _context.Notes.Add(note);
                        }
                    }

                    // Import Checklists and Tasks
                    if (values[0] == "Checklists:")
                    {
                        while (!reader.EndOfStream)
                        {
                            var checklistLine = await reader.ReadLineAsync();
                            if (checklistLine == "Tasks:")
                                break;

                            var checklistValues = checklistLine.Split(',');
                            var checklist = new Checklist
                            {
                                Id = int.Parse(checklistValues[0]),
                                Title = checklistValues[1],
                                UserId = checklistValues[2]
                            };
                            _context.Checklists.Add(checklist);
                        }

                        while (!reader.EndOfStream)
                        {
                            var taskLine = await reader.ReadLineAsync();
                            var taskValues = taskLine.Split(',');
                            var task = new ChecklistTask
                            {
                                Id = int.Parse(taskValues[0]),
                                Task = taskValues[1],
                                IsCompleted = bool.Parse(taskValues[2]),
                                ChecklistId = int.Parse(taskValues[3])
                            };
                            _context.ChecklistTasks.Add(task);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}