using AutoMapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

namespace EPYSLTEXCore.API.Contollers.Yarn_Product_Setup
{
    [Route("api/[controller]")]
    [ApiController]
    public class YarnProductSetupController : ApiBaseController
    {
        IUserService _userService;
        IDapperCRUDService<YarnProductSetup> _dapperCRUDService;
        private readonly IConfiguration _configuration;

        public YarnProductSetupController(IUserService userService, IDapperCRUDService<YarnProductSetup> dapperCRUDService, IConfiguration configuration) : base(userService)
        {
            _userService = userService;
            _configuration = configuration;
            _dapperCRUDService = dapperCRUDService;
            _dapperCRUDService.Connection = new SqlConnection(_configuration.GetConnectionString(AppConstants.TEXTILE_CONNECTION));
          
                }
        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> SaveYarnProductSetup(dynamic entity)
        {
            
            

            YarnProductSetup model = JsonConvert.DeserializeObject<YarnProductSetup>(Convert.ToString(entity));
             
            //CommonHelpers.SetNullsToDefaultValues(model);
            var yarnProductSetupModel =   await _dapperCRUDService.SaveEntityAsync(model);

            //  var s =  await _setupService.AddNestedAsync(entity);

            return Ok(yarnProductSetupModel.SetupMasterID);
        }
    }
}
