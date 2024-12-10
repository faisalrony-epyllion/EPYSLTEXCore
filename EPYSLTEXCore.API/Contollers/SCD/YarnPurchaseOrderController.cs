using AutoMapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Data.Entity;

namespace EPYSLTEX.Web.Controllers.Apis
{
    [Authorize]
    [Route("api/ypo")]
    public class YarnPurchaseOrderController : ApiBaseController
    {
        private readonly ISelect2Service _select2Service;
        private readonly IYarnPOService _yarnPOService;
        IDapperCRUDService<IDapperBaseEntity> _signatureService;
        //private readonly IEmailService _emailService;
        //private readonly IReportingService _reportingService;

        private static Logger _logger;
        private readonly IItemMasterService<YarnPOChild> _itemMasterService;

        public YarnPurchaseOrderController(IUserService userService, ISelect2Service select2Service
            , IYarnPOService yarnPOService, IDapperCRUDService<IDapperBaseEntity> signatureService
        //, IEmailService emailService
        //, IReportingService reportingService
     
            , IItemMasterService<YarnPOChild> itemMasterService) : base(userService)
        {
            _select2Service = select2Service;
            _yarnPOService = yarnPOService;
            _signatureService = signatureService;
            //_emailService = emailService;
            //_reportingService = reportingService;

            _logger = LogManager.GetCurrentClassLogger();
            _itemMasterService = itemMasterService;
        }

        [Route("list")]
        public async Task<IActionResult> GetYarnPOLists(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnPOMaster> records = await _yarnPOService.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("new")]
        public async Task<IActionResult> GetNew()
        {
            return Ok(await _yarnPOService.GetNewAsync());
        }

        [Route("get-supplier-info/{supplierId}")]
        public async Task<IActionResult> GetSupplierInfo(int supplierId)
        {
            return Ok(await _yarnPOService.GetSupplierInfo(supplierId));
        }
        [Route("get-pr-items/{companyId}/{prChildIds}")]
        public async Task<IActionResult> GetPRItems(int companyId, string prChildIds)
        {
            prChildIds = CommonFunction.ReplaceInvalidChar(prChildIds);
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnPOMaster> records = await _yarnPOService.GetPRItems(companyId, prChildIds, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("new/{purchaseReqId}/{yarnPRChildID}/{companyId}")]
        public async Task<IActionResult> GetNewByPR(string purchaseReqId, string yarnPRChildID, int companyId)
        {
            YarnPOMaster records = await _yarnPOService.GetNewAsync(purchaseReqId, yarnPRChildID, companyId);
            records.YarnPOChilds.ForEach(item =>
            {
                item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
            });
            return Ok(records);
        }

        // May need to remvoe this later
        [Route("supplier-info/{supplierId}")]
        public async Task<IActionResult> GetSupplierInformation(int supplierId)
        {
            YarnPOMaster record = await _yarnPOService.GetAsync(supplierId);
            Guard.Against.NullObject(supplierId, record);

            record.IncoTermsList = await _select2Service.GetIncoTermsSupplierWiseAsync(supplierId);
            record.PaymentTermsList = await _select2Service.GetPaymentTermsSupplierWiseAsync(supplierId);
            record.CountryOfOriginList = await _select2Service.GetSupplierWiseCountryNameAsync(supplierId);
            record.CalculationofTenureList = await _select2Service.GetEntityTypes(EntityTypeConstants.CALCULATION_OF_TENURE);
            record.ShipmentModeList = await _select2Service.GetEntityTypes(EntityTypeConstants.SHIPMENT_MODE);
            record.CreditDaysList = await _select2Service.GetEntityTypes(EntityTypeConstants.LC_TENURE);
            record.PortofLoadingList = await _select2Service.GetEntityTypes(EntityTypeConstants.PORT_OF_LOADING);

            return Ok(record);
        }

        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _yarnPOService.GetAsync(id));
        }
        [Route("revision/{id}")]
        public async Task<IActionResult> GetRevision(int id)
        {
            return Ok(await _yarnPOService.GetRevisionAsync(id));
        }

