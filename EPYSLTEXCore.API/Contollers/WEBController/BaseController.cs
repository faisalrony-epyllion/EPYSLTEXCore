using EPYSLTEX.Core.Entities.Gmt;
using EPYSLTEX.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEXCore.API.Contollers
{
    public class BaseController : Controller
    {
        private static LoginUser _user;
        private readonly IUserService _userService;

        protected BaseController(IUserService userService)
        {
            _userService = userService;
        }

        //protected int UserId => User.Identity.GetUserId<int>();
        protected LoginUser AppUser
        {
            get
            {
                _user = _userService.Find(1);
                if (_user == null)
                    throw new Exception("Can't not find logged in user.");

                return _user;
            }
            set
            {
                _user = value;
            }
        }

        //protected string UserIp => Request.UserHostAddress;
        //protected string BaseUrl => Request.Url.GetLeftPart(UriPartial.Authority);
    }
}
