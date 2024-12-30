using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System.Data.Entity;
namespace EPYSLTEX.Web.Controllers.Apis
{
    [Authorize]
    [Route("api/lab-test-result")]
    public class LabTestResultController : ApiBaseController
    {
        private readonly ILabTestResultService _LabTestResultService;
        private readonly ICommonHelpers _commonHelpers;
        private readonly ISelect2Service _select2Service;
        private readonly ICommonHelperService _commonService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public LabTestResultController(ILabTestResultService LabTestResultService, IUserService userService, IWebHostEnvironment hostingEnvironment
            , ICommonHelpers commonHelpers
        , ISelect2Service select2Service
            , ICommonHelperService commonService) : base(userService)
        {
            _LabTestResultService = LabTestResultService;
            _commonHelpers = commonHelpers;
            _select2Service = select2Service;
            _commonService = commonService;
            _hostingEnvironment = hostingEnvironment;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, int offset = 0, int limit = 10, string filter = null, string sort = null, string order = null)
        {
            var filterBy = _commonHelpers.GetFilterBy(filter);
            var orderBy = string.IsNullOrEmpty(sort) ? "" : $"ORDER BY {sort} {order}";
            List<LabTestRequisitionMaster> records = await _LabTestResultService.GetPagedAsync(status, offset, limit, filterBy, orderBy);
            var totalCount = records.FirstOrDefault() == null ? 0 : records.FirstOrDefault().TotalRows;
            var response = new TableResponseModel(totalCount, records);
            return Ok(response);
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            LabTestRequisitionMaster record = await _LabTestResultService.GetAsync(id);
            Guard.Against.NullObject(id, record);
            foreach (var child in record.LabTestRequisitionBuyers)
            {
                child.LabTestRequisitionBuyerParameters = record.LabTestRequisitionBuyers[0].LabTestRequisitionBuyerParameters.Where(x => x.LTReqBuyerID == child.LTReqBuyerID).ToList();
            }

            return Ok(record);
        }

        [HttpGet]
        [Route("buyer-parameter/{ids}")]
        public async Task<IActionResult> GetBuyerParameterByBuyerId(string ids)
        {
            List<LabTestRequisitionBuyer> labTestreqBuyers = new List<LabTestRequisitionBuyer>();
            var data = await _select2Service.GetContactNamesByCintactIdsAsync(ContactCategoryConstants.CONTACT_CATEGORY_BUYER, ids);
            foreach (var child in data)
            {
                LabTestRequisitionBuyer labTestReqBuyer = new LabTestRequisitionBuyer
                {
                    BuyerID = Convert.ToInt32(child.id),
                    BuyerName = child.text
                };
                labTestreqBuyers.Add(labTestReqBuyer);
            }
            foreach (LabTestRequisitionBuyer child in labTestreqBuyers)
            {
                child.LabTestRequisitionBuyerParameters = await _LabTestResultService.GetBuyerParameterByBuyerId(child.BuyerID);
            }
            return Ok(labTestreqBuyers);
        }


        #region save will be fixed when all will be done
        [Route("save")]
        [HttpPost]

