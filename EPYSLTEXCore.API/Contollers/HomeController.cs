using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEXCore.API.Contollers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
