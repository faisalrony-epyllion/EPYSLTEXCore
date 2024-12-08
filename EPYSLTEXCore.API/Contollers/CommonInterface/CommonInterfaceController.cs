using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
 
 
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
        [Route("combodata/{menuId}")]
        public async Task<IActionResult> GetComboData(int menuId)
        {

            
            CommonInterfaceMaster commonInterfaceMaster = await _service.GetConfigurationAsync(menuId);
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
        public async Task<IActionResult> SelectFinderData(int menuId,int id)
        {
 
            CommonInterfaceMaster commonInterfaceMaster = await _service.GetCommonInterfaceMasterChildAsync(menuId);
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
