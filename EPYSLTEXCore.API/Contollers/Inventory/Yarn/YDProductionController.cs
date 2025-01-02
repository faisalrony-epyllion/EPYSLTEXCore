using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;
namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yarn-production")]
    public class YDProductionController : ApiBaseController
    {
        private readonly IYDProductionService _service;
        public YDProductionController(IYDProductionService service, IUserService userService) : base(userService)
        {
            _service = service;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDProductionMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{newId}/{itemMasterID}/{colorID}/{ydDBatchID}")]
        public async Task<IActionResult> GetNew(int newId, int itemMasterID, int colorID, int ydDBatchID)
        {
            return Ok(await _service.GetNewAsync(newId, itemMasterID, colorID, ydDBatchID));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _service.GetAsync(id));
        }

        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(dynamic jsnString)
        {
            YDProductionMaster model = JsonConvert.DeserializeObject<YDProductionMaster>(
                Convert.ToString(jsnString),
                new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
                });

            YDProductionMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.YDProductionMasterID);

                entity.YDProductionDate = model.YDProductionDate;
                entity.DMID = model.DMID;
                entity.ShiftID = model.ShiftID;
                entity.OperatorID = model.OperatorID;
                entity.BatchNo = model.BatchNo;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                YDProductionChild child;

                foreach (YDProductionChild item in model.Childs)
                {
                    child = entity.Childs.FirstOrDefault(x => x.ItemMasterID == item.ItemMasterID);
                    if (child == null)
                    {
                        child = item;
                        child.EntityState = EntityState.Added;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.YarnDyedColorID = item.YarnDyedColorID;
                        child.Remarks = item.Remarks;
                        child.ProducedQty = item.ProducedQty;
                        child.ProducedCone = item.ProducedCone;
                        child.TodayProductionQty = item.TodayProductionQty;
                        child.EntityState = EntityState.Modified;
                    }
                }

                foreach (var item in entity.Childs.Where(x => x.EntityState == EntityState.Unchanged))
                    item.EntityState = EntityState.Deleted;
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
            }

            await _service.SaveAsync(entity);

            return Ok();
        }

        [HttpPost]
        [Route("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            YDProductionMaster entity;
            entity = await _service.GetAsync(id);
            entity.IsApprove = true;
            entity.ApproveDate = DateTime.Now;
            entity.ApproveBy = AppUser.UserCode;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("acknowledge/{id}")]
        public async Task<IActionResult> Acknowledge(int id)
        {
            YDProductionMaster entity;
            entity = await _service.GetAsync(id);
            entity.IsAcknowledge = true;
            entity.AcknowledgeBy = AppUser.UserCode;
            entity.AcknowledgeDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
    }
}
