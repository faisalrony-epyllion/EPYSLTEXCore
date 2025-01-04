using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yd-dryer-finishing")]
    public class YDDryerFinishingController : ApiBaseController
    {
        private readonly IYDDryerFinishingService _service;

        public YDDryerFinishingController(IYDDryerFinishingService service, IUserService userService) : base(userService)
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
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            YDDryerFinishingMaster model = JsonConvert.DeserializeObject<YDDryerFinishingMaster>(Convert.ToString(jsonString));

            YDDryerFinishingMaster entity;
            if (model.YDDryerFinishingMasterID > 0)
            {
                entity = await _service.GetAllAsync(model.YDDryerFinishingMasterID);

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
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                YDDryerFinishingChild child;
                foreach (YDDryerFinishingChild item in model.Childs)
                {
                    child = entity.Childs.FirstOrDefault(x => x.ItemMasterID == item.ItemMasterID);
                    if (child == null)
                    {
                        item.YDDryerFinishingMasterID = entity.YDDryerFinishingMasterID;
                        entity.Childs.Add(item);
                    }
                    else
                    {
                        child.ItemMasterID = item.ItemMasterID;
                        child.ColorID = item.ColorID;
                        child.YDBItemReqID = item.YDBItemReqID;
                        child.Qty = item.Qty;
                        child.Cone = item.Cone;
                        child.Remarks = item.Remarks;
                        child.YarnCategory = item.YarnCategory;
                        child.UnitID = item.UnitID;
                        child.EntityState = EntityState.Modified;
                    }
                }
                entity.Childs.Where(o => o.EntityState == EntityState.Unchanged).ToList().SetDeleted();
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                //entity.IsSendForApprove = true;
                //entity.SendForApproveBy = UserId;
            }

            await _service.SaveAsync(entity);

            return Ok();
        }

        [Route("saveprocess")]
        [HttpPost]
        public async Task<IActionResult> SaveProcess(dynamic jsonString)
        {
            YDDryerFinishingMaster model = JsonConvert.DeserializeObject<YDDryerFinishingMaster>(Convert.ToString(jsonString));

            YDDryerFinishingMaster entity;
            entity = await _service.GetAllAsync(model.YDDryerFinishingMasterID);

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

            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
    }
}
