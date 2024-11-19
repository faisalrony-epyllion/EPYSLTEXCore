using EPYSLTEX.Core.Entities.Gmt;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEXCore.API.Contollers.APIBaseController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiBaseController : ControllerBase
    {
        private static LoginUser _user;
        private static int _loginUserId;
        private readonly IUserService _userService;

        protected ApiBaseController(IUserService userService)
        {
            _userService = userService;
        }
        protected int UserCode
        {
            get
            {
                string userId = User.FindFirst(JwtTokenStorage.UserID)?.Value;
                if (userId == null)
                    throw new Exception("Can't not find logged in user.");

                return userId.ToInt();
            }

        }
        protected LoginUser AppUser
        {
            get
            {
                _user = _userService.Find(UserCode);
                if (_user == null)
                    throw new Exception("Can't not find logged in user.");

                return _user;
            }
            set
            {
                _user = value;
            }
        }
    }
}
