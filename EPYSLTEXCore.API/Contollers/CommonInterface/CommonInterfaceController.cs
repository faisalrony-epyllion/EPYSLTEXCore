using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
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
            var commonInterfaceMasterlst = await GetOrCreateCacheValue(InMemoryCacheKeys.CommonInterfaceConfig, AppConstants.APPLICATION_ID);
            CommonInterfaceMaster commonInterfaceMaster = commonInterfaceMasterlst.FirstOrDefault(p => p.MenuId == menuId);
            string parentNameSpace= commonInterfaceMaster.NameSpace.Trim();
            string childNameSpace = commonInterfaceMaster.ChildGrids.FirstOrDefault().NameSpace.Trim();
            string parentTable = commonInterfaceMaster.TableName.Trim();
            string childTable = commonInterfaceMaster.ChildGrids.FirstOrDefault().TableName.Trim();
            string primaryKeyColumn = commonInterfaceMaster.ChildGrids.FirstOrDefault().PrimaryKeyColumn.Trim();

            string parentTableName = (parentNameSpace + "." + parentTable).Trim();
            string childTableName =  (childNameSpace + "."+ childTable).Trim();
            Assembly assembly = Assembly.LoadFrom(@"D:\EPYSLTEXCore\EPYSLTEXCore.API\bin\Debug\net8.0\EPYSLTEXCore.Infrastructure.dll");
            Type parentEntityType = assembly.GetType(parentTableName);
            Type childEntityType = assembly.GetType(childTableName);
            var parentInstance = Activator.CreateInstance(parentEntityType);

            // Step 4: Create an instance of the class dynamically
            var childInstanceListAdded = new List<object>();
            var childInstanceListUpdated = new List<object>();
            var childInstanceListDeleted = new List<object>();
            string jsonString = JsonSerializer.Serialize(entity);

            // Now parse the string as a JsonObject (or JsonDocument)
            JsonObject jsonObject = JsonSerializer.Deserialize<JsonObject>(jsonString);

            // Extract the 'Childs' array

            JsonArray childsArray = jsonObject["Childs"].AsArray();
            string status = "";

            for (int i = 0; i < childsArray.Count; i++)
            {
                // Access the current JsonNode (item)
                JsonNode childNode = childsArray[i];
                if (childNode is JsonObject jsonObj)
                {
                    var childInstance = Activator.CreateInstance(childEntityType);
                  

                    // Loop through the properties of the JsonObject dynamically
                    foreach (var property in jsonObj)
                    {
                        // Get the property name (key) and value
                        string propertyName = property.Key;
                        JsonNode propertyValue = property.Value;

                        if(propertyName != null && propertyName.ToLower()=="status")
                        {
                            if(propertyValue != null )
                            {
                                status=propertyValue.ToString();
                            }
                           
                        }
                        try
                        {
                            SetProperty(childInstance, propertyName, propertyValue);
                            
                        }
                        catch(Exception e)
                        {
                            var msg = e.Message;
                        }

                    }
                    if(status=="add")
                    {
                        childInstanceListAdded.Add(childInstance);
                    }
                    if(status=="edit")

                    {
                        childInstanceListUpdated.Add(childInstance);
                    }
                    if(status=="delete")
                    {
                        childInstanceListDeleted.Add(childInstance);
                    }
                 
                }
            }

            // Get the type from the assembly using the full class name


            string conName = commonInterfaceMaster.ConName;

           // _service.Save(childTable, childInstanceListDeleted, conName, new List<string>() { primaryKeyColumn }, status);
            _service.Save(childTable, childInstanceListUpdated, conName, new List<string>() { primaryKeyColumn }, status);
            //_service.Save(childTable, childInstanceListAdded, conName,new List<string>() { primaryKeyColumn }, status);


            return Ok();

        }
        static void SetProperty(object target, string propertyName, JsonNode propertyValue)
        {
            var propertyInfo = target.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                // Get the property's type
                Type propertyType = propertyInfo.PropertyType;

                // Convert JsonNode to the appropriate type
                if (propertyValue is JsonValue jsonValue)
                {
                    object value = ConvertJsonValueToType(jsonValue, propertyType);
                    propertyInfo.SetValue(target, value);
                }
            }
        }

        // Helper method to convert JsonNode to the appropriate type  

        static object ConvertJsonValueToType(JsonValue jsonValue, Type targetType)
        {
            // Check if the targetType is nullable
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(int))
            {
                // Handle nullable and non-nullable integers
                return jsonValue.TryGetValue(out int intValue) ? (object)intValue : null;
            }
            else if (underlyingType == typeof(bool))
            {
                // Handle nullable and non-nullable booleans
                return jsonValue.TryGetValue(out bool boolValue) ? (object)boolValue : null;
            }
            else if (underlyingType == typeof(string))
            {
                // Handle nullable and non-nullable strings
                return jsonValue.TryGetValue(out string stringValue) ? (object)stringValue : null;
            }
            else if (underlyingType == typeof(DateTime))
            {
                // Handle nullable and non-nullable DateTime
                return jsonValue.TryGetValue(out DateTime dateTimeValue) ? (object)dateTimeValue : null;
            }
            else if (underlyingType == typeof(DateTime?))
            {
                // Handle nullable DateTime (DateTime?)
                return jsonValue.TryGetValue(out DateTime dateTimeValueNullable) ? (object)dateTimeValueNullable : null;
            }
            else
            {
                // Fallback for unsupported types (throw exception or handle other types)
                throw new InvalidOperationException($"Unsupported type: {underlyingType.FullName}");
            }
        }

    }
}
