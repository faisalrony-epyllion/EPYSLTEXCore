using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace EPYSLTEXCore.API.Contollers.ReportAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportAPIController : ControllerBase
    {
        private readonly IReportAPISetupService _setupService;
        private readonly ILogger<ReportAPIController> _logger;
        private readonly IMemoryCache _cache;
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1,1); //// For restric same cache key access multiple user at a time

        public ReportAPIController(IReportAPISetupService reportAPISetupService,ILogger<ReportAPIController> logger, IMemoryCache cache)
        {
            this._setupService = reportAPISetupService;
            this._logger = logger;
            this._cache = cache;
        }
        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(ReportAPISetup), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllReportAPISetup()
        {
            //try
            //{
                if (_cache.TryGetValue(InMemoryCacheKeys.APIReports, out IEnumerable<ReportAPISetup>? lst)) //// Check date in cache
                {
                    _logger.LogInformation("Report api data found in cache.");
                }
                else
                {
                    try
                    {
                        await _semaphoreSlim.WaitAsync(); //// Await untill other user save or get data from the cache

                        if (_cache.TryGetValue(InMemoryCacheKeys.APIReports, out lst)) //// Check date in cache
                        {
                            _logger.LogInformation("Report api data found in cache.");
                        }
                        else
                        {
                            _logger.LogInformation("Report api data not found in cache. Fatching from the database.");

                            lst = await _setupService.GetAllAsync();

                            var cacheEntryOptions = new MemoryCacheEntryOptions() //// Cache settings
                                .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                                .SetPriority(CacheItemPriority.Normal)
                                .SetSize(1);
                            _cache.Set(InMemoryCacheKeys.APIReports, lst, cacheEntryOptions); //// Save data in Cache
                        }
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
                }
                if (lst != null)
                {
                    return Ok(lst);
                }
                else
                {
                    return NotFound();
                }
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500, ex.Message);
            //}
        }

        [HttpGet("GetAll/{reportname}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReportAPISetup))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllReportAPISetup(string reportname)
        {
            //try
            //{
                if (reportname == "") {
                    return StatusCode(StatusCodes.Status400BadRequest, "Report Name can not be empty!");
                }
                var lst = await _setupService.GetAPIReportByReportName(reportname);
                if (lst != null)
                {
                    //_logger.LogInformation($"{SuccessKeys.Success} {reportname}");
                    return Ok(lst);
                }
                else
                {
                    //_logger.LogWarning($"API Report Not Found for ReportNme: {reportname}");
                    return NotFound();
                }
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            //}
        }

        [HttpPost("Add")]
        [ProducesResponseType(typeof(ReportAPISetup), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddReportAPISetup([FromBody] ReportAPISetup reportAPISetup)
        {
            //try
            //{
                var lst = await _setupService.AddAsync(reportAPISetup);
                _cache.Remove(InMemoryCacheKeys.APIReports); //// Remove cache key
                return Ok(lst);
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            //}
        }
        [HttpDelete("Delete")]
        [ProducesResponseType(typeof(ReportAPISetup), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteReportAPISetup(string reportName)
        {
            //try
            //{
                var lst = await _setupService.DeleteAsync(reportName);
                return Ok(lst);
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            //}
        }

        [HttpGet("generic-report")]
        [ProducesResponseType(typeof(ReportAPISetup), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGenericReport(string username, string token, string reportName, string values)
        {
            //try
            //{
                var lst = await _setupService.GetDynamicReportAPIDataAsync(username, token, reportName, values);

                // Serialize the result using Newtonsoft.Json
                var jsonResult = JsonConvert.SerializeObject(lst);

                return Ok(jsonResult);
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            //}
        }
        //public async Task<JsonResult> GetMenus(int applicationId)
        //{
        //    var records = await _reportSuiteSqlRepository.GetMenusAsync(UserId, applicationId, AppUser.CompanyId);
        //    return Json(records, JsonRequestBehavior.AllowGet);
        //}
    }
}
