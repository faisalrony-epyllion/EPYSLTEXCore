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
    [Route("api/yarn-dyeing-soft-winding")]
    public class YarnDyeingSoftWindingController: ApiBaseController
    {
        private readonly IYarnDyeingSoftWindingService _service;

        public YarnDyeingSoftWindingController(IYarnDyeingSoftWindingService service, IUserService userService) : base(userService)
        {
            _service = service;
        }
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
        public async Task<IActionResult> Save(dynamic jsnString)
        {

            SoftWindingMaster model = JsonConvert.DeserializeObject<SoftWindingMaster>(
                 Convert.ToString(jsnString),
                 new JsonSerializerSettings
                 {
                     DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
                 });

            SoftWindingMaster entity;
            if (model.SoftWindingMasterID > 0)
            {
                entity = await _service.GetAllAsync(model.SoftWindingMasterID);

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

                SoftWindingChild child;
                foreach (SoftWindingChild item in model.Childs)
                {
                    child = entity.Childs.FirstOrDefault(x => x.ItemMasterID == item.ItemMasterID);
                    if (child == null)
                    {
                        item.SoftWindingMasterID = entity.SoftWindingMasterID;
                        entity.Childs.Add(item);
                    }
                    else
                    {
                        child.ItemMasterID = item.ItemMasterID;
                        child.ColorID = item.ColorID;
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
        public async Task<IActionResult> SaveProcess(dynamic jsnString)
        {
            SoftWindingMaster model = JsonConvert.DeserializeObject<SoftWindingMaster>(
                 Convert.ToString(jsnString),
                 new JsonSerializerSettings
                 {
                     DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
                 });

            SoftWindingMaster entity;
            entity = await _service.GetAllAsync(model.SoftWindingMasterID);

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
