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
    [Route("api/cda-loan-issue")]
    public class CDALoanIssueController : ApiBaseController
    {
        private readonly ICDALoanIssueService _service;
        private readonly IItemMasterService<CDALoanIssueChild> _itemMasterService;
        public CDALoanIssueController(IUserService userService, ICDALoanIssueService service,
            IItemMasterService<CDALoanIssueChild> itemMasterService) : base(userService)
        {
            _service = service;
            _itemMasterService = itemMasterService;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, string Flag)
        { 
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetPagedAsync(status, paginationInfo, Flag);
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
            CDALoanIssueMaster record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> Save(CDALoanIssueMaster model)
        {
            // Set Item master Id.
            List<CDALoanIssueChild> childRecords = model.Childs;
            _itemMasterService.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            CDALoanIssueMaster entity; 
            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.CDALIssueMasterID);

                if (model.GPFlag)
                {
                    entity.GPDate = model.GPDate; 
                    entity.GPSendForApproval = model.GPSendForApproval;
                    entity.GPPrepareBy = AppUser.UserCode;
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
                    entity.DCPrepareBy = AppUser.UserCode;
                    entity.DCPrepareDate = DateTime.Now;  
                }
                entity.UpdatedBy = AppUser.UserCode;
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
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;
                entity.DCSendForApproval = model.DCSendForApproval;
                entity.DCPrepareBy = AppUser.UserCode;
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
        public async Task<IActionResult> DCApproval(int id)
        {
            CDALoanIssueMaster entity = await _service.GetAllAsync(id); 
            entity.DCApprove = true;
            entity.DCApproveDate = DateTime.Now; 
            entity.DCApproveBy = AppUser.UserCode;
            entity.EntityState = EntityState.Modified; 
            await _service.UpdateEntityAsync(entity); 
            return Ok();
        }
        [HttpPost]
        [Route("gpapproval/{id}")]
        public async Task<IActionResult> GPApproval(int id)
        { 
            CDALoanIssueMaster entity = await _service.GetAllAsync(id);
            entity.GPApprove = true;
            entity.GPApproveDate = DateTime.Now;
            entity.GPApproveBy = AppUser.UserCode;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
        [HttpPost]
        [Route("chkapproval/{id}")]
        public async Task<IActionResult> CheckApproval(int id)
        { 
            CDALoanIssueMaster entity = await _service.GetAllAsync(id);
            entity.DCCheckOut = true;
            entity.DCCheckOutDate = DateTime.Now;
            entity.DCCheckOutBy = AppUser.UserCode;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
    }
}