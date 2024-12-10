using Azure.Core;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yarn-qc-receive")]
    public class YarnQCReceiveController : ApiBaseController
    {
        private readonly IYarnQCReceiveService _service;
        public YarnQCReceiveController(IYarnQCReceiveService service)
        {
            _service = service;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IHttpActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnQCReceiveMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{qcIssuerMasterId}")]
        //[Route("new/{QCReqMasterId}")]
        public async Task<IHttpActionResult> GetNew(int qcIssuerMasterId)
        {
            return Ok(await _service.GetNewAsync(qcIssuerMasterId));
        }


        [Route("{id}")]
        [HttpGet]
        public async Task<IHttpActionResult> Get(int id)
        {
            var record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IHttpActionResult> Save(YarnQCReceiveMaster model)
        {
            YarnQCReceiveMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.QCReceiveMasterID);

                entity.QCReceiveDate = model.QCReceiveDate;
                entity.ReceiveID = model.ReceiveID;
                entity.UpdatedBy = UserId;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.YarnQCReceiveChilds.SetUnchanged();

                foreach (YarnQCReceiveChild item in model.YarnQCReceiveChilds)
                {
                    var existingYarnReceiveChilds = entity.YarnQCReceiveChilds.FirstOrDefault(x => x.QCReceiveChildID == item.QCReceiveChildID);

                    if (existingYarnReceiveChilds == null)
                    {
                        item.QCReceiveMasterID = entity.QCReceiveMasterID;
                        item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        //item.ReqQtyCarton = item.ReqBagPcs;
                        entity.YarnQCReceiveChilds.Add(item);
                    }
                    else
                    {
                        existingYarnReceiveChilds.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        //existingYarnReceiveChilds.ReqQtyCarton = item.ReqBagPcs;
                        existingYarnReceiveChilds.ReceiveQty = item.ReceiveQty;
                        existingYarnReceiveChilds.ShadeCode = item.ShadeCode;
                        existingYarnReceiveChilds.ReceiveQtyCone = item.ReceiveQtyCone;
                        existingYarnReceiveChilds.ReceiveQtyCarton = item.ReceiveQtyCarton;
                        existingYarnReceiveChilds.EntityState = EntityState.Modified;
                    }
                }
                foreach (var item in entity.YarnQCReceiveChilds.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                }
            }
            else
            {
                entity = model;
                foreach (YarnQCReceiveChild item in entity.YarnQCReceiveChilds)
                {
                    item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                    //item.ReqQtyCarton = item.ReqBagPcs;
                }
                entity.AddedBy = UserId;
                entity.QCReceivedBy = UserId;
            }
            await _service.SaveAsync(entity);
            return Ok();
        }
    }
}
