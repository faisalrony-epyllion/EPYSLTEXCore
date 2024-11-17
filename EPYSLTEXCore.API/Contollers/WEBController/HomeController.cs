using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEXCore.API.Contollers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.ProfilePic = "/Images/user.png";
            ViewBag.EmployeeName = "Anupam";
            return View();
        }
    }
}
