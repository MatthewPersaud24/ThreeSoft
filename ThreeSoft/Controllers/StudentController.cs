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
            if (reply != null) {
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
            }

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

            //saves notes first
            csvContent.AppendLine("Notes:");
            csvContent.AppendLine("Content,IsLocked,ParentNoteId,CreatedAt");
            foreach (var note in notes)
            {
                csvContent.AppendLine($"{note.Content},{note.IsLocked},{note.ParentNoteId},{note.CreatedAt}");
            }

            //saves checklists
            csvContent.AppendLine("Checklists:");
            csvContent.AppendLine("Title");
            
            foreach (var checklist in checklists)
            {
                csvContent.AppendLine($"{checklist.Title}");

                //tasks for each checklist are saved directly underneath
                csvContent.AppendLine("Tasks:");
                csvContent.AppendLine("Task,IsCompleted");
                foreach (var task in checklist.Tasks)
                {
                    csvContent.AppendLine($"{task.Task},{task.IsCompleted}");
                }

                csvContent.AppendLine();
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
                // Process each line
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var values = line.Split(',');

                    //Go through notes in .csv
                    if (values[0] == "Notes:")
                    {
                        var headers = reader.ReadLine(); 
                        
                        while (!reader.EndOfStream)
                        {
                            line = await reader.ReadLineAsync();
                            values = line.Split(','); 
                            
                            if (line == "Checklists:" || line == "Tasks:")
                                break;

                            var parentNoteId = values[2];
                            int? parentNoteIdValue = null;
                            if (!string.IsNullOrEmpty(parentNoteId))
                            {
                                if (int.TryParse(parentNoteId, out int parsedParentNoteId))
                                {
                                    parentNoteIdValue = parsedParentNoteId;
                                }
                            }

                            //creates note
                            var note = new Note
                            {
                                Content = values[0],
                                UserId = userId,
                                IsLocked = bool.Parse(values[1]),
                                ParentNoteId = parentNoteIdValue,
                                CreatedAt = DateTime.Parse(values[3])
                            };

                            _context.Notes.Add(note);
                        }
                    }

                    //go through checklists and tasks
                    if (values[0] == "Checklists:")
                    {
                        //Skip headers ("Title, UserId")
                        line = reader.ReadLine();
                        int checklistId = 0;

                        while (!reader.EndOfStream)
                        {
                            line = await reader.ReadLineAsync();
                            values = line.Split(',');

                            //tasks for the specified list
                            if (line == "Tasks:")
                            {
                                //Skip headers ("Task, IsCompleted")
                                line = reader.ReadLine();

                                while (!reader.EndOfStream)
                                {
                                    line = await reader.ReadLineAsync();
                                    values = line.Split(',');

                                    //breaks when no more tasks for specified list
                                    if (line == "")
                                        break;

                                    var task = new ChecklistTask
                                    {
                                        Task = values[0],
                                        IsCompleted = bool.Parse(values[1]),
                                        ChecklistId = checklistId
                                    };
                                    _context.ChecklistTasks.Add(task);
                                }
                            }
                            else if (line == "")
                                break;

                            //creates new checklist
                            else
                            {
                                var checklist = new Checklist
                                {
                                    Title = values[0],
                                    UserId = userId
                                };
                                _context.Checklists.Add(checklist);
                                await _context.SaveChangesAsync();

                                checklistId = checklist.Id;
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}