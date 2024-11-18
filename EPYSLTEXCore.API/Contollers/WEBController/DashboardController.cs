using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEXCore.API.Contollers.WEBController
{
    public class DashboardController : Controller


    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.ProfilePic = "/images/user.png";
            ViewBag.EmployeeName = "Test";
            return View();
        }
    }
}
