using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using Microsoft.AspNetCore.Mvc;

namespace EPYSLTEXCore.API.Contollers.RND
{
    [Route("api/fabric-con-sub-class-tech-name")]
    public class FabricConstructionSubClassTechnicalNameController : ApiBaseController
    {
        private readonly IFabricConstructionSubClassTechnicalNameService _service;
        public FabricConstructionSubClassTechnicalNameController(IFabricConstructionSubClassTechnicalNameService service, IUserService userService) : base(userService)
        {
            _service = service;
        }
        [Route("list")]
        public async Task<IActionResult> GetFabricConstructionSubClassTechnicalNames()
        {
            List<FabricConstructionSubClassTechnicalName> records = await _service.GetFabricConstructionSubClassTechnicalNames();
            return Ok(records);
        }
    }
}
