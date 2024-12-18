using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Data.Entity;

namespace EPYSLTEX.Web.Controllers.Apis.Inventory.Yarn
{

    [Authorize]
    [Route("api/yarn-pr")]
    [ApiController]
    public class YarnPRController : ApiBaseController
    {
        private readonly IYarnPRService _service;
        IDapperCRUDService<YarnPRChild> _itemMasterService;
        
        //private readonly IEmailService _emailService;
        //private readonly IReportingService _reportingService;
        private readonly ICommonHelperService _commonService;
        private readonly IFreeConceptMRService _serviceMR;
        private static Logger _logger;

        public YarnPRController(IUserService userService, IYarnPRService service
            , IDapperCRUDService<YarnPRChild> itemMasterService

        , ICommonHelperService commonService
            , IFreeConceptMRService serviceMR) : base(userService)
        {
            _service = service;
            _itemMasterService = itemMasterService;
            _logger = LogManager.GetCurrentClassLogger();
            _commonService = commonService;
            _serviceMR = serviceMR;
        }

        [Route("list")]
        public async Task<IActionResult> GetList(Status status, string pageName)
        {
            pageName = CommonFunction.ReplaceInvalidChar(pageName);
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnPRMaster> records = await _service.GetPagedAsync(status, pageName, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new")]
        public async Task<IActionResult> GetNew()
        {
            YarnPRMaster data = await _service.GetNewAsync();
            data.YarnPRBy = AppUser.UserCode;
            return Ok(data);
        }

        [HttpGet]
        [Route("new-mr")]
        public async Task<IActionResult> GetNewForMR(string iDs, string source, string revisionstatus = "")
        {
            YarnPRMaster data = await _service.GetNewForMR(iDs, source, revisionstatus);
            foreach (YarnPRChild item in data.Childs)
            {
                item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
            }
            data.YarnPRBy = data.YarnPRMasterID == 0 ? AppUser.UserCode : data.YarnPRBy;
            data.YarnPRByName = AppUser.EmployeeName;
            return Ok(data);
        }

        [HttpGet]
        [Route("compositions")]
        public async Task<IActionResult> GetCompositions(string fiberType, string yarnType)
        {
            return Ok(await _service.GetYarnCompositionsAsync(fiberType, yarnType));
        }

        [Route("{id}/{prFromID}/{source}/{isNewForPRAck}")]
        [HttpGet]
        public async Task<IActionResult> GetPR(int id, int prFromID, string source, bool isNewForPRAck)
        {
            return Ok(await _service.GetAsync(id, prFromID, source, isNewForPRAck));
        }

        [Route("revise/{id}/{prFromID}/{source}/{groupConceptNo}")]
        [HttpGet]
        public async Task<IActionResult> GetForReviseAsync(int id, int prFromID, string source, string groupConceptNo)
        {
            return Ok(await _service.GetForReviseAsync(id, prFromID, source, groupConceptNo));
        }

        [Route("commercial-company/{childPrid}")]
        [HttpGet]
        public async Task<IActionResult> GetCommercialCompany(int childPrId)
        {
            List<YarnPRChild> records = await _service.GetCommercialCompany(childPrId);
            return Ok(records);
        }
        [Route("save")]
        [HttpPost]
    
        public async Task<IActionResult> Save(dynamic model1)
        {
            YarnPRMaster model = JsonConvert.DeserializeObject<YarnPRMaster>(Convert.ToString(model1));

            string source = model.Childs.Count() > 0 ? model.Childs.First().Source : "";
            string conceptNos = string.Join(",", model.Childs.Select(x => "'" + x.ConceptNo + "'"));
            string itemIds = string.Join(",", model.Childs.Select(x => x.ItemMasterID));
            string bookingNos = string.Join(",", model.Childs.Select(x => "'" + x.BookingNo + "'"));

            var tempChildList = CommonFunction.DeepClone(model.Childs);

            //List<YarnPRChild> childRecords = model.Childs;
            //_itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

            List<YarnPRChild> prChilds = source != PRFromName.PROJECTION_YARN_BOOKING ? await _service.GetChilds(conceptNos, itemIds, bookingNos) : new List<YarnPRChild>();
            if (model.IsAdditional)
            {
                prChilds = CommonFunction.DeepClone(model.Childs);
            }

            string conceptNo = string.Join(",", model.Childs.Select(x => x.ConceptNo).Distinct());
            string bookingNo = string.Join(",", model.Childs.Select(x => x.BookingNo).Distinct());

            YarnPRMaster entity;

            if (model.YarnPRMasterID > 0)
            {
                entity = await _service.GetAllByIDAsync(model.YarnPRMasterID);

                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;
                entity.YarnPRDate = model.YarnPRDate;
                entity.YarnPRRequiredDate = model.YarnPRRequiredDate;
                //entity.YarnPRBy = model.YarnPRBy;
                entity.Remarks = model.Remarks;
                entity.IsRNDPR = model.IsRNDPR;
                entity.ConceptNo = conceptNo;
                entity.BookingNo = bookingNo;
                entity.YDMaterialRequirementNo = bookingNo;
                entity.SendForApproval = model.SendForApproval;
                entity.Approve = false;
                entity.Reject = false;

                if (model.Status == Status.Revise)
                {
                    entity.Status = model.Status;
                    entity.NeedRevision = false;
                    entity.PreProcessRevNo = entity.RevisionNo;
                    entity.RevisionNo = entity.RevisionNo + 1;
                    entity.RevisionDate = DateTime.Now;
                    entity.RevisionBy = AppUser.UserCode;
                    entity.RevisionReason = model.RevisionReason;
                }

                if (model.RevisionStatus == "Revision Pending")
                {
                    entity.Status = model.Status;
                    entity.NeedRevision = false;
                    entity.PreProcessRevNo = entity.RevisionNo;
                    entity.RevisionNo = entity.RevisionNo + 1;
                    entity.RevisionDate = DateTime.Now;
                    entity.RevisionBy = AppUser.UserCode;
                    entity.RevisionReason = model.RevisionReason;
                }

                entity.Childs.SetUnchanged();

                foreach (YarnPRChild item in model.Childs)
                {
                    YarnPRChild tempChild = new YarnPRChild();
                    int companyId = entity.Childs.Max(x => x.FPRCompanyID);
                    YarnPRChild childEntity = entity.Childs.FirstOrDefault(x => x.YarnPRChildID == item.YarnPRChildID);

                    if (childEntity == null || item.YarnPRChildID == 0)
                    {
                        childEntity = CommonFunction.DeepClone(item);
                        childEntity.ItemMasterID = item.ItemMasterID;

                        tempChild = CommonFunction.DeepClone(tempChildList.FirstOrDefault(x => x.ItemMasterID == item.ItemMasterID));

                        if (tempChild.IsNotNull())
                        {
                            childEntity.RefLotNo = tempChild.RefLotNo;
                            childEntity.RefSpinnerID = tempChild.RefSpinnerID;
                            childEntity.Remarks = tempChild.Remarks;
                        }
                        childEntity.EntityState = EntityState.Added;
                        childEntity.FPRCompanyID = childEntity.FPRCompanyID == 0 ? companyId : childEntity.FPRCompanyID;
                        childEntity.BaseTypeId = this.GetBaseTypeType(entity, source);
                        childEntity.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        entity.Childs.Add(childEntity);
                    }
                    else
                    {
                        tempChild = CommonFunction.DeepClone(tempChildList.FirstOrDefault(x => x.ItemMasterID == item.ItemMasterID));

                        childEntity.ItemMasterID = item.ItemMasterID;
                        childEntity.ShadeCode = item.ShadeCode;
                        childEntity.RefLotNo = tempChild.RefLotNo;
                        childEntity.RefSpinnerID = tempChild.RefSpinnerID;
                        childEntity.ReqQty = item.ReqQty;
                        childEntity.ReqCone = item.ReqCone;
                        childEntity.Remarks = tempChild.Remarks;
                        childEntity.HSCode = item.HSCode;
                        childEntity.FPRCompanyID = childEntity.FPRCompanyID == 0 ? companyId : childEntity.FPRCompanyID;
                        childEntity.DayValidDurationId = item.DayValidDurationId;
                        childEntity.BaseTypeId = this.GetBaseTypeType(entity, source);
                        childEntity.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        childEntity.EntityState = EntityState.Modified;
                    }
                }

                foreach (var item in entity.Childs.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                }
            }
            else
            {
                entity = model;
                entity.CompanyID = model.Childs.First().FPRCompanyID;
                int companyId = entity.Childs.Max(x => x.FPRCompanyID);
                foreach (var childEntity in entity.Childs)
                {
                    childEntity.EntityState = EntityState.Added;
                    childEntity.BaseTypeId = this.GetBaseTypeType(entity, source);
                    childEntity.FPRCompanyID = childEntity.FPRCompanyID == 0 ? companyId : childEntity.FPRCompanyID;
                }

                entity.AddedBy = AppUser.UserCode;
                entity.YarnPRBy = AppUser.UserCode; //model.YarnPRBy;
                entity.SendForApproval = model.SendForApproval;
                entity.SubGroupID = 102; // yarn
                entity.ConceptNo = conceptNo;
                entity.BookingNo = bookingNo;
                if (!model.IsAdditional)
                {
                    entity.YarnPRNo = model.GroupConceptNo != "" ? model.GroupConceptNo : bookingNo;
                }
                entity.YDMaterialRequirementNo = bookingNo;
                entity.Reject = false;
                entity.RejectDate = null;
            }

            if (entity.YarnPRNo.Contains("PB-"))
            {
                var freeConceptMR = await _serviceMR.GetMRByConceptNo(entity.YarnPRNo);
                if (freeConceptMR.IsNotNull() && freeConceptMR.FCMRMasterID > 0)
                {
                    entity.YarnPRFromTableId = YarnPRFromTable.FreeConceptMRMaster;
                    entity.YarnPRFromMasterId = freeConceptMR.FCMRMasterID;
                }
            }
            await _service.SaveAsync(entity, AppUser.UserCode);

            #region UpdateFreeConceptStatus

            string conceptIds = string.Join(",", entity.Childs.Select(x => x.ConceptID).Distinct());
            int isBDS = entity.Childs.FirstOrDefault().Source == "BDS" ? 1 : 0;
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.YarnPR, 0, "", entity.BookingID, isBDS, 0, 0, 0, conceptIds);

            #endregion UpdateFreeConceptStatus

            return Ok();
        }
        [HttpPost]
        [Route("save-cpr")]
        public async Task<IActionResult> SaveCPR(dynamic jsnString)
        {
            YarnPRMaster model = JsonConvert.DeserializeObject<YarnPRMaster>(Convert.ToString(jsnString));
            YarnPRMaster entity = await _service.GetAllByIDAsync(model.YarnPRMasterID);

            entity.UpdatedBy = AppUser.UserCode;
            entity.DateUpdated = DateTime.Now;
            entity.IsCPR = true;
            entity.CPRBy = AppUser.EmployeeCode;
            entity.CPRDate = DateTime.Now;
            //25-8-22
            entity.SendForCPRApproval = model.SendForCPRApproval;
            //25-8-22

            entity.EntityState = EntityState.Modified;
            entity.Childs.SetUnchanged();
            foreach (YarnPRChild item in model.Childs)
            {
                YarnPRChild yPRC = entity.Childs.FirstOrDefault(i => i.YarnPRChildID == item.YarnPRChildID && i.FPRCompanyID != item.FPRCompanyID);
                if (yPRC.IsNotNull())
                {
                    yPRC.FPRCompanyID = item.FPRCompanyID;
                    //25-8-22
                    yPRC.Remarks = item.Remarks;
                    yPRC.ReqQty = item.ReqQty;
                    yPRC.ReqCone = item.ReqCone;
                    yPRC.RefLotNo = item.RefLotNo;
                    yPRC.RefSpinnerID = item.RefSpinnerID;
                    //25-8-22
                    yPRC.EntityState = EntityState.Modified;
                }
            }

            await _service.SaveCPRAsync(entity, AppUser.UserCode);
            //await _commonService.UpdateFreeConceptStatus(InterfaceFrom.YarnPR, entity.ConceptID, "", entity.BookingID);
            return Ok();
        }

        [HttpPost]
        [Route("save-fpr")]
        public async Task<IActionResult> UpdateFPRCompany(dynamic jsnString)
        {
            YarnPRMaster model = JsonConvert.DeserializeObject<YarnPRMaster>(
                Convert.ToString(jsnString),
                new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
                });

            YarnPRMaster entity = await _service.GetAllByIDAsync(model.YarnPRMasterID);

            entity.UpdatedBy = AppUser.UserCode;
            entity.DateUpdated = DateTime.Now;
            entity.IsFPR = true;
            entity.FPRBy = AppUser.UserCode;
            entity.FPRDate = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            entity.Childs.SetUnchanged();

            entity.Childs.ForEach(c =>
            {
                YarnPRChild child = model.Childs.Find(x => x.YarnPRChildID == c.YarnPRChildID);
                if (child.IsNotNull())
                {
                    c.PurchaseQty = child.PurchaseQty;
                    c.AllocationQty = child.ReqQty - c.PurchaseQty;
                    c.EntityState = EntityState.Modified;
                }
            });

            await _service.SaveFPRAsync(entity, AppUser.UserCode);
            //await _commonService.UpdateFreeConceptStatus(InterfaceFrom.YarnPR, entity.ConceptID, "", entity.BookingID);
            return Ok();
        }

