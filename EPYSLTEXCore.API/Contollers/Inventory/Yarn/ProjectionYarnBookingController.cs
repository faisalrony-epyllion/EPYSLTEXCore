using Azure.Core;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Data.Entity;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEXCore.Infrastructure.Exceptions;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/projection-yarn-booking")]
    public class ProjectionYarnBookingController : ApiBaseController
    {
        private readonly ISelect2Service _select2Service;
        private readonly IProjectionYarnBookingService _service;
        private readonly IYarnPRService _servicePR;
        private readonly IItemMasterService<ProjectionYarnBookingItemChild> _itemMasterRepository;
        //private readonly IEmailService _emailService;
        //private readonly IReportingService _reportingService;
        //private readonly ICommonService _commonService;
        private static Logger _logger;

        public ProjectionYarnBookingController(ISelect2Service select2Service
            , IProjectionYarnBookingService service
            , IItemMasterService<ProjectionYarnBookingItemChild> itemMasterRepository
            , IUserService userService
        //, IEmailService emailService
        //, IReportingService reportingService
        //, ICommonService commonService
            , IYarnPRService servicePR) : base(userService)
        {
            _select2Service = select2Service;
            _service = service;
            _itemMasterRepository = itemMasterRepository;
            //_emailService = emailService;
            //_reportingService = reportingService;
            _logger = LogManager.GetCurrentClassLogger();
            //_commonService = commonService;
            _servicePR = servicePR;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, bool isApproveOrAcknowledge)
        {
            var paginationInfo = Request.GetPaginationInfo();
            int departmentId = AppUser.DepertmentID;
            string departmentName = AppUser.DepertmentDescription;
            bool IsSuperUser = AppUser.IsSuperUser;
            int employeeCode = AppUser.EmployeeCode;

            List<ProjectionYarnBookingMaster> records;
            if (!IsSuperUser && !isApproveOrAcknowledge)
            {
                records = await _service.GetPagedAsync(departmentId, status, paginationInfo, AppUser);
            }
            else
            {
                records = await _service.GetPagedAsynci(departmentId, departmentName, employeeCode, status, paginationInfo, AppUser);
            }
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new")]
        public async Task<IActionResult> GetNew()
        {
            var data = await _service.GetNewAsync(AppUser.EmployeeCode);
            data.DepartmentID = AppUser.DepertmentID;
            data.BookingByID = AppUser.UserCode;
            data.IsSuperUser = AppUser.IsSuperUser;
            return Ok(data);
        }

        [Route("GetBuyerTeamFromBuyerServiceWO/{buyerId}")]
        public async Task<IActionResult> GetBuyerTeamFromBuyerServiceWO(string buyerId)
        {
            int employeeCode = AppUser.EmployeeCode;
            var record = await _service.GetBuyerTeamAsync(buyerId, employeeCode);
            return Ok(record);
        }

        [Route("{PYBookingID}")]
        [HttpGet]
        public async Task<IActionResult> Get(int PYBookingID)
        {
            var record = await _service.GetAsync(PYBookingID, AppUser.EmployeeCode);
            record.IsSuperUser = AppUser.IsSuperUser;
            Guard.Against.NullObject(PYBookingID, record);

            return Ok(record);
        }

        [HttpGet]
        [Route("new-mr/{PYBookingIDs}")]
        public async Task<IActionResult> GetNewForYBooking(string PYBookingIDs)
        {
            ProjectionYarnBookingMaster data = await _service.GetNewPYBookingID(PYBookingIDs, AppUser.EmployeeCode);
            data.AddedBy = AppUser.UserCode;
            return Ok(data);
        }

        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(ProjectionYarnBookingMaster model)
        {
            List<ProjectionYarnBookingItemChild> childRecords = new List<ProjectionYarnBookingItemChild>();
            childRecords = model.ProjectionYarnBookingItemChilds;
            _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);
            ProjectionYarnBookingMaster entity;
            if (model.PYBookingID > 0)
            {
                entity = await _service.GetAllAsync(model.PYBookingID);

                entity.SendToApprover = model.SendToApprover;
                if (entity.SendToApprover) entity.IsReject = false;

                if (model.IsApprove)
                {
                    entity.IsApprove = model.IsApprove;
                    entity.ApproveBy = AppUser.UserCode;
                    entity.ApproveDate = DateTime.Now;
                }

                entity.BuyerID = model.BuyerID;
                entity.BookingByID = model.BookingByID;
                entity.RequiredByID = 0;
                entity.PYBookingDate = model.PYBookingDate;
                entity.SeasonID = model.SeasonID;
                entity.FinancialYearID = model.FinancialYearID;
                entity.FabricBookingStartMonth = model.FabricBookingStartMonth;
                entity.FabricBookingEndMonth = model.FabricBookingEndMonth;
                entity.Remarks = model.Remarks;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.BookingByID = entity.BookingByID == 0 ? AppUser.UserCode : entity.BookingByID;
                if (model.RevisionStatus == "Revision")
                {
                    entity.PreProcessRevNo = entity.RevisionNo;
                    entity.RevisionNo = entity.RevisionNo + 1;
                    entity.RevisionDate = DateTime.Now;
                    entity.RevisionBy = AppUser.UserCode;
                    entity.RevisionReason = model.RevisionReason;
                    entity.SendToApprover = model.SendToApprover;
                    entity.IsApprove = false;
                    entity.IsReject = false;
                    entity.IsAcknowledged = false;
                    entity.IsCancel = false;
                    entity.IsCancelAccept = false;
                    entity.IsCancelReject = false;
                    entity.IsUnacknowledge = false;
                    entity.RevisionStatus = "Revision";
                }

                entity.EntityState = EntityState.Modified;

                entity.ProjectionYarnBookingItemChilds.SetUnchanged();
                entity.ProjectionYarnBookingItemChilds.ForEach(x => x.PYBItemChildDetails.SetUnchanged());
                entity.PYBookingBuyerAndBuyerTeams.SetUnchanged();

                ProjectionYarnBookingItemChild child;
                ProjectionYarnBookingItemChildDetails childDetails;

                foreach (ProjectionYarnBookingItemChild item in childRecords)
                {
                    child = entity.ProjectionYarnBookingItemChilds.FirstOrDefault(x => x.PYBBookingChildID == item.PYBBookingChildID);
                    if (child == null)
                    {
                        item.PYBookingID = entity.PYBookingID;
                        item.SegmentValueId1 = item.Segment1ValueId;
                        item.SegmentValueId2 = item.Segment2ValueId;
                        item.SegmentValueId3 = item.Segment3ValueId;
                        item.SegmentValueId4 = item.Segment4ValueId;
                        item.SegmentValueId5 = item.Segment5ValueId;
                        item.SegmentValueId6 = item.Segment6ValueId;
                        item.SegmentValueId7 = item.Segment7ValueId;
                        item.SegmentValueId8 = item.Segment8ValueId;
                        item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        entity.ProjectionYarnBookingItemChilds.Add(item);
                    }
                    else
                    {
                        //child.ItemMasterID = item.ItemMasterID;
                        child.ItemMasterID = item.ItemMasterID;
                        child.UnitID = item.UnitID;
                        child.QTY = item.QTY;
                        child.ReqCone = item.ReqCone;
                        child.ShadeCode = item.ShadeCode;
                        child.PPrice = item.PPrice;
                        child.Remarks = item.Remarks;
                        child.DayValidDurationId = item.DayValidDurationId;
                        child.SegmentValueId1 = item.Segment1ValueId;
                        child.SegmentValueId2 = item.Segment2ValueId;
                        child.SegmentValueId3 = item.Segment3ValueId;
                        child.SegmentValueId4 = item.Segment4ValueId;
                        child.SegmentValueId5 = item.Segment5ValueId;
                        child.SegmentValueId6 = item.Segment6ValueId;
                        child.SegmentValueId7 = item.Segment7ValueId;
                        child.SegmentValueId8 = item.Segment8ValueId;
                        child.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        child.EntityState = EntityState.Modified;

                        foreach (ProjectionYarnBookingItemChildDetails childDetail in item.PYBItemChildDetails)
                        {
                            childDetails = child.PYBItemChildDetails.FirstOrDefault(x => x.PYBBookingChildDetailsID == childDetail.PYBBookingChildDetailsID);
                            if (childDetails == null)
                            {
                                childDetail.PYBookingID = entity.PYBookingID;
                                childDetail.PYBBookingChildID = item.PYBBookingChildID;
                                child.PYBItemChildDetails.Add(childDetail);
                            }
                            else
                            {
                                childDetails.BookingDate = childDetail.BookingDate;
                                childDetails.DetailsQTY = childDetail.DetailsQTY;
                                childDetails.EntityState = EntityState.Modified;
                            }
                        }
                    }
                }

                foreach (ProjectionYarnBookingItemChild item in entity.ProjectionYarnBookingItemChilds.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                }
                foreach (ProjectionYarnBookingItemChild item in entity.ProjectionYarnBookingItemChilds)
                {
                    item.PYBItemChildDetails.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x =>
                    {
                        x.EntityState = EntityState.Deleted;
                    });
                }

                string[] buyerIds = model.BuyerIDsList.Split(','); //entity.BuyerIDsList.Split(',');
                if (buyerIds != null)
                {
                    foreach (string BuyerID in buyerIds)
                    {
                        if (BuyerID != "")
                        {
                            PYBookingBuyerAndBuyerTeam bBuNBT = entity.PYBookingBuyerAndBuyerTeams.FirstOrDefault(n => n.PYBookingID == model.PYBookingID && n.BuyerID == Convert.ToInt32(BuyerID));

                            if (bBuNBT == null)
                            {
                                PYBookingBuyerAndBuyerTeam ChildGarPart = new PYBookingBuyerAndBuyerTeam
                                {
                                    BuyerID = Convert.ToInt32(BuyerID)
                                };
                                entity.PYBookingBuyerAndBuyerTeams.Add(ChildGarPart);
                            }
                            else
                            {
                                bBuNBT.PYBookingID = entity.PYBookingID;
                                bBuNBT.BuyerID = Convert.ToInt32(BuyerID);
                                bBuNBT.EntityState = EntityState.Modified;
                            }
                        }
                    }

                    entity.PYBookingBuyerAndBuyerTeams.FindAll(r => r.EntityState == EntityState.Unchanged).SetDeleted();
                }
                string[] buyerTeamIDs = model.BuyerTeamIDsList.Split(','); //entity.BuyerIDsList.Split(',');
                if (buyerTeamIDs != null)
                {
                    foreach (string BuyerTeamID in buyerTeamIDs)
                    {
                        if (BuyerTeamID != "")
                        {
                            PYBookingBuyerAndBuyerTeam bTeamBuNBT = entity.PYBookingBuyerAndBuyerTeams.FirstOrDefault(n => n.PYBookingID == model.PYBookingID && n.BuyerTeamID == Convert.ToInt32(BuyerTeamID));

                            if (bTeamBuNBT == null)
                            {
                                PYBookingBuyerAndBuyerTeam ChildGarPart = new PYBookingBuyerAndBuyerTeam
                                {
                                    BuyerTeamID = Convert.ToInt32(BuyerTeamID)
                                };
                                entity.PYBookingBuyerAndBuyerTeams.Add(ChildGarPart);
                            }
                            else
                            {
                                bTeamBuNBT.PYBookingID = entity.PYBookingID;
                                bTeamBuNBT.BuyerTeamID = Convert.ToInt32(BuyerTeamID);
                                bTeamBuNBT.EntityState = EntityState.Modified;
                            }
                        }
                    }

                    entity.PYBookingBuyerAndBuyerTeams.FindAll(r => r.EntityState == EntityState.Unchanged).SetDeleted();
                }
            }
            else
            {
                entity = model;
                string[] buyerIds = entity.BuyerIDsList.Split(',');
                buyerIds.Where(b => b != "").ToList().ForEach(BuyerID =>
                {
                    PYBookingBuyerAndBuyerTeam BuyerList = new PYBookingBuyerAndBuyerTeam();
                    {
                        BuyerList.BuyerID = Convert.ToInt32(BuyerID);
                    };
                    entity.PYBookingBuyerAndBuyerTeams.Add(BuyerList);
                });

                string[] buyerTeamIDs = entity.BuyerTeamIDsList.Split(',');
                buyerTeamIDs.Where(b => b != "").ToList().ForEach(BuyerTeamID =>
                {
                    PYBookingBuyerAndBuyerTeam BuyerTeamList = new PYBookingBuyerAndBuyerTeam();
                    {
                        BuyerTeamList.BuyerTeamID = Convert.ToInt32(BuyerTeamID);
                    };
                    entity.PYBookingBuyerAndBuyerTeams.Add(BuyerTeamList);
                });

                entity.BookingByID = entity.BookingByID == 0 ? AppUser.UserCode : entity.BookingByID;
                entity.AddedBy = AppUser.UserCode;
                entity.DepartmentID = model.DepartmentID;
                model.DepertmentDescription = AppUser.DepertmentDescription;
                entity.DepertmentDescription = AppUser.DepertmentDescription;
                if (model.DepertmentDescription == "Production Management Control" || model.DepertmentDescription == "Operation[Textile]" || model.DepertmentDescription == "Operation" || model.DepertmentDescription == "Planning, Monitoring & Control" || model.DepertmentDescription == "Knitting")
                {
                    entity.PBookingType = EnumPBookingType.Textile;
                    entity.BaseTypeId = EnumBaseType.ProjectionBasedBulk;
                }
                else if (model.DepertmentDescription == "Research & Development")
                {
                    entity.PBookingType = EnumPBookingType.Rnd;
                    entity.BaseTypeId = EnumBaseType.ProjectionBasedSample;
                }
                else if (model.DepertmentDescription == "Supply Chain")
                {
                    entity.PBookingType = EnumPBookingType.SupplyChain;
                    entity.BaseTypeId = EnumBaseType.None;
                }
                else if (model.DepertmentDescription == "Merchandiser [Fabric]" || model.DepertmentDescription == "Merchandising")
                {
                    entity.PBookingType = EnumPBookingType.Merchandising;
                    entity.BaseTypeId = EnumBaseType.ProjectionBasedBulk;
                }
                else if (model.DepertmentDescription == "Marketing & Merchandising")
                {
                    entity.PBookingType = EnumPBookingType.MnM;
                    entity.BaseTypeId = EnumBaseType.ProjectionBasedBulk;
                }
                else
                {
                    entity.PBookingType = EnumPBookingType.None;
                    entity.BaseTypeId = EnumBaseType.None;
                }
                entity.SeasonID = model.SeasonID;
                entity.FinancialYearID = model.FinancialYearID;
                entity.DateAdded = DateTime.Now;

                foreach (ProjectionYarnBookingItemChild item in entity.ProjectionYarnBookingItemChilds)
                {
                    item.SegmentValueId1 = item.Segment1ValueId;
                    item.SegmentValueId2 = item.Segment2ValueId;
                    item.SegmentValueId3 = item.Segment3ValueId;
                    item.SegmentValueId4 = item.Segment4ValueId;
                    item.SegmentValueId5 = item.Segment5ValueId;
                    item.SegmentValueId6 = item.Segment6ValueId;
                    item.SegmentValueId7 = item.Segment7ValueId;
                    item.SegmentValueId8 = item.Segment8ValueId;
                    item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                }
            }
            await _service.SaveAsync(entity, AppUser.UserCode);
            return Ok();
        }

        [Route("approve")]
        [HttpPost]
        public async Task<IActionResult> Approve(ProjectionYarnBookingMaster model)
        {
            ProjectionYarnBookingMaster entity;
            try
            {
                #region Update Projection
                entity = await _service.GetAllAsync(model.PYBookingID);

                if (model.IsApprove)
                {
                    entity.IsApprove = true;
                    entity.ApproveBy = AppUser.UserCode;
                    entity.ApproveDate = DateTime.Now;
                }

                if (model.SendToApprover)
                {
                    entity.SendToApprover = true;
                }
                entity.EntityState = EntityState.Modified;
                await _service.UpdateEntityAsync(entity);
                #endregion

                #region Send Mail
                /* //OFF FOR CORE
                if (model.IsApprove)
                {
                    
                    //If have to change anything in this mail section, also need to change 
                    //public async Task<IActionResult> SendMail(string pyBookingNo) this method => Resend mail method
                    

                    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();

                    var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Projection Booking");
                    if (model.DepertmentDescription.ToLower() == "R&D".ToLower())
                    {
                        isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Projection Booking R & D");
                    }
                    else if (model.DepertmentDescription.ToLower() == "Supply Chain".ToLower())
                    {
                        isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Projection Booking Supply Chain");
                    }
                    else if (model.DepertmentDescription.ToLower() == "Marketing & Merchandising".ToLower())
                    {
                        isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Projection Booking Marketing & Merchandising");
                    }

                    var uInfo = await _emailService.GetUserEmailInfoAsync(AppUser.UserCode);
                    List<EPYSLTEX.Service.Reporting.CustomeParameter> paramList = new List<Service.Reporting.CustomeParameter>();
                    paramList.Add(new Service.Reporting.CustomeParameter { ParameterName = "PYBookingNo", ParameterValue = entity.PYBookingNo });

                    var attachment = await _reportingService.GetPdfByteByReportName("YarnProjectionBooking.rdl", AppUser.UserCode, paramList);

                    String EditType = entity.RevisionNo > 0 ? "Revise " : "";
                    String subject = $@"{EditType}Yarn Projection Booking No {entity.PYBookingNo}";
                    String fromMailID = "";
                    String password = "";
                    String toMailID = "";
                    String ccMailID = "";
                    String bccMailID = "";
                    String messageBody = "";

                    if (Request.Headers.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
                    {
                        fromMailID = AppUser.Email;
                        password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
                        //toMailID = uInfo.Email.IsNullOrEmpty() ? AppUser.Email : uInfo.Email;
                        if (ccMailID.IsNullOrEmpty())
                            ccMailID = AppUser.Email;
                        else
                        {
                            ccMailID = ccMailID + ";" + AppUser.Email;
                        }

                        if (isgDTO.IsNotNull())
                        {
                            toMailID += isgDTO.ToMailID.IsNullOrEmpty() ? "" : ";" + isgDTO.ToMailID;
                            ccMailID += isgDTO.CCMailID.IsNullOrEmpty() ? "" : ";" + isgDTO.CCMailID;
                            bccMailID = isgDTO.BCCMailID;
                        }
                    }
                    else
                    {
                        fromMailID = "erpnoreply@epylliongroup.com";
                        password = "Ugr7jT5d";
                        toMailID = "anupam@epylliongroup.com;abdussalam@epylliongroup.com;litonekl@epylliongroup.com";
                        ccMailID = "";
                        bccMailID = "";
                    }

                    if (uInfo.IsNotNull())
                    {
                        messageBody = String.Format(@"Dear Sir,
                                             <br/><br/>
                                             Please see the attached file of Yarn Projection Booking no: {0}. For any further instruction or query regarding this purchase please feel free to contact.
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
                                            ", entity.PYBookingNo, AppUser.EmployeeName, uInfo.Designation, uInfo.Department);
                    }

                    String fileName = String.Empty;
                    fileName = $"{entity.PYBookingNo}.pdf";
                    await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, subject, messageBody, fileName, attachment);
                }
                */
                #endregion Send Mail

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return Ok();
        }

        [Route("acknowledge")]
        [HttpPost]
        public async Task<IActionResult> Acknowledge(ProjectionYarnBookingMaster model)
        {
            ProjectionYarnBookingMaster entity;
            try
            {
                #region Ack Projection

                entity = await _service.GetPYBWithPRAsync(model.PYBookingID, model.PYBookingNo);

                entity.IsAcknowledged = true;
                entity.AcknowledgedBy = AppUser.UserCode;
                entity.AcknowledgedDate = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                if (entity.YarnPR.YarnPRMasterID > 0)
                {
                    entity.YarnPR.EntityState = EntityState.Modified;
                    entity.YarnPR.UpdatedBy = AppUser.UserCode;
                    entity.YarnPR.DateUpdated = DateTime.Now;
                }
                else
                {
                    entity.YarnPR = new YarnPRMaster();
                    entity.YarnPR.EntityState = EntityState.Added;
                    entity.YarnPR.YarnPRBy = AppUser.UserCode;
                    entity.YarnPR.AddedBy = AppUser.UserCode;
                    entity.YarnPR.DateAdded = DateTime.Now;
                }
                entity.YarnPR.YarnPRFromID = PRFrom.PROJECTION_YARN_BOOKING;

                entity.YarnPR.YarnPRFromTableId = YarnPRFromTable.ProjectionYarnBookingMaster;
                entity.YarnPR.YarnPRFromMasterId = entity.PYBookingID;

                entity.YarnPR.YarnPRNo = entity.PYBookingNo;
                entity.YarnPR.SendForApproval = true;
                entity.YarnPR.Approve = true;
                entity.YarnPR.ApproveBy = AppUser.UserCode;
                entity.YarnPR.ApproveDate = DateTime.Now;

                entity.YarnPR.YarnPRDate = DateTime.Now;
                entity.YarnPR.YarnPRRequiredDate = DateTime.Now;
                entity.YarnPR.TriggerPointID = 1252; //Projection Based

                entity.YarnPR.IsRNDPR = true;
                entity.YarnPR.IsCPR = false;
                entity.YarnPR.IsFPR = false;

                entity.YarnPR.SendForCPRApproval = false;

                entity.YarnPR.Reject = false;
                entity.YarnPR.RejectBy = 0;
                entity.YarnPR.RejectDate = null;
                entity.YarnPR.RejectReason = "";

                entity.YarnPR.SubGroupID = 102; // yarn
                entity.YarnPR.BookingNo = entity.PYBookingNo;
                entity.YarnPR.YDMaterialRequirementNo = entity.PYBookingNo;
                entity.YarnPR.PreProcessRevNo = entity.RevisionNo;
                entity.YarnPR.RevisionNo = entity.RevisionNo;
                entity.YarnPR.RevisionBy = entity.RevisionBy;
                entity.YarnPR.RevisionDate = entity.RevisionDate;
                entity.YarnPR.CompanyID = entity.CompanyID;

                entity.YarnPR.YarnPOMasters.ForEach(x =>
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

                entity.YarnPR.Childs.SetUnchanged();

                entity.ProjectionYarnBookingItemChilds.ForEach(child =>
                {
                    YarnPRChild yarnPRChild = new YarnPRChild();
                    int indexF = entity.YarnPR.Childs.FindIndex(x => x.PYBBookingChildID == child.PYBBookingChildID);

                    if (indexF > -1)
                    {
                        yarnPRChild = entity.YarnPR.Childs[indexF];
                        yarnPRChild.ItemMasterID = child.ItemMasterID;
                        yarnPRChild.YarnCategory = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, child.ShadeCode);
                        yarnPRChild.EntityState = EntityState.Modified;
                        yarnPRChild.DayValidDurationId = child.DayValidDurationId;
                        yarnPRChild.BaseTypeId = entity.BaseTypeId;
                        entity.YarnPR.Childs[indexF] = CommonFunction.DeepClone(yarnPRChild);
                    }
                    else
                    {
                        yarnPRChild.ItemMasterID = child.ItemMasterID;
                        yarnPRChild.UnitID = child.UnitID;
                        yarnPRChild.ReqQty = child.QTY;
                        yarnPRChild.ReqCone = (int)child.ReqCone;
                        yarnPRChild.Remarks = child.Remarks;
                        yarnPRChild.FPRCompanyID = entity.CompanyID;
                        yarnPRChild.ShadeCode = child.ShadeCode;

                        yarnPRChild.ReqQty = child.QTY;
                        yarnPRChild.ReqCone = Convert.ToInt32(child.ReqCone);

                        yarnPRChild.SetupChildID = 0;
                        yarnPRChild.HSCode = "";
                        yarnPRChild.PYBBookingChildID = child.PYBBookingChildID;
                        yarnPRChild.YarnCategory = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, child.ShadeCode);
                        yarnPRChild.DayValidDurationId = child.DayValidDurationId;
                        yarnPRChild.BaseTypeId = entity.BaseTypeId;
                        yarnPRChild.EntityState = EntityState.Added;
                        entity.YarnPR.Childs.Add(yarnPRChild);
                    }
                });

                entity.YarnPR.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();

                await _service.AcknowledgeEntityAsync(entity, entity.YarnPR);
                #endregion
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw ex;
            }

            return Ok();
        }

        [Route("acknowledgemnm")]
        [HttpPost]
        public async Task<IActionResult> AcknowledgeMnM(ProjectionYarnBookingMaster model)
        {
            try
            {
                List<ProjectionYarnBookingItemChild> childRecords = new List<ProjectionYarnBookingItemChild>();
                childRecords = model.ProjectionYarnBookingItemChilds;
                _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

                ProjectionYarnBookingMaster entity;

                entity = await _service.GetAllAsync(model.PYBookingID);

                entity.SendToApprover = model.SendToApprover;

                if (model.IsApprove)
                {
                    entity.IsApprove = model.IsApprove;
                    entity.ApproveBy = AppUser.UserCode;
                    entity.ApproveDate = DateTime.Now;
                }

                entity.BuyerID = model.BuyerID;
                entity.BookingByID = model.BookingByID;
                entity.RequiredByID = 0;
                entity.PYBookingDate = model.PYBookingDate;
                entity.SeasonID = model.SeasonID;
                entity.FinancialYearID = model.FinancialYearID;
                entity.FabricBookingStartMonth = model.FabricBookingStartMonth;
                entity.FabricBookingEndMonth = model.FabricBookingEndMonth;
                entity.Remarks = model.Remarks;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.BookingByID = entity.BookingByID == 0 ? AppUser.UserCode : entity.BookingByID;

                if (model.RevisionStatus == "Revision")
                {
                    entity.PreProcessRevNo = entity.RevisionNo;
                    entity.RevisionNo = entity.RevisionNo + 1;
                    entity.RevisionDate = DateTime.Now;
                    entity.RevisionBy = AppUser.UserCode;
                    entity.RevisionReason = model.RevisionReason;
                    entity.SendToApprover = false;
                    //entity.IsApprove = false;
                    entity.IsReject = false;
                    entity.IsAcknowledged = false;
                    entity.IsCancel = false;
                    entity.IsCancelAccept = false;
                    entity.IsCancelReject = false;
                    entity.IsUnacknowledge = false;
                    entity.RevisionStatus = "Revision";
                }

                entity.EntityState = EntityState.Modified;

                entity.ProjectionYarnBookingItemChilds.SetUnchanged();
                entity.ProjectionYarnBookingItemChilds.ForEach(x => x.PYBItemChildDetails.SetUnchanged());
                entity.PYBookingBuyerAndBuyerTeams.SetUnchanged();

                ProjectionYarnBookingItemChild child;
                ProjectionYarnBookingItemChildDetails childDetails;

                foreach (ProjectionYarnBookingItemChild item in childRecords)
                {
                    child = entity.ProjectionYarnBookingItemChilds.FirstOrDefault(x => x.PYBBookingChildID == item.PYBBookingChildID);
                    if (child == null)
                    {
                        item.PYBookingID = entity.PYBookingID;
                        item.SegmentValueId1 = item.Segment1ValueId;
                        item.SegmentValueId2 = item.Segment2ValueId;
                        item.SegmentValueId3 = item.Segment3ValueId;
                        item.SegmentValueId4 = item.Segment4ValueId;
                        item.SegmentValueId5 = item.Segment5ValueId;
                        item.SegmentValueId6 = item.Segment6ValueId;
                        item.SegmentValueId7 = item.Segment7ValueId;
                        item.SegmentValueId8 = item.Segment8ValueId;
                        item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);
                        entity.ProjectionYarnBookingItemChilds.Add(item);
                    }
                    else
                    {
                        //child.ItemMasterID = item.ItemMasterID;
                        child.ItemMasterID = item.ItemMasterID;
                        child.UnitID = item.UnitID;
                        child.QTY = item.QTY;
                        child.ReqCone = item.ReqCone;
                        child.ShadeCode = item.ShadeCode;
                        child.PPrice = item.PPrice;
                        child.Remarks = item.Remarks;
                        child.SegmentValueId1 = item.Segment1ValueId;
                        child.SegmentValueId2 = item.Segment2ValueId;
                        child.SegmentValueId3 = item.Segment3ValueId;
                        child.SegmentValueId4 = item.Segment4ValueId;
                        child.SegmentValueId5 = item.Segment5ValueId;
                        child.SegmentValueId6 = item.Segment6ValueId;
                        child.SegmentValueId7 = item.Segment7ValueId;
                        child.SegmentValueId8 = item.Segment8ValueId;
                        child.DayValidDurationId = item.DayValidDurationId;
                        child.YarnCategory = CommonFunction.GetYarnShortForm(child.Segment1ValueDesc, child.Segment2ValueDesc, child.Segment3ValueDesc, child.Segment4ValueDesc, child.Segment5ValueDesc, child.Segment6ValueDesc, child.ShadeCode);
                        child.EntityState = EntityState.Modified;

                        foreach (ProjectionYarnBookingItemChildDetails childDetail in item.PYBItemChildDetails)
                        {
                            childDetails = child.PYBItemChildDetails.FirstOrDefault(x => x.PYBBookingChildDetailsID == childDetail.PYBBookingChildDetailsID);
                            if (childDetails == null)
                            {
                                childDetail.PYBookingID = entity.PYBookingID;
                                childDetail.PYBBookingChildID = item.PYBBookingChildID;
                                child.PYBItemChildDetails.Add(childDetail);
                            }
                            else
                            {
                                childDetails.BookingDate = childDetail.BookingDate;
                                childDetails.DetailsQTY = childDetail.DetailsQTY;
                                childDetails.EntityState = EntityState.Modified;
                            }
                        }
                    }
                }

                foreach (ProjectionYarnBookingItemChild item in entity.ProjectionYarnBookingItemChilds.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                }
                foreach (ProjectionYarnBookingItemChild item in entity.ProjectionYarnBookingItemChilds)
                {
                    item.PYBItemChildDetails.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x =>
                    {
                        x.EntityState = EntityState.Deleted;
                    });
                }

                string[] buyerIds = model.BuyerIDsList.Split(','); //entity.BuyerIDsList.Split(',');
                if (buyerIds != null)
                {
                    foreach (string BuyerID in buyerIds)
                    {
                        if (BuyerID != "")
                        {
                            PYBookingBuyerAndBuyerTeam bBuNBT = entity.PYBookingBuyerAndBuyerTeams.FirstOrDefault(n => n.PYBookingID == model.PYBookingID && n.BuyerID == Convert.ToInt32(BuyerID));

                            if (bBuNBT == null)
                            {
                                PYBookingBuyerAndBuyerTeam ChildGarPart = new PYBookingBuyerAndBuyerTeam
                                {
                                    BuyerID = Convert.ToInt32(BuyerID)
                                };
                                entity.PYBookingBuyerAndBuyerTeams.Add(ChildGarPart);
                            }
                            else
                            {
                                bBuNBT.PYBookingID = entity.PYBookingID;
                                bBuNBT.BuyerID = Convert.ToInt32(BuyerID);
                                bBuNBT.EntityState = EntityState.Modified;
                            }
                        }
                    }

                    entity.PYBookingBuyerAndBuyerTeams.FindAll(r => r.EntityState == EntityState.Unchanged).SetDeleted();
                }
                string[] buyerTeamIDs = model.BuyerTeamIDsList.Split(','); //entity.BuyerIDsList.Split(',');
                if (buyerTeamIDs != null)
                {
                    foreach (string BuyerTeamID in buyerTeamIDs)
                    {
                        if (BuyerTeamID != "")
                        {
                            PYBookingBuyerAndBuyerTeam bTeamBuNBT = entity.PYBookingBuyerAndBuyerTeams.FirstOrDefault(n => n.PYBookingID == model.PYBookingID && n.BuyerTeamID == Convert.ToInt32(BuyerTeamID));

                            if (bTeamBuNBT == null)
                            {
                                PYBookingBuyerAndBuyerTeam ChildGarPart = new PYBookingBuyerAndBuyerTeam
                                {
                                    BuyerTeamID = Convert.ToInt32(BuyerTeamID)
                                };
                                entity.PYBookingBuyerAndBuyerTeams.Add(ChildGarPart);
                            }
                            else
                            {
                                bTeamBuNBT.PYBookingID = entity.PYBookingID;
                                bTeamBuNBT.BuyerTeamID = Convert.ToInt32(BuyerTeamID);
                                bTeamBuNBT.EntityState = EntityState.Modified;
                            }
                        }
                    }

                    entity.PYBookingBuyerAndBuyerTeams.FindAll(r => r.EntityState == EntityState.Unchanged).SetDeleted();
                }

                await _service.SaveAsync(entity, AppUser.UserCode);


                //Acknowledged Process 
                entity = await _service.GetAllAsync(model.PYBookingID);
                entity.IsAcknowledged = true;
                entity.AcknowledgedBy = AppUser.UserCode;
                entity.AcknowledgedDate = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                #region Yarn PR
                YarnPRMaster yarnPRMaster = new YarnPRMaster();
                yarnPRMaster = await _servicePR.GetPRByYarnPRFromTable(YarnPRFromTable.ProjectionYarnBookingMaster, model.PYBookingID);
                yarnPRMaster.EntityState = yarnPRMaster.YarnPRMasterID > 0 ? EntityState.Modified : EntityState.Added;

                yarnPRMaster.YarnPRFromID = PRFrom.PROJECTION_YARN_BOOKING;

                yarnPRMaster.YarnPRFromTableId = YarnPRFromTable.ProjectionYarnBookingMaster;
                yarnPRMaster.YarnPRFromMasterId = entity.PYBookingID;

                yarnPRMaster.YarnPRNo = entity.PYBookingNo;
                yarnPRMaster.SendForApproval = true;
                yarnPRMaster.Approve = true;
                yarnPRMaster.ApproveBy = AppUser.UserCode;
                yarnPRMaster.ApproveDate = DateTime.Now;
                yarnPRMaster.AddedBy = AppUser.UserCode;
                yarnPRMaster.DateAdded = DateTime.Now;
                yarnPRMaster.YarnPRDate = DateTime.Now;
                yarnPRMaster.YarnPRRequiredDate = DateTime.Now;
                yarnPRMaster.YarnPRBy = AppUser.UserCode;
                yarnPRMaster.TriggerPointID = 1252; //Projection Based
                yarnPRMaster.IsRNDPR = true;
                yarnPRMaster.IsCPR = true;
                yarnPRMaster.CPRBy = AppUser.UserCode;
                yarnPRMaster.CPRDate = DateTime.Now;
                yarnPRMaster.IsFPR = true;
                yarnPRMaster.FPRDate = DateTime.Now;
                yarnPRMaster.SubGroupID = 102; // yarn
                yarnPRMaster.CompanyID = entity.CompanyID;
                yarnPRMaster.BookingNo = entity.PYBookingNo;
                yarnPRMaster.YDMaterialRequirementNo = entity.PYBookingNo;
                yarnPRMaster.PreProcessRevNo = entity.RevisionNo;
                yarnPRMaster.RevisionNo = entity.RevisionNo;
                yarnPRMaster.RevisionBy = entity.RevisionBy;
                yarnPRMaster.RevisionDate = entity.RevisionDate;

                yarnPRMaster.Childs.SetUnchanged();

                entity.ProjectionYarnBookingItemChilds.ForEach(x =>
                {
                    YarnPRChild yarnPRChild = new YarnPRChild();
                    yarnPRChild = yarnPRMaster.Childs.Find(y => y.PYBBookingChildID == x.PYBBookingChildID);
                    if (yarnPRChild.IsNull())
                    {
                        yarnPRChild = new YarnPRChild();
                        yarnPRChild.EntityState = EntityState.Added;
                        yarnPRChild.ItemMasterID = x.ItemMasterID;
                        yarnPRChild.UnitID = x.UnitID;
                        yarnPRChild.ReqQty = x.QTY;
                        yarnPRChild.ReqCone = (int)x.ReqCone;
                        yarnPRChild.Remarks = x.Remarks;
                        yarnPRChild.FPRCompanyID = entity.CompanyID;
                        yarnPRChild.ShadeCode = x.ShadeCode;
                        yarnPRChild.YarnCategory = x.YarnCategory;
                        yarnPRChild.SetupChildID = 0;
                        yarnPRChild.ReqCone = 0;
                        yarnPRChild.HSCode = "";
                        yarnPRChild.PYBBookingChildID = x.PYBBookingChildID;
                        yarnPRChild.BaseTypeId = entity.BaseTypeId;
                        yarnPRChild.DayValidDurationId = x.DayValidDurationId;
                        yarnPRMaster.Childs.Add(yarnPRChild);
                    }
                    else
                    {
                        yarnPRChild.EntityState = EntityState.Modified;
                        yarnPRChild.ItemMasterID = x.ItemMasterID;
                        yarnPRChild.UnitID = x.UnitID;
                        yarnPRChild.ReqQty = x.QTY;
                        yarnPRChild.ReqCone = (int)x.ReqCone;
                        yarnPRChild.Remarks = x.Remarks;
                        yarnPRChild.FPRCompanyID = entity.CompanyID;
                        yarnPRChild.ShadeCode = x.ShadeCode;
                        yarnPRChild.YarnCategory = x.YarnCategory;
                        yarnPRChild.SetupChildID = 0;
                        yarnPRChild.ReqCone = 0;
                        yarnPRChild.HSCode = "";
                        yarnPRChild.PYBBookingChildID = x.PYBBookingChildID;
                        yarnPRChild.BaseTypeId = entity.BaseTypeId;
                        yarnPRChild.DayValidDurationId = x.DayValidDurationId;
                    }

                });

                yarnPRMaster.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();

                await _service.AcknowledgeEntityAsync(entity, yarnPRMaster);

                #endregion

                #region Send Mail
                /* //OFF FOR CORE
                if (entity.IsAcknowledged && model.DepertmentDescription.ToLower() == "Marketing & Merchandising".ToLower())
                {
                    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();

                    var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Projection Booking Ack M&M");

                    var uInfo = await _emailService.GetUserEmailInfoAsync(AppUser.UserCode);
                    List<EPYSLTEX.Service.Reporting.CustomeParameter> paramList = new List<Service.Reporting.CustomeParameter>();
                    paramList.Add(new Service.Reporting.CustomeParameter { ParameterName = "PYBookingNo", ParameterValue = entity.PYBookingNo });

                    var attachment = await _reportingService.GetPdfByteByReportName("YarnProjectionBooking.rdl", AppUser.UserCode, paramList);

                    String EditType = entity.RevisionNo > 0 ? "Revise " : "";
                    string revisionText = entity.RevisionNo > 0 ? " Rev-" + entity.RevisionNo.ToString() : "";
                    String subject = $@"M&M {EditType}Yarn Projection Booking No {entity.PYBookingNo}{revisionText} Acknowledged by Textile";
                    String fromMailID = "";
                    String password = "";
                    String toMailID = "";
                    String ccMailID = "";
                    String bccMailID = "";
                    String messageBody = "";

                    if (Request.Headers.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
                    {
                        fromMailID = AppUser.Email;
                        password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
                        //toMailID = uInfo.Email.IsNullOrEmpty() ? AppUser.Email : uInfo.Email;
                        if (ccMailID.IsNullOrEmpty())
                            ccMailID = AppUser.Email;
                        else
                        {
                            ccMailID = ccMailID + ";" + AppUser.Email;
                        }

                        if (isgDTO.IsNotNull())
                        {
                            toMailID += isgDTO.ToMailID.IsNullOrEmpty() ? "" : ";" + isgDTO.ToMailID;
                            ccMailID += isgDTO.CCMailID.IsNullOrEmpty() ? "" : ";" + isgDTO.CCMailID;
                            bccMailID = isgDTO.BCCMailID;
                        }
                    }
                    else
                    {
                        fromMailID = "erpnoreply@epylliongroup.com";
                        password = "Ugr7jT5d";
                        toMailID = "anupam@epylliongroup.com;abdussalam@epylliongroup.com;litonekl@epylliongroup.com;imrezratin@epylliongroup.com";
                        ccMailID = "";
                        bccMailID = "";
                    }

                    if (uInfo.IsNotNull())
                    {
                        messageBody = String.Format(@"Dear Sir,
                                             <br/><br/>
                                             Please see the attached file of Yarn Projection Booking no: {0}{4} For any further instruction or query regarding this purchase please feel free to contact.
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
                                            ", entity.PYBookingNo, AppUser.EmployeeName, uInfo.Designation, uInfo.Department, revisionText);
                    }

                    String fileName = String.Empty;
                    fileName = $"{entity.PYBookingNo}.pdf";
                    if (attachment.IsNull()) attachment = Array.Empty<byte>();
                    await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, subject, messageBody, fileName, attachment);
                }
                */
                #endregion Send Mail

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw ex;
            }
            return Ok();
        }


        [HttpPost]
        [Route("reject/{id}/{reason}")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            ProjectionYarnBookingMaster entity = await _service.GetAllAsync(id);
            entity.SendToApprover = false;
            entity.IsReject = true;
            entity.RejectReason = reason;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);

            return Ok();
        }

        [HttpPost]
        [Route("unacknowledge/{id}/{reason}")]
        public async Task<IActionResult> Unacknowledge(int id, string reason)
        {
            ProjectionYarnBookingMaster entity = await _service.GetAllAsync(id);
            entity.IsUnacknowledge = true;
            entity.UnacknowledgeBy = AppUser.UserCode;
            entity.UnacknowledgeDate = DateTime.Now;
            entity.UnacknowledgeReason = reason;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);

            return Ok();
        }
        /* //OFF FOR CORE
        [HttpGet]
        [Route("sendMail/{pyBookingID}")]
        public async Task<IActionResult> SendMail(int pyBookingID)
        {
            bool isSendMail = false;
            try
            {
                  //If have to change anything in this mail section, also need to change in save method
                  //Search :
                  // #region Send Mail
                  //if (model.IsApprove)

                ProjectionYarnBookingMaster entity = await _service.GetAllAsync(pyBookingID);

                EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();

                var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Projection Booking");
                if (entity.DepertmentDescription.ToLower() == "R&D".ToLower())
                {
                    isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Projection Booking R & D");
                }
                else if (entity.DepertmentDescription.ToLower() == "Supply Chain".ToLower())
                {
                    isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Projection Booking Supply Chain");
                }
                else if (entity.DepertmentDescription.ToLower() == "Marketing & Merchandising".ToLower())
                {
                    isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Projection Booking Marketing & Merchandising");
                }

                var uInfo = await _emailService.GetUserEmailInfoAsync(AppUser.UserCode);
                List<EPYSLTEX.Service.Reporting.CustomeParameter> paramList = new List<Service.Reporting.CustomeParameter>();
                paramList.Add(new Service.Reporting.CustomeParameter { ParameterName = "PYBookingNo", ParameterValue = entity.PYBookingNo });

                var attachment = await _reportingService.GetPdfByteByReportName("YarnProjectionBooking.rdl", AppUser.UserCode, paramList);

                String EditType = entity.RevisionNo > 0 ? "Revise " : "";
                String subject = $@"{EditType}Yarn Projection Booking No {entity.PYBookingNo}";
                String fromMailID = "";
                String password = "";
                String toMailID = "";
                String ccMailID = "";
                String bccMailID = "";
                String messageBody = "";

                if (Request.Headers.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
                {
                    fromMailID = AppUser.Email;
                    password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
                    //toMailID = uInfo.Email.IsNullOrEmpty() ? AppUser.Email : uInfo.Email;
                    if (ccMailID.IsNullOrEmpty())
                        ccMailID = AppUser.Email;
                    else
                    {
                        ccMailID = ccMailID + ";" + AppUser.Email;
                    }

                    if (isgDTO.IsNotNull())
                    {
                        toMailID += isgDTO.ToMailID.IsNullOrEmpty() ? "" : ";" + isgDTO.ToMailID;
                        ccMailID += isgDTO.CCMailID.IsNullOrEmpty() ? "" : ";" + isgDTO.CCMailID;
                        bccMailID = isgDTO.BCCMailID;
                    }
                }
                else
                {
                    fromMailID = "erpnoreply@epylliongroup.com";
                    password = "Ugr7jT5d";
                    toMailID = "anupam@epylliongroup.com;abdussalam@epylliongroup.com;litonekl@epylliongroup.com";
                    ccMailID = "";
                    bccMailID = "";
                }

                if (uInfo.IsNotNull())
                {
                    messageBody = String.Format(@"Dear Sir,
                                             <br/><br/>
                                             Please see the attached file of Yarn Projection Booking no: {0}. For any further instruction or query regarding this purchase please feel free to contact.
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
                                            ", entity.PYBookingNo, AppUser.EmployeeName, uInfo.Designation, uInfo.Department);
                }

                String fileName = String.Empty;
                fileName = $"{entity.PYBookingNo}.pdf";
                await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, subject, messageBody, fileName, attachment);

                isSendMail = true;
                return Ok(isSendMail);
            }
            catch (Exception ex)
            {
                isSendMail = false;
            }
            return Ok(isSendMail);
        }
        */
    }
}
