using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces.SCD;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Text.RegularExpressions;

namespace EPYSLTEXCore.API.Contollers.SCD
{
    [Route("ImportLCApi")]
    public class ImportLCController : ApiBaseController
    {
        private readonly IImportLCService _importLCService;
        private readonly ICommonHelpers _commonHelpers;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ISelect2Service _select2Service;

        public ImportLCController(IImportLCService importLCService
            , ICommonHelpers commonHelpers
         ,IWebHostEnvironment hostingEnvironment
        , IUserService userService
            , ISelect2Service select2Service) : base(userService)
        {
            _importLCService = importLCService;
            _commonHelpers = commonHelpers;
            _hostingEnvironment = hostingEnvironment;
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
      
        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save()
        {
            var formData = Request.Form;    

            // Access the uploaded file
            var file = Request.Form.Files.FirstOrDefault();
      
            YarnLcMaster model = formData.ConvertToObject<YarnLcMaster>();
            model.LcChilds = JsonConvert.DeserializeObject<List<YarnLcChild>>(formData["LcChilds"]);
            model.LcDocuments = JsonConvert.DeserializeObject<List<YarnLcDocument>>(formData["LcDocuments"]);

            #region Save Image

            var filePath = "";
            var previewTemplate = "";


            if (file != null)
            {
                if (file.Length > 4 * 1024 * 1024)
                    return BadRequest("File is bigger than 4MB.");
                var originalFile = file;
                var inputStream = originalFile.OpenReadStream();

                var fileName = string.Join("", originalFile.FileName.Split(Path.GetInvalidFileNameChars()));
                fileName = GetValidFileName(fileName);
                var contentType = originalFile.ContentType;

                var fileExtension = Path.GetExtension(fileName);


                var provider = new FileExtensionContentTypeProvider();
                string mimeType;

                if (provider.TryGetContentType(fileName, out mimeType))
                {
                    // Set previewTemplate based on file type
                    previewTemplate = fileExtension.Contains(".pdf") ? "pdf" :
                                      mimeType.StartsWith("image/") ? "image" :
                                      "office";
                }
                else
                {
                    // If MIME type couldn't be determined, fallback to "office"
                    previewTemplate = "office";
                }

                filePath = $"{AppConstants.LC_FILE_PATH}/{string.Join("_", model.LCNo.Split(Path.GetInvalidFileNameChars()))}_{fileName}";
                var fullPath = Path.Combine(_hostingEnvironment.WebRootPath, filePath);
                string directoryPath = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }



                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await inputStream.CopyToAsync(fileStream);
                }
            }
            else
            {
                filePath = "defaultFilePath";  // Handle no file uploaded scenario
                previewTemplate = "office";    // Set default preview template
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
                entity.UpdatedBy = AppUser.UserCode;
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
                    entity.ApproveBy = AppUser.UserCode;
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
                entity.AddedBy = AppUser.UserCode;
                entity.Proposed = model.Proposed;
                entity.ProposedDate = DateTime.Now;
            }

            await _importLCService.SaveAsync(entity);

            return Ok();
        }
        private string GetValidFileName(string fileName)
        {
            fileName = fileName.Replace("#", " ")
                               .Replace("?", "");
            fileName = Regex.Replace(fileName, @"[^\u0000-\u007F]+", string.Empty);
            return fileName;
        }


    }
}
