using AutoMapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;
using EPYSLTEXCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace EPYSLTEX.Web.Controllers.Apis
{
    [Authorize]
    [Route("yiipAPI")]
    public class YarnImportPaymentController : ApiBaseController
    {
        private readonly IYarnImportPaymentService _service;
        public YarnImportPaymentController(IUserService userService, IYarnImportPaymentService service):base(userService)
        {
            _service = service;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, bool isCDAPage)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<ImportInvoicePaymentMaster> records = await _service.GetPagedAsync(status, isCDAPage, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{BankRefNumber}/{CompanyId}/{SupplierId}/{isCDAPage}")]
        public async Task<IActionResult> GetNew(String BankRefNumber, int CompanyId, int SupplierId, bool isCDAPage)
        {
            return Ok(await _service.GetNewAsync(BankRefNumber, CompanyId, SupplierId, isCDAPage));
        }

        [Route("save")]
        [HttpPost]

        public async Task<IActionResult> Save(ImportInvoicePaymentMaster models)
        {
            bool isModified = models.Modify;
            bool IsCDA = models.IsCDA;
            int vIIPMasterID = models.IIPMasterID;

            ImportInvoicePaymentMaster entity = new ImportInvoicePaymentMaster();
            ImportInvoicePaymentMaster entityTemp = new ImportInvoicePaymentMaster();
            if (isModified)
            {
                entity = new ImportInvoicePaymentMaster();
                entityTemp = await _service.GetMultiDetailsAsync(vIIPMasterID.ToString(), IsCDA);
                entityTemp.EntityState = EntityState.Unchanged;
                entityTemp.IPChilds.SetUnchanged();
                entityTemp.IPDetails.SetUnchanged();

                ImportInvoicePaymentChild childEntity;
                ImportInvoicePaymentDetails detailEntity;
                if (entityTemp == null)
                {
                    entityTemp = new ImportInvoicePaymentMaster();
                    models.AddedBy = AppUser.UserCode;
                    models.DateAdded = DateTime.Now;
                    models.IsCDA = IsCDA;
                    entityTemp.EntityState = EntityState.Added;
                    models.IPChilds.ForEach(child =>
                    {
                        child.EntityState = EntityState.Added;
                    });
                    models.IPDetails.ForEach(oItem =>
                    {
                        oItem.EntityState = EntityState.Added;
                        oItem.IPDetailSub.ForEach(item =>
                        {
                            oItem.EntityState = EntityState.Added;
                        });
                    });
                    entityTemp = models;
                }
                else
                {
                    entityTemp.IsCDA = IsCDA;
                    entityTemp.UpdatedBy = AppUser.UserCode;
                    entityTemp.DateUpdated = DateTime.Now;
                    entityTemp.EntityState = EntityState.Modified;
                    entityTemp.PaymentDate = models.PaymentDate;
                    entityTemp.BeneficiaryID = models.SupplierID;

                    foreach (ImportInvoicePaymentChild oItem in models.IPChilds)
                    {
                        childEntity = entityTemp.IPChilds.FirstOrDefault(c => c.IIPChildID == oItem.IIPChildID);
                        if (childEntity == null)
                        {
                            childEntity = oItem;
                            childEntity.EntityState = EntityState.Added;
                            entityTemp.IPChilds.Add(childEntity);
                        }
                        else
                        {
                            childEntity.IIPChildID = oItem.IIPChildID;
                            childEntity.InvoiceNo = oItem.InvoiceNo;
                            childEntity.InvoiceDate = oItem.InvoiceDate;
                            childEntity.InvoiceValue = oItem.InvoiceValue;
                            childEntity.PaymentValue = oItem.PaymentValue;
                            childEntity.PaymentedValue = oItem.PaymentedValue;
                            childEntity.BalanceAmount = oItem.BalanceAmount;
                            childEntity.EntityState = EntityState.Modified;
                        }
                    }

                    foreach (ImportInvoicePaymentDetails oItem in models.IPDetails)
                    {
                        foreach (ImportInvoicePaymentDetails itemChild in oItem.IPDetailSub)
                        {
                            detailEntity = entityTemp.IPDetails.FirstOrDefault(c => c.IIPDetailsID == itemChild.IIPDetailsID);
                            if (detailEntity == null)
                            {
                                detailEntity = itemChild;
                                detailEntity.EntityState = EntityState.Added;
                                detailEntity.SGHeadID = oItem.SGHeadID;
                                detailEntity.CTCategoryID = oItem.CTCategoryID;
                                detailEntity.DHeadNeed = oItem.DHeadNeed;
                                detailEntity.SHeadNeed = oItem.SHeadNeed;
                                detailEntity.SGHeadName = oItem.SGHeadName;
                                detailEntity.IIPDetailsID = itemChild.IIPDetailsID;
                                detailEntity.DHeadID = itemChild.DHeadID;
                                detailEntity.CalculationOn = itemChild.CalculationOn;
                                detailEntity.ValueInFC = itemChild.ValueInFC;
                                detailEntity.ValueInLC = itemChild.ValueInLC;
                                detailEntity.CurConvRate = itemChild.CurConvRate;
                                detailEntity.SHeadID = itemChild.SHeadID;
                                entityTemp.IPDetails.Add(detailEntity);
                            }
                            else
                            {
                                detailEntity.SGHeadID = oItem.SGHeadID;
                                detailEntity.CTCategoryID = oItem.CTCategoryID;
                                detailEntity.DHeadNeed = oItem.DHeadNeed;
                                detailEntity.SHeadNeed = oItem.SHeadNeed;
                                detailEntity.SGHeadName = oItem.SGHeadName;

                                detailEntity.IIPDetailsID = itemChild.IIPDetailsID;
                                detailEntity.DHeadID = itemChild.DHeadID;
                                detailEntity.CalculationOn = itemChild.CalculationOn;
                                detailEntity.ValueInFC = itemChild.ValueInFC;
                                detailEntity.ValueInLC = itemChild.ValueInLC;
                                detailEntity.CurConvRate = itemChild.CurConvRate;
                                detailEntity.SHeadID = itemChild.SHeadID;
                                detailEntity.EntityState = EntityState.Modified;

                            }
                        }

                    }

                }

                entityTemp.IPChilds.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                entityTemp.IPDetails.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();

                await _service.SaveAsync(entityTemp, EntityState.Modified);
            }
            else
            {
                entity = new ImportInvoicePaymentMaster();
                models.AddedBy = AppUser.UserCode;
                models.DateAdded = DateTime.Now;
                models.IsCDA = IsCDA;
                models.BeneficiaryID = models.SupplierID;

                models.IPChilds.ForEach(child =>
                {
                    child.EntityState = EntityState.Added;
                });
                models.IPDetails.ForEach(oItem =>
                {
                    oItem.EntityState = EntityState.Added;
                    oItem.IPDetailSub.ForEach(item =>
                    {
                        oItem.EntityState = EntityState.Added;
                    });
                });

                entity = models;
                await _service.SaveAsync(entity, EntityState.Added);

            }
        
            return Ok();
        }

        [HttpGet]
        [Route("ipEdit/{IIPMasterID}/{isCDAPage}")]
        public async Task<IActionResult> GetEdit(String IIPMasterID, bool isCDAPage)
        {
            return Ok(await _service.GetEditAsync(IIPMasterID.ToInt(), isCDAPage));
        }

    }
}
