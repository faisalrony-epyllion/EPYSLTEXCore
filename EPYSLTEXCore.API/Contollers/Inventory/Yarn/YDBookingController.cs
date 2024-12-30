using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yd-booking")]
    public class YDBookingController : ApiBaseController
    {
        private readonly IYarnDyeingBookingService _yarnDyeingBookingService;
        private readonly ICommonHelperService _commonService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        //private readonly IEmailService _emailService;
        //private readonly IReportingService _reportingService;
        public YDBookingController(IUserService userService,
            IYarnDyeingBookingService yarnDyeingBookingService
            , IWebHostEnvironment hostingEnvironment
            , ICommonHelperService commonService
            //, IEmailService emailService
            //, IReportingService reportingService
            ) : base(userService)
        {
            _yarnDyeingBookingService = yarnDyeingBookingService;
            _commonService = commonService;
            _hostingEnvironment = hostingEnvironment;
            //_emailService = emailService;
            //_reportingService = reportingService;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, string pageName)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDBookingMaster> records = await _yarnDyeingBookingService.GetPagedAsync(status, paginationInfo, pageName);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("yarn/{id}/{pName}")]
        public async Task<IActionResult> GetYarn(string id, string pName)
        {
            return Ok(await _yarnDyeingBookingService.GetNew(id, pName));
        }

        [HttpGet]
        [Route("{id}/{pName}")]
        public async Task<IActionResult> Get(int id, string pName)
        {
            return Ok(await _yarnDyeingBookingService.GetAsync(id, pName));
        }
        [HttpGet]
        [Route("revise/{id}/{groupConceptNo}/{pName}")]
        public async Task<IActionResult> GetRevise(int id, string groupConceptNo, string pName)
        {
            return Ok(await _yarnDyeingBookingService.GetReviseAsync(id, groupConceptNo, pName));
        }

        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> Save()
        {
            //if (!Request.Content.IsMimeMultipartContent()) return BadRequest("Unsupported media type.");
            //if (Request.Content.Headers.ContentLength >= 4 * 1024 * 1024) return BadRequest("File is bigger than 4MB");
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null)
                return BadRequest("File Not Found.");
            if (file.Length > 4 * 1024 * 1024)
                return BadRequest("File is bigger than 4MB.");

            (YDBookingMaster, string, string) processData = await ProcessFormDataAsync(Request.Form);
            YDBookingMaster model = processData.Item1;
            string filePath = processData.Item2;
            string previewTemplate = processData.Item3;


            YDBookingMaster entity;
            if (model.IsModified)
            {
                entity = await _yarnDyeingBookingService.GetAllAsync(model.YDBookingMasterID);

                entity.BuyerID = model.BuyerID;
                entity.Remarks = model.Remarks;
                entity.SendForApproval = model.SendForApproval;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;

                if (model.RevisionStatus == "Revision")
                {
                    entity.IsApprove = false;
                    entity.IsAcknowledge = false;
                    entity.SendForApproval = false;
                    entity.PreProcessRevNo = model.PreProcessRevNo;// entity.RevisionNo;
                    entity.RevisionNo = entity.RevisionNo + 1;
                    entity.RevisionDate = DateTime.Now;
                    entity.RevisionBy = AppUser.UserCode;
                    entity.RevisionReason = model.RevisionReason;
                }

                entity.EntityState = EntityState.Modified;

                if (filePath.NotNullOrEmpty())
                {
                    if (entity.SwatchFilePath.NotNullOrEmpty()) // Delete existing file from disc
                    {
                        //var oldPath = HttpContext.Current.Server.MapPath(entity.SwatchFilePath);
                        //File.Delete(oldPath);
                        var oldPath = Path.Combine(_hostingEnvironment.WebRootPath, entity.SwatchFilePath);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath); // Delete the existing file from disk
                        }
                    }

                    entity.SwatchFilePath = filePath;
                    entity.PreviewTemplate = previewTemplate;
                }

                #region YDBookingChild

                entity.YDBookingChilds.SetUnchanged();
                entity.YDBookingChilds.ForEach(x => x.PrintColors.SetUnchanged());
                entity.YDBookingChilds.ForEach(x => x.YDBookingChildUsesIns.SetUnchanged());

                YDBookingChild childEntity;
                foreach (YDBookingChild item in model.YDBookingChilds)
                {
                    childEntity = entity.YDBookingChilds.FirstOrDefault(x => x.YDBookingChildID == item.YDBookingChildID);
                    if (childEntity == null)
                    {
                        childEntity = item;
                        childEntity.EntityState = EntityState.Added;
                        entity.YDBookingChilds.Add(childEntity);
                    }
                    else
                    {
                        childEntity.ItemMasterID = item.ItemMasterID;
                        childEntity.ColorID = item.ColorID;
                        childEntity.ColorCode = item.ColorCode;
                        childEntity.YarnProgramID = item.YarnProgramID;
                        childEntity.BookingQty = item.BookingQty;
                        childEntity.NoOfCone = item.NoOfCone;
                        childEntity.PerConeKG = item.PerConeKG;
                        childEntity.UnitID = item.UnitID;
                        childEntity.YarnDyedColorID = item.YarnDyedColorID;
                        childEntity.Remarks = item.Remarks;
                        //childEntity.YarnCategory = item.YarnCategory;
                        childEntity.NoOfThread = item.NoOfThread;
                        childEntity.ShadeCode = item.ShadeCode;
                        childEntity.BookingFor = item.BookingFor;
                        childEntity.IsTwisting = item.IsTwisting;
                        childEntity.IsWaxing = item.IsWaxing;
                        childEntity.UsesIn = item.UsesIn;
                        childEntity.IsAdditionalItem = item.IsAdditionalItem;
                        childEntity.SpinnerID = item.SpinnerID;
                        childEntity.LotNo = item.LotNo;
                        childEntity.PhysicalCount = item.PhysicalCount;
                        childEntity.ColorBatchRefID = item.ColorBatchRefID;
                        childEntity.ColorBatchRef = item.ColorBatchRef;
                        //childEntity.YD = item.YD;
                        //childEntity.YDItem = item.YDItem;
                        childEntity.EntityState = EntityState.Modified;

                        foreach (var printColor in item.PrintColors)
                        {
                            YDBookingPrintColor colorEntity = childEntity.PrintColors.Find(x => x.PrintColorID == printColor.PrintColorID && x.ColorID == printColor.ColorID);
                            if (colorEntity == null)
                            {
                                colorEntity = printColor;
                                colorEntity.EntityState = EntityState.Added;
                                childEntity.PrintColors.Add(colorEntity);
                            }
                            else
                            {
                                colorEntity.ColorID = printColor.ColorID;
                                colorEntity.ColorCode = printColor.ColorCode;
                                colorEntity.EntityState = EntityState.Modified;
                            }
                        }
                        childEntity.PrintColors.FindAll(x => x.EntityState == EntityState.Unchanged).SetDeleted();

                        foreach (var uses in item.YDBookingChildUsesIns)
                        {
                            YDBookingChildUsesIn usesEntity = childEntity.YDBookingChildUsesIns.Find(x => x.YDBCUsesInID == uses.YDBCUsesInID && x.YDBookingChildID == childEntity.YDBookingChildID);
                            if (usesEntity == null)
                            {
                                usesEntity = uses;
                                usesEntity.EntityState = EntityState.Added;
                                childEntity.YDBookingChildUsesIns.Add(usesEntity);
                            }
                            else
                            {
                                usesEntity.UsesIn = uses.UsesIn;
                                usesEntity.EntityState = EntityState.Modified;
                            }
                        }
                        childEntity.YDBookingChildUsesIns.FindAll(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                    }
                }

                entity.YDBookingChilds.FindAll(x => x.EntityState == EntityState.Unchanged).ForEach(x =>
                {
                    x.EntityState = EntityState.Deleted;
                    x.PrintColors.SetDeleted();
                    x.YDBookingChildUsesIns.SetDeleted();
                });

                #endregion YDBookingChild

                #region YDBookingChildTwisting

                entity.YDBookingChildTwistings.SetUnchanged();
                entity.YDBookingChildTwistings.ForEach(x =>
                {
                    x.YDBookingChildTwistingColors.SetUnchanged();
                    x.YDBCTwistingUsesIns.SetUnchanged();
                });

                YDBookingChildTwisting childTwisting;
                foreach (YDBookingChildTwisting item in model.YDBookingChildTwistings)
                {
                    childTwisting = entity.YDBookingChildTwistings.FirstOrDefault(x => x.YDBCTwistingID == item.YDBCTwistingID);
                    if (childTwisting == null)
                    {
                        childTwisting = item;
                        childTwisting.EntityState = EntityState.Added;
                        entity.YDBookingChildTwistings.Add(childTwisting);
                    }
                    else
                    {
                        childTwisting.ItemMasterID = item.ItemMasterID;
                        childTwisting.ColorID = item.ColorID;
                        childTwisting.ColorCode = item.ColorCode;
                        childTwisting.YarnProgramID = item.YarnProgramID;
                        childTwisting.BookingQty = item.BookingQty;
                        childTwisting.NoOfCone = item.NoOfCone;
                        childTwisting.UnitID = item.UnitID;
                        childTwisting.YarnDyedColorID = item.YarnDyedColorID;
                        childTwisting.Remarks = item.Remarks;
                        childTwisting.YarnCategory = item.YarnCategory;
                        childTwisting.NoOfThread = item.NoOfThread;
                        childTwisting.PhysicalCount = item.PhysicalCount;
                        childTwisting.TPI = item.TPI;
                        childTwisting.ShadeCode = item.ShadeCode;
                        childTwisting.IsTwisting = item.IsTwisting;
                        childTwisting.IsWaxing = item.IsWaxing;
                        childTwisting.UsesIn = item.UsesIn;
                        childTwisting.EntityState = EntityState.Modified;

                        #region Twist Color Table
                        foreach (var twistColor in item.YDBookingChildTwistingColors)
                        {
                            YDBookingChildTwistingColor colorEntity = childTwisting.YDBookingChildTwistingColors.Find(x => x.YDBCTwistingColorID == twistColor.YDBCTwistingColorID && x.ColorID == twistColor.ColorID);
                            if (colorEntity == null)
                            {
                                colorEntity = twistColor;
                                twistColor.EntityState = EntityState.Added;
                                childTwisting.YDBookingChildTwistingColors.Add(colorEntity);
                            }
                            else
                            {
                                colorEntity.ColorID = twistColor.ColorID;
                                colorEntity.ColorCode = twistColor.ColorCode;
                                colorEntity.TwistingColorQty = twistColor.AssignQty;
                                colorEntity.EntityState = EntityState.Modified;
                            }
                        }
                        childTwisting.YDBookingChildTwistingColors.FindAll(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                        #endregion Color Table

                        #region Twist Uses In
                        foreach (var uses in item.YDBCTwistingUsesIns)
                        {
                            YDBookingChildTwistingUsesIn twistusesEntity = childTwisting.YDBCTwistingUsesIns.Find(x => x.YDBCTwistingUsesInID == uses.YDBCTwistingUsesInID);
                            if (twistusesEntity == null || uses.YDBCTwistingUsesInID == 0)
                            {
                                twistusesEntity = CommonFunction.DeepClone(uses);
                                twistusesEntity.EntityState = EntityState.Added;
                                childTwisting.YDBCTwistingUsesIns.Add(twistusesEntity);
                            }
                            else
                            {
                                twistusesEntity.UsesIn = uses.UsesIn;
                                twistusesEntity.EntityState = EntityState.Modified;
                            }
                        }
                        childTwisting.YDBCTwistingUsesIns.FindAll(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                        #endregion Color Table
                    }
                }

                entity.YDBookingChildTwistings.FindAll(x => x.EntityState == EntityState.Unchanged).ForEach(x =>
                {
                    x.EntityState = EntityState.Deleted;
                    x.YDBookingChildTwistingColors.SetDeleted();
                });

                #endregion YDBookingChildTwisting
            }
            else
            {
                entity = model;
                entity.YDBookingBy = AppUser.UserCode;
                entity.AddedBy = AppUser.UserCode;

                if (filePath.NotNullOrEmpty())
                {
                    entity.SwatchFilePath = filePath;
                    entity.PreviewTemplate = previewTemplate;
                }
            }

            await _yarnDyeingBookingService.SaveAsync(entity);

            //await _commonService.UpdateFreeConceptStatus(InterfaceFrom.YDBooking, entity.ConceptID, "", 0, entity.IsBDS, 0, 0, 0);

            return Ok();
        }

        [HttpPost]
        [Route("save-ydbno")]
        public async Task<IActionResult> SaveYDBNo(YDBookingMaster model)
        {
            YDBookingMaster entity;
            string message = "";
            bool status = false;
            if (model.IsModified)
            {
                entity = await _yarnDyeingBookingService.GetAllAsync(model.YDBookingMasterID);
                entity.YDBNo = model.YDBNo;
                entity.IsYDBNoGenerated = true;
                entity.EntityState = EntityState.Modified;
                message = await _yarnDyeingBookingService.SaveYDBNoAsync(entity);
                status = message == "ok" ? true : false;
            }

            return Ok(new { status, message });
        }

        [HttpPost]
        [Route("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            YDBookingMaster entity = await _yarnDyeingBookingService.GetAllAsync(id);

            entity.IsApprove = true;
            entity.ApproveDate = DateTime.Now;
            entity.ApproveBy = AppUser.UserCode;
            entity.EntityState = EntityState.Modified;
            await _yarnDyeingBookingService.UpdateEntityAsync(entity);
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.YDBooking, entity.ConceptID, "", 0, entity.IsBDS, 0, 0, 0);

            #region Send Mail
            //bool isMailSent = await ApproveMailResendAsync(entity);
            //if (isMailSent == false)
            //{
            //    return BadRequest("Approved But mail not send.");
            //}
            /*if (entity.IsApprove && entity.ApproveBy > 0)
            {
                //Revise Fabric Booking [232263-FBR Rev-5]
                string revise = "";
                string revisionText = "";
                string ydBookingNo = $@", YD Booking no. {entity.YDBookingNo}";
                string ydType = " Yarn Dyeing & Twisting Booking ";

                int twistingCount = entity.YDBookingChilds.Count(x => x.IsTwisting == true);
                int dyeingCount = entity.YDBookingChilds.Count(x => x.IsTwisting == false);

                if (twistingCount == entity.YDBookingChilds.Count())
                {
                    ydType = " Twisted Yarn Dyeing Booking ";
                }
                else if (dyeingCount == entity.YDBookingChilds.Count())
                {
                    ydType = " Yarn Dyeing Booking ";
                }

                if (entity.RevisionNo > 0)
                {
                    revise = "Revised ";
                    revisionText = " Rev-" + entity.RevisionNo.ToString();
                }

                var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "YD Booking");
                var uInfo = await _emailService.GetUserEmailInfoAsync((int)entity.ApproveBy);
                if (uInfo.IsNull()) uInfo = new Core.DTOs.UserEmailInfo();

                String subject = $@"{revise}{ydType} For - {entity.GroupConceptNo}{ydBookingNo}{revisionText}";
                String toMailID = "";
                String ccMailID = "";
                String bccMailID = "";
                String body = "";

                List<EPYSLTEX.Service.Reporting.CustomeParameter> paramList = new List<Service.Reporting.CustomeParameter>();
                paramList.Add(new Service.Reporting.CustomeParameter { ParameterName = "YDBookingNo", ParameterValue = entity.YDBookingNo });

                var attachment = await _reportingService.GetPdfByteByReportName("YarnDyedBooking.rdl", UserId, paramList);
                //var attachment =(byte[])null;
                string fileName = $"{entity.GroupConceptNo}.pdf";

                body = String.Format(@"Dear Sir,
                                             <br/><br/>
                                             Please proceed the attached {6} for - {0}{5}{4}. 
                                             <br/><br/>
                                             Thanks & Regards
                                             <br/>
                                             {1}
                                             <br/>
                                             {2}
                                             <br/>
                                             {3}
                                             <br/><br/>
                                             <br/><br/>
                                             <small>This is an ERP generated automatic mail, you can also access the requisition by Textile ERP access.</small>
                                            ", entity.GroupConceptNo, AppUser.EmployeeName, uInfo.Designation, uInfo.Department, revisionText, ydBookingNo, ydType);

                if (isgDTO.IsNotNull())
                {
                    toMailID = isgDTO.ToMailID;
                    ccMailID = isgDTO.CCMailID;
                    bccMailID = isgDTO.BCCMailID;
                }
                bool isMailSent = true;
                if (Request.Headers.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
                {
                    if (ccMailID.IsNullOrEmpty())
                        ccMailID = AppUser.Email;
                    else
                    {
                        if (ccMailID.ToLower().IndexOf(AppUser.Email) == -1)
                        {
                            ccMailID = AppUser.Email + ";" + ccMailID;
                        }
                    }
                    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
                    string password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);

                    isMailSent = await _emailService.SendAutoEmailAsync(AppUser.Email, password, toMailID, ccMailID, bccMailID, subject, body, fileName, attachment, isMailSent);
                    if (isMailSent == false)
                    {
                        return BadRequest("Approved But mail not send.");
                    }
                }
                else
                {
                    toMailID = "imrez.ratin@epylliongroup.com;shifuddin@epylliongroup.com";
                    ccMailID = "";
                    bccMailID = "";
                    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
                    string password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
                    isMailSent= await _emailService.SendAutoEmailAsync("imrez.ratin", toMailID, subject, body, fileName, attachment,isMailSent);
                    if (isMailSent == false)
                    {
                        return BadRequest("Approved But mail not send.");
                    }
                }
            }*/
            #endregion Send Mail

            return Ok();
        }
        [HttpPost]
        [Route("approvemailresend/{id}")]
        public async Task<IActionResult> ApproveMailResend(int id)
        {
            YDBookingMaster entity = await _yarnDyeingBookingService.GetAllAsync(id);

            #region Send Mail
            //bool isMailSent = await ApproveMailResendAsync(entity);
            //if (isMailSent == false)
            //{
            //    return BadRequest("Approved But mail not send.");
            //}
            #endregion Send Mail

            return Ok();
        }
        [HttpPost]
        [Route("acknowledge/{id}")]
        public async Task<IActionResult> Acknowledge(int id)
        {
            YDBookingMaster entity = await _yarnDyeingBookingService.GetAllAsync(id);

            entity.IsAcknowledge = true;
            entity.AcknowledgeBy = AppUser.UserCode;
            entity.AcknowledgeDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            await _yarnDyeingBookingService.UpdateEntityAsync(entity);

            //await _commonService.UpdateFreeConceptStatus(InterfaceFrom.YDBooking, entity.ConceptID, "", 0, entity.IsBDS, 0, 0, 0);

            return Ok();
        }

        [HttpPost]
        [Route("unacknowledge/{id}/{unackreason}")]
        public async Task<IActionResult> UnAcknowledge(int id, string unackreason)
        {
            YDBookingMaster entity = await _yarnDyeingBookingService.GetAllAsync(id);

            entity.IsUnAcknowledge = true;
            entity.UnAcknowledgeBy = AppUser.UserCode;
            entity.UnAcknowledgeDate = DateTime.Now;
            entity.UnAckReason = unackreason;
            entity.EntityState = EntityState.Modified;
            await _yarnDyeingBookingService.UpdateEntityAsync(entity);

            //await _commonService.UpdateFreeConceptStatus(InterfaceFrom.YDBooking, entity.ConceptID, "", 0, entity.IsBDS, 0, 0, 0);

            return Ok();
        }

        #region Helpers
        private async Task<(YDBookingMaster, string, string)> ProcessFormDataAsync(IFormCollection formData)
        {
            //var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());

            //var formData = provider.FormData;
            var file = Request.Form.Files.FirstOrDefault();
            YDBookingMaster model = formData.ConvertToObject<YDBookingMaster>();
            //model.YDBookingChilds = JsonConvert.DeserializeObject<List<YDBookingChild>>(formData.Get("YDBookingChilds"));
            model.YDBookingChilds = JsonConvert.DeserializeObject<List<YDBookingChild>>(formData["YDBookingChilds"]);

            //if (model.YDBookingChildTwistings.Count > 0)
            //{
            //model.YDBookingChildTwistings = JsonConvert.DeserializeObject<List<YDBookingChildTwisting>>(formData.Get("YDBookingChildTwistings"));
            model.YDBookingChildTwistings = JsonConvert.DeserializeObject<List<YDBookingChildTwisting>>(formData["YDBookingChildTwistings"]);
            //} 
            #region Save Image
            //if (model.YDBookingMasterID <= 0) model.YDBookingNo = await _commonService.GetMaxNoAsync(TableNames.YARN_DYEING_BOOKING_NO, 1);
            var filePath = "";
            var previewTemplate = "";
            //if (provider.Files.Any())
            if (file != null)
            {
                //var originalFile = provider.Files[0];
                var originalFile = file;
                //var inputStream = await originalFile.ReadAsStreamAsync();
                var inputStream = originalFile.OpenReadStream();
                previewTemplate = "image";

                //filePath = $"{UploadLocations.LC_FILE_PATH}/{model.YDBookingNo}{Path.GetExtension(string.Join("", originalFile.Headers.ContentDisposition.FileName.Split(Path.GetInvalidFileNameChars())))}";
                filePath = $"{UploadLocations.LC_FILE_PATH}/{model.YDBookingNo}{Path.GetExtension(string.Join("", originalFile.FileName.Split(Path.GetInvalidFileNameChars())))}";
                var fullPath = Path.Combine(_hostingEnvironment.WebRootPath, filePath);
                string directoryPath = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                //var savePath = HttpContext.Current.Server.MapPath(filePath);
                //using (var fileStream = File.Create(savePath))
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    inputStream.Seek(0, SeekOrigin.Begin);
                    await inputStream.CopyToAsync(fileStream);
                }
            }
            #endregion

            return (model, filePath, previewTemplate);
        }
        private async Task<bool> ApproveMailResendAsync(YDBookingMaster entity)
        {
            #region Send Mail
            bool isMailSent = true;
            if (entity.IsApprove && entity.ApproveBy > 0)
            {
                //Revise Fabric Booking [232263-FBR Rev-5]
                string revise = "";
                string revisionText = "";
                string ydBookingNo = $@", YD Booking no. {entity.YDBookingNo}";
                string ydType = " Yarn Dyeing & Twisting Booking ";

                int twistingCount = entity.YDBookingChilds.Count(x => x.IsTwisting == true);
                int dyeingCount = entity.YDBookingChilds.Count(x => x.IsTwisting == false);

                if (twistingCount == entity.YDBookingChilds.Count())
                {
                    ydType = " Twisted Yarn Dyeing Booking ";
                }
                else if (dyeingCount == entity.YDBookingChilds.Count())
                {
                    ydType = " Yarn Dyeing Booking ";
                }

                if (entity.RevisionNo > 0)
                {
                    revise = "Revised ";
                    revisionText = " Rev-" + entity.RevisionNo.ToString();
                }

                //var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "YD Booking");
                //var uInfo = await _emailService.GetUserEmailInfoAsync((int)entity.ApproveBy);
                //if (uInfo.IsNull()) uInfo = new Core.DTOs.UserEmailInfo();

                //String subject = $@"{revise}{ydType} For - {entity.GroupConceptNo}{ydBookingNo}{revisionText}";
                //String toMailID = "";
                //String ccMailID = "";
                //String bccMailID = "";
                //String body = "";

                //List<EPYSLTEX.Service.Reporting.CustomeParameter> paramList = new List<Service.Reporting.CustomeParameter>();
                //paramList.Add(new Service.Reporting.CustomeParameter { ParameterName = "YDBookingNo", ParameterValue = entity.YDBookingNo });

                //var attachment = await _reportingService.GetPdfByteByReportName("YarnDyedBooking.rdl", UserId, paramList);
                ////var attachment =(byte[])null;
                //string fileName = $"{entity.GroupConceptNo}.pdf";

                //body = String.Format(@"Dear Sir,
                //                             <br/><br/>
                //                             Please proceed the attached {6} for - {0}{5}{4}. 
                //                             <br/><br/>
                //                             Thanks & Regards
                //                             <br/>
                //                             {1}
                //                             <br/>
                //                             {2}
                //                             <br/>
                //                             {3}
                //                             <br/><br/>
                //                             <br/><br/>
                //                             <small>This is an ERP generated automatic mail, you can also access the requisition by Textile ERP access.</small>
                //                            ", entity.GroupConceptNo, AppUser.EmployeeName, uInfo.Designation, uInfo.Department, revisionText, ydBookingNo, ydType);

                //if (isgDTO.IsNotNull())
                //{
                //    toMailID = isgDTO.ToMailID;
                //    ccMailID = isgDTO.CCMailID;
                //    bccMailID = isgDTO.BCCMailID;
                //}
                //if (Request.Headers.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
                //{
                //    if (ccMailID.IsNullOrEmpty())
                //        ccMailID = AppUser.Email;
                //    else
                //    {
                //        if (ccMailID.ToLower().IndexOf(AppUser.Email) == -1)
                //        {
                //            ccMailID = AppUser.Email + ";" + ccMailID;
                //        }
                //    }
                //    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
                //    string password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);

                //    isMailSent = await _emailService.SendAutoEmailAsync(AppUser.Email, password, toMailID, ccMailID, bccMailID, subject, body, fileName, attachment, isMailSent);
                //}
                //else
                //{
                //    toMailID = "imrez.ratin@epylliongroup.com;shifuddin@epylliongroup.com";
                //    ccMailID = "";
                //    bccMailID = "";
                //    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
                //    string password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
                //    isMailSent = await _emailService.SendAutoEmailAsync("imrez.ratin", toMailID, subject, body, fileName, attachment, isMailSent);
                //}
            }
            #endregion Send Mail

            return (isMailSent);
        }
        #endregion
    }
}