        [HttpPost]
        [Route("approve/{id}/{YarnPRFromID}/{source}")]
        public async Task<IActionResult> Approve(int id, int YarnPRFromID, string source)
        {
            YarnPRMaster entity = await _service.GetAllByIDAsync(id);
            entity.Approve = true;
            entity.ApproveBy = AppUser.UserCode;
            entity.ApproveDate = DateTime.Now;

            entity.Reject = false;

            if (YarnPRFromID == 1 || YarnPRFromID == 2) //BDS & Concept then IsCPR = true
            {
                entity.IsCPR = true;
            }
            //if (entity.YarnPRNo.Contains("PB-"))
            //{
            //    entity.IsCPR = false;
            //    entity.SendForCPRApproval = false;
            //}
            //if (entity.YarnPRFromID == 4)
            //{
            //    entity.IsCPR = false;
            //    entity.SendForCPRApproval = false;
            //}
            entity.EntityState = EntityState.Modified;

            entity.YarnPOMasters.ForEach(x =>
            {
                x.Proposed = false;
                x.ProposedBy = 0;
                x.ProposedDate = null;

                x.Approved = false;
                x.ApprovedBy = 0;
                x.ApprovedDate = null;

                x.UnApprove = false;
                x.UnApproveBy = 0;
                x.UnApproveDate = null;
                x.UnapproveReason = "";

                x.IsCancel = false;
                x.CancelBy = 0;
                x.CancelDate = null;
                x.CancelReason = "";

                x.IsRevision = true;

                x.EntityState = EntityState.Modified;
            });

            await _service.UpdateEntityAsync(entity);

            try
            {
                #region UpdateFreeConceptStatus

                string conceptIds = string.Join(",", entity.Childs.Select(x => x.ConceptID).Distinct());
                int isBDS = source == "BDS" ? 1 : 0;
                await _commonService.UpdateFreeConceptStatus(InterfaceFrom.YarnPR, 0, "", entity.BookingID, isBDS, 0, 0, 0, conceptIds);

                #endregion UpdateFreeConceptStatus

                #region Send Mail

                //string EditType = entity.RevisionNo > 0 ? "Revise " : "";
                //string conceptNoTextSubject = "R&D Concept No";
                //string conceptNoTextBody = "concept no";

                //if (entity.ConceptNo.StartsWith("PB-"))
                //{
                //    conceptNoTextSubject = "Projection Fabric Booking No";
                //    conceptNoTextBody = "Projection Fabric Booking No";
                //}

                ////var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "PR R&D");// email service will be written
                ////var uInfo = await _emailService.GetUserEmailInfoAsync(UserId);
                ////var attachment = await _reportingService.GetPdfBytePR(1303, UserId, entity.YarnPRNo);
                //String subject = $@"{EditType}Yarn Purchase Requisition For {conceptNoTextSubject} {entity.ConceptNo}";
                //String toMailID = "";
                //String ccMailID = "";
                //String bccMailID = "";
                //String body = "";


                //if (uInfo.IsNotNull())
                //{
                //    body = String.Format(@"Dear Sir,
                //                             <br/><br/>
                //                             Please see the attached file of Yarn Purchase Requisition for {4} : {0}. For any further instruction or query regarding this purchase please feel free to contact.
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
                //                            ", entity.ConceptNo, AppUser.EmployeeName, uInfo.Designation, uInfo.Department, conceptNoTextBody);
                //}
                //if (isgDTO.IsNotNull())
                //{
                //    toMailID = isgDTO.ToMailID;
                //    ccMailID = isgDTO.CCMailID;
                //    bccMailID = isgDTO.BCCMailID;
                //}
                //if (HttpContext.Request.Host.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
                //{
                //    if (ccMailID.IsNullOrEmpty())
                //        ccMailID = AppUser.Email;
                //    else
                //    {
                //        ccMailID = ccMailID + ";" + AppUser.Email;
                //    }
                //    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
                //    string password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
                //    await _emailService.SendAutoEmailAsync(AppUser.Email, password, toMailID, ccMailID, bccMailID, subject, body, $"{entity.YarnPRNo}.pdf", attachment);// email service will be written
                //}
                //else
                //{
                //    toMailID = "mutasim@epylliongroup.com";
                //    ccMailID = "";
                //    bccMailID = "";
                //    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
                //    string password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
                //    await _emailService.SendAutoEmailAsync("Erp No Reply", "", toMailID, ccMailID, bccMailID, subject, body, $"{entity.YarnPRNo}.pdf", attachment);// email service will be written
                //}

                #endregion Send Mail
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return Ok();
        }

        [HttpPost]
        [Route("reject/{id}/{reason}/{source}")]
        public async Task<IActionResult> Reject(int id, string reason, string source)
        {
            YarnPRMaster entity = await _service.GetAllByIDAsync(id);
            entity.Reject = true;
            entity.RejectDate = DateTime.Now;
            entity.RejectReason = reason;
            entity.RejectBy = AppUser.UserCode;
            entity.EntityState = EntityState.Modified;

            entity.SendForApproval = false;
            entity.Approve = false;

            await _service.UpdateEntityAsync(entity);

            #region UpdateFreeConceptStatus

            string conceptIds = string.Join(",", entity.Childs.Select(x => x.ConceptID).Distinct());
            int isBDS = source == "BDS" ? 1 : 0;
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.YarnPR, 0, "", entity.BookingID, isBDS, 0, 0, 0, conceptIds);

            #endregion UpdateFreeConceptStatus

            return Ok();
        }

