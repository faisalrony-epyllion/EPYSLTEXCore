using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Application.Interfaces.SCD;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Text.Json;
using EPYSLTEXCore.API.Extension;
using EPYSLTEX.Infrastructure.Services;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.SCD
{
    [Route("ciAPI")]
    public class CommercialInvoiceController : ApiBaseController
    {
        private readonly ICommercialInvoiceService _service;

        public CommercialInvoiceController(ICommercialInvoiceService service, IUserService userService) : base(userService)
        {
            _service = service;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, bool isCDAPage)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnCIMaster> records = await _service.GetPagedAsync(status, isCDAPage, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{Lcid}")]
        public async Task<IActionResult> GetNew(int Lcid)
        {
            return Ok(await _service.GetNewAsync(Lcid));
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            YarnCIMaster record = await _service.GetAsync(id);
            return Ok(record);
        }
        /*//OFF FOR CORE// Have to solve error
        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save()
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type.");

            if (Request.Content.Headers.ContentLength >= 4 * 1024 * 1024)
                return BadRequest("File is bigger than 4MB");

            var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());

            if (!provider.Files.Any()) return BadRequest("You must upload PI file.");


            var formData = provider.FormData;

            YarnCIMaster model = formData.ConvertToObject<YarnCIMaster>();
            model.CIChilds = JsonConvert.DeserializeObject<List<YarnCIChild>>(formData.Get("CiChilds"));
            model.CIChildPIs = JsonConvert.DeserializeObject<List<YarnCIChildPI>>(formData.Get("CiChildPis"));



            #region Single Image Save

            string fileName = "";
            string filePath = "";
            string previewTemplate = "";

            if (provider.Files.Any())
            {
                var originalFile = provider.Files[0];
                var inputStream = await originalFile.ReadAsStreamAsync();

                fileName = string.Join("", originalFile.Headers.ContentDisposition.FileName.Split(Path.GetInvalidFileNameChars()));
                var contentType = originalFile.Headers.ContentType.ToString();

                var fileExtension = Path.GetExtension(fileName);
                previewTemplate = fileExtension.Contains(".pdf") ? "pdf" : MimeMapping.GetMimeMapping(fileName).StartsWith("image/") ? "image" : "office";

                filePath = $"{AppConstants.YARN_CI_FILE_PATH}/{string.Join("_", model.CINo.Split(Path.GetInvalidFileNameChars()))}_{fileName}";

                model.CIFilePath = filePath;
                model.AttachmentPreviewTemplate = previewTemplate;

                var savePath = HttpContext.Current.Server.MapPath(filePath);
                using (var fileStream = File.Create(savePath))
                {
                    inputStream.Seek(0, SeekOrigin.Begin);
                    await inputStream.CopyToAsync(fileStream);
                }
            }

            #endregion Single Image Save 

            #region Other Attachments Save
            fileName = "";
            filePath = "";
            previewTemplate = "";

            model.YarnCIDocs.ForEach(x =>
            {
                x.FileName = Path.GetFileNameWithoutExtension(x.FileName);
            });

            for (int i = 1; i < provider.Files.Count(); i++)
            {
                var originalFile = provider.Files[i];
                var inputStream = await originalFile.ReadAsStreamAsync();

                fileName = string.Join("", originalFile.Headers.ContentDisposition.FileName.Split(Path.GetInvalidFileNameChars()));
                var splitFileName = fileName.Split('~');
                int docTypeID = 0;
                if (splitFileName.Count() > 0)
                {
                    fileName = splitFileName[0];
                    docTypeID = Convert.ToInt32(splitFileName[1]);
                }
                var contentType = originalFile.Headers.ContentType.ToString();

                var fileExtension = Path.GetExtension(fileName);
                previewTemplate = fileExtension.Contains(".pdf") ? "pdf" : MimeMapping.GetMimeMapping(fileName).StartsWith("image/") ? "image" : "office";

                filePath = $"{AppConstants.YARN_CI_FILE_PATH}/CommercialInvoice_{fileName}";
                var savePath = HttpContext.Current.Server.MapPath(filePath);

                using (var fileStream = File.Create(savePath))
                {
                    inputStream.Seek(0, SeekOrigin.Begin);
                    await inputStream.CopyToAsync(fileStream);
                }

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                int indexF = model.YarnCIDocs.FindIndex(x => x.FileName == fileNameWithoutExtension);
                if (indexF > -1)
                {
                    model.YarnCIDocs[indexF].FileName = !string.IsNullOrEmpty(fileName) ? Path.GetFileNameWithoutExtension(fileName) : "'";
                    model.YarnCIDocs[indexF].ImagePath = filePath;
                    model.YarnCIDocs[indexF].PreviewTemplate = previewTemplate;
                }
                else
                {
                    model.YarnCIDocs.Add(new YarnCIDoc()
                    {
                        FileName = !string.IsNullOrEmpty(fileName) ? Path.GetFileNameWithoutExtension(fileName) : "'",
                        ImagePath = filePath,
                        PreviewTemplate = previewTemplate,
                        DocTypeID = docTypeID
                    });
                }
            }
            #endregion Other Attachments Save

            YarnCIMaster entity;
            if (model.CIID > 0)
            {
                entity = await _service.GetAllAsync(model.CIID);

                entity.CIDate = model.CIDate;
                entity.CIValue = model.CIValue;
                entity.ConsigneeId = model.ConsigneeId;
                entity.ExpNo = model.ExpNo;
                entity.CINo = model.CINo;
                entity.ExpDate = model.ExpDate;
                entity.NotifyPartyId = model.NotifyPartyId;
                entity.BOENo = model.BOENo;
                entity.BOEDate = model.BOEDate;
                entity.SG = model.SG;
                entity.BLNo = model.BLNo;
                entity.BLDate = model.BLDate;
                entity.ContainerStatus = model.ContainerStatus;
                entity.CIFilePath = filePath;
                entity.AttachmentPreviewTemplate = previewTemplate;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                foreach (YarnCIChild item in entity.CIChilds)
                {
                    foreach (YarnCIChildYarnSubProgram subProgram in item.SubPrograms)
                        subProgram.EntityState = EntityState.Modified;
                    item.EntityState = EntityState.Unchanged;
                }

                foreach (YarnCIChildPI item in entity.CIChildPIs)
                    item.EntityState = EntityState.Unchanged;

                YarnCIChild child;
                foreach (YarnCIChild item in model.CIChilds)
                {
                    child = entity.CIChilds.FirstOrDefault(x => x.ItemMasterID == item.ItemMasterID);
                    if (child == null)
                    {
                        item.CIID = entity.CIID;
                        entity.CIChilds.Add(item);
                    }
                    else
                    {
                        child.ItemDescription = item.ItemDescription;
                        child.InvoiceQty = item.InvoiceQty;
                        child.Rate = item.Rate;
                        child.PdValue = item.PdValue;
                        child.NoOfCarton = item.NoOfCarton;
                        child.GrossWeight = item.GrossWeight;
                        child.NetWeight = item.NetWeight;
                        child.NoOfCone = item.NoOfCone;
                        child.PIQty = item.PIQty;
                        child.PIValue = item.PIValue;
                        child.EntityState = EntityState.Modified;

                        #region ChildSubProgram

                        var subProgramIdArray = Array.ConvertAll(item.YarnSubProgramIDs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), int.Parse);
                        foreach (var subProgramId in subProgramIdArray)
                        {
                            var subProgramEntity = child.SubPrograms.FirstOrDefault(x => x.SubProgramID == subProgramId);
                            if (subProgramEntity == null)
                            {
                                subProgramEntity = new YarnCIChildYarnSubProgram
                                {
                                    SubProgramID = subProgramId
                                };
                                child.SubPrograms.Add(subProgramEntity);
                            }
                            else
                            {
                                subProgramEntity.EntityState = EntityState.Modified;
                            }
                        }

                        #endregion ChildSubProgram
                    }
                }

                YarnCIChildPI YarnCiChildPi;
                foreach (YarnCIChildPI item in model.CIChildPIs)
                {
                    YarnCiChildPi = entity.CIChildPIs.FirstOrDefault(x => x.YPIMasterID == item.YPIMasterID);
                    if (YarnCiChildPi == null)
                    {
                        item.CIID = entity.CIID;
                        entity.CIChildPIs.Add(item);
                    }
                    else YarnCiChildPi.EntityState = EntityState.Modified;
                }
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
            }

            await _service.SaveAsync(entity);

            return Ok();
        }
        */
        [Route("approve")]
        [HttpPost]
        public async Task<IActionResult> Approve(YarnCIMaster model)
        {
            YarnCIMaster entity = new YarnCIMaster();
            List<YarnCIMaster> entityList = new List<YarnCIMaster>();

            bool isAcceptance = model.Acceptance;
            bool isBankAccept = model.BankAccept;



            if (isAcceptance == true)
            {
                entity = await _service.GetAsync(model.CIID);
                entity.Acceptance = true;
                entity.AcceptanceBy = AppUser.UserCode;
                entity.AcceptanceDate = model.AcceptanceDate;

                entity.Reject = false;
                entity.RejectBy = 0;
                entity.RejectDate = null;
                entity.EntityState = EntityState.Modified;
                await _service.UpdateEntityAsync(entity);

            }
            else if (isBankAccept == true)
            {
                ImportDocumentAcceptanceChargeDetails detailEntity;
                List<ImportDocumentAcceptanceChargeDetails> tempDetailList = new List<ImportDocumentAcceptanceChargeDetails>();

                string CIID = "";
                CIID = String.Join(",", model.YarnCIList.Select(s => s.CIID).ToList());
                entityList = await _service.GetAllCIInfoByIDAsync(CIID, model.BankRefNumber);

                entity.IDACDetails = entityList.FirstOrDefault().IDACDetails;
                entity.IDACDetails.SetUnchanged();

                entityList.ForEach(child =>
                {
                    child.BankAccept = true;
                    child.BankAcceptBy = AppUser.UserCode;
                    child.BankAcceptDate = model.BankAcceptDate;
                    child.MaturityDate = model.MaturityDate;
                    child.BankRefNumber = model.BankRefNumber;

                    child.Reject = false;
                    child.RejectBy = 0;
                    child.RejectDate = null;
                    child.EntityState = EntityState.Modified;
                });
                if (entity.IDACDetails == null)
                {
                    model.IDACDetails.ForEach(oItem =>
                    {
                        oItem.EntityState = EntityState.Added;
                        oItem.IDACDetailSub.ForEach(item =>
                        {
                            oItem.EntityState = EntityState.Added;
                            tempDetailList.Add(oItem);
                        });

                    });
                }
                else
                {
                    model.IDACDetails.ForEach(oItem =>
                    {
                        oItem.EntityState = EntityState.Added;
                        oItem.IDACDetailSub.ForEach(item =>
                        {
                            detailEntity = entity.IDACDetails.FirstOrDefault(c => c.ADetailsID == item.ADetailsID);
                            if (detailEntity == null)
                            {
                                detailEntity = item;
                                detailEntity.EntityState = EntityState.Added;
                                detailEntity.SGHeadID = oItem.SGHeadID;
                                detailEntity.CTCategoryID = oItem.CTCategoryID;
                                detailEntity.DHeadNeed = oItem.DHeadNeed;
                                detailEntity.SHeadNeed = oItem.SHeadNeed;
                                detailEntity.SGHeadName = oItem.SGHeadName;
                                detailEntity.ADetailsID = item.ADetailsID;
                                detailEntity.DHeadID = item.DHeadID;
                                detailEntity.CalculationOn = item.CalculationOn;
                                detailEntity.ValueInFC = item.ValueInFC;
                                detailEntity.ValueInLC = item.ValueInLC;
                                detailEntity.CurConvRate = item.CurConvRate;
                                detailEntity.SHeadID = item.SHeadID;
                                //entity.IDACDetails.Add(detailEntity);
                                tempDetailList.Add(detailEntity);
                            }
                            else
                            {
                                detailEntity.SGHeadID = oItem.SGHeadID;
                                detailEntity.CTCategoryID = oItem.CTCategoryID;
                                detailEntity.DHeadNeed = oItem.DHeadNeed;
                                detailEntity.SHeadNeed = oItem.SHeadNeed;
                                detailEntity.SGHeadName = oItem.SGHeadName;

                                detailEntity.ADetailsID = item.ADetailsID;
                                detailEntity.DHeadID = item.DHeadID;
                                detailEntity.CalculationOn = item.CalculationOn;
                                detailEntity.ValueInFC = item.ValueInFC;
                                detailEntity.ValueInLC = item.ValueInLC;
                                detailEntity.CurConvRate = item.CurConvRate;
                                detailEntity.SHeadID = item.SHeadID;
                                detailEntity.EntityState = EntityState.Modified;
                                tempDetailList.Add(detailEntity);
                            }
                        });
                        oItem.EntityState = EntityState.Added;
                    });
                }

                entityList.ForEach(oItem => { oItem.IDACDetails = tempDetailList; });
                await _service.UpdateMultiEntityAsync(entityList);

            }
            return Ok();
        }

        [Route("reject")]
        [HttpPost]
        public async Task<IActionResult> Reject(YarnCIMaster model)
        {
            YarnCIMaster entity;
            entity = await _service.GetAsync(model.CIID);
            entity.Acceptance = false;
            entity.AcceptanceBy = 0;
            entity.AcceptanceDate = null;
            entity.Reject = true;
            entity.RejectBy = AppUser.UserCode;

            entity.BankAccept = model.BankAccept;
            entity.BankAcceptDate = model.BankAcceptDate;
            entity.BankAcceptBy = AppUser.UserCode;

            entity.MaturityDate = model.MaturityDate;
            entity.BankRefNumber = model.BankRefNumber;

            entity.RejectDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }

        /*
        [Route("approve/{id}")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            YarnCIMaster entity;
            entity = await _service.GetAsync(id);
            entity.Acceptance = true;
            entity.AcceptanceBy = AppUser.UserCode;
            entity.AcceptanceDate = DateTime.Now;
            entity.Reject = false;
            entity.RejectBy = 0;
            entity.RejectDate = null;

            //entity.BankAccept = model.BankAccept;
            //entity.BankAcceptDate = model.BankAcceptDate;
            //entity.BankAcceptBy = AppUser.UserCode;

            //entity.MaturityDate = model.MaturityDate;
            //entity.BankRefNumber = model.BankRefNumber;

            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }

        [Route("reject/{id}")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            YarnCIMaster entity;
            entity = await _service.GetAsync(id);
            entity.Acceptance = false;
            entity.AcceptanceBy = 0;
            entity.AcceptanceDate = null;
            entity.Reject = true;
            entity.RejectBy = AppUser.UserCode;
            entity.RejectDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);
            return Ok();
        }
        */


        [HttpGet]
        [Route("get-item-details/{itemIds}")]
        public async Task<IActionResult> GetBatchDetails(string itemIds)
        {
            List<YarnCIMaster> records = await _service.GetItemDetails(itemIds);
            return Ok(records);
        }

        [HttpGet]
        [Route("createBankAcceptance/{nCIIDs}/{companyIDs}/{supplierIDs}/{bankBranchIDs}")]
        public async Task<IActionResult> createBankAcceptance(string nCIIDs, string companyIDs, string supplierIDs, string bankBranchIDs)
        {
            return Ok(await _service.createBankAcceptance(nCIIDs, companyIDs, supplierIDs, bankBranchIDs));
        }
    }
}
