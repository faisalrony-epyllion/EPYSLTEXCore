using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEXCore.API.Contollers.WEBController
{
    public class DashboardController : BaseController
    {
        private readonly IUserService _userService;
        public DashboardController(IUserService userService
           ) : base(userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public ActionResult Index()
        {
             

            ViewBag.ProfilePic = "/images/user.png";
            ViewBag.EmployeeName =  AppUser.Name;
            ViewBag.ViewBag =  AppUser.IsSuperUser;
            return View();
        }
    }
}
