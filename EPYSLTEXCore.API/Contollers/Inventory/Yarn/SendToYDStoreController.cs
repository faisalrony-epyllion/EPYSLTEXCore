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
    [Route("api/send-to-yd-store")]
    public class SendToYDStoreController : ApiBaseController
    {
        private readonly ISendToYDStoreService _service;

        public SendToYDStoreController(IUserService userService, ISendToYDStoreService service) : base(userService)
        {
            _service = service;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<SendToYDStoreMaster> records = await _service.GetPagedAsync(status, paginationInfo);
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
            SendToYDStoreMaster model= JsonConvert.DeserializeObject<SendToYDStoreMaster>(Convert.ToString(jsonString));
            SendToYDStoreMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.SendToYDStoreMasterID);

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

                entity.SendToYDStoreDate = model.SendToYDStoreDate;
                entity.Remarks = model.Remarks;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                foreach (SendToYDStoreChild item in model.Childs)
                {
                    var child = entity.Childs.FirstOrDefault(x => x.SendToYDStoreChildID == item.SendToYDStoreChildID);

                    if (child == null)
                    {
                        item.SendToYDStoreMasterID = entity.SendToYDStoreMasterID;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.SendQty = item.SendQty;
                        child.SendConeQty = item.SendConeQty;
                        child.SendPacketQty = item.SendPacketQty;
                        child.Remarks = item.Remarks;
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
            }

            await _service.SaveAsync(entity);

            return Ok();
        }

    }
}
