using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Data.Entity;
using System.Reflection;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yd-requisition")]
    public class YDRequisitionController : ApiBaseController
    {
        private readonly IYDRequisitionService _service;

        public YDRequisitionController(IUserService userService, IYDRequisitionService service) : base(userService)
        {
            _service = service;
        }

        [Route("list/{status}")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{ydBookingMasterId}/{isBDS}")]
        public async Task<IActionResult> GetNew(int ydBookingMasterId, int isBDS)
        {
            return Ok(await _service.GetNewAsync(ydBookingMasterId, isBDS));
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
        public async Task<IActionResult> Save(YDReqMaster model)
        {
            YDReqMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.YDReqMasterID);

                entity.YDReqDate = model.YDReqDate;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                foreach (YDReqChild item in model.Childs)
                {
                    YDReqChild child = entity.Childs.FirstOrDefault(x => x.YDReqChildID == item.YDReqChildID);

                    if (child == null)
                    {
                        child = item;
                        child.EntityState = EntityState.Added;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.ReqQty = item.ReqQty;
                        child.ReqCone = item.ReqCone;
                        child.Remarks = item.Remarks;
                        child.SpinnerID = item.SpinnerID;
                        child.LotNo = item.LotNo;
                        child.PhysicalCount = item.PhysicalCount;
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
                entity.DateAdded = DateTime.Now;
                entity.YDReqBy = AppUser.UserCode;
            }

            await _service.SaveAsync(entity);

            return Ok();
        }
        [Route("approve")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> ApproveRequisition(YDReqMaster model)
        {

            string ret = "";
            YDReqMaster entity = await _service.GetAllAsync(model.YDReqMasterID);

            entity.IsApprove = true;
            entity.ApproveBy = AppUser.EmployeeCode;
            entity.ApproveDate = DateTime.Now;

            entity.IsReject = false;
            entity.RejectBy = 0;
            entity.RejectDate = null;

            entity.EntityState = EntityState.Modified;

            entity.Childs = new List<YDReqChild>();

            await _service.SaveAsync(entity);



            return Ok(ret);
        }
        [Route("reject")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> RejectRequisition(YDReqMaster model)
        {

            string ret = "";
            YDReqMaster entity = await _service.GetAllAsync(model.YDReqMasterID);

            entity.IsApprove = false;
            entity.ApproveBy = 0;
            entity.ApproveDate = null;

            entity.IsReject = true;
            entity.RejectBy = AppUser.EmployeeCode;
            entity.RejectDate = DateTime.Now;

            entity.EntityState = EntityState.Modified;

            entity.Childs = new List<YDReqChild>();

            await _service.SaveAsync(entity);
            return Ok(ret);
        }
    }
}
