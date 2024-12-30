using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Inventory
{
    [Route("api/yarn-receive")]
    public class YarnReceiveController : ApiBaseController
    {
        private readonly IYarnReceiveService _service;
        private readonly IItemMasterService<YarnReceiveChild> _itemMasterService;

        //IUserService _userService = null;

        public YarnReceiveController(IUserService userService, IYarnReceiveService service
        , IItemMasterService<YarnReceiveChild> itemMasterService
        , ICommonHelperService commonService) : base(userService)
        {
            _service = service;
            _itemMasterService = itemMasterService;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, bool isCDAPage)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnReceiveMaster> records = await _service.GetPagedAsync(status, isCDAPage, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{CIID}/{POID}/{isCDAPage}")]
        public async Task<IActionResult> GetNew(int CIID, int POID, bool isCDAPage)
        {
            return Ok(await _service.GetNewAsync(CIID, POID, isCDAPage));
        }
        [HttpGet]
        [Route("new/sample-yarn")]
        public async Task<IActionResult> GetSampleYarn()
        {
            return Ok(await _service.GetNewSampleYarnAsync());
        }

        [Route("{id}/{poId}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, int poId)
        {
            var record = await _service.GetAsync(id, poId);
            return Ok(record);
        }
        [Route("getPrevReq/{LotNo}/{ItemMasterID}")]
        [HttpGet]
        public async Task<IActionResult> getPrevReq(string LotNo, int ItemMasterID)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPrevReq(paginationInfo, LotNo, ItemMasterID);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(YarnReceiveMaster modelParam)
        {
            YarnReceiveMaster model = JsonConvert.DeserializeObject<YarnReceiveMaster>(Convert.ToString(modelParam));

            YarnReceiveMaster entity = new YarnReceiveMaster();
            List<YarnReceiveChild> childRecords = model.YarnReceiveChilds;
            _itemMasterService.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref childRecords);

            if (model.ReceiveID > 0)
            {
                entity = await _service.GetAllAsync(model.ReceiveID);
                entity.ReceiveDate = model.ReceiveDate;
                entity.SpinnerID = model.SpinnerID;
                entity.TransportMode = model.TransportMode;
                entity.VehicalNo = model.VehicalNo;
                entity.ChallanNo = model.ChallanNo;
                entity.ACompanyInvoice = model.ACompanyInvoice;
                entity.TransportTypeID = model.TransportTypeID;
                entity.ShipmentStatus = model.ShipmentStatus;
                entity.ChallanDate = model.ChallanDate;
                entity.CContractorID = model.CContractorID;
                entity.LocationID = model.LocationID;
                entity.PLNo = model.PLNo;
                entity.PLDate = model.PLDate;
                entity.BLNo = model.BLNo;
                entity.BLDate = DateTime.Now;
                entity.GPNo = model.GPNo;
                entity.GPDate = model.GPDate;
                entity.GPTime = DateTime.Now.ToString("mm:ss");
                entity.ReceivedByID = AppUser.UserCode; // model.ReceivedById;
                entity.Remarks = model.Remarks;

                entity.MushakNo = model.MushakNo;
                entity.MushakDate = model.MushakDate;
                entity.BillEntryNo = model.BillEntryNo;

                entity.SupplierID = model.SupplierID;
                entity.RCompanyID = model.RCompanyID;
                entity.RCompany = model.RCompany;
                entity.OCompanyID = model.RCompanyID; //Both Same OCompanyID = RCompanyID

                entity.TruckChallanNo = model.TruckChallanNo;
                entity.TruckChallanDate = model.TruckChallanDate;

                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.YarnReceiveChilds.ForEach(c =>
                {
                    c.EntityState = EntityState.Unchanged;
                    c.YarnReceiveChildOrders.SetUnchanged();
                    c.YarnReceiveChildBuyers.SetUnchanged();
                });

                //YarnReceiveChild child;
                foreach (var item in childRecords)
                {
                    var child = entity.YarnReceiveChilds.FirstOrDefault(x => x.ChildID == item.ChildID);
                    if (child == null)
                    {
                        if (item.POForName == "Projection Based Sample" || item.POForName == "Order Based Sample" || item.ReceiveForName == "Sample")
                        {
                            item.IsNoTest = true;
                            item.NoTestRemarks = "Auto No Test Based On POForName/ReceiveForName";
                            item.NoTestBy = AppUser.UserCode;
                            item.NoTestDate = DateTime.Now;
                        }
                        item.ReceiveID = entity.ReceiveID;
                        item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        item.EntityState = EntityState.Added;
                        entity.YarnReceiveChilds.Add(item);
                    }
                    else
                    {
                        if (child.POForName == "Projection Based Sample" || child.POForName == "Order Based Sample" || child.ReceiveForName == "Sample")
                        {
                            child.IsNoTest = true;
                            child.NoTestRemarks = "Auto No Test Based On POForName/ReceiveForName";
                            child.NoTestBy = AppUser.UserCode;
                            child.NoTestDate = DateTime.Now;
                        }
                        child.LotNo = item.LotNo;
                        child.NoOfCartoon = item.NoOfCartoon;
                        child.NoOfCone = item.NoOfCone;
                        child.ChallanQty = item.ChallanQty;
                        child.ReceiveQty = item.ReceiveQty;
                        child.ExcessQty = item.ExcessQty;
                        child.ShortQty = item.ShortQty;
                        child.Remarks = item.Remarks;
                        child.ShadeCode = item.ShadeCode;
                        child.ReceiveForId = item.ReceiveForId;
                        child.SpinnerID = item.SpinnerID;
                        child.PhysicalCount = item.PhysicalCount;
                        child.ChallanLot = item.ChallanLot;
                        child.ChallanCount = item.ChallanCount;
                        child.YarnPackingID = item.YarnPackingID;
                        child.Segment6ValueId = item.Segment6ValueId;
                        child.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        child.EntityState = EntityState.Modified;

                        #region YarnReceiveChildBuyer
                        item.YarnReceiveChildBuyers.ForEach(o =>
                        {
                            var buyer = child.YarnReceiveChildBuyers.FirstOrDefault(x => x.BuyerID == o.BuyerID);
                            if (buyer == null)
                            {
                                buyer = CommonFunction.DeepClone(o);
                                buyer.ReceiveChildID = child.ChildID;
                                buyer.EntityState = EntityState.Added;
                                child.YarnReceiveChildBuyers.Add(buyer);
                            }
                            else
                            {
                                buyer.BuyerID = o.BuyerID;
                                buyer.EntityState = EntityState.Modified;
                            }
                        });
                        #endregion YarnReceiveChildBuyer

                        #region YarnReceiveChildOrder
                        item.YarnReceiveChildOrders.ForEach(o =>
                        {
                            var order = child.YarnReceiveChildOrders.FirstOrDefault(x => x.ExportOrderID == o.ExportOrderID);
                            if (order == null)
                            {
                                order = CommonFunction.DeepClone(o);
                                order.ReceiveChildID = child.ChildID;
                                order.IsSample = model.IsSampleYarn;
                                order.EntityState = EntityState.Added;
                                child.YarnReceiveChildOrders.Add(order);
                            }
                            else
                            {
                                order.BuyerID = o.BuyerID;
                                order.ExportOrderID = o.ExportOrderID;
                                order.EWONo = o.EWONo;
                                order.Qty = o.Qty;
                                order.BuyerName = o.BuyerName;
                                order.BuyerTeamID = o.BuyerTeamID;
                                order.IsSample = model.IsSampleYarn;
                                order.EntityState = EntityState.Modified;
                            }
                        });

                        #endregion YarnReceiveChildOrder
                    }
                }

                foreach (var item in entity.YarnReceiveChilds.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                    item.YarnReceiveChildOrders.SetDeleted();
                    item.YarnReceiveChildBuyers.SetDeleted();
                }
                entity.YarnReceiveChilds.ForEach(c =>
                {
                    c.YarnReceiveChildOrders.Where(o => o.EntityState == EntityState.Unchanged).ToList().SetDeleted();
                    c.YarnReceiveChildBuyers.Where(o => o.EntityState == EntityState.Unchanged).ToList().SetDeleted();
                });
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;
                entity.ReceivedByID = AppUser.UserCode;
                //entity.PLDate = DateTime.Now;
                entity.BLDate = DateTime.Now;
                //entity.GPDate = DateTime.Now;
                entity.GPTime = DateTime.Now.ToString("HH:mm");
                entity.RCompanyID = model.RCompanyID;
                entity.OCompanyID = model.RCompanyID; //Both Same OCompanyID = RCompanyID
                entity.TransportMode = 0;

                entity.YarnReceiveChilds.ForEach(c =>
                {
                    if (c.POForName == "Projection Based Sample" || c.POForName == "Order Based Sample" || c.ReceiveForName == "Sample")
                    {
                        c.IsNoTest = true;
                        c.NoTestRemarks = "Auto No Test Based On POForName/ReceiveForName";
                        c.NoTestBy = AppUser.UserCode;
                        c.NoTestDate = DateTime.Now;
                    }
                    c.YarnCategory = CommonFunction.GetYarnShortForm(c.Segment1ValueDesc, c.Segment2ValueDesc, c.Segment3ValueDesc, c.Segment4ValueDesc, c.Segment5ValueDesc, c.Segment6ValueDesc, c.ShadeCode);
                    c.YarnReceiveChildOrders.ForEach(o =>
                    {
                        o.IsSample = model.IsSampleYarn;
                    });
                });
            }

            if (model.IsSendForApprove)
            {
                entity.IsSendForApprove = true;
            }
            else if (model.IsApproved)
            {
                entity.IsApproved = true;
                entity.ApprovedBy = AppUser.UserCode;
                entity.ApprovedDate = DateTime.Now;
            }
            await _service.SaveAsync(entity, AppUser.UserCode);
            return Ok();
        }

        [Route("delete")]
        [HttpPost]
        public async Task<IActionResult> Delete(YarnReceiveMaster model)
        {
            YarnReceiveMaster entity = new YarnReceiveMaster();
            if (model.ReceiveID > 0)
            {
                entity = await _service.GetAllAsync(model.ReceiveID);
                if (!entity.IsApproved)
                {
                    entity.EntityState = EntityState.Deleted;
                    entity.YarnReceiveChilds.ForEach(c =>
                    {
                        c.EntityState = EntityState.Deleted;
                        c.YarnReceiveChildBuyers.SetDeleted();
                        c.YarnReceiveChildOrders.SetDeleted();
                    });
                    await _service.DeleteAsync(entity, AppUser.UserCode);
                }
                else
                {
                    throw new Exception("Delete not possible, already approved.");
                }
            }
            return Ok();
        }

        [Route("update-NoTest")]
        [HttpPost]
        public async Task<IActionResult> UpdateNoTest(YarnReceiveChild model)
        {
            YarnReceiveChild entityChild = new YarnReceiveChild();
            if (model.ChildID > 0)
            {
                entityChild = await _service.GetReceiveChild(model.ChildID);
                entityChild.IsNoTest = true;
                entityChild.NoTestRemarks = model.NoTestRemarks;
                entityChild.NoTestBy = AppUser.UserCode;
                entityChild.NoTestDate = DateTime.Now;
                entityChild.EntityState = EntityState.Modified;
                await _service.UpdateChildAsync(entityChild);
            }
            return Ok();
        }

        [Route("update-Tag")]
        [HttpPost]
        //public async Task<IActionResult> UpdateTag(YarnReceiveChild model)
        public async Task<IActionResult> UpdateTag(dynamic jsonString)
        {
            YarnReceiveChild model = JsonConvert.DeserializeObject<YarnReceiveChild>(Convert.ToString(jsonString));
            YarnReceiveChild entityChild = new YarnReceiveChild();
            if (model.ChildID > 0 && model.TagYarnReceiveChildID > 0)
            {
                entityChild = await _service.GetReceiveChild(model.ChildID);
                entityChild.TagYarnReceiveChildID = model.TagYarnReceiveChildID;
                entityChild.TagBy = AppUser.UserCode;
                entityChild.TagDate = DateTime.Now;
                entityChild.EntityState = EntityState.Modified;
                await _service.UpdateChildAsync(entityChild);
            }
            return Ok();
        }
    }
}
