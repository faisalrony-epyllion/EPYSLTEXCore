using EPYSL.Encription;
using EPYSLTEX.Core.Entities;
using EPYSLTEX.Core.Entities.Gmt;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Models.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace EPYSLTEXCore.API.Contollers
{

    public class AccountController : BaseController
    {
        private readonly IUserService _userService;
        private readonly Encryption _encryption;
        private readonly ITokenBuilder _tokenBuilder;
        private readonly IDeSerializeJwtToken _deSerializeJwtToken;

        public AccountController(IUserService userService
           , ITokenBuilder tokenBuilder
           , IDeSerializeJwtToken deSerializeJwtToken

           ) : base(userService)
        {

            _userService = userService;
            _tokenBuilder = tokenBuilder;

            _encryption = new Encryption();
            _deSerializeJwtToken = deSerializeJwtToken;
        }

   
        [HttpPost]
        public async Task<ActionResult> Login(LoginBindingModel model)
        {
            AppUser = null;
            LoginUser user = await _userService.FindUserForLoginAsync(model.Username);
            if (user == null) return Unauthorized(new { message = "Invalid username or password" });

            var password = _encryption.Encrypt(model.Password, model.Username);
            if (password != user.Password) return Unauthorized(new { message = "Invalid username or password" });


            var expiresAtUtc = DateTime.UtcNow.AddDays(1);
            var token = _tokenBuilder.BuildToken(user, expiresAtUtc);

            var claims = _deSerializeJwtToken.GetClaims(token);


            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);







            //LoginHistory loginHistory = this.GetLoginHistory(user.UserCode);
            //loginHistory.UserCode = user.UserCode;
            //loginHistory.LogInTime = DateTime.Now;
            //loginHistory.EntityState = EntityState.Added;
            //await _loginHistoryService.SaveAsync(loginHistory);

            return Ok(new { statusCode = HttpStatusCode.OK, accessToken = token });

        }
        [HttpGet]
        public async Task<ActionResult> LogOff()
        {
            AppUser = null;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
        public ActionResult ChangePassword()
        {
            return PartialView("~/Views/Account/_ChangePassword.cshtml");
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction(actionName: "Index", controllerName: "Dashboard");
            ViewBag.ReturnUrl = returnUrl;

            return View("~/Views/Account/Login.cshtml");
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


        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            LoginUser user = await _userService.FindUserForLoginAsync(AppUser.UserName);

            try
            {

                if (model.IsMailPassword == 1)
                {
                    var password = _encryption.Encrypt(model.OldPassword, AppUser.UserName);

                    if (password != user.EmailPassword)
                        return Ok(new { message = "Old password doesn't match", StatusCode = 404 });

                    if (model.NewPassword != model.ConfirmPassword)
                        return Ok(new { message = "New password and Confirm Password doesn't match", StatusCode = 404 });

                    if (model.NewPassword == model.OldPassword)
                        return Ok(new { message = "New password can't be same as Old Password", StatusCode = 404 });

                    await _userService.UpdateEmailPasswordAsync(AppUser.UserCode, _encryption.Encrypt(model.NewPassword, AppUser.UserName));
                    return Ok(new { message = "Email Password Updated Successfully", StatusCode = 200 });
                }
                else
                {
                    var password = _encryption.Encrypt(model.OldPassword, AppUser.UserName);

                    if (password != user.Password)
                        return Ok(new { message = "Old password doesn't match", StatusCode = 404 });

                    if (model.NewPassword != model.ConfirmPassword)
                        return Ok(new { message = "New password and Confirm Password doesn't match", StatusCode = 404 });

                    if (model.NewPassword == model.OldPassword)
                        return Ok(new { message = "New password can't be same as Old Password", StatusCode = 404 });

                    await _userService.UpdateUserPasswordAsync(AppUser.UserCode, _encryption.Encrypt(model.NewPassword, AppUser.UserName));
                    return Ok(new { message = "User Password Updated Successfully", StatusCode = 200 });
                }

            }
            catch (Exception ex)
            {
                return Ok(new { message = "Password Updating Failed due to  "+ ex.Message, StatusCode = 404 });
            }
        }
    }
}
