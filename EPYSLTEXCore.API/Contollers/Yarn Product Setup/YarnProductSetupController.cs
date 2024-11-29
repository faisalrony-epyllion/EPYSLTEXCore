using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.DataAccess;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Application.Interfaces.YarnProductSetup;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Text.Json;

namespace EPYSLTEXCore.API.Contollers.Yarn_Product_Setup
{
    [Route("api/[controller]")]
    [ApiController]
    public class YarnProductSetupController : ApiBaseController
    {
        IUserService _userService;
        IDapperCRUDService<YarnProductSetup> _dapperCRUDService;
        IYarnProductSetupService _yarnProductSetupService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public YarnProductSetupController(IUserService userService, IYarnProductSetupService yarnProductSetupService, IDapperCRUDService<YarnProductSetup> dapperCRUDService, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(userService)
        {
            _userService = userService;
            _configuration = configuration;
            _dapperCRUDService = dapperCRUDService;
            _yarnProductSetupService = yarnProductSetupService;
            _dapperCRUDService.Connection = new SqlConnection(_configuration.GetConnectionString(AppConstants.TEXTILE_CONNECTION));



            _httpContextAccessor = httpContextAccessor;
            var userClaims = _httpContextAccessor.HttpContext?.User?.Claims;

            if (userClaims != null)
            {
                _dapperCRUDService.UserCode = Convert.ToInt32(userClaims.FirstOrDefault(c => c.Type == JwtTokenStorage.UserID)?.Value);
            }
        }
        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> SaveYarnProductSetup(dynamic entity)
        {
            YarnProductSetup model = JsonConvert.DeserializeObject<YarnProductSetup>(Convert.ToString(entity));
            var yarnProductSetupModel = await _dapperCRUDService.SaveEntityAsync(model);
            return Ok(yarnProductSetupModel.SetupMasterID);
        }

        //[Route("ewo-list/{buyerIds}")]
        //public async Task<IHttpActionResult> GetEWOList(string buyerIds)
        //{
        //    var paginationInfo = _httpContextAccessor.HttpContext.Request.GetPaginationInfo();
        //    List<YarnPOChildOrder> records = await _service.GetChildOrdersAsync(buyerIds, paginationInfo);
        //    return Ok(new TableResponseModel(records, paginationInfo.GridType));
        //}

        [Route("fiberType/{getall}")]
        public async Task<IActionResult> GetAllFiberType()
        {
          
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _yarnProductSetupService.GetAllFiberType(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
    }
}
