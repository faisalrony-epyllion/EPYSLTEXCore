using Azure.Core;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using Newtonsoft.Json;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yarn-qc-requisition")]
    public class YarnQCRequisitionController : ApiBaseController
    {
        private readonly IYarnReceiveService _serviceYR;
        private readonly IYarnQCReqService _service;
        private readonly IYarnQCRemarksService _serviceQCRemarks;
        public YarnQCRequisitionController(IUserService userService, IYarnQCReqService service, IYarnReceiveService serviceYR, IYarnQCRemarksService serviceQCRemarks) : base(userService)
        {
            _service = service;
            _serviceYR = serviceYR;
            _serviceQCRemarks = serviceQCRemarks;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnQCReqMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new")]
        public async Task<IActionResult> GetNew()
        {
            return Ok(await _service.GetNewAsync());
        }

        [HttpGet]
        [Route("new/receiveData")]
        public async Task<IActionResult> GetReceiveData(int receiveId)
        {
            return Ok(await _service.GetReceiveData(receiveId));
        }

        [Route("{id}/{status}/{itemMasterID}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, Status status, int itemMasterID)
        {
            var record = await _service.GetAsync(id, status, itemMasterID);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }
        [Route("retest/{id}/{qcRemarksChildID}/")]
        [HttpGet]
        public async Task<IActionResult> GetByRemarksChildId(int id, int qcRemarksChildID)
        {
            var record = await _service.GetByRemarksChildId(id, qcRemarksChildID);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }
        [Route("receive-child/{receiveChildIds}")]
        [HttpGet]
        public async Task<IActionResult> GetByReceiveChildIds(string receiveChildIds)
        {
            var record = await _service.GetByReceiveChildIds(receiveChildIds);
            return Ok(record);
        }
        [Route("retest/{qcReqMasterID}")]
        [HttpGet]
        public async Task<IActionResult> GetRetest(int qcReqMasterID)
        {
            var record = await _service.GetRetest(qcReqMasterID);
            return Ok(record);
        }
        [Route("save")]
        [HttpPost]
        [ValidateModel]
        //public async Task<IActionResult> Save(YarnQCReqMaster model)
        public async Task<IActionResult> Save(dynamic model1)
        {
            YarnQCReqMaster model = JsonConvert.DeserializeObject<YarnQCReqMaster>(Convert.ToString(model1));
            YarnQCReqMaster entity = new YarnQCReqMaster();
            YarnQCRemarksMaster entityQCRemark = new YarnQCRemarksMaster();

            if (model.IsModified && !model.IsRetest && !model.IsRetestDiagnostic && !model.IsRetestForRequisition && model.RetestParentQCRemarksMasterID == 0)
            {
                entity = await _service.GetAllAsync(model.QCReqMasterID);

                entity.QCReqDate = model.QCReqDate;
                entity.QCForId = model.QCForId;
                entity.ReceiveID = model.ReceiveID;
                entity.SupplierID = model.SupplierID;
                entity.LocationID = model.LocationID;
                entity.RCompanyID = model.RCompanyID;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;
                entity.NeedUSTER = model.NeedUSTER;
                entity.NeedYarnTRF = model.NeedYarnTRF;
                entity.NeedFabricTRF = model.NeedFabricTRF;

                if (model.IsApprove)
                {
                    entity.IsApprove = true;
                    entity.ApproveDate = DateTime.Now;
                    entity.ApproveBy = AppUser.UserCode;

                    entity.IsReject = false;
                    entity.RejectDate = null;
                    entity.RejectBy = 0;
                    entity.RejectReason = "";
                }
                else if (model.IsReject)
                {
                    entity.IsReject = true;
                    entity.RejectDate = DateTime.Now;
                    entity.RejectBy = AppUser.UserCode;
                    entity.RejectReason = model.RejectReason;

                    entity.IsApprove = false;
                    entity.ApproveDate = null;
                    entity.ApproveBy = 0;
                }
                else if (model.IsRevise)
                {
                    entity.RevisioNo = entity.RevisioNo + 1;

                    entity.IsSendForApproval = true;
                    entity.IsSendBy = 0;
                    entity.IsSendDate = DateTime.Now;

                    entity.IsApprove = false;
                    entity.ApproveBy = 0;
                    entity.ApproveDate = null;

                    entity.IsAcknowledge = false;
                    entity.AcknowledgeBy = 0;
                    entity.AcknowledgeDate = null;

                    entity.IsReject = false;
                    entity.RejectBy = 0;
                    entity.RejectDate = null;
                }

                entity.YarnQCReqChilds.SetUnchanged();

                foreach (YarnQCReqChild item in model.YarnQCReqChilds)
                {
                    var existingYarnReceiveChilds = entity.YarnQCReqChilds.FirstOrDefault(x => x.QCReqChildID == item.QCReqChildID);

                    if (existingYarnReceiveChilds == null)
                    {
                        item.QCReqMasterID = entity.QCReqMasterID;
                        item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        entity.YarnQCReqChilds.Add(item);
                    }
                    else
                    {
                        existingYarnReceiveChilds.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        existingYarnReceiveChilds.ReqQty = item.ReqQty;
                        existingYarnReceiveChilds.ReqCone = item.ReqCone;
                        existingYarnReceiveChilds.MachineTypeId = item.MachineTypeId;
                        existingYarnReceiveChilds.TechnicalNameId = item.TechnicalNameId;
                        existingYarnReceiveChilds.BuyerID = item.BuyerID;
                        existingYarnReceiveChilds.ReqBagPcs = item.ReqBagPcs;
                        existingYarnReceiveChilds.QCReqRemarks = item.QCReqRemarks;
                        existingYarnReceiveChilds.EntityState = EntityState.Modified;
                    }
                }
                foreach (var item in entity.YarnQCReqChilds.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                }
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                entity.QCReqBy = AppUser.UserCode;
                entity.YarnQCReqChilds.ForEach(item =>
                {
                    item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                });
                if (model.IsApprove)
                {
                    entity.IsApprove = true;
                    entity.ApproveDate = DateTime.Now;
                    entity.ApproveBy = AppUser.UserCode;
                }

                #region Update Retest of QC Remarks
                if (model.RetestParentQCRemarksMasterID > 0)
                {
                    entityQCRemark = await _serviceQCRemarks.GetAsync(model.RetestParentQCRemarksMasterID);
                    entityQCRemark.EntityState = EntityState.Modified;

                    if (model.IsRetestForRequisition)
                    {
                        entityQCRemark.IsRetestForRequisition = false;
                    }
                    if (model.IsRetest)
                    {
                        entityQCRemark.IsRetest = false;
                    }
                }
                #endregion
            }

            if (model.IsFromNoTest && !model.IsRetest && !model.IsRetestForRequisition)
            {
                string childIds = string.Join(",", entity.YarnQCReqChilds.Select(x => x.ReceiveChildID).Distinct());
                if (childIds.IsNotNullOrEmpty())
                {
                    entity.YarnReceiveChilds = await _serviceYR.GetReceiveChilds(childIds);
                    entity.YarnReceiveChilds.ForEach(x =>
                    {
                        x.IsNoTest = false;
                        x.NoTestRemarks = "";
                        x.NoTestBy = AppUser.UserCode;
                        x.NoTestDate = DateTime.Now;
                        x.EntityState = EntityState.Modified;
                    });
                }
            }
            if (model.IsRetest)
            {
                entity.IsRetest = true;
                entity.RetestQCReqMasterID = entity.QCReqMasterID;
            }
            if (model.IsRetestForRequisition)
            {
                entity.IsRetestForRequisition = true;
                entity.RetestForRequisitionQCReqMasterID = entity.QCReqMasterID;
            }

            if (model.IsSendForApproval)
            {
                entity.IsSendForApproval = true;
                entity.IsSendBy = AppUser.UserCode;
                entity.IsSendDate = DateTime.Now;
            }
            else if (model.IsApprove)
            {
                entity.IsApprove = true;
                entity.ApproveBy = AppUser.UserCode;
                entity.ApproveDate = DateTime.Now;
            }

            await _service.SaveAsync(entity, entityQCRemark);

            return Ok(entity);
        }
    }
}