        public async Task<IActionResult> Save()
        {
            var formData = Request.Form;
            // Access the uploaded file
            var file = Request.Form.Files.FirstOrDefault();

            // Validate file
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded or the file is empty.");

            if (file.Length > 4 * 1024 * 1024)
                return BadRequest("File is bigger than 4MB.");

            var model = JsonConvert.DeserializeObject<LabTestRequisitionMaster>(formData["data"]);
            model.LabTestRequisitionBuyers = JsonConvert.DeserializeObject<List<LabTestRequisitionBuyer>>(formData["LabTestRequisitionBuyers"]);
            model.LabTestRequisitionImages = JsonConvert.DeserializeObject<List<LabTestRequisitionImage>>(formData["LabTestRequisitionImages"]);

       

            string fileName = "",
               filePath = "",
               previewTemplate = "";

            model.LabTestRequisitionImages.ForEach(x =>
            {
                x.FileName = Path.GetFileNameWithoutExtension(x.FileName);
            });
            if (file != null)
            {
                for (int i = 1; i < Request.Form.Files.Count(); i++)
                {
                     fileName = "";
                    filePath = "";
                    previewTemplate = "";
                    var originalFile = Request.Form.Files[i];
                    var inputStream = originalFile.OpenReadStream();

                    fileName = string.Join("", originalFile.FileName.Split(Path.GetInvalidFileNameChars()));
                    var splitFileName = fileName.Split('~');
                    int docTypeID = 0;
                    if (splitFileName.Count() > 0)
                    {
                        fileName = splitFileName[0];
                        docTypeID = Convert.ToInt32(splitFileName[1]);
                    }
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


                    filePath = $"{UploadLocations.RND_LABTEST_FILE_PATH}/CommercialInvoice_{fileName}";
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
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    int indexF = model.LabTestRequisitionImages.FindIndex(x => x.FileName == fileNameWithoutExtension);
                    if (indexF > -1)
                    {
                        model.LabTestRequisitionImages[indexF].FileName = !string.IsNullOrEmpty(fileName) ? Path.GetFileNameWithoutExtension(fileName) : "'";
                        model.LabTestRequisitionImages[indexF].ImagePath = filePath;
                        model.LabTestRequisitionImages[indexF].PreviewTemplate = previewTemplate;
                    }
                    else
                    {
                        model.LabTestRequisitionImages.Add(new LabTestRequisitionImage()
                        {
                            FileName = !string.IsNullOrEmpty(fileName) ? Path.GetFileNameWithoutExtension(fileName) : "'",
                            ImagePath = filePath,
                            PreviewTemplate = previewTemplate
                     
                        });
                    }
                }
            }
            else
            {
                filePath = "defaultFilePath";  // Handle no file uploaded scenario
                previewTemplate = "office";    // Set default preview template
            }
           


            LabTestRequisitionMaster entity = await _LabTestResultService.GetAllByIDAsync(model.LTReqMasterID);
            foreach (LabTestRequisitionBuyer reqBuyer in entity.LabTestRequisitionBuyers)
            {
                reqBuyer.LabTestRequisitionBuyerParameters = entity.LabTestRequisitionBuyerParameters.Where(x => x.LTReqBuyerID == reqBuyer.LTReqBuyerID).ToList();
            }

            entity.LabTestRequisitionBuyers.SetUnchanged();
            entity.LabTestRequisitionBuyers.ForEach(x => { x.LabTestRequisitionBuyerParameters.SetUnchanged(); });
            //For multiple image
            entity.LabTestRequisitionImages.SetUnchanged();

            LabTestRequisitionImage labtestRequisitionimage;
            foreach (LabTestRequisitionImage child in model.LabTestRequisitionImages)
            {
                if (!child.IsDelete)
                {
                    LabTestRequisitionImage childimage = entity.LabTestRequisitionImages.Find(x => x.LTReqImageID == child.LTReqImageID && x.LTReqImageID != 0);
                    if (childimage == null)
                    {
                        labtestRequisitionimage = CommonFunction.DeepClone(child);
                        labtestRequisitionimage.LTReqMasterID = entity.LTReqMasterID;
                        labtestRequisitionimage.EntityState = EntityState.Added;
                        entity.LabTestRequisitionImages.Add(labtestRequisitionimage);
                    }
                    else
                    {
                        childimage.ImageGroup = child.ImageGroup;
                        childimage.ImageSubGroup = child.ImageSubGroup;
                        childimage.BPID = child.BPID;
                        childimage.ImagePath = child.ImagePath;
                        childimage.FileName = child.FileName;
                        childimage.EntityState = EntityState.Modified;
                    }
                }

            }
            foreach (LabTestRequisitionImage item in entity.LabTestRequisitionImages.Where(x => x.EntityState == EntityState.Unchanged))
            {
                item.EntityState = EntityState.Deleted;
            }
            //end for multiple image
            entity.ReqDate = model.ReqDate;
            entity.FabricQty = model.FabricQty;
            entity.UnitID = model.UnitID;
            entity.UpdatedBy = AppUser.UserCode;
            entity.DateUpdated = DateTime.Now;
            entity.PerformanceCode = model.PerformanceCode;
          
            if (file is not null)
            {
                var fullPath = Path.Combine(_hostingEnvironment.WebRootPath, entity.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                entity.FileName = !string.IsNullOrEmpty(fileName) ? Path.GetFileNameWithoutExtension(fileName) : "'";
                entity.ImagePath = filePath;
                entity.PreviewTemplate = previewTemplate;
            }

            entity.EntityState = EntityState.Modified;

            LabTestRequisitionBuyer labTestRequisitionBuyer;
            LabTestRequisitionBuyerParameter labTestRequisitionBuyerParameter;
            foreach (LabTestRequisitionBuyer child in model.LabTestRequisitionBuyers)
            {
                labTestRequisitionBuyer = entity.LabTestRequisitionBuyers.FirstOrDefault(x => x.LTReqBuyerID == child.LTReqBuyerID);
                if (labTestRequisitionBuyer == null)
                {
                    labTestRequisitionBuyer = child;
                    labTestRequisitionBuyer.EntityState = EntityState.Added;
                    if (model.IsSend)
                    {
                        labTestRequisitionBuyer.IsSend = true;
                        labTestRequisitionBuyer.SendBy = AppUser.UserCode;
                        labTestRequisitionBuyer.SendDate = DateTime.Now;
                    }
                    entity.LabTestRequisitionBuyers.Add(labTestRequisitionBuyer);
                }
                else
                {
                    labTestRequisitionBuyer.LTResultUpdate = true;
                    labTestRequisitionBuyer.LTResultUpdateBy = AppUser.UserCode;
                    labTestRequisitionBuyer.LTResultUpdateDate = DateTime.Now;
                    if (model.IsSend)
                    {
                        labTestRequisitionBuyer.IsSend = true;
                        labTestRequisitionBuyer.SendBy = AppUser.UserCode;
                        labTestRequisitionBuyer.SendDate = DateTime.Now;
                    }
                    if (child.IsApproved)
                    {
                        labTestRequisitionBuyer.IsApproved = true;
                        labTestRequisitionBuyer.ApprovedBy = AppUser.UserCode;
                        labTestRequisitionBuyer.ApprovedDate = DateTime.Now;
                    }
                    if (child.IsAcknowledge)
                    {
                        labTestRequisitionBuyer.IsAcknowledge = true;
                        labTestRequisitionBuyer.AcknowledgeBy = AppUser.UserCode;
                        labTestRequisitionBuyer.AcknowledgeDate = DateTime.Now;
                    }
                    labTestRequisitionBuyer.IsPass = child.IsPass;
                    labTestRequisitionBuyer.Remarks = child.Remarks;
                    labTestRequisitionBuyer.EntityState = EntityState.Modified;

                    int indexB = entity.LabTestRequisitionBuyers.FindIndex(x => x.LTReqBuyerID == child.LTReqBuyerID);
                    entity.LabTestRequisitionBuyers[indexB] = labTestRequisitionBuyer;

                    foreach (LabTestRequisitionBuyerParameter item in child.LabTestRequisitionBuyerParameters)
                    {
                        labTestRequisitionBuyerParameter = labTestRequisitionBuyer.LabTestRequisitionBuyerParameters.FirstOrDefault(x => x.LTReqBPID == item.LTReqBPID);
                        if (labTestRequisitionBuyerParameter == null)
                        {
                            labTestRequisitionBuyerParameter = item;
                            labTestRequisitionBuyerParameter.EntityState = EntityState.Added;
                            labTestRequisitionBuyer.LabTestRequisitionBuyerParameters.Add(labTestRequisitionBuyerParameter);
                        }
                        else
                        {
                            labTestRequisitionBuyerParameter.TestValue = item.TestValue;
                            labTestRequisitionBuyerParameter.TestValue1 = item.TestValue1;
                            labTestRequisitionBuyerParameter.Addition1 = item.Addition1;
                            labTestRequisitionBuyerParameter.Addition2 = item.Addition2;
                            labTestRequisitionBuyerParameter.AdditionalInfo = item.AdditionalInfo;
                            labTestRequisitionBuyerParameter.Requirement = item.Requirement;
                            labTestRequisitionBuyerParameter.Requirement1 = item.Requirement1;
                            labTestRequisitionBuyerParameter.RefValueFrom = item.RefValueFrom;
                            labTestRequisitionBuyerParameter.RefValueTo = item.RefValueTo;
                            labTestRequisitionBuyerParameter.IsPass = item.IsPass;
                            labTestRequisitionBuyerParameter.Remarks = item.Remarks;
                            labTestRequisitionBuyerParameter.IsParameterPass = item.IsParameterPass;
                            labTestRequisitionBuyerParameter.ParameterStatus = item.ParameterStatus;
                            labTestRequisitionBuyerParameter.ParameterRemarks = item.ParameterRemarks;
                            labTestRequisitionBuyerParameter.EntityState = EntityState.Modified;


                            int indexF = entity.LabTestRequisitionBuyers.FirstOrDefault(x => x.LTReqBuyerID == child.LTReqBuyerID)
                                .LabTestRequisitionBuyerParameters.FindIndex(x => x.LTReqBPID == item.LTReqBPID);

                            entity.LabTestRequisitionBuyers.FirstOrDefault(x => x.LTReqBuyerID == child.LTReqBuyerID)
                                .LabTestRequisitionBuyerParameters[indexF] = labTestRequisitionBuyerParameter;
                        }
                    }
                }
            }

            await _LabTestResultService.SaveAsync(entity);
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.LabTestResult, entity.ConceptID, "", entity.BookingID, 0, 0, entity.ColorID, entity.ItemMasterID);
            if (entity.LTReqMasterID > 0)
            {
                await _LabTestResultService.UpdateBDSTNA_TestReportPlanAsync(entity.LTReqMasterID);
            }
            return Ok(entity);
        }
        #endregion


    }
}