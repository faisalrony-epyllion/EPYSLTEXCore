using AutoMapper;
using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Entities.Tex;
using EPYSLTEX.Core.GuardClauses;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEX.Infrastructure.Data.Repositories;
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
    [RoutePrefix("api/cda-loan-receive")]
    public class CDALoanReceiveController : ApiBaseController
    {
        private readonly ICDALoanReceiveService _service;
        private readonly ItemMasterRepository<CDALoanReceiveChild> _itemMasterRepository;
        public CDALoanReceiveController(ICDALoanReceiveService service, ItemMasterRepository<CDALoanReceiveChild> itemMasterRepository)
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
            CDALoanReceiveMaster record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IHttpActionResult> Save(CDALoanReceiveMaster model)
        {
            // Set Item master Id.
            List<CDALoanReceiveChild> childRecords = model.Childs;
            _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

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
                entity.UpdatedBy = UserId;
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
                entity.AddedBy = UserId;
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
        public async Task<IHttpActionResult> Acknowledge(int id)
        {
            CDALoanReceiveMaster entity = await _service.GetAllAsync(id); 
            entity.InspAck = true;
            entity.InspAckDate = DateTime.Now; 
            entity.InspAckBy = UserId;
            entity.EntityState = EntityState.Modified; 
            await _service.UpdateEntityAsync(entity); 
            return Ok();
        }

    }
}