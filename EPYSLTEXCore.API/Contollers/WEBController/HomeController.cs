using Microsoft.AspNetCore.Mvc;
using EPYSLTEX.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
namespace EPYSLTEXCore.API.Contollers
{
    [Authorize]
    public class HomeController : BaseController
    {

        private readonly IUserService _userService;
        public HomeController(IUserService userService
           ) : base(userService)
        {
            _userService = userService;
        }
        public IActionResult Index(int menuId, string pageName,string navUrlName, string menuParam)
        {
            //var s = AppUser;
            var rootName = "~/Views/";

            ViewBag.MenuId = menuId;
            ViewBag.PageName = pageName;
            ViewBag.MenuParam = menuParam;
            string viewPath= rootName + navUrlName.SplitAndAddUnderscore();
           
            return View(viewPath);
         
          
        }

       
    }
}
