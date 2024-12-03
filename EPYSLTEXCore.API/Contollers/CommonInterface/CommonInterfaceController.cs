using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        public CommonInterfaceController(IUserService userService,  ICommonInterfaceService service
        ) : base(userService)
        {
             
            _service = service;
            _userService = userService;
           // _sqlQueryRepository = sqlQueryRepository;
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
        [Route("list")]
        public async Task<IActionResult> GetList(int menuId)
        {
             var menuData = await _service.GetConfigurationAsync(menuId);
           // return Ok(JsonSerializer.Serialize(menuData, options));
            return Ok();
        }


   
        [Route("finderdata/{menuId}")]
        public async Task<IActionResult> GetFinderData(int menuId)
        {

            var paginationInfo = Request.GetPaginationInfo();
            CommonInterfaceMaster commonInterfaceMaster = await _service.GetConfigurationAsync(menuId);
            string connKey = commonInterfaceMaster.ConName;
            CommonInterfaceChild commonInterfaceChild = commonInterfaceMaster.Childs.Where(p=>p.FinderSql!=null).FirstOrDefault();
            if (commonInterfaceChild != null && !string.IsNullOrWhiteSpace(connKey))
            {
                string finderSql = commonInterfaceChild.FinderSql;
               var records = await _service.GetFinderData(finderSql, connKey, commonInterfaceMaster.PrimaryKeyColumn, paginationInfo);
                return Ok(new TableResponseModel(records, paginationInfo.GridType));
            }
             
           return Ok();
           
        }
        [Route("details/{menuId}/{id}")]
        public async Task<IActionResult> GetCommonInterfaceDetails(int menuId, int id)
        {
            var interfaceInfo = await _service.GetMasterDetailsAsync(menuId);

            var query = $@"Select * From {interfaceInfo.TableName} Where {interfaceInfo.PrimaryKeyColumn} = {id}";
            //dynamic entity = await _sqlQueryRepository.GetFirstOrDefaultDynamicDataDapperAsync(query);
            //Dictionary<string, object> record = ExtensionMethods.ToDictionary(entity);

            //if (entity != null)
            //{
            //    var childTableInfo = interfaceInfo.ChildGrids.FirstOrDefault();
            //    var childQuery = $@"Select * From {childTableInfo.TableName} Where {childTableInfo.ParentColumn} = {id}";
            //    var childRecords = await _sqlQueryRepository.GetDynamicDataDapperAsync(childQuery);
            //    record.Add("Childs", childRecords);
            //}
            //else record.Add("Childs", Array.Empty<object>());

            //foreach (var item in interfaceInfo.Childs)
            //{
            //    if (!item.EntryType.Equals("select", StringComparison.OrdinalIgnoreCase)) continue;

            //    var key = Regex.Replace(item.ColumnName, "Id", "", RegexOptions.IgnoreCase);
            //    key += "List";

            //    var records = await _sqlQueryRepository.GetDynamicDataDapperAsync(item.SelectSql);
            //    record.Add(key, records);
            //}

            //foreach (var item in interfaceInfo.ChildGridColumns)
            //{
            //    if (!item.EntryType.Equals("select", StringComparison.OrdinalIgnoreCase)) continue;

            //    var key = Regex.Replace(item.ColumnName, "Id", "", RegexOptions.IgnoreCase);
            //    key += "List";

            //    var records = await _sqlQueryRepository.GetDynamicDataDapperAsync(item.SelectSql);
            //    record.Add(key, records);
            //}

            return Ok();
        }
    }
}
