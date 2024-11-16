using EPYSLTEXCore.API.Contollers.ReportAPI;
using EPYSLTEXCore.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace EPYSLTEXCore.API.Contollers.Menu
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuAPIController : ControllerBase
    {
        private readonly IMenuService _IMenuService;
        private readonly ILogger<ReportAPIController> _logger;
        private readonly IMemoryCache _cache;
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1); //// For restric same cache key access multiple user at a time

        public MenuAPIController(IMenuService IMenuService, ILogger<ReportAPIController> logger, IMemoryCache cache)
        {
            this._IMenuService = IMenuService;
            this._logger = logger;
            this._cache = cache;
        }

        [HttpGet("GetAllMenu")]
        public async Task<IActionResult> GetAllMenu(int applicationId)
        {
            var lst = _IMenuService.GetAllAsync();
            return Ok(lst);
        }
    }
}