        private int GetBaseTypeType(YarnPRMaster pr, string source)
        {
            var noValue = pr.GroupConceptNo.IsNullOrEmpty() ? pr.YarnPRNo : pr.GroupConceptNo;

            //if (!noValue.Any(c => "PB-".Contains(c)))
            if (!noValue.Contains("PB-"))
            {
                if (source == PRFromName.BDS || source == PRFromName.CONCEPT)
                {
                    return EnumBaseType.OrderBasedSample;
                }
                else if (source == PRFromName.FABRIC_PROJECTION_YARN_BOOKING)
                {
                    return EnumBaseType.ProjectionBasedBulk;
                }
                else if (source == PRFromName.ROL_BASE_BOOKING)
                {
                    return EnumBaseType.ProjectionBasedSample;
                }
            }
            else if (noValue.Contains("PB-"))
            {
                return EnumBaseType.ProjectionBasedBulk;
            }
            return 0;
        }

        #region mail will be updated after service integration
        //[HttpGet]
        //[Route("smail/{prMasterID}")]
        //public async Task<IActionResult> SendMail(int prMasterID)
        //{
        //    bool IsSendMail = false;
        //    try
        //    {
        //        YarnPRMaster entity = await _service.GetAllByIDAsync(prMasterID);

        //        string EditType = entity.RevisionNo > 0 ? "Revise " : "";
        //        string conceptNoTextSubject = "R&D Concept No";
        //        string conceptNoTextBody = "concept no";

