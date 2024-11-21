using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEXCore.API.Contollers.CommonInterface
{
    [Route("api/common-interface")]
    [ApiController]

    public class CommonInterfaceController : ApiBaseController
    {
        private readonly IUserService _userService;
        private readonly ICommonInterfaceService _service;
        public CommonInterfaceController(IUserService userService,  ICommonInterfaceService service
        ) : base(userService)
        {
            _service = service;
            _userService = userService;
        }

        [Route("configs")]
        public async Task<IActionResult> GetConfigs(int menuId)
        {
            return Ok(await _service.GetConfigurationAsync(menuId));
        }

    }
}
