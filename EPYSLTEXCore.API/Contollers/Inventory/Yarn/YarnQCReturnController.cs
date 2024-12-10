using Azure.Core;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yarn-qc-return")]
    public class YarnQCReturnController : ApiBaseController
    {
        private readonly IYarnQCReturnService _service;
        public YarnQCReturnController(IYarnQCReturnService service)
        {
            _service = service;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IHttpActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnQCReturnMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{reqMasterId}")]
        public async Task<IHttpActionResult> GetNew(int reqMasterId)
        {
            return Ok(await _service.GetNewAsync(reqMasterId));
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IHttpActionResult> Get(int id)
        {
            var record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }
        [Route("qc-receive-child/{qcReceiveChildIds}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetDetailsByQCReturnChilds(string qcReceiveChildIds)
        {
            var record = await _service.GetDetailsByQCReturnChilds(qcReceiveChildIds);
            return Ok(record);
        }
        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IHttpActionResult> SaveYarnReturn(YarnQCReturnMaster model)
        {
            YarnQCReturnMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.QCReturnMasterID);

                entity.QCReturnDate = model.QCReturnDate;
                entity.UpdatedBy = UserId;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.YarnQCReturnChilds.SetUnchanged();

                foreach (YarnQCReturnChild item in model.YarnQCReturnChilds)
                {
                    var yarnReturnChildEntity = entity.YarnQCReturnChilds.FirstOrDefault(x => x.QCReturnChildID == item.QCReturnChildID);

                    if (yarnReturnChildEntity == null)
                    {
                        item.QCReturnMasterID = entity.QCReturnMasterID;
                        entity.YarnQCReturnChilds.Add(yarnReturnChildEntity);
                    }
                    else
                    {
                        yarnReturnChildEntity.Remarks = item.Remarks;
                        yarnReturnChildEntity.ReturnQty = item.ReturnQty;
                        yarnReturnChildEntity.ReturnQtyCarton = item.ReturnQtyCarton;
                        yarnReturnChildEntity.ReturnQtyCone = item.ReturnQtyCone;
                        yarnReturnChildEntity.EntityState = EntityState.Modified;
                    }
                }
                foreach (var item in entity.YarnQCReturnChilds.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                }
            }
            else
            {
                entity = model;
                entity.AddedBy = UserId;
                entity.QCReturnBy = UserId;
                entity.DateAdded = DateTime.Now;
            }

            await _service.SaveAsync(entity);

            return Ok();
        }
    }
}
