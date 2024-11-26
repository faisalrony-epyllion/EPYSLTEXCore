using AutoMapper;
using Azure;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEX.Web.Models;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Data.SqlClient;

namespace EPYSLTEXCore.API.Contollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KnittingsController : ApiBaseController
    {
        IUserService _userService;
        IDapperCRUDService<EPYSLTEX.Web.Models.KnittingUnit> _dapperCRUDService;
        private readonly ICommonHelpers _commonHelpers;
        private readonly IConfiguration _configuration;


        public KnittingsController(ICommonHelpers commonHelpers, IUserService userService, IDapperCRUDService<EPYSLTEX.Web.Models.KnittingUnit> dapperCRUDService, IConfiguration configuration) : base(userService)
        {
            _configuration = configuration;
            _commonHelpers = commonHelpers;     
            _userService = userService;          
            _dapperCRUDService = dapperCRUDService;
            _dapperCRUDService.Connection = new SqlConnection(_configuration.GetConnectionString(AppConstants.TEXTILE_CONNECTION));
        }
    [HttpGet]
    [Route("knitting-unit")]
    public IActionResult GetKnittingUnit(int offset = 0, int limit = 10, string filter = null, string sort = null, string order = "asc")
    {
        var filterBy = _commonHelpers.GetFilterByModel(filter);
        //  var records = _knittingUnitRepository.ListAll(offset, limit, filterBy, sort, order, out int count);

        //var response = new TableResponseModel(count, _mapper.Map<List<KnittingUnitViewModel>>(records));

        return Ok();
    }

    [HttpPost]
    [Route("knitting-unit")]
    public async Task<IActionResult> SaveKnittingUnit(EPYSLTEX.Web.Models.KnittingUnit model)
    {
           
            var s = _dapperCRUDService.SaveEntityAsync(model);
           // var s1 = _dapperCRUDService.SaveEntityCompositKeyAsync(model);
            
           
            return Ok(s);
    }


   
}
}
