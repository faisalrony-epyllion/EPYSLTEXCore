using EPYSLTEX.Core.Entities.Tex;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Admin
{
    [Authorize]
    [Route("api/finishing-machine-setup")]
    public class FinishingMachineSetupController : ApiBaseController
    {
        private readonly IFinishingMachineSetupService _service;
        private readonly ICommonHelpers _commonHelpers;
       

        public FinishingMachineSetupController(
             IFinishingMachineSetupService service
            , ICommonHelpers commonHelpers, IUserService userService
           ) : base(userService)
        {
            _service = service;
            _commonHelpers = commonHelpers;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, int offset = 0, int limit = 10, string filter = null, string sort = null, string order = null)
        {
            var filterBy = _commonHelpers.GetFilterBy(filter);
            var orderBy = string.IsNullOrEmpty(sort) ? "" : $"ORDER BY {sort} {order}";

            var records = await _service.GetPagedAsync(status, offset, limit, filterBy, orderBy);
            var totalCount = records.FirstOrDefault() == null ? 0 : records.FirstOrDefault().TotalRows;
            var response = new TableResponseModel(totalCount, records);

            return Ok(response);
        }

        [HttpGet]
        [Route("new/{newId}")]
        public async Task<IActionResult> GetNew(int newId)
        {
            return Ok(await _service.GetNewAsync(newId));
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }

        [Route("machinelist")]
        [HttpGet]
        public async Task<IActionResult> GetMachineList(String processId, string processTypeId)
        {
            var data = await _service.GetAsyncFinishingMachineConfiguration(processId.ToInt(), processTypeId.ToInt());
            return Ok(data);
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(FinishingMachineConfigurationMaster model)
        {
            FinishingMachineConfigurationMaster entity;

            //List<CDASTSIssueChild> childs = model.Childs;
            //_itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childs);

            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.FMCMasterID);
                entity.EntityState = EntityState.Modified;
                entity.FinishingMachineSetups.SetUnchanged();

                foreach (var item in model.FinishingMachineSetups)
                {
                    var child = entity.FinishingMachineSetups.FirstOrDefault(x => x.FMSID == item.FMSID);

                    if (child == null)
                    {
                        child = item;
                        item.FMCMasterID = entity.FMCMasterID;
                        child.EntityState = EntityState.Added;
                        entity.FinishingMachineSetups.Add(child);
                    }
                    else
                    {
                        child.BrandID = item.BrandID;
                        child.UnitID = item.UnitID;
                        child.Capacity = item.Capacity;
                        child.MachineNo = item.MachineNo;
                        child.Param1Value = item.Param1Value;
                        child.Param2Value = item.Param2Value;
                        child.Param3Value = item.Param3Value;
                        child.Param4Value = item.Param4Value;
                        child.Param5Value = item.Param5Value;
                        child.Param6Value = item.Param6Value;
                        child.Param7Value = item.Param7Value;
                        child.Param8Value = item.Param8Value;
                        child.Param9Value = item.Param9Value;
                        child.Param10Value = item.Param10Value;
                        child.Param11Value = item.Param11Value;
                        child.Param12Value = item.Param12Value;
                        child.Param13Value = item.Param13Value;
                        child.Param14Value = item.Param14Value;
                        child.Param15Value = item.Param15Value;
                        child.Param16Value = item.Param16Value;
                        child.Param17Value = item.Param17Value;
                        child.Param18Value = item.Param18Value;
                        child.Param19Value = item.Param19Value;
                        child.Param20Value = item.Param20Value;
                        child.UpdatedBy = AppUser.UserCode;
                        child.DateUpdated = DateTime.Now;

                        child.EntityState = EntityState.Modified;
                    }
                }
                foreach (var item in entity.FinishingMachineSetups.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                }
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;
               
            }

            await _service.SaveAsync(entity);

            return Ok();
        }
       
    }
}