using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn.Knitting;
using EPYSLTEXCore.Application.Services.Inventory;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Data.Entity;
using System.Reflection;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn.Knitting
{
    [Route("api/ksc-issue")]
    public class KnittingSubContractIssueController : ApiBaseController
    {
        private readonly IKnittingSubContractIssueService _service;
        private readonly IYDReqIssueService _yDservice;
        private readonly IItemMasterService<KnittingSubContractIssueChild> _itemMasterRepository;
        private readonly IYarnRackBinAllocationService _serviceRackBin;
        public KnittingSubContractIssueController(IUserService userService, IKnittingSubContractIssueService service, IYDReqIssueService yDservice, IItemMasterService<KnittingSubContractIssueChild> itemMasterRepository, IYarnRackBinAllocationService serviceRackBin): base(userService)
        {
            _service = service;
            _yDservice = yDservice;
            _itemMasterRepository = itemMasterRepository;
            _serviceRackBin = serviceRackBin;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<KnittingSubContractIssueMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{yBookingId}/{ReqType}/{programName}")]
        public async Task<IActionResult> GetNew(int yBookingId, string reqType, string programName)
        {
            return Ok(await _service.GetNewAsync(yBookingId, reqType, programName));
        }

        [Route("{id}/{reqType}/{programName}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, string reqType, string programName)
        {
            var record = await _service.GetAsync(id, reqType, programName);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }
        //private async Task<string> GetMaxSCYDIssueNoAsync()
        //{
        //    var id = await _service.GetMaxIdAsync(TableNames.SC_YD_Yarn_Issue_No, RepeatAfterEnum.EveryYear);
        //    var datePart = DateTime.Now.ToString("yyMMdd");
        //    return $@"IS-{datePart}{id:00000}";
        //}
        //private async Task<string> GetMaxSCYDChallanNoAsync()
        //{
        //    var id = await _service.GetMaxIdAsync(TableNames.SC_YD_Yarn_Challan_No, RepeatAfterEnum.EveryYear);
        //    var datePart = DateTime.Now.ToString("yyMMdd");
        //    return $@"CH-{datePart}{id:00000}";
        //}
        //private async Task<string> GetMaxSCYDGPNoAsync()
        //{
        //    var id = await _service.GetMaxIdAsync(TableNames.GP_NO, RepeatAfterEnum.EveryYear);
        //    var datePart = DateTime.Now.ToString("yyMMdd");
        //    return $@"GP-{datePart}{id:00000}";
        //}
        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(KnittingSubContractIssueMaster model)
        {
            List<YarnReceiveChildRackBin> rackBins = new List<YarnReceiveChildRackBin>();

            #region Rack Bin List
            List<int> rackBinIds = new List<int>();

            model.Childs.ForEach(c =>
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

            if (model.ReqType == "YD")
            {
                List<KnittingSubContractIssueChild> childRecords = model.Childs;
                //_itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

                YDReqIssueMaster entity;
                if (model.KSCIssueMasterID > 0)
                {
                    entity = await _yDservice.GetAllAsync(model.KSCIssueMasterID);
                    entity.ProgramName = model.ProgramName;

                    entity.YDReqIssueDate = DateTime.Now;
                    entity.ReqType = model.ReqType;
                    entity.ProgramName = model.ProgramName;
                    entity.TransportTypeID = model.TransportTypeID;
                    entity.TransportAgencyID = model.TransportAgencyID;
                    entity.VehicleNo = model.VehicleNo;
                    entity.DriverName = model.DriverName;
                    entity.ContactNo = model.ContactNo;
                    entity.IsCompleted = model.IsCompleted;
                    entity.IsSendForApprove = model.IsSendForApprove;
                    if (model.IsSendForApprove)
                    {
                        entity.SendForApproveBy = model.SendForApproveBy;
                        entity.SendForApproveDate = DateTime.Now;
                    }
                    entity.Remarks = model.Remarks;
                    if (model.IsApprove == true)
                    {
                        entity.IsApprove = true;
                        entity.ApproveBy = AppUser.UserCode;
                        entity.ApproveDate = DateTime.Now;

                        entity.IsReject = false;
                        entity.RejectBy = 0;
                        entity.RejectDate = null;
                        entity.RejectReason = "";
                    }
                    else if (model.IsReject == true)
                    {
                        entity.IsApprove = false;
                        entity.ApproveBy = 0;
                        entity.ApproveDate = null;

                        entity.IsReject = true;
                        entity.RejectBy = AppUser.UserCode;
                        entity.RejectDate = DateTime.Now;
                        entity.RejectReason = model.RejectReason;
                    }
                    entity.UpdatedBy = AppUser.UserCode;
                    entity.DateUpdated = DateTime.Now;
                    entity.EntityState = EntityState.Modified;

                    //entity.Childs.SetUnchanged();
                    entity.Childs.ForEach(x =>
                    {
                        x.EntityState = EntityState.Unchanged;
                        x.ChildRackBins.SetUnchanged();
                    });

                    foreach (KnittingSubContractIssueChild item in model.Childs)
                    {
                        YDReqIssueChild childEntity = entity.Childs.FirstOrDefault(x => x.YDReqIssueChildID == item.KSCIssueChildID);
                        int childIndexF = entity.Childs.FindIndex(x => x.YDReqIssueChildID == item.KSCIssueChildID);

                        if (childEntity == null)
                        {
                            childEntity = new YDReqIssueChild();
                            //childEntity.YDReqIssueMasterID = entity.YDReqIssueMasterID;
                            //childEntity.YarnProgramID = item.YarnProgramID;
                            childEntity.YDReqChildID = item.KSCReqChildID;
                            childEntity.ItemMasterID = item.ItemMasterID;
                            childEntity.UnitID = item.UnitID;
                            childEntity.ReqQty = item.ReqQty;
                            childEntity.IssueQty = item.IssueQty;
                            childEntity.IssueQtyCone = item.IssueQtyCone;
                            childEntity.IssueQtyCarton = item.IssueQtyCarton;
                            childEntity.Remarks = item.Remarks;
                            childEntity.YarnCategory = item.YarnCategory;
                            childEntity.NoOfThread = item.NoOfThread;
                            childEntity.LotNo = item.YarnLotNo;
                            childEntity.PhysicalCount = item.PhysicalCount;
                            //childEntity.YarnBrandID = item.YarnBrandID;
                            //childEntity.Rate = item.Rate;
                            //childEntity.ShadeCode = item.ShadeCode;
                            entity.Childs.Add(childEntity);
                        }
                        else
                        {
                            childEntity.YDReqChildID = item.KSCReqChildID;
                            childEntity.YarnProgramID = item.YarnProgramID;
                            childEntity.ItemMasterID = item.ItemMasterID;
                            childEntity.UnitID = item.UnitID;
                            childEntity.ReqQty = item.ReqQty;
                            childEntity.IssueQty = item.IssueQty;
                            childEntity.IssueQtyCone = item.IssueQtyCone;
                            childEntity.IssueQtyCarton = item.IssueQtyCarton;
                            childEntity.Remarks = item.Remarks;
                            childEntity.YarnCategory = item.YarnCategory;
                            childEntity.NoOfThread = item.NoOfThread;
                            childEntity.LotNo = item.YarnLotNo;
                            childEntity.PhysicalCount = item.PhysicalCount;
                            //childEntity.YarnBrandID = item.YarnBrandID;
                            //childEntity.Rate = item.Rate;
                            //childEntity.ShadeCode = item.ShadeCode;
                            childEntity.EntityState = EntityState.Modified;
                        }

                        foreach (var crbObj in item.ChildRackBins)
                        {
                            YDReqIssueChildRackBinMapping crbObjKY = new YDReqIssueChildRackBinMapping();
                            crbObjKY.YDRICRBId = crbObj.KSCICRBId;
                            crbObjKY.YDReqIssueChildID = crbObj.KSCIssueChildID;
                            crbObjKY.ChildRackBinID = crbObj.ChildRackBinID;
                            crbObjKY.IssueCartoon = crbObj.IssueCartoon;
                            crbObjKY.IssueQtyCone = crbObj.IssueQtyCone;
                            crbObjKY.IssueQtyKg = crbObj.IssueQtyKg;
                            crbObjKY.YarnStockSetId = crbObj.YarnStockSetId;

                            #region rack bin update
                            var childRackBins = childEntity.ChildRackBins.Where(x => x.ChildRackBinID == crbObjKY.ChildRackBinID).ToList();
                            int cone = 0;
                            int cartoon = 0;
                            decimal qtyKg = 0;

                            childEntity.ChildRackBins.ForEach(c =>
                            {
                                cone += c.IssueQtyCone;
                                cartoon += c.IssueCartoon;
                                qtyKg += c.IssueQtyKg;
                            });

                            cone = crbObjKY.IssueQtyCone - cone;
                            cartoon = crbObjKY.IssueCartoon - cartoon;
                            qtyKg = crbObjKY.IssueQtyKg - qtyKg;
                            #endregion

                            var childMapping = childEntity.ChildRackBins.FirstOrDefault(x => x.YDRICRBId == crbObjKY.YDRICRBId);
                            int indexF = childEntity.ChildRackBins.FindIndex(x => x.YDRICRBId == crbObjKY.YDRICRBId);

                            if (childMapping == null)
                            {
                                childMapping = CommonFunction.DeepClone(crbObjKY);
                                childMapping.YDReqIssueChildID = item.KSCIssueChildID;
                                childMapping.EntityState = EntityState.Added;
                                childEntity.ChildRackBins.Add(childMapping);
                            }
                            else if (indexF > -1)
                            {
                                childEntity.ChildRackBins[indexF].IssueCartoon = crbObjKY.IssueCartoon;
                                childEntity.ChildRackBins[indexF].IssueQtyCone = crbObjKY.IssueQtyCone;
                                childEntity.ChildRackBins[indexF].IssueQtyKg = crbObjKY.IssueQtyKg;
                                childEntity.ChildRackBins[indexF].EntityState = EntityState.Modified;
                            }

                            #region rack bin update
                            rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, crbObjKY.ChildRackBinID, EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType.Deduction, cone, cartoon, qtyKg);
                            #endregion
                        }
                        if (childIndexF > -1)
                        {
                            entity.Childs[childIndexF] = childEntity;
                        }
                    }
                    //foreach (YDReqIssueChild item in entity.Childs.Where(x => x.EntityState == EntityState.Unchanged))
                    //{
                    //    item.EntityState = EntityState.Deleted;
                    //}
                    foreach (var item in entity.Childs)
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
                                rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType.Addition, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                                #endregion
                            }
                        });
                    }
                }
                else
                {
                    entity = new YDReqIssueMaster();
                    //entity.YDReqIssueNo = await GetMaxSCYDIssueNoAsync();
                    entity.YDReqIssueDate = DateTime.Now;
                    entity.YDReqIssueBy = AppUser.UserCode;
                    entity.YDReqMasterID = model.KSCReqMasterID;
                    //entity.ChallanNo = await GetMaxSCYDChallanNoAsync();
                    entity.ChallanDate = DateTime.Now;
                    entity.ReqType = model.ReqType;
                    entity.ProgramName = model.ProgramName;
                    entity.GPDate = DateTime.Now;
                    entity.TransportTypeID = model.TransportTypeID;
                    entity.TransportAgencyID = model.TransportAgencyID;
                    entity.VehicleNo = model.VehicleNo;
                    entity.DriverName = model.DriverName;
                    entity.ContactNo = model.ContactNo;
                    entity.IsCompleted = model.IsCompleted;
                    entity.IsSendForApprove = model.IsSendForApprove;
                    if (model.IsSendForApprove)
                    {
                        entity.SendForApproveBy = model.SendForApproveBy;
                        entity.SendForApproveDate = DateTime.Now;
                    }
                    entity.Remarks = model.Remarks;
                    entity.AddedBy = AppUser.UserCode;
                    entity.DateAdded = DateTime.Now;
                    entity.EntityState = EntityState.Added;
                    foreach (KnittingSubContractIssueChild item in model.Childs)
                    {
                        YDReqIssueChild childEntity = new YDReqIssueChild();
                        //childEntity.YDReqIssueMasterID = entity.YDReqIssueMasterID;
                        childEntity.YDReqChildID = item.KSCReqChildID;
                        childEntity.YarnProgramID = item.YarnProgramID;
                        childEntity.ItemMasterID = item.ItemMasterID;
                        childEntity.UnitID = item.UnitID;
                        childEntity.ReqQty = item.ReqQty;
                        childEntity.IssueQty = item.IssueQty;
                        childEntity.IssueQtyCone = item.IssueQtyCone;
                        childEntity.IssueQtyCarton = item.IssueQtyCarton;
                        childEntity.Remarks = item.Remarks;
                        childEntity.YarnCategory = item.YarnCategory;
                        childEntity.NoOfThread = item.NoOfThread;
                        childEntity.LotNo = item.YarnLotNo;
                        childEntity.PhysicalCount = item.PhysicalCount;
                        //childEntity.YarnBrandID = item.YarnBrandID;
                        //childEntity.Rate = item.Rate;
                        //childEntity.ShadeCode = item.ShadeCode;
                        entity.Childs.Add(childEntity);

                        foreach (var crbObj in item.ChildRackBins)
                        {
                            YDReqIssueChildRackBinMapping crbObjKY = new YDReqIssueChildRackBinMapping();
                            crbObjKY.YDRICRBId = crbObj.KSCICRBId;
                            crbObjKY.YDReqIssueChildID = crbObj.KSCIssueChildID;
                            crbObjKY.ChildRackBinID = crbObj.ChildRackBinID;
                            crbObjKY.IssueCartoon = crbObj.IssueCartoon;
                            crbObjKY.IssueQtyCone = crbObj.IssueQtyCone;
                            crbObjKY.IssueQtyKg = crbObj.IssueQtyKg;
                            crbObjKY.YarnStockSetId = crbObj.YarnStockSetId;
                            childEntity.ChildRackBins.Add(crbObjKY);
                        }
                    }
                    entity.Childs.ForEach(c =>
                    {
                        c.ChildRackBins.ForEach(rb =>
                        {
                            rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType.Deduction, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                        });
                    });
                    //foreach (YDReqIssueChild child in entity.Childs)
                    //{
                    //    child.ItemMasterID = childRecords.Find(x => x.KSCIssueChildID == child.YDReqIssueChildID).ItemMasterID;
                    //}
                }

                await _yDservice.SaveAsyncYD(entity, rackBins);
            }
            else
            {
                List<KnittingSubContractIssueChild> childRecords = model.Childs;
                //_itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

                KnittingSubContractIssueMaster entity;
                if (model.KSCIssueMasterID > 0)
                {
                    entity = await _service.GetAllAsync(model.KSCIssueMasterID);

                    entity.TransportTypeID = model.TransportTypeID;
                    entity.TransportAgencyID = model.TransportAgencyID;
                    entity.ReqType = model.ReqType;
                    entity.ProgramName = model.ProgramName;
                    entity.VehicleNo = model.VehicleNo;
                    entity.DriverName = model.DriverName;
                    entity.ContactNo = model.ContactNo;
                    entity.IsCompleted = model.IsCompleted;
                    entity.IsSendForApprove = model.IsSendForApprove;
                    entity.KSCIssueDate = model.KSCIssueDate;
                    entity.KSCIssueByID = AppUser.UserCode;
                    entity.Remarks = model.Remarks;
                    entity.UpdatedBy = AppUser.UserCode;
                    entity.DateUpdated = DateTime.Now;
                    if (model.IsApprove == true)
                    {
                        entity.IsApprove = true;
                        entity.ApproveBy = AppUser.UserCode;
                        entity.ApproveDate = DateTime.Now;

                        entity.IsReject = false;
                        entity.RejectBy = 0;
                        entity.RejectDate = null;
                        entity.RejectReason = "";
                    }
                    else if (model.IsReject == true)
                    {
                        entity.IsApprove = false;
                        entity.ApproveBy = 0;
                        entity.ApproveDate = null;

                        entity.IsReject = true;
                        entity.RejectBy = AppUser.UserCode;
                        entity.RejectDate = DateTime.Now;
                        entity.RejectReason = model.RejectReason;
                    }
                    entity.EntityState = EntityState.Modified;

                    //entity.Childs.SetUnchanged();
                    entity.Childs.ForEach(x =>
                    {
                        x.EntityState = EntityState.Unchanged;
                        x.ChildRackBins.SetUnchanged();
                    });
                    foreach (KnittingSubContractIssueChild item in model.Childs)
                    {
                        KnittingSubContractIssueChild childEntity = entity.Childs.FirstOrDefault(x => x.KSCIssueChildID == item.KSCIssueChildID);
                        int childIndexF = entity.Childs.FindIndex(x => x.KSCIssueChildID == item.KSCIssueChildID);

                        if (childEntity == null)
                        {
                            item.KSCIssueMasterID = entity.KSCIssueMasterID;
                            item.ItemMasterID = item.ItemMasterID;
                            entity.Childs.Add(item);
                        }
                        else
                        {
                            childEntity.SpinnerID = item.SpinnerID;
                            childEntity.YarnLotNo = item.YarnLotNo;
                            childEntity.PhysicalCount = item.PhysicalCount;
                            childEntity.IssueQty = item.IssueQty;
                            childEntity.IssueQtyCarton = item.IssueQtyCarton;
                            childEntity.IssueQtyCone = item.IssueQtyCone;
                            childEntity.Remarks = item.Remarks;
                            childEntity.EntityState = EntityState.Modified;
                        }

                        foreach (var crbObj in item.ChildRackBins)
                        {
                            #region rack bin update
                            var childRackBins = childEntity.ChildRackBins.Where(x => x.ChildRackBinID == crbObj.ChildRackBinID).ToList();
                            int cone = 0;
                            int cartoon = 0;
                            decimal qtyKg = 0;

                            childEntity.ChildRackBins.ForEach(c =>
                            {
                                cone += c.IssueQtyCone;
                                cartoon += c.IssueCartoon;
                                qtyKg += c.IssueQtyKg;
                            });

                            cone = crbObj.IssueQtyCone - cone;
                            cartoon = crbObj.IssueCartoon - cartoon;
                            qtyKg = crbObj.IssueQtyKg - qtyKg;
                            #endregion

                            var childMapping = childEntity.ChildRackBins.FirstOrDefault(x => x.KSCICRBId == crbObj.KSCICRBId);
                            int indexF = childEntity.ChildRackBins.FindIndex(x => x.KSCICRBId == crbObj.KSCICRBId);

                            if (childMapping == null)
                            {
                                childMapping = CommonFunction.DeepClone(crbObj);
                                childMapping.KSCIssueChildID = item.KSCIssueChildID;
                                childMapping.EntityState = EntityState.Added;
                                childEntity.ChildRackBins.Add(childMapping);
                            }
                            else if (indexF > -1)
                            {
                                childEntity.ChildRackBins[indexF].IssueCartoon = crbObj.IssueCartoon;
                                childEntity.ChildRackBins[indexF].IssueQtyCone = crbObj.IssueQtyCone;
                                childEntity.ChildRackBins[indexF].IssueQtyKg = crbObj.IssueQtyKg;
                                childEntity.ChildRackBins[indexF].EntityState = EntityState.Modified;
                            }

                            #region rack bin update
                            rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, crbObj.ChildRackBinID, EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType.Deduction, cone, cartoon, qtyKg);
                            #endregion
                        }
                        if (childIndexF > -1)
                        {
                            entity.Childs[childIndexF] = childEntity;
                        }
                    }
                    //foreach (KnittingSubContractIssueChild item in entity.Childs.Where(x => x.EntityState == EntityState.Unchanged))
                    //{
                    //    item.EntityState = EntityState.Deleted;
                    //}
                    foreach (var item in entity.Childs)
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
                                rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType.Addition, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                                #endregion
                            }
                        });
                    }
                }
                else
                {
                    entity = model;
                    //entity.KSCIssueNo = await GetMaxSCYDIssueNoAsync();
                    entity.KSCIssueDate = DateTime.Now;
                    entity.KSCIssueByID = AppUser.UserCode;
                    entity.KSCReqMasterID = model.KSCReqMasterID;
                    //entity.ChallanNo = await GetMaxSCYDChallanNoAsync();
                    entity.ChallanDate = DateTime.Now;
                    entity.GPDate = DateTime.Now;
                    entity.ReqType = model.ReqType;
                    entity.ProgramName = model.ProgramName;
                    entity.TransportTypeID = model.TransportTypeID;
                    entity.TransportAgencyID = model.TransportAgencyID;
                    entity.VehicleNo = model.VehicleNo;
                    entity.DriverName = model.DriverName;
                    entity.ContactNo = model.ContactNo;
                    entity.IsCompleted = model.IsCompleted;
                    entity.IsSendForApprove = model.IsSendForApprove;
                    if (model.IsSendForApprove)
                    {
                        entity.SendForApproveBy = model.SendForApproveBy;
                        entity.SendForApproveDate = DateTime.Now;
                    }
                    entity.Remarks = model.Remarks;
                    entity.AddedBy = AppUser.UserCode;
                    entity.KSCIssueByID = AppUser.UserCode;
                    //foreach (KnittingSubContractIssueChild child in entity.Childs)
                    //{
                    //    child.ItemMasterID = childRecords.Find(x => x.KSCIssueChildID == child.KSCIssueChildID).ItemMasterID;
                    //}
                    entity.Childs.ForEach(c =>
                    {
                        //c.ItemMasterID = childRecords.Find(x => x.KSCIssueChildID == c.KSCIssueChildID).ItemMasterID;
                        c.ChildRackBins.ForEach(rb =>
                        {
                            rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType.Deduction, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                        });
                    });
                }

                await _service.SaveAsyncSC(entity, rackBins);
            }
            return Ok();
        }
        [Route("approve")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Approve(KnittingSubContractIssueMaster model)
        {
            List<YarnReceiveChildRackBin> rackBins = new List<YarnReceiveChildRackBin>();

            #region Rack Bin List
            List<int> rackBinIds = new List<int>();

            model.Childs.ForEach(c =>
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

            if (model.ReqType == "YD")
            {
                YDReqIssueMaster entity = await _yDservice.GetAllAsync(model.KSCIssueMasterID);
                entity.ProgramName = model.ProgramName;

                entity.IsApprove = true;
                entity.ApproveBy = AppUser.UserCode;
                entity.ApproveDate = DateTime.Now;

                entity.IsReject = false;
                entity.RejectBy = 0;
                entity.RejectDate = null;
                entity.RejectReason = "";
                //entity.Childs.ForEach(c =>
                //{
                //    c.ChildRackBins.ForEach(rb =>
                //    {
                //        rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EnumRackBinOperationType.Deduction, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                //    });
                //});
                entity.EntityState = EntityState.Modified;
                entity.Childs = new List<YDReqIssueChild>();
                await _yDservice.SaveAsync(entity);
            }
            else
            {
                KnittingSubContractIssueMaster entity = await _service.GetAllAsync(model.KSCIssueMasterID);
                entity.ProgramName = model.ProgramName;

                entity.IsApprove = true;
                entity.ApproveBy = AppUser.UserCode;
                entity.ApproveDate = DateTime.Now;

                entity.IsReject = false;
                entity.RejectBy = 0;
                entity.RejectDate = null;
                entity.RejectReason = "";
                //entity.Childs.ForEach(c =>
                //{
                //    c.ChildRackBins.ForEach(rb =>
                //    {
                //        rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EnumRackBinOperationType.Deduction, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                //    });
                //});
                entity.EntityState = EntityState.Modified;
                entity.Childs = new List<KnittingSubContractIssueChild>();
                await _service.SaveAsync(entity);
            }
            return Ok();
        }
        [Route("reject")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Reject(KnittingSubContractIssueMaster model)
        {
            List<YarnReceiveChildRackBin> rackBins = new List<YarnReceiveChildRackBin>();

            #region Rack Bin List
            List<int> rackBinIds = new List<int>();

            model.Childs.ForEach(c =>
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

            if (model.ProgramName == "YD")
            {

                YDReqIssueMaster entity = await _yDservice.GetAllAsync(model.KSCIssueMasterID);

                entity.IsApprove = false;
                entity.ApproveBy = 0;
                entity.ApproveDate = null;

                entity.IsReject = true;
                entity.RejectBy = AppUser.UserCode;
                entity.RejectDate = DateTime.Now;
                entity.RejectReason = model.RejectReason;
                entity.Childs.ForEach(c =>
                {
                    c.ChildRackBins.ForEach(rb =>
                    {
                        rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType.Addition, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                    });
                });
                entity.EntityState = EntityState.Modified;
                entity.Childs = new List<YDReqIssueChild>();

                await _yDservice.SaveAsync(entity, rackBins);
            }
            else
            {
                KnittingSubContractIssueMaster entity = await _service.GetAllAsync(model.KSCIssueMasterID);

                entity.IsApprove = false;
                entity.ApproveBy = 0;
                entity.ApproveDate = null;

                entity.IsReject = true;
                entity.RejectBy = AppUser.UserCode;
                entity.RejectDate = DateTime.Now;
                entity.RejectReason = model.RejectReason;
                entity.Childs.ForEach(c =>
                {
                    c.ChildRackBins.ForEach(rb =>
                    {
                        rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType.Addition, rb.IssueQtyCone, rb.IssueCartoon, rb.IssueQtyKg);
                    });
                });
                entity.EntityState = EntityState.Modified;
                entity.Childs = new List<KnittingSubContractIssueChild>();

                await _service.SaveAsync(entity, rackBins);
            }
            return Ok();
        }
        [Route("approveGP")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> ApproveGP(KnittingSubContractIssueMaster model)
        {
            string GPNo = "";
            if (model.ReqType == "YD")
            {

                YDReqIssueMaster entity = await _yDservice.GetAllAsync(model.KSCIssueMasterID);

                //entity.GPNo = await GetMaxSCYDGPNoAsync();
                GPNo = entity.GPNo;
                entity.GPDate = DateTime.Now;
                entity.IsGPApprove = true;
                entity.GPApproveBy = AppUser.UserCode;
                entity.GPApproveDate = DateTime.Now;

                entity.EntityState = EntityState.Modified;
                entity.Childs = new List<YDReqIssueChild>();

                await _yDservice.SaveAsync(entity);
            }
            else
            {
                KnittingSubContractIssueMaster entity = await _service.GetAllAsync(model.KSCIssueMasterID);
                //entity.GPNo = await GetMaxSCYDGPNoAsync();
                GPNo = entity.GPNo;
                entity.GPDate = DateTime.Now;
                entity.IsGPApprove = true;
                entity.GPApproveBy = AppUser.UserCode;
                entity.GPApproveDate = DateTime.Now;

                entity.EntityState = EntityState.Modified;
                entity.Childs = new List<KnittingSubContractIssueChild>();
                await _service.SaveAsync(entity);
            }
            return Ok(GPNo);
        }
    }
}