        [Route("orderlistsfromcompany")]
        public async Task<IActionResult> GetOrderListsFromCompany(PaginationInfo paginationInfo)
        {
            List<YarnPOChildOrder> records = await _yarnPOService.GetOrderListsFromCompany(paginationInfo);
            var items = records.Select(x => new
            {
                x.ExportOrderId,
                x.BuyerID,
                x.BuyerName,
                x.BuyerTeamID,
                x.BuyerTeam,
                x.EWONo,
                x.StyleNo
            });
            var totalCount = records.FirstOrDefault() == null ? 0 : records.FirstOrDefault().TotalRows;
            var response = new TableResponseModel(totalCount, items);

            return Ok(response);
        }

        [Route("yarnpolistsShowInPIReceive")]
        public async Task<IActionResult> GetYarnPOListsShowInPIReceive(int SupplierId, PaginationInfo paginationInfo)
        {
            List<YarnPOMaster> records = await _yarnPOService.GetYarnPOListsShowInPIReceive(SupplierId, paginationInfo);
            var items = records.Select(x => new
            {
                x.YPOMasterID,
                x.PoNo,
                PoDateStr = x.PoDate.ToShortDateString(),
                x.SupplierName,
                x.QuotationRefNo,
                //x.POFor,
                DeliveryStartDateStr = x.DeliveryStartDate.ToShortDateString(),
                DeliveryEndDateStr = x.DeliveryEndDate.ToShortDateString(),
                x.TotalQty,
                x.TotalValue,
                //x.POStatus,
                //InHouseDateStr = x.InHouseDate.Value.ToShortDateString(),
                //x.UserName,
                x.CompanyName,
                x.PoForId,
                x.CurrencyId,
                x.Remarks,
                x.InternalNotes,
                x.IncoTermsId,
                x.PaymentTermsId,
                x.TypeOfLcId,
                x.TenureofLc,
                x.CalculationofTenure,
                x.CreditDays,
                x.OfferValidity,
                x.ReImbursementCurrencyId,
                x.Charges,
                x.CountryOfOriginId,
                x.TransShipmentAllow,
                x.ShippingTolerance,
                x.PortofLoadingID,
                x.PortofDischargeID,
                x.ShipmentModeId
            });
            int totalCount = records.FirstOrDefault() == null ? 0 : records.FirstOrDefault().TotalRows;
            var response = new TableResponseModel(totalCount, items);

            return Ok(response);
        }

        [Route("BuyerListsfromCompany")]
        public async Task<IActionResult> GetBuyerListsFromCompany(PaginationInfo paginationInfo)
        {
            List<YarnPOChildOrder> records = await _yarnPOService.GetBuyerListsFromCompany(paginationInfo);
            var items = records.Select(x => new
            {
                x.BuyerID,
                x.BuyerName
            });
            int totalCount = records.FirstOrDefault() == null ? 0 : records.FirstOrDefault().TotalRows;
            var response = new TableResponseModel(totalCount, items);

            return Ok(response);
        }

        [Route("exportorderlistsfromidedit/{id}")]
        public async Task<IActionResult> GetExportOrderListsFromIdEdit(int id)
        {
            List<YarnPOChildOrder> records = await _yarnPOService.GetExportOrderListsFromIdEdit(id);
            var items = records.Select(x => new
            {
                x.ExportOrderId,
                x.BuyerID,
                x.BuyerName,
                x.BuyerTeamID,
                x.BuyerTeam,
                x.EWONo,
                x.StyleNo
            });

            return Ok(items);
        }

