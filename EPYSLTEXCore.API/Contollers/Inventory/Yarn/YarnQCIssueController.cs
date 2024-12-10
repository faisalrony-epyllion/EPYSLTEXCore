using Azure.Core;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EnumRackBinOperationType = EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yarn-qc-issue")]
    public class YarnQCIssueController : ApiBaseController
    {
        private readonly IYarnQCIssueService _service;
        private readonly IYarnRackBinAllocationService _serviceRackBin;
        public YarnQCIssueController(IUserService userService, IYarnQCIssueService service,
            IYarnRackBinAllocationService serviceRackBin) : base(userService)
        {
            _service = service;
            _serviceRackBin = serviceRackBin;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnQCIssueMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{qcReqMasterId}")]
        public async Task<IActionResult> GetNew(int qcReqMasterId)
        {
            return Ok(await _service.GetNewAsync(qcReqMasterId));
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
        public async Task<IActionResult> Save(YarnQCIssueMaster model)
        {
            YarnQCIssueMaster entity;

            List<YarnReceiveChildRackBin> rackBins = new List<YarnReceiveChildRackBin>();

            #region Rack Bin List
            List<int> rackBinIds = new List<int>();

            model.YarnQCIssueChilds.ForEach(c =>
            {
                rackBinIds.AddRange(c.ChildRackBins.Select(x => x.ChildRackBinID));
            });
            string sChildRackBinIDs = string.Join(",", rackBinIds.Distinct());

            if (sChildRackBinIDs.IsNotNullOrEmpty())
            {
                rackBins = await _serviceRackBin.GetRackBinById(sChildRackBinIDs);
                rackBins.SetModified();
            }
            #endregion


            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.QCIssueMasterID);

                entity.QCIssueDate = model.QCIssueDate;
                entity.ReceiveID = model.ReceiveID;
                entity.SupplierId = model.SupplierId;
                entity.LocationId = model.LocationId;
                entity.RCompanyId = model.RCompanyId;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                if (model.Approve)
                {
                    entity.Approve = true;
                    entity.ApproveDate = DateTime.Now;
                    entity.ApproveBy = AppUser.UserCode;
                    entity.Reject = false;
                    entity.RejectDate = null;
                    entity.RejectBy = 0;
                }
                if (model.Reject)
                {
                    entity.Approve = false;
                    entity.ApproveDate = null;
                    entity.ApproveBy = 0;
                    entity.Reject = true;
                    entity.RejectDate = DateTime.Now;
                    entity.RejectBy = AppUser.UserCode;
                }
                entity.YarnQCIssueChilds.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.ChildRackBins.SetUnchanged();
                });

                foreach (YarnQCIssueChild item in model.YarnQCIssueChilds)
                {
                    var existingYarnReceiveChild = entity.YarnQCIssueChilds.FirstOrDefault(x => x.QCIssueChildID == item.QCIssueChildID);
                    int childIndexF = entity.YarnQCIssueChilds.FindIndex(x => x.QCIssueChildID == item.QCIssueChildID);

                    if (existingYarnReceiveChild == null)
                    {
                        item.QCIssueMasterID = entity.QCIssueMasterID;
                        item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        item.ReqQtyCarton = item.ReqBagPcs;
                        existingYarnReceiveChild = CommonFunction.DeepClone(item);
                        entity.YarnQCIssueChilds.Add(item);
                    }
                    else
                    {
                        existingYarnReceiveChild.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        existingYarnReceiveChild.ReqQtyCarton = item.ReqBagPcs;
                        existingYarnReceiveChild.IssueQty = item.IssueQty;
                        existingYarnReceiveChild.ShadeCode = item.ShadeCode;
                        existingYarnReceiveChild.IssueQtyCone = item.IssueQtyCone;
                        existingYarnReceiveChild.IssueQtyCarton = item.IssueQtyCarton;
                        existingYarnReceiveChild.EntityState = EntityState.Modified;
                    }

                    foreach (var crbObj in item.ChildRackBins)
                    {
                        #region rack bin update
                        var childRackBins = existingYarnReceiveChild.ChildRackBins.Where(x => x.ChildRackBinID == crbObj.ChildRackBinID).ToList();
                        int cone = 0;
                        int cartoon = 0;
                        decimal qtyKg = 0;

                        existingYarnReceiveChild.ChildRackBins.ForEach(c =>
                        {
                            cone += c.IssueQtyCone;
                            cartoon += c.IssueCartoon;
                            qtyKg += c.IssueQtyKg;
                        });

                        cone = crbObj.IssueQtyCone - cone;
                        cartoon = crbObj.IssueCartoon - cartoon;
                        qtyKg = crbObj.IssueQtyKg - qtyKg;
                        #endregion

                        var childMapping = existingYarnReceiveChild.ChildRackBins.FirstOrDefault(x => x.YQCICRBId == crbObj.YQCICRBId);
                        int indexF = existingYarnReceiveChild.ChildRackBins.FindIndex(x => x.YQCICRBId == crbObj.YQCICRBId);

                        if (childMapping == null)
                        {
                            childMapping = CommonFunction.DeepClone(crbObj);
                            childMapping.QCIssueChildID = item.QCIssueChildID;
                            childMapping.EntityState = EntityState.Added;
                            existingYarnReceiveChild.ChildRackBins.Add(childMapping);
                        }
                        else if (indexF > -1)
                        {
                            existingYarnReceiveChild.ChildRackBins[indexF].IssueCartoon = crbObj.IssueCartoon;
                            existingYarnReceiveChild.ChildRackBins[indexF].IssueQtyCone = crbObj.IssueQtyCone;
                            existingYarnReceiveChild.ChildRackBins[indexF].IssueQtyKg = crbObj.IssueQtyKg;
                            existingYarnReceiveChild.ChildRackBins[indexF].EntityState = EntityState.Modified;
                        }

                        #region rack bin update
                        rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, crbObj.ChildRackBinID, EnumRackBinOperationType.Deduction, cone, cartoon, qtyKg);
                        #endregion
                    }
                    if (childIndexF > -1)
                    {
                        entity.YarnQCIssueChilds[childIndexF] = existingYarnReceiveChild;
                    }
                }
                foreach (var item in entity.YarnQCIssueChilds)
                {
                    if (item.EntityState == EntityState.Unchanged)
                    {
                        item.EntityState = EntityState.Deleted;
                    }
                    item.ChildRackBins.ForEach(rb =>
                    {
                        if (rb.EntityState == EntityState.Unchanged || item.EntityState == EntityState.Deleted)
                        {
                            rb.EntityState = EntityState.Deleted;

                            #region rack bin update
                            rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EnumRackBinOperationType.Addition, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                            #endregion
                        }
                    });
                }
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                entity.QCIssueBy = AppUser.UserCode;
                foreach (YarnQCIssueChild item in entity.YarnQCIssueChilds)
                {
                    item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                    item.ReqQtyCarton = item.ReqBagPcs;
                }
                if (model.Approve)
                {
                    entity.Approve = true;
                    entity.ApproveDate = DateTime.Now;
                    entity.ApproveBy = AppUser.UserCode;
                    entity.Reject = false;
                    entity.RejectDate = null;
                    entity.RejectBy = 0;

                    entity.YarnQCIssueChilds.ForEach(c =>
                    {
                        c.ChildRackBins.ForEach(rb =>
                        {
                            rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EnumRackBinOperationType.Deduction, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                        });
                    });
                }
                else if (model.Reject)
                {
                    entity.Approve = false;
                    entity.ApproveDate = null;
                    entity.ApproveBy = 0;
                    entity.Reject = true;
                    entity.RejectDate = DateTime.Now;
                    entity.RejectBy = AppUser.UserCode;
                }
                else
                {
                    entity.YarnQCIssueChilds.ForEach(c =>
                    {
                        c.ChildRackBins.ForEach(rb =>
                        {
                            rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EnumRackBinOperationType.Deduction, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                        });
                    });
                }
            }
            await _service.SaveAsync(entity, rackBins);

            return Ok();
        }
    }
}
