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
    [RoutePrefix("api/cda-loan-issue")]
    public class CDALoanIssueController : ApiBaseController
    {
        private readonly ICDALoanIssueService _service;
        private readonly ItemMasterRepository<CDALoanIssueChild> _itemMasterRepository;
        public CDALoanIssueController(ICDALoanIssueService service, 
            ItemMasterRepository<CDALoanIssueChild> itemMasterRepository)
        {
            _service = service;
            _itemMasterRepository = itemMasterRepository;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IHttpActionResult> GetList(Status status, string Flag)
        { 
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(status, paginationInfo, Flag);
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
            CDALoanIssueMaster record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IHttpActionResult> Save(CDALoanIssueMaster model)
        {
            // Set Item master Id.
            List<CDALoanIssueChild> childRecords = model.Childs;
            _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            CDALoanIssueMaster entity; 
            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.CDALIssueMasterID);

                if (model.GPFlag)
                {
                    entity.GPDate = model.GPDate; 
                    entity.GPSendForApproval = model.GPSendForApproval;
                    entity.GPPrepareBy = UserId;
                    entity.GPPrepareDate = DateTime.Now;
                    entity.GPFlag = model.GPFlag;
                }
                else
                { 
                    entity.LIssueDate = model.LIssueDate; 
                    entity.LocationID = model.LocationID;
                    entity.CompanyID = model.CompanyID;
                    entity.IssueFromCompanyID = model.IssueFromCompanyID; 
                    entity.TransportMode = model.TransportMode;
                    entity.TransportTypeID = model.TransportTypeID;
                    entity.ChallanDate = model.ChallanDate; 
                    entity.VehichleID = model.VehichleID;
                    entity.VehichleNo = model.VehichleNo;
                    entity.DriverName = model.DriverName;
                    entity.DriverContactNo = model.DriverContactNo;
                    entity.LockNo = model.LockNo;
                    entity.Remarks = model.Remarks;
                    entity.EstimatedReleaseTime = model.EstimatedReleaseTime;
                    entity.MushakChallanNo = model.MushakChallanNo;
                    //DC
                    entity.DCSendForApproval = model.DCSendForApproval;
                    entity.DCPrepareBy = UserId;
                    entity.DCPrepareDate = DateTime.Now;  
                }
                entity.UpdatedBy = UserId;
                entity.DateUpdated = DateTime.Now;
                
                entity.EntityState = EntityState.Modified; 

                entity.Childs.SetUnchanged();  

                foreach (CDALoanIssueChild item in childRecords)
                {
                    CDALoanIssueChild child = entity.Childs.FirstOrDefault(x => x.CDALIssueChildID == item.CDALIssueChildID);

                    if (child == null)
                    {
                        child = item;
                        child.ItemMasterID = item.ItemMasterID;
                        child.EntityState = EntityState.Added;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.ItemMasterID = item.ItemMasterID;
                        child.BatchNo = item.BatchNo;
                        child.ExpiryDate = item.ExpiryDate;
                        child.IssueQty = item.IssueQty;
                        child.Rate = item.Rate; 
                        child.Remarks = item.Remarks; 
                        child.EntityState = EntityState.Modified; 
                    }
                }

                foreach (CDALoanIssueChild dbChild in entity.Childs)
                {
                    if (dbChild.EntityState == EntityState.Unchanged) dbChild.EntityState = EntityState.Deleted; 
                }
            }
            else
            { 
                entity = model;
                entity.AddedBy = UserId;
                entity.DateAdded = DateTime.Now;
                entity.DCSendForApproval = model.DCSendForApproval;
                entity.DCPrepareBy = UserId;
                entity.DCPrepareDate = DateTime.Now; 

                foreach (var child in entity.Childs)
                {
                    child.ItemMasterID = childRecords.Find(x => x.CDALIssueChildID == child.CDALIssueChildID).ItemMasterID;
                }
            }

            await _service.SaveAsync(entity);

            return Ok();
        }

        [HttpPost]
        [Route("dcapproval/{id}")]
        public async Task<IHttpActionResult> DCApproval(int id)
        {
            CDALoanIssueMaster entity = await _service.GetAllAsync(id); 
            entity.DCApprove = true;
            entity.DCApproveDate = DateTime.Now; 
            entity.DCApproveBy = UserId;
            entity.EntityState = EntityState.Modified; 
            await _service.UpdateEntityAsync(entity); 
            return Ok();
        }
        [HttpPost]
        [Route("gpapproval/{id}")]
        public async Task<IHttpActionResult> GPApproval(int id)
        { 
            CDALoanIssueMaster entity = await _service.GetAllAsync(id);
            entity.GPApprove = true;
            entity.GPApproveDate = DateTime.Now;
            entity.GPApproveBy = UserId;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
        [HttpPost]
        [Route("chkapproval/{id}")]
        public async Task<IHttpActionResult> CheckApproval(int id)
        { 
            CDALoanIssueMaster entity = await _service.GetAllAsync(id);
            entity.DCCheckOut = true;
            entity.DCCheckOutDate = DateTime.Now;
            entity.DCCheckOutBy = UserId;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
    }
}