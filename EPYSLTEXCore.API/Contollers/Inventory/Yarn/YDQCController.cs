using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yarn-qc")]
    public class YDQCController : ApiBaseController
    {
        private readonly IYDQCService _service;

        public YDQCController(IUserService userService, IYDQCService service) : base(userService)
        {
            _service = service;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDQCMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
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

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            YDQCMaster model = JsonConvert.DeserializeObject<YDQCMaster>(Convert.ToString(jsonString));
            YDQCMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.YDQCMasterID);

                if (model.IsSendForApprove && model.IsApprove == false && model.IsReject == false)
                {
                    entity.IsSendForApprove = model.IsSendForApprove;
                    entity.SendForApproveBy = AppUser.UserCode;
                    entity.SendForApproveDate = DateTime.Now;
                }
                if (model.IsApprove)
                {
                    entity.IsApprove = true;
                    entity.ApproveBy = AppUser.UserCode;
                    entity.ApproveDate = DateTime.Now;
                }
                else if (model.IsReject)
                {
                    //entity.RejectReason = model.RejectReason;
                    entity.IsReject = true;
                    entity.RejectBy = AppUser.UserCode;
                    entity.RejectDate = DateTime.Now;
                }

                entity.YDQCDate = model.YDQCDate;
                entity.Remarks = model.Remarks;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                foreach (YDQCChild item in model.Childs)
                {
                    var child = entity.Childs.FirstOrDefault(x => x.YDQCChildID == item.YDQCChildID);

                    if (child == null)
                    {
                        item.YDQCMasterID = entity.YDQCMasterID;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.LotNo = item.LotNo;
                        child.ProductionQty = item.ProductionQty;
                        child.SupplierID = item.SupplierID;
                        child.SpinnerID = item.SpinnerID;
                        child.Remarks = item.Remarks;
                        if (item.QCPass)
                        {
                            child.QCPass = true;
                            child.QCPassBy = AppUser.UserCode;
                            child.QCPassDate = DateTime.Now;
                            child.QCFail = false;
                            child.QCFailBy = 0;
                            child.QCFailDate = null;
                            child.ReTest = false;
                            child.ReTestBy = 0;
                            child.ReTestDate = null;
                        }
                        else if (item.QCFail)
                        {
                            child.QCPass = false;
                            child.QCPassBy = 0;
                            child.QCPassDate = null;
                            child.QCFail = true;
                            child.QCFailBy = AppUser.UserCode;
                            child.QCFailDate = DateTime.Now;
                            child.ReTest = false;
                            child.ReTestBy = 0;
                            child.ReTestDate = null;
                        }
                        else if (item.ReTest)
                        {
                            child.QCPass = false;
                            child.QCPassBy = 0;
                            child.QCPassDate = null;
                            child.QCFail = false;
                            child.QCFailBy = 0;
                            child.QCFailDate = null;
                            child.ReTest = true;
                            child.ReTestBy = AppUser.UserCode;
                            child.ReTestDate = DateTime.Now;
                        }
                        child.EntityState = EntityState.Modified;
                    }
                }
                foreach (var item in entity.Childs.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                }
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                foreach (var item in entity.Childs)
                {
                    if (item.QCPass)
                    {
                        item.QCPass = true;
                        item.QCPassBy = AppUser.UserCode;
                        item.QCPassDate = DateTime.Now;
                        item.QCFail = false;
                        item.QCFailBy = 0;
                        item.QCFailDate = null;
                        item.ReTest = false;
                        item.ReTestBy = 0;
                        item.ReTestDate = null;
                    }
                    else if (item.QCFail)
                    {
                        item.QCPass = false;
                        item.QCPassBy = 0;
                        item.QCPassDate = null;
                        item.QCFail = true;
                        item.QCFailBy = AppUser.UserCode;
                        item.QCFailDate = DateTime.Now;
                        item.ReTest = false;
                        item.ReTestBy = 0;
                        item.ReTestDate = null;
                    }
                    else if (item.ReTest)
                    {
                        item.QCPass = false;
                        item.QCPassBy = 0;
                        item.QCPassDate = null;
                        item.QCFail = false;
                        item.QCFailBy = 0;
                        item.QCFailDate = null;
                        item.ReTest = true;
                        item.ReTestBy = AppUser.UserCode;
                        item.ReTestDate = DateTime.Now;
                    }
                }
            }

            await _service.SaveAsync(entity);

            return Ok();
        }
    }
}