        [Route("pr-child-list")]
        public async Task<IActionResult> GetPRChildList(Status status, string childIDs, int CompanyId)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnPOChild> records = await _yarnPOService.GetPRChildList(status, paginationInfo, childIDs, CompanyId);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(YarnPOMaster model)
        {
            string conceptNo = string.Join(",", model.YarnPOChilds.Select(x => x.ConceptNo).Distinct());
            List<YarnPOChild> childRecords = model.YarnPOChilds;
            _itemMasterService.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            YarnPOMaster entity;
            if (model.YPOMasterID > 0)
            {
                entity = await _yarnPOService.GetAllByIDAsync(model.YPOMasterID);
                entity.ConceptNo = conceptNo;
                entity.EntityState = EntityState.Modified;

                if (entity.CompanyId != model.CompanyId)
                {
                    entity.CompanyId = model.CompanyId;
                    entity.PoNo = await _signatureService.GetMaxNoAsync(TableNames.YPONo, entity.CompanyId, RepeatAfterEnum.EveryYear);
                }

                if (model.Proposed && !entity.Proposed)
                {
                    entity.Proposed = model.Proposed;
                    entity.ProposedBy = AppUser.UserCode;
                    entity.ProposedDate = DateTime.Now;
                }
                else
                {
                    entity.Proposed = model.Proposed;
                }
                entity.PoDate = model.PoDate;
                entity.CompanyId = model.CompanyId;
                entity.SupplierId = model.SupplierId;
                entity.CurrencyId = model.CurrencyId;
                entity.DeliveryStartDate = model.DeliveryStartDate;
                entity.DeliveryEndDate = model.DeliveryEndDate;
                entity.Remarks = model.Remarks;
                entity.InternalNotes = model.InternalNotes;
                entity.IncoTermsId = model.IncoTermsId;
                entity.PaymentTermsId = model.PaymentTermsId;
                entity.TypeOfLcId = model.TypeOfLcId;
                entity.TenureofLc = model.TenureofLc;
                entity.CalculationofTenure = model.CalculationofTenure;
                entity.CreditDays = model.CreditDays;
                entity.ReImbursementCurrencyId = model.ReImbursementCurrencyId;
                entity.Charges = model.Charges;
                entity.CountryOfOriginId = model.CountryOfOriginId;
                entity.TransShipmentAllow = model.TransShipmentAllow;
                entity.ShippingTolerance = model.ShippingTolerance;
                entity.PortofLoadingID = model.PortofLoadingID;
                entity.PortofDischargeID = model.PortofDischargeID;
                entity.ShipmentModeId = model.ShipmentModeId;
                entity.QuotationRefNo = model.QuotationRefNo;
                entity.QuotationRefDate = model.QuotationRefDate;
                entity.UnApprove = false;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;

                entity.EntityState = EntityState.Modified;
                if (model.IsRevision)
                {
                    entity.Approved = false;
                    entity.ApprovedBy = 0;
                    entity.ApprovedDate = null;
                    entity.RevisionNo = entity.RevisionNo + 1;
                    entity.IsRevision = true;
                    entity.RevisionBy = AppUser.EmployeeCode;
                    entity.RevisionDate = DateTime.Now;
                    entity.RevisionReason = model.RevisionReason;
                }
                else if (model.IsCancel)
                {
                    entity.IsCancel = true;
                    entity.CancelBy = AppUser.EmployeeCode;
                    entity.CancelDate = DateTime.Now;
                    entity.CancelReason = model.CancelReason;
                }
                foreach (YarnPOChild item in entity.YarnPOChilds)
                {
                    item.YarnPOChildBuyers.SetUnchanged();
                    item.YarnPOChildOrders.SetUnchanged();
                    item.EntityState = EntityState.Unchanged;
                }

                foreach (YarnPOChild item in model.YarnPOChilds)
                {
                    YarnPOChild existingYarnPOChild = entity.YarnPOChilds.FirstOrDefault(x => x.YPOChildID == item.YPOChildID);
                    if (existingYarnPOChild != null)
                    {
                        existingYarnPOChild.PRMasterID = item.PRMasterID;
                        existingYarnPOChild.PRChildID = item.PRChildID;
                        existingYarnPOChild.YarnCategory = item.YarnCategory;
                        existingYarnPOChild.UnitID = item.UnitID;
                        existingYarnPOChild.BuyerID = item.BuyerID;
                        existingYarnPOChild.PoQty = item.PoQty;
                        existingYarnPOChild.Rate = item.Rate;
                        existingYarnPOChild.PIValue = item.PIValue;
                        existingYarnPOChild.Remarks = item.Remarks;
                        existingYarnPOChild.YarnLotNo = item.YarnLotNo;
                        existingYarnPOChild.HSCode = item.HSCode;
                        existingYarnPOChild.QuotationRefNo = item.QuotationRefNo;
                        existingYarnPOChild.QuotationRefDate = item.QuotationRefDate;
                        existingYarnPOChild.BookingNo = item.BookingNo;
                        existingYarnPOChild.YarnShade = item.Segment4ValueDesc;
                        existingYarnPOChild.YarnProgramId = item.YarnProgramId;
                        existingYarnPOChild.NoOfThread = item.NoOfThread;
                        existingYarnPOChild.PoForId = item.PoForId;
                        existingYarnPOChild.ShadeCode = item.ShadeCode;
                        existingYarnPOChild.POCone = item.POCone;
                        //existingYarnPOChild.HSCode = item.HSCode;
                        existingYarnPOChild.ConceptID = item.ConceptID;
                        existingYarnPOChild.ReceivedCompleted = item.ReceivedCompleted;
                        existingYarnPOChild.ReceivedDate = DateTime.Now;
                        existingYarnPOChild.ItemMasterID = item.ItemMasterID;
                        existingYarnPOChild.YarnPRNo = item.YarnPRNo;
                        existingYarnPOChild.ConceptID = item.ConceptID;
                        existingYarnPOChild.ConceptNo = item.ConceptNo;
                        existingYarnPOChild.DayValidDurationId = item.DayValidDurationId;
                        existingYarnPOChild.BaseTypeId = item.BaseTypeId;
                        existingYarnPOChild.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        existingYarnPOChild.EntityState = EntityState.Modified;

                        #region Process YarnPOChildBuyer

                        item.YarnChildPoBuyerIds = item.YarnChildPoBuyerIds ?? "";
                        var buyerIdArray = Array.ConvertAll(item.YarnChildPoBuyerIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), int.Parse).Where(x => x > 0); ;
                        foreach (var buyerId in buyerIdArray)
                        {
                            YarnPOChildBuyer yarnPOChildBuyerEntity = existingYarnPOChild.YarnPOChildBuyers.FirstOrDefault(x => x.BuyerId == buyerId);
                            if (yarnPOChildBuyerEntity == null)
                            {
                                yarnPOChildBuyerEntity = new YarnPOChildBuyer
                                {
                                    BuyerId = buyerId,
                                    YPOMasterID = entity.YPOMasterID,
                                    YPOChildID = existingYarnPOChild.YPOChildID
                                };
                                existingYarnPOChild.YarnPOChildBuyers.Add(yarnPOChildBuyerEntity);
                            }
                        }

                        foreach (YarnPOChildBuyer buyer in existingYarnPOChild.YarnPOChildBuyers.Where(x => !buyerIdArray.Contains(x.BuyerId)))
                            buyer.EntityState = EntityState.Deleted;

                        #endregion Process YarnPOChildBuyer

                        #region Process YarnPOChildOrder

                        item.YarnChildPoExportIds = item.YarnChildPoExportIds ?? "";
                        var exportOrderIdArray = Array.ConvertAll(item.YarnChildPoExportIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), int.Parse).Where(x => x > 0); ;
                        foreach (var exportOrderId in exportOrderIdArray)
                        {
                            YarnPOChildOrder yarnPOChildOrderEntity = existingYarnPOChild.YarnPOChildOrders.FirstOrDefault(x => x.ExportOrderId == exportOrderId);
                            if (yarnPOChildOrderEntity == null)
                            {
                                yarnPOChildOrderEntity = new YarnPOChildOrder
                                {
                                    ExportOrderId = exportOrderId,
                                    YPOMasterID = entity.YPOMasterID,
                                    YPOChildID = existingYarnPOChild.YPOChildID
                                };
                                existingYarnPOChild.YarnPOChildOrders.Add(yarnPOChildOrderEntity);
                            }
                        }

                        foreach (YarnPOChildOrder exportOrder in existingYarnPOChild.YarnPOChildOrders.Where(x => !exportOrderIdArray.Contains(x.ExportOrderId)))
                            exportOrder.EntityState = EntityState.Deleted;

                        #endregion Process YarnPOChildOrder
                    }
                }

