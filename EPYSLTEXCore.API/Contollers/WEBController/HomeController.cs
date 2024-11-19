using Microsoft.AspNetCore.Mvc;
using EPYSLTEX.Core.Interfaces.Services;
namespace EPYSLTEXCore.API.Contollers
{
    public class HomeController : BaseController
    {

        private readonly IUserService _userService;
        public HomeController(IUserService userService
           ) : base(userService)
        {
            _userService = userService;
        }
        public IActionResult Index(int menuId, string pageName,string navUrlName)
        {
            var s = AppUser;
            var rootName = "~/Views/";

            ViewBag.MenuId = menuId;
            ViewBag.PageName = pageName;
            string viewPath= rootName + navUrlName.SplitAndAddUnderscore();
           
            return View(viewPath);
         
          
        }

       
    }
}
