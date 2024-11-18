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
using System.Data.Entity;
using EPYSLTEX.Core.Entities;

namespace EPYSLTEXCore.API.Contollers
{
    public class AccountController : BaseController
    {
        private readonly IUserService _userService;
        private readonly Encryption _encryption;
        private readonly TokenBuilder _tokenBuilder;

        public AccountController(IUserService userService
           , TokenBuilder tokenBuilder
           ) : base(userService)
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

            //LoginHistory loginHistory = this.GetLoginHistory(user.UserCode);
            //loginHistory.UserCode = user.UserCode;
            //loginHistory.LogInTime = DateTime.Now;
            //loginHistory.EntityState = EntityState.Added;
            //await _loginHistoryService.SaveAsync(loginHistory);

            return Json(new { statusCode = HttpStatusCode.OK, accessToken = token });

        }
        [HttpGet]
        public async Task<ActionResult> LogOff()
        {
            var s = AppUser;
            #region LogOutTime Set
            //LoginHistory loginHistory = this.GetLoginHistory(AppUser.UserCode);
            //loginHistory = await _loginHistoryService.GetAsync(loginHistory);
            //if (loginHistory.IsNotNull())
            //{
            //    loginHistory.LogOutTime = DateTime.Now;
            //    loginHistory.EntityState = EntityState.Modified;
            //    await _loginHistoryService.SaveAsync(loginHistory);
            //}
            #endregion

            //AppUser = null;

            return RedirectToAction("Login");
        }

        private LoginHistory GetLoginHistory(int userCode)
        {
            LoginHistory loginHistory = new LoginHistory();
            if (Request.IsNotNull())
            {
                //loginHistory.IPAddress = Request.UserHostName;
                //loginHistory.OpenPortNo = Request.Url.Port;
                //loginHistory.LogonUserIdentityName = Request.LogonUserIdentity.Name;
                //loginHistory.UserHostName = Request.UserHostName;
                //loginHistory.MachineName = Environment.MachineName;

                //if (Request.UserHostAddress.IsNotNull() && userCode != 542)
                //{
                //    IPHostEntry hostEntry = Dns.GetHostEntry(Request.UserHostAddress);
                //    loginHistory.MachineName = hostEntry.HostName;
                //}
            }
            loginHistory.MachineUserName = System.Environment.UserName;
            return loginHistory;
        }


    }
}
