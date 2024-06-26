using Microsoft.AspNetCore.Mvc;

namespace ThreeSoft.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Notes()
        {
            return View();
        }

        public IActionResult Checklist()
        {
            return View();
        }
    }
}