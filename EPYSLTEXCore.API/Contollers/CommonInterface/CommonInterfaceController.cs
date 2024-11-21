using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            // Enabling PascalCase for this specific response
            PropertyNamingPolicy = null // This keeps properties in PascalCase
        };

        [Route("configs")]
        public async Task<IActionResult> GetConfigs(int menuId)
        {
            var menuData = await _service.GetConfigurationAsync(menuId);
            return Ok(JsonSerializer.Serialize(menuData, options));

        }
    }
}
