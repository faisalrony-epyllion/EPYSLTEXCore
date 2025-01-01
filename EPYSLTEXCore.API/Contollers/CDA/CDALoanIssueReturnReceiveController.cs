using AutoMapper;
using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Entities;
using EPYSLTEX.Core.Entities.Tex;
using EPYSLTEX.Core.GuardClauses;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEX.Infrastructure.Data.Repositories;
using EPYSLTEX.Infrastrucure.Entities;
using EPYSLTEX.Web.Extends.Filters;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEX.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace EPYSLTEX.Web.Controllers.Apis.CDA
{
    [AuthorizeJwt]
    [RoutePrefix("api/cda-loan-issue-rr")]
    public class CDALoanIssueReturnReceiveController : ApiBaseController
    {
        private readonly ICDALoanIssueReturnReceiveService _service;
        private readonly ItemMasterRepository<CDALoanIssueReturnReceiveChild> _itemMasterRepository;
        public CDALoanIssueReturnReceiveController(ICDALoanIssueReturnReceiveService service, 
            ItemMasterRepository<CDALoanIssueReturnReceiveChild> itemMasterRepository)
        {
            _service = service;
            _itemMasterRepository = itemMasterRepository;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IHttpActionResult> GetList(Status status)
        { 
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new")]
        public async Task<IHttpActionResult> GetNew()
        {
            return Ok(await _service.GetNewAsync()); 
        } 
        
        [Route("{id}")]
        [HttpGet]
        public async Task<IHttpActionResult> Get(int id)
        {
            CDALoanIssueReturnReceiveMaster record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IHttpActionResult> Save(CDALoanIssueReturnReceiveMaster model)
        {
            // Set Item master Id.
            List<CDALoanIssueReturnReceiveChild> childRecords = model.Childs;
            _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

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
                entity.UpdatedBy = UserId; 
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
                entity.AddedBy = UserId;
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
        public async Task<IHttpActionResult> Acknowledgment(int id)
        {
            CDALoanIssueReturnReceiveMaster entity = await _service.GetAllAsync(id);
            entity.InspAck = true;
            entity.InspAckDate = DateTime.Now;
            entity.InspAckBy = UserId;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        } 
    }
}