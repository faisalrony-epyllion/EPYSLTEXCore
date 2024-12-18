using AutoMapper;
using Azure.Core;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yarn-qc-remarks")]
    public class YarnQCRemarksController : ApiBaseController
    {
        private readonly IYarnQCRemarksService _service;
        //private readonly IMapper _mapper;
        public YarnQCRemarksController(IUserService userService, IYarnQCRemarksService service) : base(userService) //, IMapper mapper
        {
            _service = service;
            //_mapper = mapper;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnQCRemarksMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("new/{qcReceiveChildID}")]
        public async Task<IActionResult> GetNew2(int qcReceiveChildID)
        {
            return Ok(await _service.GetNew2Async(qcReceiveChildID));
        }
        //[HttpGet]
        //[Route("new/{qcReceiveMasterId}")]
        //public async Task<IActionResult> GetNew(int qcReceiveMasterId)
        //{
        //    return Ok(await _service.GetNewAsync(qcReceiveMasterId));
        //}

        [Route("{qcRemarksChildID}")]
        [HttpGet]
        public async Task<IActionResult> Get(int qcRemarksChildID)
        {
            YarnQCRemarksMaster record = await _service.Get2Async(qcRemarksChildID);
            return Ok(record);
        }

        /*
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            YarnQCRemarksMaster record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }
        */

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> SaveYarnQCRemarks(dynamic jsonString)
        {
            YarnQCRemarksMaster model = JsonConvert.DeserializeObject<YarnQCRemarksMaster>(Convert.ToString(jsonString));

            YarnQCRemarksMaster entity = new YarnQCRemarksMaster();

            if (model.IsRetest)
            {
                entity = await _service.GetAllAsync(model.QCRemarksMasterID);
                entity.EntityState = EntityState.Modified;
                entity.YarnQCRemarksChilds.SetUnchanged();

                entity.IsRetest = true;
                entity.RetestBy = AppUser.UserCode;
                entity.RetestDate = DateTime.Now;
            }
            else if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.QCRemarksMasterID);
                entity.QCRemarksDate = model.QCRemarksDate;
                entity.ReceiveID = model.ReceiveID;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                if (model.IsRetestForRequisition)
                {
                    entity.IsRetestForRequisition = true;
                    entity.RetestForRequisitionBy = AppUser.UserCode;
                    entity.RetestForRequisitionDate = DateTime.Now;
                }

                if (model.IsSendForApproval)
                {
                    entity.IsSendForApproval = true;
                    entity.SendForApprovalBy = AppUser.UserCode;
                    entity.SendForApprovalDate = DateTime.Now;

                    entity.IsApproved = false;
                    entity.ApprovedBy = 0;
                    entity.ApprovedDate = null;
                }

                entity.YarnQCRemarksChilds.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.YarnQCRemarksChildResults.SetUnchanged();
                    x.YarnQCRemarksChildFibers.SetUnchanged();
                });

                foreach (YarnQCRemarksChild child in model.YarnQCRemarksChilds)
                {
                    var childEntity = entity.YarnQCRemarksChilds.Find(x => x.QCRemarksChildID == child.QCRemarksChildID);
                    if (childEntity == null)
                    {
                        childEntity = CommonFunction.DeepClone(child);
                        childEntity.EntityState = EntityState.Added;

                        entity.YarnQCRemarksChilds.Add(childEntity);
                    }
                    else
                    {
                        childEntity.TechnicalNameID = child.TechnicalNameID;
                        childEntity.ReceiveQty = child.ReceiveQty;
                        childEntity.ReceiveQtyCarton = child.ReceiveQtyCarton;
                        childEntity.ReceiveQtyCone = child.ReceiveQtyCone;
                        childEntity.Remarks = child.Remarks;
                        childEntity.YarnStatusID = child.YarnStatusID;
                        childEntity.EntityState = EntityState.Modified;
                        if (child.id == "ReTest")
                        {
                            childEntity.ReTest = true;
                            childEntity.ReTestBy = AppUser.UserCode;
                            childEntity.ReTestDate = DateTime.Now;

                            childEntity.Approve = false;
                            childEntity.ApproveDate = null;
                            childEntity.ApproveBy = 0;

                            childEntity.Reject = false;
                            childEntity.RejectDate = null;
                            childEntity.RejectBy = 0;

                            childEntity.Diagnostic = false;
                            childEntity.DiagnosticDate = null;
                            childEntity.DiagnosticBy = 0;

                            childEntity.CommerciallyApprove = false;
                            childEntity.CommerciallyApproveDate = null;
                            childEntity.CommerciallyApproveBy = 0;
                        }
                        else if (child.id == "Approve")
                        {
                            childEntity.ReTest = false;
                            childEntity.ReTestBy = 0;
                            childEntity.ReTestDate = null;

                            childEntity.Approve = true;
                            childEntity.ApproveDate = DateTime.Now;
                            childEntity.ApproveBy = AppUser.UserCode;

                            childEntity.Reject = false;
                            childEntity.RejectDate = null;
                            childEntity.RejectBy = 0;

                            childEntity.Diagnostic = false;
                            childEntity.DiagnosticDate = null;
                            childEntity.DiagnosticBy = 0;

                            childEntity.CommerciallyApprove = false;
                            childEntity.CommerciallyApproveDate = null;
                            childEntity.CommerciallyApproveBy = 0;
                        }
                        else if (child.id == "Reject")
                        {
                            childEntity.ReTest = false;
                            childEntity.ReTestBy = 0;
                            childEntity.ReTestDate = null;

                            childEntity.Approve = false;
                            childEntity.ApproveDate = null;
                            childEntity.ApproveBy = 0;

                            childEntity.Reject = true;
                            childEntity.RejectDate = DateTime.Now;
                            childEntity.RejectBy = AppUser.UserCode;

                            childEntity.Diagnostic = false;
                            childEntity.DiagnosticDate = null;
                            childEntity.DiagnosticBy = 0;

                            childEntity.CommerciallyApprove = false;
                            childEntity.CommerciallyApproveDate = null;
                            childEntity.CommerciallyApproveBy = 0;
                        }
                        else if (child.id == "Diagnostic")
                        {
                            childEntity.ReTest = false;
                            childEntity.ReTestBy = 0;
                            childEntity.ReTestDate = null;

                            childEntity.Approve = false;
                            childEntity.ApproveDate = null;
                            childEntity.ApproveBy = 0;

                            childEntity.Reject = false;
                            childEntity.RejectDate = null;
                            childEntity.RejectBy = 0;

                            childEntity.Diagnostic = true;
                            childEntity.DiagnosticDate = DateTime.Now;
                            childEntity.DiagnosticBy = AppUser.UserCode;

                            childEntity.CommerciallyApprove = false;
                            childEntity.CommerciallyApproveDate = null;
                            childEntity.CommerciallyApproveBy = 0;
                        }
                        else if (child.id == "CommerciallyApprove")
                        {
                            childEntity.ReTest = false;
                            childEntity.ReTestBy = 0;
                            childEntity.ReTestDate = null;

                            childEntity.Approve = false;
                            childEntity.ApproveDate = null;
                            childEntity.ApproveBy = 0;

                            childEntity.Reject = false;
                            childEntity.RejectDate = null;
                            childEntity.RejectBy = 0;

                            childEntity.Diagnostic = false;
                            childEntity.DiagnosticDate = null;
                            childEntity.DiagnosticBy = 0;

                            childEntity.CommerciallyApprove = true;
                            childEntity.CommerciallyApproveDate = DateTime.Now;
                            childEntity.CommerciallyApproveBy = AppUser.UserCode;
                        }

                        int childEntityIndex = entity.YarnQCRemarksChilds.FindIndex(x => x.QCRemarksChildID == child.QCRemarksChildID);

                        #region YarnQCRemarksChildResult
                        child.YarnQCRemarksChildResults.ToList().ForEach(x =>
                        {
                            YarnQCRemarksChildResult childResultEntity = childEntity.YarnQCRemarksChildResults.Find(c => c.QCRemarksChildResultID == x.QCRemarksChildResultID);
                            if (childResultEntity == null || x.QCRemarksChildResultID == 0)
                            {
                                childResultEntity = CommonFunction.DeepClone(x);
                                childResultEntity.EntityState = EntityState.Added;
                                childEntity.YarnQCRemarksChildResults.Add(childResultEntity);
                            }
                            else
                            {
                                int indexF = childEntity.YarnQCRemarksChildResults.FindIndex(c => c.QCRemarksChildResultID == x.QCRemarksChildResultID);

                                childResultEntity.ACountNe = x.ACountNe;
                                childResultEntity.Twist = x.Twist;
                                childResultEntity.CSP = x.CSP;
                                childResultEntity.FabricColorID = x.FabricColorID;
                                childResultEntity.DyeingProcessID = x.DyeingProcessID;
                                childResultEntity.ThickThin = x.ThickThin;
                                childResultEntity.BarreMark = x.BarreMark;
                                childResultEntity.Naps = x.Naps;
                                childResultEntity.Hairiness = x.Hairiness;
                                childResultEntity.WhiteSpecks = x.WhiteSpecks;
                                childResultEntity.Polypropylyne = x.Polypropylyne;
                                childResultEntity.Contamination = x.Contamination;
                                childResultEntity.TestMethodRefID = x.TestMethodRefID;
                                childResultEntity.PillingGrade = x.PillingGrade;
                                childResultEntity.TechnicalNameID = x.TechnicalNameID;
                                childResultEntity.Remarks = x.Remarks;
                                childResultEntity.EntityState = EntityState.Modified;
                                childEntity.YarnQCRemarksChildResults[indexF] = CommonFunction.DeepClone(childResultEntity);
                            }
                        });
                        childEntity.YarnQCRemarksChildResults.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                        #endregion

                        #region YarnQCRemarksChildFiber
                        child.YarnQCRemarksChildFibers.ToList().ForEach(x =>
                        {
                            YarnQCRemarksChildFiber childFiberEntity = childEntity.YarnQCRemarksChildFibers.Find(c => c.QCRemarksChildFiberID == x.QCRemarksChildFiberID);
                            if (childFiberEntity == null)
                            {
                                childFiberEntity = CommonFunction.DeepClone(x);
                                childFiberEntity.EntityState = EntityState.Added;

                                childEntity.YarnQCRemarksChildFibers.Add(childFiberEntity);
                            }
                            else
                            {
                                int indexF = childEntity.YarnQCRemarksChildFibers.FindIndex(c => c.QCRemarksChildFiberID == x.QCRemarksChildFiberID);

                                childFiberEntity.ComponentID = x.ComponentID;
                                childFiberEntity.PercentageValue = x.PercentageValue;
                                childFiberEntity.EntityState = EntityState.Modified;
                                childEntity.YarnQCRemarksChildFibers[indexF] = CommonFunction.DeepClone(childFiberEntity);
                            }
                        });
                        childEntity.YarnQCRemarksChildFibers.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                        #endregion


                        entity.YarnQCRemarksChilds[childEntityIndex] = childEntity;

                    }
                }
                entity.YarnQCRemarksChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x =>
                {
                    x.EntityState = EntityState.Deleted;
                    x.YarnQCRemarksChildResults.SetDeleted();
                    x.YarnQCRemarksChildFibers.SetDeleted();
                });
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                entity.QCRemarksBy = AppUser.UserCode;
                foreach (YarnQCRemarksChild child in entity.YarnQCRemarksChilds)
                {
                    switch (child.id)
                    {
                        case "ReTest":
                            child.ReTest = true;
                            child.ReTestBy = AppUser.UserCode;
                            child.ReTestDate = DateTime.Now;

                            child.Approve = false;
                            child.ApproveDate = null;
                            child.ApproveBy = 0;

                            child.Reject = false;
                            child.RejectDate = null;
                            child.RejectBy = 0;

                            child.Diagnostic = false;
                            child.DiagnosticDate = null;
                            child.DiagnosticBy = 0;

                            child.CommerciallyApprove = false;
                            child.CommerciallyApproveDate = null;
                            child.CommerciallyApproveBy = 0;
                            break;
                        case "Approve":
                            child.ReTest = false;
                            child.ReTestBy = 0;
                            child.ReTestDate = null;

                            child.Approve = true;
                            child.ApproveDate = DateTime.Now;
                            child.ApproveBy = AppUser.UserCode;

                            child.Reject = false;
                            child.RejectDate = null;
                            child.RejectBy = 0;

                            child.Diagnostic = false;
                            child.DiagnosticDate = null;
                            child.DiagnosticBy = 0;

                            child.CommerciallyApprove = false;
                            child.CommerciallyApproveDate = null;
                            child.CommerciallyApproveBy = 0;
                            break;
                        case "Reject":
                            child.ReTest = false;
                            child.ReTestBy = 0;
                            child.ReTestDate = null;

                            child.Approve = false;
                            child.ApproveDate = null;
                            child.ApproveBy = 0;

                            child.Reject = true;
                            child.RejectDate = DateTime.Now;
                            child.RejectBy = AppUser.UserCode;

                            child.Diagnostic = false;
                            child.DiagnosticDate = null;
                            child.DiagnosticBy = 0;

                            child.CommerciallyApprove = false;
                            child.CommerciallyApproveDate = null;
                            child.CommerciallyApproveBy = 0;
                            break;
                        case "Diagnostic":
                            child.ReTest = false;
                            child.ReTestBy = 0;
                            child.ReTestDate = null;

                            child.Approve = false;
                            child.ApproveDate = null;
                            child.ApproveBy = 0;

                            child.Reject = false;
                            child.RejectDate = null;
                            child.RejectBy = 0;

                            child.Diagnostic = true;
                            child.DiagnosticDate = DateTime.Now;
                            child.DiagnosticBy = AppUser.UserCode;

                            child.CommerciallyApprove = false;
                            child.CommerciallyApproveDate = null;
                            child.CommerciallyApproveBy = 0;
                            break;
                        case "CommerciallyApprove":
                            child.ReTest = false;
                            child.ReTestBy = 0;
                            child.ReTestDate = null;

                            child.Approve = false;
                            child.ApproveDate = null;
                            child.ApproveBy = 0;

                            child.Reject = false;
                            child.RejectDate = null;
                            child.RejectBy = 0;

                            child.Diagnostic = false;
                            child.DiagnosticDate = null;
                            child.DiagnosticBy = 0;

                            child.CommerciallyApprove = true;
                            child.CommerciallyApproveDate = DateTime.Now;
                            child.CommerciallyApproveBy = AppUser.UserCode;
                            break;
                        default:
                            // code block
                            break;
                    }
                }
            }
            await _service.SaveAsync(entity);

            return Ok(entity);
        }

        [Route("approve")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> ApproveYarnQCRemarks(dynamic jsonString)
        {
           YarnQCRemarksMaster model = JsonConvert.DeserializeObject<YarnQCRemarksMaster>(Convert.ToString(jsonString));

            YarnQCRemarksMaster entity = await _service.GetAllAsync(model.QCRemarksMasterID);
            entity.IsApproved = true;
            entity.ApprovedBy = AppUser.UserCode;
            entity.ApprovedDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            entity.YarnQCRemarksChilds.SetUnchanged();

            entity.YarnQCRemarksChilds.ForEach(c =>
            {
                c.EntityState = EntityState.Modified;
                var child = model.YarnQCRemarksChilds.Find(x => x.QCRemarksChildID == c.QCRemarksChildID);
                if (child.IsNotNull() && child.ReTest)
                {
                    c.Approve = false;
                    c.Reject = false;
                    c.Diagnostic = false;
                    c.CommerciallyApprove = false;

                    c.ReTest = true;
                    c.ReTestBy = AppUser.UserCode;
                    c.ReTestDate = DateTime.Now;
                }
            });
            await _service.ApproveAsync(entity);

            return Ok(entity.QCRemarksMasterID);
        }
    }
}
