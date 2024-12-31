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
    [Route("api/yarn-yd-req")]
    public class YarnYDRequisitionController : ApiBaseController
    {
        private readonly IYarnYDRequisitionService _service;

        public YarnYDRequisitionController(IUserService userService, IYarnYDRequisitionService service) : base(userService)
        {
            _service = service;
        }

        //[Route("list")]
        [HttpGet]
        [Route("list/{status}/{pageName}")]
        public async Task<IActionResult> GetList(Status status, string pageName)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(status, pageName, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{id}")]
        //[Route("new")]
        public async Task<IActionResult> GetNew(int id)
        {
            return Ok(await _service.GetNewAsync(id));
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _service.GetAsync(id));
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(YarnYDReqMaster model)
        {
            YarnYDReqMaster entity;
            if (model.YDReqMasterID > 0)
            {
                entity = await _service.GetAllAsync(model.YDReqMasterID);

                entity.IsSendForApprove = model.IsSendForApprove;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                YarnYDReqChild child;
                foreach (YarnYDReqChild item in model.Childs)
                {
                    child = entity.Childs.FirstOrDefault(x => x.ItemMasterID == item.ItemMasterID);
                    if (child == null)
                    {
                        item.YDReqMasterID = entity.YDReqMasterID;
                        entity.Childs.Add(item);
                    }
                    else
                    {
                        child.ItemMasterID = item.ItemMasterID;
                        child.PhysicalCount = item.PhysicalCount;
                        child.Remarks = item.Remarks;
                        child.NoOfThread = item.NoOfThread;
                        child.UnitID = item.UnitID;
                        child.RequsitionQty = item.RequsitionQty;
                        child.NoOfCone = item.NoOfCone;
                        child.ColorID = item.ColorID;
                        child.ColorCode = item.ColorCode;
                        child.BookingFor = item.BookingFor;
                        child.IsTwisting = item.IsTwisting;
                        child.IsWaxing = item.IsWaxing;
                        child.ShadeCode = item.ShadeCode;
                        child.IsAdditionalItem = item.IsAdditionalItem;
                    }
                }
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                //entity.IsSendForApprove = true;
                entity.SendForApproveBy = AppUser.UserCode;
            }

            await _service.SaveAsync(entity);

            return Ok();
        }

        [Route("saveprocess")]
        [HttpPost]
        public async Task<IActionResult> SaveProcess(YarnYDReqMaster model)
        {
            YarnYDReqMaster entity;
            entity = await _service.GetAllAsync(model.YDReqMasterID);

            if (model.IsApprove)
            {
                entity.IsApprove = true;
                entity.ApproveBy = AppUser.UserCode;
                entity.ApproveDate = DateTime.Now;
            }

            else if (model.IsAcknowledge)
            {
                entity.IsAcknowledge = true;
                entity.AcknowledgeBy = AppUser.UserCode;
                entity.AcknowledgeDate = DateTime.Now;
            }
            else if (model.IsReject)
            {
                //entity.RejectReason = model.RejectReason;
                entity.IsReject = true;
                entity.RejectBy = AppUser.UserCode;
                entity.RejectDate = DateTime.Now;
            }

            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
    }
}
