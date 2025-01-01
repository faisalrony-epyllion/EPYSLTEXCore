using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Services;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.CDA;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
namespace EPYSLTEX.Web.Controllers.Apis.CDA
{
    [Authorize]
    [Route("api/cda-loan-issue-rr")]
    public class CDALoanIssueReturnReceiveController : ApiBaseController
    {
        private readonly ICDALoanIssueReturnReceiveService _service;
        private readonly IItemMasterService<CDALoanIssueReturnReceiveChild> _itemMasterService;
        public CDALoanIssueReturnReceiveController(IUserService userService, ICDALoanIssueReturnReceiveService service,
            IItemMasterService<CDALoanIssueReturnReceiveChild> itemMasterService) : base(userService)
        {
            _service = service;
            _itemMasterService = itemMasterService;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        { 
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new")]
        public async Task<IActionResult> GetNew()
        {
            return Ok(await _service.GetNewAsync()); 
        } 
        
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            CDALoanIssueReturnReceiveMaster record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> Save(CDALoanIssueReturnReceiveMaster model)
        {
            // Set Item master Id.
            List<CDALoanIssueReturnReceiveChild> childRecords = model.Childs;
            _itemMasterService.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            CDALoanIssueReturnReceiveMaster entity;
            List<CDALoanIssueReturnReceiveChildAdjutment> cAdjRecords = new List<CDALoanIssueReturnReceiveChildAdjutment>();

            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.CDALIssueReturnID); 
                entity.LIReturnDate = model.LIReturnDate; 
                entity.LocationID = model.LocationID;
                entity.CompanyID = model.CompanyID;
                entity.RCompanyID = model.RCompanyID;
                entity.LoanProviderID = model.LoanProviderID; 
                entity.ChallanDate = model.ChallanDate;
                entity.MChallanNo = model.MChallanNo;
                entity.MChallanDate = model.MChallanDate;
                entity.GPDate = model.GPDate;              
                entity.TransportMode = model.TransportMode;
                entity.TransportTypeID = model.TransportTypeID;
                entity.CContractorID = model.CContractorID;
                entity.VehicleNo = model.VehicleNo;
                entity.Remarks = model.Remarks;
                entity.RTypeID = model.RTypeID;
                entity.ITStatusID = model.ITStatusID;
                entity.InspSend = model.InspSend;
                entity.InspSendDate = DateTime.Now;
                entity.ACompanyInvoice = model.ACompanyInvoice;
                entity.PDID = model.PDID; 
                entity.UpdatedBy = AppUser.UserCode; 
                entity.DateUpdated = DateTime.Now; 
                entity.EntityState = EntityState.Modified; 

                entity.Childs.SetUnchanged(); 
                foreach (CDALoanIssueReturnReceiveChild child in entity.Childs)
                {
                    child.ChildAdjutment.SetUnchanged(); 
                }

                foreach (CDALoanIssueReturnReceiveChild item in childRecords)
                {
                    CDALoanIssueReturnReceiveChild child = entity.Childs.FirstOrDefault(x => x.CDALIssueReturnChildID == item.CDALIssueReturnChildID);

                    if (child == null)
                    {
                        child = item; 
                        child.EntityState = EntityState.Added;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.ItemMasterID = item.ItemMasterID;
                        child.BatchNo = item.BatchNo;
                        child.ExpiryDate = item.ExpiryDate;
                        child.ChallanQty = item.ChallanQty;
                        child.ReceiveQty = item.ReceiveQty;
                        child.Rate = item.Rate; 
                        child.Remarks = item.Remarks; 
                        child.EntityState = EntityState.Modified;

                        CDALoanIssueReturnReceiveChildAdjutment childDetails;
                        foreach (CDALoanIssueReturnReceiveChildAdjutment childDetail in item.ChildAdjutment)
                        {
                            childDetails = child.ChildAdjutment.FirstOrDefault(x => x.CDALIRRAdjID == childDetail.CDALIRRAdjID);
                            if (childDetails == null)
                            {
                                childDetail.CDALIssueReturnID = entity.CDALIssueReturnID;
                                childDetail.CDALIssueReturnChildID = item.CDALIssueReturnChildID;
                                child.ChildAdjutment.Add(childDetail); 
                            }
                            else
                            {
                                childDetails.ItemMasterID = childDetails.ItemMasterID;
                                childDetails.BatchNo = childDetails.BatchNo;
                                childDetails.ExpiryDate = childDetails.ExpiryDate;
                                childDetails.AdjustQty = childDetails.AdjustQty;
                                childDetails.Rate = childDetails.Rate;
                                childDetails.EntityState = EntityState.Modified; 
                            }
                        }
                    }
                }

                foreach (CDALoanIssueReturnReceiveChild dbChild in entity.Childs)
                {
                    if (dbChild.EntityState == EntityState.Unchanged) dbChild.EntityState = EntityState.Deleted;
                    foreach (CDALoanIssueReturnReceiveChildAdjutment mChildItems in dbChild.ChildAdjutment)
                    {
                        if (mChildItems.EntityState == EntityState.Unchanged) mChildItems.EntityState = EntityState.Deleted;
                    } 
                }
            }
            else
            { 
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;
                entity.InspSend = model.InspSend;
                entity.InspSendDate = DateTime.Now; 

                foreach (var child in entity.Childs)
                {
                    child.ItemMasterID = childRecords.Find(x => x.CDALIssueReturnChildID == child.CDALIssueReturnChildID).ItemMasterID;
                }
            }

            await _service.SaveAsync(entity);

            return Ok();
        }
        [HttpPost]
        [Route("acknowledgment/{id}")]
        public async Task<IActionResult> Acknowledgment(int id)
        {
            CDALoanIssueReturnReceiveMaster entity = await _service.GetAllAsync(id);
            entity.InspAck = true;
            entity.InspAckDate = DateTime.Now;
            entity.InspAckBy = AppUser.UserCode;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        } 
    }
}