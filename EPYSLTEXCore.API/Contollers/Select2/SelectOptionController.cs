using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEX.Web.Controllers.Apis
{

    [Route("api/selectoption")]
    public class SelectOptionController : ApiBaseController
    {
   
        private readonly ISelect2Service _select2Service;
        private readonly IUserService _userService;
        public SelectOptionController( IUserService userService,
        ISelect2Service select2Service) : base(userService)
        {
            _userService = userService;
            _select2Service = select2Service;
        }

        

        [Route("knitting-unit-contact")]
        public async Task<IActionResult> GetKnittingUnit()
        {
            return Ok(await _select2Service.GetContactNamesAsync("Knitting Unit"));
            return Ok();
        }

        [Route("knitting-unit")]
        public async Task<IActionResult> GetKnittingUnitId()
        {
            return Ok(await _select2Service.GetKnittingUnit());
        }

        [Route("yarnproducttype")]
        public async Task<IActionResult> GetYarnType()
        {
           // var data = await _textileSqlRepository.GetDataDapperAsync<List<Select2OptionModel>>(CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.YARN_TYPE));
            return Ok( );
        }

     

        [Route("entity-types/{entityTypeName}")]
        public async Task<IActionResult> GetEntityTypesByType(string entityTypeName)
        {
            var data = await _select2Service.GetEntityTypesAsync(entityTypeName);
            return Ok( data);
        }
        


    }
}