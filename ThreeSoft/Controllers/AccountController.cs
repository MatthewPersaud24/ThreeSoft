using Microsoft.AspNetCore.Mvc;
using ThreeSoft.Models;
using System.Threading.Tasks;
using ThreeSoft.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ThreeSoft.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newUser = new User();

                if (model.UserType == "Student")
                {
                    newUser = new Student { FirstName = model.FirstName, LastName = model.LastName, UserName = model.Username };
                }
                else if (model.UserType == "Teacher")
                {
                    newUser = new Teacher { FirstName = model.FirstName, LastName = model.LastName, UserName = model.Username };                    
                }

                var result = await _userManager.CreateAsync(newUser, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, model.UserType);
                    await _signInManager.SignInAsync(newUser, isPersistent: false);

                    return RedirectToAction("Index", model.UserType);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult LogIn(string returnURL = "")
        {
            var model = new LoginViewModel { ReturnUrl = returnURL };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password,
                            isPersistent: model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (roles.Contains("Teacher"))
                        {
                            return RedirectToAction("Index", "Teacher");
                        }
                        else 
                        {
                            return RedirectToAction("Index", "Student");
                        }
                    }
                }
            }

            ModelState.AddModelError("", "Invalid username/password.");
            return View(model);
        }

        [HttpGet]
        public ViewResult AccessDenied()
        {
            return View();
        }
    }
}