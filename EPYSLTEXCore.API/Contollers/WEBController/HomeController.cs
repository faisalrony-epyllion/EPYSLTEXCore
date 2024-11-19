using Microsoft.AspNetCore.Mvc;
using EPYSLTEXCore.API.Extension;
namespace EPYSLTEXCore.API.Contollers
{
    public class HomeController : Controller
    {
        public IActionResult Index(int menuId, string pageName,string navUrlName)
        {
            var rootName = "~/Views/";

            ViewBag.MenuId = menuId;
            ViewBag.PageName = pageName;
            string viewPath= rootName + navUrlName.SplitAndAddUnderscore();
           
            return View(viewPath);
         
          
        }

       
    }
}
