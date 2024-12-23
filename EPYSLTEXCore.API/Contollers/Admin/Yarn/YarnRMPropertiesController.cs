using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Admin
{
    [Route("api/yarn-rm-properties")]
    public class YarnRMPropertiesController : ApiBaseController
    {
        private readonly IYarnRMPropertiesService _service;
        public YarnRMPropertiesController(
            IUserService userService, IYarnRMPropertiesService SegmentFilterSetupService
            ) : base(userService)
        {
            _service = SegmentFilterSetupService;
        }

        [Route("list")]
        public async Task<IActionResult> GetList()
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnRMProperties> records = await _service.GetPagedAsync(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            YarnRMProperties model = JsonConvert.DeserializeObject<YarnRMProperties>(Convert.ToString(jsonString));
            YarnRMProperties entity = new YarnRMProperties();

            if (model.YRMPID > 0)
            {
                entity = await _service.GetAsync(model.YRMPID);
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
            }
            else
            {
                entity = model;
                entity.EntityState = EntityState.Added;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;
            }
            await _service.SaveAsync(entity);
            return Ok();
        }
        [HttpGet]
        [Route("GetMaster")]
        public async Task<IActionResult> GetMaster()
        {
            var data = await _service.GetMaster();
            return Ok(data);
        }
    }
}
