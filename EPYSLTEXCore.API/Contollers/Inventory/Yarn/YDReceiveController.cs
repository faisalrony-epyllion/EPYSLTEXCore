using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    
    [Route("api/yd-receive")]
    public class YDReceiveController : ApiBaseController
    {
        private readonly IYDReceiveService _service;

        public YDReceiveController(IYDReceiveService service, IUserService userService) : base(userService)
        {
            _service = service;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("{ydReceiveMasterID}")]
        [HttpGet]
        public async Task<IActionResult> Get(int ydReceiveMasterID)
        {
            var record = await _service.GetAsync(ydReceiveMasterID);
            return Ok(record);
        }
        [Route("new/{ydReqIssueMasterID}")]
        [HttpGet]
        public async Task<IActionResult> GetNew(int ydReqIssueMasterID)
        {
            var record = await _service.GetNewAsync(ydReqIssueMasterID);
            return Ok(record);
        }
        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(dynamic jsnString)
        {
            YDReceiveMaster model = JsonConvert.DeserializeObject<YDReceiveMaster>(
                  Convert.ToString(jsnString),
                  new JsonSerializerSettings
                  {
                      DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
                  });

            YDReceiveMaster entity = new YDReceiveMaster();

            if (model.YDReceiveMasterID > 0)
            {
                entity = await _service.GetAllAsync(model.YDReceiveMasterID);
                entity.CompanyID = model.CompanyID;
                entity.YDReceiveDate = model.YDReceiveDate;
                entity.YDReceiveBy = AppUser.UserCode; 
                entity.Remarks = model.Remarks;

                entity.UpdatedBy = AppUser.UserCode; 
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                foreach (var item in model.Childs)
                {
                    var child = entity.Childs.FirstOrDefault(x => x.YDReceiveChildID == item.YDReceiveChildID);
                    if (child == null)
                    {
                        item.YDReceiveMasterID = entity.YDReceiveMasterID;
                        item.EntityState = EntityState.Added;
                        entity.Childs.Add(item);
                    }
                    else
                    {
                        child.ReceiveQty = item.ReceiveQty;
                        child.ReceiveCone = item.ReceiveCone;
                        child.ReceiveCarton = item.ReceiveCarton;
                        child.Remarks = item.Remarks;
                        child.EntityState = EntityState.Modified;
                    }
                }
                entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            }
            else
            {
                entity = model;
                entity.YDReceiveBy = AppUser.UserCode; 
                entity.AddedBy = AppUser.UserCode; 
                entity.DateAdded = DateTime.Now;
            }

            await _service.SaveAsync(entity, AppUser.UserCode);
            return Ok();
        }

    }
}
