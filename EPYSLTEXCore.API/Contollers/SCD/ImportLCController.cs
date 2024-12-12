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

namespace EPYSLTEXCore.API.Contollers.SCD
{
    [Route("ImportLCApi")]
    public class ImportLCController : ApiBaseController
    {
        private readonly IImportLCService _importLCService;
        private readonly ICommonHelpers _commonHelpers;
        //private readonly ISignatureRepository _signatureRepository;
        private readonly ISelect2Service _select2Service;

        public ImportLCController(IImportLCService importLCService
            , ICommonHelpers commonHelpers
        //, ISignatureRepository signatureRepository
        , IUserService userService
            , ISelect2Service select2Service) : base(userService)
        {
            _importLCService = importLCService;
            _commonHelpers = commonHelpers;
            //_signatureRepository = signatureRepository;
            _select2Service = select2Service;
        }

        [Route("importLCMasterData")]
        public async Task<IActionResult> GetImportLCData(Status status, bool isCDAPage)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnLcMaster> records = await _importLCService.GetImportLCData(status, isCDAPage, paginationInfo);
            var totalCount = records.FirstOrDefault() == null ? 0 : records.FirstOrDefault().TotalRows;
            var response = new TableResponseModel(totalCount, records);

            return Ok(response);
        }

        [HttpGet]
        [Route("getProposal/{ProposalID}")]
        public async Task<IActionResult> GetNew(int ProposalID)
        {
            return Ok(await _importLCService.GetNewAsync(ProposalID));
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            YarnLcMaster record = await _importLCService.GetAsync(id);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }
        /*//OFF FOR CORE// Have to solve error
        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save()
        {
            //if (!Request.Content.IsMimeMultipartContent())
            if (Request.ContentType != null && Request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Unsupported media type.");

            //if (Request.Content.Headers.ContentLength >= 4 * 1024 * 1024)
            if (Request.ContentLength.HasValue && Request.ContentLength >= 4 * 1024 * 1024)
                return BadRequest("File is bigger than 4MB");

            //OFF FOR CORE//var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());
            #region New For CORE
            var form = await Request.ReadFormAsync();
            var provider = new InMemoryMultipartFormDataStreamProvider();

            // Mimicking the functionality of the old `InMemoryMultipartFormDataStreamProvider`
            foreach (var file in form.Files)
            {
                using var stream = file.OpenReadStream();
                var fileData = new InMemoryFileData
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Content = new MemoryStream()
                };

                await stream.CopyToAsync(fileData.Content);
                provider.Files.Add(fileData);
            }

            foreach (var field in form)
            {
                provider.FormData.Add(field.Key, field.Value.ToString());
            }

            // Your existing variable `provider` now has the files and form data.
            #endregion

            var formData = provider.FormData;

            YarnLcMaster model = formData.ConvertToObject<YarnLcMaster>();
            model.LcChilds = JsonConvert.DeserializeObject<List<YarnLcChild>>(formData.Get("LcChilds"));
            model.LcDocuments = JsonConvert.DeserializeObject<List<YarnLcDocument>>(formData.Get("LcDocuments"));
            //model.LcNo = await _signatureRepository.GetMaxNoAsync(TableNames.IMPORT_LC_NO);

            //// Validate Data should be called for each type before saving file
            //var validator = new YarnLcMasterValidator();
            //var validationResult = validator.Validate(model);
            //if (!validationResult.IsValid)
            //{
            //    string messages = string.Join("<br>", validationResult.Errors.Select(x => x.PropertyName.Replace("Childs", "Row") + " : " + x.ErrorMessage));
            //    return BadRequest(messages);
            //}

            #region Save Image

            var filePath = "";
            var previewTemplate = "";
            if (provider.Files.Any())
            {
                var originalFile = provider.Files[0];
                var inputStream = await originalFile.ReadAsStreamAsync();

                var fileName = string.Join("", originalFile.Headers.ContentDisposition.FileName.Split(Path.GetInvalidFileNameChars()));
                var contentType = originalFile.Headers.ContentType.ToString();

                var fileExtension = Path.GetExtension(fileName);
                previewTemplate = fileExtension.Contains(".pdf") ? "pdf" : MimeMapping.GetMimeMapping(fileName).StartsWith("image/") ? "image" : "office";

                //model.LcNo = await _signatureRepository.GetMaxNoAsync(TableNames.IMPORT_LC_NO);
                filePath = $"{AppConstants.LC_FILE_PATH}/{string.Join("_", model.LCNo.Split(Path.GetInvalidFileNameChars()))}_{fileName}";
                var savePath = HttpContext.Current.Server.MapPath(filePath);
                using (var fileStream = File.Create(savePath))
                {
                    inputStream.Seek(0, SeekOrigin.Begin);
                    await inputStream.CopyToAsync(fileStream);
                }
            }
            else
            {
                filePath = model.LCFilePath;
                previewTemplate = model.AttachmentPreviewTemplate;
            }

            #endregion Save Image

            YarnLcMaster entity;
            if (model.LCID > 0)
            {
                entity = await _importLCService.GetAllByIDAsync(model.LCID);

                entity.CompanyID = model.CompanyID;
                entity.IssueBankID = model.IssueBankID;
                entity.CalculationOfTenorID = model.CalculationOfTenorID;
                entity.PaymentModeID = model.PaymentModeID;
                entity.LCNo = model.LCNo;
                entity.LCDate = model.LCDate;
                entity.LCValue = model.LCValue;
                entity.LCQty = model.LCQty;
                entity.LCUnit = model.LCUnit;
                entity.LCReceiveDate = model.LCReceiveDate;
                entity.LCExpiryDate = model.LCExpiryDate;
                entity.CurrencyID = model.CurrencyID;
                entity.LienBankID = model.LienBankID;
                entity.PaymentBankID = model.PaymentBankID;
                entity.NotifyPaymentBank = model.NotifyPaymentBank;
                entity.BBReportingNumber = model.BBReportingNumber;
                entity.BankAcceptanceFrom = model.BankAcceptanceFrom;
                entity.MaturityCalculationID = model.MaturityCalculationID;
                entity.DocPresentationDays = model.DocPresentationDays;
                entity.Tolerance = model.Tolerance;
                entity.IncoTermsID = model.IncoTermsID;
                entity.CIDecID = model.CIDecID;
                entity.BCDecID = model.BCDecID;
                entity.HSCode = model.HSCode;
                entity.LCFilePath = filePath;
                entity.AttachmentPreviewTemplate = previewTemplate;
                entity.BankRefNo = model.BankRefNo;
                entity.AccountNo = model.AccountNo;
                entity.FormOfDC = model.FormOfDC;
                entity.TTTypeID = model.TTTypeID;
                entity.AvailableWithID = model.AvailableWithID;
                entity.TenureofLCID = model.TenureofLCID;
                entity.PartialShipmentID = model.PartialShipmentID;
                entity.TransshipmentID = model.TransshipmentID;
                entity.LoadingPortID = model.LoadingPortID;
                entity.DischargePortID = model.DischargePortID;
                entity.IsConInsWith = model.IsConInsWith;
                entity.UpdatedBy = UserId;
                entity.DateUpdated = DateTime.Now;
                entity.PreRevisionNo = entity.YarnPIRevision;

                if (model.isAmendentValue)
                {
                    //entity.RevisionNo = (entity.RevisionNo + 1);
                    entity.RevisionNo = model.RevisionNo;
                    entity.isAmendentValue = model.isAmendentValue;
                }
                //If Need to Approve
                if (model.Approve)
                {
                    entity.Approve = model.Approve;
                    entity.ApproveBy = UserId;
                    entity.ApproveDate = DateTime.Now;
                }
                entity.EntityState = EntityState.Modified;

                foreach (YarnLcChild item in entity.LcChilds)
                    item.EntityState = EntityState.Unchanged;

                foreach (YarnLcDocument item in entity.LcDocuments)
                    item.EntityState = EntityState.Unchanged;

                YarnLcChild child;
                foreach (YarnLcChild item in model.LcChilds)
                {
                    child = entity.LcChilds.FirstOrDefault(x => x.YPIReceiveMasterID == item.YPIReceiveMasterID);
                    if (child == null)
                    {
                        child = item;
                        entity.LcChilds.Add(child);
                    }
                    else
                    {
                        // Assigning all properties
                        // Values can be changed because multiple PO can be joined together
                        //child.ItemMasterID = item.ItemMasterID;
                        //child.YarnProgramId = item.YarnProgramId;
                        child.EntityState = EntityState.Modified;
                    }
                }

                YarnLcDocument yarnLcDocument;
                foreach (YarnLcDocument item in model.LcDocuments)
                {
                    yarnLcDocument = entity.LcDocuments.FirstOrDefault(x => x.DocId == item.DocId);
                    if (yarnLcDocument == null)
                    {
                        yarnLcDocument = item;
                        entity.LcDocuments.Add(yarnLcDocument);
                    }
                    else yarnLcDocument.EntityState = EntityState.Modified;
                }
            }
            else
            {
                entity = model;
                entity.LCFilePath = filePath;
                entity.AttachmentPreviewTemplate = previewTemplate;
                entity.AddedBy = UserId;
                entity.Proposed = model.Proposed;
                entity.ProposedDate = DateTime.Now;
            }

            await _importLCService.SaveAsync(entity);

            return Ok();
        }
        */
    }
}
