using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Contollers.ReportAPI;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;

namespace EPYSLTEXCore.API.Contollers.Menu
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuAPIController : ApiBaseController
    {
        private readonly IMenuService _IMenuService;
        private readonly ILogger<ReportAPIController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IUserService _userService;
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1); //// For restric same cache key access multiple user at a time

        public MenuAPIController(IMenuService IMenuService, ILogger<ReportAPIController> logger, IMemoryCache cache, IUserService userService) : base(userService)
        {
            this._IMenuService = IMenuService;
            this._logger = logger;
            this._cache = cache;
            this._userService = userService;
        }


        [HttpGet("GetAllMenu/{applicationId}")]
        public async Task<IActionResult> GetAllMenu(int applicationId)
        {


            var lst = await _IMenuService.GetMenusAsync(AppUser.UserCode, applicationId, 2);
            return Ok(lst);
        }

        [AllowAnonymous]
        [HttpGet("GetAllMenuReport/{applicationId}")]
        public async Task<IActionResult> GetAllMenuReport(int applicationId)
        {
            var userId = "0";
            var companyId = "0";

            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
                companyId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "CompanyId")?.Value;

                var lst = await _IMenuService.GetAllMenuReport(Convert.ToInt32(userId), applicationId, Convert.ToInt32(companyId));
                return Ok(lst);
            }
            else
            {
                return Unauthorized(new { Message = "Authorization header is missing or invalid." });
            }
        }
    }
}