        //        //if (entity.ConceptNo.StartsWith("PB-"))
        //        if(entity.YarnPRFromID == PRFrom.FABRIC_PROJECTION_YARN_BOOKING)
        //        {
        //            conceptNoTextSubject = "Projection Fabric Booking No";
        //            conceptNoTextBody = "Projection Fabric Booking No";
        //        }

        //        var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "PR R&D");
        //        var uInfo = await _emailService.GetUserEmailInfoAsync(UserId);
        //        var attachment = await _reportingService.GetPdfBytePR(1303, UserId, entity.YarnPRNo);
        //        String subject = $@"{EditType}Yarn Purchase Requisition For {conceptNoTextSubject} {entity.ConceptNo}";
        //        String toMailID = "";
        //        String ccMailID = "";
        //        String bccMailID = "";
        //        String body = "";


        //        if (uInfo.IsNotNull())
        //        {
        //            body = String.Format(@"Dear Sir,
        //                                     <br/><br/>
        //                                     Please see the attached file of Yarn Purchase Requisition for {4} : {0}. For any further instruction or query regarding this purchase please feel free to contact.
        //                                     <br/><br/>
        //                                     Thanks & Regards
        //                                     <br/>
        //                                     {1}
        //                                     <br/>
        //                                     {2}
        //                                     <br/>
        //                                     {3}
        //                                     <br/><br/>
        //                                     <br/><br/>
        //                                     <small>This is an ERP generated automatic mail, you can also access the requisition by Textile ERP access.</small>
        //                                    ", entity.ConceptNo, AppUser.EmployeeName, uInfo.Designation, uInfo.Department, conceptNoTextBody);
        //        }
        //        if (isgDTO.IsNotNull())
        //        {
        //            toMailID = isgDTO.ToMailID;
        //            ccMailID = isgDTO.CCMailID;
        //            bccMailID = isgDTO.BCCMailID;
        //        }

