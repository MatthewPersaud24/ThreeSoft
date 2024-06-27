using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreeSoft.Entities;
using ThreeSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace ThreeSoft.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TeacherController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var classrooms = await _context.Classrooms
                                           .Include(c => c.Students)
                                           .Where(c => c.TeacherId == user.Id)
                                           .ToListAsync();
            var students = await _userManager.GetUsersInRoleAsync("Student");

            var model = new TeacherViewModel
            {
                Classrooms = classrooms,
                Students = students.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClassroom(string name)
        {
            var user = await _userManager.GetUserAsync(User);
            var classroom = new Classroom
            {
                Name = name,
                TeacherId = user.Id
            };

            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClassroom(int id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom != null)
            {
                _context.Classrooms.Remove(classroom);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentToClassroom(int classroomId, string studentId)
        {
            var classroom = await _context.Classrooms
                                          .Include(c => c.Students)
                                          .FirstOrDefaultAsync(c => c.Id == classroomId);
            var student = await _userManager.FindByIdAsync(studentId);

            if (classroom != null && student != null)
            {
                classroom.Students.Add(student);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudentFromClassroom(int classroomId, string studentId)
        {
            var classroom = await _context.Classrooms
                                          .Include(c => c.Students)
                                          .FirstOrDefaultAsync(c => c.Id == classroomId);
            var student = await _userManager.FindByIdAsync(studentId);

            if (classroom != null && student != null)
            {
                classroom.Students.Remove(student);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> InteractWithStudent(string studentId)
        {
            // Redirect to a page where teacher can interact with the student (e.g., add notes or checklists)
            return RedirectToAction("Interact", "Student", new { id = studentId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(string studentId)
        {
            var student = await _userManager.FindByIdAsync(studentId);
            if (student != null)
            {
                await _userManager.DeleteAsync(student);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
