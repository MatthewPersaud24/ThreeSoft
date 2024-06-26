using Microsoft.AspNetCore.Mvc;

namespace ThreeSoft.Controllers
{
    public class TeacherController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ClassList()
        {
            return View();
        }

        public IActionResult AddStudent()
        {
            return View();
        }

        public IActionResult RemoveStudent()
        {
            return View();
        }
    }
}