        //        if (HttpContext.Request.Host.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
        //        {
        //            if (ccMailID.IsNullOrEmpty())
        //                ccMailID = AppUser.Email;
        //            else
        //            {
        //                ccMailID = ccMailID + ";" + AppUser.Email;
        //            }
        //            EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
        //            string password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
        //          //  await _emailService.SendAutoEmailAsync(AppUser.Email, password, toMailID, ccMailID, bccMailID, subject, body, $"{entity.YarnPRNo}.pdf", attachment);// email service will be written
        //            IsSendMail = true;
        //        }
        //        else
        //        {
        //            MailBasicProps mailBasicProps = new MailBasicProps();
        //            //string fromMailID = mailBasicProps.DefaultFromEmailId;
        //            //string password = mailBasicProps.DefaultPassword;
        //            toMailID = mailBasicProps.DefaultToEmailIds;
        //            ccMailID = mailBasicProps.DefaultCCEmailIds;
        //            bccMailID = mailBasicProps.DefaultBCCEmailIds;

        //            EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
        //            string password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
        //        //    await _emailService.SendAutoEmailAsync("Erp No Reply", "", toMailID, ccMailID, bccMailID, subject, body, $"{entity.YarnPRNo}.pdf", attachment);// email service will be written
        //            IsSendMail = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        IsSendMail = false;
        //    }
        //    return Ok(IsSendMail);
        //}
        #endregion
    }
}