                // Remove deleted childs
                var deletedChilds = entity.YarnPOChilds.ToList().Where(x => !model.YarnPOChilds.Select(cm => cm.YPOChildID).Contains(x.YPOChildID));
                foreach (YarnPOChild item in deletedChilds)
                {
                    item.YarnPOChildBuyers.SetDeleted();
                    item.YarnPOChildOrders.SetDeleted();
                    item.EntityState = EntityState.Deleted;
                }

            }
            else
            {
                entity = CommonFunction.DeepClone(model);
                entity.ConceptNo = conceptNo;
                entity.AddedBy = AppUser.UserCode;

                if (model.Proposed)
                {
                    entity.Proposed = model.Proposed;
                    entity.ProposedBy = AppUser.UserCode;
                    entity.ProposedDate = DateTime.Now;
                }
            }

            await _yarnPOService.SaveAsync(entity, model.YarnPOChilds.ToList(), AppUser.UserCode);
            return Ok(entity.PoNo);
        }

        [Route("propose-ypo/{id}")]
        [HttpPost]
        public async Task<IActionResult> ProposeYPO(int id)
        {
            YarnPOMaster entity = await _yarnPOService.GetAllByIDAsync(id);
            entity.Proposed = true;
            entity.ProposedBy = AppUser.UserCode;
            entity.ProposedDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;

            await _yarnPOService.UpdateEntityAsync(entity);
            return Ok();
        }

        [Route("approve-ypo/{id}")]
        [HttpPost]
        public async Task<IActionResult> ApproveYarnPOList(int id)
        {
            YarnPOMaster entity = await _yarnPOService.GetAllByIDAsync(id);
            entity.Approved = true;
            entity.ApprovedBy = AppUser.UserCode;
            entity.ApprovedDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _yarnPOService.UpdateEntityAsync(entity);
            //try
            //{
            //    var attachment = await _reportingService.GetPdfByte(990, AppUser.UserCode, entity.PoNo);
            //    await _emailService.SendAutoEmailAsync("ERP Team", "h.rahman@epylliongroup.com;alam.hossain@epylliongroup.com", "Yanr PO Approved", $"Yarn PO <b>{entity.PoNo}</b> approved.", $"{entity.PoNo}.pdf", attachment);
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error(ex);
            //}

            return Ok();
        }

        [Route("reject-ypo")]
        [HttpPost]
        public async Task<IActionResult> UnApproveYarnPOList(YarnPOMaster model)
        {
            YarnPOMaster entity = await _yarnPOService.GetAllByIDAsync(model.YPOMasterID);
            entity.Proposed = false;
            entity.Approved = false;
            entity.UnApprove = true;
            entity.UnApproveBy = AppUser.UserCode;
            entity.UnApproveDate = DateTime.Now;
            entity.UnapproveReason = model.UnapproveReason;
            entity.EntityState = EntityState.Modified;

            await _yarnPOService.UpdateEntityAsync(entity);
            return Ok();
        }

        [Route("revision-ypo")]
        [HttpPost]
        public async Task<IActionResult> RevisionYarnPOList(YarnPOMaster model)
        {
            List<YarnPOChild> childRecords = model.YarnPOChilds;
            _itemMasterService.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            string conceptNo = string.Join(",", model.YarnPOChilds.Select(x => x.ConceptNo).Distinct());
            YarnPOMaster entity = await _yarnPOService.GetAllByIDAsync(model.YPOMasterID);
            //Revision from Reject
            if (model.YPOMasterID > 0)
            {
                entity = await _yarnPOService.GetAllByIDAsync(model.YPOMasterID);
                entity.ConceptNo = conceptNo;
                entity.EntityState = EntityState.Modified;

                if (entity.CompanyId != model.CompanyId)
                {
                    entity.CompanyId = model.CompanyId;
                    entity.PoNo = await _signatureService.GetMaxNoAsync(TableNames.YPONo, entity.CompanyId, RepeatAfterEnum.EveryYear);
                }

                if (model.Proposed && !entity.Proposed)
                {
                    entity.Proposed = model.Proposed;
                    entity.ProposedBy = AppUser.UserCode;
                    entity.ProposedDate = DateTime.Now;
                }

                entity.PoDate = model.PoDate;
                entity.CompanyId = model.CompanyId;
                entity.SupplierId = model.SupplierId;
                entity.CurrencyId = model.CurrencyId;
                entity.DeliveryStartDate = model.DeliveryStartDate;
                entity.DeliveryEndDate = model.DeliveryEndDate;
                entity.Remarks = model.Remarks;
                entity.InternalNotes = model.InternalNotes;
                entity.IncoTermsId = model.IncoTermsId;
                entity.PaymentTermsId = model.PaymentTermsId;
                entity.TypeOfLcId = model.TypeOfLcId;
                entity.OfferValidity = model.OfferValidity;
                entity.TenureofLc = model.TenureofLc;
                entity.CalculationofTenure = model.CalculationofTenure;
                entity.CreditDays = model.CreditDays;
                entity.ReImbursementCurrencyId = model.ReImbursementCurrencyId;
                entity.Charges = model.Charges;
                entity.CountryOfOriginId = model.CountryOfOriginId;
                entity.TransShipmentAllow = model.TransShipmentAllow;
                entity.ShippingTolerance = model.ShippingTolerance;
                entity.PortofLoadingID = model.PortofLoadingID;
                entity.PortofDischargeID = model.PortofDischargeID;
                entity.ShipmentModeId = model.ShipmentModeId;
                entity.QuotationRefNo = model.QuotationRefNo;
                entity.QuotationRefDate = model.QuotationRefDate;
                entity.UnApprove = false;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                //Main part of Revision after Reject
                entity.RevisionNo = entity.RevisionNo + 1;
                entity.IsRevision = true;
                entity.Proposed = true;
                entity.Approved = false;
                entity.UnApprove = false;
                entity.UnApproveBy = 0;
                entity.UnApproveDate = null;
                entity.UnapproveReason = "";
                entity.EntityState = EntityState.Modified;
                //end Main part of Revision after Reject

                //if (model.IsRevision)
                //{
                //    entity.Approved = false;
                //    entity.ApprovedBy = 0;
                //    entity.ApprovedDate = null;
                //    entity.RevisionNo = entity.RevisionNo + 1;
                //    entity.IsRevision = true;
                //    entity.RevisionBy = AppUser.EmployeeCode;
                //    entity.RevisionDate = DateTime.Now;
                //    entity.RevisionReason = model.RevisionReason;
                //}
                //else if (model.IsCancel)
                //{
                //    entity.IsCancel = true;
                //    entity.CancelBy = AppUser.EmployeeCode;
                //    entity.CancelDate = DateTime.Now;
                //    entity.CancelReason = model.CancelReason;
                //}

                foreach (YarnPOChild item in entity.YarnPOChilds)
                {
                    item.YarnPOChildBuyers.SetUnchanged();
                    item.YarnPOChildOrders.SetUnchanged();
                    item.EntityState = EntityState.Unchanged;
                }

                foreach (YarnPOChild item in model.YarnPOChilds)
                {
                    YarnPOChild existingYarnPOChild = entity.YarnPOChilds.FirstOrDefault(x => x.YPOChildID == item.YPOChildID);
                    if (existingYarnPOChild != null)
                    {
                        existingYarnPOChild.PRMasterID = item.PRMasterID;
                        existingYarnPOChild.PRChildID = item.PRChildID;
                        existingYarnPOChild.ItemMasterID = item.ItemMasterID;
                        existingYarnPOChild.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        existingYarnPOChild.UnitID = item.UnitID;
                        existingYarnPOChild.BuyerID = item.BuyerID;
                        existingYarnPOChild.PoQty = item.PoQty;
                        existingYarnPOChild.Rate = item.Rate;
                        existingYarnPOChild.PIValue = item.PIValue;
                        existingYarnPOChild.Remarks = item.Remarks;
                        existingYarnPOChild.YarnLotNo = item.YarnLotNo;
                        existingYarnPOChild.HSCode = item.HSCode;
                        existingYarnPOChild.QuotationRefNo = item.QuotationRefNo;
                        existingYarnPOChild.QuotationRefDate = item.QuotationRefDate;
                        existingYarnPOChild.BookingNo = item.BookingNo;
                        existingYarnPOChild.YarnShade = item.Segment4ValueDesc;
                        existingYarnPOChild.YarnProgramId = item.YarnProgramId;
                        existingYarnPOChild.NoOfThread = item.NoOfThread;
                        existingYarnPOChild.PoForId = item.PoForId;
                        //existingYarnPOChild.HSCode = item.HSCode;
                        existingYarnPOChild.ConceptID = item.ConceptID;
                        existingYarnPOChild.ReceivedCompleted = item.ReceivedCompleted;
                        existingYarnPOChild.ReceivedDate = DateTime.Now;
                        existingYarnPOChild.DayValidDurationId = item.DayValidDurationId;
                        existingYarnPOChild.EntityState = EntityState.Modified;

                        #region Process YarnPOChildBuyer

                        item.YarnChildPoBuyerIds = item.YarnChildPoBuyerIds ?? "";
                        var buyerIdArray = Array.ConvertAll(item.YarnChildPoBuyerIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), int.Parse).Where(x => x > 0); ;
                        foreach (var buyerId in buyerIdArray)
                        {
                            YarnPOChildBuyer yarnPOChildBuyerEntity = existingYarnPOChild.YarnPOChildBuyers.FirstOrDefault(x => x.BuyerId == buyerId);
                            if (yarnPOChildBuyerEntity == null)
                            {
                                yarnPOChildBuyerEntity = new YarnPOChildBuyer
                                {
                                    BuyerId = buyerId,
                                    YPOMasterID = entity.YPOMasterID,
                                    YPOChildID = existingYarnPOChild.YPOChildID
                                };
                                existingYarnPOChild.YarnPOChildBuyers.Add(yarnPOChildBuyerEntity);
                            }
                        }

                        foreach (YarnPOChildBuyer buyer in existingYarnPOChild.YarnPOChildBuyers.Where(x => !buyerIdArray.Contains(x.BuyerId)))
                            buyer.EntityState = EntityState.Deleted;

                        #endregion Process YarnPOChildBuyer

                        #region Process YarnPOChildOrder

                        item.YarnChildPoExportIds = item.YarnChildPoExportIds ?? "";
                        var exportOrderIdArray = Array.ConvertAll(item.YarnChildPoExportIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), int.Parse).Where(x => x > 0); ;
                        foreach (var exportOrderId in exportOrderIdArray)
                        {
                            YarnPOChildOrder yarnPOChildOrderEntity = existingYarnPOChild.YarnPOChildOrders.FirstOrDefault(x => x.ExportOrderId == exportOrderId);
                            if (yarnPOChildOrderEntity == null)
                            {
                                yarnPOChildOrderEntity = new YarnPOChildOrder
                                {
                                    ExportOrderId = exportOrderId,
                                    YPOMasterID = entity.YPOMasterID,
                                    YPOChildID = existingYarnPOChild.YPOChildID
                                };
                                existingYarnPOChild.YarnPOChildOrders.Add(yarnPOChildOrderEntity);
                            }
                        }

                        foreach (YarnPOChildOrder exportOrder in existingYarnPOChild.YarnPOChildOrders.Where(x => !exportOrderIdArray.Contains(x.ExportOrderId)))
                            exportOrder.EntityState = EntityState.Deleted;

                        #endregion Process YarnPOChildOrder
                    }
                    else
                    {
                        item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                    }
                }

                // Remove deleted childs
                var deletedChilds = entity.YarnPOChilds.ToList().Where(x => !model.YarnPOChilds.Select(cm => cm.YPOChildID).Contains(x.YPOChildID));
                foreach (YarnPOChild item in deletedChilds)
                {
                    item.YarnPOChildBuyers.SetDeleted();
                    item.YarnPOChildOrders.SetDeleted();
                    item.EntityState = EntityState.Deleted;
                }
            }
            //End Revision After Reject

            await _yarnPOService.SaveAsyncRevision(entity, model.YarnPOChilds.ToList(), entity.PoNo, AppUser.EmployeeCode);
            return Ok(entity.PoNo);
            //await _yarnPOService.UpdateEntityAsync(entity);
            //return Ok();
        }
    }
}