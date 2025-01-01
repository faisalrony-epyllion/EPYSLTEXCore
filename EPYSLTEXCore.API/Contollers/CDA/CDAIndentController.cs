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
    [Route("api/cda-indent")]
    public class CDAIndentController : ApiBaseController
    {
        private readonly ICDAIndentService _service;
        private readonly IItemMasterService<CDAIndentChild> _itemMasterService;

        public CDAIndentController(IUserService userService, ICDAIndentService service, IItemMasterService<CDAIndentChild> itemMasterRepository) : base(userService)
        {
            _service = service;
            _itemMasterService = itemMasterRepository;
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
            List<CDAIndentMaster> records = await _service.GetPagedAsync(status, pageName, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{SubGroupName}")]
        public async Task<IActionResult> GetNew(string SubGroupName)
        {
            var obj = await _service.GetNewAsync(SubGroupName);
            obj.CIndentBy = AppUser.UserCode;
            return Ok(obj);
        }

        [Route("{id}/{SubGroupName}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, string SubGroupName)
        {
            var record = await _service.GetAsync(id, SubGroupName);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(CDAIndentMaster model)
        {
            //_itemMasterService.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW,ref childs);
            //Set ItemMasterID

            List<CDAIndentChild> childs = model.Childs;
            CDAIndentMaster entity;

            if (!model.IsModified)
            {
                entity = CommonFunction.DeepClone(model);
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;

                if (model.IsSendForApprove)
                {
                    entity.SendForApproval = true;
                    entity.SendForApproveBy = AppUser.UserCode;
                    entity.SendForApproveDate = DateTime.Now;
                    entity.SendForCheck = true;
                    entity.SendForCheckBy = AppUser.UserCode;
                    entity.SendForCheckDate = DateTime.Now;
                }

                entity.Childs = new List<CDAIndentChild>();
                foreach (CDAIndentChild mChild in model.Childs)
                {
                    mChild.EntityState = EntityState.Added;
                    var mChildCompanys = CommonFunction.DeepClone(mChild.CDAIndentCompanies);
                    mChild.CDAIndentCompanies = new List<CDAIndentChildCompany>();
                    foreach (CDAIndentChildCompany mChildCompany in mChildCompanys)
                    {
                        mChildCompany.EntityState = EntityState.Added;
                        mChild.CDAIndentCompanies.Add(mChildCompany);
                    }
                    entity.Childs.Add(mChild);
                }
            }
            else
            {
                entity = await _service.GetAllAsync(model.CDAIndentMasterID);
                entity.TriggerPointID = 1252;
                entity.EntityState = EntityState.Modified;

                if (model.IsTexAck) //Ignore Childs
                {
                    entity.Childs = new List<CDAIndentChild>();
                }
                else  if ((!model.IsApporve || model.IsApporve) && (!model.IsTexAck))
                {
                    entity.Childs.SetUnchanged();
                    foreach (CDAIndentChild child in entity.Childs)
                    {
                        child.CDAIndentCompanies.SetUnchanged();
                        child.ChildItems.SetUnchanged();
                    }
                    if (!model.IsAck)
                    {
                        entity.UpdatedBy = AppUser.UserCode;
                        entity.DateUpdated = DateTime.Now;

                        entity.IsCIndent = true;
                        entity.CIndentBy = model.CIndentBy;
                        entity.IndentDate = model.IndentDate;

                        entity.Remarks = model.Remarks;
                        entity.IndentStartMonth = model.IndentStartMonth;
                        entity.IndentEndMonth = model.IndentEndMonth;
                    }

                    foreach (CDAIndentChild mChild in model.Childs)
                    {
                        var dbChild = entity.Childs.FirstOrDefault(x => x.CDAIndentChildID == mChild.CDAIndentChildID);
                        if (dbChild == null)
                        {
                            dbChild = CommonFunction.DeepClone(mChild);
                            dbChild.EntityState = EntityState.Added;

                            foreach (CDAIndentChildCompany iCompany in mChild.CDAIndentCompanies)
                            {
                                iCompany.EntityState = EntityState.Added;
                                dbChild.CDAIndentCompanies.Add(iCompany);
                            }
                            entity.Childs.Add(dbChild);
                        }
                        else
                        {
                            dbChild.ItemMasterID = mChild.ItemMasterID;
                            dbChild.UnitID = mChild.UnitID;
                            dbChild.IndentQty = mChild.IndentQty;
                            dbChild.CheckQty = mChild.CheckQty;
                            dbChild.ApprovQty = mChild.ApprovQty;
                            dbChild.ReqQty = mChild.ReqQty;
                            dbChild.Remarks = mChild.Remarks;

                            dbChild.HSCode = mChild.HSCode;
                            dbChild.CompanyID = mChild.CompanyID;

                            dbChild.EntityState = EntityState.Modified;

                            CDAIndentChildDetails childDetails;

                            foreach (CDAIndentChildCompany mChildCompany in mChild.CDAIndentCompanies)
                            {
                                var dbChildCompany = dbChild.CDAIndentCompanies.FirstOrDefault(x => x.ItemMasterID == mChildCompany.ItemMasterID);
                                if (dbChildCompany == null)
                                {
                                    dbChildCompany = CommonFunction.DeepClone(mChildCompany);
                                    dbChildCompany.EntityState = EntityState.Added;
                                    dbChild.CDAIndentCompanies.Add(dbChildCompany);
                                }
                                else
                                {
                                    dbChildCompany.CompanyID = mChildCompany.CompanyID;
                                    dbChildCompany.ItemMasterID = mChildCompany.ItemMasterID;
                                    dbChildCompany.UnitID = mChildCompany.UnitID;
                                    dbChildCompany.IndentQty = mChildCompany.IndentQty;
                                    dbChildCompany.ReqQty = mChildCompany.ReqQty;
                                    dbChildCompany.EntityState = EntityState.Modified;
                                }
                            }

                            foreach (CDAIndentChildDetails childDetail in mChild.ChildItems)
                            {
                                childDetails = dbChild.ChildItems.FirstOrDefault(x => x.CDAIndentChildDetailsID == childDetail.CDAIndentChildDetailsID);
                                if (childDetails == null)
                                {
                                    childDetail.CDAIndentMasterID = entity.CDAIndentMasterID;
                                    childDetail.CDAIndentChildID = mChild.CDAIndentChildID;
                                    dbChild.ChildItems.Add(childDetail);
                                }
                                else
                                {
                                    childDetails.BookingDate = childDetail.BookingDate;
                                    childDetails.IndentQty = childDetail.IndentQty;
                                    childDetails.CheckQty = childDetail.CheckQty;
                                    childDetails.ApprovQty = childDetail.ApprovQty;
                                    childDetails.DetailsQTY = childDetail.DetailsQTY;
                                    childDetails.EntityState = EntityState.Modified;
                                }
                            }
                        }
                    }
                    foreach (CDAIndentChild dbChild in entity.Childs)
                    {
                        if (dbChild.EntityState == EntityState.Unchanged) dbChild.EntityState = EntityState.Deleted;
                        foreach (CDAIndentChildDetails mChildItems in dbChild.ChildItems)
                        {
                            if (mChildItems.EntityState == EntityState.Unchanged) mChildItems.EntityState = EntityState.Deleted;
                        }
                        foreach (CDAIndentChildCompany mChildCompany in dbChild.CDAIndentCompanies)
                        {
                            if (mChildCompany.EntityState == EntityState.Unchanged) mChildCompany.EntityState = EntityState.Deleted;
                        }
                    }
                }

                if (model.IsSendForApprove)
                {
                    entity.SendForApproval = true;
                    entity.SendForApproveBy = AppUser.UserCode;
                    entity.SendForApproveDate = DateTime.Now;
                    entity.SendForCheck = true;
                    entity.SendForCheckBy = AppUser.UserCode;
                    entity.SendForCheckDate = DateTime.Now;
                }
                else if (model.IsApporve)
                {
                    entity.Approve = true;
                    entity.ApproveBy = AppUser.UserCode;
                    entity.ApproveDate = DateTime.Now;

                    entity.SendForAcknowledge = true;
                    entity.SendForAcknowledgeBy = AppUser.UserCode;
                    entity.SendForAcknowledgeDate = DateTime.Now;
                }
                else if (model.IsAck)
                {
                    entity.Acknowledge = true;
                    entity.AcknowledgeBy = AppUser.UserCode;
                    entity.AcknowledgeDate = DateTime.Now;
                }
                else if (model.IsTexAck)
                {
                    entity.TexAcknowledge = true;
                    entity.TexAcknowledgeBy = AppUser.UserCode;
                    entity.AcknowledgeDate = DateTime.Now;
                }
                else if (model.IsCheck)
                {
                    entity.IsCheck = true;
                    entity.CheckBy = AppUser.UserCode;
                    entity.CheckDate = DateTime.Now;
                }
            }
            await _service.SaveAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            CDAIndentMaster entity = await _service.GetAllAsync(id);
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
            CDAIndentMaster entity = await _service.GetAllAsync(id);
            entity.Acknowledge = true;
            entity.AcknowledgeBy = AppUser.UserCode;
            entity.AcknowledgeDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("texAcknowledge/{id}")]
        public async Task<IActionResult> TexAcknowledge(int id)
        {
            CDAIndentMaster entity = await _service.GetAllAsync(id);
            entity.TexAcknowledge = true;
            entity.TexAcknowledgeBy = AppUser.UserCode;
            entity.TexAcknowledgeDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("reject/{id}/{reason}")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            CDAIndentMaster entity = await _service.GetAllAsync(id);
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