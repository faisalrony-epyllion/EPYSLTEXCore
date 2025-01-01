using EPYSLTEX.Core.Entities.Tex;
using EPYSLTEX.Core.GuardClauses;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEX.Infrastructure.Data.Repositories;
using EPYSLTEX.Web.Extends.Filters;
using EPYSLTEX.Web.Models;
using Newtonsoft.Json;
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
    [RoutePrefix("api/cda-indent")]
    public class CDAIndentController : ApiBaseController
    {
        private readonly ICDAIndentService _service;
        private readonly ItemMasterRepository<CDAIndentChild> _itemMasterRepository;

        public CDAIndentController(ICDAIndentService service, ItemMasterRepository<CDAIndentChild> itemMasterRepository)
        {
            _service = service;
            _itemMasterRepository = itemMasterRepository;
        }

        [Route("cda-dyes-chemical")]
        public async Task<IHttpActionResult> GetDyesChemicalLists()
        {
            return Ok(await _service.GetDyesChemicalsAsync());
        }

        [Route("list")]
        public async Task<IHttpActionResult> GetList(Status status, string pageName)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<CDAIndentMaster> records = await _service.GetPagedAsync(status, pageName, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{SubGroupName}")]
        public async Task<IHttpActionResult> GetNew(string SubGroupName)
        {
            var obj = await _service.GetNewAsync(SubGroupName);
            obj.CIndentBy = UserId;
            return Ok(obj);
        }

        [Route("{id}/{SubGroupName}")]
        [HttpGet]
        public async Task<IHttpActionResult> Get(int id, string SubGroupName)
        {
            var record = await _service.GetAsync(id, SubGroupName);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        public async Task<IHttpActionResult> Save(CDAIndentMaster model)
        {
            //_itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW,ref childs);
            //Set ItemMasterID

            List<CDAIndentChild> childs = model.Childs;
            CDAIndentMaster entity;

            if (!model.IsModified)
            {
                entity = CommonFunction.DeepClone(model);
                entity.AddedBy = UserId;
                entity.DateAdded = DateTime.Now;

                if (model.IsSendForApprove)
                {
                    entity.SendForApproval = true;
                    entity.SendForApproveBy = UserId;
                    entity.SendForApproveDate = DateTime.Now;
                    entity.SendForCheck = true;
                    entity.SendForCheckBy = UserId;
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
                        entity.UpdatedBy = UserId;
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
                    entity.SendForApproveBy = UserId;
                    entity.SendForApproveDate = DateTime.Now;
                    entity.SendForCheck = true;
                    entity.SendForCheckBy = UserId;
                    entity.SendForCheckDate = DateTime.Now;
                }
                else if (model.IsApporve)
                {
                    entity.Approve = true;
                    entity.ApproveBy = UserId;
                    entity.ApproveDate = DateTime.Now;

                    entity.SendForAcknowledge = true;
                    entity.SendForAcknowledgeBy = UserId;
                    entity.SendForAcknowledgeDate = DateTime.Now;
                }
                else if (model.IsAck)
                {
                    entity.Acknowledge = true;
                    entity.AcknowledgeBy = UserId;
                    entity.AcknowledgeDate = DateTime.Now;
                }
                else if (model.IsTexAck)
                {
                    entity.TexAcknowledge = true;
                    entity.TexAcknowledgeBy = UserId;
                    entity.AcknowledgeDate = DateTime.Now;
                }
                else if (model.IsCheck)
                {
                    entity.IsCheck = true;
                    entity.CheckBy = UserId;
                    entity.CheckDate = DateTime.Now;
                }
            }
            await _service.SaveAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("approve/{id}")]
        public async Task<IHttpActionResult> Approve(int id)
        {
            CDAIndentMaster entity = await _service.GetAllAsync(id);
            entity.Approve = true;
            entity.ApproveBy = UserId;
            entity.ApproveDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("acknowledge/{id}")]
        public async Task<IHttpActionResult> Acknowledge(int id)
        {
            CDAIndentMaster entity = await _service.GetAllAsync(id);
            entity.Acknowledge = true;
            entity.AcknowledgeBy = UserId;
            entity.AcknowledgeDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("texAcknowledge/{id}")]
        public async Task<IHttpActionResult> TexAcknowledge(int id)
        {
            CDAIndentMaster entity = await _service.GetAllAsync(id);
            entity.TexAcknowledge = true;
            entity.TexAcknowledgeBy = UserId;
            entity.TexAcknowledgeDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }

        [HttpPost]
        [Route("reject/{id}/{reason}")]
        public async Task<IHttpActionResult> Reject(int id, string reason)
        {
            CDAIndentMaster entity = await _service.GetAllAsync(id);
            entity.Reject = true;
            entity.RejectDate = DateTime.Now;
            entity.RejectReason = reason;
            entity.RejectBy = UserId;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
    }
}