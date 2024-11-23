using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEX.Web.Controllers.Apis
{

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

       
    }
}