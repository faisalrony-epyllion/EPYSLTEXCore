using Microsoft.AspNetCore.Mvc;

namespace EPYSLEMSCore.API.Contollers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
