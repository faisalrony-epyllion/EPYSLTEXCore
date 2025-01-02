using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.CDA;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.CDA;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.CDA
{
    [Route("api/cda-pr")]
    public class CDAPurchaseRequisitionController : ApiBaseController
    {
        private readonly ICDAPRService _service;
        private readonly IItemMasterService<CDAPRChild> _itemMasterRepository;
        public CDAPurchaseRequisitionController(IUserService userService, ICDAPRService service, IItemMasterService<CDAPRChild> itemMasterRepository) : base(userService)
        {
            _service = service;
            _itemMasterRepository = itemMasterRepository;
        }

        [Route("cda-dyes-chemical")]
        public async Task<IActionResult> GetDyesChemicalLists()
        {
            return Ok(await _service.GetDyesChemicalsAsync());
        }

        [Route("list")]
        public async Task<IActionResult> GetList(Status status, string pageName)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<CDAPRMaster> records = await _service.GetPagedAsync(status, pageName, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("pendingIndentList")]
        public async Task<IActionResult> GetPendingIndentList(Status status, string pageName)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<CDAIndentChild> records = await _service.GetPendingIndentPagedAsync(status, pageName, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{SubGroupName}")]
        public async Task<IActionResult> GetNew(string SubGroupName)
        {
            return Ok(await _service.GetNewAsync(SubGroupName));
        }
        [Route("new/{SubGroupName}/{IDs}")]
        [HttpGet]
        public async Task<IActionResult> GetIndentNewAsync(string SubGroupName, string IDs)
        {
            return Ok(await _service.GetIndentNewAsync(SubGroupName, IDs));
        }

        [Route("{id}/{SubGroupName}")]
        [HttpGet]
        public async Task<IActionResult> GetPR(int id, string SubGroupName)
        {
            var record = await _service.GetAsync(id, SubGroupName);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }


        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(CDAPRMaster model)
        {
            #region Validation 
            var validator = new CDAPRMasterValidator(model.SubGroupName);
            var validationResult = validator.Validate(model);
            //if (!validationResult.IsValid)
            //{
            //    string messages = string.Join("<br>", validationResult.Errors.Select(x => x.PropertyName.Replace("Childs", "Row") + " : " + x.ErrorMessage));
            //    return BadRequest(messages);
            //}
            #endregion Validation

            List<CDAPRChild> childs = model.Childs;
            //_itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW,ref childs);

            CDAPRMaster entity;

            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.CDAPRMasterID);

                entity.CDAPRDate = model.CDAPRDate;
                entity.TriggerPointID = model.TriggerPointID;
                entity.CompanyID = model.CompanyID;
                entity.CDAPRRequiredDate = model.CDAPRRequiredDate;
                entity.CDAPRBy = model.CDAPRBy;
                entity.Remarks = model.Remarks;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;

                if (model.IsCheckReject)
                {
                    entity.IsCheckReject = true;
                    entity.CheckRejectBy = AppUser.UserCode;
                    entity.CheckRejectDate = DateTime.Now;
                    entity.CheckRejectReason = model.CheckRejectReason;

                    entity.IsCheck = false;
                    entity.CheckBy = 0;
                    entity.CheckDate = null;

                    entity.IsSendForCheck = false;
                    entity.SendForCheckBy = 0;
                    entity.SendForCheckDate = null;
                }
                else if (model.IsSendForCheck && !model.SendForApproval)
                {
                    entity.IsSendForCheck = true;
                    entity.SendForCheckBy = AppUser.UserCode;
                    entity.SendForCheckDate = DateTime.Now;

                    entity.IsCheckReject = false;
                    entity.CheckRejectBy = 0;
                    entity.CheckRejectDate = null;
                }
                else if (!model.IsSendForCheck && model.SendForApproval)
                {
                    entity.IsCheck = true;
                    entity.CheckBy = AppUser.UserCode;
                    entity.CheckDate = DateTime.Now;

                    entity.SendForApproval = true;
                    entity.SendForApproveBy = AppUser.UserCode;
                    entity.SendForApproveDate = DateTime.Now;

                    entity.IsCheckReject = false;
                    entity.CheckRejectBy = 0;
                    entity.CheckRejectDate = null;
                }
                else if (model.SendForApproval)
                {
                    entity.Approve = true;
                    entity.ApproveBy = AppUser.UserCode;
                    entity.ApproveDate = DateTime.Now;

                    entity.IsCheckReject = false;
                    entity.CheckRejectBy = 0;
                    entity.CheckRejectDate = null;
                }

                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();

                foreach (CDAPRChild child in entity.Childs)
                    child.CDAPRCompanies.SetUnchanged();

                foreach (CDAPRChild item in model.Childs)
                {
                    var itemMasterID = childs.Find(x => x.CDAPRChildID == item.CDAPRChildID).ItemMasterID;
                    CDAPRChild childEntity = entity.Childs.FirstOrDefault(x => x.CDAPRChildID == item.CDAPRChildID);

                    if (childEntity == null)
                    {
                        childEntity = item;
                        childEntity.ItemMasterID = itemMasterID;// item.ItemMasterID;
                        childEntity.FPRCompanyID = entity.CompanyID;
                        childEntity.EntityState = EntityState.Added;
                        entity.Childs.Add(childEntity);

                        CDAPRCompany companyEntity = childEntity.CDAPRCompanies.FirstOrDefault(x => x.CDAPRCompanyID == childEntity.CDAPRCompanies.First().CDAPRCompanyID);
                        if (companyEntity == null)
                        {
                            CDAPRCompany prCompany = new CDAPRCompany
                            {
                                CDAPRMasterID = entity.CDAPRMasterID,
                                CompanyID = entity.CompanyID
                            };
                            childEntity.CDAPRCompanies.Add(prCompany);
                        }
                        else
                        {
                            companyEntity.CDAPRMasterID = entity.CDAPRMasterID;
                            companyEntity.CompanyID = entity.CompanyID;
                            companyEntity.EntityState = EntityState.Modified;
                        }

                        //CDAPRCompany prCompany = new CDAPRCompany
                        //{
                        //    CDAPRMasterID = entity.CDAPRMasterID,
                        //    CompanyID = entity.CompanyID
                        //};
                        //childEntity.CDAPRCompanies.Add(prCompany);  
                    }
                    else
                    {
                        childEntity.ItemMasterID = item.ItemMasterID;
                        childEntity.UnitID = item.UnitID;
                        childEntity.ReqQty = item.ReqQty;
                        childEntity.SuggestedQty = item.SuggestedQty;
                        childEntity.Remarks = item.Remarks;
                        childEntity.FPRCompanyID = entity.CompanyID;
                        childEntity.EntityState = EntityState.Modified;
                        childEntity.CDAIndentChildID = item.CDAIndentChildID;
                        childEntity.HSCode = item.HSCode;

                        CDAPRCompany companyEntity = childEntity.CDAPRCompanies.FirstOrDefault(x => x.CDAPRCompanyID == childEntity.CDAPRCompanies.First().CDAPRCompanyID);
                        if (companyEntity == null)
                        {
                            CDAPRCompany prCompany = new CDAPRCompany
                            {
                                CDAPRMasterID = entity.CDAPRMasterID,
                                CompanyID = entity.CompanyID
                            };
                            childEntity.CDAPRCompanies.Add(prCompany);
                        }
                        else
                        {
                            companyEntity.CDAPRMasterID = entity.CDAPRMasterID;
                            companyEntity.CompanyID = entity.CompanyID;
                            companyEntity.EntityState = EntityState.Modified;
                        }
                    }
                }

                foreach (CDAPRChild item in entity.Childs.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                    item.CDAPRCompanies.SetDeleted();
                }

                entity.CompanyID = 0;
            }
            else
            {
                entity = model;

                foreach (CDAPRChild childEntity in entity.Childs)
                {
                    childEntity.EntityState = EntityState.Added;
                    //childEntity.ItemMasterID = childs.Find(x => x.CDAPRChildID == childEntity.CDAPRChildID).ItemMasterID;
                    childEntity.FPRCompanyID = entity.CompanyID;

                    CDAPRCompany prCompany = new CDAPRCompany
                    {
                        CDAPRMasterID = childEntity.CDAPRMasterID,
                        CompanyID = entity.CompanyID,
                        IsCPR = false
                    };
                    childEntity.CDAPRCompanies.Add(prCompany);
                }

                entity.CompanyID = 0;
                entity.AddedBy = AppUser.UserCode;
                entity.CDAPRBy = model.CDAPRBy;
                entity.SendForApproveBy = AppUser.UserCode;

                //foreach (CDAPRChild child in entity.Childs)
                //{
                //    child.ItemMasterID = childs.Find(x => x.CDAPRChildID == child.CDAPRChildID).ItemMasterID;
                //}
            }

            await _service.SaveAsync(entity);

            return Ok(entity.CDAPRNo);
        }

        [HttpPost]
        [Route("save-cpr")]
        public async Task<IActionResult> SaveCPR(CDAPRMaster model)
        {
            CDAPRMaster entity = await _service.GetAllAsync(model.CDAPRMasterID);
            entity.UpdatedBy = AppUser.UserCode;
            entity.DateUpdated = DateTime.Now;
            entity.IsCPR = true;
            entity.CPRBy = AppUser.UserCode;
            entity.CPRDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            entity.Childs.SetUnchanged();
            entity.Childs.ForEach(x => x.CDAPRCompanies.SetUnchanged());
            await _service.SaveCPRAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("save-fpr")]
        public async Task<IActionResult> SaveFPRAsync(CDAPRMaster model)
        {
            CDAPRMaster entity = await _service.GetAllAsync(model.CDAPRMasterID);
            entity.UpdatedBy = AppUser.UserCode;
            entity.DateUpdated = DateTime.Now;
            entity.IsFPR = true;
            entity.FPRBy = AppUser.UserCode;
            entity.FPRDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;

            entity.Childs.SetUnchanged();
            entity.Childs.ForEach(x => x.CDAPRCompanies.SetUnchanged());
            await _service.SaveAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            CDAPRMaster entity = await _service.GetAllAsync(id);
            entity.Approve = true;
            entity.ApproveBy = AppUser.UserCode;
            entity.ApproveDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
        [HttpPost]
        [Route("acknowledge/{id}")]
        public async Task<IActionResult> Acknowledge(int id)
        {
            CDAPRMaster entity = await _service.GetAllAsync(id);
            entity.Acknowledge = true;
            entity.AcknowledgeBy = AppUser.UserCode;
            entity.AcknowledgeDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("reject/{id}/{reason}")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            CDAPRMaster entity = await _service.GetAllAsync(id);
            entity.Reject = true;
            entity.RejectDate = DateTime.Now;
            entity.RejectReason = reason;
            entity.RejectBy = AppUser.UserCode;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
    }
}
