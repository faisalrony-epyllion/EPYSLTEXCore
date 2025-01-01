using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
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
    [Route("api/cda-loan-receive")]
    public class CDALoanReceiveController : ApiBaseController
    {
        private readonly ICDALoanReceiveService _service;
        private readonly IItemMasterService<CDALoanReceiveChild> _itemMasterService;
        public CDALoanReceiveController(IUserService userService, ICDALoanReceiveService service, IItemMasterService<CDALoanReceiveChild> itemMasterRepository) : base(userService)
        {
            _service = service;
            _itemMasterService = itemMasterRepository;
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
            CDALoanReceiveMaster record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> Save(CDALoanReceiveMaster model)
        {
            // Set Item master Id.
            List<CDALoanReceiveChild> childRecords = model.Childs;
            _itemMasterService.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            CDALoanReceiveMaster entity;
            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.CDALReceiveMasterID);

                entity.TransportMode = model.TransportMode;
                entity.LReceiveDate = model.LReceiveDate; 
                entity.LocationID = model.LocationID;
                entity.CompanyID = model.CompanyID;
                entity.RCompanyID = model.RCompanyID;
                entity.LoanProviderID = model.LoanProviderID;
                entity.ChallanNo = model.ChallanNo;
                entity.ChallanDate = model.ChallanDate;
                entity.MChallanNo = model.MChallanNo;
                entity.MChallanDate = model.MChallanDate;
                entity.GPNo = model.GPNo;
                entity.GPDate = model.GPDate;
                entity.TransportTypeID = model.TransportTypeID;
                entity.VehicleNo = model.VehicleNo;
                entity.Remarks = model.Remarks;
                entity.InspSend = model.InspSend;
                entity.InspSendDate = DateTime.Now; 
                entity.ACompanyInvoice = model.ACompanyInvoice;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;  
                entity.EntityState = EntityState.Modified; 

                entity.Childs.SetUnchanged();

                foreach (CDALoanReceiveChild item in childRecords)
                {
                    CDALoanReceiveChild child = entity.Childs.FirstOrDefault(x => x.CDALReceiveChildID == item.CDALReceiveChildID);

                    if (child == null)
                    {
                        child = item;
                        child.ItemMasterID = item.ItemMasterID;
                        child.EntityState = EntityState.Added;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.BatchNo = item.BatchNo;
                        child.ExpiryDate = item.ExpiryDate;
                        child.Rate = item.Rate;
                        child.ChallanQty = item.ChallanQty; 
                        child.ReceiveQty = item.ReceiveQty; 
                        child.Remarks = item.Remarks;
                        child.ItemMasterID = item.ItemMasterID; 
                        child.EntityState = EntityState.Modified;
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
                    child.ItemMasterID = childRecords.Find(x => x.CDALReceiveChildID == child.CDALReceiveChildID).ItemMasterID;
                }
            }

            await _service.SaveAsync(entity);

            return Ok();
        }

        [HttpPost]
        [Route("acknowledge/{id}")]
        public async Task<IActionResult> Acknowledge(int id)
        {
            CDALoanReceiveMaster entity = await _service.GetAllAsync(id); 
            entity.InspAck = true;
            entity.InspAckDate = DateTime.Now; 
            entity.InspAckBy = AppUser.UserCode;
            entity.EntityState = EntityState.Modified; 
            await _service.UpdateEntityAsync(entity); 
            return Ok();
        }

    }
}