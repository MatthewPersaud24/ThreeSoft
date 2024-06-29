using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using ThreeSoft.Entities;
using System.Linq;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;

namespace ThreeSoft.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;

        public AdminController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            var teachers = users.Where(u => _userManager.IsInRoleAsync(u, "Teacher").Result).ToList();
            var students = users.Where(u => _userManager.IsInRoleAsync(u, "Student").Result).ToList();

            var model = new AdminIndexViewModel
            {
                Users = users,
                Teachers = teachers,
                Students = students
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                // Handle failure if needed
            }
            // Handle user not found if needed
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string userName, string newPassword)
        {
            var user = await _userManager.FindByNameAsync(userName);
            

            if (user != null)
            {
                //generate token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                //change password
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword );
                if (result.Succeeded)
                {
         
                    return RedirectToAction("Index");
                }
                // Handle failure if needed
            }
            // Handle user not found if needed
            return RedirectToAction("Index");
        }

    }




    public class AdminIndexViewModel
    {
        public List<User> Users { get; set; }
        public List<User> Teachers { get; set; }
        public List<User> Students { get; set; }
    }
}
