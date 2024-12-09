﻿using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Contollers.ReportAPI;
using EPYSLTEXCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EPYSLTEXCore.API.Contollers.Menu
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuAPIController : Controller
    {
        private readonly IMenuService _IMenuService;
        private readonly ILogger<ReportAPIController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IUserService _userService;
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1); //// For restric same cache key access multiple user at a time

        public MenuAPIController(IMenuService IMenuService, ILogger<ReportAPIController> logger, IMemoryCache cache,IUserService userService)
        {
            this._IMenuService = IMenuService;
            this._logger = logger;
            this._cache = cache;
            this._userService = userService;
        }

        [HttpGet("GetAllMenu/{applicationId}")]
        public async Task<IActionResult> GetAllMenu(int applicationId)
        {
            //var UserInfo = AppUser;
            //var lst = await _IMenuService.GetMenusAsync(UserInfo.UserCode, applicationId, UserInfo.CompanyId);// for report project
            var lst = await _IMenuService.GetMenusAsync(1, applicationId, 2);
            return Ok(lst);
        }

        [HttpGet("GetAllMenuReport/{applicationId}")]
        public async Task<IActionResult> GetAllMenuReport(int applicationId)
        {
            //var UserInfo = AppUser;
            //var lst = await _IMenuService.GetMenusAsync(UserInfo.UserCode, applicationId, UserInfo.CompanyId);// for report project
            var lst = await _IMenuService.GetAllMenuReport(1, applicationId, 2);
            return Ok(lst);
        }
    }
}
