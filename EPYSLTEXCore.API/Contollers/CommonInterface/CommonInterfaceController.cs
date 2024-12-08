using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using System.Text.Json;
using System.Text.Json.Nodes;

namespace EPYSLTEXCore.API.Contollers.CommonInterface
{
    [Route("api/common-interface")]
    [ApiController]

    public class CommonInterfaceController : ApiBaseController
    {
        // private readonly ISqlQueryRepository<BaseDTO> _sqlQueryRepository;
        private readonly IUserService _userService;
        private readonly ICommonInterfaceService _service;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        public CommonInterfaceController(IUserService userService, ICommonInterfaceService service, IMemoryCache memoryCache
        ) : base(userService)
        {

            _service = service;
            _userService = userService;
            _memoryCache = memoryCache;
            // _sqlQueryRepository = sqlQueryRepository;
        }
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            // Enabling PascalCase for this specific response
            PropertyNamingPolicy = null // This keeps properties in PascalCase
        };

        public async Task<IEnumerable<CommonInterfaceMaster>> GetOrCreateCacheValue(string cacheKey, int applicationId)
        {
            // Check if the value is in cache
            if (!_memoryCache.TryGetValue(InMemoryCacheKeys.CommonInterfaceConfig, out IEnumerable<CommonInterfaceMaster> commonInterfaceMasterlst))
            {
                commonInterfaceMasterlst = await _service.GetConfigurationAsyncByApplicationID(applicationId);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSize(1)  // Specify the size
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetPriority(CacheItemPriority.High);
                // Set data in cache with an expiration time (e.g., 5 minutes)
                _memoryCache.Set(cacheKey, commonInterfaceMasterlst, cacheEntryOptions);



            }

            // Return the cached value
            return commonInterfaceMasterlst;

        }
        [Route("configs")]
        public async Task<IActionResult> GetConfigs(int menuId)
        {


            var commonInterfaceMasterlst = await GetOrCreateCacheValue(InMemoryCacheKeys.CommonInterfaceConfig, AppConstants.APPLICATION_ID);
            var menuData = commonInterfaceMasterlst.FirstOrDefault(p => p.MenuId == menuId);
            return Ok(JsonSerializer.Serialize(menuData, options));

        }
        [Route("list")]
        public async Task<IActionResult> GetList(int menuId)
        {



            var commonInterfaceMasterlst = await GetOrCreateCacheValue(InMemoryCacheKeys.CommonInterfaceConfig, AppConstants.APPLICATION_ID);
            var menuData = commonInterfaceMasterlst.FirstOrDefault(p => p.MenuId == menuId);
             
            return Ok(JsonSerializer.Serialize(menuData, options));
        }



        [Route("finderdata/{menuId}")]
        public async Task<IActionResult> GetFinderData(int menuId)
        {

            var paginationInfo = Request.GetPaginationInfo();
            var commonInterfaceMasterlst = await GetOrCreateCacheValue(InMemoryCacheKeys.CommonInterfaceConfig, AppConstants.APPLICATION_ID);
            CommonInterfaceMaster commonInterfaceMaster = commonInterfaceMasterlst.FirstOrDefault(p => p.MenuId == menuId);           
  
            string connKey = commonInterfaceMaster.ConName;
            CommonInterfaceChild commonInterfaceChild = commonInterfaceMaster.Childs.Where(p => p.FinderSql != null).FirstOrDefault();
            if (commonInterfaceChild != null && !string.IsNullOrWhiteSpace(connKey))
            {
                string finderSql = commonInterfaceChild.FinderSql;
                var records = await _service.GetFinderData(finderSql, connKey, commonInterfaceMaster.PrimaryKeyColumn, paginationInfo);
                return Ok(new TableResponseModel(records, paginationInfo.GridType));
            }

            return Ok();

        }
        [Route("combodata/{menuId}")]
        public async Task<IActionResult> GetComboData(int menuId)
        {

            var commonInterfaceMasterlst = await GetOrCreateCacheValue(InMemoryCacheKeys.CommonInterfaceConfig, AppConstants.APPLICATION_ID);
            CommonInterfaceMaster commonInterfaceMaster = commonInterfaceMasterlst.FirstOrDefault(p => p.MenuId == menuId);
             
            string connKey = commonInterfaceMaster.ChildGrids.FirstOrDefault().ConName;
            var childGridColumns = commonInterfaceMaster.ChildGridColumns;
            foreach (var childGridColumn in childGridColumns)
            {
                string selectSql = childGridColumn.SelectSql;
                var records = await _service.GetDynamicDataAsync(selectSql, connKey);
                return Ok(records);
            }

            return Ok();

        }
        [Route("selectfinderdata/{menuId}/{id}")]
        public async Task<IActionResult> SelectFinderData(int menuId, int id)
        {

            var commonInterfaceMasterlst = await GetOrCreateCacheValue(InMemoryCacheKeys.CommonInterfaceConfig, AppConstants.APPLICATION_ID);
            CommonInterfaceMaster commonInterfaceMaster = commonInterfaceMasterlst.FirstOrDefault(p => p.MenuId == menuId);

          
            string connKey = commonInterfaceMaster.ConName;

            var commonInterfaceChild = commonInterfaceMaster.Childs.Where(p => p.FinderSql != null).FirstOrDefault();
            if (commonInterfaceChild != null && !string.IsNullOrWhiteSpace(connKey))
            {
                string selectSql = commonInterfaceChild.SelectSql;
                var records = await _service.GetDynamicDataAsync(selectSql, connKey, new { id });
                return Ok(records);
            }

            return Ok();

        }
        [Route("save/{menuId}")]
        public async Task<IActionResult> Save(int menuId, dynamic entity)
        {

            string jsonString = JsonSerializer.Serialize(entity);

            // Now parse the string as a JsonObject (or JsonDocument)
            JsonObject jsonObject = JsonSerializer.Deserialize<JsonObject>(jsonString);

            // Extract the 'Childs' array
            var childs = jsonObject["Childs"];

            string json = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true });

            CommonInterfaceMaster commonInterfaceMaster = await _service.GetCommonInterfaceMasterChildAsync(menuId);
            string connKey = commonInterfaceMaster.ConName;
            string tableName = commonInterfaceMaster.TableName;
            string conName = commonInterfaceMaster.ConName;

            _service.Save(tableName, childs, conName);


            return Ok();

        }


    }
}
