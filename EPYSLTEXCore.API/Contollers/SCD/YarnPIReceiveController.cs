using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Text.RegularExpressions;

namespace EPYSLTEX.Web.Controllers.Apis.SCD
{
    [Authorize]    
    [Route("api/ypi-receive")]
    public class YarnPIReceiveController : ApiBaseController
    {
        private readonly IYarnPIReceiveService _service;
        public YarnPIReceiveController(IUserService userService, IYarnPIReceiveService yarnPIReceiveService) : base(userService)
        {
            _service = yarnPIReceiveService;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> Get(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnPIReceiveMaster> records = await _service.GetAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));

            
        }

        [Route("cda-list")]
        [HttpGet]
        public async Task<IActionResult> GetCDAList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnPIReceiveMaster> records = await _service.GetCDAAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

    

        [HttpGet]
        [Route("proposallist")]
        public async Task<IActionResult> GetList(Status status,string lcNumber)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetAsync(status, paginationInfo, lcNumber);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("{id}/{supplierId}/{companyId}/{isYarnReceivePage}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, int supplierId, int companyId, bool isYarnReceivePage)
        {
            var record = await _service.GetAsync(id, supplierId, companyId, isYarnReceivePage);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }
        [Route("revise/{id}/{supplierId}/{companyId}/{isYarnReceivePage}/{poIds}")]
        [HttpGet]
        public async Task<IActionResult> GetRevise(int id, int supplierId, int companyId, bool isYarnReceivePage, string poIds)
        {
            var record = await _service.GetReviceAsync(id, supplierId, companyId, isYarnReceivePage, poIds);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }

        [Route("new/{yPOMasterId}/{supplierId}/{customerId}")]
        [HttpGet]
        public async Task<IActionResult> GetNew(int yPOMasterId, int supplierId, int customerId)
        {
            return Ok(await _service.GetNewAsync(yPOMasterId, supplierId, customerId));
        }

        [Route("new-cda/{yPOMasterId}/{supplierId}/{customerId}")]
        [HttpGet]
        public async Task<IActionResult> GetNewCDA(int yPOMasterId, int supplierId, int customerId)
        {
            return Ok(await _service.GetNewCDAAsync(yPOMasterId, supplierId, customerId));
        }

        [Route("yarn-po-items")]
        [HttpGet]
        public async Task<IActionResult> GetYarnPOItems(string ypoMasterIds, int yPIReceiveMasterID)
        {
            //var ids = Array.ConvertAll(ypoMasterIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), int.Parse);
            YarnPIReceiveMaster yarnPIReceiveMaster = await _service.GetYarnPIReceiveChildItemsAsync(ypoMasterIds, yPIReceiveMasterID);
            return Ok(yarnPIReceiveMaster);
        }

        [Route("cda-po-items")]
        [HttpGet]
        public async Task<IActionResult> GetCDAPOItems(string ypoMasterIds, int yPIReceiveMasterID)
        {
            YarnPIReceiveMaster yarnPIReceiveMaster = await _service.GetCDAPIReceiveChildItemsAsync(ypoMasterIds, yPIReceiveMasterID);
            return Ok(yarnPIReceiveMaster);
        }

        [Route("available-po-for-pi")]
        [HttpGet]
        public async Task<IActionResult> GetAvailablePOForPI(string poMasterIds, int supplierId, int companyId, int yPIReceiveMasterID)
        {
            var poMasterIdArray = Array.ConvertAll(poMasterIds.Split(','), int.Parse);
            return Ok(await _service.GetAvailablePOForPIAsync(poMasterIdArray, supplierId, companyId, yPIReceiveMasterID));
        }

        [Route("available-cda-po-for-pi")]
        [HttpGet]
        public async Task<IActionResult> GetAvailableCDAPOForPI(string poMasterIds, int supplierId, int companyId, int yPIReceiveMasterID)
        {
            var poMasterIdArray = Array.ConvertAll(poMasterIds.Split(','), int.Parse);
            return Ok(await _service.GetAvailableCDAPOForPIAsync(poMasterIdArray, supplierId, companyId, yPIReceiveMasterID));
        }

        //[Route("save")]
        //[HttpPost]
        //public async Task<IActionResult> SaveYarnPIReceive()
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("No file uploaded or the file is empty.");

        //    if (file.Length > 4 * 1024 * 1024)
        //        return BadRequest("File is bigger than 4MB.");

        //    var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());

           
        //    //if (!provider.Files.Any()) return BadRequest("You must upload PI file.");

        //    var formData = provider.FormData;

        //    YarnPIReceiveMaster model = formData.ConvertToObject<YarnPIReceiveMaster>();

        //    if (!model.IsRevise)
        //    {
        //        if(model.isFileExist == null) { 
        //        if (!provider.Files.Any()) return BadRequest("You must upload PI file.");
        //        }
        //    }
        //    model.YarnPOMasterRevision = formData.Get("YarnPOMasterRevision").ToInt();
        //    model.Childs = JsonConvert.DeserializeObject<List<YarnPIReceiveChild>>(formData.Get("Childs"));
        //    model.YarnPIReceivePOList = JsonConvert.DeserializeObject<List<YarnPIReceivePO>>(formData.Get("YarnPIReceivePOList"));
        //    model.YarnPIReceiveAdditionalValueList = JsonConvert.DeserializeObject<List<YarnPIReceiveAdditionalValue>>(formData.Get("YarnPIReceiveAdditionalValueList"));
        //    model.YarnPIReceiveDeductionValueList = JsonConvert.DeserializeObject<List<YarnPIReceiveDeductionValue>>(formData.Get("YarnPIReceiveDeductionValueList"));







        //    #region Save Image

        //    string filePath = "";
        //    string previewTemplate = "";

        //    if (Request.Form.Files.Any())
        //    {
        //        var originalFile = Request.Form.Files[0];
        //        var fileName = Path.GetInvalidFileNameChars().Aggregate(
        //            originalFile.FileName,
        //            (current, c) => current.Replace(c.ToString(), "")
        //        );
        //        fileName = GetValidFileName(fileName);

        //        var contentType = originalFile.ContentType;
        //        var fileExtension = Path.GetExtension(fileName);

        //        previewTemplate = fileExtension.Contains(".pdf", StringComparison.OrdinalIgnoreCase)
        //            ? "pdf"
        //            : contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
        //                ? "image"
        //                : "office";

        //        filePath = $"{AppConstants.YARN_PI_FILE_PATH}/{string.Join("_", model.YPINo.Split(Path.GetInvalidFileNameChars()))}_{fileName}";

        //        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
        //        Directory.CreateDirectory(Path.GetDirectoryName(savePath) ?? string.Empty);

        //        using (var fileStream = new FileStream(savePath, FileMode.Create))
        //        {
        //            await originalFile.CopyToAsync(fileStream);
        //        }
        //    }
        //    else
        //    {
        //        filePath = model.isFileExist;
        //        var fileExtension = Path.GetExtension(filePath);

        //        previewTemplate = fileExtension.Contains(".pdf", StringComparison.OrdinalIgnoreCase)
        //            ? "pdf"
        //            : fileExtension.StartsWith(".") && MimeTypes.GetMimeType(filePath).StartsWith("image/")
        //                ? "image"
        //                : "office";
        //    }

        //    #endregion Save Image


        //    model.Childs.ForEach(pc =>
        //    {
        //        decimal PIQty = pc.PIQty;
        //        //List<YarnPIReceivePO> childList = model.YarnPIReceivePOList.FindAll(x => x.ItemMasterID == pc.ItemMasterID && x.Rate == pc.Rate);
        //        List<YarnPIReceivePO> childList = model.YarnPIReceivePOList.FindAll(x => x.YPOChildID == pc.YPOChildID);

        //        foreach (YarnPIReceivePO l in childList)
        //        {
        //            if (PIQty == 0)
        //            {
        //                l.PIQty = 0;
        //            }
        //            else
        //            {
        //                if (l.BalancePOQty == PIQty)
        //                {
        //                    l.PIQty = PIQty;
        //                    PIQty = 0;
        //                }
        //                //------------------------
        //                else if (l.BalancePOQty == 0)
        //                {
        //                    //PIQty = l.PIQty;
        //                    PIQty = 0;
        //                }
        //                //------------------------
        //                else if (l.BalancePOQty < PIQty)
        //                {
        //                    l.PIQty = l.BalancePOQty;
        //                    PIQty -= l.BalancePOQty;
        //                }
        //                else
        //                {
        //                    l.PIQty = PIQty;
        //                    PIQty = 0;
        //                }
        //            }
        //        }

        //        // if PIQty > total distribution qty then we add remaning qty in last row
        //        if (PIQty > 0)
        //        {
        //            //YarnPIReceivePO child = model.YarnPIReceivePOList.FindLast(x => x.ItemMasterID == pc.ItemMasterID);
        //            YarnPIReceivePO child = model.YarnPIReceivePOList.FindLast(x => x.YPOChildID == pc.YPOChildID);
        //            if (child != null) child.PIQty += PIQty;
        //        }
        //    });


        //    YarnPIReceiveMaster entity;
        //    if (model.YPIReceiveMasterID > 0)
        //    {
        //        entity = await _service.GetAllByIDAsync(model.YPIReceiveMasterID);

        //        entity.YPINo = model.YPINo;
        //        entity.PIDate = model.PIDate;
        //        //entity.RevisionNo = model.RevisionNo;
        //        //entity.RevisionDate = model.RevisionDate;
        //        entity.RevisionNo = model.RevisionNo + 1;
        //        entity.RevisionDate = DateTime.Now;
        //        entity.SupplierID = model.SupplierID;
        //        entity.CompanyID = model.CompanyID;
        //        entity.Remarks = model.Remarks;
        //        if (provider.Files.Any())
        //        {
        //            entity.PIFilePath = filePath;
        //            entity.AttachmentPreviewTemplate = previewTemplate;
        //            //entity.PreProcessRevNo = entity.YarnPOMasterRevision;
        //        }
        //        entity.NetPIValue = model.NetPIValue;
        //        entity.IncoTermsID = model.IncoTermsID;
        //        entity.TypeOfLCID = model.TypeOfLCID;
        //        entity.TenureofLC = model.TenureofLC;
        //        entity.CalculationofTenure = model.CalculationofTenure;
        //        entity.CreditDays = model.CreditDays;
        //        entity.OfferValidity = model.OfferValidity;
        //        entity.ReImbursementCurrencyID = model.ReImbursementCurrencyID;
        //        entity.Charges = model.Charges;
        //        entity.CountryOfOriginID = model.CountryOfOriginID;
        //        entity.TransShipmentAllow = model.TransShipmentAllow;
        //        entity.ShippingTolerance = model.ShippingTolerance;
        //        entity.PortofLoadingID = model.PortofLoadingID;
        //        entity.PortofDischargeID = model.PortofDischargeID;
        //        entity.ShipmentModeID = model.ShipmentModeID;
        //        entity.PONo = model.PONo;
        //        entity.UpdatedBy = AppUser.UserCode;
        //        entity.DateUpdated = DateTime.Now;
        //        entity.Acknowledge = false;
        //        entity.UnAcknowledge = false;
        //        entity.Accept = false;
        //        entity.EntityState = EntityState.Modified;

        //        entity.Childs.SetUnchanged();
        //        entity.YarnPIReceivePOList.SetUnchanged();
        //        entity.YarnPIReceiveAdditionalValueList.SetUnchanged();
        //        entity.YarnPIReceiveDeductionValueList.SetUnchanged();

        //        foreach (YarnPIReceiveChild item in model.Childs)
        //        {
        //            //YarnPIReceiveChild child = entity.Childs.FirstOrDefault(x => x.ItemMasterID == item.ItemMasterID
        //            //                                                          && x.ShadeCode == item.ShadeCode);
        //            YarnPIReceiveChild child = entity.Childs.FirstOrDefault(x => x.YPIReceiveChildID == item.YPIReceiveChildID);

        //            if (child == null)
        //            {
        //                child = item;
        //                child.YPIReceiveMasterID = entity.YPIReceiveMasterID;
        //                entity.Childs.Add(child);
        //            }
        //            else
        //            {
        //                // Assigning all properties
        //                // Values can be changed because multiple PO can be joined together
        //                child.ItemMasterID = item.ItemMasterID;
        //                child.ShadeCode = item.ShadeCode;
        //                child.YarnProgramID = item.YarnProgramID;
        //                child.YarnCategory = item.YarnCategory;
        //                child.UnitID = item.UnitID;
        //                child.POQty = item.POQty;
        //                child.Rate = item.Rate;
        //                child.PIQty = item.PIQty;
        //                child.PIValue = item.PIValue;
        //                child.Remarks = item.Remarks;
        //                child.YarnLotNo = item.YarnLotNo;
        //                child.HSCode = item.HSCode;
        //                child.YPOChildID = item.YPOChildID;
        //                child.EntityState = EntityState.Modified;
        //            }
        //        }
        //        if (model.IsRevise)
        //        {
        //            YarnPIReceivePO yarnPIReceivePO;
        //            foreach (YarnPIReceivePO item in model.YarnPIReceivePOList)
        //            {
        //                yarnPIReceivePO = entity.YarnPIReceivePOList.FirstOrDefault(x => x.YPIReceiveMasterID == item.YPIReceiveMasterID && x.YPOChildID == item.YPOChildID);
        //                if (yarnPIReceivePO == null)
        //                {
        //                    yarnPIReceivePO = item;
        //                    yarnPIReceivePO.YPIReceiveMasterID = entity.YPIReceiveMasterID;
        //                    entity.YarnPIReceivePOList.Add(yarnPIReceivePO);
        //                }
        //                else
        //                {
        //                    yarnPIReceivePO.PIQty = item.PIQty;
        //                    yarnPIReceivePO.EntityState = EntityState.Modified;
        //                }
        //            }
        //        }
        //        else
        //        {

        //            if (model.YarnPIReceivePOList != null) //model.YarnPIReceivePOList
        //            {
        //                YarnPIReceivePO yarnPIReceivePO;
        //                foreach (YarnPIReceivePO item in model.YarnPIReceivePOList) //model.YarnPIReceivePOList
        //                {
        //                    //yarnPIReceivePO = entity.YarnPIReceivePOList.FirstOrDefault(x => x.YPOMasterID == item.YPOMasterID && x.ItemMasterID == item.ItemMasterID);
        //                    yarnPIReceivePO = entity.YarnPIReceivePOList.FirstOrDefault(x => x.YPIReceiveMasterID == item.YPIReceiveMasterID && x.YPOMasterID == item.YPOMasterID && x.YPOChildID == item.YPOChildID);
        //                    if (yarnPIReceivePO == null)
        //                    {
        //                        yarnPIReceivePO = item;
        //                        yarnPIReceivePO.YPIReceiveMasterID = entity.YPIReceiveMasterID;
        //                        yarnPIReceivePO.RevisionNo = model.YarnPOMasterRevision;
        //                        entity.YarnPIReceivePOList.Add(yarnPIReceivePO);
        //                    }
        //                    else
        //                    {
        //                        //yarnPIReceivePO.PIQty = entity.Childs.Where(x => x.YPIReceiveMasterID == item.YPIReceiveMasterID && x.ItemMasterID == item.ItemMasterID).Select(x => x.PIQty).FirstOrDefault();
        //                        yarnPIReceivePO.PIQty = entity.Childs.Where(x => x.YPIReceiveMasterID == item.YPIReceiveMasterID && x.YPOChildID == item.YPOChildID).Select(x => x.PIQty).FirstOrDefault();
        //                        yarnPIReceivePO.RevisionNo = model.YarnPOMasterRevision;
        //                        yarnPIReceivePO.ItemMasterID = item.ItemMasterID;
        //                        yarnPIReceivePO.EntityState = EntityState.Modified;
        //                    }
        //                }
        //            }
        //        }

        //        YarnPIReceiveAdditionalValue yarnPIReceiveAdditionalValue;
        //        foreach (YarnPIReceiveAdditionalValue item in model.YarnPIReceiveAdditionalValueList)
        //        {
        //            yarnPIReceiveAdditionalValue = entity.YarnPIReceiveAdditionalValueList.FirstOrDefault(x => x.AdditionalValueID == item.AdditionalValueID);
        //            if (yarnPIReceiveAdditionalValue == null)
        //            {
        //                yarnPIReceiveAdditionalValue = item;
        //                item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
        //                entity.YarnPIReceiveAdditionalValueList.Add(yarnPIReceiveAdditionalValue);
        //            }
        //            else
        //            {
        //                yarnPIReceiveAdditionalValue.AdditionalValue = item.AdditionalValue;
        //                yarnPIReceiveAdditionalValue.EntityState = EntityState.Modified;
        //            }
        //        }

        //        YarnPIReceiveDeductionValue pIReceiveDeductionValue;
        //        foreach (YarnPIReceiveDeductionValue item in model.YarnPIReceiveDeductionValueList)
        //        {
        //            pIReceiveDeductionValue = entity.YarnPIReceiveDeductionValueList.FirstOrDefault(x => x.DeductionValueID == item.DeductionValueID);
        //            if (pIReceiveDeductionValue == null)
        //            {
        //                pIReceiveDeductionValue = item;
        //                pIReceiveDeductionValue.YPIReceiveMasterID = entity.YPIReceiveMasterID;
        //                entity.YarnPIReceiveDeductionValueList.Add(pIReceiveDeductionValue);
        //            }
        //            else
        //            {
        //                pIReceiveDeductionValue.DeductionValue = item.DeductionValue;
        //                pIReceiveDeductionValue.EntityState = EntityState.Modified;
        //            }
        //        }

        //        if (entity.Reject)
        //        {
        //            entity.NeedsReview = true;
        //            entity.Reject = false;
        //            entity.RejectBy = 0;
        //            entity.RejectDate = null;
        //            entity.RejectReason = "";
        //        }
        //        entity.Childs.Where(c => c.EntityState == EntityState.Unchanged).ToList().ForEach(c =>
        //        {
        //            c.EntityState = EntityState.Deleted;
        //        });
        //    }
        //    else
        //    {
        //        entity = CommonFunction.DeepClone(model);

        //        if (!model.IsRevise)
        //        {
        //            entity.PIFilePath = filePath;
        //            entity.AttachmentPreviewTemplate = previewTemplate;
        //        }
        //        entity.PreProcessRevNo = model.YarnPOMasterRevision;
        //        entity.AddedBy = AppUser.UserCode;
        //        entity.ReceivePI = true;
        //        entity.ReceivePIDate = DateTime.Now;
        //        entity.ReceivePIBy = AppUser.UserCode;
        //        entity.NeedsReview = true;


        //        YarnPIReceivePO yarnPIReceivePO;


        //        foreach (YarnPIReceivePO item in model.YarnPIReceivePOList)
        //        {
        //            //yarnPIReceivePO = entity.YarnPIReceivePOList.FirstOrDefault(x => x.YPOMasterID == item.YPOMasterID && x.ItemMasterID == item.ItemMasterID && x.YPOChildID == item.YPOChildID);
        //            yarnPIReceivePO = entity.YarnPIReceivePOList.FirstOrDefault(x => x.YPOMasterID == item.YPOMasterID && x.YPOChildID == item.YPOChildID);

        //            if (yarnPIReceivePO != null)
        //            {
        //                yarnPIReceivePO.RevisionNo = model.Childs.Find(x => x.YPOChildID == item.YPOChildID).RevisionNo;
        //            }
        //        }

        //    }

        //    if (model.IsRevise)
        //    {
        //        entity.Reject = false;
        //        entity.RejectBy = 0;
        //        entity.RejectDate = null;
        //        entity.RejectReason = "";

        //        entity.Acknowledge = false;
        //        entity.UnAcknowledge = false;
        //        //entity.PreProcessRevNo = entity.RevisionNo;
        //        entity.RevisionNo = entity.RevisionNo + 1;
        //        entity.RevisionDate = DateTime.Now;
        //        entity.IsRevise = true;
        //    }

        //    await _service.SaveAsync(entity);

        //    return Ok(entity.YPINo);
        //}

        [Route("acknowledge")]
        [HttpPost]
        public async Task<IActionResult> SaveAcknowledge(YarnPIReceiveMaster model)
        {
            YarnPIReceiveMaster entity;
            entity = await _service.GetAllByIDAsync(model.YPIReceiveMasterID);
            if (model.Acknowledge == false)
            {
                entity.UnAcknowledge = true;
                entity.Acknowledge = false;
                entity.UnAcknowledgeBy = AppUser.UserCode;
                entity.UnAcknowledgeDate = DateTime.Now;
                entity.EntityState = EntityState.Modified;

            }
            else
            {
                entity.Acknowledge = true;
                entity.AcknowledgeBy = AppUser.UserCode;
                entity.AcknowledgeDate = DateTime.Now;
                entity.EntityState = EntityState.Modified;

            }

            await _service.UpdateEntityAsync(entity);
            return Ok();
        }


        #region revision this will be fixed
        //[Route("revision")]
        //[HttpPost]
        //public async Task<IActionResult> RevisionYarnPIReceive()
        //{
        //    if (!Request.Content.IsMimeMultipartContent())
        //        return BadRequest("Unsupported media type.");

        //    if (Request.Content.Headers.ContentLength >= 4 * 1024 * 1024)
        //        return BadRequest("File is bigger than 4MB");

        //    var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());

        //    if (!provider.Files.Any()) return BadRequest("You must upload PI file.");

        //    var formData = provider.FormData;

        //    YarnPIReceiveMaster model = formData.ConvertToObject<YarnPIReceiveMaster>();
        //    model.Childs = JsonConvert.DeserializeObject<List<YarnPIReceiveChild>>(formData.Get("Childs"));
        //    model.YarnPIReceivePOList = JsonConvert.DeserializeObject<List<YarnPIReceivePO>>(formData.Get("YarnPIReceivePOList"));
        //    model.YarnPIReceiveAdditionalValueList = JsonConvert.DeserializeObject<List<YarnPIReceiveAdditionalValue>>(formData.Get("YarnPIReceiveAdditionalValueList"));
        //    model.YarnPIReceiveDeductionValueList = JsonConvert.DeserializeObject<List<YarnPIReceiveDeductionValue>>(formData.Get("YarnPIReceiveDeductionValueList"));


        //    #region Save Image

        //    var filePath = "";
        //    var previewTemplate = "";
        //        if (provider.Files.Any())
        //        {
        //            var originalFile = provider.Files[0];
        //            var inputStream = await originalFile.ReadAsStreamAsync();

        //            var fileName = string.Join("", originalFile.Headers.ContentDisposition.FileName.Split(Path.GetInvalidFileNameChars()));
        //            var ext = System.IO.Path.GetExtension(fileName);
        //            var fileNameWithouExtension = fileName.Replace(ext, "");
        //            fileName = fileNameWithouExtension + "_" + Guid.NewGuid().ToString("N") + "." + ext;
        //            var contentType = originalFile.Headers.ContentType.ToString();

        //            var fileExtension = Path.GetExtension(fileName);
        //            previewTemplate = fileExtension.Contains(".pdf") ? "pdf" : MimeMapping.GetMimeMapping(fileName).StartsWith("image/") ? "image" : "office";

        //            //string yPINo = model.YPINo.Replace("/", "_");
        //            string yPINo = Regex.Replace(model.YPINo, "[/:]", "_");

        //            filePath = $"{AppConstants.YARN_PI_FILE_PATH}/{yPINo}_{fileName}";
        //            var savePath = HttpContext.Current.Server.MapPath(filePath);
        //            using (var fileStream = File.Create(savePath))
        //            {
        //                inputStream.Seek(0, SeekOrigin.Begin);
        //                await inputStream.CopyToAsync(fileStream);
        //            }
        //        }

        //    #endregion Save Image

        //    model.Childs.ForEach(pc =>
        //    {
        //        decimal PIQty = pc.PIQty;
        //        List<YarnPIReceivePO> childList = model.YarnPIReceivePOList.FindAll(x => x.ItemMasterID == pc.ItemMasterID && x.Rate == pc.Rate);

        //        foreach (YarnPIReceivePO l in childList)
        //        {
        //            if (PIQty == 0)
        //            {
        //                l.PIQty = 0;
        //            }
        //            else
        //            {
        //                if (l.BalancePOQty == PIQty)
        //                {
        //                    l.PIQty = PIQty;
        //                    PIQty = 0;
        //                }
        //                //------------------------
        //                else if (l.BalancePOQty == 0)
        //                {
        //                    //PIQty = l.PIQty;
        //                    PIQty = 0;
        //                }
        //                //------------------------
        //                else if (l.BalancePOQty < PIQty)
        //                {
        //                    l.PIQty = l.BalancePOQty;
        //                    PIQty -= l.BalancePOQty;
        //                }
        //                else
        //                {
        //                    l.PIQty = PIQty;
        //                    PIQty = 0;
        //                }
        //            }
        //        }

        //        // if PIQty > total distribution qty then we add remaning qty in last row
        //        if (PIQty > 0)
        //        {
        //            YarnPIReceivePO child = model.YarnPIReceivePOList.FindLast(x => x.ItemMasterID == pc.ItemMasterID);
        //            if (child != null) child.PIQty += PIQty;
        //        }
        //    });


        //    YarnPIReceiveMaster entity;
        //    if (model.YPIReceiveMasterID > 0)
        //    {
        //        entity = await _service.GetAllByIDAsync(model.YPIReceiveMasterID);

        //        entity.YPINo = model.YPINo;
        //        entity.PIDate = model.PIDate;
        //        //entity.RevisionNo = entity.RevisionNo+1;
        //        entity.RevisionDate = model.RevisionDate;
        //        entity.SupplierID = model.SupplierID;
        //        entity.CompanyID = model.CompanyID;
        //        entity.Remarks = model.Remarks;

        //        entity.PIFilePath = filePath;
        //        entity.AttachmentPreviewTemplate = previewTemplate;

        //        entity.NetPIValue = model.NetPIValue;
        //        entity.IncoTermsID = model.IncoTermsID;
        //        entity.TypeOfLCID = model.TypeOfLCID;
        //        entity.TenureofLC = model.TenureofLC;
        //        entity.CalculationofTenure = model.CalculationofTenure;
        //        entity.CreditDays = model.CreditDays;
        //        entity.OfferValidity = model.OfferValidity;
        //        entity.ReImbursementCurrencyID = model.ReImbursementCurrencyID;
        //        entity.Charges = model.Charges;
        //        entity.CountryOfOriginID = model.CountryOfOriginID;
        //        entity.TransShipmentAllow = model.TransShipmentAllow;
        //        entity.ShippingTolerance = model.ShippingTolerance;
        //        entity.PortofLoadingID = model.PortofLoadingID;
        //        entity.PortofDischargeID = model.PortofDischargeID;
        //        entity.ShipmentModeID = model.ShipmentModeID;
        //        entity.PONo = model.PONo;
        //        entity.UpdatedBy = AppUser.UserCode;
        //        entity.DateUpdated = DateTime.Now;
        //        entity.EntityState = EntityState.Modified;
        //        entity.IsRevise = model.IsRevise;
        //        entity.Childs.SetUnchanged();
        //        entity.YarnPIReceivePOList.SetUnchanged();
        //        entity.YarnPIReceiveAdditionalValueList.SetUnchanged();
        //        entity.YarnPIReceiveDeductionValueList.SetUnchanged();

        //        foreach (YarnPIReceiveChild item in model.Childs)
        //        {
        //            YarnPIReceiveChild child = entity.Childs.FirstOrDefault(x => x.ItemMasterID == item.ItemMasterID
        //                                                                      && x.ShadeCode == item.ShadeCode && x.Rate == item.Rate);
        //            if (child == null)
        //            {
        //                child = item;
        //                child.YPIReceiveMasterID = entity.YPIReceiveMasterID;
        //                entity.Childs.Add(child);
        //            }
        //            else
        //            {
        //                // Assigning all properties
        //                // Values can be changed because multiple PO can be joined together
        //                child.ItemMasterID = item.ItemMasterID;
        //                child.ShadeCode = item.ShadeCode;
        //                child.YarnProgramID = item.YarnProgramID;
        //                child.YarnCategory = item.YarnCategory;
        //                child.UnitID = item.UnitID;
        //                child.POQty = item.POQty;
        //                child.Rate = item.Rate;
        //                child.PIQty = item.PIQty;
        //                child.PIValue = item.PIValue;
        //                child.Remarks = item.Remarks;
        //                child.YarnLotNo = item.YarnLotNo;
        //                child.HSCode = item.HSCode;
        //                child.EntityState = EntityState.Modified;
        //            }
        //        }

        //        YarnPIReceivePO yarnPIReceivePO;
        //        foreach (YarnPIReceivePO item in model.YarnPIReceivePOList)
        //        {
        //            yarnPIReceivePO = entity.YarnPIReceivePOList.FirstOrDefault(x => x.YPOMasterID == item.YPOMasterID && x.ItemMasterID == item.ItemMasterID);
        //            if (yarnPIReceivePO == null)
        //            {
        //                yarnPIReceivePO = item;
        //                yarnPIReceivePO.YPIReceiveMasterID = entity.YPIReceiveMasterID;
        //                yarnPIReceivePO.RevisionNo = item.MasterRevisioNo;
        //                entity.YarnPIReceivePOList.Add(yarnPIReceivePO);
        //            }
        //            else
        //            {
        //                yarnPIReceivePO.ItemMasterID = item.ItemMasterID;
        //                yarnPIReceivePO.PIQty = item.PIQty;
        //                yarnPIReceivePO.YPIReceiveMasterID = item.YPIReceiveMasterID;
        //                yarnPIReceivePO.YPOMasterID = item.YPOMasterID;
        //                yarnPIReceivePO.RevisionNo = item.MasterRevisioNo;
        //                yarnPIReceivePO.EntityState = EntityState.Modified;
        //            }
        //        }

        //        YarnPIReceiveAdditionalValue yarnPIReceiveAdditionalValue;
        //        foreach (YarnPIReceiveAdditionalValue item in model.YarnPIReceiveAdditionalValueList)
        //        {
        //            yarnPIReceiveAdditionalValue = entity.YarnPIReceiveAdditionalValueList.FirstOrDefault(x => x.AdditionalValueID == item.AdditionalValueID);
        //            if (yarnPIReceiveAdditionalValue == null)
        //            {
        //                yarnPIReceiveAdditionalValue = item;
        //                item.YPIReceiveMasterID = entity.YPIReceiveMasterID;
        //                entity.YarnPIReceiveAdditionalValueList.Add(yarnPIReceiveAdditionalValue);
        //            }
        //            else
        //            {
        //                yarnPIReceiveAdditionalValue.AdditionalValue = item.AdditionalValue;
        //                yarnPIReceiveAdditionalValue.EntityState = EntityState.Modified;
        //            }
        //        }

        //        YarnPIReceiveDeductionValue pIReceiveDeductionValue;
        //        foreach (YarnPIReceiveDeductionValue item in model.YarnPIReceiveDeductionValueList)
        //        {
        //            pIReceiveDeductionValue = entity.YarnPIReceiveDeductionValueList.FirstOrDefault(x => x.DeductionValueID == item.DeductionValueID);
        //            if (pIReceiveDeductionValue == null)
        //            {
        //                pIReceiveDeductionValue = item;
        //                pIReceiveDeductionValue.YPIReceiveMasterID = entity.YPIReceiveMasterID;
        //                entity.YarnPIReceiveDeductionValueList.Add(pIReceiveDeductionValue);
        //            }
        //            else
        //            {
        //                pIReceiveDeductionValue.DeductionValue = item.DeductionValue;
        //                pIReceiveDeductionValue.EntityState = EntityState.Modified;
        //            }
        //        }

        //        if (entity.Reject)
        //        {
        //            entity.NeedsReview = true;
        //            entity.Reject = false;
        //            entity.RejectBy = 0;
        //            entity.RejectDate = null;
        //            entity.RejectReason = "";
        //        }
        //        entity.Childs.Where(c => c.EntityState == EntityState.Unchanged).ToList().ForEach(c =>
        //        {
        //            c.EntityState = EntityState.Deleted;
        //        });
        //    }
        //    else
        //    {
        //        entity = CommonFunction.DeepClone(model);
        //        //entity.PIFilePath = filePath;
        //        //entity.AttachmentPreviewTemplate = previewTemplate;
        //        entity.AddedBy = AppUser.UserCode;
        //        entity.ReceivePI = true;
        //        entity.ReceivePIDate = DateTime.Now;
        //        entity.ReceivePIBy = AppUser.UserCode;
        //        entity.NeedsReview = true;
        //    }

        //    if (model.IsRevise)
        //    {
        //        entity.Reject = false;
        //        entity.RejectBy = 0;
        //        entity.RejectDate = null;
        //        entity.RejectReason = "";

        //        entity.Accept = false;
        //        entity.AcceptBy = 0;
        //        entity.AcceptDate = null;

        //        entity.Acknowledge = false;
        //        entity.UnAcknowledge = false;
        //        entity.RevisionNo = entity.RevisionNo + 1;
        //        entity.RevisionDate = DateTime.Now;
        //    }

        //    await _service.SaveAsync(entity);

        //    return Ok(entity.YPINo);
        //}

        #endregion
        private string GetValidFileName(string fileName)
        {
            fileName = fileName.Replace("#", " ")
                               .Replace("?", "");
            fileName = Regex.Replace(fileName, @"[^\u0000-\u007F]+", string.Empty);
            return fileName;
        }
    }
}