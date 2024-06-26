using Microsoft.AspNetCore.Mvc;

namespace ThreeSoft.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ManageUsers()
        {
            return View();
        }
    }
}