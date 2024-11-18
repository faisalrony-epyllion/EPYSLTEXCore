using EPYSL.Encription;
using EPYSLTEX.Core.Entities.Gmt;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Services;
using EPYSLTEXCore.API.Models.Security;
using EPYSLTEXCore.Application.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using System.Net;
using System.Security.Claims;

namespace EPYSLTEXCore.API.Contollers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly Encryption _encryption;
        private readonly TokenBuilder _tokenBuilder;

        public AccountController(IUserService userService
           , TokenBuilder tokenBuilder
           )
        {

            _userService = userService;
            _tokenBuilder = tokenBuilder;

            _encryption = new Encryption();
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction(actionName: "Index", controllerName: "Dashboard");
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginBindingModel model)
        {
            LoginUser user = await _userService.FindUserForLoginAsync(model.Username);
            if (user == null) return Unauthorized(new { message = "Invalid username or password" });
             
            var password = _encryption.Encrypt(model.Password, model.Username);
            if (password == null) return Unauthorized(new { message = "Invalid username or password" });
             
             
            var expiresAtUtc = DateTime.UtcNow.AddHours(1);
            var token = _tokenBuilder.BuildToken(user, expiresAtUtc);
            return Json(new { statusCode = HttpStatusCode.OK, accessToken = token });

        }
    }
}
