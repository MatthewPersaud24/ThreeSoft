using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThreeSoft.Entities;
using ThreeSoft.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ThreeSoft.Controllers
{
    public class TeacherController : Controller
    {

        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public TeacherController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager; 
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var teacher = await _userManager.FindByIdAsync(userId);

            if (!teacher.isVerified)
            {
                return View("Unverified");
            }

            var classrooms = await _context.Classrooms
                .Where(c => c.TeacherId == userId)
                .Include(c => c.Students)
                .ToListAsync();

            var students = await _context.Users
                .Where(u => u.StudentClassrooms.Any(sc => sc.TeacherId == userId))
                .ToListAsync();

            var model = new TeacherViewModel
            {
                Classrooms = classrooms,
                Students = students,
                SearchResults = new List<User>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SearchStudents(string searchTerm)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var classrooms = await _context.Classrooms
                .Where(c => c.TeacherId == userId)
                .Include(c => c.Students)
                .ToListAsync();

            var students = await _context.Users
                .Where(u => u.StudentClassrooms.Any(sc => sc.TeacherId == userId))
                .ToListAsync();

            var searchResults = await _context.Users
                .Where(u => u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm))
                .ToListAsync();

            searchResults = searchResults.Where(u => _userManager.IsInRoleAsync(u, "Student").Result).ToList();

            var model = new TeacherViewModel
            {
                Classrooms = classrooms,
                Students = students,
                SearchTerm = searchTerm,
                SearchResults = searchResults
            };

            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentToClassroom(string studentId, int classroomId)
        {
            var classroom = await _context.Classrooms.Include(c => c.Students).FirstOrDefaultAsync(c => c.Id == classroomId);
            var student = await _context.Users.FindAsync(studentId);

            if (classroom != null && student != null)
            {
                classroom.Students.Add(student);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudentFromClassroom(string studentId, int classroomId)
        {
            var classroom = await _context.Classrooms.Include(c => c.Students).FirstOrDefaultAsync(c => c.Id == classroomId);
            var student = await _context.Users.FindAsync(studentId);

            if (classroom != null && student != null)
            {
                classroom.Students.Remove(student);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RenameClassroom(int classroomId, string newName)
        {
            var classroom = await _context.Classrooms.FindAsync(classroomId);

            if (classroom != null)
            {
                classroom.Name = newName;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClassroom(int classroomId)
        {
            var classroom = await _context.Classrooms.FindAsync(classroomId);

            if (classroom != null)
            {
                _context.Classrooms.Remove(classroom);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateClassroom(string name)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var classroom = new Classroom { Name = name, TeacherId = userId };

            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
