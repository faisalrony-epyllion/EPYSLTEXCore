using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;
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
        [Route("save1")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save1(dynamic jsonString)
        {
            YarnRMProperties model = JsonConvert.DeserializeObject<YarnRMProperties>(Convert.ToString(jsonString));
            YarnRMProperties entity = new YarnRMProperties();

            if (model.YRMPID > 0)
            {
                entity = await _service.GetById(model.YRMPID);
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
            var checkDuplicate = await _service.CheckDuplicateValue(entity);
            if (!checkDuplicate)
            {
                await _service.SaveAsync(entity);
                return Ok();
            }
            else
            {
                return BadRequest("Duplicate data found!!!");
            }
        }
        [Route("save")]
        [HttpPost]

        public async Task<IActionResult> Save(dynamic jsonString)
        {
            YarnRMProperties model = JsonConvert.DeserializeObject<YarnRMProperties>(Convert.ToString(jsonString));
            YarnRMProperties entity;

            if (model.YRMPID > 0)
            {
                entity = await _service.GetById(model.YRMPID);

                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;
                
                entity.Childs.SetUnchanged();

                model.Childs.ForEach(modelChild =>
                {
                    var child = entity.Childs.FirstOrDefault(c => c.YRMPChildID == modelChild.YRMPChildID);

                    if (child.IsNull())
                    {
                        child = modelChild;
                        child.EntityState = EntityState.Added;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.EntityState = EntityState.Modified;
                        child.SupplierID = modelChild.SupplierID;
                        child.SpinnerID = modelChild.SpinnerID;
                    }
                });
                entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            }
            else
            {
                entity = model;
                entity.EntityState = EntityState.Added;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;

                foreach (var item in entity.Childs)
                {
                    item.EntityState = EntityState.Added;
                }
            }
            var checkDuplicate = await _service.CheckDuplicateValue(entity);
            if (!checkDuplicate)
            {
                await _service.SaveAsync(entity);
                return Ok();
            }
            else
            {
                return BadRequest("Duplicate data found!!!");
            }
            //await _service.SaveAsync(entity);

            return Ok();
        }
        [HttpGet]
        [Route("new")]
        public async Task<IActionResult> GetNew()
        {
            return Ok(await _service.GetNewAsync());
        }
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetAsync(int id)
        {
            return Ok(await _service.GetDetails(id));
        }
    }
}
