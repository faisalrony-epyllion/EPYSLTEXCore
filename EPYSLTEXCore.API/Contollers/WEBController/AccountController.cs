using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEXCore.API.Contollers
{
    public class AccountController : Controller
    {
        [HttpGet, AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction(actionName: "Index", controllerName: "Dashboard");
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }
    }
}
