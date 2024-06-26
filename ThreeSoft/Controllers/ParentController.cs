using Microsoft.AspNetCore.Mvc;

namespace ThreeSoft.Controllers
{
    public class ParentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LockedMessages()
        {
            return View();
        }
    }
}