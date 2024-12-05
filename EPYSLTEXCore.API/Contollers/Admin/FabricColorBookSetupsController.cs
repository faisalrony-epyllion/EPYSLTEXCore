using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General;
using EPYSLTEXCore.Infrastructure.Exceptions;
//using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
namespace EPYSLTEXCore.API.Contollers.RND
{
    [Route("api/fabric-color-book-setups")]

    public class FabricColorBookSetupsController : ApiBaseController
    {
        private readonly IFabricColorBookSetupService _service;
        public FabricColorBookSetupsController(IUserService userService, IFabricColorBookSetupService service) : base(userService)
        {
            _service = service;
        }
        [HttpGet]
        [Route("list")]
        public async Task<IActionResult> GetList()
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("allcolor")]
        public async Task<IActionResult> GetAllColorList()
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetAllColorAsync(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllList()
        {
            return Ok(await _service.GetAllListAsync());
        }

        [HttpGet]
        [Route("new")]
        public async Task<IActionResult> GetNew()
        {
            return Ok(await _service.GetNewAsync());
        }

        [HttpPost]
        [Route("")]
        //[ValidateModel, CheckApiModelForNull]
        public async Task<IActionResult> Add(FabricColorBookSetup model)
        {
            await _service.SaveAsync(model);
            return Ok();
        }

        [HttpPut]
        [Route("")]
        //[ValidateModel, CheckApiModelForNull]
        public async Task<IActionResult> Edit(FabricColorBookSetup model)
        {
            var entity = await _service.GetAsync(model.PTNID);
            Guard.Against.NullObject(entity);

            model.EntityState = System.Data.Entity.EntityState.Modified;
            await _service.SaveAsync(model);
            return Ok();
        }

        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteFabricTechnicalNameSubClassSetup(int id)
        {
            var entity = await _service.GetAsync(id);
            Guard.Against.NullObject(entity);

            entity.EntityState = System.Data.Entity.EntityState.Deleted;
            await _service.SaveAsync(entity);

            return Ok();
        }

    }
}
