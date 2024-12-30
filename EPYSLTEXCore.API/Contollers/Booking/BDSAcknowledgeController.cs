using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Booking;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.CountEntities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;


namespace EPYSLTEXCore.API.Contollers.Booking
{
    [Route("api/bds-acknowledge")]
    public class KnittingProgramBDSController : ApiBaseController
    {
        bool _isValidMailSending = true;
        private readonly IBDSAcknowledgeService _service;
        private readonly IFreeConceptService _fcService;
        private readonly IFreeConceptMRService _fcMRService;
        private readonly IYarnBookingService _serviceYB;
        //private readonly ItemMasterRepository<YarnBookingChild> _yarnItemMasterRepository;
        //private readonly ChildItemMasterRepository<YarnBookingChildItem> _yarnChildItemMasterRepository;
        private readonly IItemMasterService<YarnBookingChild> _yarnItemMasterRepository;
        private readonly IChildItemMasterService<YarnBookingChildItem> _yarnChildItemMasterRepository;
        //private readonly IEmailService _emailService;
        //private readonly IReportingService _reportingService;


        //private static Logger _logger;
        private readonly ICommonHelperService _commonService;

        private readonly IFreeConceptMRService _serviceFreeConceptMR;
        private readonly IFBookingAcknowledgeService _fbaService;

        public KnittingProgramBDSController(
             //IEmailService emailService
             //, IReportingService reportingService
             IUserService userService
            , IBDSAcknowledgeService KnittingProgramBDSService
            , IFreeConceptService FreeConceptService
            , IFreeConceptMRService fcMRService
            , IFreeConceptMRService serviceFreeConceptMR
            , ICommonHelperService commonService
            , IYarnBookingService serviceYB
            , IItemMasterService<YarnBookingChild> yarnItemMasterRepository
            , IChildItemMasterService<YarnBookingChildItem> yarnChildItemMasterRepository
            , IFBookingAcknowledgeService fbaService) : base(userService)
        {
            _service = KnittingProgramBDSService;
            _fcService = FreeConceptService;
            _fcMRService = fcMRService;
            //_emailService = emailService;
            //_reportingService = reportingService;
            _commonService = commonService;
            _serviceFreeConceptMR = serviceFreeConceptMR;
            _serviceYB = serviceYB;
            _yarnItemMasterRepository = yarnItemMasterRepository;
            _yarnChildItemMasterRepository = yarnChildItemMasterRepository;
            _fbaService = fbaService;
            //_logger = LogManager.GetCurrentClassLogger();


#if DEBUG
            _isValidMailSending = false;
#else
            _isValidMailSending=true;
#endif

        }

        [Route("list")]
        public async Task<IActionResult> GetList(Status status, int isBDS)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<FBookingAcknowledge> records = await _service.GetPagedAsync(status, isBDS, paginationInfo, AppUser);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("bulk/list")]
        public async Task<IActionResult> GetBulkList(Status status, int paramTypeId)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<FBookingAcknowledge> records = await _service.GetBulkPagedAsync(status, paginationInfo, AppUser, paramTypeId);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("labdip/list")]
        public async Task<IActionResult> GetLabDipList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<FBookingAcknowledge> records = await _service.GetLabDipPagedAsync(status, paginationInfo, AppUser);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("TNAlist")]
        public async Task<IActionResult> GetTNAlist()
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<BDSDependentTNACalander> records = await _service.GetPagedAsyncTNA(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("Eventlist")]
        public async Task<IActionResult> GetEventlist()
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<BDSDependentTNACalander> records = await _service.GetPagedAsyncEventlist(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("boookingIdList")]
        public async Task<IActionResult> GetboookingIdList()
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<BDSDependentTNACalander> records = await _service.GetBoookingList(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("bookingList")]
        public async Task<IActionResult> GetjobCardMasterNo(DateTime FromDate, DateTime TotDate)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<BDSDependentTNACalander> records = await _service.GetbookingWiseList(paginationInfo, FromDate, TotDate);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
            //return Ok(await _service.GetbookingWiseList());
        }

        [HttpGet]
        [Route("bookingWiseList")]
        public async Task<IActionResult> GetBookingwiseTNA(string ListData)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<BDSDependentTNACalander> records = await _service.GetbookingWiseTNAList(paginationInfo, ListData);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
            //return Ok(await _service.GetbookingWiseList());
        }

        [HttpGet]
        [Route("EventWiseList")]
        public async Task<IActionResult> GetEventWiseTNA(string EventListData)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<BDSDependentTNACalander> records = await _service.GetEventWiseTNA(paginationInfo, EventListData);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
            //return Ok(await _service.GetbookingWiseList());
        }

        [HttpGet]
        [Route("EventDescriptionList")]
        public async Task<IActionResult> GetEventList(int EventID)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<BDSDependentTNACalander> records = await _service.GetEventWiseList(paginationInfo, EventID);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
            //return Ok(await _service.GetbookingWiseList());
        }

        [HttpGet]
        [Route("new/{bookingId}")]
        public async Task<IActionResult> GetNew(int bookingId)
        {
            FBookingAcknowledge data = await _service.GetNewAsync(bookingId);
            return Ok(data);
        }
        [HttpGet]
        [Route("new/forRevise/{bookingId}")]
        public async Task<IActionResult> GetNewForRevise(int bookingId)
        {
            FBookingAcknowledge data = await _service.GetNewForReviseAsync(bookingId);
            return Ok(data);
        }
        [HttpGet]
        [Route("{fbAckId}")]
        public async Task<IActionResult> GetData(int fbAckId)
        {
            FBookingAcknowledge data = await _service.GetDataAsync(fbAckId);
            return Ok(data);
        }
        [HttpGet]
        [Route("getRefSourceItem/{bookingID}/{consumptionID}")]
        public async Task<IActionResult> GetRefSourceItem(int bookingID, int consumptionID)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetRefSourceItem(paginationInfo, bookingID, consumptionID);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("save")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> Save(dynamic jsnString)
        {
            FBookingAcknowledge modelDynamic = JsonConvert.DeserializeObject<FBookingAcknowledge>(
              Convert.ToString(jsnString),
              new JsonSerializerSettings
              {
                  DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
              });

            DateTime currentDate = DateTime.Now;

            //Return error from service if any child has missing any criteria then check

            FBookingAcknowledge entity = modelDynamic;// models.FirstOrDefault();
            string pageName = modelDynamic.PageName;
            bool isBulkBooking = modelDynamic.IsBulkBooking;
            string grpConceptNo = modelDynamic.grpConceptNo;
            int isBDS = modelDynamic.IsBDS;
            string statusText = modelDynamic.StatusText;

            bool isRevised = modelDynamic.IsRevised;
            int preRevisionNo = modelDynamic.PreRevisionNo;

            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<BDSDependentTNACalander> BDCalander = new List<BDSDependentTNACalander>();

            var BDSTNAEvent = await _service.GetAllAsyncBDSTNAEvent_HK();

            entity.IsSample = true;

            if (entity.IsUnAcknowledge)
            {
                entity.IsUnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.EmployeeCode;
                entity.UnAcknowledgeDate = currentDate;
                entity.UnAcknowledgeReason = entity.UnAcknowledgeReason;
            }
            if (grpConceptNo != "" && grpConceptNo != null)
            {
                //var a = await _fcService.GetDatasAsync(grpConceptNo);
                entities.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.FBookingChildCollor.SetUnchanged();
                    x.FBookingAcknowledgeChildAddProcess.SetUnchanged();
                    x.FBookingAcknowledgeChildGarmentPart.SetUnchanged();
                });
            }
            else
            {
                List<FBookingAcknowledgeChild> entityChilds = new List<FBookingAcknowledgeChild>();
                List<FBookingAcknowledgeChildAddProcess> entityChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
                List<FBookingAcknowledgeChildDetails> entityChildDetails = new List<FBookingAcknowledgeChildDetails>();
                List<FBookingAcknowledgeChildGarmentPart> entityChildsGpart = new List<FBookingAcknowledgeChildGarmentPart>();
                List<FBookingAcknowledgeChildProcess> entityChildsProcess = new List<FBookingAcknowledgeChildProcess>();
                List<FBookingAcknowledgeChildText> entityChildsText = new List<FBookingAcknowledgeChildText>();
                List<FBookingAcknowledgeChildDistribution> entityChildsDistribution = new List<FBookingAcknowledgeChildDistribution>();
                List<FBookingAcknowledgeChildYarnSubBrand> entityChildsYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
                List<FBookingAcknowledgeImage> entityChildsImage = new List<FBookingAcknowledgeImage>();

                List<FBookingAcknowledgeChild> ackChilds = new List<FBookingAcknowledgeChild>();

                entity.FBookingChild.ForEach(item =>
                {
                    FBookingAcknowledgeChild ObjEntityChild = new FBookingAcknowledgeChild();
                    item.DateAdded = currentDate;
                    item.AddedBy = AppUser.UserCode;
                    ObjEntityChild = CommonFunction.DeepClone(item);
                    entityChilds.Add(ObjEntityChild);

                    FBookingAcknowledgeChildAddProcess ObjEntityChildAddProces = new FBookingAcknowledgeChildAddProcess();
                    ObjEntityChildAddProces.BookingCAddProcessID = 0;
                    ObjEntityChildAddProces.BookingChildID = item.BookingChildID;
                    ObjEntityChildAddProces.BookingID = item.BookingID;
                    ObjEntityChildAddProces.ConsumptionID = item.ConsumptionID;
                    ObjEntityChildAddProces.ProcessID = 0;
                    entityChildAddProcess.Add(ObjEntityChildAddProces);

                    FBookingAcknowledgeChildDetails ObjEntityChildDetails = new FBookingAcknowledgeChildDetails();
                    ObjEntityChildDetails.BookingCDetailsID = 0;
                    ObjEntityChildDetails.BookingChildID = item.BookingChildID;
                    ObjEntityChildDetails.BookingID = item.BookingID;
                    ObjEntityChildDetails.ConsumptionID = item.ConsumptionID;
                    ObjEntityChildDetails.ItemGroupID = item.ItemGroupID;
                    ObjEntityChildDetails.SubGroupID = item.SubGroupID;
                    ObjEntityChildDetails.ItemMasterID = item.ItemMasterID;
                    ObjEntityChildDetails.OrderBankPOID = item.OrderBankPOID;
                    ObjEntityChildDetails.Color = item.Color;
                    ObjEntityChildDetails.ColorID = item.ColorID;
                    ObjEntityChildDetails.SizeID = item.SizeID;
                    ObjEntityChildDetails.TechPackID = item.TechPackID;
                    ObjEntityChildDetails.ConsumptionQty = item.ConsumptionQty;
                    ObjEntityChildDetails.BookingQty = item.BookingQty;
                    ObjEntityChildDetails.BookingUnitID = item.BookingUnitID;
                    ObjEntityChildDetails.RequisitionQty = item.RequisitionQty;
                    ObjEntityChildDetails.AddedBy = AppUser.UserCode;
                    ObjEntityChildDetails.ExecutionCompanyID = item.ExecutionCompanyID;
                    ObjEntityChildDetails.TechnicalNameId = item.TechnicalNameId;
                    ObjEntityChildDetails.TechnicalName = item.TechnicalName;
                    ObjEntityChildDetails.DateAdded = item.DateAdded;
                    entityChildDetails.Add(ObjEntityChildDetails);

                    FBookingAcknowledgeChildGarmentPart ObjEntityChildsGpart = new FBookingAcknowledgeChildGarmentPart();
                    ObjEntityChildsGpart.BookingCGPID = 0;
                    ObjEntityChildsGpart.BookingChildID = item.BookingChildID;
                    ObjEntityChildsGpart.BookingID = item.BookingID;
                    ObjEntityChildsGpart.ConsumptionID = item.ConsumptionID;
                    ObjEntityChildsGpart.FUPartID = item.FUPartID;
                    entityChildsGpart.Add(ObjEntityChildsGpart);

                    FBookingAcknowledgeChildProcess ObjEntityChildsProcess = new FBookingAcknowledgeChildProcess();
                    ObjEntityChildsProcess.BookingCProcessID = 0;
                    ObjEntityChildsProcess.BookingChildID = item.BookingChildID;
                    ObjEntityChildsProcess.BookingID = item.BookingID;
                    ObjEntityChildsProcess.ConsumptionID = item.ConsumptionID;
                    ObjEntityChildsProcess.ProcessID = 0;
                    entityChildsProcess.Add(ObjEntityChildsProcess);

                    FBookingAcknowledgeChildText ObjEntityChildsText = new FBookingAcknowledgeChildText();
                    ObjEntityChildsText.TextID = 0;
                    ObjEntityChildsText.BookingChildID = item.BookingChildID;
                    ObjEntityChildsText.BookingID = item.BookingID;
                    ObjEntityChildsText.ConsumptionID = item.ConsumptionID;
                    entityChildsText.Add(ObjEntityChildsText);

                    item.ChildsDistribution.ForEach(dis =>
                    {
                        FBookingAcknowledgeChildDistribution obj = new FBookingAcknowledgeChildDistribution();
                        obj.DistributionID = 0;
                        obj.BookingChildID = item.BookingChildID;
                        obj.BookingID = item.BookingID;
                        obj.ConsumptionID = item.ConsumptionID;
                        obj.DeliveryDate = dis.DeliveryDate;
                        obj.DistributionQty = dis.DistributionQty;
                        entityChildsDistribution.Add(obj);
                    });

                    FBookingAcknowledgeChildYarnSubBrand ObjEntityChildsYarnSubBrand = new FBookingAcknowledgeChildYarnSubBrand();
                    ObjEntityChildsYarnSubBrand.BookingCYSubBrandID = 0;
                    ObjEntityChildsYarnSubBrand.BookingChildID = item.BookingChildID;
                    ObjEntityChildsYarnSubBrand.BookingID = item.BookingID;
                    ObjEntityChildsYarnSubBrand.ConsumptionID = item.ConsumptionID;
                    ObjEntityChildsYarnSubBrand.YarnSubBrandID = 0;
                    entityChildsYarnSubBrand.Add(ObjEntityChildsYarnSubBrand);

                    FBookingAcknowledgeImage ObjEntityChildsImage = new FBookingAcknowledgeImage();
                    ObjEntityChildsImage.ChildImgID = 0;
                    ObjEntityChildsImage.BookingID = item.BookingID;
                    ObjEntityChildsImage.ExportOrderID = item.ExportOrderID;
                    entityChildsImage.Add(ObjEntityChildsImage);
                });

                entityChildsImage = entityChildsImage.GroupBy(x => x.BookingID).Select(y => y.First()).ToList();

                var fbMaster = new FBookingAcknowledge();
                if (entity.FBAckID > 0)
                {
                    List<FBookingAcknowledgeChild> newEntityChilds = new List<FBookingAcknowledgeChild>();
                    List<FBookingAcknowledgeChildAddProcess> newEntityChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
                    List<FBookingAcknowledgeChildDetails> newEntityChildDetails = new List<FBookingAcknowledgeChildDetails>();
                    List<FBookingAcknowledgeChildGarmentPart> newEntityChildsGpart = new List<FBookingAcknowledgeChildGarmentPart>();
                    List<FBookingAcknowledgeChildProcess> newEntityChildsProcess = new List<FBookingAcknowledgeChildProcess>();
                    List<FBookingAcknowledgeChildText> newEntityChildsText = new List<FBookingAcknowledgeChildText>();
                    List<FBookingAcknowledgeChildDistribution> newEntityChildsDistribution = new List<FBookingAcknowledgeChildDistribution>();
                    List<FBookingAcknowledgeChildYarnSubBrand> newEntityChildsYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
                    List<FBAChildPlanning> newFBAChildPlanning = new List<FBAChildPlanning>();
                    List<BDSDependentTNACalander> newBDCalander = new List<BDSDependentTNACalander>();

                    fbMaster = await _service.GetFBAcknowledge(entity.FBAckID);

                    if (statusText == "ReviseLabDip")
                    {
                        fbMaster.RevisionNoLabdip = fbMaster.PreRevisionNoLabdip;
                        fbMaster.IsUnAcknowledge = false;
                    }

                    if (isBulkBooking)
                    {
                        fbMaster.FBookingChild = CommonFunction.DeepClone(fbMaster.FBChilds);

                        fbMaster.FBookingChild.SetModified();
                        fbMaster.FBookingChild.ForEach(x => x.FBAChildPlannings.SetModified());
                        fbMaster.FBookingAcknowledgeChildAddProcess.SetModified();
                        fbMaster.FBookingChildDetails.SetModified();
                        fbMaster.FBookingAcknowledgeChildGarmentPart.SetModified();
                        fbMaster.FBookingAcknowledgeChildProcess.SetModified();
                        fbMaster.FBookingAcknowledgeChildText.SetModified();
                        fbMaster.FBookingAcknowledgeChildDistribution.SetModified();
                        fbMaster.FBookingAcknowledgeChildYarnSubBrand.SetModified();
                        fbMaster.BDSDependentTNACalander.SetModified();
                        fbMaster.FBookingAcknowledgeImage.SetModified();
                    }
                    else
                    {
                        fbMaster.EntityState = EntityState.Unchanged;
                        fbMaster.FBookingChild.SetUnchanged();
                        fbMaster.FBookingChild.ForEach(x => x.FBAChildPlannings.SetUnchanged());
                        fbMaster.FBookingAcknowledgeChildAddProcess.SetUnchanged();
                        fbMaster.FBookingChildDetails.SetUnchanged();
                        fbMaster.FBookingAcknowledgeChildGarmentPart.SetUnchanged();
                        fbMaster.FBookingAcknowledgeChildProcess.SetUnchanged();
                        fbMaster.FBookingAcknowledgeChildText.SetUnchanged();
                        fbMaster.FBookingAcknowledgeChildDistribution.SetUnchanged();
                        fbMaster.FBookingAcknowledgeChildYarnSubBrand.SetUnchanged();
                        fbMaster.BDSDependentTNACalander.SetUnchanged();
                        fbMaster.FBookingAcknowledgeImage.SetUnchanged();
                    }

                    if (isBulkBooking) entity.FBookingAcknowledgeList = await _service.GetFBAcknowledgeMasterBulk(entity.BookingNo);

                    entityChilds.ForEach(bookingChild =>
                    {
                        bookingChild.ChildAddProcess = entityChildAddProcess.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.FBChildDetails = entityChildDetails.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.ChildsGpart = entityChildsGpart.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.ChildsProcess = entityChildsProcess.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.ChildsText = entityChildsText.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.ChildsDistribution = entityChildsDistribution.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.ChildsYarnSubBrand = entityChildsYarnSubBrand.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.BDCalander = BDCalander.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                    });
                    int newBookingChildId = 0;

                    entityChilds.ForEach(item =>
                    {
                        newFBAChildPlanning = new List<FBAChildPlanning>();
                        if (isBDS != 2)
                        {
                            item.AcknowledgeID = entity.FBAckID;
                        }
                        FBookingAcknowledgeChild fbChild = fbMaster.FBookingChild.Find(x => x.BookingChildID == item.BookingChildID);
                        if (fbChild != null)
                        {
                            int bookingChildID = fbChild.BookingChildID;
                            item.BookingChildID = bookingChildID;
                            item.DateUpdated = currentDate;
                            item.EntityState = EntityState.Modified;

                            item.ChildAddProcess.ForEach(obj =>
                            {
                                FBookingAcknowledgeChildAddProcess tempObj = fbMaster.FBookingAcknowledgeChildAddProcess.Find(x => x.BookingChildID == obj.BookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingCAddProcessID = tempObj.BookingCAddProcessID;
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildAddProcess.Add(obj);
                                }
                                else
                                {
                                    FBookingAcknowledgeChildAddProcess newObj = new FBookingAcknowledgeChildAddProcess();
                                    newObj.BookingCAddProcessID = 0;
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newEntityChildAddProcess.Add(newObj);
                                }
                            });

                            item.FBChildDetails.ForEach(obj =>
                            {
                                FBookingAcknowledgeChildDetails tempObj = fbMaster.FBookingChildDetails.Find(x => x.BookingChildID == obj.BookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingCDetailsID = tempObj.BookingCDetailsID;
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildDetails.Add(obj);
                                }
                                else
                                {
                                    FBookingAcknowledgeChildDetails newObj = new FBookingAcknowledgeChildDetails();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newObj.ItemGroupID = item.ItemGroupID;
                                    newObj.SubGroupID = item.SubGroupID;
                                    newObj.ItemMasterID = item.ItemMasterID;
                                    newObj.OrderBankPOID = item.OrderBankPOID;
                                    newObj.ColorID = item.ColorID;
                                    newObj.Color = item.Color;
                                    newObj.SizeID = item.SizeID;
                                    newObj.TechPackID = item.TechPackID;
                                    newObj.ConsumptionQty = item.ConsumptionQty;
                                    newObj.BookingQty = item.BookingQty;
                                    newObj.BookingUnitID = item.BookingUnitID;
                                    newObj.RequisitionQty = item.RequisitionQty;
                                    newObj.ExecutionCompanyID = item.ExecutionCompanyID;
                                    newObj.TechnicalNameId = item.TechnicalNameId;

                                    newObj.AddedBy = entity.AddedBy;
                                    newObj.DateAdded = currentDate;
                                    newEntityChildDetails.Add(newObj);
                                }
                            });

                            item.ChildsGpart.ForEach(obj =>
                            {
                                FBookingAcknowledgeChildGarmentPart tempObj = fbMaster.FBookingAcknowledgeChildGarmentPart.Find(x => x.BookingChildID == obj.BookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingCGPID = tempObj.BookingCGPID;
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildsGpart.Add(obj);
                                }
                                else
                                {
                                    FBookingAcknowledgeChildGarmentPart newObj = new FBookingAcknowledgeChildGarmentPart();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newObj.FUPartID = item.FUPartID;
                                    newEntityChildsGpart.Add(newObj);
                                }
                            });

                            item.ChildsProcess.ForEach(obj =>
                            {
                                FBookingAcknowledgeChildProcess tempObj = fbMaster.FBookingAcknowledgeChildProcess.Find(x => x.BookingChildID == obj.BookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingCProcessID = tempObj.BookingCProcessID;
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildsProcess.Add(obj);
                                }
                                else
                                {
                                    FBookingAcknowledgeChildProcess newObj = new FBookingAcknowledgeChildProcess();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newEntityChildsProcess.Add(newObj);
                                }
                            });

                            item.ChildsText.ForEach(obj =>
                            {
                                FBookingAcknowledgeChildText tempObj = fbMaster.FBookingAcknowledgeChildText.Find(x => x.BookingChildID == obj.BookingChildID);
                                if (tempObj != null)
                                {
                                    obj.TextID = tempObj.TextID;
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildsText.Add(obj);
                                }
                                else
                                {
                                    FBookingAcknowledgeChildText newObj = new FBookingAcknowledgeChildText();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newEntityChildsText.Add(newObj);
                                }
                            });

                            item.ChildsDistribution.ForEach(obj =>
                            {
                                FBookingAcknowledgeChildDistribution tempObj = fbMaster.FBookingAcknowledgeChildDistribution.Find(x => x.BookingChildID == obj.BookingChildID);
                                if (tempObj != null)
                                {
                                    obj.DistributionID = tempObj.DistributionID;
                                    obj.BookingChildID = bookingChildID;
                                    obj.BookingID = item.BookingID;
                                    obj.ConsumptionID = item.ConsumptionID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildsDistribution.Add(obj);
                                }
                                else
                                {
                                    FBookingAcknowledgeChildDistribution newObj = new FBookingAcknowledgeChildDistribution();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newEntityChildsDistribution.Add(newObj);
                                }
                            });

                            item.ChildsYarnSubBrand.ForEach(obj =>
                            {
                                FBookingAcknowledgeChildYarnSubBrand tempObj = fbMaster.FBookingAcknowledgeChildYarnSubBrand.Find(x => x.BookingChildID == obj.BookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingCYSubBrandID = tempObj.BookingCYSubBrandID;
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildsYarnSubBrand.Add(obj);
                                }
                                else
                                {
                                    FBookingAcknowledgeChildYarnSubBrand newObj = new FBookingAcknowledgeChildYarnSubBrand();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newEntityChildsYarnSubBrand.Add(newObj);
                                }
                            });

                            if (item.CriteriaIDs.IsNotNullOrEmpty())
                            {
                                string[] criteriaIds = CommonFunction.DeepClone(item.CriteriaIDs.Split(',').Where(cr => cr.Length > 0).Distinct().ToArray());
                                if (criteriaIds.Length > 0 && !criteriaIds[0].Equals(""))
                                {
                                    foreach (string criteriaID in criteriaIds)
                                    {
                                        if (criteriaID.IsNotNullOrEmpty())
                                        {
                                            var obj = fbChild.FBAChildPlannings.FirstOrDefault(x => x.CriteriaID == Convert.ToInt32(criteriaID));
                                            if (obj != null)
                                            {
                                                obj.BookingChildID = bookingChildID;
                                                obj.AcknowledgeID = item.AcknowledgeID;
                                                obj.CriteriaID = obj.CriteriaID;
                                                obj.EntityState = EntityState.Modified;
                                                newFBAChildPlanning.Add(obj);

                                                #region Delete Duplicate Criteria BookingChildID wise

                                                var fBACPlannings = fbChild.FBAChildPlannings.Where(x => x.CriteriaID == Convert.ToInt32(criteriaID)).ToList();
                                                if (fBACPlannings.Count() > 1)
                                                {
                                                    int count = 0;
                                                    fBACPlannings.ForEach(x =>
                                                    {
                                                        count++;
                                                        if (count > 1)
                                                        {
                                                            x.EntityState = EntityState.Deleted;
                                                            newFBAChildPlanning.Add(x);
                                                        }
                                                    });
                                                }

                                                #endregion Delete Duplicate Criteria BookingChildID wise
                                            }
                                            else
                                            {
                                                var newObj = new FBAChildPlanning();
                                                newObj.BookingChildID = bookingChildID;
                                                newObj.AcknowledgeID = item.AcknowledgeID;
                                                newObj.CriteriaID = Convert.ToInt32(criteriaID);
                                                newFBAChildPlanning.Add(newObj);
                                            }
                                        }
                                    }
                                    List<int> deletedFBACIds = new List<int>();
                                    fbChild.FBAChildPlannings.ForEach(x =>
                                    {
                                        if (!criteriaIds.Contains(x.CriteriaID.ToString()))
                                        {
                                            deletedFBACIds.Add(x.CriteriaID);
                                        }
                                    });
                                    deletedFBACIds.ForEach(criteriaID =>
                                    {
                                        var obj = fbChild.FBAChildPlannings.Find(x => x.CriteriaID == criteriaID);
                                        obj.EntityState = EntityState.Deleted;
                                        newFBAChildPlanning.Add(obj);
                                    });
                                }
                                item.FBAChildPlannings = CommonFunction.DeepClone(newFBAChildPlanning);
                            }
                            else
                            {
                                #region Delete Duplicate Criteria BookingChildID wise

                                newFBAChildPlanning = new List<FBAChildPlanning>();
                                string[] criteriaIDs = string.Join(",", fbChild.FBAChildPlannings.Select(x => x.CriteriaID).Distinct()).Split(',');
                                foreach (string critentialId in criteriaIDs)
                                {
                                    var fBACPlannings = fbChild.FBAChildPlannings.Where(x => x.CriteriaID == Convert.ToInt32(critentialId)).ToList();
                                    if (fBACPlannings.Count() > 1)
                                    {
                                        int count = 0;
                                        fBACPlannings.ForEach(x =>
                                        {
                                            count++;
                                            if (count > 1)
                                            {
                                                x.EntityState = EntityState.Deleted;
                                                newFBAChildPlanning.Add(x);
                                            }
                                        });
                                    }
                                }
                                item.FBAChildPlannings = CommonFunction.DeepClone(newFBAChildPlanning);

                                #endregion Delete Duplicate Criteria BookingChildID wise
                            }
                        }
                        else
                        {
                            newBookingChildId++;
                            item.BookingChildID = newBookingChildId;
                            item.EntityState = EntityState.Added;
                            item.DateAdded = currentDate;

                            item.ChildAddProcess.ForEach(childAddProc =>
                            {
                                var obj = CommonFunction.DeepClone(childAddProc);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.EntityState = EntityState.Added;
                                newEntityChildAddProcess.Add(obj);
                            });

                            item.FBChildDetails.ForEach(ChildDetail =>
                            {
                                FBookingAcknowledgeChildDetails obj = CommonFunction.DeepClone(ChildDetail);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.BookingQty = item.BookingQty;
                                obj.ItemGroupID = item.ItemGroupID;
                                obj.SubGroupID = item.SubGroupID;
                                obj.ItemMasterID = item.ItemMasterID;
                                obj.OrderBankPOID = item.OrderBankPOID;
                                obj.ColorID = item.ColorID;
                                obj.Color = item.Color;
                                obj.SizeID = item.SizeID;
                                obj.TechPackID = item.TechPackID;
                                obj.ConsumptionQty = item.ConsumptionQty;
                                obj.BookingUnitID = item.BookingUnitID;
                                obj.RequisitionQty = item.RequisitionQty;
                                obj.ExecutionCompanyID = item.ExecutionCompanyID;
                                obj.TechnicalNameId = item.TechnicalNameId;

                                obj.EntityState = EntityState.Added;
                                newEntityChildDetails.Add(obj);
                            });

                            item.ChildsGpart.ForEach(ChildsGp =>
                            {
                                FBookingAcknowledgeChildGarmentPart obj = CommonFunction.DeepClone(ChildsGp);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.FUPartID = item.FUPartID;
                                obj.EntityState = EntityState.Added;
                                newEntityChildsGpart.Add(obj);
                            });

                            item.ChildsProcess.ForEach(ChildsProc =>
                            {
                                FBookingAcknowledgeChildProcess obj = CommonFunction.DeepClone(ChildsProc);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.EntityState = EntityState.Added;
                                newEntityChildsProcess.Add(obj);
                            });

                            item.ChildsText.ForEach(ChildTxt =>
                            {
                                FBookingAcknowledgeChildText obj = CommonFunction.DeepClone(ChildTxt);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.EntityState = EntityState.Added;
                                newEntityChildsText.Add(obj);
                            });

                            item.ChildsDistribution.ForEach(ChildDis =>
                            {
                                FBookingAcknowledgeChildDistribution obj = CommonFunction.DeepClone(ChildDis);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.DeliveryDate = ChildDis.DeliveryDate;
                                obj.DistributionQty = ChildDis.DistributionQty;
                                obj.EntityState = EntityState.Added;
                                newEntityChildsDistribution.Add(obj);
                            });

                            item.ChildsYarnSubBrand.ForEach(ChildYarnSubBrand =>
                            {
                                FBookingAcknowledgeChildYarnSubBrand obj = CommonFunction.DeepClone(ChildYarnSubBrand);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.EntityState = EntityState.Added;
                                newEntityChildsYarnSubBrand.Add(obj);
                            });

                            if (item.CriteriaIDs.IsNotNullOrEmpty())
                            {
                                foreach (string criteriaID in item.CriteriaIDs.Split(',').Distinct())
                                {
                                    if (criteriaID.IsNotNullOrEmpty())
                                    {
                                        var obj = new FBAChildPlanning();
                                        obj.BookingChildID = newBookingChildId;
                                        obj.AcknowledgeID = item.AcknowledgeID;
                                        obj.CriteriaID = Convert.ToInt32(criteriaID);
                                        obj.EntityState = EntityState.Added;
                                        newFBAChildPlanning.Add(obj);
                                    }
                                }
                            }
                            item.FBAChildPlannings = newFBAChildPlanning;
                        }
                        newEntityChilds.Add(CommonFunction.DeepClone(item));
                    });

                    fbMaster.FBookingChild.ForEach(x =>
                    {
                        FBookingAcknowledgeChild obj = newEntityChilds.Find(y => y.BookingChildID == x.BookingChildID && y.EntityState != EntityState.Deleted);
                        if (obj == null)
                        {
                            x.EntityState = EntityState.Deleted;
                            newEntityChilds.Add(x);

                            newEntityChildAddProcess.Where(p => p.BookingChildID == x.BookingChildID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                p.EntityState = EntityState.Deleted;
                            });

                            fbMaster.FBookingAcknowledgeChildAddProcess.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                FBookingAcknowledgeChildAddProcess objP = newEntityChildAddProcess.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildAddProcess.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildAddProcess.Add(p);
                                }
                            });
                            fbMaster.FBookingChildDetails.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                FBookingAcknowledgeChildDetails objP = newEntityChildDetails.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildDetails.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildDetails.Add(p);
                                }
                            });
                            fbMaster.FBookingAcknowledgeChildGarmentPart.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                FBookingAcknowledgeChildGarmentPart objP = newEntityChildsGpart.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildsGpart.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildsGpart.Add(p);
                                }
                            });
                            fbMaster.FBookingAcknowledgeChildProcess.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                FBookingAcknowledgeChildProcess objP = newEntityChildsProcess.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildsProcess.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildsProcess.Add(p);
                                }
                            });
                            fbMaster.FBookingAcknowledgeChildText.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                FBookingAcknowledgeChildText objP = newEntityChildsText.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildsText.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildsText.Add(p);
                                }
                            });
                            fbMaster.FBookingAcknowledgeChildDistribution.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                FBookingAcknowledgeChildDistribution objP = newEntityChildsDistribution.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildsDistribution.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildsDistribution.Add(p);
                                }
                            });
                            fbMaster.FBookingAcknowledgeChildYarnSubBrand.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                FBookingAcknowledgeChildYarnSubBrand objP = newEntityChildsYarnSubBrand.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildsYarnSubBrand.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildsYarnSubBrand.Add(p);
                                }
                            });
                            fbMaster.BDSDependentTNACalander.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID).ToList().ForEach(p =>
                            {
                                p.EntityState = EntityState.Deleted;
                                newBDCalander.Add(p);
                            });
                        }
                    });

                    entityChilds = newEntityChilds;
                    entityChildAddProcess = newEntityChildAddProcess;
                    entityChildDetails = newEntityChildDetails;
                    entityChildsGpart = newEntityChildsGpart;
                    entityChildsProcess = newEntityChildsProcess;
                    entityChildsText = newEntityChildsText;
                    entityChildsDistribution = newEntityChildsDistribution;
                    entityChildsYarnSubBrand = newEntityChildsYarnSubBrand;
                    BDCalander = newBDCalander;
                    fbMaster.FBookingAcknowledgeImage.ForEach(x => x.EntityState = EntityState.Modified);
                    entityChildsImage = fbMaster.FBookingAcknowledgeImage;

                    #region For Bulk Booking Knitting Info

                    if (isBulkBooking && modelDynamic.IsKnittingComplete)
                    {

                        isBDS = 2;
                        entity.IsKnittingComplete = true;
                        entity.KnittingCompleteBy = AppUser.UserCode;
                        entity.KnittingCompleteDate = currentDate;

                        entity.EntityState = EntityState.Modified;
                        entity.UpdatedBy = AppUser.UserCode;
                        entity.DateUpdated = currentDate;

                        entity.FBookingAcknowledgeList.ForEach(x =>
                        {
                            x.IsKnittingComplete = true;
                            x.KnittingCompleteBy = AppUser.UserCode;
                            x.KnittingCompleteDate = currentDate;

                            x.EntityState = EntityState.Modified;
                            x.UpdatedBy = AppUser.UserCode;
                            x.DateUpdated = currentDate;
                        });
                        entityChilds.ForEach(x =>
                        {
                            x.EntityState = EntityState.Modified;
                            x.UpdatedBy = AppUser.UserCode;
                            x.DateUpdated = currentDate;
                        });

                        entityChildDetails = new List<FBookingAcknowledgeChildDetails>();
                        entityChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
                        entityChildDetails = new List<FBookingAcknowledgeChildDetails>();
                        entityChildsGpart = new List<FBookingAcknowledgeChildGarmentPart>();
                        entityChildsProcess = new List<FBookingAcknowledgeChildProcess>();
                        entityChildsText = new List<FBookingAcknowledgeChildText>();
                        entityChildsDistribution = new List<FBookingAcknowledgeChildDistribution>();
                        entityChildsYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
                        entityChildsImage = new List<FBookingAcknowledgeImage>();
                        BDCalander = new List<BDSDependentTNACalander>();
                    }
                    #endregion
                }

                if (entity.FBAckID == 0)
                {
                    entity.AddedBy = AppUser.UserCode;
                    entity.DateAdded = currentDate;
                }

                var fabricWastageGrids = await _commonService.GetFabricWastageGridAsync("BDS");

                entityChilds.Where(x => x.EntityState != EntityState.Deleted).ToList().ForEach(details =>
                {
                    BDSTNAEvent.BDSTNAEvent_HKNames.ForEach(hk_details =>
                    {
                        BDSDependentTNACalander objCalender = new BDSDependentTNACalander();
                        objCalender.BookingChildID = details.BookingChildID;
                        objCalender.BookingID = details.BookingID;
                        objCalender.EventID = hk_details.EventID;
                        objCalender.SeqNo = hk_details.SeqNo;
                        objCalender.SystemEvent = hk_details.SystemEvent;
                        objCalender.HasDependent = hk_details.HasDependent;
                        objCalender.BookingDate = currentDate;

                        if (entity.FBAckID > 0)
                        {
                            BDSDependentTNACalander obj = fbMaster.BDSDependentTNACalander.Find(x => x.BookingChildID == details.BookingChildID);
                            if (obj != null)
                            {
                                objCalender.EntityState = EntityState.Modified;
                            }
                        }

                        switch (hk_details.EventID)
                        {
                            case 1:
                                objCalender.EventDate = DateTime.Now.Date.AddDays(details.StructureDays); //(objCalender.BookingDate).AddDays + details.StructureDays;
                                objCalender.CompleteDate = currentDate;
                                objCalender.TNADays = details.StructureDays;
                                break;
                            case 2:
                                objCalender.EventDate = DateTime.Now.Date.AddDays(details.MaterialDays);
                                objCalender.TNADays = details.MaterialDays;
                                break;
                            case 3:
                                objCalender.EventDate = DateTime.Now.Date.AddDays(details.KnittingDays);
                                objCalender.TNADays = details.KnittingDays;
                                break;
                            case 4:
                                objCalender.EventDate = DateTime.Now.Date.AddDays(details.PreprocessDays);
                                objCalender.TNADays = details.PreprocessDays;
                                break;
                            case 5:
                                objCalender.EventDate = DateTime.Now.Date.AddDays(details.BatchPreparationDays);
                                objCalender.TNADays = details.BatchPreparationDays;
                                break;
                            case 6:
                                objCalender.EventDate = DateTime.Now.Date.AddDays(details.DyeingDays);
                                objCalender.TNADays = details.DyeingDays;
                                break;
                            case 7:
                                objCalender.EventDate = DateTime.Now.Date.AddDays(details.FinishingDays);
                                objCalender.TNADays = details.FinishingDays;
                                break;
                            case 8:
                                objCalender.EventDate = DateTime.Now.Date.AddDays(details.QualityDays);
                                objCalender.TNADays = details.QualityDays;
                                break;
                            case 9:
                                objCalender.EventDate = DateTime.Now.Date.AddDays(details.TestReportDays);
                                objCalender.TNADays = details.TestReportDays;
                                break;
                            default:
                                objCalender.EventDate = DateTime.Now.Date.AddDays(details.TotalDays);
                                objCalender.TNADays = details.TotalDays;
                                break;
                        }
                        objCalender.SystemEvent = false;
                        BDCalander.Add(objCalender);
                    });

                    //objCalender

                    if (entity.FBAckID == 0)
                    {
                        List<FBAChildPlanning> planningList = new List<FBAChildPlanning>();
                        string[] criteriaIds = details.CriteriaIDs.Split(',');
                        if (criteriaIds.Length > 0 && !criteriaIds[0].Equals(""))
                        {
                            foreach (string criteria in criteriaIds)
                            {
                                if (!string.IsNullOrEmpty(criteria))
                                    planningList.Add(new FBAChildPlanning { CriteriaID = Convert.ToInt32(criteria) });
                            }
                            details.FBAChildPlannings = planningList;
                        }
                        else
                        {
                            details.FBAChildPlannings = new List<FBAChildPlanning>();
                        }
                    }

                    #region Set FabricWastageGrid Values

                    FabricWastageGrid fabricWastageGrid = new FabricWastageGrid();
                    details.GSM = string.IsNullOrEmpty(details.GSM) ? 0.ToString() : details.GSM;
                    if (Convert.ToInt32(details.GSM) > 0 && details.SubGroupID == 1)
                    {
                        fabricWastageGrid = fabricWastageGrids.Where(x => x.IsFabric == true).ToList().Find(x => x.GSMFrom <= Convert.ToInt32(details.GSM)
                                                                            && Convert.ToInt32(details.GSM) <= x.GSMTo
                                                                            && x.BookingQtyFrom <= details.BookingQty
                                                                            && details.BookingQty <= x.BookingQtyTo);
                    }
                    else if (details.SubGroupID == 11 || details.SubGroupID == 12)
                    {
                        fabricWastageGrid = fabricWastageGrids.Where(x => x.IsFabric == false).ToList().Find(x => x.BookingQtyFrom <= details.BookingQty
                                                                            && details.BookingQty <= x.BookingQtyTo);
                    }
                    if (fabricWastageGrid != null)
                    {
                        if (fabricWastageGrid.FixedQty)
                        {
                            details.ExcessPercentage = 0;
                            if (details.BookingQty > 0)
                            {
                                if (isBDS == 3 || modelDynamic.IsLabdip == true)
                                {
                                    details.ExcessQty = 0;
                                    details.ExcessPercentage = 0;
                                    details.TotalQty = details.BookingQty;
                                }
                                else
                                {
                                    details.ExcessQty = fabricWastageGrid.ExcessQty;
                                    details.TotalQty = details.BookingQty + details.ExcessQty;
                                }

                            }
                            if (details.TotalQtyInKG > 0)
                            {
                                details.ExcessQtyInKG = fabricWastageGrid.ExcessQty;
                                if (isBDS == 3 || modelDynamic.IsLabdip == true)
                                {
                                    details.ExcessQtyInKG = 0;
                                    details.TotalQtyInKG = details.TotalQtyInKG;
                                }
                                else
                                {
                                    details.ExcessQtyInKG = fabricWastageGrid.ExcessQty;
                                    details.TotalQtyInKG = details.TotalQtyInKG + details.ExcessQty;
                                }
                            }
                        }
                        else
                        {
                            details.ExcessPercentage = fabricWastageGrid.ExcessPercentage;
                            if (details.BookingQty > 0)
                            {
                                if (isBDS == 3 || modelDynamic.IsLabdip == true)
                                {
                                    details.ExcessQty = 0;
                                    details.ExcessPercentage = 0;
                                    details.TotalQty = details.BookingQty;
                                }
                                else
                                {
                                    details.ExcessQty = Math.Floor(details.BookingQty * fabricWastageGrid.ExcessPercentage / 100);
                                    details.TotalQty = details.BookingQty + details.ExcessQty;
                                }
                            }
                            if (details.TotalQtyInKG > 0)
                            {
                                if (isBDS == 3 || modelDynamic.IsLabdip == true)
                                {
                                    details.ExcessQtyInKG = 0;
                                    details.TotalQtyInKG = details.TotalQtyInKG;
                                }
                                else
                                {
                                    details.ExcessQtyInKG = Math.Floor(details.BookingQty * fabricWastageGrid.ExcessPercentage / 100);
                                    details.TotalQtyInKG = details.TotalQtyInKG + details.ExcessQtyInKG;
                                }
                            }
                        }
                    }

                    #endregion Set FabricWastageGrid Values
                });


                string ColorIDs = "";
                ColorIDs = string.Join(",", entityChilds.Select(x => x.Color)).ToString();

                entity.ColorCodeList = await _service.GetAllAsyncColorIDs(ColorIDs);

                List<FreeConceptMaster> entityFreeConcepts = new List<FreeConceptMaster>();
                List<FreeConceptMRMaster> freeConceptMRList = new List<FreeConceptMRMaster>();
                string groupConceptNo = entity.BookingNo; //entities.Count() > 0 ? entities[0].BookingNo : "";
                if (groupConceptNo.IsNotNullOrEmpty())
                {
                    entityFreeConcepts = await _fcService.GetDatasAsync(groupConceptNo);
                    entityFreeConcepts.ForEach(x =>
                    {
                        x.EntityState = EntityState.Unchanged;
                        x.ChildColors.SetUnchanged();

                        if (modelDynamic.PageName == "BulkBookingKnittingInfo")
                        {
                            List<FBookingAcknowledgeChild> fBACs = entityChilds.Where(y => y.SubGroupID == x.SubGroupID && y.ItemMasterID == x.ItemMasterID && y.ConsumptionID == x.ConsumptionID).ToList();

                            if (fBACs.IsNotNull() && fBACs.Count() > 0)
                            {
                                var obj = fBACs.First();

                                fBACs.ForEach(y =>
                                {
                                    y.MachineTypeId = obj.MachineTypeId;
                                    y.TechnicalNameId = obj.TechnicalNameId;
                                    y.MachineGauge = obj.MachineGauge;
                                    y.MachineDia = obj.MachineDia;
                                    y.BrandID = obj.BrandID;
                                    y.EntityState = EntityState.Modified;
                                });

                                x.MCSubClassID = obj.MachineTypeId;
                                x.TechnicalNameId = obj.TechnicalNameId;
                                x.MachineGauge = obj.MachineGauge;
                                x.MachineDia = obj.MachineDia;
                                x.BrandID = obj.BrandID;
                                x.TotalQty = obj.TotalQty;
                                x.TotalQtyInKG = obj.TotalQtyInKG;
                                x.ProduceKnittingQty = obj.GreyProdQty;
                                x.EntityState = EntityState.Modified;
                            }
                        }
                    });
                    if (isBDS == 2)
                    {
                        freeConceptMRList = await _serviceFreeConceptMR.GetByGroupConceptNo(groupConceptNo);
                        freeConceptMRList.ForEach(x =>
                        {
                            x.EntityState = EntityState.Unchanged;
                            x.Childs.SetUnchanged();
                        });
                    }
                }

                if (isRevised && !entity.IsUnAcknowledge)
                {
                    entity.PreRevisionNo = preRevisionNo;
                    entity.RevisionNo = entity.RevisionNo + 1;
                    entity.RevisionDate = currentDate;

                    //entityFreeConcepts
                    entityFreeConcepts.ForEach(c =>
                    {
                        c.PreProcessRevNo = entity.RevisionNo;
                        c.RevisionNo = entity.RevisionNo;
                        c.RevisionBy = AppUser.UserCode;
                        c.RevisionDate = currentDate;
                    });
                }

                List<FreeConceptMaster> freeConceptsRevice = new List<FreeConceptMaster>();
                if (entity.BookingNo.IsNotNullOrEmpty())
                {
                    freeConceptsRevice = await _service.GetFreeConcepts(entity.BookingNo);
                    if (isRevised && !entity.IsUnAcknowledge)
                    {
                        freeConceptsRevice.ForEach(c =>
                        {
                            c.PreProcessRevNo = entity.RevisionNo;
                            c.RevisionNo = entity.RevisionNo;
                            c.RevisionBy = AppUser.UserCode;
                            c.RevisionDate = currentDate;
                        });
                    }
                }

                List<SampleBookingConsumption> sampleBookingChilds = new List<SampleBookingConsumption>();
                var childs = entityChilds.Where(x => x.IsFabricReq == true).ToList();
                if (isRevised && childs.Count() > 0)
                {
                    SampleBookingMaster sampleBooking = await _service.GetSampleBooking(entity.BookingID);
                    sampleBookingChilds = sampleBooking.Childs;
                    sampleBookingChilds.ForEach(x =>
                    {
                        var obj = childs.Find(y => y.BookingID == x.BookingID && y.ConsumptionID == x.ConsumptionID);
                        if (obj.IsNotNull())
                        {
                            x.ConsumptionQty = obj.BookingQty;
                            x.IsFabricReq = obj.IsFabricReq;
                            x.EntityState = EntityState.Modified;
                        }
                    });
                }

                if (isBulkBooking)
                {
                    if (modelDynamic.GridStatus == Status.Internal_Rejection)
                    {
                        if (modelDynamic.BtnId == "btnUnAcknowledge")
                        {
                            entity.IsUnAcknowledge = true;
                            entity.UnAcknowledgeReason = modelDynamic.UnAcknowledgeReason;
                            entity.UnAcknowledgeBy = AppUser.UserCode;
                            entity.UnAcknowledgeDate = currentDate;
                        }
                    }
                }

                if (isBDS == EnumBDSType.ProjectionBooking)
                {
                    entity.BaseTypeId = EnumBaseType.ProjectionBasedBulk;
                }
                else if (isBDS == EnumBDSType.BDS)
                {
                    entity.BaseTypeId = EnumBaseType.OrderBasedSample;
                }

                await _service.SaveAsync(entity, entityChilds,
                entityChildAddProcess, entityChildDetails,
                entityChildsGpart, entityChildsProcess,
                entityChildsText, entityChildsDistribution,
                entityChildsYarnSubBrand, entityChildsImage,
                BDCalander, isBDS,
                entityFreeConcepts,
                freeConceptMRList,
                null, null, null, null,
                freeConceptsRevice, sampleBookingChilds,
                isRevised, AppUser.UserCode);


                bool isSendMail = true;
                if (entity.BookingNo.Contains("PB-") || entity.IsBDS == 2)
                {
                    isSendMail = false;
                }
                if (entity.BookingNo.Contains("LR-"))
                {
                    isSendMail = false;
                }
                // 
                //if(pageName == "BDSAcknowledge")
                //{
                //    isSendMail = false;
                //}
                //
                /*//OFF FOR CORE//
                if (isSendMail)
                {
                    #region :: Send Mail

                    try
                    {
                        EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();

                        //OFF FOR CORE//var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "BDS Ack");
                        //OFF FOR CORE//var uInfo = await _emailService.GetUserEmailInfoAsync(entity.BookingBy);

                        //OFF FOR CORE//var buyerTeamWise = await _emailService.GetAllEmployeeMailSetupByUserCodeAndSetupForNameAndBuyerTeam(entity.BuyerTeamID.ToString(), AppUser.UserCode.ToString(), "BDS Ack");

                        //var attachment = await _reportingService.GetPdfByte(990, AppUser.UserCode, entity.BookingNo);
                        var attachment = new byte[] { 0 };
                        String fromMailID = "";
                        String toMailID = "";
                        String ccMailID = "";
                        String bccMailID = "";
                        String password = "";

                        String EditType = entity.PreRevisionNo > 0 ? "Revise " : "";
                        string revisionText = entity.PreRevisionNo > 0 ? " Rev-" + entity.PreRevisionNo.ToString() : "";

                        if (Request.Headers.Host.ToString().ToUpper() == "texerp.epylliongroup.com".ToUpper())
                        {
                            fromMailID = AppUser.Email;
                            password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
                            toMailID = uInfo.Email.IsNullOrEmpty() ? AppUser.Email : uInfo.Email;
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
                            if (buyerTeamWise.IsNotNull())
                            {
                                if (buyerTeamWise.ToMailID.IsNotNullOrEmpty())
                                {
                                    toMailID = toMailID.IsNullOrEmpty() ? buyerTeamWise.ToMailID : toMailID + ";" + buyerTeamWise.ToMailID;
                                }
                                if (buyerTeamWise.CCMailID.IsNotNullOrEmpty())
                                {
                                    ccMailID = ccMailID.IsNullOrEmpty() ? buyerTeamWise.CCMailID : ccMailID + ";" + buyerTeamWise.CCMailID;
                                }
                                if (buyerTeamWise.BCCMailID.IsNotNullOrEmpty())
                                {
                                    bccMailID = bccMailID.IsNullOrEmpty() ? buyerTeamWise.BCCMailID : bccMailID + ";" + buyerTeamWise.BCCMailID;
                                }
                            }
                        }
                        else
                        {
                            fromMailID = "erpnoreply@epylliongroup.com";
                            password = "Ugr7jT5d";
                            //toMailID = "anupam@epylliongroup.com;abdussalam@epylliongroup.com;litonekl@epylliongroup.com;imrez.ratin@epylliongroup.com";
                            toMailID = "imrez.ratin@epylliongroup.com";
                            ccMailID = "";
                            bccMailID = "";
                        }
                        string tddetail = "";

                        string sampleType = await _service.GetSampleTypeByBookingID(entity.BookingID);
                        if (sampleType.IsNullOrEmpty())
                        {
                            sampleType = "Buyer Development Sample";
                        }
                        List<FBookingAcknowledgeChild> GetDataForAcknowledgColour = await _service.GetDataForAcknowledgColourAsync(entity.BookingID);
                        DateTime date = new DateTime();
                        string deleveryDate = "";
                        string subject = entity.IsUnAcknowledge == true ? $"{EditType}Sample Booking No : {entity.BookingNo}{revisionText} for Garments Buyer {entity.BuyerName} {entity.BuyerTeamName}" : $"{EditType}Sample Booking No : {entity.BookingNo}{revisionText} for Garments Buyer {entity.BuyerName} {entity.BuyerTeamName}";
                        string messageBody = "";

                        if (entity.IsUnAcknowledge == true)
                        {
                            messageBody = $"<center><u><b>Booking Not Acknowledged (Revision Request)</b></u> </center></br></br>" +
                                $"Dear Sir,</br>" +
                                $"Fabric Booking No: <b>{entity.BookingNo}{revisionText}</b>, {sampleType} for the Garments Buyer {entity.BuyerName}" +
                                $"<b> cannot be acknowledged</b> for the below comments:</br>" +
                                $"</br>" +
                                $"<b>\"{entity.UnAcknowledgeReason}\"</b></br>" +
                                $"</br>" +
                                $"Therefore please revise the booking to proceed further and if you have any query contact with the concerns.</br>" +
                                $"</br>" +
                                 $"</br>" +
                                $"Thanks & Best Regards," +
                                $"</br>" +
                                $"{AppUser.EmployeeName}</br>" +
                                $"{AppUser.DepertmentDescription}</br></br>" +
                                $"This is system generated mail.";
                        }
                        else
                        {
                            messageBody = $"<center><u><b>Booking Acknowledged</b></u> </center></br></br>" +
                                    $"Dear Sir,</br>" +
                                    $"Fabric Booking No: <b>{entity.BookingNo}{revisionText}</b>, {sampleType} for the Garments Buyer {entity.BuyerName}" +
                                    $" has been acknowledged for further processing. Please note that the fabrics will be delivered from Textile Unit as per the following schedule:" +
                                    $"</br></br>" +
                                     $"<div style='float:center;padding:10px;font-family:Tahoma;'>" +
                                     $"<table border='1' style='color:black;font-size:10pt;font-family:Tahoma;'>" +
                                     $"<tr style='background-color:#dde9ed'>" +
                                     $"<th>Garments Color</th>" +
                                     $"<th>Delivery plan</th>" +
                                     $"</tr>";

                            for (int i = 0; i < GetDataForAcknowledgColour.Count(); i++)
                            {
                                date = (DateTime)GetDataForAcknowledgColour[i].DeliveryDate;
                                deleveryDate = date.ToString("dd/M/yyyy");
                                tddetail += $"<tr><td>{GetDataForAcknowledgColour[i].Color}</td>" +
                                               $"<td>{deleveryDate}</td></tr>";
                            }
                            messageBody += tddetail +
                                 $"</table>" +
                                 $"</div>" +
                                $"</br>" +
                                $"<div>" +
                                $"The Fabric Delivery plan has been generated by Textile ERP considering the shortest possible operational " +
                                $"timeframe based on fabric requirements on the booking. Please contact with concerns for any issue. </ br > " +
                                $"</div>" +
                                $"</br>" +
                                $"Thanks & Best Regards," +
                                $"</br>" +
                                $"{AppUser.EmployeeName}</br>" +
                                $"{AppUser.DepertmentDescription}</br></br>" +
                                $"This is system generated mail.";
                        }
                        String fileName = String.Empty;
                        //fileName = $"{entity.BookingNo}.pdf";
                        await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, subject, messageBody, fileName, attachment);
                    }
                    catch (Exception ex)
                    {
                    }

                    #endregion Send Mail 
                }
                */
            }

            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.FBookingAcknowledge, 0, grpConceptNo, entity.BookingID);
            return Ok(entity);
        }

        #region Bulk List Count
        [HttpGet]
        [Route("bulk/bulk-booking-knitting-info/get-list-count/{menuType}")]
        public async Task<IActionResult> GetListCountBBKI(int menuType)
        {
            CountListItem data = await _service.GetListCountBBKI((EnumBDSAcknowledgeParamType)menuType);
            return Ok(data);
        }
        #endregion

        #region Bulk

        [HttpGet]
        [Route("bulk/{bookingNo}/{isSample}/{yBookingNo}/{isYarnRevisionMenu}/{isFromYBAck}")]
        public async Task<IActionResult> GetDataByBookingNo(string bookingNo, bool isSample, string yBookingNo, bool isYarnRevisionMenu, bool isFromYBAck)
        {
            FBookingAcknowledge data = new FBookingAcknowledge();
            string[] parts = yBookingNo.Split(new string[] { "YB" }, StringSplitOptions.None);
            if (parts.Length >= 2 && parts[1].Contains("Add"))
            {
                data = await _service.GetDataByYBookingNo(bookingNo, isSample, true, yBookingNo, false, false, isYarnRevisionMenu, isFromYBAck);
            }
            else
            {
                data = await _service.GetDataByBookingNo(bookingNo, isSample, false, yBookingNo, false, false, isYarnRevisionMenu, isFromYBAck);
            }

            return Ok(data);
        }
        [HttpGet]
        [Route("bulk/revise/{bookingNo}/{isSample}")]
        public async Task<IActionResult> GetDataByBookingNoRevise(string bookingNo, bool isSample)
        {
            FBookingAcknowledge data = await _service.GetDataByBookingNoRevise(bookingNo, isSample);
            return Ok(data);
        }
        [HttpGet]
        [Route("bulk/new/{bookingId}")]
        public async Task<IActionResult> GetNewBulk(int bookingId)
        {
            FBookingAcknowledge data = await _service.GetNewBulkAsync(bookingId);
            return Ok(data);
        }
        private decimal GetQtyFromPer(decimal bookingQty, decimal distributionPer, decimal allowancePer)
        {
            decimal PLoss = (decimal)0.50;
            decimal YarnReqQty = (bookingQty * (distributionPer / 100)) / (1 + (allowancePer / 100) - (PLoss / 100));
            return YarnReqQty;
        }
        private decimal GetBookingQtyKG(decimal gm, string size, string bcLength, string bcWidth, decimal pcs)
        {
            decimal qtyInKg = 0;
            if (size.IsNotNullOrEmpty())
            {
                var splitSize = size.Split('X');
                decimal selectedLenght = Convert.ToDecimal(splitSize[0].Trim());
                decimal selectedWidth = Convert.ToDecimal(splitSize[1].Trim());
                if (selectedLenght > 0 && selectedWidth > 0)
                {
                    qtyInKg = pcs * ((gm * Convert.ToDecimal(bcLength) * Convert.ToDecimal(bcWidth)) / (selectedLenght * selectedWidth));
                    qtyInKg = qtyInKg / 1000;
                }
            }
            return Math.Round(qtyInKg, 2);
        }
        private FBookingAcknowledgeChild GetCalculatedFieldsFBC(FBookingAcknowledgeChild fbc)
        {
            fbc.ReqFinishFabricQty = Math.Round(fbc.BookingQtyKG - fbc.FinishFabricUtilizationQty, 2);
            //fbc.GreyReqQty = fbc.ReqFinishFabricQty * (1 + (fbc.YarnAllowance / 100) - Convert.ToDecimal(0.5 / 100));
            fbc.GreyReqQty = Math.Round(fbc.ReqFinishFabricQty * (1 + (fbc.YarnAllowance / 100) - Convert.ToDecimal(0.5 / 100)), 2);
            fbc.GreyReqQty = Math.Round(fbc.YarnAllowance == 0 ? fbc.BookingQtyKG : fbc.GreyReqQty, 2);
            fbc.GreyReqQty = Math.Round(fbc.GreyReqQty, 2);
            fbc.GreyProdQty = Math.Round((fbc.GreyReqQty - fbc.GreyLeftOverQty), 2);
            fbc.ExcessQtyInKG = Math.Round(fbc.GreyProdQty - fbc.BookingQtyKG, 2);
            return fbc;
        }

        private YarnBookingChildItem SetYarnUtilizationPOPUP(YarnBookingChildItem entityChildItemObj, YarnBookingChildItem modelChildItemObj, decimal totalNetYarnReqQty)
        {
            entityChildItemObj.Blending = modelChildItemObj.Blending;
            entityChildItemObj.Distribution = modelChildItemObj.Distribution;
            entityChildItemObj.BookingQty = modelChildItemObj.BookingQty;
            entityChildItemObj.RequiredQty = modelChildItemObj.RequiredQty;
            entityChildItemObj.Specification = modelChildItemObj.Specification;
            entityChildItemObj.YarnReqQty = modelChildItemObj.YarnReqQty;
            entityChildItemObj.YarnLeftOverQty = modelChildItemObj.YarnLeftOverQty;
            entityChildItemObj.Allowance = modelChildItemObj.Allowance;
            entityChildItemObj.GreyAllowance = modelChildItemObj.GreyAllowance;
            entityChildItemObj.YDAllowance = modelChildItemObj.YDAllowance;
            entityChildItemObj.NetYarnReqQty = modelChildItemObj.NetYarnReqQty;
            entityChildItemObj.YarnBalanceQty = modelChildItemObj.YarnBalanceQty;
            entityChildItemObj.YBChildItemID = modelChildItemObj.YBChildItemID;
            entityChildItemObj.YBChildID = modelChildItemObj.YBChildID;
            entityChildItemObj.Remarks = modelChildItemObj.Remarks;
            entityChildItemObj.Segment1ValueId = modelChildItemObj.Segment1ValueId;
            entityChildItemObj.Segment1ValueDesc = modelChildItemObj.Segment1ValueDesc;
            entityChildItemObj.Segment2ValueId = modelChildItemObj.Segment2ValueId;
            entityChildItemObj.Segment2ValueDesc = modelChildItemObj.Segment2ValueDesc;
            entityChildItemObj.Segment3ValueId = modelChildItemObj.Segment3ValueId;
            entityChildItemObj.Segment3ValueDesc = modelChildItemObj.Segment3ValueDesc;
            entityChildItemObj.Segment4ValueId = modelChildItemObj.Segment4ValueId;
            entityChildItemObj.Segment4ValueDesc = modelChildItemObj.Segment4ValueDesc;
            entityChildItemObj.Segment5ValueId = modelChildItemObj.Segment5ValueId;
            entityChildItemObj.Segment5ValueDesc = modelChildItemObj.Segment5ValueDesc;
            entityChildItemObj.Segment6ValueId = modelChildItemObj.Segment6ValueId;
            entityChildItemObj.Segment6ValueDesc = modelChildItemObj.Segment6ValueDesc;
            entityChildItemObj.Segment7ValueId = modelChildItemObj.Segment7ValueId;
            entityChildItemObj.Segment7ValueDesc = modelChildItemObj.Segment7ValueDesc;
            entityChildItemObj.YItemMasterID = modelChildItemObj.YItemMasterID;
            entityChildItemObj.YarnCategory = modelChildItemObj.YarnCategory;
            entityChildItemObj.ShadeCode = modelChildItemObj.ShadeCode;
            entityChildItemObj.YarnPly = modelChildItemObj.YarnPly;
            entityChildItemObj.Spinner = modelChildItemObj.Spinner;
            entityChildItemObj.SpinnerId = modelChildItemObj.SpinnerId;
            entityChildItemObj.YarnLotNo = modelChildItemObj.YarnLotNo;
            entityChildItemObj.YDItem = modelChildItemObj.YDItem;
            entityChildItemObj.YD = modelChildItemObj.YD;
            entityChildItemObj.Remarks = modelChildItemObj.Remarks;

            if (entityChildItemObj.GreyYarnUtilizationPopUpList.IsNull()) entityChildItemObj.GreyYarnUtilizationPopUpList = new List<BulkBookingGreyYarnUtilization>();
            if (entityChildItemObj.DyedYarnUtilizationPopUpList.IsNull()) entityChildItemObj.DyedYarnUtilizationPopUpList = new List<BulkBookingDyedYarnUtilization>();

            entityChildItemObj.GreyYarnUtilizationPopUpList.SetDeleted();
            entityChildItemObj.DyedYarnUtilizationPopUpList.SetDeleted();

            var nTotalQty = totalNetYarnReqQty;
            var nChildItemQTY = entityChildItemObj.NetYarnReqQty;
            var nPercent = (nChildItemQTY * 100) / nTotalQty;

            List<BulkBookingGreyYarnUtilization> tempGreyList = new List<BulkBookingGreyYarnUtilization>();
            List<BulkBookingDyedYarnUtilization> tempDyedList = new List<BulkBookingDyedYarnUtilization>();

            modelChildItemObj.GreyYarnUtilizationPopUpList.ForEach(gy =>
            {
                var nDistUtilizationSampleStock = (gy.UtilizationSampleStock / 100) * nPercent;
                var nDistUtilizationLiabilitiesStock = (gy.UtilizationLiabilitiesStock / 100) * nPercent;
                var nDistUtilizationUnusableStock = (gy.UtilizationUnusableStock / 100) * nPercent;
                var nDistUtilizationLeftoverStock = (gy.UtilizationLeftoverStock / 100) * nPercent;

                var obj = entityChildItemObj.GreyYarnUtilizationPopUpList.Find(x => x.YBChildItemID == modelChildItemObj.YBChildItemID && x.YarnStockSetID == gy.YarnStockSetID);
                if (obj == null)
                {
                    obj = CommonFunction.DeepClone(gy);
                    obj.EntityState = EntityState.Added;
                    obj.AddedBy = AppUser.UserCode;
                    obj.DateAdded = DateTime.Now;
                }
                else
                {
                    obj.EntityState = EntityState.Modified;
                    obj.DateUpdated = DateTime.Now;
                    obj.UpdatedBy = AppUser.UserCode;
                }
                obj.UtilizationSampleStock = Math.Round(nDistUtilizationSampleStock, 2);
                obj.UtilizationLiabilitiesStock = Math.Round(nDistUtilizationLiabilitiesStock, 2);
                obj.UtilizationUnusableStock = Math.Round(nDistUtilizationUnusableStock, 2);
                obj.UtilizationLeftoverStock = Math.Round(nDistUtilizationLeftoverStock, 2);

                obj.TotalUtilization = obj.UtilizationSampleStock + obj.UtilizationLiabilitiesStock + obj.UtilizationUnusableStock + obj.UtilizationLeftoverStock;

                tempGreyList.Add(CommonFunction.DeepClone(obj));

            });
            entityChildItemObj.GreyYarnUtilizationPopUpList = tempGreyList;

            modelChildItemObj.DyedYarnUtilizationPopUpList.ForEach(gy =>
            {
                var nDyedYarnUtilizationQty = (gy.DyedYarnUtilizationQty / 100) * nPercent;

                var obj = entityChildItemObj.DyedYarnUtilizationPopUpList.Find(x => x.YBChildItemID == modelChildItemObj.YBChildItemID);
                if (obj == null)
                {
                    obj = CommonFunction.DeepClone(gy);
                    obj.EntityState = EntityState.Added;
                    obj.AddedBy = AppUser.UserCode;
                    obj.DateAdded = DateTime.Now;
                }
                else
                {
                    obj.EntityState = EntityState.Modified;
                    obj.DateUpdated = DateTime.Now;
                    obj.UpdatedBy = AppUser.UserCode;
                }
                obj.DyedYarnUtilizationQty = Math.Round(nDyedYarnUtilizationQty, 2);

                tempDyedList.Add(CommonFunction.DeepClone(obj));

            });
            entityChildItemObj.DyedYarnUtilizationPopUpList = tempDyedList;

            return entityChildItemObj;
        }

        private FBookingAcknowledgeChild SetFinishAndGreyFabricUtilization(FBookingAcknowledgeChild bChild, List<FBookingAcknowledgeChild> modelChilds, int YBChildID)
        {
            bChild.FinishFabricUtilizationPopUpList.SetDeleted();
            bChild.GreyFabricUtilizationPopUpList.SetDeleted();

            var nTotalBookingQTY = modelChilds.Where(x => x.Construction == bChild.Construction && x.Composition == bChild.Composition && x.Color == bChild.Color).Sum(s => s.BookingQty);
            var nYBCBookingQTY = bChild.BookingQty;
            var nBookingPercent = nTotalBookingQTY > 0 ? (nYBCBookingQTY * 100) / nTotalBookingQTY : nTotalBookingQTY;

            List<BulkBookingFinishFabricUtilization> finalFF = new List<BulkBookingFinishFabricUtilization>();
            List<FBookingAcknowledgeChildGFUtilization> finalGreyF = new List<FBookingAcknowledgeChildGFUtilization>();
            modelChilds.Where(x => x.Construction == bChild.Construction && x.Composition == bChild.Composition && x.Color == bChild.Color).ToList().ForEach(mc =>
            {
                mc.FinishFabricUtilizationPopUpList.ForEach(ff =>
                {
                    ff = CommonFunction.DeepClone(ff);
                    ff.FinishFabricUtilizationQTYinkg = (ff.FinishFabricUtilizationQTYinkg / 100) * nBookingPercent;
                    ff.BookingChildID = bChild.BookingChildID;
                    ff.YBChildID = YBChildID;
                    ff.SubGroupID = mc.SubGroupID;

                    var finishFabric = bChild.FinishFabricUtilizationPopUpList.Find(x => x.YBChildID == ff.YBChildID && x.ExportOrderID == ff.ExportOrderID &&
                    x.ItemMasterID == ff.ItemMasterID && x.WeightSheetNo == ff.WeightSheetNo && x.BatchNo == ff.BatchNo && x.ColorID == ff.ColorID);

                    if (finishFabric.IsNull())
                    {
                        finishFabric = CommonFunction.DeepClone(ff);
                        finishFabric.EntityState = EntityState.Added;
                        finishFabric.AddedBy = AppUser.UserCode;
                        finishFabric.DateAdded = DateTime.Now;

                    }
                    else
                    {
                        finishFabric.EntityState = EntityState.Modified;
                        finishFabric.FinishFabricUtilizationQTYinkg = ff.FinishFabricUtilizationQTYinkg;
                        finishFabric.FinishFabricExcessQtyKg = ff.FinishFabricExcessQtyKg;
                        finishFabric.FinishFabricRejectQtyKg = ff.FinishFabricRejectQtyKg;
                        finishFabric.FinishFabricBookingQtyDecreasedbyMerchantQtyKg = ff.FinishFabricBookingQtyDecreasedbyMerchantQtyKg;
                        finishFabric.FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg = ff.FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg;
                        finishFabric.UpdatedBy = AppUser.UserCode;
                        finishFabric.DateUpdated = DateTime.Now;
                    }
                    finalFF.Add(CommonFunction.DeepClone(finishFabric));
                });
                mc.GreyFabricUtilizationPopUpList.ForEach(ff =>
                {
                    ff = CommonFunction.DeepClone(ff);
                    ff.GreyFabricUtilizationQTYinkg = (ff.GreyFabricUtilizationQTYinkg / 100) * nBookingPercent;
                    ff.BookingChildID = bChild.BookingChildID;
                    ff.YBChildID = YBChildID;
                    ff.SubGroupID = mc.SubGroupID;

                    var greyFabric = bChild.GreyFabricUtilizationPopUpList.Find(x => x.YBChildID == ff.YBChildID && x.ExportOrderID == ff.ExportOrderID &&
                    x.ItemMasterID == ff.ItemMasterID && x.ColorID == ff.ColorID);
                    if (greyFabric.IsNull())
                    {
                        greyFabric = CommonFunction.DeepClone(ff);
                        greyFabric.EntityState = EntityState.Added;
                    }
                    else
                    {
                        greyFabric.EntityState = EntityState.Modified;
                        greyFabric.GreyFabricUtilizationQTYinkg = ff.GreyFabricUtilizationQTYinkg;
                    }
                    finalGreyF.Add(CommonFunction.DeepClone(greyFabric));
                });
            });
            bChild.FinishFabricUtilizationPopUpList = CommonFunction.DeepClone(finalFF);
            bChild.GreyFabricUtilizationPopUpList = CommonFunction.DeepClone(finalGreyF);
            return bChild;
        }
        private decimal GetNetYarnReqQty(decimal yarnDistribution, decimal finishFabricUtilizationQty, decimal greyUtilizationQty, decimal dyedYarnUtilizationQty, decimal totalAllowance, decimal yDAllowance, decimal greyYarnUtilizationQty, decimal reqFinishFabricQty)
        {

            decimal yarnFFU = yarnDistribution * (finishFabricUtilizationQty / 100);
            yarnFFU = yarnFFU + (yarnFFU * totalAllowance) / 100;

            decimal yarnGU = yarnDistribution * (greyUtilizationQty / 100);
            yarnGU = yarnGU + (yarnGU * (yDAllowance + Convert.ToDecimal(0.5))) / 100;

            //decimal yarnDYU = yarnDistribution * (dyedYarnUtilizationQty / 100);
            //yarnDYU = yarnDYU + (yarnDYU * (yDAllowance)) / 100;

            //decimal yarnGYU = greyYarnUtilizationQty;
            reqFinishFabricQty = (reqFinishFabricQty / 100) * yarnDistribution;

            decimal netReqQty = reqFinishFabricQty + ((reqFinishFabricQty * totalAllowance) / 100) - yarnGU;
            //decimal netReqQty = reqFinishFabricQty + ((reqFinishFabricQty * totalAllowance) / 100) - yarnFFU - yarnGU;
            netReqQty = Math.Round(netReqQty, 2);
            return netReqQty;
        }
        private decimal GetYarnBalanceQty(decimal netYarnReqQty, decimal yarnDistribution, decimal dyedYarnUtilizationQty, decimal yDAllowance, decimal greyYarnUtilizationQty)
        {

            decimal yarnDYU = yarnDistribution * (dyedYarnUtilizationQty / 100);
            yarnDYU = yarnDYU + (yarnDYU * (yDAllowance)) / 100;

            decimal yarnGYU = greyYarnUtilizationQty;

            decimal balanceQty = netYarnReqQty - yarnDYU - yarnGYU;

            return Math.Round(balanceQty, 2);
        }
        private List<FBookingAcknowledgeChild> GetExtendChilds(List<FBookingAcknowledgeChild> modelChilds, List<FBookingAcknowledgeChild> entityChilds, List<YarnBookingChildItem> yarnBookingChildItems = null, FBookingAcknowledge model = null, bool isAddition = false)
        {
            if (yarnBookingChildItems == null) yarnBookingChildItems = new List<YarnBookingChildItem>();

            entityChilds.ForEach(ec =>
            {
                ec.ChildItems = new List<YarnBookingChildItem>();

                var modelChild = modelChilds.Find(x => x.Construction == ec.Construction && x.Composition == ec.Composition && x.Color == ec.Color);
                if (modelChild.IsNotNull())
                {
                    ec.YarnAllowance = modelChild.YarnAllowance;
                }

                if (ec.SubGroupID == 11)
                {
                    model.CollarWeightInGm = model.CollarWeightInGm.IsNull() ? 0 : model.CollarWeightInGm;
                    model.CollarSizeID = model.CollarSizeID.IsNullOrEmpty() ? "" : model.CollarSizeID;
                    ec.BookingQtyKG = this.GetBookingQtyKG(model.CollarWeightInGm, model.CollarSizeID, ec.Length, ec.Width, ec.BookingQty);
                    ec = this.GetCalculatedFieldsFBC(ec);
                }
                else if (ec.SubGroupID == 12)
                {
                    model.CuffWeightInGm = model.CuffWeightInGm.IsNull() ? 0 : model.CuffWeightInGm;
                    model.CuffSizeID = model.CuffSizeID.IsNullOrEmpty() ? "" : model.CuffSizeID;
                    ec.BookingQtyKG = this.GetBookingQtyKG(model.CuffWeightInGm, model.CuffSizeID, ec.Length, ec.Width, ec.BookingQty);

                    ec = this.GetCalculatedFieldsFBC(ec);
                }

                if (yarnBookingChildItems.Where(x => x.SubGroupId == ec.SubGroupID).Count() > 0)
                {
                    var obj = yarnBookingChildItems.FirstOrDefault(x => x.BookingChildID == ec.BookingChildID);
                    if (obj.IsNotNull())
                    {
                        int ybChildID = yarnBookingChildItems.FirstOrDefault(x => x.BookingChildID == ec.BookingChildID).YBChildID;
                        ec = this.SetFinishAndGreyFabricUtilization(ec, modelChilds, ybChildID);
                    }
                }

                //var modelChild = modelChilds.Find(x => x.Construction == ec.Construction && x.Composition == ec.Composition && x.Color == ec.Color);

                if (modelChild.IsNotNull())
                {
                    ec.MachineTypeId = modelChild.MachineTypeId;
                    ec.TechnicalNameId = modelChild.TechnicalNameId;
                    ec.BrandID = modelChild.BrandID;
                    ec.Brand = modelChild.Brand;
                    ec.PostFinishingProcessChilds = modelChild.PostFinishingProcessChilds;
                    ec.PreFinishingProcessChilds = modelChild.PreFinishingProcessChilds;
                    ec.YarnAllowance = modelChild.YarnAllowance;
                    ec.Remarks = modelChild.Remarks;
                    modelChild.ChildItems.ForEach(y =>
                    {
                        decimal totalNetYarnReqQty = CommonFunction.DeepClone(y.NetYarnReqQty);

                        y = CommonFunction.DeepClone(y);
                        y.RequiredQty = this.GetQtyFromPer(ec.GreyProdQty, y.Distribution, y.Allowance);
                        decimal netYRQ = GetNetYarnReqQty(y.Distribution, ec.FinishFabricUtilizationQty, ec.GreyLeftOverQty, y.DyedYarnUtilizationQty, y.Allowance, y.YDAllowance, y.GreyYarnUtilizationQty, ec.ReqFinishFabricQty);
                        netYRQ = netYRQ > 0 ? netYRQ : 0;
                        //decimal NetYarnReqQty = netYRQ;//Convert.ToDecimal(((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100))) * (y.Distribution / 100)));
                        y.NetYarnReqQty = netYRQ;// Convert.ToDecimal((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100)) * (y.Distribution / 100) * (1 + (y.YDAllowance / 100))));
                        y.YarnBalanceQty = GetYarnBalanceQty(netYRQ, y.Distribution, y.DyedYarnUtilizationQty, y.YDAllowance, y.GreyYarnUtilizationQty);// Convert.ToDecimal(((NetYarnReqQty - y.DyedYarnUtilizationQty) * (1 + (y.YDAllowance / 100)) - y.GreyYarnUtilizationQty));

                        if (yarnBookingChildItems.Where(x => x.SubGroupId == ec.SubGroupID).Count() > 0)
                        {
                            var yarnChildItem = CommonFunction.DeepClone(yarnBookingChildItems.Where(x => x.SubGroupId == ec.SubGroupID && x.BookingChildID == ec.BookingChildID && x.YItemMasterID == y.ItemMasterID));
                            if (yarnChildItem.IsNotNull() && yarnChildItem.Count() > 0)
                            {
                                y.YBChildItemID = yarnChildItem.FirstOrDefault().YBChildItemID;
                                y.YBChildID = yarnChildItem.FirstOrDefault().YBChildID;
                            }
                        }

                        var objEntityYarnItem = CommonFunction.DeepClone(yarnBookingChildItems.Find(x => x.BookingChildID == ec.BookingChildID && x.YItemMasterID == y.ItemMasterID));
                        if (objEntityYarnItem.IsNotNull())
                        {
                            y = CommonFunction.DeepClone(this.SetYarnUtilizationPOPUP(objEntityYarnItem, y, totalNetYarnReqQty));
                        }
                        ec.ChildItems.Add(y);
                    });
                }
            });
            modelChilds = CommonFunction.DeepClone(entityChilds);
            return modelChilds;
        }
        /*private List<FBookingAcknowledgeChild> GetExtendChilds(List<FBookingAcknowledgeChild> modelChilds, List<FBookingAcknowledgeChild> entityChilds, List<YarnBookingChildItem> yarnBookingChildItems = null, FBookingAcknowledge model = null, bool isAddition = false)
        {
            if (yarnBookingChildItems == null) yarnBookingChildItems = new List<YarnBookingChildItem>();

            entityChilds.ForEach(ec =>
            {
                ec.ChildItems = new List<YarnBookingChildItem>();

                var modelChild = modelChilds.Find(x => x.Construction == ec.Construction && x.Composition == ec.Composition && x.Color == ec.Color);
                if (modelChild.IsNotNull())
                {
                    ec.YarnAllowance = modelChild.YarnAllowance;
                }

                if (ec.SubGroupID == 11)
                {
                    model.CollarWeightInGm = model.CollarWeightInGm.IsNull() ? 0 : model.CollarWeightInGm;
                    model.CollarSizeID = model.CollarSizeID.IsNullOrEmpty() ? "" : model.CollarSizeID;
                    ec.BookingQtyKG = this.GetBookingQtyKG(model.CollarWeightInGm, model.CollarSizeID, ec.Length, ec.Width, ec.BookingQty);
                    ec = this.GetCalculatedFieldsFBC(ec);
                }
                else if (ec.SubGroupID == 12)
                {
                    model.CuffWeightInGm = model.CuffWeightInGm.IsNull() ? 0 : model.CuffWeightInGm;
                    model.CuffSizeID = model.CuffSizeID.IsNullOrEmpty() ? "" : model.CuffSizeID;
                    ec.BookingQtyKG = this.GetBookingQtyKG(model.CuffWeightInGm, model.CuffSizeID, ec.Length, ec.Width, ec.BookingQty);

                    ec = this.GetCalculatedFieldsFBC(ec);
                }

                if (yarnBookingChildItems.Where(x => x.SubGroupId == ec.SubGroupID).Count() > 0)
                {
                    var obj = yarnBookingChildItems.FirstOrDefault(x => x.BookingChildID == ec.BookingChildID);
                    if (obj.IsNotNull())
                    {
                        int ybChildID = yarnBookingChildItems.FirstOrDefault(x => x.BookingChildID == ec.BookingChildID).YBChildID;
                        ec = this.SetFinishAndGreyFabricUtilization(ec, modelChilds, ybChildID);
                    }
                }

                //var modelChild = modelChilds.Find(x => x.Construction == ec.Construction && x.Composition == ec.Composition && x.Color == ec.Color);

                if (modelChild.IsNotNull())
                {
                    ec.MachineTypeId = modelChild.MachineTypeId;
                    ec.TechnicalNameId = modelChild.TechnicalNameId;
                    ec.BrandID = modelChild.BrandID;
                    ec.Brand = modelChild.Brand;
                    ec.PostFinishingProcessChilds = modelChild.PostFinishingProcessChilds;
                    ec.PreFinishingProcessChilds = modelChild.PreFinishingProcessChilds;
                    ec.YarnAllowance = modelChild.YarnAllowance;
                    ec.Remarks = modelChild.Remarks;
                    modelChild.ChildItems.ForEach(y =>
                    {
                        decimal totalNetYarnReqQty = CommonFunction.DeepClone(y.NetYarnReqQty);

                        y = CommonFunction.DeepClone(y);
                        y.RequiredQty = this.GetQtyFromPer(ec.GreyProdQty, y.Distribution, y.Allowance);
                        if (y.YD)
                        {
                            //decimal NetYarnReqQty = Convert.ToDecimal(((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100))) * (y.Distribution / 100)).ToString("00"));
                            //y.NetYarnReqQty = Convert.ToDecimal((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100)) * (y.Distribution / 100) * (1 + (y.YDAllowance / 100))).ToString("00"));
                            //y.YarnBalanceQty = Convert.ToDecimal(((NetYarnReqQty - y.DyedYarnUtilizationQty) * (1 + (y.YDAllowance / 100)) - y.GreyYarnUtilizationQty).ToString("00"));
                            decimal NetYarnReqQty = Convert.ToDecimal(((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100))) * (y.Distribution / 100)));
                            y.NetYarnReqQty = Convert.ToDecimal((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100)) * (y.Distribution / 100) * (1 + (y.YDAllowance / 100))));
                            y.YarnBalanceQty = Convert.ToDecimal(((NetYarnReqQty - y.DyedYarnUtilizationQty) * (1 + (y.YDAllowance / 100)) - y.GreyYarnUtilizationQty));
                        }
                        else
                        {
                            //y.NetYarnReqQty = Convert.ToDecimal(((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100))) * (y.Distribution / 100)).ToString("00"));
                            //y.YarnBalanceQty = Convert.ToDecimal((y.NetYarnReqQty - (y.GreyYarnUtilizationQty + y.DyedYarnUtilizationQty)).ToString("00"));
                            y.NetYarnReqQty = Convert.ToDecimal(((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100))) * (y.Distribution / 100)));
                            y.YarnBalanceQty = Convert.ToDecimal((y.NetYarnReqQty - (y.GreyYarnUtilizationQty + y.DyedYarnUtilizationQty)));
                        }
                        if (yarnBookingChildItems.Where(x => x.SubGroupId == ec.SubGroupID).Count() > 0)
                        {
                            var yarnChildItem = CommonFunction.DeepClone(yarnBookingChildItems.Where(x => x.SubGroupId == ec.SubGroupID && x.BookingChildID == ec.BookingChildID && x.YItemMasterID == y.ItemMasterID));
                            if (yarnChildItem.IsNotNull() && yarnChildItem.Count() > 0)
                            {
                                y.YBChildItemID = yarnChildItem.FirstOrDefault().YBChildItemID;
                                y.YBChildID = yarnChildItem.FirstOrDefault().YBChildID;
                            }
                        }

                        var objEntityYarnItem = CommonFunction.DeepClone(yarnBookingChildItems.Find(x => x.BookingChildID == ec.BookingChildID && x.YItemMasterID == y.ItemMasterID));
                        if (objEntityYarnItem.IsNotNull())
                        {
                            y = CommonFunction.DeepClone(this.SetYarnUtilizationPOPUP(objEntityYarnItem, y, totalNetYarnReqQty));
                        }
                        ec.ChildItems.Add(y);
                    });
                }
            });
            modelChilds = CommonFunction.DeepClone(entityChilds);
            return modelChilds;
        }*/
        private List<FBookingAcknowledgeChild> GetExtendChildsAddition(List<FBookingAcknowledgeChild> modelChilds, List<FBookingAcknowledgeChild> entityChilds, List<YarnBookingChildItem> yarnBookingChildItems = null, FBookingAcknowledge model = null, bool isAddition = false)
        {
            if (yarnBookingChildItems == null) yarnBookingChildItems = new List<YarnBookingChildItem>();
            List<FBookingAcknowledgeChild> entityChildsAddition = new List<FBookingAcknowledgeChild>();
            List<int> modelBookingChildIDs = new List<int>();
            entityChilds.ForEach(ec =>
            {
                ec.ChildItems = new List<YarnBookingChildItem>();

                var modelChild = modelChilds.Find(x => x.Construction == ec.Construction && x.Composition == ec.Composition && x.Color == ec.Color);

                //var modelChild = modelChilds.Find(x => x.Construction == ec.Construction && x.Composition == ec.Composition && x.Color == ec.Color);

                if (modelChild.IsNotNull())
                {
                    ec.YarnAllowance = modelChild.YarnAllowance;

                    if (modelBookingChildIDs.Contains(modelChild.BookingChildID) == false)
                    {
                        modelBookingChildIDs.Add(modelChild.BookingChildID);
                        if (ec.SubGroupID == 11)
                        {
                            model.CollarWeightInGm = model.CollarWeightInGm.IsNull() ? 0 : model.CollarWeightInGm;
                            model.CollarSizeID = model.CollarSizeID.IsNullOrEmpty() ? "" : model.CollarSizeID;
                            if (isAddition)
                            {
                                ec.BookingQty = modelChild.BookingQty;
                                ec.BookingQtyKG = this.GetBookingQtyKG(model.CollarWeightInGm, model.CollarSizeID, ec.Length, ec.Width, modelChild.BookingQty);
                            }
                            else
                            {
                                ec.BookingQtyKG = this.GetBookingQtyKG(model.CollarWeightInGm, model.CollarSizeID, ec.Length, ec.Width, ec.BookingQty);
                            }
                            ec = this.GetCalculatedFieldsFBC(ec);
                        }
                        else if (ec.SubGroupID == 12)
                        {
                            model.CuffWeightInGm = model.CuffWeightInGm.IsNull() ? 0 : model.CuffWeightInGm;
                            model.CuffSizeID = model.CuffSizeID.IsNullOrEmpty() ? "" : model.CuffSizeID;

                            if (isAddition)
                            {
                                ec.BookingQty = modelChild.BookingQty;
                                ec.BookingQtyKG = this.GetBookingQtyKG(model.CuffWeightInGm, model.CuffSizeID, ec.Length, ec.Width, modelChild.BookingQty);
                            }
                            else
                            {
                                ec.BookingQtyKG = this.GetBookingQtyKG(model.CuffWeightInGm, model.CuffSizeID, ec.Length, ec.Width, ec.BookingQty);
                            }
                            ec = this.GetCalculatedFieldsFBC(ec);
                        }

                        ec.MachineTypeId = modelChild.MachineTypeId;
                        ec.TechnicalNameId = modelChild.TechnicalNameId;
                        ec.BrandID = modelChild.BrandID;
                        ec.Brand = modelChild.Brand;
                        ec.PostFinishingProcessChilds = modelChild.PostFinishingProcessChilds;
                        ec.PreFinishingProcessChilds = modelChild.PreFinishingProcessChilds;
                        ec.YarnAllowance = modelChild.YarnAllowance;
                        ec.IsForFabric = modelChild.IsForFabric;
                        ec.ReqFinishFabricQty = modelChild.ReqFinishFabricQty;
                        ec.GreyFabricUtilizationPopUpList = modelChild.GreyFabricUtilizationPopUpList;
                        ec.FinishFabricUtilizationPopUpList = modelChild.FinishFabricUtilizationPopUpList;
                        ec.FinishFabricUtilizationQty = modelChild.FinishFabricUtilizationQty;
                        ec.GreyLeftOverQty = modelChild.GreyLeftOverQty;
                        modelChild.ChildItems.ForEach(y =>
                        {
                            y = CommonFunction.DeepClone(y);
                            y.RequiredQty = this.GetQtyFromPer(ec.GreyProdQty, y.Distribution, y.Allowance);
                            if (isAddition)
                            {
                                y.NetYarnReqQty = Math.Round(y.NetYarnReqQty, 2);
                                y.YarnBalanceQty = Math.Round(y.YarnBalanceQty, 2);
                            }
                            else
                            {
                                decimal netYRQ = GetNetYarnReqQty(y.Distribution, ec.FinishFabricUtilizationQty, ec.GreyLeftOverQty, y.DyedYarnUtilizationQty, y.Allowance, y.YDAllowance, y.GreyYarnUtilizationQty, ec.ReqFinishFabricQty);
                                netYRQ = netYRQ > 0 ? netYRQ : 0;
                                //decimal NetYarnReqQty = netYRQ;//Convert.ToDecimal(((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100))) * (y.Distribution / 100)).ToString("00"));

                                y.NetYarnReqQty = netYRQ;// Convert.ToDecimal((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100)) * (y.Distribution / 100) * (1 + (y.YDAllowance / 100))).ToString("00"));
                                y.YarnBalanceQty = GetYarnBalanceQty(netYRQ, y.Distribution, y.DyedYarnUtilizationQty, y.YDAllowance, y.GreyYarnUtilizationQty); //Convert.ToDecimal(((NetYarnReqQty - y.DyedYarnUtilizationQty) * (1 + (y.YDAllowance / 100)) - y.GreyYarnUtilizationQty).ToString("00"));


                            }
                            if (yarnBookingChildItems.Count() > 0)
                            {
                                var yarnChildItem = CommonFunction.DeepClone(yarnBookingChildItems.Where(x => x.BookingChildID == ec.BookingChildID && x.YItemMasterID == y.ItemMasterID));
                                if (yarnChildItem.IsNotNull())
                                {
                                    y.YBChildItemID = yarnChildItem.FirstOrDefault().YBChildItemID;
                                    y.YBChildID = yarnChildItem.FirstOrDefault().YBChildID;
                                }
                            }
                            y.BookingQty = y.YarnReqQty;
                            ec.ChildItems.Add(y);
                        });

                        ec.AdditionalReplacementPOPUPList = modelChild.AdditionalReplacementPOPUPList;


                        entityChildsAddition.Add(ec);
                    }
                }
            });
            modelChilds = CommonFunction.DeepClone(entityChildsAddition);
            return modelChilds;
        }
        /*private List<FBookingAcknowledgeChild> GetExtendChildsAddition(List<FBookingAcknowledgeChild> modelChilds, List<FBookingAcknowledgeChild> entityChilds, List<YarnBookingChildItem> yarnBookingChildItems = null, FBookingAcknowledge model = null, bool isAddition = false)
        {
            if (yarnBookingChildItems == null) yarnBookingChildItems = new List<YarnBookingChildItem>();
            List<FBookingAcknowledgeChild> entityChildsAddition = new List<FBookingAcknowledgeChild>();
            List<int> modelBookingChildIDs = new List<int>();
            entityChilds.ForEach(ec =>
            {
                ec.ChildItems = new List<YarnBookingChildItem>();

                var modelChild = modelChilds.Find(x => x.Construction == ec.Construction && x.Composition == ec.Composition && x.Color == ec.Color);

                //var modelChild = modelChilds.Find(x => x.Construction == ec.Construction && x.Composition == ec.Composition && x.Color == ec.Color);

                if (modelChild.IsNotNull())
                {
                    if (modelBookingChildIDs.Contains(modelChild.BookingChildID) == false)
                    {
                        modelBookingChildIDs.Add(modelChild.BookingChildID);
                        if (ec.SubGroupID == 11)
                        {
                            model.CollarWeightInGm = model.CollarWeightInGm.IsNull() ? 0 : model.CollarWeightInGm;
                            model.CollarSizeID = model.CollarSizeID.IsNullOrEmpty() ? "" : model.CollarSizeID;
                            if (isAddition)
                            {
                                ec.BookingQty = modelChild.BookingQty;
                                ec.BookingQtyKG = this.GetBookingQtyKG(model.CollarWeightInGm, model.CollarSizeID, ec.Length, ec.Width, modelChild.BookingQty);
                            }
                            else
                            {
                                ec.BookingQtyKG = this.GetBookingQtyKG(model.CollarWeightInGm, model.CollarSizeID, ec.Length, ec.Width, ec.BookingQty);
                            }
                            ec = this.GetCalculatedFieldsFBC(ec);
                        }
                        else if (ec.SubGroupID == 12)
                        {
                            model.CuffWeightInGm = model.CuffWeightInGm.IsNull() ? 0 : model.CuffWeightInGm;
                            model.CuffSizeID = model.CuffSizeID.IsNullOrEmpty() ? "" : model.CuffSizeID;

                            if (isAddition)
                            {
                                ec.BookingQty = modelChild.BookingQty;
                                ec.BookingQtyKG = this.GetBookingQtyKG(model.CuffWeightInGm, model.CuffSizeID, ec.Length, ec.Width, modelChild.BookingQty);
                            }
                            else
                            {
                                ec.BookingQtyKG = this.GetBookingQtyKG(model.CuffWeightInGm, model.CuffSizeID, ec.Length, ec.Width, ec.BookingQty);
                            }
                            ec = this.GetCalculatedFieldsFBC(ec);
                        }

                        ec.MachineTypeId = modelChild.MachineTypeId;
                        ec.TechnicalNameId = modelChild.TechnicalNameId;
                        ec.BrandID = modelChild.BrandID;
                        ec.Brand = modelChild.Brand;
                        ec.PostFinishingProcessChilds = modelChild.PostFinishingProcessChilds;
                        ec.PreFinishingProcessChilds = modelChild.PreFinishingProcessChilds;
                        ec.YarnAllowance = modelChild.YarnAllowance;
                        ec.IsForFabric = modelChild.IsForFabric;
                        ec.ReqFinishFabricQty = modelChild.ReqFinishFabricQty;
                        ec.GreyFabricUtilizationPopUpList = modelChild.GreyFabricUtilizationPopUpList;
                        ec.FinishFabricUtilizationPopUpList = modelChild.FinishFabricUtilizationPopUpList;
                        ec.FinishFabricUtilizationQty = modelChild.FinishFabricUtilizationQty;
                        ec.GreyLeftOverQty = modelChild.GreyLeftOverQty;
                        modelChild.ChildItems.ForEach(y =>
                        {
                            y = CommonFunction.DeepClone(y);
                            y.RequiredQty = this.GetQtyFromPer(ec.GreyProdQty, y.Distribution, y.Allowance);
                            if (isAddition)
                            {
                                y.NetYarnReqQty = Math.Round(y.NetYarnReqQty, 2);
                                y.YarnBalanceQty = Math.Round(y.YarnBalanceQty, 2);
                            }
                            else
                            {
                                if (y.YD)
                                {
                                    decimal NetYarnReqQty = Convert.ToDecimal(((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100))) * (y.Distribution / 100)).ToString("00"));

                                    y.NetYarnReqQty = Convert.ToDecimal((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100)) * (y.Distribution / 100) * (1 + (y.YDAllowance / 100))).ToString("00"));
                                    y.YarnBalanceQty = Convert.ToDecimal(((NetYarnReqQty - y.DyedYarnUtilizationQty) * (1 + (y.YDAllowance / 100)) - y.GreyYarnUtilizationQty).ToString("00"));
                                }
                                else
                                {
                                    y.NetYarnReqQty = Convert.ToDecimal(((ec.GreyProdQty * (1 + Convert.ToDecimal(0.5 / 100))) * (y.Distribution / 100)).ToString("00"));
                                    y.YarnBalanceQty = Convert.ToDecimal((y.NetYarnReqQty - (y.GreyYarnUtilizationQty + y.DyedYarnUtilizationQty)).ToString("00"));
                                }
                            }
                            if (yarnBookingChildItems.Count() > 0)
                            {
                                var yarnChildItem = CommonFunction.DeepClone(yarnBookingChildItems.Where(x => x.BookingChildID == ec.BookingChildID && x.YItemMasterID == y.ItemMasterID));
                                if (yarnChildItem.IsNotNull())
                                {
                                    y.YBChildItemID = yarnChildItem.FirstOrDefault().YBChildItemID;
                                    y.YBChildID = yarnChildItem.FirstOrDefault().YBChildID;
                                }
                            }
                            y.BookingQty = y.YarnReqQty;
                            ec.ChildItems.Add(y);
                        });

                        ec.AdditionalReplacementPOPUPList = modelChild.AdditionalReplacementPOPUPList;


                        entityChildsAddition.Add(ec);
                    }
                }
            });
            modelChilds = CommonFunction.DeepClone(entityChildsAddition);
            return modelChilds;
        }*/
        private List<FBookingAcknowledgeChildReplacement> SetChildReplacementPOPUP(FBookingAcknowledgeChild entityChild, FBookingAcknowledgeChild modelChilds)
        {
            entityChild.AdditionalReplacementPOPUPList.SetDeleted();
            List<FBookingAcknowledgeChildReplacement> finalPOPUPList = new List<FBookingAcknowledgeChildReplacement>();
            modelChilds.AdditionalReplacementPOPUPList.ForEach(ff =>
            {
                ff = CommonFunction.DeepClone(ff);
                var replacementPOPUP = entityChild.AdditionalReplacementPOPUPList.Find(x => x.BookingChildID == ff.BookingChildID && x.ReplacementID == ff.ReplacementID);

                if (replacementPOPUP.IsNull())
                {
                    replacementPOPUP = CommonFunction.DeepClone(ff);
                    replacementPOPUP.AddedBy = AppUser.UserCode;
                    replacementPOPUP.DateAdded = DateTime.Now;
                    replacementPOPUP.EntityState = EntityState.Added;
                }
                else
                {
                    replacementPOPUP.EntityState = EntityState.Modified;
                    replacementPOPUP.UpdatedBy = AppUser.UserCode;
                    replacementPOPUP.DateUpdated = DateTime.Now;
                }

                replacementPOPUP.DepertmentID = ff.DepertmentID;
                replacementPOPUP.ReasonID = ff.ReasonID;
                replacementPOPUP.Remarks = ff.Remarks;
                finalPOPUPList.Add(CommonFunction.DeepClone(replacementPOPUP));
            });
            entityChild.AdditionalReplacementPOPUPList = CommonFunction.DeepClone(finalPOPUPList);

            return entityChild.AdditionalReplacementPOPUPList;

        }

        [Route("bulk/save")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> BulkSave(dynamic jsnString)
        {
            FBookingAcknowledge modelDynamic = JsonConvert.DeserializeObject<FBookingAcknowledge>(
             Convert.ToString(jsnString),
             new JsonSerializerSettings
             {
                 DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
             });

            DateTime currentDate = DateTime.Now;

            FBookingAcknowledge model = modelDynamic;
            bool isRevised = model.IsRevised;
            bool isAddition = model.IsAddition;
            bool isReviseBBKI = model.IsReviseBBKI;
            bool isUnAcknowledge = model.IsUnAcknowledge;
            string unAcknowledgeReason = model.UnAcknowledgeReason;

            FBookingAcknowledge entity = new FBookingAcknowledge();
            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<FreeConceptMRChild> mrChilds = new List<FreeConceptMRChild>();

            if (model.BookingNo.IsNotNullOrEmpty())
            {
                entity = await _service.GetFBAcknowledgeBulk(model.BookingNo, isAddition);
                entity.FBookingChild.SetModified();

                entity.CollarSizeID = model.CollarSizeID;
                entity.CollarWeightInGm = model.CollarWeightInGm;
                entity.CuffSizeID = model.CuffSizeID;
                entity.CuffWeightInGm = model.CuffWeightInGm;

                entity.ParentYBookingNo = modelDynamic.ParentYBookingNo;
                entity.EntityState = EntityState.Modified;

                entities = await _service.GetFBAcknowledgeMasterBulk(model.BookingNo);
                entities.SetModified();
                entities.ForEach(x =>
                {
                    x.FBookingChild.SetModified();
                    x.CollarSizeID = model.CollarSizeID;
                    x.CollarWeightInGm = model.CollarWeightInGm;
                    x.CuffSizeID = model.CuffSizeID;
                    x.CuffWeightInGm = model.CuffWeightInGm;
                });

                if (!model.IsUnAcknowledge)
                {
                    #region Extend Model
                    var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);

                    var collarListModel = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                    var cuffListModel = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                    var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                    var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                    List<FBookingAcknowledgeChild> collarList = this.GetExtendChilds(collarListModel, collarsEntity, null, model, isAddition);
                    List<FBookingAcknowledgeChild> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, null, model, isAddition);

                    modelDynamic.FBookingChild = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                    modelDynamic.FBookingChild.AddRange(collarList);
                    modelDynamic.FBookingChild.AddRange(cuffList);

                    model.FBookingChild = CommonFunction.DeepClone(modelDynamic.FBookingChild);

                    #endregion

                    if (!isAddition)
                    {
                        entity.DateUpdated = currentDate;
                        entity.UpdatedBy = AppUser.UserCode;

                        if (model.IsCheckByKnittingHead || model.IsRejectByKnittingHead || model.IsUnAcknowledge)
                        {
                            entities.ForEach(e =>
                            {
                                e.EntityState = EntityState.Modified;
                                e.IsSample = model.IsSample;

                                if (model.IsUnAcknowledge)
                                {
                                    e.IsUnAcknowledge = true;
                                    e.UnAcknowledgeBy = AppUser.EmployeeCode;
                                    e.UnAcknowledgeDate = currentDate;
                                    e.UnAcknowledgeReason = model.UnAcknowledgeReason;
                                }

                                if (model.IsKnittingComplete)
                                {
                                    e.IsKnittingComplete = true;
                                    e.KnittingCompleteDate = currentDate;
                                    e.KnittingCompleteBy = AppUser.UserCode;

                                    e.KnittingRevisionNo = entity.RevisionNo;
                                }

                                if (model.IsRevised)
                                {
                                    e.RevisionNo = e.PreRevisionNo;
                                    e.RevisionDate = currentDate;
                                }

                                if (model.IsRejectByKnittingHead)
                                {
                                    e.IsCheckByKnittingHead = false;
                                    e.CheckByKnittingHead = 0;
                                    e.CheckDateKnittingHead = null;

                                    e.IsRejectByKnittingHead = true;
                                    e.RejectByKnittingHead = AppUser.UserCode;
                                    e.RejectDateKnittingHead = currentDate;
                                    e.RejectReasonKnittingHead = model.RejectReasonKnittingHead;
                                }
                                else
                                {
                                    e.IsCheckByKnittingHead = true;
                                    e.CheckByKnittingHead = AppUser.UserCode;
                                    e.CheckDateKnittingHead = currentDate;

                                    e.IsRejectByKnittingHead = false;
                                    e.RejectByKnittingHead = 0;
                                    e.RejectDateKnittingHead = null;
                                }

                                e.EntityState = EntityState.Modified;
                                e.DateUpdated = currentDate;
                                e.UpdatedBy = AppUser.UserCode;
                            });
                        }
                    }
                }
            }

            entity.CompanyID = model.CompanyID;
            entity.ExportOrderID = model.ExportOrderID;

            string grpConceptNo = model.grpConceptNo;
            int isBDS = model.IsBDS;

            int preRevisionNo = model.PreRevisionNo;

            entity.IsSample = model.IsSample;
            entity.AddedBy = AppUser.UserCode;

            if (model.IsKnittingComplete)
            {
                if (model.IsRevised)
                {
                    entity.RevisionNo = entity.PreRevisionNo;
                    entity.RevisionDate = currentDate;
                }
                entity.IsKnittingComplete = true;
                entity.KnittingCompleteBy = AppUser.UserCode;
                entity.KnittingCompleteDate = currentDate;

                entity.KnittingRevisionNo = entity.RevisionNo;
            }

            if (model.IsUnAcknowledge)
            {
                entity.IsUnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.EmployeeCode;
                entity.UnAcknowledgeDate = currentDate;
                entity.UnAcknowledgeReason = model.UnAcknowledgeReason;

                entity.IsInternalRevise = false;

                entity.IsRejectByAllowance = false;
                entity.IsRejectByKnittingHead = false;
                entity.IsRejectByProdHead = false;

                entity.RejectByAllowance = 0;
                entity.RejectByKnittingHead = 0;
                entity.RejectByProdHead = 0;

                entities.ForEach(x =>
                {
                    x.IsUnAcknowledge = true;
                    x.UnAcknowledgeBy = AppUser.EmployeeCode;
                    x.UnAcknowledgeDate = currentDate;
                    x.UnAcknowledgeReason = model.UnAcknowledgeReason;

                    x.IsInternalRevise = false;

                    x.IsRejectByAllowance = false;
                    x.IsRejectByKnittingHead = false;
                    x.IsRejectByProdHead = false;

                    x.RejectByAllowance = 0;
                    x.RejectByKnittingHead = 0;
                    x.RejectByProdHead = 0;
                });
            }

            if (!model.IsUnAcknowledge)
            {
                List<YarnBookingMaster> yarnBookings = entity.YarnBookings;
                List<YarnBookingChild> tempYarnBookingChilds = new List<YarnBookingChild>();
                List<YarnBookingChildItem> tempYarnBookingChildItems = new List<YarnBookingChildItem>();

                mrChilds = await _fcMRService.GetMRChildByBookingNo(model.BookingNo);
                mrChilds.SetModified();

                yarnBookings.ForEach(yb =>
                {
                    tempYarnBookingChilds.AddRange(yb.Childs);
                });

                List<ItemMasterBomTemp> ItemList = new List<ItemMasterBomTemp>();
                ItemList = _yarnChildItemMasterRepository.GetItemMasterList(AppConstants.ITEM_SUB_GROUP_YARN_LIVE);


                entity.FBookingChild.ForEach(item =>
                {
                    tempYarnBookingChildItems = new List<YarnBookingChildItem>();

                    FBookingAcknowledgeChild obj = model.FBookingChild.Find(x => x.BookingChildID == item.BookingChildID);

                    if (obj != null)
                    {
                        item.MachineTypeId = obj.MachineTypeId;
                        item.TechnicalNameId = obj.TechnicalNameId;
                        item.MachineGauge = obj.MachineGauge;
                        item.MachineDia = obj.MachineDia;
                        item.GreyReqQty = obj.GreyReqQty;
                        item.GreyLeftOverQty = obj.GreyLeftOverQty;
                        item.GreyProdQty = obj.GreyProdQty;
                        item.FinishFabricUtilizationQty = obj.FinishFabricUtilizationQty;
                        item.ReqFinishFabricQty = obj.ReqFinishFabricQty;
                        item.RefSourceID = obj.RefSourceID;
                        item.RefSourceNo = obj.RefSourceNo;
                        item.SourceConsumptionID = obj.SourceConsumptionID;
                        item.SourceItemMasterID = obj.SourceItemMasterID;
                        item.BookingQtyKG = obj.BookingQtyKG;
                        item.BrandID = obj.BrandID;
                        item.Brand = obj.Brand;
                        item.YarnAllowance = obj.YarnAllowance;
                        //item.Remarks = obj.Remarks;
                        if (isAddition)
                        {
                            item.IsForFabric = obj.IsForFabric;
                            item.BookingQty = obj.IsForFabric ? obj.BookingQty : 0;
                        }
                        item.EntityState = EntityState.Modified;

                        List<YarnBookingChildItem> childItemRecords = new List<YarnBookingChildItem>();
                        childItemRecords = obj.ChildItems;
                        _yarnChildItemMasterRepository.GenerateItemWithYItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref ItemList, ref childItemRecords);

                        obj.ChildItems.ForEach(x =>
                        {
                            var objItem = childItemRecords.Find(y => y.Segment1ValueId == x.Segment1ValueId && y.Segment2ValueId == x.Segment2ValueId
                                                        && y.Segment3ValueId == x.Segment3ValueId && y.Segment4ValueId == x.Segment4ValueId
                                                        && y.Segment5ValueId == x.Segment5ValueId && y.Segment6ValueId == x.Segment6ValueId
                                                        && y.Segment7ValueId == x.Segment7ValueId && y.Segment8ValueId == x.Segment8ValueId
                                                        && y.Segment9ValueId == x.Segment9ValueId && y.Segment10ValueId == x.Segment10ValueId);

                            x.ItemMasterID = objItem.ItemMasterID;
                            x.YItemMasterID = objItem.ItemMasterID;
                            x.BookingChildID = item.BookingChildID;
                            x.ConsumptionID = item.ConsumptionID;
                            x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);


                            mrChilds = this.SetMRChildValues(mrChilds, x);
                        });

                        YarnBookingChild childTemp = tempYarnBookingChilds.Find(x => x.BookingChildID == item.BookingChildID);
                        if (childTemp != null && childTemp.ChildItems.Count() > 0)
                        {
                            childTemp.ChildItems.ForEach(ybci =>
                            {
                                YarnBookingChildItem ybciObj = obj.ChildItems.Find(x => x.YBChildItemID == ybci.YBChildItemID);
                                if (ybciObj == null)
                                {
                                    ybci.EntityState = EntityState.Deleted;
                                    obj.ChildItems.Add(ybci);
                                }
                            });
                        }
                        item.ChildItems = obj.ChildItems;

                        if (entity.YarnBookings.Count() > 0)
                        {
                            YarnBookingMaster MasterDB = null;
                            try
                            {
                                MasterDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);
                            }
                            catch (Exception ex) { }
                            if (MasterDB != null)
                            {
                                YarnBookingChild childsDB = null;
                                try
                                {
                                    childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                      .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                                }
                                catch (Exception ex) { }

                                if (childsDB == null)
                                {
                                    var yb = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);

                                    item.Length = item.Length == null || item.Length == "" ? 0.ToString() : item.Length;
                                    item.Height = item.Height == null || item.Height == "" ? 0.ToString() : item.Height;

                                    YarnBookingChild yarnBookingChild = new YarnBookingChild();
                                    yarnBookingChild = new YarnBookingChild();
                                    yarnBookingChild.IsNewObj = true;
                                    yarnBookingChild.YBChildID = 0;
                                    if (yb != null)
                                    {
                                        yarnBookingChild.YBookingID = yb.YBookingID;
                                    }
                                    yarnBookingChild.BookingChildID = item.BookingChildID;
                                    yarnBookingChild.ConsumptionID = item.ConsumptionID;
                                    yarnBookingChild.ItemMasterID = item.ItemMasterID;
                                    yarnBookingChild.YarnTypeID = 0;
                                    yarnBookingChild.YarnBrandID = item.YarnBrandID;
                                    yarnBookingChild.FUPartID = item.FUPartID;
                                    yarnBookingChild.BookingUnitID = item.BookingUnitID;
                                    yarnBookingChild.BookingQty = item.BookingQty;
                                    yarnBookingChild.FTechnicalName = "";
                                    yarnBookingChild.IsCompleteReceive = item.IsCompleteReceive;
                                    yarnBookingChild.LastDCDate = item.LastDCDate;
                                    yarnBookingChild.ClosingRemarks = item.ClosingRemarks;
                                    yarnBookingChild.Remarks = item.Remarks;
                                    yarnBookingChild.GreyReqQty = item.GreyReqQty;
                                    yarnBookingChild.GreyLeftOverQty = item.GreyLeftOverQty;
                                    yarnBookingChild.GreyProdQty = item.GreyProdQty;

                                    yarnBookingChild.QtyInKG = (Convert.ToDecimal(item.Length) *
                                         Convert.ToDecimal(item.Height) *
                                         Convert.ToDecimal(0.045) *
                                         item.BookingQty) / 420;

                                    yarnBookingChild.ExcessPercentage = item.ExcessPercentage;
                                    yarnBookingChild.ExcessQty = item.ExcessQty;
                                    yarnBookingChild.ExcessQtyInKG = item.ExcessQtyInKG;
                                    yarnBookingChild.TotalQty = item.TotalQty;
                                    yarnBookingChild.YarnAllowance = item.YarnAllowance;
                                    yarnBookingChild.TotalQtyInKG = (Convert.ToDecimal(item.Length) *
                                                    Convert.ToDecimal(item.Height) *
                                                    Convert.ToDecimal(0.045) *
                                                    item.TotalQty) / 420;

                                    yarnBookingChild.EntityState = EntityState.Added;

                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs.Add(yarnBookingChild);

                                    childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                   .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                                }
                                else
                                {
                                    //childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                    //                               .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                                    childsDB.EntityState = EntityState.Modified;
                                    childsDB.Remarks = obj.Remarks;
                                    //entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                    //                               .Childs.Find(x => x.BookingChildID == item.BookingChildID).EntityState = EntityState.Modified;
                                }

                                int indexChild = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                   .Childs.FindIndex(x => x.BookingChildID == item.BookingChildID);

                                item.ChildItems.Where(x => x.EntityState != EntityState.Deleted).ToList().ForEach(childItemModel =>
                                {
                                    int indexCItem = childsDB.ChildItems.FindIndex(x => x.YBChildItemID == childItemModel.YBChildItemID);
                                    if (indexChild > -1 && indexCItem > -1)
                                    {
                                        childItemModel.EntityState = EntityState.Modified;
                                        childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                        entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems[indexCItem] = CommonFunction.DeepClone(childItemModel);
                                    }
                                    else
                                    {
                                        YarnBookingMaster masterObj = CommonFunction.DeepClone(entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID));
                                        YarnBookingChild childObj = CommonFunction.DeepClone(masterObj.Childs[indexChild]);
                                        childItemModel.YBookingID = masterObj.YBookingID;
                                        childItemModel.YBChildID = childObj.YBChildID;
                                        childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                        childItemModel.EntityState = EntityState.Added;
                                        entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems.Add(CommonFunction.DeepClone(childItemModel));
                                    }
                                });
                            }
                        }

                        item.ChildDetails.ForEach(itemDetail =>
                        {
                            itemDetail.TechnicalNameId = obj.TechnicalNameId;
                            itemDetail.EntityState = EntityState.Modified;
                        });

                        #region Finishing Process Operations
                        var childIndexE = entity.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);
                        var childIndexM = model.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);

                        if (childIndexE > -1 && childIndexM > -1)
                        {
                            entity.FBookingChild[childIndexE].PreFinishingProcessChilds.SetUnchanged();
                            entity.FBookingChild[childIndexE].PostFinishingProcessChilds.SetUnchanged();

                            model.FBookingChild[childIndexM].PreFinishingProcessChilds.ForEach(mChildFP =>
                            {
                                int indexChildFP = entity.FBookingChild[childIndexE].PreFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                                if (indexChildFP == -1)
                                {
                                    mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                    mChildFP.EntityState = EntityState.Added;
                                    entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Add(mChildFP);
                                }
                                else
                                {
                                    mChildFP.EntityState = EntityState.Modified;
                                    entity.FBookingChild[childIndexE].PreFinishingProcessChilds[indexChildFP] = mChildFP;
                                }
                            });

                            model.FBookingChild[childIndexM].PostFinishingProcessChilds.ForEach(mChildFP =>
                            {
                                int indexChildFP = entity.FBookingChild[childIndexE].PostFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                                if (indexChildFP == -1)
                                {
                                    mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                    mChildFP.EntityState = EntityState.Added;
                                    entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Add(mChildFP);
                                }
                                else
                                {
                                    mChildFP.EntityState = EntityState.Modified;
                                    entity.FBookingChild[childIndexE].PostFinishingProcessChilds[indexChildFP] = mChildFP;
                                }
                            });

                            entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                            entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                        }
                        #endregion
                    }
                });

                /*entity.FBookingChild.ForEach(item =>
                {
                    tempYarnBookingChildItems = new List<YarnBookingChildItem>();

                    FBookingAcknowledgeChild obj = model.FBookingChild.Find(x => x.BookingChildID == item.BookingChildID);

                    if (obj != null)
                    {
                        item.MachineTypeId = obj.MachineTypeId;
                        item.TechnicalNameId = obj.TechnicalNameId;
                        item.MachineGauge = obj.MachineGauge;
                        item.MachineDia = obj.MachineDia;
                        item.GreyReqQty = obj.GreyReqQty;
                        item.GreyLeftOverQty = obj.GreyLeftOverQty;
                        item.GreyProdQty = obj.GreyProdQty;
                        item.RefSourceID = obj.RefSourceID;
                        item.RefSourceNo = obj.RefSourceNo;
                        item.SourceConsumptionID = obj.SourceConsumptionID;
                        item.SourceItemMasterID = obj.SourceItemMasterID;
                        item.BookingQtyKG = obj.BookingQtyKG;
                        item.BrandID = obj.BrandID;
                        item.Brand = obj.Brand;

                        if (isAddition)
                        {
                            item.IsForFabric = obj.IsForFabric;
                            item.BookingQty = obj.IsForFabric ? obj.BookingQty : 0;
                        }
                        item.EntityState = EntityState.Modified;

                        List<YarnBookingChildItem> childItemRecords = new List<YarnBookingChildItem>();
                        childItemRecords = obj.ChildItems;
                        _yarnChildItemMasterRepository.GenerateItemWithYItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref ItemList, ref childItemRecords);

                        obj.ChildItems.ForEach(x =>
                        {
                            var objItem = childItemRecords.Find(y => y.Segment1ValueId == x.Segment1ValueId && y.Segment2ValueId == x.Segment2ValueId
                                                        && y.Segment3ValueId == x.Segment3ValueId && y.Segment4ValueId == x.Segment4ValueId
                                                        && y.Segment5ValueId == x.Segment5ValueId && y.Segment6ValueId == x.Segment6ValueId
                                                        && y.Segment7ValueId == x.Segment7ValueId && y.Segment8ValueId == x.Segment8ValueId
                                                        && y.Segment9ValueId == x.Segment9ValueId && y.Segment10ValueId == x.Segment10ValueId);

                            x.ItemMasterID = objItem.ItemMasterID;
                            x.YItemMasterID = objItem.ItemMasterID;
                            x.BookingChildID = item.BookingChildID;
                            x.ConsumptionID = item.ConsumptionID;
                            x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);

                            mrChilds = this.SetMRChildValues(mrChilds, x);
                        });

                        YarnBookingChild childTemp = tempYarnBookingChilds.Find(x => x.BookingChildID == item.BookingChildID);
                        if (childTemp != null && childTemp.ChildItems.Count() > 0)
                        {
                            childTemp.ChildItems.ForEach(ybci =>
                            {
                                YarnBookingChildItem ybciObj = obj.ChildItems.Find(x => x.YBChildItemID == ybci.YBChildItemID);
                                if (ybciObj == null)
                                {
                                    ybci.EntityState = EntityState.Deleted;
                                    obj.ChildItems.Add(ybci);
                                }
                            });
                        }
                        item.ChildItems = obj.ChildItems;

                        if (entity.YarnBookings.Count() > 0)
                        {
                            YarnBookingMaster MasterDB = null;
                            try
                            {
                                MasterDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);
                            }
                            catch (Exception ex) { }
                            if (MasterDB != null)
                            {
                                YarnBookingChild childsDB = null;
                                try
                                {
                                    childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                      .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                                }
                                catch (Exception ex) { }

                                if (childsDB == null)
                                {
                                    var yb = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);

                                    item.Length = item.Length == null || item.Length == "" ? 0.ToString() : item.Length;
                                    item.Height = item.Height == null || item.Height == "" ? 0.ToString() : item.Height;

                                    YarnBookingChild yarnBookingChild = new YarnBookingChild();
                                    yarnBookingChild = new YarnBookingChild();
                                    yarnBookingChild.IsNewObj = true;
                                    yarnBookingChild.YBChildID = 0;
                                    if (yb != null)
                                    {
                                        yarnBookingChild.YBookingID = yb.YBookingID;
                                    }
                                    yarnBookingChild.BookingChildID = item.BookingChildID;
                                    yarnBookingChild.ConsumptionID = item.ConsumptionID;
                                    yarnBookingChild.ItemMasterID = item.ItemMasterID;
                                    yarnBookingChild.YarnTypeID = 0;
                                    yarnBookingChild.YarnBrandID = item.YarnBrandID;
                                    yarnBookingChild.FUPartID = item.FUPartID;
                                    yarnBookingChild.BookingUnitID = item.BookingUnitID;
                                    yarnBookingChild.BookingQty = item.BookingQty;
                                    yarnBookingChild.FTechnicalName = "";
                                    yarnBookingChild.IsCompleteReceive = item.IsCompleteReceive;
                                    yarnBookingChild.LastDCDate = item.LastDCDate;
                                    yarnBookingChild.ClosingRemarks = item.ClosingRemarks;

                                    yarnBookingChild.GreyReqQty = item.GreyReqQty;
                                    yarnBookingChild.GreyLeftOverQty = item.GreyLeftOverQty;
                                    yarnBookingChild.GreyProdQty = item.GreyProdQty;

                                    yarnBookingChild.QtyInKG = (Convert.ToDecimal(item.Length) *
                                         Convert.ToDecimal(item.Height) *
                                         Convert.ToDecimal(0.045) *
                                         item.BookingQty) / 420;

                                    yarnBookingChild.ExcessPercentage = item.ExcessPercentage;
                                    yarnBookingChild.ExcessQty = item.ExcessQty;
                                    yarnBookingChild.ExcessQtyInKG = item.ExcessQtyInKG;
                                    yarnBookingChild.TotalQty = item.TotalQty;
                                    yarnBookingChild.TotalQtyInKG = (Convert.ToDecimal(item.Length) *
                                                    Convert.ToDecimal(item.Height) *
                                                    Convert.ToDecimal(0.045) *
                                                    item.TotalQty) / 420;

                                    yarnBookingChild.EntityState = EntityState.Added;

                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs.Add(yarnBookingChild);

                                    childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                   .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                                }
                                else
                                {
                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                   .Childs.Find(x => x.BookingChildID == item.BookingChildID).EntityState = EntityState.Modified;
                                }

                                int indexChild = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                   .Childs.FindIndex(x => x.BookingChildID == item.BookingChildID);

                                item.ChildItems.Where(x => x.EntityState != EntityState.Deleted).ToList().ForEach(childItemModel =>
                                {
                                    int indexCItem = childsDB.ChildItems.FindIndex(x => x.YBChildItemID == childItemModel.YBChildItemID);
                                    if (indexChild > -1 && indexCItem > -1)
                                    {
                                        childItemModel.EntityState = EntityState.Modified;
                                        childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                        entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems[indexCItem] = CommonFunction.DeepClone(childItemModel);
                                    }
                                    else
                                    {
                                        YarnBookingMaster masterObj = CommonFunction.DeepClone(entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID));
                                        YarnBookingChild childObj = CommonFunction.DeepClone(masterObj.Childs[indexChild]);
                                        childItemModel.YBookingID = masterObj.YBookingID;
                                        childItemModel.YBChildID = childObj.YBChildID;
                                        childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                        childItemModel.EntityState = EntityState.Added;
                                        entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems.Add(CommonFunction.DeepClone(childItemModel));
                                    }
                                });
                            }
                        }

                        item.ChildDetails.ForEach(itemDetail =>
                        {
                            itemDetail.TechnicalNameId = obj.TechnicalNameId;
                            itemDetail.EntityState = EntityState.Modified;
                        });

                        #region Finishing Process Operations
                        var childIndexE = entity.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);
                        var childIndexM = model.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);

                        if (childIndexE > -1 && childIndexM > -1)
                        {
                            entity.FBookingChild[childIndexE].PreFinishingProcessChilds.SetUnchanged();
                            entity.FBookingChild[childIndexE].PostFinishingProcessChilds.SetUnchanged();

                            model.FBookingChild[childIndexM].PreFinishingProcessChilds.ForEach(mChildFP =>
                            {
                                int indexChildFP = entity.FBookingChild[childIndexE].PreFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                                if (indexChildFP == -1)
                                {
                                    mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                    mChildFP.EntityState = EntityState.Added;
                                    entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Add(mChildFP);
                                }
                                else
                                {
                                    mChildFP.EntityState = EntityState.Modified;
                                    entity.FBookingChild[childIndexE].PreFinishingProcessChilds[indexChildFP] = mChildFP;
                                }
                            });

                            model.FBookingChild[childIndexM].PostFinishingProcessChilds.ForEach(mChildFP =>
                            {
                                int indexChildFP = entity.FBookingChild[childIndexE].PostFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                                if (indexChildFP == -1)
                                {
                                    mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                    mChildFP.EntityState = EntityState.Added;
                                    entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Add(mChildFP);
                                }
                                else
                                {
                                    mChildFP.EntityState = EntityState.Modified;
                                    entity.FBookingChild[childIndexE].PostFinishingProcessChilds[indexChildFP] = mChildFP;
                                }
                            });

                            entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                            entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                        }
                        #endregion
                    }
                });*/

                if (!isAddition)
                {
                    if (entity.YarnBookings.Count() > 0)
                    {
                        entity.HasYarnBooking = true;
                    }
                    if (entities.Count() > 0)
                    {
                        entities.ForEach(x =>
                        {
                            x.ExportOrderID = entity.ExportOrderID;
                            x.CompanyID = entity.CompanyID;
                            x.AddedBy = entity.AddedBy;
                            x.DateAdded = entity.DateAdded;
                            x.UpdatedBy = entity.UpdatedBy;
                            x.DateUpdated = entity.DateUpdated;
                        });
                    }
                }

                #region BBKI Revision Operation
                if (isReviseBBKI)
                {
                    entity.UserId = AppUser.UserCode;
                    entity.IsReviseBBKI = true;
                    entity.PreRevisionNoBBKI++;
                    entity.IsUnAcknowledge = false;
                    entity.IsCheckByKnittingHead = false;
                    entity.IsRejectByKnittingHead = false;
                    entity.IsUtilizationProposalSend = false;
                    entity.IsUtilizationProposalConfirmed = false;
                    entity.IsApprovedByProdHead = false;
                    entity.IsRejectByProdHead = false;
                    entity.IsApprovedByPMC = false;
                    entity.IsRejectByPMC = false;
                    entity.IsApprovedByAllowance = false;
                    entity.IsRejectByAllowance = false;
                    entity.RivisionReason = model.RivisionReason;

                    entities.Where(x => x.FBAckID != entity.FBAckID).ToList().ForEach(x =>
                    {
                        x.IsReviseBBKI = entity.IsReviseBBKI;
                        x.PreRevisionNoBBKI = entity.PreRevisionNoBBKI;
                        x.IsUnAcknowledge = false;
                        x.IsCheckByKnittingHead = false;
                        x.IsRejectByKnittingHead = false;
                        x.IsUtilizationProposalSend = false;
                        x.IsUtilizationProposalConfirmed = false;
                        x.IsApprovedByProdHead = false;
                        x.IsRejectByProdHead = false;
                        x.IsApprovedByPMC = false;
                        x.IsRejectByPMC = false;
                        x.IsApprovedByAllowance = false;
                        x.IsRejectByAllowance = false;
                    });
                }
                #endregion
            }

            string result = await _service.SaveAsyncBulk(AppUser.UserCode, entity, entities, isAddition, mrChilds);

            if (isAddition) entity.YBookingNo = result;

            return Ok(entity);
        }
        [Route("bulk/saveAddition")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> BulkSaveAddition(dynamic jsnString)
        {
            FBookingAcknowledge modelDynamic = JsonConvert.DeserializeObject<FBookingAcknowledge>(
             Convert.ToString(jsnString),
             new JsonSerializerSettings
             {
                 DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
             });

            FBookingAcknowledge model = modelDynamic;
            bool isRevised = model.IsRevised;
            bool isAddition = model.IsAddition;
            bool isReviseBBKI = model.IsReviseBBKI;
            bool isUnAcknowledge = model.IsUnAcknowledge;
            string unAcknowledgeReason = model.UnAcknowledgeReason;
            bool isUpdateAddition = model.IsUpdateAddition;
            bool isQtyMoreThan100 = false;
            bool isRevisedYarn = model.IsRevisedYarn;

            FBookingAcknowledge entity = new FBookingAcknowledge();
            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<FreeConceptMRChild> mrChilds = new List<FreeConceptMRChild>();
            List<YarnBookingChildItem> yarnBookingChildItems = new List<YarnBookingChildItem>();

            if (model.BookingNo.IsNotNullOrEmpty())
            {
                entity = await _service.GetFBAcknowledgeBulkAddition(model.BookingNo, isAddition, modelDynamic.ParentYBookingNo, isUpdateAddition);
                entity.FBookingChild.SetModified();

                entity.CollarSizeID = model.CollarSizeID;
                entity.CollarWeightInGm = model.CollarWeightInGm;
                entity.CuffSizeID = model.CuffSizeID;
                entity.CuffWeightInGm = model.CuffWeightInGm;

                entity.ParentYBookingNo = modelDynamic.ParentYBookingNo;
                entity.EntityState = EntityState.Modified;

                entities = await _service.GetFBAcknowledgeMasterBulk(model.BookingNo);
                entities.SetModified();
                entities.ForEach(x =>
                {
                    x.FBookingChild.SetModified();
                    x.CollarSizeID = model.CollarSizeID;
                    x.CollarWeightInGm = model.CollarWeightInGm;
                    x.CuffSizeID = model.CuffSizeID;
                    x.CuffWeightInGm = model.CuffWeightInGm;
                });
                //--
                #region Extend Model
                var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);
                yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByYBookingNoWithRevision(model.YBookingNo);

                var collarListModel = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                var cuffListModel = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                List<FBookingAcknowledgeChild> collarList = this.GetExtendChildsAddition(collarListModel, collarsEntity, yarnBookingChildItems, model, isAddition);
                List<FBookingAcknowledgeChild> cuffList = this.GetExtendChildsAddition(cuffListModel, cuffsEntity, yarnBookingChildItems, model, isAddition);

                modelDynamic.FBookingChild = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                modelDynamic.FBookingChild.AddRange(collarList);
                modelDynamic.FBookingChild.AddRange(cuffList);

                model.FBookingChild = CommonFunction.DeepClone(modelDynamic.FBookingChild);

                #endregion
            }

            entity.CompanyID = model.CompanyID;
            entity.ExportOrderID = model.ExportOrderID;

            string grpConceptNo = model.grpConceptNo;
            int isBDS = model.IsBDS;

            int preRevisionNo = model.PreRevisionNo;

            entity.IsSample = model.IsSample;
            entity.AddedBy = AppUser.UserCode;


            List<YarnBookingMaster> yarnBookings = entity.YarnBookings;
            List<YarnBookingChild> tempYarnBookingChilds = new List<YarnBookingChild>();
            List<YarnBookingChildItem> tempYarnBookingChildItems = new List<YarnBookingChildItem>();

            mrChilds = await _fcMRService.GetMRChildByBookingNo(model.BookingNo);
            mrChilds.SetModified();

            yarnBookings.ForEach(yb =>
            {
                tempYarnBookingChilds.AddRange(yb.Childs);
            });

            List<ItemMasterBomTemp> ItemList = new List<ItemMasterBomTemp>();
            ItemList = _yarnChildItemMasterRepository.GetItemMasterList(AppConstants.ITEM_SUB_GROUP_YARN_LIVE);
            /*
            if (isAddition)
            {
                entity.FBookingChild.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.ChildItems.SetUnchanged();
                });
            }*/
            entity.FBookingChild.ForEach(item =>
            {
                tempYarnBookingChildItems = new List<YarnBookingChildItem>();

                FBookingAcknowledgeChild obj = model.FBookingChild.Find(x => x.BookingChildID == item.BookingChildID);

                if (obj != null)
                {
                    item.MachineTypeId = obj.MachineTypeId;
                    item.TechnicalNameId = obj.TechnicalNameId;
                    item.MachineGauge = obj.MachineGauge;
                    item.MachineDia = obj.MachineDia;
                    item.GreyReqQty = obj.GreyReqQty;
                    item.GreyLeftOverQty = obj.GreyLeftOverQty;
                    item.GreyProdQty = obj.GreyProdQty;
                    item.ReqFinishFabricQty = obj.ReqFinishFabricQty;
                    item.FinishFabricUtilizationQty = obj.FinishFabricUtilizationQty;
                    item.RefSourceID = obj.RefSourceID;
                    item.RefSourceNo = obj.RefSourceNo;
                    item.SourceConsumptionID = obj.SourceConsumptionID;
                    item.SourceItemMasterID = obj.SourceItemMasterID;
                    item.BookingQtyKG = obj.BookingQtyKG;
                    item.BrandID = obj.BrandID;
                    item.Brand = obj.Brand;
                    item.YarnAllowance = obj.YarnAllowance;
                    item.TotalYarnAllowance = obj.TotalYarnAllowance;
                    item.ExistingYarnAllowance = obj.ExistingYarnAllowance;
                    item.ReqFinishFabricQty = obj.ReqFinishFabricQty;

                    item.FinishFabricUtilizationQty = obj.FinishFabricUtilizationQty;
                    item.GreyLeftOverQty = obj.GreyLeftOverQty;
                    item.GreyFabricUtilizationPopUpList = this.GetAdditionGreyUtilizationPopUpList(item, obj);
                    item.FinishFabricUtilizationPopUpList = this.GetAdditionUtilizationPopUpList(item, obj);
                    item.AdditionalReplacementPOPUPList = CommonFunction.DeepClone(this.GetAdditionReplacementPopUpList(item, obj));
                    if (obj.BookingQty > 100) isQtyMoreThan100 = true;
                    if (isAddition)
                    {
                        item.IsForFabric = obj.IsForFabric;
                        item.BookingQty = obj.IsForFabric ? obj.BookingQty : 0;
                    }

                    item.EntityState = EntityState.Modified;

                    List<YarnBookingChildItem> childItemRecords = new List<YarnBookingChildItem>();
                    childItemRecords = obj.ChildItems;
                    _yarnChildItemMasterRepository.GenerateItemWithYItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref ItemList, ref childItemRecords);

                    obj.ChildItems.ForEach(x =>
                    {
                        var objItem = childItemRecords.Find(y => y.Segment1ValueId == x.Segment1ValueId && y.Segment2ValueId == x.Segment2ValueId
                                                    && y.Segment3ValueId == x.Segment3ValueId && y.Segment4ValueId == x.Segment4ValueId
                                                    && y.Segment5ValueId == x.Segment5ValueId && y.Segment6ValueId == x.Segment6ValueId
                                                    && y.Segment7ValueId == x.Segment7ValueId && y.Segment8ValueId == x.Segment8ValueId
                                                    && y.Segment9ValueId == x.Segment9ValueId && y.Segment10ValueId == x.Segment10ValueId);

                        x.ItemMasterID = objItem.ItemMasterID;
                        x.YItemMasterID = objItem.ItemMasterID;
                        x.BookingChildID = item.BookingChildID;
                        x.ConsumptionID = item.ConsumptionID;
                        x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);

                        mrChilds = this.SetMRChildValues(mrChilds, x);
                    });

                    YarnBookingChild childTemp = tempYarnBookingChilds.Find(x => x.BookingChildID == item.BookingChildID);
                    if (childTemp != null && childTemp.ChildItems.Count() > 0)
                    {
                        childTemp.ChildItems.ForEach(ybci =>
                        {
                            YarnBookingChildItem ybciObj = obj.ChildItems.Find(x => x.YBChildItemID == ybci.YBChildItemID);
                            if (ybciObj == null)
                            {
                                ybci.EntityState = EntityState.Deleted;
                                obj.ChildItems.Add(ybci);
                            }
                            else
                            {
                                ybciObj.EntityState = EntityState.Modified;
                            }
                        });
                    }
                    obj.ChildItems.ForEach(x =>
                    {
                        YarnBookingChildItem ChildItemDB = item.ChildItems.Where(m => m.YBChildItemID == x.YBChildItemID).FirstOrDefault();
                        if (ChildItemDB != null)
                        {
                            x.GreyYarnUtilizationPopUpList = GetAdditionGreyYarnUtilizationPopUpList(ChildItemDB.GreyYarnUtilizationPopUpList, x, item.SubGroupID);
                            x.DyedYarnUtilizationPopUpList = GetAdditionDyedYarnUtilizationPopUpList(ChildItemDB.DyedYarnUtilizationPopUpList, x, item.SubGroupID);
                            x.AdditionalNetReqPOPUPList = CommonFunction.DeepClone(this.GetAdditionChildItemNetReqQTYPopUpList(ChildItemDB, x)); // x = model,objItem = Entity 
                            if (x.YarnReqQty > 100) isQtyMoreThan100 = true;
                        }
                        else
                        {
                            x.GreyYarnUtilizationPopUpList = GetAdditionGreyYarnUtilizationPopUpList(new List<BulkBookingGreyYarnUtilization>(), x, item.SubGroupID);
                            x.DyedYarnUtilizationPopUpList = GetAdditionDyedYarnUtilizationPopUpList(new List<BulkBookingDyedYarnUtilization>(), x, item.SubGroupID);
                            x.AdditionalNetReqPOPUPList = CommonFunction.DeepClone(this.GetAdditionChildItemNetReqQTYPopUpList(new YarnBookingChildItem(), x)); // x = model,objItem = Entity 

                        }
                    });
                    item.ChildItems = obj.ChildItems;

                    if (entity.YarnBookings.Count() > 0)
                    {
                        YarnBookingMaster MasterDB = null;
                        try
                        {
                            MasterDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);
                        }
                        catch (Exception ex) { }
                        if (MasterDB != null)
                        {
                            YarnBookingChild childsDB = null;
                            try
                            {
                                childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                  .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                            }
                            catch (Exception ex) { }

                            if (childsDB == null)
                            {
                                var yb = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);

                                item.Length = item.Length == null || item.Length == "" ? 0.ToString() : item.Length;
                                item.Height = item.Height == null || item.Height == "" ? 0.ToString() : item.Height;

                                YarnBookingChild yarnBookingChild = new YarnBookingChild();
                                yarnBookingChild = new YarnBookingChild();
                                yarnBookingChild.IsNewObj = true;
                                yarnBookingChild.YBChildID = 0;
                                if (yb != null)
                                {
                                    yarnBookingChild.YBookingID = yb.YBookingID;
                                }
                                yarnBookingChild.BookingChildID = item.BookingChildID;
                                yarnBookingChild.ConsumptionID = item.ConsumptionID;
                                yarnBookingChild.ItemMasterID = item.ItemMasterID;
                                yarnBookingChild.YarnTypeID = 0;
                                yarnBookingChild.YarnBrandID = item.YarnBrandID;
                                yarnBookingChild.FUPartID = item.FUPartID;
                                yarnBookingChild.BookingUnitID = item.BookingUnitID;
                                yarnBookingChild.BookingQty = item.BookingQty;
                                yarnBookingChild.FTechnicalName = "";
                                yarnBookingChild.IsCompleteReceive = item.IsCompleteReceive;
                                yarnBookingChild.LastDCDate = item.LastDCDate;
                                yarnBookingChild.ClosingRemarks = item.ClosingRemarks;

                                yarnBookingChild.GreyReqQty = item.GreyReqQty;
                                yarnBookingChild.GreyLeftOverQty = item.GreyLeftOverQty;
                                yarnBookingChild.GreyProdQty = item.GreyProdQty;
                                yarnBookingChild.ReqFinishFabricQty = item.ReqFinishFabricQty;
                                yarnBookingChild.FinishFabricUtilizationQty = item.FinishFabricUtilizationQty;

                                yarnBookingChild.QtyInKG = (Convert.ToDecimal(item.Length) *
                                     Convert.ToDecimal(item.Height) *
                                     Convert.ToDecimal(0.045) *
                                     item.BookingQty) / 420;

                                yarnBookingChild.ExcessPercentage = item.ExcessPercentage;
                                yarnBookingChild.ExcessQty = item.ExcessQty;
                                yarnBookingChild.ExcessQtyInKG = item.ExcessQtyInKG;
                                yarnBookingChild.TotalQty = item.TotalQty;
                                yarnBookingChild.YarnAllowance = item.YarnAllowance;
                                yarnBookingChild.QtyInKG = item.BookingQtyKG;
                                yarnBookingChild.TotalQtyInKG = (Convert.ToDecimal(item.Length) *
                                                Convert.ToDecimal(item.Height) *
                                                Convert.ToDecimal(0.045) *
                                                item.TotalQty) / 420;

                                yarnBookingChild.EntityState = EntityState.Added;

                                entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs.Add(yarnBookingChild);

                                childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                               .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                            }
                            else
                            {
                                entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                               .Childs.Find(x => x.BookingChildID == item.BookingChildID).EntityState = EntityState.Modified;
                            }

                            int indexChild = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                               .Childs.FindIndex(x => x.BookingChildID == item.BookingChildID);

                            item.ChildItems.Where(x => x.EntityState != EntityState.Deleted).ToList().ForEach(childItemModel =>
                            {
                                int indexCItem = childsDB.ChildItems.FindIndex(x => x.YBChildItemID == childItemModel.YBChildItemID);
                                if (indexChild > -1 && indexCItem > -1)
                                {
                                    childItemModel.EntityState = EntityState.Modified;
                                    childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems[indexCItem] = CommonFunction.DeepClone(childItemModel);
                                }
                                else
                                {
                                    YarnBookingMaster masterObj = CommonFunction.DeepClone(entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID));
                                    YarnBookingChild childObj = CommonFunction.DeepClone(masterObj.Childs[indexChild]);
                                    childItemModel.YBookingID = masterObj.YBookingID;
                                    childItemModel.YBChildID = childObj.YBChildID;
                                    childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                    childItemModel.EntityState = EntityState.Added;
                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems.Add(CommonFunction.DeepClone(childItemModel));
                                }
                            });
                        }
                    }

                    item.ChildDetails.ForEach(itemDetail =>
                    {
                        itemDetail.TechnicalNameId = obj.TechnicalNameId;
                        itemDetail.EntityState = EntityState.Modified;
                    });

                    #region Finishing Process Operations
                    var childIndexE = entity.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);
                    var childIndexM = model.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);

                    if (childIndexE > -1 && childIndexM > -1)
                    {
                        entity.FBookingChild[childIndexE].PreFinishingProcessChilds.SetUnchanged();
                        entity.FBookingChild[childIndexE].PostFinishingProcessChilds.SetUnchanged();

                        model.FBookingChild[childIndexM].PreFinishingProcessChilds.ForEach(mChildFP =>
                        {
                            int indexChildFP = entity.FBookingChild[childIndexE].PreFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                            if (indexChildFP == -1)
                            {
                                mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                mChildFP.EntityState = EntityState.Added;
                                entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Add(mChildFP);
                            }
                            else
                            {
                                mChildFP.EntityState = EntityState.Modified;
                                entity.FBookingChild[childIndexE].PreFinishingProcessChilds[indexChildFP] = mChildFP;
                            }
                        });

                        model.FBookingChild[childIndexM].PostFinishingProcessChilds.ForEach(mChildFP =>
                        {
                            int indexChildFP = entity.FBookingChild[childIndexE].PostFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                            if (indexChildFP == -1)
                            {
                                mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                mChildFP.EntityState = EntityState.Added;
                                entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Add(mChildFP);
                            }
                            else
                            {
                                mChildFP.EntityState = EntityState.Modified;
                                entity.FBookingChild[childIndexE].PostFinishingProcessChilds[indexChildFP] = mChildFP;
                            }
                        });

                        entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                        entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                    }
                    #endregion
                }
            });

            //entity.YarnBookings.ForEach(yb=> {
            //    yb.Childs.ForEach(child => {
            //        child.AdditionalReplacementPOPUPList = entity.FBookingChild.Where(x => x.BookingChildID == child.BookingChildID).FirstOrDefault().AdditionalReplacementPOPUPList;
            //    });
            //});

            #region Addition Approve Reject
            bool isReject = model.IsReject;
            int paramTypeId = model.ParamTypeId;
            entity.ParamTypeId = model.ParamTypeId;
            entity.IsReject = model.IsReject;
            entity.IsApprove = model.IsApprove;
            entity.IsAdditionalRevise = model.IsAdditionalRevise;
            entity.YarnBookings.ForEach(YB =>
            {
                YB.IsApprove = model.IsApprove;
                YB.IsReject = model.IsReject;
                YB.UnAcknowledge = false;
                YB.UnAcknowledgeBy = 0;
                YB.UnAcknowledgeDate = null;
                YB.UnAckReason = "";
                if (isRevisedYarn)
                {
                    YB.IsRevised = true;
                    YB.RevisedBy = AppUser.UserCode;
                    YB.RevisedDate = DateTime.Now;
                    YB.UnAcknowledge = false;
                    //YB.EntityState = EntityState.Modified;
                    YB.IsQtyFinalizationPMCApprove = false;
                    YB.QtyFinalizationPMCApproveBy = 0;
                    YB.QtyFinalizationPMCApproveDate = null;
                    YB.IsProdHeadApprove = false;
                    YB.ProdHeadApproveBy = 0;
                    YB.ProdHeadApproveDate = null;
                    YB.IsTextileHeadApprove = false;
                    YB.TextileHeadApproveBy = 0;
                    YB.TextileHeadApproveDate = null;
                    YB.IsKnittingUtilizationApprove = false;
                    YB.KnittingUtilizationApproveBy = 0;
                    YB.KnittingUtilizationApproveDate = null;
                    YB.IsKnittingHeadApprove = false;
                    YB.KnittingHeadApproveBy = 0;
                    YB.KnittingHeadApproveDate = null;
                    YB.IsOperationHeadApprove = false;
                    YB.OperationHeadApproveBy = 0;
                    YB.OperationHeadApproveDate = null;

                    //YB.PreProcessRevNo = YB.PreProcessRevNo + 1;

                }
                YB.EntityState = EntityState.Modified;
                switch (paramTypeId)
                {
                    case (int)ParamTypeId.AYBQtyFinalizationPMC:
                        if (isReject)
                        {
                            YB.IsQtyFinalizationPMCReject = true;
                            YB.QtyFinalizationPMCRejectBy = AppUser.UserCode;
                            YB.QtyFinalizationPMCRejectDate = DateTime.Now;
                            YB.QtyFinalizationPMCRejectReason = model.QtyFinalizationPMCRejectReason;

                            YB.IsQtyFinalizationPMCApprove = false;
                            YB.QtyFinalizationPMCApproveBy = 0;
                            YB.QtyFinalizationPMCApproveDate = null;
                        }
                        else
                        {
                            YB.IsQtyFinalizationPMCApprove = true;
                            YB.QtyFinalizationPMCApproveBy = AppUser.UserCode;
                            YB.QtyFinalizationPMCApproveDate = DateTime.Now;

                            YB.IsQtyFinalizationPMCReject = false;
                            YB.QtyFinalizationPMCRejectBy = 0;
                            YB.QtyFinalizationPMCRejectDate = null;
                            YB.QtyFinalizationPMCRejectReason = "";
                        }
                        break;
                    case (int)ParamTypeId.AYBProdHeadApproval:
                        if (isReject)
                        {
                            YB.IsProdHeadReject = true;
                            YB.ProdHeadRejectBy = AppUser.UserCode;
                            YB.ProdHeadRejectDate = DateTime.Now;
                            YB.ProdHeadRejectReason = model.ProdHeadRejectReason;

                            YB.IsProdHeadApprove = false;
                            YB.ProdHeadApproveBy = 0;
                            YB.ProdHeadApproveDate = null;
                        }
                        else
                        {
                            YB.IsProdHeadApprove = true;
                            YB.ProdHeadApproveBy = AppUser.UserCode;
                            YB.ProdHeadApproveDate = DateTime.Now;

                            YB.IsProdHeadReject = false;
                            YB.ProdHeadRejectBy = 0;
                            YB.ProdHeadRejectDate = null;
                            YB.ProdHeadRejectReason = "";

                            if (!isQtyMoreThan100) //If Less than 100 then auti textile head approve
                            {
                                YB.IsTextileHeadApprove = true;
                            }
                        }
                        break;
                    case (int)ParamTypeId.AYBTextileHeadApproval:
                        if (isReject)
                        {
                            YB.IsTextileHeadReject = true;
                            YB.TextileHeadRejectBy = AppUser.UserCode;
                            YB.TextileHeadRejectDate = DateTime.Now;
                            YB.TextileHeadRejectReason = model.TextileHeadRejectReason;

                            YB.IsTextileHeadApprove = false;
                            YB.TextileHeadApproveBy = 0;
                            YB.TextileHeadApproveDate = null;
                        }
                        else
                        {
                            YB.IsTextileHeadApprove = true;
                            YB.TextileHeadApproveBy = AppUser.UserCode;
                            YB.TextileHeadApproveDate = DateTime.Now;

                            YB.IsTextileHeadReject = false;
                            YB.TextileHeadRejectBy = 0;
                            YB.TextileHeadRejectDate = null;
                            YB.TextileHeadRejectReason = "";
                        }
                        break;
                    case (int)ParamTypeId.AYBKnittingUtilization:
                        if (isReject)
                        {
                            YB.IsKnittingUtilizationReject = true;
                            YB.KnittingUtilizationRejectBy = AppUser.UserCode;
                            YB.KnittingUtilizationRejectDate = DateTime.Now;
                            YB.KnittingUtilizationRejectReason = model.KnittingUtilizationRejectReason;

                            YB.IsKnittingUtilizationApprove = false;
                            YB.KnittingUtilizationApproveBy = 0;
                            YB.KnittingUtilizationApproveDate = null;
                        }
                        else
                        {
                            YB.IsKnittingUtilizationApprove = true;
                            YB.KnittingUtilizationApproveBy = AppUser.UserCode;
                            YB.KnittingUtilizationApproveDate = DateTime.Now;

                            YB.IsKnittingUtilizationReject = false;
                            YB.KnittingUtilizationRejectBy = 0;
                            YB.KnittingUtilizationRejectDate = null;
                            YB.KnittingUtilizationRejectReason = "";

                        }
                        break;
                    case (int)ParamTypeId.AYBKnittingHeadApproval:
                        if (isReject)
                        {
                            YB.IsKnittingHeadReject = true;
                            YB.KnittingHeadRejectBy = AppUser.UserCode;
                            YB.KnittingHeadRejectDate = DateTime.Now;
                            YB.KnittingHeadRejectReason = model.KnittingHeadRejectReason;

                            YB.IsKnittingHeadApprove = false;
                            YB.KnittingHeadApproveBy = 0;
                            YB.KnittingHeadApproveDate = null;
                        }
                        else
                        {
                            YB.IsKnittingHeadApprove = true;
                            YB.KnittingHeadApproveBy = AppUser.UserCode;
                            YB.KnittingHeadApproveDate = DateTime.Now;

                            YB.IsKnittingHeadReject = false;
                            YB.KnittingHeadRejectBy = 0;
                            YB.KnittingHeadRejectDate = null;
                            YB.KnittingHeadRejectReason = "";

                            YB.IsOperationHeadApprove = true;
                            YB.OperationHeadApproveBy = AppUser.UserCode;
                            YB.OperationHeadApproveDate = DateTime.Now;

                            YB.PMCFinalApproveCount++;
                            YB.IsAllocationInternalRevise_Additional = false;
                            YB.AllocationInternalReviseBy_Additional = 0;
                            YB.AllocationInternalReviseDate_Additional = null;
                            YB.AllocationInternalReviseReason_Additional = "";

                            YB.Acknowledge = false;
                            YB.AcknowledgeBy = 0;
                            YB.AcknowledgeDate = null;
                            YB.IsRevised = false;

                        }
                        break;
                    case (int)ParamTypeId.AYBOperationHeadApproval:
                        if (isReject)
                        {
                            YB.IsOperationHeadReject = true;
                            YB.OperationHeadRejectBy = AppUser.UserCode;
                            YB.OperationHeadRejectDate = DateTime.Now;
                            YB.OperationHeadRejectReason = model.OperationHeadRejectReason;

                            YB.IsOperationHeadApprove = false;
                            YB.OperationHeadApproveBy = 0;
                            YB.OperationHeadApproveDate = null;
                        }
                        else
                        {
                            YB.IsOperationHeadApprove = true;
                            YB.OperationHeadApproveBy = AppUser.UserCode;
                            YB.OperationHeadApproveDate = DateTime.Now;

                            YB.IsOperationHeadReject = false;
                            YB.OperationHeadRejectBy = 0;
                            YB.OperationHeadRejectDate = null;
                            YB.OperationHeadRejectReason = "";
                        }
                        break;
                    default:
                        // code block
                        break;
                }
            });
            #endregion

            string result = await _service.SaveAsyncBulkAddition(AppUser.UserCode, entity, entities, isAddition, mrChilds, isUpdateAddition, model.RevisionReasonList, isRevisedYarn);

            if (isAddition) entity.YBookingNo = result;

            if (paramTypeId == (int)ParamTypeId.AYBOperationHeadApproval)
            {
                Boolean IsSendMail = false;
                string BookingNo = model.BookingNo;
                string YBookingNo = model.YBookingNo;
                Boolean IsYarnRevision = model.IsYarnRevision;
                string BuyerName = model.BuyerName;
                string BuyerTeam = model.BuyerTeamName;
                int RevisionNo = model.RevisionNo;

                //OFF FOR CORE//IsSendMail = await SystemMail(BookingNo, YBookingNo, IsYarnRevision, BuyerName, BuyerTeam, RevisionNo, IsSendMail);
            }

            return Ok(entity);
        }
        [Route("bulk/saveWithFreeConcept")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> BulkSaveWithFreeConcept(dynamic jsnString)
        {
            FBookingAcknowledge modelDynamic = JsonConvert.DeserializeObject<FBookingAcknowledge>(
             Convert.ToString(jsnString),
             new JsonSerializerSettings
             {
                 DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
             });
            DateTime currentDate = DateTime.Now;

            FBookingAcknowledge model = modelDynamic;
            bool isRevised = model.IsRevised;
            bool isYarnRevision = model.IsYarnRevision;
            bool isAddition = model.IsAddition;
            bool isReviseBBKI = model.IsReviseBBKI;

            FBookingAcknowledge entity = new FBookingAcknowledge();
            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<YarnBookingChildItem> yarnBookingChildItems = new List<YarnBookingChildItem>();
            List<YarnBookingMaster> entitiesYB = new List<YarnBookingMaster>();

            if (model.BookingNo.IsNotNullOrEmpty())
            {
                entity = await _service.GetFBAcknowledgeBulk(model.BookingNo, isAddition);
                entity.ParentYBookingNo = modelDynamic.ParentYBookingNo;
                entity.EntityState = EntityState.Modified;
                entity.DateUpdated = currentDate;
                entity.UpdatedBy = AppUser.UserCode;

                if (model.BtnId == "btnSave" && (model.GridStatus == Status.Draft || model.GridStatus == Status.Internal_Rejection))
                {
                    if (model.GridStatus == Status.Internal_Rejection)
                    {
                        entity.IsValidForYarnBookingAcknowledge = true;
                    }
                    entity.IsApprovedByAllowance = false;
                    entity.IsCheckByKnittingHead = false;
                    entity.IsApprovedByProdHead = false;
                    entity.IsApprovedByPMC = false;
                    entity.IsRejectByAllowance = false;

                    entity.IsUtilizationProposalSend = false;
                    entity.IsUtilizationProposalConfirmed = false;

                    entity.IsRejectByKnittingHead = false;
                    entity.IsRejectByProdHead = false;
                    entity.IsRejectByPMC = false;

                    entitiesYB = await _serviceYB.GetByBookingNo(model.BookingNo, isAddition);
                    entitiesYB.ForEach(y =>
                    {
                        y.EntityState = EntityState.Modified;
                        y.UnAcknowledge = false;
                    });
                }

                entity.CollarSizeID = model.CollarSizeID;
                entity.CollarWeightInGm = model.CollarWeightInGm;
                entity.CuffSizeID = model.CuffSizeID;
                entity.CuffWeightInGm = model.CuffWeightInGm;

                entities = await _service.GetFBAcknowledgeMasterBulk(model.BookingNo, isAddition);
                entities.ForEach(x =>
                {
                    x.EntityState = EntityState.Modified;
                    x.DateUpdated = entity.DateUpdated;
                    x.UpdatedBy = AppUser.UserCode;

                    if (model.BtnId == "btnSave" && (model.GridStatus == Status.Draft || model.GridStatus == Status.Internal_Rejection))
                    {
                        if (model.GridStatus == Status.Internal_Rejection)
                        {
                            x.IsValidForYarnBookingAcknowledge = true;
                        }
                        x.IsApprovedByAllowance = false;
                        x.IsCheckByKnittingHead = false;
                        x.IsApprovedByProdHead = false;
                        x.IsApprovedByPMC = false;
                        x.IsRejectByAllowance = false;
                        x.IsUtilizationProposalSend = false;
                        x.IsUtilizationProposalConfirmed = false;
                        x.IsRejectByKnittingHead = false;
                        x.IsRejectByProdHead = false;
                        x.IsRejectByPMC = false;
                    }
                });
                //--
                #region Extend Model
                var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);
                yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByBookingNoWithRevision(model.BookingNo);

                var collarListModel = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                var cuffListModel = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                List<FBookingAcknowledgeChild> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnBookingChildItems, model);
                List<FBookingAcknowledgeChild> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnBookingChildItems, model);

                modelDynamic.FBookingChild = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                modelDynamic.FBookingChild.AddRange(collarList);
                modelDynamic.FBookingChild.AddRange(cuffList);

                model.FBookingChild = CommonFunction.DeepClone(modelDynamic.FBookingChild);

                #endregion

                if (modelDynamic.GridStatus == Status.Internal_Rejection)
                {
                    if (modelDynamic.BtnId == "btnSave")
                    {
                        entity.IsReject = false;
                        entity.IsUnAcknowledge = false;

                        entity.IsApprovedByAllowance = false;
                        entity.IsCheckByKnittingHead = false;
                        entity.IsApprovedByProdHead = false;
                        entity.IsApprovedByPMC = false;

                        entity.IsRejectByAllowance = false;
                        entity.IsRejectByKnittingHead = false;
                        entity.IsRejectByProdHead = false;
                        entity.IsRejectByPMC = false;
                    }
                    else if (modelDynamic.BtnId == "btnUnAcknowledge")
                    {
                        entity.IsUnAcknowledge = true;
                        entity.UnAcknowledgeReason = modelDynamic.UnAcknowledgeReason;
                        entity.UnAcknowledgeBy = AppUser.UserCode;
                        entity.UnAcknowledgeDate = currentDate;
                    }
                }

                if (!isAddition)
                {
                    entity.DateUpdated = currentDate;
                    entity.UpdatedBy = AppUser.UserCode;

                    if (modelDynamic.GridStatus == Status.Internal_Rejection)
                    {
                        entities.ForEach(e =>
                        {
                            e.EntityState = EntityState.Modified;
                            e.DateUpdated = currentDate;
                            e.UpdatedBy = AppUser.UserCode;

                            if (modelDynamic.BtnId == "btnSave")
                            {
                                e.IsReject = false;
                                e.IsUnAcknowledge = false;

                                e.IsApprovedByAllowance = false;
                                e.IsCheckByKnittingHead = false;
                                e.IsApprovedByProdHead = false;
                                e.IsApprovedByPMC = false;

                                e.IsRejectByAllowance = false;
                                e.IsRejectByKnittingHead = false;
                                e.IsRejectByProdHead = false;
                                e.IsRejectByPMC = false;
                            }
                            else if (modelDynamic.BtnId == "btnUnAcknowledge")
                            {
                                e.IsUnAcknowledge = true;
                                e.UnAcknowledgeReason = modelDynamic.UnAcknowledgeReason;
                                e.UnAcknowledgeBy = AppUser.UserCode;
                                e.UnAcknowledgeDate = currentDate;
                            }
                        });
                    }
                    else if ((model.IsCheckByKnittingHead || model.IsRejectByKnittingHead || model.IsUnAcknowledge))
                    {
                        entities.ForEach(e =>
                        {
                            e.EntityState = EntityState.Modified;
                            e.IsSample = model.IsSample;
                            e.IsInternalRevise = false;

                            if (model.IsUnAcknowledge)
                            {
                                e.IsUnAcknowledge = true;
                                e.UnAcknowledgeBy = AppUser.EmployeeCode;
                                e.UnAcknowledgeDate = currentDate;
                                e.UnAcknowledgeReason = model.UnAcknowledgeReason;
                            }

                            if (model.IsKnittingComplete)
                            {
                                e.IsKnittingComplete = true;
                                e.KnittingCompleteBy = AppUser.UserCode;
                                e.KnittingCompleteDate = currentDate;

                                e.KnittingRevisionNo = entity.RevisionNo;
                            }

                            if (model.IsRevised)
                            {
                                e.RevisionNo = e.PreRevisionNo;
                                e.RevisionDate = currentDate;
                            }

                            if (model.IsRejectByKnittingHead)
                            {
                                e.IsCheckByKnittingHead = false;
                                e.CheckByKnittingHead = 0;
                                e.CheckDateKnittingHead = null;

                                e.IsRejectByKnittingHead = true;
                                e.RejectByKnittingHead = AppUser.UserCode;
                                e.RejectDateKnittingHead = currentDate;
                                e.RejectReasonKnittingHead = model.RejectReasonKnittingHead;
                            }
                            else
                            {
                                e.IsCheckByKnittingHead = true;
                                e.CheckByKnittingHead = AppUser.UserCode;
                                e.CheckDateKnittingHead = currentDate;

                                e.IsRejectByKnittingHead = false;
                                e.RejectByKnittingHead = 0;
                                e.RejectDateKnittingHead = null;
                            }

                            e.EntityState = EntityState.Modified;
                            e.DateUpdated = currentDate;
                            e.UpdatedBy = AppUser.UserCode;
                        });
                    }
                }
            }

            entity.CompanyID = model.CompanyID;
            entity.ExportOrderID = model.ExportOrderID;

            string grpConceptNo = model.grpConceptNo;
            int isBDS = model.IsBDS;

            int preRevisionNo = model.PreRevisionNo;

            entity.IsSample = model.IsSample;
            entity.AddedBy = AppUser.UserCode;

            if (model.IsKnittingComplete)
            {
                if (model.IsRevised)
                {
                    entity.RevisionNo = entity.PreRevisionNo;
                    entity.RevisionDate = currentDate;

                    entities.ForEach(x =>
                    {
                        x.RevisionNo = entity.PreRevisionNo;
                        x.RevisionDate = currentDate;
                    });
                }
                entity.IsKnittingComplete = true;
                entity.KnittingCompleteBy = AppUser.UserCode;
                entity.KnittingCompleteDate = currentDate;

                entity.KnittingRevisionNo = entity.RevisionNo;

                entities.ForEach(x =>
                {
                    x.IsKnittingComplete = true;
                    x.KnittingCompleteBy = AppUser.UserCode;
                    x.KnittingCompleteDate = currentDate;

                    x.KnittingRevisionNo = entity.RevisionNo;

                    x.CollarSizeID = model.CollarSizeID;
                    x.CollarWeightInGm = model.CollarWeightInGm;
                    x.CuffSizeID = model.CuffSizeID;
                    x.CuffWeightInGm = model.CuffWeightInGm;
                });
            }

            if (model.IsUnAcknowledge)
            {
                entity.IsUnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.EmployeeCode;
                entity.UnAcknowledgeDate = currentDate;
                entity.UnAcknowledgeReason = model.UnAcknowledgeReason;
            }

            List<YarnBookingMaster> yarnBookings = entity.YarnBookings;
            List<YarnBookingChild> tempYarnBookingChilds = new List<YarnBookingChild>();
            List<YarnBookingChildItem> tempYarnBookingChildItems = new List<YarnBookingChildItem>();
            List<FBookingAcknowledgeChild> entityChilds = new List<FBookingAcknowledgeChild>();

            yarnBookings.ForEach(yb =>
            {
                tempYarnBookingChilds.AddRange(yb.Childs);
            });
            List<ItemMasterBomTemp> ItemList = new List<ItemMasterBomTemp>();
            ItemList = _yarnChildItemMasterRepository.GetItemMasterList(AppConstants.ITEM_SUB_GROUP_YARN_LIVE);

            entity.FBookingChild.ForEach(item =>
            {
                tempYarnBookingChildItems = new List<YarnBookingChildItem>();

                if (item.BookingChildID > 0 && item.EntityState == EntityState.Added) item.EntityState = EntityState.Modified;

                FBookingAcknowledgeChild obj = model.FBookingChild.Find(x => x.BookingChildID == item.BookingChildID);

                if (obj != null)
                {
                    item.MachineTypeId = obj.MachineTypeId;
                    item.TechnicalNameId = obj.TechnicalNameId;
                    item.MachineGauge = obj.MachineGauge;
                    item.MachineDia = obj.MachineDia;
                    item.GreyReqQty = obj.GreyReqQty;
                    item.GreyLeftOverQty = obj.GreyLeftOverQty;
                    item.GreyProdQty = obj.GreyProdQty;
                    item.FinishFabricUtilizationQty = obj.FinishFabricUtilizationQty;
                    item.ReqFinishFabricQty = obj.ReqFinishFabricQty;
                    item.RefSourceID = obj.RefSourceID;
                    item.RefSourceNo = obj.RefSourceNo;
                    item.SourceConsumptionID = obj.SourceConsumptionID;
                    item.SourceItemMasterID = obj.SourceItemMasterID;
                    item.BookingQtyKG = obj.BookingQtyKG;
                    item.BrandID = obj.BrandID;
                    item.Brand = obj.Brand;

                    if (isAddition)
                    {
                        item.IsForFabric = obj.IsForFabric;
                        item.BookingQty = obj.IsForFabric ? obj.BookingQty : 0;
                    }
                    item.EntityState = EntityState.Modified;

                    List<YarnBookingChildItem> childItemRecords = new List<YarnBookingChildItem>();
                    childItemRecords = obj.ChildItems;
                    _yarnChildItemMasterRepository.GenerateItemWithYItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref ItemList, ref childItemRecords);

                    obj.ChildItems.ForEach(x =>
                    {
                        var objItem = childItemRecords.Find(y => y.Segment1ValueId == x.Segment1ValueId && y.Segment2ValueId == x.Segment2ValueId
                                                    && y.Segment3ValueId == x.Segment3ValueId && y.Segment4ValueId == x.Segment4ValueId
                                                    && y.Segment5ValueId == x.Segment5ValueId && y.Segment6ValueId == x.Segment6ValueId
                                                    && y.Segment7ValueId == x.Segment7ValueId && y.Segment8ValueId == x.Segment8ValueId
                                                    && y.Segment9ValueId == x.Segment9ValueId && y.Segment10ValueId == x.Segment10ValueId);

                        x.ItemMasterID = objItem.ItemMasterID;
                        x.YItemMasterID = objItem.ItemMasterID;
                        x.BookingChildID = item.BookingChildID;
                        x.ConsumptionID = item.ConsumptionID;
                        x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);
                    });

                    YarnBookingChild childTemp = tempYarnBookingChilds.Find(x => x.BookingChildID == item.BookingChildID);
                    if (childTemp != null && childTemp.ChildItems.Count() > 0)
                    {
                        childTemp.ChildItems.ForEach(ybci =>
                        {
                            YarnBookingChildItem ybciObj = obj.ChildItems.Find(x => x.YBChildItemID == ybci.YBChildItemID);
                            if (ybciObj == null)
                            {
                                ybci.EntityState = EntityState.Deleted;
                                obj.ChildItems.Add(ybci);
                            }
                        });
                    }
                    item.ChildItems = obj.ChildItems;

                    if (entity.YarnBookings.Count() > 0)
                    {
                        YarnBookingMaster MasterDB = null;
                        MasterDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);

                        if (MasterDB != null)
                        {
                            YarnBookingChild childsDB = null;
                            try
                            {
                                childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                  .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                            }
                            catch (Exception ex) { }

                            if (childsDB == null)
                            {
                                var yb = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);

                                item.Length = item.Length == null || item.Length == "" ? 0.ToString() : item.Length;
                                item.Height = item.Height == null || item.Height == "" ? 0.ToString() : item.Height;

                                YarnBookingChild yarnBookingChild = new YarnBookingChild();
                                yarnBookingChild = new YarnBookingChild();
                                yarnBookingChild.IsNewObj = true;
                                yarnBookingChild.YBChildID = 0;
                                if (yb != null)
                                {
                                    yarnBookingChild.YBookingID = yb.YBookingID;
                                }
                                yarnBookingChild.BookingChildID = item.BookingChildID;
                                yarnBookingChild.ConsumptionID = item.ConsumptionID;
                                yarnBookingChild.ItemMasterID = item.ItemMasterID;
                                yarnBookingChild.YarnTypeID = 0;
                                yarnBookingChild.YarnBrandID = item.YarnBrandID;
                                yarnBookingChild.FUPartID = item.FUPartID;
                                yarnBookingChild.BookingUnitID = item.BookingUnitID;
                                yarnBookingChild.BookingQty = item.BookingQty;
                                yarnBookingChild.FTechnicalName = "";
                                yarnBookingChild.IsCompleteReceive = item.IsCompleteReceive;
                                yarnBookingChild.LastDCDate = item.LastDCDate;
                                yarnBookingChild.ClosingRemarks = item.ClosingRemarks;

                                yarnBookingChild.GreyReqQty = item.GreyReqQty;
                                yarnBookingChild.GreyLeftOverQty = item.GreyLeftOverQty;
                                yarnBookingChild.GreyProdQty = item.GreyProdQty;
                                yarnBookingChild.FinishFabricUtilizationQty = item.FinishFabricUtilizationQty;
                                yarnBookingChild.ReqFinishFabricQty = item.ReqFinishFabricQty;

                                yarnBookingChild.QtyInKG = (Convert.ToDecimal(item.Length) *
                                     Convert.ToDecimal(item.Height) *
                                     Convert.ToDecimal(0.045) *
                                     item.BookingQty) / 420;

                                yarnBookingChild.ExcessPercentage = item.ExcessPercentage;
                                yarnBookingChild.ExcessQty = item.ExcessQty;
                                yarnBookingChild.ExcessQtyInKG = item.ExcessQtyInKG;
                                yarnBookingChild.TotalQty = item.TotalQty;
                                yarnBookingChild.TotalQtyInKG = (Convert.ToDecimal(item.Length) *
                                                Convert.ToDecimal(item.Height) *
                                                Convert.ToDecimal(0.045) *
                                                item.TotalQty) / 420;

                                yarnBookingChild.EntityState = EntityState.Added;

                                entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs.Add(yarnBookingChild);

                                childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                               .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                            }
                            else
                            {
                                entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                               .Childs.Find(x => x.BookingChildID == item.BookingChildID).EntityState = EntityState.Modified;
                            }

                            int indexChild = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                               .Childs.FindIndex(x => x.BookingChildID == item.BookingChildID);

                            item.ChildItems.Where(x => x.EntityState != EntityState.Deleted).ToList().ForEach(childItemModel =>
                            {
                                int indexCItem = childsDB.ChildItems.FindIndex(x => x.YBChildItemID == childItemModel.YBChildItemID);
                                if (indexChild > -1 && indexCItem > -1)
                                {
                                    childItemModel.EntityState = EntityState.Modified;
                                    childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems[indexCItem] = CommonFunction.DeepClone(childItemModel);
                                }
                                else
                                {
                                    YarnBookingMaster masterObj = CommonFunction.DeepClone(entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID));
                                    YarnBookingChild childObj = CommonFunction.DeepClone(masterObj.Childs[indexChild]);
                                    childItemModel.YBookingID = masterObj.YBookingID;
                                    childItemModel.YBChildID = childObj.YBChildID;
                                    childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                    childItemModel.EntityState = EntityState.Added;
                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems.Add(CommonFunction.DeepClone(childItemModel));
                                }
                            });
                        }
                    }

                    item.ChildDetails.ForEach(itemDetail =>
                    {
                        itemDetail.TechnicalNameId = obj.TechnicalNameId;
                        itemDetail.EntityState = EntityState.Modified;
                    });

                    #region Finishing Process Operations
                    var childIndexE = entity.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);
                    var childIndexM = model.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);

                    if (childIndexE > -1 && childIndexM > -1)
                    {
                        entity.FBookingChild[childIndexE].PreFinishingProcessChilds.SetUnchanged();
                        entity.FBookingChild[childIndexE].PostFinishingProcessChilds.SetUnchanged();

                        model.FBookingChild[childIndexM].PreFinishingProcessChilds.ForEach(mChildFP =>
                        {
                            int indexChildFP = entity.FBookingChild[childIndexE].PreFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                            if (indexChildFP == -1)
                            {
                                mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                mChildFP.EntityState = EntityState.Added;
                                entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Add(mChildFP);
                            }
                            else
                            {
                                mChildFP.EntityState = EntityState.Modified;
                                entity.FBookingChild[childIndexE].PreFinishingProcessChilds[indexChildFP] = mChildFP;
                            }
                        });

                        model.FBookingChild[childIndexM].PostFinishingProcessChilds.ForEach(mChildFP =>
                        {
                            int indexChildFP = entity.FBookingChild[childIndexE].PostFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                            if (indexChildFP == -1)
                            {
                                mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                mChildFP.EntityState = EntityState.Added;
                                entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Add(mChildFP);
                            }
                            else
                            {
                                mChildFP.EntityState = EntityState.Modified;
                                entity.FBookingChild[childIndexE].PostFinishingProcessChilds[indexChildFP] = mChildFP;
                            }
                        });

                        entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                        entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                    }
                    #endregion
                }

                #region FreeConcept
                FBookingAcknowledgeChild ObjEntityChild = new FBookingAcknowledgeChild();
                ObjEntityChild = CommonFunction.DeepClone(item);
                entityChilds.Add(ObjEntityChild);
                #endregion
            });

            if (!isAddition)
            {
                if (entity.YarnBookings.Count() > 0)
                {
                    entity.HasYarnBooking = true;
                }
                if (entities.Count() > 0)
                {
                    entities.ForEach(x =>
                    {
                        x.ExportOrderID = entity.ExportOrderID;
                        x.CompanyID = entity.CompanyID;
                        x.AddedBy = entity.AddedBy;
                        x.DateAdded = entity.DateAdded;
                        x.UpdatedBy = entity.UpdatedBy;
                        x.DateUpdated = entity.DateUpdated;
                    });
                }
            }

            #region BBKI Revision Operation
            if (isReviseBBKI)
            {
                entity.UserId = AppUser.UserCode;
                entity.IsReviseBBKI = true;
                entity.PreRevisionNoBBKI++;
                entity.IsUnAcknowledge = false;
                entity.IsCheckByKnittingHead = false;
                entity.IsUtilizationProposalSend = false;
                entity.IsUtilizationProposalConfirmed = false;
                entity.IsRejectByKnittingHead = false;
                entity.IsApprovedByProdHead = false;
                entity.IsRejectByProdHead = false;
                entity.IsApprovedByPMC = false;
                entity.IsRejectByPMC = false;
                entity.IsApprovedByAllowance = false;
                entity.IsRejectByAllowance = false;
                entity.RivisionReason = model.RivisionReason;
                entities.Where(x => x.FBAckID != entity.FBAckID).ToList().ForEach(x =>
                {
                    x.IsReviseBBKI = entity.IsReviseBBKI;
                    x.PreRevisionNoBBKI = entity.PreRevisionNoBBKI;
                    x.IsUnAcknowledge = false;
                    x.IsCheckByKnittingHead = false;
                    x.IsUtilizationProposalSend = false;
                    x.IsUtilizationProposalConfirmed = false;
                    x.IsRejectByKnittingHead = false;
                    x.IsApprovedByProdHead = false;
                    x.IsRejectByProdHead = false;
                    x.IsApprovedByPMC = false;
                    x.IsRejectByPMC = false;
                    x.IsApprovedByAllowance = false;
                    x.IsRejectByAllowance = false;
                    x.RivisionReason = model.RivisionReason;
                });
            }
            #endregion

            #region FreeConcept
            entity.PageName = model.PageName;

            string ColorIDs = "";
            ColorIDs = string.Join(",", entityChilds.Select(x => x.Color)).ToString();

            entity.ColorCodeList = await _service.GetAllAsyncColorIDs(ColorIDs);

            List<FreeConceptMaster> entityFreeConcepts = new List<FreeConceptMaster>();
            List<FreeConceptMRMaster> freeConceptMRList = new List<FreeConceptMRMaster>();
            string groupConceptNo = model.BookingNo;// entities.Count() > 0 ? entities[0].BookingNo : "";
            if (groupConceptNo.IsNotNullOrEmpty())
            {
                entityFreeConcepts = await _fcService.GetDatasAsync(groupConceptNo);
                entityFreeConcepts.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.ChildColors.SetUnchanged();

                    if (modelDynamic.PageName == "BulkBookingKnittingInfo")
                    {
                        List<FBookingAcknowledgeChild> fBACs = entityChilds.Where(y => y.SubGroupID == x.SubGroupID && y.ItemMasterID == x.ItemMasterID && y.ConsumptionID == x.ConsumptionID).ToList();

                        if (fBACs.IsNotNull() && fBACs.Count() > 0)
                        {
                            var obj = fBACs.First();

                            fBACs.ForEach(y =>
                            {
                                y.MachineTypeId = obj.MachineTypeId;
                                y.TechnicalNameId = obj.TechnicalNameId;
                                y.MachineGauge = obj.MachineGauge;
                                y.MachineDia = obj.MachineDia;
                                y.BrandID = obj.BrandID;
                                y.EntityState = EntityState.Modified;
                            });

                            x.MCSubClassID = obj.MachineTypeId;
                            x.TechnicalNameId = obj.TechnicalNameId;
                            x.MachineGauge = obj.MachineGauge;
                            x.MachineDia = obj.MachineDia;
                            x.BrandID = obj.BrandID;
                            x.TotalQty = obj.TotalQty;
                            x.TotalQtyInKG = obj.TotalQtyInKG;
                            x.ProduceKnittingQty = obj.GreyProdQty;
                            x.EntityState = EntityState.Modified;
                        }
                    }
                });
                if (isBDS == 2)
                {
                    freeConceptMRList = await _serviceFreeConceptMR.GetByGroupConceptNo(groupConceptNo);
                    freeConceptMRList.ForEach(x =>
                    {
                        x.EntityState = EntityState.Unchanged;
                        x.Childs.SetUnchanged();
                    });
                }
            }
            List<FreeConceptMaster> freeConceptsRevice = new List<FreeConceptMaster>();
            if (entity.BookingNo.IsNotNullOrEmpty())
            {
                freeConceptsRevice = await _service.GetFreeConcepts(entity.BookingNo);
                if (isRevised && !entity.IsUnAcknowledge)
                {
                    freeConceptsRevice.ForEach(c =>
                    {
                        c.PreProcessRevNo = entity.RevisionNo;
                        c.RevisionNo = entity.RevisionNo;
                        c.RevisionBy = AppUser.UserCode;
                        c.RevisionDate = currentDate;
                    });
                }
            }

            #endregion

            entity.FBookingChild = entityChilds;
            entity.IsInternalRevise = false;
            entities.ForEach(x => x.IsInternalRevise = false);
            if (entity.IsAllocationInternalRevise == true)
            {
                entitiesYB.ForEach(m =>
                {
                    m.IsForAllocationRevise = true;
                });
            }
            string result = await _service.SaveAsyncBulkWithFreeConcept(AppUser.UserCode, entity, entities, isAddition, isBDS, entityFreeConcepts, freeConceptMRList, freeConceptsRevice, isRevised, entitiesYB, isYarnRevision, model.RevisionReasonList);

            if (isAddition) entity.YBookingNo = result;

            return Ok(entity);
        }
        [Route("bulk/saveWithFreeConceptWithRevision")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> BulkSaveWithFreeConceptWithRevision(dynamic jsnString)
        {
            FBookingAcknowledge modelDynamic = JsonConvert.DeserializeObject<FBookingAcknowledge>(
             Convert.ToString(jsnString),
             new JsonSerializerSettings
             {
                 DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
             });
            DateTime currentDate = DateTime.Now;

            FBookingAcknowledge model = modelDynamic;
            bool isRevised = model.IsRevised;
            bool isYarnRevision = model.IsYarnRevision;
            bool isAddition = model.IsAddition;
            bool isReviseBBKI = model.IsReviseBBKI;

            FBookingAcknowledge entity = new FBookingAcknowledge();
            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<YarnBookingChildItem> yarnBookingChildItems = new List<YarnBookingChildItem>();
            List<YarnBookingMaster> entitiesYB = new List<YarnBookingMaster>();

            if (model.BookingNo.IsNotNullOrEmpty())
            {
                entity = await _service.GetFBAcknowledgeBulkWithRevision(model.BookingNo, isAddition);
                entity.ParentYBookingNo = modelDynamic.ParentYBookingNo;
                entity.EntityState = EntityState.Modified;
                entity.DateUpdated = currentDate;
                entity.UpdatedBy = AppUser.UserCode;

                if (model.BtnId == "btnSave" && (model.GridStatus == Status.Draft || model.GridStatus == Status.Internal_Rejection))
                {
                    if (model.GridStatus == Status.Internal_Rejection)
                    {
                        entity.IsValidForYarnBookingAcknowledge = true;
                    }
                    entity.IsApprovedByAllowance = false;
                    entity.IsCheckByKnittingHead = false;
                    entity.IsApprovedByProdHead = false;
                    entity.IsApprovedByPMC = false;
                    entity.IsRejectByAllowance = false;

                    entity.IsUtilizationProposalSend = false;
                    entity.IsUtilizationProposalConfirmed = false;

                    entity.IsRejectByKnittingHead = false;
                    entity.IsRejectByProdHead = false;
                    entity.IsRejectByPMC = false;

                    entitiesYB = await _serviceYB.GetByBookingNo(model.BookingNo, isAddition);
                    entitiesYB.ForEach(y =>
                    {
                        y.EntityState = EntityState.Modified;
                        y.UnAcknowledge = false;
                    });
                }

                entity.CollarSizeID = model.CollarSizeID;
                entity.CollarWeightInGm = model.CollarWeightInGm;
                entity.CuffSizeID = model.CuffSizeID;
                entity.CuffWeightInGm = model.CuffWeightInGm;

                entities = await _service.GetFBAcknowledgeMasterBulkWithRevision(model.BookingNo, isAddition);
                entities.ForEach(x =>
                {
                    x.EntityState = EntityState.Modified;
                    x.DateUpdated = entity.DateUpdated;
                    x.UpdatedBy = AppUser.UserCode;

                    if (model.BtnId == "btnSave" && (model.GridStatus == Status.Draft || model.GridStatus == Status.Internal_Rejection))
                    {
                        if (model.GridStatus == Status.Internal_Rejection)
                        {
                            x.IsValidForYarnBookingAcknowledge = true;
                        }
                        x.IsApprovedByAllowance = false;
                        x.IsCheckByKnittingHead = false;
                        x.IsApprovedByProdHead = false;
                        x.IsApprovedByPMC = false;
                        x.IsRejectByAllowance = false;
                        x.IsUtilizationProposalSend = false;
                        x.IsUtilizationProposalConfirmed = false;
                        x.IsRejectByKnittingHead = false;
                        x.IsRejectByProdHead = false;
                        x.IsRejectByPMC = false;
                    }
                });

                #region Extend Model
                var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);
                yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByBookingNoWithRevision(model.BookingNo);

                var collarListModel = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                var cuffListModel = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                List<FBookingAcknowledgeChild> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnBookingChildItems, model);
                List<FBookingAcknowledgeChild> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnBookingChildItems, model);

                modelDynamic.FBookingChild = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                modelDynamic.FBookingChild.AddRange(collarList);
                modelDynamic.FBookingChild.AddRange(cuffList);

                model.FBookingChild = CommonFunction.DeepClone(modelDynamic.FBookingChild);

                #endregion

                if (modelDynamic.GridStatus == Status.Internal_Rejection)
                {
                    if (modelDynamic.BtnId == "btnSave")
                    {
                        entity.IsReject = false;
                        entity.IsUnAcknowledge = false;

                        entity.IsApprovedByAllowance = false;
                        entity.IsCheckByKnittingHead = false;
                        entity.IsApprovedByProdHead = false;
                        entity.IsApprovedByPMC = false;

                        entity.IsRejectByAllowance = false;
                        entity.IsRejectByKnittingHead = false;
                        entity.IsRejectByProdHead = false;
                        entity.IsRejectByPMC = false;
                    }
                    else if (modelDynamic.BtnId == "btnUnAcknowledge")
                    {
                        entity.IsUnAcknowledge = true;
                        entity.UnAcknowledgeReason = modelDynamic.UnAcknowledgeReason;
                        entity.UnAcknowledgeBy = AppUser.UserCode;
                        entity.UnAcknowledgeDate = currentDate;
                    }
                }

                if (!isAddition)
                {
                    entity.DateUpdated = currentDate;
                    entity.UpdatedBy = AppUser.UserCode;

                    if (modelDynamic.GridStatus == Status.Internal_Rejection)
                    {
                        entities.ForEach(e =>
                        {
                            e.EntityState = EntityState.Modified;
                            e.DateUpdated = currentDate;
                            e.UpdatedBy = AppUser.UserCode;

                            if (modelDynamic.BtnId == "btnSave")
                            {
                                e.IsReject = false;
                                e.IsUnAcknowledge = false;

                                e.IsApprovedByAllowance = false;
                                e.IsCheckByKnittingHead = false;
                                e.IsApprovedByProdHead = false;
                                e.IsApprovedByPMC = false;

                                e.IsRejectByAllowance = false;
                                e.IsRejectByKnittingHead = false;
                                e.IsRejectByProdHead = false;
                                e.IsRejectByPMC = false;
                            }
                            else if (modelDynamic.BtnId == "btnUnAcknowledge")
                            {
                                e.IsUnAcknowledge = true;
                                e.UnAcknowledgeReason = modelDynamic.UnAcknowledgeReason;
                                e.UnAcknowledgeBy = AppUser.UserCode;
                                e.UnAcknowledgeDate = currentDate;
                            }
                        });
                    }
                    else if ((model.IsCheckByKnittingHead || model.IsRejectByKnittingHead || model.IsUnAcknowledge))
                    {
                        entities.ForEach(e =>
                        {
                            e.EntityState = EntityState.Modified;
                            e.IsSample = model.IsSample;
                            e.IsInternalRevise = false;

                            if (model.IsUnAcknowledge)
                            {
                                e.IsUnAcknowledge = true;
                                e.UnAcknowledgeBy = AppUser.EmployeeCode;
                                e.UnAcknowledgeDate = currentDate;
                                e.UnAcknowledgeReason = model.UnAcknowledgeReason;
                            }

                            if (model.IsKnittingComplete)
                            {
                                e.IsKnittingComplete = true;
                                e.KnittingCompleteBy = AppUser.UserCode;
                                e.KnittingCompleteDate = currentDate;

                                e.KnittingRevisionNo = entity.RevisionNo;
                            }

                            if (model.IsRevised)
                            {
                                e.RevisionNo = e.PreRevisionNo;
                                e.RevisionDate = currentDate;
                            }

                            if (model.IsRejectByKnittingHead)
                            {
                                e.IsCheckByKnittingHead = false;
                                e.CheckByKnittingHead = 0;
                                e.CheckDateKnittingHead = null;

                                e.IsRejectByKnittingHead = true;
                                e.RejectByKnittingHead = AppUser.UserCode;
                                e.RejectDateKnittingHead = currentDate;
                                e.RejectReasonKnittingHead = model.RejectReasonKnittingHead;
                            }
                            else
                            {
                                e.IsCheckByKnittingHead = true;
                                e.CheckByKnittingHead = AppUser.UserCode;
                                e.CheckDateKnittingHead = currentDate;

                                e.IsRejectByKnittingHead = false;
                                e.RejectByKnittingHead = 0;
                                e.RejectDateKnittingHead = null;
                            }

                            e.EntityState = EntityState.Modified;
                            e.DateUpdated = currentDate;
                            e.UpdatedBy = AppUser.UserCode;
                        });
                    }
                }
            }

            entity.CompanyID = model.CompanyID;
            entity.ExportOrderID = model.ExportOrderID;

            string grpConceptNo = model.grpConceptNo;
            int isBDS = model.IsBDS;

            int preRevisionNo = model.PreRevisionNo;

            entity.IsSample = model.IsSample;
            entity.AddedBy = AppUser.UserCode;

            if (model.IsKnittingComplete)
            {
                if (model.IsRevised)
                {
                    entity.RevisionNo = entity.PreRevisionNo;
                    entity.RevisionDate = currentDate;

                    entities.ForEach(x =>
                    {
                        x.RevisionNo = entity.PreRevisionNo;
                        x.RevisionDate = currentDate;
                    });
                }
                entity.IsKnittingComplete = true;
                entity.KnittingCompleteBy = AppUser.UserCode;
                entity.KnittingCompleteDate = currentDate;

                entity.KnittingRevisionNo = entity.RevisionNo;

                entities.ForEach(x =>
                {
                    x.IsKnittingComplete = true;
                    x.KnittingCompleteBy = AppUser.UserCode;
                    x.KnittingCompleteDate = currentDate;

                    x.KnittingRevisionNo = entity.RevisionNo;

                    x.CollarSizeID = model.CollarSizeID;
                    x.CollarWeightInGm = model.CollarWeightInGm;
                    x.CuffSizeID = model.CuffSizeID;
                    x.CuffWeightInGm = model.CuffWeightInGm;
                });
            }

            if (model.IsUnAcknowledge)
            {
                entity.IsUnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.EmployeeCode;
                entity.UnAcknowledgeDate = currentDate;
                entity.UnAcknowledgeReason = model.UnAcknowledgeReason;
            }

            List<YarnBookingMaster> yarnBookings = entity.YarnBookings;
            List<YarnBookingChild> tempYarnBookingChilds = new List<YarnBookingChild>();
            List<YarnBookingChildItem> tempYarnBookingChildItems = new List<YarnBookingChildItem>();
            List<FBookingAcknowledgeChild> entityChilds = new List<FBookingAcknowledgeChild>();

            yarnBookings.ForEach(yb =>
            {
                tempYarnBookingChilds.AddRange(yb.Childs);
            });
            List<ItemMasterBomTemp> ItemList = new List<ItemMasterBomTemp>();
            ItemList = _yarnChildItemMasterRepository.GetItemMasterList(AppConstants.ITEM_SUB_GROUP_YARN_LIVE);

            entity.FBookingChild.ForEach(item =>
            {
                tempYarnBookingChildItems = new List<YarnBookingChildItem>();

                if (item.BookingChildID > 0 && item.EntityState == EntityState.Added) item.EntityState = EntityState.Modified;

                FBookingAcknowledgeChild obj = model.FBookingChild.Find(x => x.BookingChildID == item.BookingChildID);

                if (obj != null)
                {
                    item.MachineTypeId = obj.MachineTypeId;
                    item.TechnicalNameId = obj.TechnicalNameId;
                    item.MachineGauge = obj.MachineGauge;
                    item.MachineDia = obj.MachineDia;
                    item.GreyReqQty = obj.GreyReqQty;
                    item.GreyLeftOverQty = obj.GreyLeftOverQty;
                    item.GreyProdQty = obj.GreyProdQty;
                    item.FinishFabricUtilizationQty = obj.FinishFabricUtilizationQty;
                    item.ReqFinishFabricQty = obj.ReqFinishFabricQty;
                    item.RefSourceID = obj.RefSourceID;
                    item.RefSourceNo = obj.RefSourceNo;
                    item.SourceConsumptionID = obj.SourceConsumptionID;
                    item.SourceItemMasterID = obj.SourceItemMasterID;
                    item.BookingQtyKG = obj.BookingQtyKG;
                    item.BrandID = obj.BrandID;
                    item.Brand = obj.Brand;

                    if (isAddition)
                    {
                        item.IsForFabric = obj.IsForFabric;
                        item.BookingQty = obj.IsForFabric ? obj.BookingQty : 0;
                    }
                    item.EntityState = EntityState.Modified;

                    List<YarnBookingChildItem> childItemRecords = new List<YarnBookingChildItem>();
                    childItemRecords = obj.ChildItems;
                    _yarnChildItemMasterRepository.GenerateItemWithYItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref ItemList, ref childItemRecords);

                    obj.ChildItems.ForEach(x =>
                    {
                        var objItem = childItemRecords.Find(y => y.Segment1ValueId == x.Segment1ValueId && y.Segment2ValueId == x.Segment2ValueId
                                                    && y.Segment3ValueId == x.Segment3ValueId && y.Segment4ValueId == x.Segment4ValueId
                                                    && y.Segment5ValueId == x.Segment5ValueId && y.Segment6ValueId == x.Segment6ValueId
                                                    && y.Segment7ValueId == x.Segment7ValueId && y.Segment8ValueId == x.Segment8ValueId
                                                    && y.Segment9ValueId == x.Segment9ValueId && y.Segment10ValueId == x.Segment10ValueId);

                        x.ItemMasterID = objItem.ItemMasterID;
                        x.YItemMasterID = objItem.ItemMasterID;
                        x.BookingChildID = item.BookingChildID;
                        x.ConsumptionID = item.ConsumptionID;
                        x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);
                    });

                    YarnBookingChild childTemp = tempYarnBookingChilds.Find(x => x.BookingChildID == item.BookingChildID);
                    if (childTemp != null && childTemp.ChildItems.Count() > 0)
                    {
                        childTemp.ChildItems.ForEach(ybci =>
                        {
                            YarnBookingChildItem ybciObj = obj.ChildItems.Find(x => x.YBChildItemID == ybci.YBChildItemID);
                            if (ybciObj == null)
                            {
                                ybci.EntityState = EntityState.Deleted;
                                obj.ChildItems.Add(ybci);
                            }
                        });
                    }
                    item.ChildItems = obj.ChildItems;

                    if (entity.YarnBookings.Count() > 0)
                    {
                        YarnBookingMaster MasterDB = null;
                        MasterDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);

                        if (MasterDB != null)
                        {
                            YarnBookingChild childsDB = null;
                            try
                            {
                                childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                  .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                            }
                            catch (Exception ex) { }

                            if (childsDB == null)
                            {
                                var yb = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);

                                item.Length = item.Length == null || item.Length == "" ? 0.ToString() : item.Length;
                                item.Height = item.Height == null || item.Height == "" ? 0.ToString() : item.Height;

                                YarnBookingChild yarnBookingChild = new YarnBookingChild();
                                yarnBookingChild = new YarnBookingChild();
                                yarnBookingChild.IsNewObj = true;
                                yarnBookingChild.YBChildID = 0;
                                if (yb != null)
                                {
                                    yarnBookingChild.YBookingID = yb.YBookingID;
                                }
                                yarnBookingChild.BookingChildID = item.BookingChildID;
                                yarnBookingChild.ConsumptionID = item.ConsumptionID;
                                yarnBookingChild.ItemMasterID = item.ItemMasterID;
                                yarnBookingChild.YarnTypeID = 0;
                                yarnBookingChild.YarnBrandID = item.YarnBrandID;
                                yarnBookingChild.FUPartID = item.FUPartID;
                                yarnBookingChild.BookingUnitID = item.BookingUnitID;
                                yarnBookingChild.BookingQty = item.BookingQty;
                                yarnBookingChild.FTechnicalName = "";
                                yarnBookingChild.IsCompleteReceive = item.IsCompleteReceive;
                                yarnBookingChild.LastDCDate = item.LastDCDate;
                                yarnBookingChild.ClosingRemarks = item.ClosingRemarks;

                                yarnBookingChild.GreyReqQty = item.GreyReqQty;
                                yarnBookingChild.GreyLeftOverQty = item.GreyLeftOverQty;
                                yarnBookingChild.GreyProdQty = item.GreyProdQty;
                                yarnBookingChild.FinishFabricUtilizationQty = item.FinishFabricUtilizationQty;
                                yarnBookingChild.ReqFinishFabricQty = item.ReqFinishFabricQty;

                                yarnBookingChild.QtyInKG = (Convert.ToDecimal(item.Length) *
                                     Convert.ToDecimal(item.Height) *
                                     Convert.ToDecimal(0.045) *
                                     item.BookingQty) / 420;

                                yarnBookingChild.ExcessPercentage = item.ExcessPercentage;
                                yarnBookingChild.ExcessQty = item.ExcessQty;
                                yarnBookingChild.ExcessQtyInKG = item.ExcessQtyInKG;
                                yarnBookingChild.TotalQty = item.TotalQty;
                                yarnBookingChild.TotalQtyInKG = (Convert.ToDecimal(item.Length) *
                                                Convert.ToDecimal(item.Height) *
                                                Convert.ToDecimal(0.045) *
                                                item.TotalQty) / 420;

                                yarnBookingChild.EntityState = EntityState.Added;

                                entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs.Add(yarnBookingChild);

                                childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                               .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                            }
                            else
                            {
                                entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                               .Childs.Find(x => x.BookingChildID == item.BookingChildID).EntityState = EntityState.Modified;
                            }

                            int indexChild = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                               .Childs.FindIndex(x => x.BookingChildID == item.BookingChildID);

                            item.ChildItems.Where(x => x.EntityState != EntityState.Deleted).ToList().ForEach(childItemModel =>
                            {
                                int indexCItem = childsDB.ChildItems.FindIndex(x => x.YBChildItemID == childItemModel.YBChildItemID);
                                if (indexChild > -1 && indexCItem > -1)
                                {
                                    childItemModel.EntityState = EntityState.Modified;
                                    childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems[indexCItem] = CommonFunction.DeepClone(childItemModel);
                                }
                                else
                                {
                                    YarnBookingMaster masterObj = CommonFunction.DeepClone(entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID));
                                    YarnBookingChild childObj = CommonFunction.DeepClone(masterObj.Childs[indexChild]);
                                    childItemModel.YBookingID = masterObj.YBookingID;
                                    childItemModel.YBChildID = childObj.YBChildID;
                                    childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                    childItemModel.EntityState = EntityState.Added;
                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems.Add(CommonFunction.DeepClone(childItemModel));
                                }
                            });
                        }
                    }

                    item.ChildDetails.ForEach(itemDetail =>
                    {
                        itemDetail.TechnicalNameId = obj.TechnicalNameId;
                        itemDetail.EntityState = EntityState.Modified;
                    });

                    #region Finishing Process Operations
                    var childIndexE = entity.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);
                    var childIndexM = model.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);

                    if (childIndexE > -1 && childIndexM > -1)
                    {
                        entity.FBookingChild[childIndexE].PreFinishingProcessChilds.SetUnchanged();
                        entity.FBookingChild[childIndexE].PostFinishingProcessChilds.SetUnchanged();

                        model.FBookingChild[childIndexM].PreFinishingProcessChilds.ForEach(mChildFP =>
                        {
                            int indexChildFP = entity.FBookingChild[childIndexE].PreFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                            if (indexChildFP == -1)
                            {
                                mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                mChildFP.EntityState = EntityState.Added;
                                entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Add(mChildFP);
                            }
                            else
                            {
                                mChildFP.EntityState = EntityState.Modified;
                                entity.FBookingChild[childIndexE].PreFinishingProcessChilds[indexChildFP] = mChildFP;
                            }
                        });

                        model.FBookingChild[childIndexM].PostFinishingProcessChilds.ForEach(mChildFP =>
                        {
                            int indexChildFP = entity.FBookingChild[childIndexE].PostFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                            if (indexChildFP == -1)
                            {
                                mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                mChildFP.EntityState = EntityState.Added;
                                entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Add(mChildFP);
                            }
                            else
                            {
                                mChildFP.EntityState = EntityState.Modified;
                                entity.FBookingChild[childIndexE].PostFinishingProcessChilds[indexChildFP] = mChildFP;
                            }
                        });

                        entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                        entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                    }
                    #endregion
                }

                #region FreeConcept
                FBookingAcknowledgeChild ObjEntityChild = new FBookingAcknowledgeChild();
                ObjEntityChild = CommonFunction.DeepClone(item);
                entityChilds.Add(ObjEntityChild);
                #endregion
            });

            if (!isAddition)
            {
                if (entity.YarnBookings.Count() > 0)
                {
                    entity.HasYarnBooking = true;
                }
                if (entities.Count() > 0)
                {
                    entities.ForEach(x =>
                    {
                        x.ExportOrderID = entity.ExportOrderID;
                        x.CompanyID = entity.CompanyID;
                        x.AddedBy = entity.AddedBy;
                        x.DateAdded = entity.DateAdded;
                        x.UpdatedBy = entity.UpdatedBy;
                        x.DateUpdated = entity.DateUpdated;
                    });
                }
            }

            #region BBKI Revision Operation
            if (isReviseBBKI)
            {
                entity.UserId = AppUser.UserCode;
                entity.IsReviseBBKI = true;
                entity.PreRevisionNoBBKI++;
                entity.IsUnAcknowledge = false;
                entity.IsCheckByKnittingHead = false;
                entity.IsUtilizationProposalSend = false;
                entity.IsUtilizationProposalConfirmed = false;
                entity.IsRejectByKnittingHead = false;
                entity.IsApprovedByProdHead = false;
                entity.IsRejectByProdHead = false;
                entity.IsApprovedByPMC = false;
                entity.IsRejectByPMC = false;
                entity.IsApprovedByAllowance = false;
                entity.IsRejectByAllowance = false;
                entity.RivisionReason = model.RivisionReason;
                entities.Where(x => x.FBAckID != entity.FBAckID).ToList().ForEach(x =>
                {
                    x.IsReviseBBKI = entity.IsReviseBBKI;
                    x.PreRevisionNoBBKI = entity.PreRevisionNoBBKI;
                    x.IsUnAcknowledge = false;
                    x.IsCheckByKnittingHead = false;
                    x.IsUtilizationProposalSend = false;
                    x.IsUtilizationProposalConfirmed = false;
                    x.IsRejectByKnittingHead = false;
                    x.IsApprovedByProdHead = false;
                    x.IsRejectByProdHead = false;
                    x.IsApprovedByPMC = false;
                    x.IsRejectByPMC = false;
                    x.IsApprovedByAllowance = false;
                    x.IsRejectByAllowance = false;
                    x.RivisionReason = model.RivisionReason;
                });
            }
            #endregion

            #region FreeConcept
            entity.PageName = model.PageName;

            string ColorIDs = "";
            ColorIDs = string.Join(",", entityChilds.Select(x => x.Color)).ToString();

            entity.ColorCodeList = await _service.GetAllAsyncColorIDs(ColorIDs);

            List<FreeConceptMaster> entityFreeConcepts = new List<FreeConceptMaster>();
            List<FreeConceptMRMaster> freeConceptMRList = new List<FreeConceptMRMaster>();
            string groupConceptNo = model.BookingNo;// entities.Count() > 0 ? entities[0].BookingNo : "";
            if (groupConceptNo.IsNotNullOrEmpty())
            {
                entityFreeConcepts = await _fcService.GetDatasAsync(groupConceptNo);
                entityFreeConcepts.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.ChildColors.SetUnchanged();

                    if (modelDynamic.PageName == "BulkBookingKnittingInfo")
                    {
                        List<FBookingAcknowledgeChild> fBACs = entityChilds.Where(y => y.SubGroupID == x.SubGroupID && y.ItemMasterID == x.ItemMasterID && y.ConsumptionID == x.ConsumptionID).ToList();

                        if (fBACs.IsNotNull() && fBACs.Count() > 0)
                        {
                            var obj = fBACs.First();

                            fBACs.ForEach(y =>
                            {
                                y.MachineTypeId = obj.MachineTypeId;
                                y.TechnicalNameId = obj.TechnicalNameId;
                                y.MachineGauge = obj.MachineGauge;
                                y.MachineDia = obj.MachineDia;
                                y.BrandID = obj.BrandID;
                                y.EntityState = EntityState.Modified;
                            });

                            x.MCSubClassID = obj.MachineTypeId;
                            x.TechnicalNameId = obj.TechnicalNameId;
                            x.MachineGauge = obj.MachineGauge;
                            x.MachineDia = obj.MachineDia;
                            x.BrandID = obj.BrandID;
                            x.TotalQty = obj.TotalQty;
                            x.TotalQtyInKG = obj.TotalQtyInKG;
                            x.ProduceKnittingQty = obj.GreyProdQty;
                            x.EntityState = EntityState.Modified;
                        }
                    }
                });
                if (isBDS == 2)
                {
                    freeConceptMRList = await _serviceFreeConceptMR.GetByGroupConceptNo(groupConceptNo);
                    freeConceptMRList.ForEach(x =>
                    {
                        x.EntityState = EntityState.Unchanged;
                        x.Childs.SetUnchanged();
                    });
                }
            }
            List<FreeConceptMaster> freeConceptsRevice = new List<FreeConceptMaster>();
            if (entity.BookingNo.IsNotNullOrEmpty())
            {
                freeConceptsRevice = await _service.GetFreeConcepts(entity.BookingNo);
                if (isRevised && !entity.IsUnAcknowledge)
                {
                    freeConceptsRevice.ForEach(c =>
                    {
                        c.PreProcessRevNo = entity.RevisionNo;
                        c.RevisionNo = entity.RevisionNo;
                        c.RevisionBy = AppUser.UserCode;
                        c.RevisionDate = currentDate;
                    });
                }
            }

            #endregion

            entity.FBookingChild = entityChilds;
            entity.IsInternalRevise = false;
            entities.ForEach(x => x.IsInternalRevise = false);
            if (entity.IsAllocationInternalRevise == true)
            {
                entitiesYB.ForEach(m =>
                {
                    m.IsForAllocationRevise = true;
                });
            }

            if (model.IsYarnRevision)
            {
                //List<YarnBookingChildItemRevision> yarnBookingChildItemsRevision = GetYarnRevisedChildItems(yarnBookingChildItems);
                //List<YarnBookingChild> yarnBookingChildsRevision = GetYarnRevisedChilds(yarnBookingChilds);
                //await _service.UpdateBulkStatusYarnRevision(entities, yarnBookingChildItemsRevision, new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            }
            else
            {
                //await _service.UpdateBulkStatus2(AppUser.UserCode, entities, yarnBookingChildItems, new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            }
            string result = await _service.SaveAsyncBulkWithFreeConceptWithYarnRevision(AppUser.UserCode, entity, entities, isAddition, isBDS, entityFreeConcepts, freeConceptMRList, freeConceptsRevice, isRevised, entitiesYB, isYarnRevision, model.RevisionReasonList);

            if (isAddition) entity.YBookingNo = result;

            return Ok(entity);
        }
        [Route("bulk/approveAllowance")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> BulkApproveAllowance(dynamic jsnString)
        {
            FBookingAcknowledge model = JsonConvert.DeserializeObject<FBookingAcknowledge>(
             Convert.ToString(jsnString),
             new JsonSerializerSettings
             {
                 DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
             });

            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<YarnBookingChild> yarnBookingChilds = new List<YarnBookingChild>();
            List<YarnBookingChildItem> yarnBookingChildItems = new List<YarnBookingChildItem>();
            List<FreeConceptMRChild> mrChilds = new List<FreeConceptMRChild>();

            if (model.FBAckID > 0 && model.BookingNo.IsNotNullOrEmpty())
            {
                entities = await _service.GetFBAcknowledgeMasterBulkWithRevision(model.BookingNo);
                entities.ForEach(entity =>
                {
                    entity.EntityState = EntityState.Modified;
                    if (model.IsRejectByAllowance)
                    {
                        entity.IsApprovedByAllowance = false;
                        entity.ApprovedByAllowance = 0;
                        entity.ApprovedDateAllowance = null;

                        entity.IsRejectByAllowance = true;
                        entity.RejectByAllowance = AppUser.UserCode;
                        entity.RejectDateAllowance = DateTime.Now;
                        entity.RejectReasonAllowance = model.RejectReasonAllowance;
                    }
                    else
                    {
                        entity.IsApprovedByAllowance = true;
                        entity.ApprovedByAllowance = AppUser.UserCode;
                        entity.ApprovedDateAllowance = DateTime.Now;

                        entity.IsRejectByAllowance = false;
                        entity.RejectByAllowance = 0;
                        entity.RejectDateAllowance = null;
                    }
                });

                #region Extend Model
                var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);
                yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByBookingNoWithRevision(model.BookingNo);

                var collarListModel = model.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                var cuffListModel = model.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                var yarnItemsCollar = yarnBookingChildItems.Where(x => x.SubGroupId == 11).ToList();
                var yarnItemsCuff = yarnBookingChildItems.Where(x => x.SubGroupId == 12).ToList();

                List<FBookingAcknowledgeChild> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnItemsCollar, model);
                List<FBookingAcknowledgeChild> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnItemsCuff, model);

                model.FBookingChild = model.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                model.FBookingChild.AddRange(collarList);
                model.FBookingChild.AddRange(cuffList);

                model.FBookingChild = CommonFunction.DeepClone(model.FBookingChild);

                model.ChildItems = model.ChildItems.Where(x => x.SubGroupId == 1).ToList();
                collarList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));
                cuffList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));

                //model.YChilds.Where(x => x.SubGroupId != 1).ToList().ForEach(x =>
                //{
                //    x.ChildItems = model.ChildItems.Where(y => y.YBChildID == x.YBChildID).ToList();
                //});

                #endregion

                if (model.BookingNo.IsNotNullOrEmpty() && model.ChildItems.Count() > 0)
                {
                    mrChilds = await _fcMRService.GetMRChildByBookingNoWithRevision(model.BookingNo);
                    mrChilds.SetModified();

                    string sYBChildItemIDs = string.Join(",", model.ChildItems.Select(x => x.YBChildItemID));
                    yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsWithRevision(sYBChildItemIDs);
                    model.ChildItems.ForEach(ci =>
                    {
                        var objList = yarnBookingChildItems.Where(x => x.YBChildItemID == ci.YBChildItemID).ToList();
                        if (objList.Count > 0)
                        {
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YD = ci.YD;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YDItem = ci.YDItem;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).Allowance = ci.Allowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).GreyAllowance = ci.GreyAllowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YDAllowance = ci.YDAllowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).GreyYarnUtilizationQty = ci.GreyYarnUtilizationQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).DyedYarnUtilizationQty = ci.DyedYarnUtilizationQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YarnReqQty = ci.YarnReqQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).NetYarnReqQty = ci.NetYarnReqQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YarnBalanceQty = ci.YarnBalanceQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).Remarks = ci.Remarks;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).EntityState = EntityState.Modified;

                            mrChilds = this.SetMRChildValues(mrChilds, ci);
                        }
                    });
                }

                List<YarnBookingMaster> yarnBookings = await _service.GetYarnBookingsBulkWithRevision(model.BookingNo);

                yarnBookings.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.Childs.ForEach(c =>
                    {
                        c.EntityState = EntityState.Unchanged;
                        c.ChildItems.SetUnchanged();
                    });
                });

                model.YChilds.ForEach(c =>
                {
                    YarnBookingMaster yb = yarnBookings.Find(x => x.YBookingID == c.YBookingID);

                    if (yb.IsNotNull())
                    {
                        List<YarnBookingChild> ybcs = new List<YarnBookingChild>();
                        if (c.SubGroupId == 1)
                        {
                            ybcs = yb.Childs.Where(x => x.YBChildID == c.YBChildID).ToList();
                        }
                        else
                        {
                            ybcs = yb.Childs.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color).ToList();
                        }
                        int maxChildCount = ybcs.Count();
                        int loopIndex = 0;

                        while (loopIndex < maxChildCount)
                        {
                            var ybc = ybcs[loopIndex];

                            c.ChildItems = model.ChildItems.Where(y => y.YBChildID == ybc.YBChildID).ToList();
                            if (ybc == null)
                            {
                                ybc = CommonFunction.DeepClone(c);
                                ybc.EntityState = EntityState.Added;
                            }
                            else
                            {
                                var extendedC = model.FBookingChild.Where(y => y.BookingChildID == ybc.BookingChildID).FirstOrDefault();
                                if (extendedC.IsNull()) extendedC = new FBookingAcknowledgeChild();

                                ybc.EntityState = EntityState.Modified;
                                ybc.FinishFabricUtilizationQty = extendedC.FinishFabricUtilizationQty;
                                ybc.ReqFinishFabricQty = extendedC.ReqFinishFabricQty;
                                ybc.GreyReqQty = extendedC.GreyReqQty;
                                ybc.GreyProdQty = extendedC.GreyProdQty;
                                ybc.YarnAllowance = c.YarnAllowance;
                                List<YarnBookingChildItem> bookingChildItems = new List<YarnBookingChildItem>();

                                List<YarnBookingChild> bookingChildsForItemMasterID = new List<YarnBookingChild>();
                                ybc.ChildItems = CommonFunction.DeepClone(yarnBookingChildItems.Where(m => m.YBChildID == ybc.YBChildID).ToList());
                            }

                            yarnBookingChilds.Add(ybc);
                            loopIndex++;
                        }
                    }
                });
            }
            if (model.IsYarnRevision)
            {
                List<YarnBookingChildItemRevision> yarnBookingChildItemsRevision = GetYarnRevisedChildItems(yarnBookingChildItems);
                List<YarnBookingChild> yarnBookingChildsRevision = GetYarnRevisedChilds(yarnBookingChilds);
                await _service.UpdateBulkStatusYarnRevision(AppUser.UserCode, entities, yarnBookingChildItemsRevision, new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            }
            else
            {
                await _service.UpdateBulkStatus2(AppUser.UserCode, entities, yarnBookingChildItems, new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            }
            return Ok(entities);
        }
        [Route("bulk/utilizationProposalSend")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> BulkUtilizationProposalSend(dynamic jsnString)
        {
            FBookingAcknowledge model = JsonConvert.DeserializeObject<FBookingAcknowledge>(
             Convert.ToString(jsnString),
             new JsonSerializerSettings
             {
                 DateTimeZoneHandling = DateTimeZoneHandling.Local // Ensures the date is interpreted as local time
             });
            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<YarnBookingChild> yarnBookingChilds = new List<YarnBookingChild>();
            List<YarnBookingChildItem> yarnBookingChildItems = new List<YarnBookingChildItem>();
            List<FreeConceptMRChild> mrChilds = new List<FreeConceptMRChild>();

            if (model.FBAckID > 0 && model.BookingNo.IsNotNullOrEmpty())
            {
                entities = await _service.GetFBAcknowledgeMasterBulkWithRevision(model.BookingNo);
                entities.ForEach(entity =>
                {
                    entity.EntityState = EntityState.Modified;

                    entity.IsUtilizationProposalSend = true;
                    entity.UtilizationProposalSendBy = AppUser.UserCode;
                    entity.UtilizationProposalSendDate = DateTime.Now;
                });

                #region Extend Model
                var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);
                yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByBookingNoWithRevision(model.BookingNo);

                var collarListModel = model.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                var cuffListModel = model.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                var yarnItemsCollar = yarnBookingChildItems.Where(x => x.SubGroupId == 11).ToList();
                var yarnItemsCuff = yarnBookingChildItems.Where(x => x.SubGroupId == 12).ToList();

                List<FBookingAcknowledgeChild> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnItemsCollar, model);
                List<FBookingAcknowledgeChild> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnItemsCuff, model);

                model.FBookingChild = model.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                model.FBookingChild.AddRange(collarList);
                model.FBookingChild.AddRange(cuffList);

                model.FBookingChild = CommonFunction.DeepClone(model.FBookingChild);

                model.ChildItems = model.ChildItems.Where(x => x.SubGroupId == 1).ToList();
                collarList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));
                cuffList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));

                //model.YChilds.Where(x => x.SubGroupId != 1).ToList().ForEach(x =>
                //{
                //    x.ChildItems = model.ChildItems.Where(y => y.YBChildID == x.YBChildID).ToList();
                //});
                #endregion

                if (model.BookingNo.IsNotNullOrEmpty() && model.ChildItems.Count() > 0)
                {
                    mrChilds = await _fcMRService.GetMRChildByBookingNoWithRevision(model.BookingNo);
                    mrChilds.SetModified();

                    string sYBChildItemIDs = string.Join(",", model.ChildItems.Select(x => x.YBChildItemID));
                    yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsWithRevision(sYBChildItemIDs);
                    model.ChildItems.ForEach(ci =>
                    {
                        var objList = yarnBookingChildItems.Where(x => x.YBChildItemID == ci.YBChildItemID).ToList();
                        if (objList.Count > 0)
                        {
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YD = ci.YD;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YDItem = ci.YDItem;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).Allowance = ci.Allowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).GreyAllowance = ci.GreyAllowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YDAllowance = ci.YDAllowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).GreyYarnUtilizationQty = ci.GreyYarnUtilizationQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).DyedYarnUtilizationQty = ci.DyedYarnUtilizationQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YarnReqQty = ci.YarnReqQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).NetYarnReqQty = ci.NetYarnReqQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YarnBalanceQty = ci.YarnBalanceQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).Remarks = ci.Remarks;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).EntityState = EntityState.Modified;

                            mrChilds = this.SetMRChildValues(mrChilds, ci);

                        }
                    });
                }

                List<YarnBookingMaster> yarnBookings = await _service.GetYarnBookingsBulkWithRevision(model.BookingNo);


                yarnBookings.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.Childs.ForEach(c =>
                    {
                        c.EntityState = EntityState.Unchanged;
                        c.ChildItems.SetUnchanged();
                        c.ChildItems.ForEach(CI =>
                        {
                            CI.GreyYarnUtilizationPopUpList.SetUnchanged();
                            CI.DyedYarnUtilizationPopUpList.SetUnchanged();

                        });
                        //c.FinishFabricUtilizationPopUpList.SetUnchanged();
                        //c.GreyFabricUtilizationPopUpList.SetUnchanged();
                    });
                });

                List<ItemMasterBomTemp> ItemList = new List<ItemMasterBomTemp>();
                ItemList = _yarnChildItemMasterRepository.GetItemMasterList(AppConstants.ITEM_SUB_GROUP_YARN_LIVE);

                model.YChilds.ForEach(c =>
                {
                    YarnBookingMaster yb = yarnBookings.Find(x => x.YBookingID == c.YBookingID);

                    if (yb.IsNotNull())
                    {
                        List<YarnBookingChild> ybcs = new List<YarnBookingChild>();
                        if (c.SubGroupId == 1)
                        {
                            ybcs = yb.Childs.Where(x => x.YBChildID == c.YBChildID).ToList();
                        }
                        else
                        {
                            ybcs = yb.Childs.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color).ToList();
                        }
                        int maxChildCount = ybcs.Count();
                        int loopIndex = 0;

                        //YarnBookingChild ybc = yb.Childs.Find(x => x.YBChildID == c.YBChildID);
                        while (loopIndex < maxChildCount)
                        {
                            var ybc = ybcs[loopIndex];
                            c.ChildItems = model.ChildItems.Where(y => y.YBChildID == ybc.YBChildID).ToList();
                            if (ybc == null)
                            {
                                ybc = CommonFunction.DeepClone(c);
                                ybc.EntityState = EntityState.Added;
                            }
                            else
                            {
                                var extendedC = model.FBookingChild.Where(y => y.BookingChildID == ybc.BookingChildID).FirstOrDefault();
                                if (extendedC.IsNull()) extendedC = new FBookingAcknowledgeChild();

                                ybc.EntityState = EntityState.Modified;
                                ybc.FinishFabricUtilizationQty = extendedC.FinishFabricUtilizationQty;
                                ybc.GreyLeftOverQty = extendedC.GreyLeftOverQty;
                                ybc.ReqFinishFabricQty = extendedC.ReqFinishFabricQty;
                                ybc.GreyReqQty = extendedC.GreyReqQty;
                                ybc.GreyProdQty = extendedC.GreyProdQty;
                                ybc.YarnAllowance = c.YarnAllowance;
                                List<YarnBookingChildItem> bookingChildItems = new List<YarnBookingChildItem>();

                                List<YarnBookingChild> bookingChildsForItemMasterID = new List<YarnBookingChild>();

                                c.ChildItems.ForEach(ci =>
                                {
                                    var childItem = ybc.ChildItems.Find(ybci => ybci.YBChildItemID == ci.YBChildItemID);
                                    if (childItem == null)
                                    {
                                        childItem = CommonFunction.DeepClone(ci);
                                        childItem.YBChildID = c.YBChildID;
                                        childItem.YBookingID = c.YBookingID;
                                        childItem.EntityState = EntityState.Added;
                                    }
                                    else
                                    {
                                        childItem.EntityState = EntityState.Modified;
                                    }
                                    childItem.Segment1ValueId = ci.Segment1ValueId;
                                    childItem.Segment2ValueId = ci.Segment2ValueId;
                                    childItem.Segment3ValueId = ci.Segment3ValueId;
                                    childItem.Segment4ValueId = ci.Segment4ValueId;
                                    childItem.Segment5ValueId = ci.Segment5ValueId;
                                    childItem.Segment6ValueId = ci.Segment6ValueId;
                                    childItem.Segment7ValueId = ci.Segment7ValueId;
                                    childItem.Segment8ValueId = ci.Segment8ValueId;
                                    childItem.Segment9ValueId = ci.Segment9ValueId;
                                    childItem.Segment10ValueId = ci.Segment10ValueId;

                                    childItem.Segment1ValueDesc = ci.Segment1ValueDesc;
                                    childItem.Segment2ValueDesc = ci.Segment2ValueDesc;
                                    childItem.Segment3ValueDesc = ci.Segment3ValueDesc;
                                    childItem.Segment4ValueDesc = ci.Segment4ValueDesc;
                                    childItem.Segment5ValueDesc = ci.Segment5ValueDesc;
                                    childItem.Segment6ValueDesc = ci.Segment6ValueDesc;
                                    childItem.Segment7ValueDesc = ci.Segment7ValueDesc;
                                    childItem.Segment8ValueDesc = ci.Segment8ValueDesc;
                                    childItem.Segment9ValueDesc = ci.Segment9ValueDesc;
                                    childItem.Segment10ValueDesc = ci.Segment10ValueDesc;

                                    childItem.ShadeCode = ci.ShadeCode;
                                    childItem.Allowance = ci.Allowance;
                                    childItem.GreyAllowance = ci.GreyAllowance;
                                    childItem.YDAllowance = ci.YDAllowance;
                                    childItem.GreyYarnUtilizationQty = ci.GreyYarnUtilizationQty;
                                    childItem.DyedYarnUtilizationQty = ci.DyedYarnUtilizationQty;
                                    childItem.YarnReqQty = ci.YarnReqQty;
                                    childItem.NetYarnReqQty = ci.NetYarnReqQty;
                                    childItem.YarnBalanceQty = ci.YarnBalanceQty;
                                    childItem.Distribution = ci.Distribution;
                                    childItem.BookingQty = ci.BookingQty;
                                    childItem.RequiredQty = ci.RequiredQty;
                                    childItem.Remarks = ci.Remarks;
                                    childItem.Specification = ci.Specification;
                                    childItem.YD = ci.YD;
                                    bookingChildItems.Add(childItem);
                                });

                                List<YarnBookingChildItem> childItemRecords = new List<YarnBookingChildItem>();
                                childItemRecords = bookingChildItems;
                                _yarnChildItemMasterRepository.GenerateItemWithYItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref ItemList, ref childItemRecords);

                                bookingChildItems.ForEach(x =>
                                {
                                    var objItem = childItemRecords.Find(y => y.Segment1ValueId == x.Segment1ValueId && y.Segment2ValueId == x.Segment2ValueId
                                                                && y.Segment3ValueId == x.Segment3ValueId && y.Segment4ValueId == x.Segment4ValueId
                                                                && y.Segment5ValueId == x.Segment5ValueId && y.Segment6ValueId == x.Segment6ValueId
                                                                && y.Segment7ValueId == x.Segment7ValueId && y.Segment8ValueId == x.Segment8ValueId
                                                                && y.Segment9ValueId == x.Segment9ValueId && y.Segment10ValueId == x.Segment10ValueId);

                                    x.ItemMasterID = objItem.ItemMasterID;
                                    x.YItemMasterID = objItem.ItemMasterID;
                                    x.BookingChildID = c.BookingChildID;
                                    x.ConsumptionID = c.ConsumptionID;
                                    x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);

                                    mrChilds = this.SetMRChildValues(mrChilds, x);
                                });

                                ybc.ChildItems.Where(cii => cii.EntityState == EntityState.Unchanged).ToList().ForEach(cii =>
                                {
                                    cii.EntityState = EntityState.Deleted;
                                    bookingChildItems.Add(cii);

                                    mrChilds = this.SetMRChildValues(mrChilds, cii);
                                });
                                ybc.ChildItems = CommonFunction.DeepClone(bookingChildItems);
                            }

                            if (yb.SubGroupID == 1)
                            {
                                ybc.FinishFabricUtilizationPopUpList = this.GetUtilizationPopUpList(ybc, c, yb.Childs, yb.SubGroupID);
                                ybc.GreyFabricUtilizationPopUpList = this.GetGreyUtilizationPopUpList(ybc, c, yb.Childs, yb.SubGroupID);
                            }

                            //ybc.FinishFabricUtilizationPopUpList = this.GetUtilizationPopUpList(ybc, c, yb.Childs, yb.SubGroupID);
                            //ybc.GreyFabricUtilizationPopUpList = this.GetGreyUtilizationPopUpList(ybc, c, yb.Childs, yb.SubGroupID);

                            if (yb.SubGroupID == 1)
                            {
                                List<BulkBookingGreyYarnUtilization> greyYarnUTListSave = new List<BulkBookingGreyYarnUtilization>();
                                List<BulkBookingDyedYarnUtilization> dyedYarnUTListSave = new List<BulkBookingDyedYarnUtilization>();
                                c.ChildItems.ForEach(ci =>
                                {
                                    List<BulkBookingGreyYarnUtilization> greyYarnUtilizationListFromDB = new List<BulkBookingGreyYarnUtilization>();
                                    List<BulkBookingDyedYarnUtilization> dyedYarnUtilizationListFromDB = new List<BulkBookingDyedYarnUtilization>();
                                    ybc.ChildItems.ForEach(gy =>
                                    {

                                        var greyYarnUtilizationList = gy.GreyYarnUtilizationPopUpList.Where(xgy => xgy.YBChildItemID == ci.YBChildItemID).ToList();
                                        if (greyYarnUtilizationList.Count > 0)
                                        {
                                            greyYarnUtilizationListFromDB.AddRange(CommonFunction.DeepClone(greyYarnUtilizationList));
                                        }

                                        var dyedYarnUtilizationList = gy.DyedYarnUtilizationPopUpList.Where(xgy => xgy.YBChildItemID == ci.YBChildItemID).ToList();
                                        if (dyedYarnUtilizationList.Count > 0)
                                        {
                                            dyedYarnUtilizationListFromDB.AddRange(CommonFunction.DeepClone(dyedYarnUtilizationList));
                                        }


                                    });
                                    greyYarnUtilizationListFromDB.SetUnchanged();
                                    List<BulkBookingGreyYarnUtilization> tempGreyYarnUTList = new List<BulkBookingGreyYarnUtilization>();

                                    tempGreyYarnUTList = this.GetGreyYarnUtilizationPopUpList(greyYarnUtilizationListFromDB, ci, yb.SubGroupID, ybc.YBChildID, yb.YBookingID, ci.YBChildItemID);
                                    greyYarnUTListSave.AddRange(CommonFunction.DeepClone(tempGreyYarnUTList));

                                    dyedYarnUtilizationListFromDB.SetUnchanged();
                                    List<BulkBookingDyedYarnUtilization> tempDyedYarnUTList = new List<BulkBookingDyedYarnUtilization>();
                                    tempDyedYarnUTList = this.GetDyedYarnUtilizationPopUpList(dyedYarnUtilizationListFromDB, ci, yb.SubGroupID, ybc.YBChildID, yb.YBookingID, ci.YBChildItemID);

                                    dyedYarnUTListSave.AddRange(CommonFunction.DeepClone(tempDyedYarnUTList));
                                });


                                ybc.ChildItems.ForEach(gy =>
                                {
                                    var greyYarnUtilizationList = greyYarnUTListSave.Where(xgy => xgy.YBChildItemID == gy.YBChildItemID).ToList();
                                    if (greyYarnUtilizationList.Count > 0)
                                        gy.GreyYarnUtilizationPopUpList = CommonFunction.DeepClone(greyYarnUtilizationList);


                                    var dyedYarnUtilizationList = dyedYarnUTListSave.Where(xgy => xgy.YBChildItemID == gy.YBChildItemID).ToList();
                                    if (dyedYarnUtilizationList.Count > 0)
                                        gy.DyedYarnUtilizationPopUpList = CommonFunction.DeepClone(dyedYarnUtilizationList);

                                });

                            }
                            else
                            {
                                ybc.ChildItems.ForEach(gy =>
                                {
                                    //var bb = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).ChildItems.Where(x => x.YBChildItemID == gy.YBChildItemID).ToList());
                                    //bb.ForEach(ci =>
                                    //{
                                    //    gy.GreyYarnUtilizationPopUpList = ci.GreyYarnUtilizationPopUpList;
                                    //    gy.DyedYarnUtilizationPopUpList = ci.DyedYarnUtilizationPopUpList;

                                    //});
                                    var bb = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).ChildItems.Where(x => x.YBChildItemID == gy.YBChildItemID).FirstOrDefault());
                                    if (bb != null)
                                    {
                                        bb.GreyYarnUtilizationPopUpList.ForEach(gyupup =>
                                        {
                                            var greyYU = gy.GreyYarnUtilizationPopUpList.Find(ybci => ybci.BBGreyYarnUtilizationID == gyupup.BBGreyYarnUtilizationID);
                                            if (greyYU == null)
                                            {
                                                greyYU = CommonFunction.DeepClone(gyupup);
                                                greyYU.YBChildItemID = gy.YBChildItemID;
                                                greyYU.EntityState = EntityState.Added;
                                                gy.GreyYarnUtilizationPopUpList.Add(greyYU);
                                            }
                                            else
                                            {
                                                greyYU.EntityState = EntityState.Modified;
                                                greyYU.YarnStockSetID = gyupup.YarnStockSetID;
                                                greyYU.UtilizationSampleStock = gyupup.UtilizationSampleStock;
                                                greyYU.UtilizationLeftoverStock = gyupup.UtilizationLeftoverStock;
                                                greyYU.UtilizationLiabilitiesStock = gyupup.UtilizationLiabilitiesStock;
                                                greyYU.UtilizationUnusableStock = gyupup.UtilizationUnusableStock;
                                                greyYU.TotalUtilization = gyupup.TotalUtilization;
                                                greyYU.UpdatedBy = AppUser.UserCode;
                                                greyYU.DateUpdated = DateTime.Now;
                                            }
                                        });

                                        gy.GreyYarnUtilizationPopUpList.Where(u => u.EntityState == EntityState.Unchanged).SetDeleted();

                                        bb.DyedYarnUtilizationPopUpList.ForEach(gyupup =>
                                        {
                                            var greyYU = gy.DyedYarnUtilizationPopUpList.Find(ybci => ybci.BBDyedYarnUtilizationID == gyupup.BBDyedYarnUtilizationID);
                                            if (greyYU == null)
                                            {
                                                greyYU = CommonFunction.DeepClone(gyupup);
                                                greyYU.YBChildItemID = gy.YBChildItemID;
                                                greyYU.EntityState = EntityState.Added;
                                                gy.DyedYarnUtilizationPopUpList.Add(greyYU);
                                            }
                                            else
                                            {
                                                greyYU.EntityState = EntityState.Modified;
                                                greyYU.ExportOrderID = gyupup.ExportOrderID;
                                                greyYU.BuyerID = gyupup.BuyerID;
                                                greyYU.PhysicalCount = gyupup.PhysicalCount;
                                                greyYU.ColorID = gyupup.ColorID;
                                                greyYU.ColorName = gyupup.ColorName;
                                                greyYU.DyedYarnUtilizationQty = gyupup.DyedYarnUtilizationQty;
                                                greyYU.UpdatedBy = AppUser.UserCode;
                                                greyYU.DateUpdated = DateTime.Now;
                                            }
                                        });

                                        gy.DyedYarnUtilizationPopUpList.Where(u => u.EntityState == EntityState.Unchanged).SetDeleted();
                                    }
                                });

                            }
                            if (yb.SubGroupID != 1)
                            {
                                ybc.FinishFabricUtilizationPopUpList = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).FinishFabricUtilizationPopUpList);
                                ybc.GreyFabricUtilizationPopUpList = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).GreyFabricUtilizationPopUpList);
                            }

                            yarnBookingChilds.Add(ybc);
                            loopIndex++;
                        }
                    }
                });
            }

            if (model.IsYarnRevision)
            {
                List<YarnBookingChildItemRevision> yarnBookingChildItemsRevision = GetYarnRevisedChildItems(yarnBookingChildItems);
                List<YarnBookingChild> yarnBookingChildsRevision = GetYarnRevisedChilds(yarnBookingChilds);
                await _service.UpdateBulkStatusYarnRevision(AppUser.UserCode, entities, yarnBookingChildItemsRevision, new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            }
            else
            {
                await _service.UpdateBulkStatus2(AppUser.UserCode, entities, yarnBookingChildItems, new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            }

            //await _service.UpdateBulkStatus2(entities, yarnBookingChildItems, new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            return Ok(entities);
        }
        private List<BulkBookingFinishFabricUtilization> GetUtilizationPopUpList(YarnBookingChild yarnBookingChildFromDB, YarnBookingChild yarnBookingChildList, List<YarnBookingChild> ybChildList, int SubGroupID)
        {
            yarnBookingChildFromDB.FinishFabricUtilizationPopUpList.SetUnchanged();

            //int BBFFUtilizationID = 1;

            //BBFFUtilizationID = yarnBookingChildFromDB.FinishFabricUtilizationPopUpList.Count() > 0 ? yarnBookingChildFromDB.FinishFabricUtilizationPopUpList.Max(x => x.BBFFUtilizationID) + 1 : 1;
            //yarnBookingChildList.FinishFabricUtilizationPopUpList.ForEach(im =>
            //{
            //    //if (im.EntityState == EntityState.Added)
            //    im.BBFFUtilizationID = BBFFUtilizationID++;
            //});

            if (SubGroupID == 1)
            {
                yarnBookingChildList.FinishFabricUtilizationPopUpList.ForEach(ci =>
                {
                    var fFUtilizationList = yarnBookingChildFromDB.FinishFabricUtilizationPopUpList.Find(ybci => ybci.YBChildID == ci.YBChildID && ybci.ItemMasterID == ci.ItemMasterID && ybci.WeightSheetNo == ci.WeightSheetNo && ybci.ExportOrderID == ci.ExportOrderID);

                    if (fFUtilizationList == null)
                    {
                        fFUtilizationList = CommonFunction.DeepClone(ci);
                        fFUtilizationList.YBChildID = yarnBookingChildList.YBChildID;
                        fFUtilizationList.EntityState = EntityState.Added;
                        fFUtilizationList.AddedBy = AppUser.UserCode;
                        fFUtilizationList.DateAdded = DateTime.Now;
                        yarnBookingChildFromDB.FinishFabricUtilizationPopUpList.Add(CommonFunction.DeepClone(fFUtilizationList));
                    }
                    else
                    {
                        fFUtilizationList.EntityState = EntityState.Modified;
                        fFUtilizationList.UpdatedBy = AppUser.UserCode;
                        fFUtilizationList.DateUpdated = DateTime.Now;
                    }
                    fFUtilizationList.ExportOrderID = ci.ExportOrderID;
                    fFUtilizationList.ItemMasterID = ci.ItemMasterID;
                    fFUtilizationList.GSM = ci.GSM;
                    fFUtilizationList.ColorID = ci.ColorID;
                    fFUtilizationList.BuyerID = ci.BuyerID;
                    fFUtilizationList.Width = ci.Width;
                    fFUtilizationList.BatchNo = ci.BatchNo;
                    fFUtilizationList.GSMID = ci.GSMID;
                    fFUtilizationList.CompositionID = ci.CompositionID;
                    fFUtilizationList.WeightSheetNo = ci.WeightSheetNo;
                    fFUtilizationList.SubGroupID = ci.SubGroupID;
                    fFUtilizationList.FinishFabricUtilizationQTYinkg = ci.FinishFabricUtilizationQTYinkg;
                    fFUtilizationList.FinishFabricExcessQtyKg = ci.FinishFabricExcessQtyKg;
                    fFUtilizationList.FinishFabricRejectQtyKg = ci.FinishFabricRejectQtyKg;
                    fFUtilizationList.FinishFabricBookingQtyDecreasedbyMerchantQtyKg = ci.FinishFabricBookingQtyDecreasedbyMerchantQtyKg;
                    fFUtilizationList.FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg = ci.FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg;
                });
            }

            #region off
            //else
            //{
            //    ybChildList.ForEach(ybc =>
            //    {
            //        yarnBookingChildList.FinishFabricUtilizationPopUpList.ForEach(ci =>
            //        {

            //            //var nTotalBookingQTY = yarnBookingChildList.BookingQty;
            //            //var nYBCBookingQTY = ybc.BookingQty;
            //            //var nBookingPercent = (nYBCBookingQTY * 100) / nTotalBookingQTY;

            //            //var nFFUtilizationQTY = (ci.FinishFabricUtilizationQTYinkg / 100) * nBookingPercent;

            //            var fFUtilizationList = yarnBookingChildFromDB.FinishFabricUtilizationPopUpList.Find(ybci => ybci.ItemMasterID == ci.ItemMasterID && ybci.WeightSheetNo == ci.WeightSheetNo && ybci.ExportOrderID == ci.ExportOrderID && ybci.YBChildID == ci.YBChildID);

            //            if (fFUtilizationList == null)
            //            {
            //                fFUtilizationList = CommonFunction.DeepClone(ci);
            //                fFUtilizationList.YBChildID = ybc.YBChildID;
            //                fFUtilizationList.EntityState = EntityState.Added;
            //                yarnBookingChildFromDB.FinishFabricUtilizationPopUpList.Add(CommonFunction.DeepClone(fFUtilizationList));
            //            }
            //            else
            //            {
            //                fFUtilizationList.EntityState = EntityState.Modified;
            //            }
            //            fFUtilizationList.ExportOrderID = ci.ExportOrderID;
            //            fFUtilizationList.ItemMasterID = ci.ItemMasterID;
            //            fFUtilizationList.GSM = ci.GSM;
            //            fFUtilizationList.ColorID = ci.ColorID;
            //            fFUtilizationList.BuyerID = ci.BuyerID;
            //            fFUtilizationList.Width = ci.Width;
            //            fFUtilizationList.BatchNo = ci.BatchNo;
            //            fFUtilizationList.GSMID = ci.GSMID;
            //            fFUtilizationList.CompositionID = ci.CompositionID;
            //            fFUtilizationList.WeightSheetNo = ci.WeightSheetNo;
            //            fFUtilizationList.SubGroupID = ci.SubGroupID;
            //            fFUtilizationList.FinishFabricUtilizationQTYinkg = ci.FinishFabricUtilizationQTYinkg;

            //        });


            //    });

            //}
            #endregion
            yarnBookingChildFromDB.FinishFabricUtilizationPopUpList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            return yarnBookingChildFromDB.FinishFabricUtilizationPopUpList;
        }
        private List<FBookingAcknowledgeChildGFUtilization> GetGreyUtilizationPopUpList(YarnBookingChild yarnBookingChildFromDB, YarnBookingChild yarnBookingChildList, List<YarnBookingChild> ybChildList, int SubGroupID)
        {
            yarnBookingChildFromDB.GreyFabricUtilizationPopUpList.SetUnchanged();

            //int GFUtilizationID = 1;

            //GFUtilizationID = yarnBookingChildFromDB.GreyFabricUtilizationPopUpList.Count() > 0 ? yarnBookingChildFromDB.GreyFabricUtilizationPopUpList.Max(x => x.GFUtilizationID) + 1 : 1;
            //yarnBookingChildList.GreyFabricUtilizationPopUpList.ForEach(im =>
            //{
            //    im.GFUtilizationID = GFUtilizationID++;
            //});

            yarnBookingChildList.GreyFabricUtilizationPopUpList.ForEach(ci =>
            {
                if (SubGroupID == 1)
                {
                    var gFUtilizationList = yarnBookingChildFromDB.GreyFabricUtilizationPopUpList.Find(ybci => ybci.YBChildID == ci.YBChildID && ybci.GFUtilizationID == ci.GFUtilizationID);

                    if (gFUtilizationList == null)
                    {
                        gFUtilizationList = CommonFunction.DeepClone(ci);
                        gFUtilizationList.YBChildID = yarnBookingChildList.YBChildID;
                        gFUtilizationList.EntityState = EntityState.Added;
                        yarnBookingChildFromDB.GreyFabricUtilizationPopUpList.Add(CommonFunction.DeepClone(gFUtilizationList));
                    }
                    else
                    {
                        gFUtilizationList.EntityState = EntityState.Modified;
                    }
                    gFUtilizationList.ExportOrderID = ci.ExportOrderID;
                    gFUtilizationList.ItemMasterID = ci.ItemMasterID;
                    gFUtilizationList.SubGroupID = ci.SubGroupID;
                    gFUtilizationList.GSMID = ci.GSMID;
                    gFUtilizationList.ColorID = ci.ColorID;
                    gFUtilizationList.BuyerID = ci.BuyerID;
                    gFUtilizationList.FabricTypeID = ci.FabricTypeID;
                    gFUtilizationList.CompositionID = ci.CompositionID;
                    gFUtilizationList.GSM = ci.GSM;
                    gFUtilizationList.FabricStyle = ci.FabricStyle;
                    gFUtilizationList.GreyFabricUtilizationQTYinkg = ci.GreyFabricUtilizationQTYinkg;



                }

                #region Off
                //else
                //{
                //    ybChildList.ForEach(ybc =>
                //    {

                //        var nTotalBookingQTY = yarnBookingChildList.BookingQty;
                //        var nYBCBookingQTY = ybc.BookingQty;
                //        var nBookingPercent = (nYBCBookingQTY * 100) / nTotalBookingQTY;

                //        var nGFUtilizationQTY = (ci.GreyFabricUtilizationQTYinkg / 100) * nBookingPercent;

                //        //ybci.YBChildID == ybc.YBChildID
                //        var gFUtilizationList = yarnBookingChildFromDB.GreyFabricUtilizationPopUpList.Find(ybci => ybci.ExportOrderID == ci.ExportOrderID && ybci.ItemMasterID == ci.ItemMasterID && ybci.SubGroupID == ci.SubGroupID);

                //        if (gFUtilizationList == null)
                //        {
                //            gFUtilizationList = CommonFunction.DeepClone(ci);
                //            gFUtilizationList.YBChildID = ybc.YBChildID;
                //            gFUtilizationList.EntityState = EntityState.Added;
                //            yarnBookingChildFromDB.GreyFabricUtilizationPopUpList.Add(CommonFunction.DeepClone(gFUtilizationList));
                //        }
                //        else
                //        {
                //            gFUtilizationList.EntityState = EntityState.Modified;
                //        }
                //        gFUtilizationList.ExportOrderID = ci.ExportOrderID;
                //        gFUtilizationList.ItemMasterID = ci.ItemMasterID;
                //        gFUtilizationList.SubGroupID = ci.SubGroupID;
                //        gFUtilizationList.GSMID = ci.GSMID;
                //        gFUtilizationList.ColorID = ci.ColorID;
                //        gFUtilizationList.BuyerID = ci.BuyerID;
                //        gFUtilizationList.FabricTypeID = ci.FabricTypeID;
                //        gFUtilizationList.CompositionID = ci.CompositionID;
                //        gFUtilizationList.GSM = ci.GSM;
                //        gFUtilizationList.FabricStyle = ci.FabricStyle;
                //        gFUtilizationList.GreyFabricUtilizationQTYinkg = ci.GreyFabricUtilizationQTYinkg;



                //    });

                //}

                #endregion

            });



            yarnBookingChildFromDB.GreyFabricUtilizationPopUpList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            return yarnBookingChildFromDB.GreyFabricUtilizationPopUpList;
        }

        private List<BulkBookingGreyYarnUtilization> GetGreyYarnUtilizationPopUpList(List<BulkBookingGreyYarnUtilization> greyYarnUtilizationListFromDB, YarnBookingChildItem yarnBookingChildItem, int SubGroupID, int YBChildID, int YBookingID, int YBChildItemID)
        {
            // model List Grey Yarn utilization
            yarnBookingChildItem.GreyYarnUtilizationPopUpList.ForEach(item =>
            {

                var greyYarnUtilizationList = greyYarnUtilizationListFromDB.Find(x => x.YBChildID == item.YBChildID && x.YarnStockSetID == item.YarnStockSetID && x.YBChildItemID == item.YBChildItemID);

                if (greyYarnUtilizationList == null)
                {
                    greyYarnUtilizationList = CommonFunction.DeepClone(item);
                    greyYarnUtilizationList.EntityState = EntityState.Added;
                    greyYarnUtilizationList.AddedBy = AppUser.UserCode;
                    greyYarnUtilizationList.DateAdded = DateTime.Now;
                    greyYarnUtilizationListFromDB.Add(CommonFunction.DeepClone(greyYarnUtilizationList));
                }
                else
                {
                    greyYarnUtilizationList.EntityState = EntityState.Modified;
                }

                greyYarnUtilizationList.BBGreyYarnUtilizationID = item.BBGreyYarnUtilizationID;
                greyYarnUtilizationList.YBChildItemID = YBChildItemID;
                greyYarnUtilizationList.YBookingID = YBookingID;
                greyYarnUtilizationList.YBChildID = YBChildID;
                greyYarnUtilizationList.SubGroupID = SubGroupID;
                greyYarnUtilizationList.YarnStockSetID = item.YarnStockSetID;
                greyYarnUtilizationList.UtilizationSampleStock = item.UtilizationSampleStock;
                greyYarnUtilizationList.UtilizationLiabilitiesStock = item.UtilizationLiabilitiesStock;
                greyYarnUtilizationList.UtilizationUnusableStock = item.UtilizationUnusableStock;
                greyYarnUtilizationList.UtilizationLeftoverStock = item.UtilizationLeftoverStock;
                greyYarnUtilizationList.TotalUtilization = item.UtilizationSampleStock + item.UtilizationLiabilitiesStock + item.UtilizationUnusableStock + item.UtilizationLeftoverStock;

            });


            greyYarnUtilizationListFromDB.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();


            return greyYarnUtilizationListFromDB;

        }
        private List<BulkBookingDyedYarnUtilization> GetDyedYarnUtilizationPopUpList(List<BulkBookingDyedYarnUtilization> dyedYarnUtilizationListFromDB, YarnBookingChildItem yarnBookingChildItem, int SubGroupID, int YBChildID, int YBookingID, int YBChildItemID)
        {
            // model List Grey Yarn utilization
            yarnBookingChildItem.DyedYarnUtilizationPopUpList.ForEach(item =>
            {

                var dyedYarnUtilizationList = dyedYarnUtilizationListFromDB.Find(x => x.YBChildID == item.YBChildID && x.ColorID == item.ColorID && x.YBChildItemID == item.YBChildItemID);

                if (dyedYarnUtilizationList == null)
                {
                    dyedYarnUtilizationList = CommonFunction.DeepClone(item);
                    dyedYarnUtilizationList.EntityState = EntityState.Added;
                    dyedYarnUtilizationList.AddedBy = AppUser.UserCode;
                    dyedYarnUtilizationList.DateAdded = DateTime.Now;
                    dyedYarnUtilizationListFromDB.Add(CommonFunction.DeepClone(dyedYarnUtilizationList));
                }
                else
                {
                    dyedYarnUtilizationList.EntityState = EntityState.Modified;
                }

                dyedYarnUtilizationList.BBDyedYarnUtilizationID = item.BBDyedYarnUtilizationID;
                dyedYarnUtilizationList.YBChildItemID = YBChildItemID;
                dyedYarnUtilizationList.YBookingID = YBookingID;
                dyedYarnUtilizationList.YBChildID = YBChildID;
                dyedYarnUtilizationList.SubGroupID = SubGroupID;
                dyedYarnUtilizationList.ColorName = item.ColorName;
                dyedYarnUtilizationList.ColorID = item.ColorID;
                dyedYarnUtilizationList.BuyerID = item.BuyerID;
                dyedYarnUtilizationList.ExportOrderID = item.ExportOrderID;
                dyedYarnUtilizationList.PhysicalCount = item.PhysicalCount;
                dyedYarnUtilizationList.DyedYarnUtilizationQty = item.DyedYarnUtilizationQty;

            });



            dyedYarnUtilizationListFromDB.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();

            return dyedYarnUtilizationListFromDB;

        }
        private List<BulkBookingFinishFabricUtilization> GetAdditionUtilizationPopUpList(FBookingAcknowledgeChild fBookingChildFromDB, FBookingAcknowledgeChild fBookingChildList)
        {
            fBookingChildFromDB.FinishFabricUtilizationPopUpList.SetUnchanged();

            int BBFFUtilizationID = 1;

            //if (SubGroupID == 1)
            //{
            fBookingChildList.FinishFabricUtilizationPopUpList.ForEach(ci =>
            {
                var fFUtilizationList = fBookingChildFromDB.FinishFabricUtilizationPopUpList.Find(ybci => ybci.BBFFUtilizationID == ci.BBFFUtilizationID);

                if (fFUtilizationList == null)
                {
                    fFUtilizationList = CommonFunction.DeepClone(ci);
                    fFUtilizationList.BBFFUtilizationID = BBFFUtilizationID++;
                    fFUtilizationList.BookingChildID = fBookingChildList.BookingChildID;
                    fFUtilizationList.EntityState = EntityState.Added;
                    fBookingChildFromDB.FinishFabricUtilizationPopUpList.Add(CommonFunction.DeepClone(fFUtilizationList));
                }
                else
                {
                    fFUtilizationList.EntityState = EntityState.Modified;
                }
                fFUtilizationList.ExportOrderID = ci.ExportOrderID;
                fFUtilizationList.ItemMasterID = ci.ItemMasterID;
                fFUtilizationList.GSM = ci.GSM;
                fFUtilizationList.ColorID = ci.ColorID;
                fFUtilizationList.BuyerID = ci.BuyerID;
                fFUtilizationList.Width = ci.Width;
                fFUtilizationList.BatchNo = ci.BatchNo;
                fFUtilizationList.GSMID = ci.GSMID;
                fFUtilizationList.CompositionID = ci.CompositionID;
                fFUtilizationList.WeightSheetNo = ci.WeightSheetNo;
                fFUtilizationList.SubGroupID = ci.SubGroupID;
                fFUtilizationList.FinishFabricUtilizationQTYinkg = ci.FinishFabricUtilizationQTYinkg;
            });
            //}

            fBookingChildFromDB.FinishFabricUtilizationPopUpList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            return fBookingChildFromDB.FinishFabricUtilizationPopUpList;
        }
        private List<FBookingAcknowledgeChildGFUtilization> GetAdditionGreyUtilizationPopUpList(FBookingAcknowledgeChild fBookingChildFromDB, FBookingAcknowledgeChild fBookingChildList)
        {
            fBookingChildFromDB.GreyFabricUtilizationPopUpList.SetUnchanged();

            int GFUtilizationID = 1;

            fBookingChildList.GreyFabricUtilizationPopUpList.ForEach(ci =>
            {
                //if (SubGroupID == 1)
                //{
                var gFUtilizationList = fBookingChildFromDB.GreyFabricUtilizationPopUpList.Find(ybci => ybci.GFUtilizationID == ci.GFUtilizationID);

                if (gFUtilizationList == null)
                {
                    gFUtilizationList = CommonFunction.DeepClone(ci);
                    gFUtilizationList.GFUtilizationID = GFUtilizationID++;
                    gFUtilizationList.BookingChildID = fBookingChildList.BookingChildID;
                    gFUtilizationList.EntityState = EntityState.Added;
                    fBookingChildFromDB.GreyFabricUtilizationPopUpList.Add(CommonFunction.DeepClone(gFUtilizationList));
                }
                else
                {
                    gFUtilizationList.EntityState = EntityState.Modified;
                }
                gFUtilizationList.ExportOrderID = ci.ExportOrderID;
                gFUtilizationList.ItemMasterID = ci.ItemMasterID;
                gFUtilizationList.SubGroupID = ci.SubGroupID;
                gFUtilizationList.GSMID = ci.GSMID;
                gFUtilizationList.ColorID = ci.ColorID;
                gFUtilizationList.BuyerID = ci.BuyerID;
                gFUtilizationList.FabricTypeID = ci.FabricTypeID;
                gFUtilizationList.CompositionID = ci.CompositionID;
                gFUtilizationList.GSM = ci.GSM;
                gFUtilizationList.FabricStyle = ci.FabricStyle;
                gFUtilizationList.GreyFabricUtilizationQTYinkg = ci.GreyFabricUtilizationQTYinkg;



                //}

            });



            //yarnBookingChildFromDB.GreyFabricUtilizationPopUpList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            return fBookingChildFromDB.GreyFabricUtilizationPopUpList;
        }
        private List<FBookingAcknowledgeChildReplacement> GetAdditionReplacementPopUpList(FBookingAcknowledgeChild fBookingChildFromDB, FBookingAcknowledgeChild fBookingChildList)
        {
            fBookingChildFromDB.AdditionalReplacementPOPUPList.SetUnchanged();

            int ReplacementID = 1;

            fBookingChildList.AdditionalReplacementPOPUPList.ForEach(ci =>
            {
                var ReplacementPOPU = fBookingChildFromDB.AdditionalReplacementPOPUPList.Find(ybci => ybci.ReplacementID == ci.ReplacementID);

                if (ReplacementPOPU == null)
                {
                    ReplacementPOPU = CommonFunction.DeepClone(ci);
                    ReplacementPOPU.ReplacementID = ReplacementID++;
                    ReplacementPOPU.BookingChildID = fBookingChildList.BookingChildID;
                    ReplacementPOPU.EntityState = EntityState.Added;
                    ReplacementPOPU.AddedBy = AppUser.UserCode;
                    ReplacementPOPU.DateAdded = DateTime.Now;
                    fBookingChildFromDB.AdditionalReplacementPOPUPList.Add(CommonFunction.DeepClone(ReplacementPOPU));
                }
                else
                {
                    ReplacementPOPU.EntityState = EntityState.Modified;
                    ReplacementPOPU.UpdatedBy = AppUser.UserCode;
                    ReplacementPOPU.DateUpdated = DateTime.Now;
                }
                ReplacementPOPU.ReasonID = ci.ReasonID;
                ReplacementPOPU.DepertmentID = ci.DepertmentID;
                ReplacementPOPU.Remarks = ci.Remarks;
                ReplacementPOPU.ReplacementQTY = ci.ReplacementQTY;
            });


            fBookingChildFromDB.AdditionalReplacementPOPUPList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            return fBookingChildFromDB.AdditionalReplacementPOPUPList;
        }
        private List<FBookingAcknowledgeChildReplacement> GetAdditionReplacementPopUpListYBC(YarnBookingChild yarnBookingChildFromDB, FBookingAcknowledgeChild fBookingChildList)
        {
            yarnBookingChildFromDB.AdditionalReplacementPOPUPList.SetUnchanged();

            int ReplacementID = 1;

            fBookingChildList.AdditionalReplacementPOPUPList.ForEach(ci =>
            {
                var ReplacementPOPU = yarnBookingChildFromDB.AdditionalReplacementPOPUPList.Find(ybci => ybci.ReplacementID == ci.ReplacementID);

                if (ReplacementPOPU == null)
                {
                    ReplacementPOPU = CommonFunction.DeepClone(ci);
                    ReplacementPOPU.ReplacementID = ReplacementID++;
                    ReplacementPOPU.BookingChildID = fBookingChildList.BookingChildID;
                    ReplacementPOPU.EntityState = EntityState.Added;
                    ReplacementPOPU.AddedBy = AppUser.UserCode;
                    ReplacementPOPU.DateAdded = DateTime.Now;
                    yarnBookingChildFromDB.AdditionalReplacementPOPUPList.Add(CommonFunction.DeepClone(ReplacementPOPU));
                }
                else
                {
                    ReplacementPOPU.EntityState = EntityState.Modified;
                    ReplacementPOPU.UpdatedBy = AppUser.UserCode;
                    ReplacementPOPU.DateUpdated = DateTime.Now;
                }
                ReplacementPOPU.ReasonID = ci.ReasonID;
                ReplacementPOPU.DepertmentID = ci.DepertmentID;
                ReplacementPOPU.Remarks = ci.Remarks;
                ReplacementPOPU.ReplacementQTY = ci.ReplacementQTY;
            });


            yarnBookingChildFromDB.AdditionalReplacementPOPUPList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            return yarnBookingChildFromDB.AdditionalReplacementPOPUPList;
        }

        private List<FBookingAcknowledgeChildItemNetReqQTY> GetAdditionChildItemNetReqQTYPopUpList(YarnBookingChildItem yarnBookingChildItemFromDB, YarnBookingChildItem yarnBookingChildItemList)
        {
            yarnBookingChildItemFromDB.AdditionalNetReqPOPUPList.SetUnchanged();

            int ReplacementID = 1;

            yarnBookingChildItemList.AdditionalNetReqPOPUPList.ForEach(ci =>
            {
                var ReplacementPOPU = yarnBookingChildItemFromDB.AdditionalNetReqPOPUPList.Find(ybci => ybci.ReplacementID == ci.ReplacementID);

                if (ReplacementPOPU == null)
                {
                    ReplacementPOPU = CommonFunction.DeepClone(ci);
                    ReplacementPOPU.ReplacementID = ReplacementID++;
                    ReplacementPOPU.YBChildItemID = yarnBookingChildItemList.YBChildItemID;
                    ReplacementPOPU.EntityState = EntityState.Added;
                    ReplacementPOPU.AddedBy = AppUser.UserCode;
                    ReplacementPOPU.DateAdded = DateTime.Now;
                    yarnBookingChildItemFromDB.AdditionalNetReqPOPUPList.Add(CommonFunction.DeepClone(ReplacementPOPU));
                }
                else
                {
                    ReplacementPOPU.EntityState = EntityState.Modified;
                    ReplacementPOPU.UpdatedBy = AppUser.UserCode;
                    ReplacementPOPU.DateUpdated = DateTime.Now;
                }
                ReplacementPOPU.ReasonID = ci.ReasonID;
                ReplacementPOPU.DepertmentID = ci.DepertmentID;
                ReplacementPOPU.Remarks = ci.Remarks;
                ReplacementPOPU.ReplacementQTY = ci.ReplacementQTY;
            });


            yarnBookingChildItemFromDB.AdditionalNetReqPOPUPList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            return yarnBookingChildItemFromDB.AdditionalNetReqPOPUPList;
        }

        private List<BulkBookingGreyYarnUtilization> GetAdditionGreyYarnUtilizationPopUpList(List<BulkBookingGreyYarnUtilization> greyYarnUtilizationListFromDB, YarnBookingChildItem yarnBookingChildItem, int SubGroupID)
        {
            // model List Grey Yarn utilization
            greyYarnUtilizationListFromDB.SetUnchanged();

            int BBGreyYarnUtilizationID = 1;
            yarnBookingChildItem.GreyYarnUtilizationPopUpList.ForEach(item =>
            {

                var greyYarnUtilizationList = greyYarnUtilizationListFromDB.Find(x => x.BBGreyYarnUtilizationID == item.BBGreyYarnUtilizationID);

                if (greyYarnUtilizationList == null)
                {
                    greyYarnUtilizationList = CommonFunction.DeepClone(item);
                    greyYarnUtilizationList.EntityState = EntityState.Added;
                    greyYarnUtilizationList.BBGreyYarnUtilizationID = BBGreyYarnUtilizationID++;
                    greyYarnUtilizationList.AddedBy = AppUser.UserCode;
                    greyYarnUtilizationList.DateAdded = DateTime.Now;
                    greyYarnUtilizationListFromDB.Add(CommonFunction.DeepClone(greyYarnUtilizationList));
                }
                else
                {
                    greyYarnUtilizationList.EntityState = EntityState.Modified;

                    greyYarnUtilizationList.BBGreyYarnUtilizationID = item.BBGreyYarnUtilizationID;
                    greyYarnUtilizationList.YBChildItemID = item.YBChildItemID;
                    greyYarnUtilizationList.YBookingID = item.YBookingID;
                    greyYarnUtilizationList.YBChildID = item.YBChildID;
                    greyYarnUtilizationList.SubGroupID = SubGroupID;
                    greyYarnUtilizationList.YarnStockSetID = item.YarnStockSetID;
                    greyYarnUtilizationList.UtilizationSampleStock = item.UtilizationSampleStock;
                    greyYarnUtilizationList.UtilizationLiabilitiesStock = item.UtilizationLiabilitiesStock;
                    greyYarnUtilizationList.UtilizationUnusableStock = item.UtilizationUnusableStock;
                    greyYarnUtilizationList.UtilizationLeftoverStock = item.UtilizationLeftoverStock;
                    greyYarnUtilizationList.TotalUtilization = item.UtilizationSampleStock + item.UtilizationLiabilitiesStock + item.UtilizationUnusableStock + item.UtilizationLeftoverStock;
                }
            });


            greyYarnUtilizationListFromDB.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();


            return greyYarnUtilizationListFromDB;

        }
        private List<BulkBookingDyedYarnUtilization> GetAdditionDyedYarnUtilizationPopUpList(List<BulkBookingDyedYarnUtilization> dyedYarnUtilizationListFromDB, YarnBookingChildItem yarnBookingChildItem, int SubGroupID)
        {
            dyedYarnUtilizationListFromDB.SetUnchanged();
            // model List Grey Yarn utilization
            int BBDyedYarnUtilizationID = 1;
            yarnBookingChildItem.DyedYarnUtilizationPopUpList.ForEach(item =>
            {

                var dyedYarnUtilizationList = dyedYarnUtilizationListFromDB.Find(x => x.BBDyedYarnUtilizationID == item.BBDyedYarnUtilizationID);

                if (dyedYarnUtilizationList == null)
                {
                    dyedYarnUtilizationList = CommonFunction.DeepClone(item);
                    dyedYarnUtilizationList.EntityState = EntityState.Added;
                    dyedYarnUtilizationList.AddedBy = AppUser.UserCode;
                    dyedYarnUtilizationList.DateAdded = DateTime.Now;
                    dyedYarnUtilizationList.BBDyedYarnUtilizationID = BBDyedYarnUtilizationID++;
                    dyedYarnUtilizationListFromDB.Add(CommonFunction.DeepClone(dyedYarnUtilizationList));
                }
                else
                {
                    dyedYarnUtilizationList.EntityState = EntityState.Modified;
                    dyedYarnUtilizationList.BBDyedYarnUtilizationID = item.BBDyedYarnUtilizationID;
                    dyedYarnUtilizationList.YBChildItemID = item.YBChildItemID;
                    dyedYarnUtilizationList.YBookingID = item.YBookingID;
                    dyedYarnUtilizationList.YBChildID = item.YBChildID;
                    dyedYarnUtilizationList.SubGroupID = SubGroupID;
                    dyedYarnUtilizationList.ColorName = item.ColorName;
                    dyedYarnUtilizationList.ColorID = item.ColorID;
                    dyedYarnUtilizationList.BuyerID = item.BuyerID;
                    dyedYarnUtilizationList.ExportOrderID = item.ExportOrderID;
                    dyedYarnUtilizationList.PhysicalCount = item.PhysicalCount;
                    dyedYarnUtilizationList.DyedYarnUtilizationQty = item.DyedYarnUtilizationQty;
                }

            });



            dyedYarnUtilizationListFromDB.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();

            return dyedYarnUtilizationListFromDB;

        }
        private List<BulkBookingFinishFabricUtilization> GetAdditionUtilizationPopUpListYBC(YarnBookingChild fBookingChildFromDB, FBookingAcknowledgeChild fBookingChildList)
        {
            fBookingChildFromDB.FinishFabricUtilizationPopUpList.SetUnchanged();

            int BBFFUtilizationID = 1;

            //if (SubGroupID == 1)
            //{
            fBookingChildList.FinishFabricUtilizationPopUpList.ForEach(ci =>
            {
                var fFUtilizationList = fBookingChildFromDB.FinishFabricUtilizationPopUpList.Find(ybci => ybci.BBFFUtilizationID == ci.BBFFUtilizationID);

                if (fFUtilizationList == null)
                {
                    fFUtilizationList = CommonFunction.DeepClone(ci);
                    fFUtilizationList.BBFFUtilizationID = BBFFUtilizationID++;
                    fFUtilizationList.BookingChildID = fBookingChildList.BookingChildID;
                    fFUtilizationList.EntityState = EntityState.Added;
                    fBookingChildFromDB.FinishFabricUtilizationPopUpList.Add(CommonFunction.DeepClone(fFUtilizationList));
                }
                else
                {
                    fFUtilizationList.EntityState = EntityState.Modified;
                }
                fFUtilizationList.ExportOrderID = ci.ExportOrderID;
                fFUtilizationList.ItemMasterID = ci.ItemMasterID;
                fFUtilizationList.GSM = ci.GSM;
                fFUtilizationList.ColorID = ci.ColorID;
                fFUtilizationList.BuyerID = ci.BuyerID;
                fFUtilizationList.Width = ci.Width;
                fFUtilizationList.BatchNo = ci.BatchNo;
                fFUtilizationList.GSMID = ci.GSMID;
                fFUtilizationList.CompositionID = ci.CompositionID;
                fFUtilizationList.WeightSheetNo = ci.WeightSheetNo;
                fFUtilizationList.SubGroupID = ci.SubGroupID;
                fFUtilizationList.FinishFabricUtilizationQTYinkg = ci.FinishFabricUtilizationQTYinkg;
            });
            //}

            fBookingChildFromDB.FinishFabricUtilizationPopUpList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            return fBookingChildFromDB.FinishFabricUtilizationPopUpList;
        }
        private List<FBookingAcknowledgeChildGFUtilization> GetAdditionGreyUtilizationPopUpListYBC(YarnBookingChild fBookingChildFromDB, FBookingAcknowledgeChild fBookingChildList)
        {
            fBookingChildFromDB.GreyFabricUtilizationPopUpList.SetUnchanged();

            int GFUtilizationID = 1;

            fBookingChildList.GreyFabricUtilizationPopUpList.ForEach(ci =>
            {
                //if (SubGroupID == 1)
                //{
                var gFUtilizationList = fBookingChildFromDB.GreyFabricUtilizationPopUpList.Find(ybci => ybci.GFUtilizationID == ci.GFUtilizationID);

                if (gFUtilizationList == null)
                {
                    gFUtilizationList = CommonFunction.DeepClone(ci);
                    gFUtilizationList.GFUtilizationID = GFUtilizationID++;
                    gFUtilizationList.BookingChildID = fBookingChildList.BookingChildID;
                    gFUtilizationList.EntityState = EntityState.Added;
                    fBookingChildFromDB.GreyFabricUtilizationPopUpList.Add(CommonFunction.DeepClone(gFUtilizationList));
                }
                else
                {
                    gFUtilizationList.EntityState = EntityState.Modified;
                }
                gFUtilizationList.ExportOrderID = ci.ExportOrderID;
                gFUtilizationList.ItemMasterID = ci.ItemMasterID;
                gFUtilizationList.SubGroupID = ci.SubGroupID;
                gFUtilizationList.GSMID = ci.GSMID;
                gFUtilizationList.ColorID = ci.ColorID;
                gFUtilizationList.BuyerID = ci.BuyerID;
                gFUtilizationList.FabricTypeID = ci.FabricTypeID;
                gFUtilizationList.CompositionID = ci.CompositionID;
                gFUtilizationList.GSM = ci.GSM;
                gFUtilizationList.FabricStyle = ci.FabricStyle;
                gFUtilizationList.GreyFabricUtilizationQTYinkg = ci.GreyFabricUtilizationQTYinkg;



                //}

            });



            //yarnBookingChildFromDB.GreyFabricUtilizationPopUpList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            return fBookingChildFromDB.GreyFabricUtilizationPopUpList;
        }
        private List<FreeConceptMRChild> SetMRChildValues(List<FreeConceptMRChild> mrChilds, YarnBookingChildItem ybci)
        {
            var mrChild = mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID);
            if (mrChild.IsNotNull())
            {
                if (ybci.EntityState == EntityState.Deleted)
                {
                    mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID).EntityState = EntityState.Deleted;
                    return mrChilds;
                }
                mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID).ItemMasterID = ybci.YItemMasterID;
                mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID).YarnCategory = ybci.YarnCategory;
                mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID).YD = ybci.YD;
                mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID).YDItem = ybci.YDItem;
                mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID).Remarks = ybci.Remarks;
                mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID).ShadeCode = ybci.ShadeCode;
                mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID).Allowance = ybci.Allowance;
                mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID).BookingQty = ybci.BookingQty;
                mrChilds.Find(x => x.YBChildItemID == ybci.YBChildItemID).Distribution = ybci.Distribution;
            }
            return mrChilds;
        }
        [Route("bulk/utilizationProposalConfirmed")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> BulkUtilizationProposalConfirmed(FBookingAcknowledge model)
        {
            bool isRevisedYarn = model.IsRevisedYarn;

            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<YarnBookingMaster> yarnBookingMasters = new List<YarnBookingMaster>();
            List<YarnBookingChild> yarnBookingChilds = new List<YarnBookingChild>();
            List<YarnBookingChildItem> yarnBookingChildItems = new List<YarnBookingChildItem>();
            List<FreeConceptMRChild> mrChilds = new List<FreeConceptMRChild>();

            if (model.FBAckID > 0 && model.BookingNo.IsNotNullOrEmpty())
            {
                entities = await _service.GetFBAcknowledgeMasterBulkWithRevision(model.BookingNo);
                entities.SetModified();

                if (!isRevisedYarn)
                {
                    entities.ForEach(entity =>
                    {
                        entity.IsUtilizationProposalConfirmed = true;
                        entity.UtilizationProposalConfirmedBy = AppUser.UserCode;
                        entity.UtilizationProposalConfirmedDate = DateTime.Now;
                    });
                }

                #region Extend Model
                var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);
                yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByBookingNoWithRevision(model.BookingNo);

                var collarListModel = model.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                var cuffListModel = model.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                var yarnItemsCollar = yarnBookingChildItems.Where(x => x.SubGroupId == 11).ToList();
                var yarnItemsCuff = yarnBookingChildItems.Where(x => x.SubGroupId == 12).ToList();

                List<FBookingAcknowledgeChild> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnItemsCollar, model);
                List<FBookingAcknowledgeChild> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnItemsCuff, model);

                model.FBookingChild = model.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                model.FBookingChild.AddRange(collarList);
                model.FBookingChild.AddRange(cuffList);

                model.FBookingChild = CommonFunction.DeepClone(model.FBookingChild);

                model.ChildItems = model.ChildItems.Where(x => x.SubGroupId == 1).ToList();
                collarList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));
                cuffList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));

                //model.YChilds.Where(x => x.SubGroupId != 1).ToList().ForEach(x =>
                //{
                //    x.ChildItems = model.ChildItems.Where(y => y.YBChildID == x.YBChildID).ToList();
                //});


                #endregion

                if (model.BookingNo.IsNotNullOrEmpty() && model.ChildItems.Count() > 0)
                {
                    mrChilds = await _fcMRService.GetMRChildByBookingNoWithRevision(model.BookingNo);
                    mrChilds.SetModified();

                    string sYBChildItemIDs = string.Join(",", model.ChildItems.Select(x => x.YBChildItemID));
                    yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsWithRevision(sYBChildItemIDs);

                    model.ChildItems.ForEach(ci =>
                    {
                        var yarnObj = yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID);
                        if (yarnObj.IsNotNull())
                        {
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YD = ci.YD;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YDItem = ci.YDItem;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).Allowance = ci.Allowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).GreyAllowance = ci.GreyAllowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YDAllowance = ci.YDAllowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).GreyYarnUtilizationQty = ci.GreyYarnUtilizationQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).DyedYarnUtilizationQty = ci.DyedYarnUtilizationQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YarnReqQty = ci.YarnReqQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).NetYarnReqQty = ci.NetYarnReqQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YarnBalanceQty = ci.YarnBalanceQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).Remarks = ci.Remarks;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).EntityState = EntityState.Modified;

                            mrChilds = this.SetMRChildValues(mrChilds, ci);
                        }
                    });
                }

                List<YarnBookingMaster> yarnBookings = await _service.GetYarnBookingsBulkWithRevision(model.BookingNo);

                yarnBookings.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    if (isRevisedYarn)
                    {
                        x.IsRevised = true;
                        x.RevisedBy = AppUser.UserCode;
                        x.RevisedDate = DateTime.Now;
                        x.EntityState = EntityState.Modified;

                        x.PreProcessRevNo = x.PreProcessRevNo + 1;
                    }

                    x.Childs.ForEach(c =>
                    {
                        c.EntityState = EntityState.Unchanged;
                        c.ChildItems.SetUnchanged();
                        c.ChildItems.ForEach(CI =>
                        {
                            CI.GreyYarnUtilizationPopUpList.SetUnchanged();
                            CI.DyedYarnUtilizationPopUpList.SetUnchanged();

                        });
                        //c.FinishFabricUtilizationPopUpList.SetUnchanged();
                        //c.GreyFabricUtilizationPopUpList.SetUnchanged();
                    });
                });

                if (isRevisedYarn)
                {
                    yarnBookingMasters = yarnBookings;

                    entities.ForEach(x =>
                    {
                        x.IsCheckByKnittingHead = false;
                        x.IsRejectByAllowance = false;
                        x.IsRejectByKnittingHead = false;
                        x.IsRejectByPMC = false;
                        x.IsRejectByProdHead = false;
                        x.IsApprovedByAllowance = false;
                        x.IsApprovedByPMC = false;
                        x.IsApprovedByProdHead = false;
                        x.IsUtilizationProposalConfirmed = false;
                        x.IsUtilizationProposalSend = false;
                    });
                }

                List<ItemMasterBomTemp> ItemList = new List<ItemMasterBomTemp>();
                ItemList = _yarnChildItemMasterRepository.GetItemMasterList(AppConstants.ITEM_SUB_GROUP_YARN_LIVE);

                model.YChilds.ForEach(c =>
                {
                    YarnBookingMaster yb = yarnBookings.Find(x => x.YBookingID == c.YBookingID);

                    if (yb.IsNotNull())
                    {
                        List<YarnBookingChild> ybcs = new List<YarnBookingChild>();
                        if (c.SubGroupId == 1)
                        {
                            ybcs = yb.Childs.Where(x => x.YBChildID == c.YBChildID).ToList();
                        }
                        else
                        {
                            ybcs = yb.Childs.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color).ToList();
                        }
                        int maxChildCount = ybcs.Count();
                        int loopIndex = 0;

                        //YarnBookingChild ybc = yb.Childs.Find(x => x.YBChildID == c.YBChildID);
                        while (loopIndex < maxChildCount)
                        {
                            var ybc = ybcs[loopIndex];
                            c.ChildItems = model.ChildItems.Where(y => y.YBChildID == ybc.YBChildID).ToList();
                            if (ybc == null)
                            {
                                ybc = CommonFunction.DeepClone(c);
                                ybc.EntityState = EntityState.Added;
                            }
                            else
                            {
                                var extendedC = model.FBookingChild.Where(y => y.BookingChildID == ybc.BookingChildID).FirstOrDefault();
                                if (extendedC.IsNull()) extendedC = new FBookingAcknowledgeChild();

                                ybc.EntityState = EntityState.Modified;
                                ybc.FinishFabricUtilizationQty = extendedC.FinishFabricUtilizationQty;
                                ybc.ReqFinishFabricQty = extendedC.ReqFinishFabricQty;
                                ybc.GreyReqQty = extendedC.GreyReqQty;
                                ybc.GreyProdQty = extendedC.GreyProdQty;
                                ybc.YarnAllowance = c.YarnAllowance;
                                List<YarnBookingChildItem> bookingChildItems = new List<YarnBookingChildItem>();

                                List<YarnBookingChild> bookingChildsForItemMasterID = new List<YarnBookingChild>();

                                c.ChildItems.ForEach(ci =>
                                {
                                    var childItem = ybc.ChildItems.Find(ybci => ybci.YBChildItemID == ci.YBChildItemID);
                                    if (childItem == null)
                                    {
                                        childItem = CommonFunction.DeepClone(ci);
                                        childItem.YBChildID = c.YBChildID;
                                        childItem.YBookingID = c.YBookingID;
                                        childItem.EntityState = EntityState.Added;
                                    }
                                    else
                                    {
                                        childItem.EntityState = EntityState.Modified;
                                    }
                                    childItem.Segment1ValueId = ci.Segment1ValueId;
                                    childItem.Segment2ValueId = ci.Segment2ValueId;
                                    childItem.Segment3ValueId = ci.Segment3ValueId;
                                    childItem.Segment4ValueId = ci.Segment4ValueId;
                                    childItem.Segment5ValueId = ci.Segment5ValueId;
                                    childItem.Segment6ValueId = ci.Segment6ValueId;
                                    childItem.Segment7ValueId = ci.Segment7ValueId;
                                    childItem.Segment8ValueId = ci.Segment8ValueId;
                                    childItem.Segment9ValueId = ci.Segment9ValueId;
                                    childItem.Segment10ValueId = ci.Segment10ValueId;

                                    childItem.Segment1ValueDesc = ci.Segment1ValueDesc;
                                    childItem.Segment2ValueDesc = ci.Segment2ValueDesc;
                                    childItem.Segment3ValueDesc = ci.Segment3ValueDesc;
                                    childItem.Segment4ValueDesc = ci.Segment4ValueDesc;
                                    childItem.Segment5ValueDesc = ci.Segment5ValueDesc;
                                    childItem.Segment6ValueDesc = ci.Segment6ValueDesc;
                                    childItem.Segment7ValueDesc = ci.Segment7ValueDesc;
                                    childItem.Segment8ValueDesc = ci.Segment8ValueDesc;
                                    childItem.Segment9ValueDesc = ci.Segment9ValueDesc;
                                    childItem.Segment10ValueDesc = ci.Segment10ValueDesc;

                                    childItem.ShadeCode = ci.ShadeCode;
                                    childItem.Allowance = ci.Allowance;
                                    childItem.GreyAllowance = ci.GreyAllowance;
                                    childItem.YDAllowance = ci.YDAllowance;
                                    childItem.GreyYarnUtilizationQty = ci.GreyYarnUtilizationQty;
                                    childItem.DyedYarnUtilizationQty = ci.DyedYarnUtilizationQty;
                                    childItem.YarnReqQty = ci.YarnReqQty;
                                    childItem.NetYarnReqQty = ci.NetYarnReqQty;
                                    childItem.YarnBalanceQty = ci.YarnBalanceQty;
                                    childItem.Distribution = ci.Distribution;
                                    childItem.BookingQty = ci.BookingQty;
                                    childItem.RequiredQty = ci.RequiredQty;
                                    childItem.Remarks = ci.Remarks;
                                    childItem.Specification = ci.Specification;
                                    childItem.YD = ci.YD;

                                    bookingChildItems.Add(childItem);
                                });
                                List<YarnBookingChildItem> childItemRecords = new List<YarnBookingChildItem>();
                                childItemRecords = bookingChildItems;
                                _yarnChildItemMasterRepository.GenerateItemWithYItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref ItemList, ref childItemRecords);

                                bookingChildItems.ForEach(x =>
                                {
                                    var objItem = childItemRecords.Find(y => y.Segment1ValueId == x.Segment1ValueId && y.Segment2ValueId == x.Segment2ValueId
                                                                && y.Segment3ValueId == x.Segment3ValueId && y.Segment4ValueId == x.Segment4ValueId
                                                                && y.Segment5ValueId == x.Segment5ValueId && y.Segment6ValueId == x.Segment6ValueId
                                                                && y.Segment7ValueId == x.Segment7ValueId && y.Segment8ValueId == x.Segment8ValueId
                                                                && y.Segment9ValueId == x.Segment9ValueId && y.Segment10ValueId == x.Segment10ValueId);

                                    x.ItemMasterID = objItem.ItemMasterID;
                                    x.YItemMasterID = objItem.ItemMasterID;
                                    x.BookingChildID = c.BookingChildID;
                                    x.ConsumptionID = c.ConsumptionID;
                                    x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);
                                });

                                ybc.ChildItems.Where(cii => cii.EntityState == EntityState.Unchanged).ToList().ForEach(cii =>
                                {
                                    cii.EntityState = EntityState.Deleted;
                                    bookingChildItems.Add(cii);

                                    mrChilds = this.SetMRChildValues(mrChilds, cii);
                                });
                                ybc.ChildItems = CommonFunction.DeepClone(bookingChildItems);
                            }
                            if (yb.SubGroupID == 1)
                            {
                                ybc.FinishFabricUtilizationPopUpList = this.GetUtilizationPopUpList(ybc, c, yb.Childs, yb.SubGroupID);
                                ybc.GreyFabricUtilizationPopUpList = this.GetGreyUtilizationPopUpList(ybc, c, yb.Childs, yb.SubGroupID);
                            }
                            List<BulkBookingGreyYarnUtilization> greyYarnUTListSave = new List<BulkBookingGreyYarnUtilization>();
                            List<BulkBookingDyedYarnUtilization> dyedYarnUTListSave = new List<BulkBookingDyedYarnUtilization>();
                            /*
                            c.ChildItems.ForEach(ci =>
                            {
                                List<BulkBookingGreyYarnUtilization> greyYarnUtilizationListFromDB = new List<BulkBookingGreyYarnUtilization>();
                                List<BulkBookingDyedYarnUtilization> dyedYarnUtilizationListFromDB = new List<BulkBookingDyedYarnUtilization>();
                                ybc.ChildItems.ForEach(gy =>
                                {

                                    var greyYarnUtilizationList = gy.GreyYarnUtilizationPopUpList.Where(xgy => xgy.YBChildItemID == ci.YBChildItemID).ToList();
                                    if (greyYarnUtilizationList.Count > 0)
                                    {
                                        greyYarnUtilizationListFromDB.AddRange(CommonFunction.DeepClone(greyYarnUtilizationList));
                                    }
                                    var dyedYarnUtilizationList = gy.DyedYarnUtilizationPopUpList.Where(xgy => xgy.YBChildItemID == ci.YBChildItemID).ToList();
                                    if (dyedYarnUtilizationList.Count > 0)
                                    {
                                        dyedYarnUtilizationListFromDB.AddRange(CommonFunction.DeepClone(dyedYarnUtilizationList));
                                    }


                                });
                                greyYarnUtilizationListFromDB.SetUnchanged();
                                List<BulkBookingGreyYarnUtilization> tempGreyYarnUTList = new List<BulkBookingGreyYarnUtilization>();

                                tempGreyYarnUTList = this.GetGreyYarnUtilizationPopUpList(greyYarnUtilizationListFromDB, ci, yb.SubGroupID, ybc.YBChildID, yb.YBookingID, ci.YBChildItemID);
                                greyYarnUTListSave.AddRange(CommonFunction.DeepClone(tempGreyYarnUTList));

                                dyedYarnUtilizationListFromDB.SetUnchanged();
                                List<BulkBookingDyedYarnUtilization> tempDyedYarnUTList = new List<BulkBookingDyedYarnUtilization>();

                                tempDyedYarnUTList = this.GetDyedYarnUtilizationPopUpList(dyedYarnUtilizationListFromDB, ci, yb.SubGroupID, ybc.YBChildID, yb.YBookingID, ci.YBChildItemID);
                                dyedYarnUTListSave.AddRange(CommonFunction.DeepClone(tempDyedYarnUTList));

                            });
                            */

                            ybc.ChildItems.ForEach(gy =>
                            {
                                /*var greyYarnUtilizationList = greyYarnUTListSave.Where(xgy => xgy.YBChildItemID == gy.YBChildItemID).ToList();
                                if (greyYarnUtilizationList.Count > 0)
                                    gy.GreyYarnUtilizationPopUpList = CommonFunction.DeepClone(greyYarnUtilizationList);

                                var dyedYarnUtilizationList = dyedYarnUTListSave.Where(xgy => xgy.YBChildItemID == gy.YBChildItemID).ToList();
                                if (dyedYarnUtilizationList.Count > 0)
                                    gy.DyedYarnUtilizationPopUpList = CommonFunction.DeepClone(dyedYarnUtilizationList);
                               */
                                var fbc = CommonFunction.DeepClone(model.FBookingChild.Where(x => x.BookingChildID == ybc.BookingChildID).FirstOrDefault());
                                if (fbc != null)
                                {
                                    var bb = CommonFunction.DeepClone(fbc.ChildItems.Where(x => x.YBChildItemID == gy.YBChildItemID).FirstOrDefault());
                                    //var bb = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).ChildItems.Where(x => x.YBChildItemID == gy.YBChildItemID).FirstOrDefault());
                                    if (bb != null)
                                    {
                                        bb.GreyYarnUtilizationPopUpList.ForEach(gyupup =>
                                        {
                                            var greyYU = gy.GreyYarnUtilizationPopUpList.Find(ybci => ybci.BBGreyYarnUtilizationID == gyupup.BBGreyYarnUtilizationID);
                                            if (greyYU == null)
                                            {
                                                greyYU = CommonFunction.DeepClone(gyupup);
                                                greyYU.YBChildItemID = gy.YBChildItemID;
                                                greyYU.EntityState = EntityState.Added;
                                                gy.GreyYarnUtilizationPopUpList.Add(greyYU);
                                            }
                                            else
                                            {
                                                greyYU.EntityState = EntityState.Modified;
                                                greyYU.YarnStockSetID = gyupup.YarnStockSetID;
                                                greyYU.UtilizationSampleStock = gyupup.UtilizationSampleStock;
                                                greyYU.UtilizationLeftoverStock = gyupup.UtilizationLeftoverStock;
                                                greyYU.UtilizationLiabilitiesStock = gyupup.UtilizationLiabilitiesStock;
                                                greyYU.UtilizationUnusableStock = gyupup.UtilizationUnusableStock;
                                                greyYU.TotalUtilization = gyupup.TotalUtilization;
                                                greyYU.UpdatedBy = AppUser.UserCode;
                                                greyYU.DateUpdated = DateTime.Now;
                                            }
                                        });

                                        gy.GreyYarnUtilizationPopUpList.Where(u => u.EntityState == EntityState.Unchanged).SetDeleted();

                                        bb.DyedYarnUtilizationPopUpList.ForEach(gyupup =>
                                        {
                                            var greyYU = gy.DyedYarnUtilizationPopUpList.Find(ybci => ybci.BBDyedYarnUtilizationID == gyupup.BBDyedYarnUtilizationID);
                                            if (greyYU == null)
                                            {
                                                greyYU = CommonFunction.DeepClone(gyupup);
                                                greyYU.YBChildItemID = gy.YBChildItemID;
                                                greyYU.EntityState = EntityState.Added;
                                                gy.DyedYarnUtilizationPopUpList.Add(greyYU);
                                            }
                                            else
                                            {
                                                greyYU.EntityState = EntityState.Modified;
                                                greyYU.ExportOrderID = gyupup.ExportOrderID;
                                                greyYU.BuyerID = gyupup.BuyerID;
                                                greyYU.PhysicalCount = gyupup.PhysicalCount;
                                                greyYU.ColorID = gyupup.ColorID;
                                                greyYU.ColorName = gyupup.ColorName;
                                                greyYU.DyedYarnUtilizationQty = gyupup.DyedYarnUtilizationQty;
                                                greyYU.UpdatedBy = AppUser.UserCode;
                                                greyYU.DateUpdated = DateTime.Now;
                                            }
                                        });

                                        gy.DyedYarnUtilizationPopUpList.Where(u => u.EntityState == EntityState.Unchanged).SetDeleted();

                                    }
                                }
                            });



                            if (yb.SubGroupID != 1)
                            {
                                var fbc = CommonFunction.DeepClone(model.FBookingChild.Where(x => x.BookingChildID == ybc.BookingChildID).FirstOrDefault());
                                if (fbc != null)
                                {
                                    ybc.FinishFabricUtilizationPopUpList = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).FinishFabricUtilizationPopUpList);
                                    ybc.GreyFabricUtilizationPopUpList = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).GreyFabricUtilizationPopUpList);
                                }
                            }
                            yarnBookingChilds.Add(ybc);
                            loopIndex++;
                        }
                    }
                });
            }
            if (model.IsYarnRevision)
            {
                List<YarnBookingChildItemRevision> yarnBookingChildItemsRevision = GetYarnRevisedChildItems(yarnBookingChildItems);
                List<YarnBookingChild> yarnBookingChildsRevision = GetYarnRevisedChilds(yarnBookingChilds);
                await _service.UpdateBulkStatusYarnRevision(AppUser.UserCode, entities, yarnBookingChildItemsRevision, isRevisedYarn ? yarnBookingMasters : new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            }
            else
            {
                await _service.UpdateBulkStatus2(AppUser.UserCode, entities, yarnBookingChildItems, isRevisedYarn ? yarnBookingMasters : new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            }
            //await _service.UpdateBulkStatus2(entities, yarnBookingChildItems, isRevisedYarn ? yarnBookingMasters : new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            return Ok(entities);
        }
        [Route("bulk/YarnBookingRevision")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> YarnBookingRevision(FBookingAcknowledge model)
        {
            bool isRevisedYarn = model.IsRevisedYarn;

            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<YarnBookingMaster> yarnBookingMasters = new List<YarnBookingMaster>();
            List<YarnBookingChild> yarnBookingChilds = new List<YarnBookingChild>();
            List<YarnBookingChildItem> yarnBookingChildItems = new List<YarnBookingChildItem>();
            List<YarnBookingChildItemRevision> yarnBookingChildItemsRevision = new List<YarnBookingChildItemRevision>();
            List<FreeConceptMRChild> mrChilds = new List<FreeConceptMRChild>();

            if (model.FBAckID > 0 && model.BookingNo.IsNotNullOrEmpty())
            {
                entities = await _service.GetFBAcknowledgeMasterBulk(model.BookingNo);
                entities.SetModified();

                #region Extend Model
                var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);
                yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByBookingNo(model.BookingNo);

                var collarListModel = model.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                var cuffListModel = model.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                var yarnItemsCollar = yarnBookingChildItems.Where(x => x.SubGroupId == 11).ToList();
                var yarnItemsCuff = yarnBookingChildItems.Where(x => x.SubGroupId == 12).ToList();

                List<FBookingAcknowledgeChild> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnItemsCollar, model);
                List<FBookingAcknowledgeChild> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnItemsCuff, model);

                model.FBookingChild = model.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                model.FBookingChild.AddRange(collarList);
                model.FBookingChild.AddRange(cuffList);

                model.FBookingChild = CommonFunction.DeepClone(model.FBookingChild);

                model.ChildItems = model.ChildItems.Where(x => x.SubGroupId == 1).ToList();
                collarList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));
                cuffList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));

                //model.YChilds.Where(x => x.SubGroupId != 1).ToList().ForEach(x =>
                //{
                //    x.ChildItems = model.ChildItems.Where(y => y.YBChildID == x.YBChildID).ToList();
                //});

                #endregion

                if (model.BookingNo.IsNotNullOrEmpty() && model.ChildItems.Count() > 0)
                {
                    mrChilds = await _fcMRService.GetMRChildByBookingNo(model.BookingNo);
                    mrChilds.SetModified();

                    string sYBChildItemIDs = string.Join(",", model.ChildItems.Select(x => x.YBChildItemID));
                    yarnBookingChildItems = await _serviceYB.GetYanBookingChildItems(sYBChildItemIDs);

                    model.ChildItems.ForEach(ci =>
                    {
                        var yarnObj = yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID);
                        if (yarnObj.IsNotNull())
                        {
                            //yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).ItemMasterID = ci.YItemMasterID;
                            //yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YItemMasterID = ci.YItemMasterID;
                            //yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YarnCategory = ci.YarnCategory;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YD = ci.YD;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YDItem = ci.YDItem;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).Allowance = ci.Allowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).GreyAllowance = ci.GreyAllowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YDAllowance = ci.YDAllowance;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).GreyYarnUtilizationQty = ci.GreyYarnUtilizationQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).DyedYarnUtilizationQty = ci.DyedYarnUtilizationQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YarnReqQty = ci.YarnReqQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).NetYarnReqQty = ci.NetYarnReqQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YarnBalanceQty = ci.YarnBalanceQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).Remarks = ci.Remarks;
                            //yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).RequiredQty = ci.RequiredQty;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).EntityState = EntityState.Modified;

                            mrChilds = this.SetMRChildValues(mrChilds, ci);
                        }
                    });
                }

                List<YarnBookingMaster> yarnBookings = await _service.GetYarnBookingsBulk(model.BookingNo);
                //--
                yarnBookings.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    if (isRevisedYarn)
                    {
                        x.IsRevised = true;
                        x.RevisedBy = AppUser.UserCode;
                        x.RevisedDate = DateTime.Now;
                        x.UnAcknowledge = false;
                        x.EntityState = EntityState.Modified;

                        x.PreProcessRevNo = x.PreProcessRevNo + 1;

                    }

                    x.Childs.ForEach(c =>
                    {
                        c.EntityState = EntityState.Unchanged;
                        c.ChildItems.SetUnchanged();
                    });
                });

                if (isRevisedYarn)
                {
                    yarnBookingMasters = yarnBookings;

                    entities.ForEach(x =>
                    {
                        x.IsCheckByKnittingHead = false;
                        x.IsRejectByAllowance = false;
                        x.IsRejectByKnittingHead = false;
                        x.IsRejectByPMC = false;
                        x.IsRejectByProdHead = false;
                        x.IsApprovedByAllowance = false;
                        x.IsApprovedByPMC = false;
                        x.IsApprovedByProdHead = false;
                        x.IsUtilizationProposalConfirmed = false;
                        x.IsUtilizationProposalSend = false;
                    });
                }

                List<ItemMasterBomTemp> ItemList = new List<ItemMasterBomTemp>();
                ItemList = _yarnChildItemMasterRepository.GetItemMasterList(AppConstants.ITEM_SUB_GROUP_YARN_LIVE);

                model.YChilds.ForEach(c =>
                {
                    YarnBookingMaster yb = yarnBookings.Find(x => x.YBookingID == c.YBookingID);

                    if (yb.IsNotNull())
                    {
                        List<YarnBookingChild> ybcs = new List<YarnBookingChild>();
                        if (c.SubGroupId == 1)
                        {
                            ybcs = yb.Childs.Where(x => x.YBChildID == c.YBChildID).ToList();
                        }
                        else
                        {
                            ybcs = yb.Childs.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color).ToList();
                        }
                        int maxChildCount = ybcs.Count();
                        int loopIndex = 0;

                        while (loopIndex < maxChildCount)
                        {
                            var ybc = ybcs[loopIndex];
                            c.ChildItems = model.ChildItems.Where(y => y.YBChildID == ybc.YBChildID).ToList();
                            if (ybc == null)
                            {
                                ybc = CommonFunction.DeepClone(c);
                                ybc.EntityState = EntityState.Added;
                            }
                            else
                            {
                                ybc.EntityState = EntityState.Modified;
                                ybc.FinishFabricUtilizationQty = c.FinishFabricUtilizationQty;
                                ybc.ReqFinishFabricQty = c.ReqFinishFabricQty;
                                ybc.GreyReqQty = c.GreyReqQty;
                                ybc.GreyProdQty = c.GreyProdQty;
                                ybc.YarnAllowance = c.YarnAllowance;
                                List<YarnBookingChildItem> bookingChildItems = new List<YarnBookingChildItem>();

                                List<YarnBookingChild> bookingChildsForItemMasterID = new List<YarnBookingChild>();

                                c.ChildItems.ForEach(ci =>
                                {
                                    var childItem = ybc.ChildItems.Find(ybci => ybci.YBChildItemID == ci.YBChildItemID);
                                    if (childItem == null)
                                    {
                                        childItem = CommonFunction.DeepClone(ci);
                                        childItem.YBChildID = c.YBChildID;
                                        childItem.YBookingID = c.YBookingID;
                                        childItem.EntityState = EntityState.Added;
                                    }
                                    else
                                    {
                                        childItem.EntityState = EntityState.Modified;
                                    }
                                    childItem.Segment1ValueId = ci.Segment1ValueId;
                                    childItem.Segment2ValueId = ci.Segment2ValueId;
                                    childItem.Segment3ValueId = ci.Segment3ValueId;
                                    childItem.Segment4ValueId = ci.Segment4ValueId;
                                    childItem.Segment5ValueId = ci.Segment5ValueId;
                                    childItem.Segment6ValueId = ci.Segment6ValueId;
                                    childItem.Segment7ValueId = ci.Segment7ValueId;
                                    childItem.Segment8ValueId = ci.Segment8ValueId;
                                    childItem.Segment9ValueId = ci.Segment9ValueId;
                                    childItem.Segment10ValueId = ci.Segment10ValueId;

                                    childItem.Segment1ValueDesc = ci.Segment1ValueDesc;
                                    childItem.Segment2ValueDesc = ci.Segment2ValueDesc;
                                    childItem.Segment3ValueDesc = ci.Segment3ValueDesc;
                                    childItem.Segment4ValueDesc = ci.Segment4ValueDesc;
                                    childItem.Segment5ValueDesc = ci.Segment5ValueDesc;
                                    childItem.Segment6ValueDesc = ci.Segment6ValueDesc;
                                    childItem.Segment7ValueDesc = ci.Segment7ValueDesc;
                                    childItem.Segment8ValueDesc = ci.Segment8ValueDesc;
                                    childItem.Segment9ValueDesc = ci.Segment9ValueDesc;
                                    childItem.Segment10ValueDesc = ci.Segment10ValueDesc;

                                    childItem.ShadeCode = ci.ShadeCode;
                                    childItem.YarnLotNo = ci.YarnLotNo;
                                    childItem.Allowance = ci.Allowance;
                                    childItem.GreyAllowance = ci.GreyAllowance;
                                    childItem.YDAllowance = ci.YDAllowance;
                                    childItem.GreyYarnUtilizationQty = ci.GreyYarnUtilizationQty;
                                    childItem.DyedYarnUtilizationQty = ci.DyedYarnUtilizationQty;
                                    childItem.YarnReqQty = ci.YarnReqQty;
                                    childItem.NetYarnReqQty = ci.NetYarnReqQty;
                                    childItem.YarnBalanceQty = ci.YarnBalanceQty;
                                    childItem.Distribution = ci.Distribution;
                                    childItem.BookingQty = ci.BookingQty;
                                    //childItem.RequiredQty = ci.RequiredQty;
                                    childItem.Remarks = ci.Remarks;
                                    childItem.Specification = ci.Specification;
                                    childItem.YD = ci.YD;
                                    childItem.ItemMasterID = ci.YItemMasterID;
                                    childItem.YItemMasterID = ci.YItemMasterID;
                                    //childItem.YarnCategory = ci.YarnCategory;

                                    bookingChildItems.Add(childItem);
                                });
                                List<YarnBookingChildItem> childItemRecords = new List<YarnBookingChildItem>();
                                childItemRecords = bookingChildItems;
                                _yarnChildItemMasterRepository.GenerateItemWithYItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref ItemList, ref childItemRecords);

                                bookingChildItems.ForEach(x =>
                                {
                                    var objItem = childItemRecords.Find(y => y.Segment1ValueId == x.Segment1ValueId && y.Segment2ValueId == x.Segment2ValueId
                                                                && y.Segment3ValueId == x.Segment3ValueId && y.Segment4ValueId == x.Segment4ValueId
                                                                && y.Segment5ValueId == x.Segment5ValueId && y.Segment6ValueId == x.Segment6ValueId
                                                                && y.Segment7ValueId == x.Segment7ValueId && y.Segment8ValueId == x.Segment8ValueId
                                                                && y.Segment9ValueId == x.Segment9ValueId && y.Segment10ValueId == x.Segment10ValueId);

                                    x.ItemMasterID = objItem.ItemMasterID;
                                    x.YItemMasterID = objItem.ItemMasterID;
                                    x.BookingChildID = c.BookingChildID;
                                    x.ConsumptionID = c.ConsumptionID;
                                    x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);
                                });

                                ybc.ChildItems.Where(cii => cii.EntityState == EntityState.Unchanged).ToList().ForEach(cii =>
                                {
                                    cii.EntityState = EntityState.Deleted;
                                    bookingChildItems.Add(cii);

                                    mrChilds = this.SetMRChildValues(mrChilds, cii);
                                });
                                ybc.ChildItems = CommonFunction.DeepClone(bookingChildItems);
                            }
                            if (yb.SubGroupID == 1)
                            {
                                ybc.FinishFabricUtilizationPopUpList = this.GetUtilizationPopUpList(ybc, c, yb.Childs, yb.SubGroupID);
                                ybc.GreyFabricUtilizationPopUpList = this.GetGreyUtilizationPopUpList(ybc, c, yb.Childs, yb.SubGroupID);
                            }
                            List<BulkBookingGreyYarnUtilization> greyYarnUTListSave = new List<BulkBookingGreyYarnUtilization>();
                            List<BulkBookingDyedYarnUtilization> dyedYarnUTListSave = new List<BulkBookingDyedYarnUtilization>();
                            c.ChildItems.ForEach(ci =>
                            {
                                List<BulkBookingGreyYarnUtilization> greyYarnUtilizationListFromDB = new List<BulkBookingGreyYarnUtilization>();
                                List<BulkBookingDyedYarnUtilization> dyedYarnUtilizationListFromDB = new List<BulkBookingDyedYarnUtilization>();
                                ybc.ChildItems.ForEach(gy =>
                                {

                                    var greyYarnUtilizationList = gy.GreyYarnUtilizationPopUpList.Where(xgy => xgy.YBChildItemID == ci.YBChildItemID).ToList();
                                    if (greyYarnUtilizationList.Count > 0)
                                    {
                                        greyYarnUtilizationListFromDB.AddRange(CommonFunction.DeepClone(greyYarnUtilizationList));
                                    }

                                    var dyedYarnUtilizationList = gy.DyedYarnUtilizationPopUpList.Where(xgy => xgy.YBChildItemID == ci.YBChildItemID).ToList();
                                    if (dyedYarnUtilizationList.Count > 0)
                                    {
                                        dyedYarnUtilizationListFromDB.AddRange(CommonFunction.DeepClone(dyedYarnUtilizationList));
                                    }


                                });
                                greyYarnUtilizationListFromDB.SetUnchanged();
                                List<BulkBookingGreyYarnUtilization> tempGreyYarnUTList = new List<BulkBookingGreyYarnUtilization>();

                                tempGreyYarnUTList = this.GetGreyYarnUtilizationPopUpList(greyYarnUtilizationListFromDB, ci, yb.SubGroupID, ybc.YBChildID, yb.YBookingID, ci.YBChildItemID);
                                greyYarnUTListSave.AddRange(CommonFunction.DeepClone(tempGreyYarnUTList));

                                dyedYarnUtilizationListFromDB.SetUnchanged();
                                List<BulkBookingDyedYarnUtilization> tempDyedYarnUTList = new List<BulkBookingDyedYarnUtilization>();

                                tempDyedYarnUTList = this.GetDyedYarnUtilizationPopUpList(dyedYarnUtilizationListFromDB, ci, yb.SubGroupID, ybc.YBChildID, yb.YBookingID, ci.YBChildItemID);
                                dyedYarnUTListSave.AddRange(CommonFunction.DeepClone(tempDyedYarnUTList));

                            });


                            ybc.ChildItems.ForEach(gy =>
                            {
                                var greyYarnUtilizationList = greyYarnUTListSave.Where(xgy => xgy.YBChildItemID == gy.YBChildItemID).ToList();
                                if (greyYarnUtilizationList.Count > 0)
                                    gy.GreyYarnUtilizationPopUpList = CommonFunction.DeepClone(greyYarnUtilizationList);

                                var dyedYarnUtilizationList = dyedYarnUTListSave.Where(xgy => xgy.YBChildItemID == gy.YBChildItemID).ToList();
                                if (dyedYarnUtilizationList.Count > 0)
                                    gy.DyedYarnUtilizationPopUpList = CommonFunction.DeepClone(dyedYarnUtilizationList);

                            });



                            if (yb.SubGroupID != 1)
                            {
                                ybc.FinishFabricUtilizationPopUpList = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).FinishFabricUtilizationPopUpList);
                                ybc.GreyFabricUtilizationPopUpList = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).GreyFabricUtilizationPopUpList);
                            }
                            yarnBookingChilds.Add(ybc);

                            loopIndex++;
                        }

                        //YarnBookingChild ybc = yb.Childs.Find(x => x.YBChildID == c.YBChildID);

                    }
                });
                //List<YarnBookingChildItemRevision> TestList = new List<YarnBookingChildItemRevision>();
                yarnBookingChilds.ForEach(m =>
                {
                    m.ChildItemsRevision = new List<YarnBookingChildItemRevision>();
                    m.ChildItems.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified).ToList().ForEach(c =>
                    {
                        YarnBookingChildItemRevision obj = new YarnBookingChildItemRevision();
                        obj.YBChildItemID = c.YBChildItemID;
                        obj.YBChildID = c.YBChildID;
                        obj.YBookingID = c.YBookingID;
                        obj.YItemMasterID = c.YItemMasterID;
                        obj.UnitID = c.UnitID;
                        obj.Blending = c.Blending;
                        obj.YarnCategory = c.YarnCategory;
                        obj.Distribution = c.Distribution;
                        obj.BookingQty = c.BookingQty;
                        obj.Allowance = c.Allowance;
                        obj.RequiredQty = c.RequiredQty;
                        obj.ShadeCode = c.ShadeCode;
                        obj.Remarks = c.Remarks;
                        obj.Specification = c.Specification;
                        obj.YD = c.YD;
                        obj.YDItem = c.YDItem;
                        obj.StitchLength = c.StitchLength;
                        obj.PhysicalCount = c.PhysicalCount;
                        obj.BatchNo = c.BatchNo;
                        obj.SpinnerId = c.SpinnerId;
                        obj.YarnLotNo = c.YarnLotNo;
                        obj.YarnReqQty = c.YarnReqQty;
                        obj.YarnLeftOverQty = c.YarnLeftOverQty;
                        obj.NetYarnReqQty = c.NetYarnReqQty;
                        obj.YarnBalanceQty = c.YarnBalanceQty;
                        obj.YarnPly = c.YarnPly;
                        obj.GreyAllowance = c.GreyAllowance;
                        obj.YDAllowance = c.YDAllowance;
                        obj.GreyYarnUtilizationQty = c.GreyYarnUtilizationQty;
                        obj.DyedYarnUtilizationQty = c.DyedYarnUtilizationQty;
                        obj.AllowanceFM = c.AllowanceFM;
                        obj.RequiredQtyFM = c.RequiredQtyFM;
                        obj.SourcingRate = c.SourcingRate;
                        obj.SourcingLandedCost = c.SourcingLandedCost;
                        obj.TotalSourcingRate = c.TotalSourcingRate;
                        obj.DyeingCostFM = c.DyeingCostFM;
                        obj.EntityState = c.EntityState;
                        m.ChildItemsRevision.Add(obj);
                        yarnBookingChildItemsRevision.Add(obj);
                    });
                });
                /*
                yarnBookingChildItems.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified).ToList().ForEach(c =>
                {
                    YarnBookingChildItemRevision obj = new YarnBookingChildItemRevision();
                    obj.YBChildItemID = c.YBChildItemID;
                    obj.YBChildID = c.YBChildID;
                    obj.YBookingID = c.YBookingID;
                    obj.YItemMasterID = c.YItemMasterID;
                    obj.UnitID = c.UnitID;
                    obj.Blending = c.Blending;
                    obj.YarnCategory = c.YarnCategory;
                    obj.Distribution = c.Distribution;
                    obj.BookingQty = c.BookingQty;
                    obj.Allowance = c.Allowance;
                    obj.RequiredQty = c.RequiredQty;
                    obj.ShadeCode = c.ShadeCode;
                    obj.Remarks = c.Remarks;
                    obj.Specification = c.Specification;
                    obj.YD = c.YD;
                    obj.YDItem = c.YDItem;
                    obj.StitchLength = c.StitchLength;
                    obj.PhysicalCount = c.PhysicalCount;
                    obj.BatchNo = c.BatchNo;
                    obj.SpinnerId = c.SpinnerId;
                    obj.YarnLotNo = c.YarnLotNo;
                    obj.YarnReqQty = c.YarnReqQty;
                    obj.YarnLeftOverQty = c.YarnLeftOverQty;
                    obj.NetYarnReqQty = c.NetYarnReqQty;
                    obj.YarnBalanceQty = c.YarnBalanceQty;
                    obj.YarnPly = c.YarnPly;
                    obj.GreyAllowance = c.GreyAllowance;
                    obj.YDAllowance = c.YDAllowance;
                    obj.GreyYarnUtilizationQty = c.GreyYarnUtilizationQty;
                    obj.DyedYarnUtilizationQty = c.DyedYarnUtilizationQty;
                    obj.AllowanceFM = c.AllowanceFM;
                    obj.RequiredQtyFM = c.RequiredQtyFM;
                    obj.SourcingRate = c.SourcingRate;
                    obj.SourcingLandedCost = c.SourcingLandedCost;
                    obj.TotalSourcingRate = c.TotalSourcingRate;
                    obj.DyeingCostFM = c.DyeingCostFM;
                    obj.EntityState = c.EntityState;
                    yarnBookingChildItemsRevision.Add(obj);
                });*/
                //YarnBookingChildItemRevision O1 = TestList.Where(x => x.YBChildItemID == 4198101).FirstOrDefault();
                //YarnBookingChildItemRevision O2 = yarnBookingChildItemsRevision.Where(x => x.YBChildItemID == 4198101).FirstOrDefault();
            }

            await _service.SaveRevision(entities, yarnBookingChildItemsRevision, isRevisedYarn ? yarnBookingMasters : new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds, model.RevisionReasonList);
            return Ok(entities);
        }
        /*[Route("bulk/checkKnittingHead")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> BulkCheck(FBookingAcknowledge model)
        {
            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<FreeConceptMRChild> mrChilds = new List<FreeConceptMRChild>();

            if (model.FBAckID > 0 && model.BookingNo.IsNotNullOrEmpty())
            {
                entities = await _service.GetFBAcknowledgeMasterBulkWithRevision(model.BookingNo);
                entities.ForEach(entity =>
                {
                    entity.EntityState = EntityState.Modified;
                    if (model.IsRejectByKnittingHead)
                    {
                        entity.IsCheckByKnittingHead = false;
                        entity.CheckByKnittingHead = 0;
                        entity.CheckDateKnittingHead = null;

                        entity.IsRejectByKnittingHead = true;
                        entity.RejectByKnittingHead = AppUser.UserCode;
                        entity.RejectDateKnittingHead = DateTime.Now;
                        entity.RejectReasonKnittingHead = model.RejectReasonKnittingHead;
                    }
                    else
                    {
                        entity.IsCheckByKnittingHead = true;
                        entity.CheckByKnittingHead = AppUser.UserCode;
                        entity.CheckDateKnittingHead = DateTime.Now;

                        entity.IsRejectByKnittingHead = false;
                        entity.RejectByKnittingHead = 0;
                        entity.RejectDateKnittingHead = null;
                    }
                });
            }
            await _service.UpdateBulkStatus(entities, new List<YarnBookingChildItem>(), new List<YarnBookingMaster>(), new List<YarnBookingChild>(), mrChilds);
            return Ok(entities);
        }*/
        [Route("bulk/checkKnittingHead")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> checkKnittingHead(FBookingAcknowledge modelDynamic)
        {
            DateTime currentDate = DateTime.Now;

            FBookingAcknowledge model = modelDynamic;
            bool isRevised = model.IsRevised;
            bool isAddition = model.IsAddition;
            bool isReviseBBKI = model.IsReviseBBKI;
            bool isUnAcknowledge = model.IsUnAcknowledge;
            string unAcknowledgeReason = model.UnAcknowledgeReason;

            FBookingAcknowledge entity = new FBookingAcknowledge();
            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<FreeConceptMRChild> mrChilds = new List<FreeConceptMRChild>();

            if (model.BookingNo.IsNotNullOrEmpty())
            {
                entity = await _service.GetFBAcknowledgeBulkWithRevision(model.BookingNo, isAddition);
                entity.FBookingChild.SetModified();

                entity.CollarSizeID = model.CollarSizeID;
                entity.CollarWeightInGm = model.CollarWeightInGm;
                entity.CuffSizeID = model.CuffSizeID;
                entity.CuffWeightInGm = model.CuffWeightInGm;

                entity.ParentYBookingNo = modelDynamic.ParentYBookingNo;
                entity.EntityState = EntityState.Modified;

                entities = await _service.GetFBAcknowledgeMasterBulkWithRevision(model.BookingNo, isAddition);
                entities.SetModified();
                entities.ForEach(x =>
                {
                    x.FBookingChild.SetModified();
                    x.CollarSizeID = model.CollarSizeID;
                    x.CollarWeightInGm = model.CollarWeightInGm;
                    x.CuffSizeID = model.CuffSizeID;
                    x.CuffWeightInGm = model.CuffWeightInGm;
                });

                if (!model.IsUnAcknowledge)
                {
                    #region Extend Model
                    var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo, isAddition);
                    var yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByBookingNoWithRevision(model.BookingNo, isAddition);

                    var collarListModel = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                    var cuffListModel = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                    var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                    var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                    var yarnItemsCollar = yarnBookingChildItems.Where(x => x.SubGroupId == 11).ToList();
                    var yarnItemsCuff = yarnBookingChildItems.Where(x => x.SubGroupId == 12).ToList();

                    List<FBookingAcknowledgeChild> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnItemsCollar, model);
                    List<FBookingAcknowledgeChild> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnItemsCuff, model);

                    modelDynamic.FBookingChild = modelDynamic.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                    modelDynamic.FBookingChild.AddRange(collarList);
                    modelDynamic.FBookingChild.AddRange(cuffList);

                    model.FBookingChild = CommonFunction.DeepClone(modelDynamic.FBookingChild);

                    model.YChilds.Where(x => x.SubGroupId != 1).ToList().ForEach(x =>
                    {
                        x.ChildItems = model.ChildItems.Where(y => y.YBChildID == x.YBChildID).ToList();
                    });
                    #endregion

                    if (!isAddition)
                    {
                        entity.DateUpdated = currentDate;
                        entity.UpdatedBy = AppUser.UserCode;

                        if (model.IsCheckByKnittingHead || model.IsRejectByKnittingHead || model.IsUnAcknowledge)
                        {
                            entities.ForEach(e =>
                            {
                                e.EntityState = EntityState.Modified;
                                e.IsSample = model.IsSample;

                                if (model.IsUnAcknowledge)
                                {
                                    e.IsUnAcknowledge = true;
                                    e.UnAcknowledgeBy = AppUser.EmployeeCode;
                                    e.UnAcknowledgeDate = currentDate;
                                    e.UnAcknowledgeReason = model.UnAcknowledgeReason;
                                }

                                if (model.IsKnittingComplete)
                                {
                                    e.IsKnittingComplete = true;
                                    e.KnittingCompleteBy = AppUser.UserCode;
                                    e.KnittingCompleteDate = currentDate;

                                    e.KnittingRevisionNo = entity.RevisionNo;
                                }

                                if (model.IsRevised)
                                {
                                    e.RevisionNo = e.PreRevisionNo;
                                    e.RevisionDate = currentDate;
                                }

                                if (model.IsRejectByKnittingHead)
                                {
                                    e.IsCheckByKnittingHead = false;
                                    e.CheckByKnittingHead = 0;
                                    e.CheckDateKnittingHead = null;

                                    e.IsRejectByKnittingHead = true;
                                    e.RejectByKnittingHead = AppUser.UserCode;
                                    e.RejectDateKnittingHead = currentDate;
                                    e.RejectReasonKnittingHead = model.RejectReasonKnittingHead;
                                }
                                else
                                {
                                    e.IsCheckByKnittingHead = true;
                                    e.CheckByKnittingHead = AppUser.UserCode;
                                    e.CheckDateKnittingHead = currentDate;

                                    e.IsRejectByKnittingHead = false;
                                    e.RejectByKnittingHead = 0;
                                    e.RejectDateKnittingHead = null;
                                }

                                e.EntityState = EntityState.Modified;
                                e.DateUpdated = currentDate;
                                e.UpdatedBy = AppUser.UserCode;
                            });
                        }
                    }
                }
            }

            entity.CompanyID = model.CompanyID;
            entity.ExportOrderID = model.ExportOrderID;

            string grpConceptNo = model.grpConceptNo;
            int isBDS = model.IsBDS;

            int preRevisionNo = model.PreRevisionNo;

            entity.IsSample = model.IsSample;
            entity.AddedBy = AppUser.UserCode;

            if (model.IsKnittingComplete)
            {
                if (model.IsRevised)
                {
                    entity.RevisionNo = entity.PreRevisionNo;
                    entity.RevisionDate = currentDate;
                }
                entity.IsKnittingComplete = true;
                entity.KnittingCompleteBy = AppUser.UserCode;
                entity.KnittingCompleteDate = currentDate;

                entity.KnittingRevisionNo = entity.RevisionNo;
            }

            if (model.IsUnAcknowledge)
            {
                entity.IsUnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.EmployeeCode;
                entity.UnAcknowledgeDate = currentDate;
                entity.UnAcknowledgeReason = model.UnAcknowledgeReason;

                entity.IsInternalRevise = false;

                entity.IsRejectByAllowance = false;
                entity.IsRejectByKnittingHead = false;
                entity.IsRejectByProdHead = false;

                entity.RejectByAllowance = 0;
                entity.RejectByKnittingHead = 0;
                entity.RejectByProdHead = 0;

                entities.ForEach(x =>
                {
                    x.IsUnAcknowledge = true;
                    x.UnAcknowledgeBy = AppUser.EmployeeCode;
                    x.UnAcknowledgeDate = currentDate;
                    x.UnAcknowledgeReason = model.UnAcknowledgeReason;

                    x.IsInternalRevise = false;

                    x.IsRejectByAllowance = false;
                    x.IsRejectByKnittingHead = false;
                    x.IsRejectByProdHead = false;

                    x.RejectByAllowance = 0;
                    x.RejectByKnittingHead = 0;
                    x.RejectByProdHead = 0;
                });
            }

            if (!model.IsUnAcknowledge)
            {
                List<YarnBookingMaster> yarnBookings = entity.YarnBookings;
                List<YarnBookingChild> tempYarnBookingChilds = new List<YarnBookingChild>();
                List<YarnBookingChildItem> tempYarnBookingChildItems = new List<YarnBookingChildItem>();

                mrChilds = await _fcMRService.GetMRChildByBookingNoWithRevision(model.BookingNo);
                mrChilds.SetModified();

                yarnBookings.ForEach(yb =>
                {
                    tempYarnBookingChilds.AddRange(yb.Childs);
                });

                List<ItemMasterBomTemp> ItemList = new List<ItemMasterBomTemp>();
                ItemList = _yarnChildItemMasterRepository.GetItemMasterList(AppConstants.ITEM_SUB_GROUP_YARN_LIVE);

                entity.FBookingChild.ForEach(item =>
                {
                    tempYarnBookingChildItems = new List<YarnBookingChildItem>();

                    FBookingAcknowledgeChild obj = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == item.BookingChildID));

                    if (obj != null)
                    {
                        //if (obj.SubGroupID != 1)
                        //{
                        //    item.FinishFabricUtilizationPopUpList = CommonFunction.DeepClone(CommonFunction.DeepClone(obj.FinishFabricUtilizationPopUpList));
                        //    item.GreyFabricUtilizationPopUpList = CommonFunction.DeepClone(CommonFunction.DeepClone(obj.GreyFabricUtilizationPopUpList));
                        //}

                        item.MachineTypeId = obj.MachineTypeId;
                        item.TechnicalNameId = obj.TechnicalNameId;
                        item.MachineGauge = obj.MachineGauge;
                        item.MachineDia = obj.MachineDia;
                        item.GreyReqQty = obj.GreyReqQty;
                        item.GreyLeftOverQty = obj.GreyLeftOverQty;
                        item.GreyProdQty = obj.GreyProdQty;
                        item.ReqFinishFabricQty = obj.ReqFinishFabricQty;
                        item.FinishFabricUtilizationQty = obj.FinishFabricUtilizationQty;
                        item.RefSourceID = obj.RefSourceID;
                        item.RefSourceNo = obj.RefSourceNo;
                        item.SourceConsumptionID = obj.SourceConsumptionID;
                        item.SourceItemMasterID = obj.SourceItemMasterID;
                        item.BookingQtyKG = obj.BookingQtyKG;
                        item.BrandID = obj.BrandID;
                        item.Brand = obj.Brand;
                        item.YarnAllowance = obj.YarnAllowance;

                        if (isAddition)
                        {
                            item.IsForFabric = obj.IsForFabric;
                            item.BookingQty = obj.IsForFabric ? obj.BookingQty : 0;
                        }
                        item.EntityState = EntityState.Modified;

                        List<YarnBookingChildItem> childItemRecords = new List<YarnBookingChildItem>();
                        childItemRecords = obj.ChildItems;
                        _yarnChildItemMasterRepository.GenerateItemWithYItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref ItemList, ref childItemRecords);

                        obj.ChildItems.ForEach(x =>
                        {
                            var objItem = childItemRecords.Find(y => y.Segment1ValueId == x.Segment1ValueId && y.Segment2ValueId == x.Segment2ValueId
                                                        && y.Segment3ValueId == x.Segment3ValueId && y.Segment4ValueId == x.Segment4ValueId
                                                        && y.Segment5ValueId == x.Segment5ValueId && y.Segment6ValueId == x.Segment6ValueId
                                                        && y.Segment7ValueId == x.Segment7ValueId && y.Segment8ValueId == x.Segment8ValueId
                                                        && y.Segment9ValueId == x.Segment9ValueId && y.Segment10ValueId == x.Segment10ValueId);

                            x.ItemMasterID = objItem.ItemMasterID;
                            x.YItemMasterID = objItem.ItemMasterID;
                            x.BookingChildID = item.BookingChildID;
                            x.ConsumptionID = item.ConsumptionID;
                            x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);


                            mrChilds = this.SetMRChildValues(mrChilds, x);
                        });

                        YarnBookingChild childTemp = tempYarnBookingChilds.Find(x => x.BookingChildID == item.BookingChildID);
                        if (childTemp != null && childTemp.ChildItems.Count() > 0)
                        {
                            childTemp.ChildItems.ForEach(ybci =>
                            {
                                YarnBookingChildItem ybciObj = obj.ChildItems.Find(x => x.YBChildItemID == ybci.YBChildItemID);
                                if (ybciObj == null)
                                {
                                    ybci.EntityState = EntityState.Deleted;
                                    obj.ChildItems.Add(ybci);
                                }
                            });
                        }
                        item.ChildItems = obj.ChildItems;

                        if (entity.YarnBookings.Count() > 0)
                        {
                            YarnBookingMaster MasterDB = null;
                            try
                            {
                                MasterDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);
                            }
                            catch (Exception ex) { }
                            if (MasterDB != null)
                            {
                                YarnBookingChild childsDB = null;
                                try
                                {
                                    childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                      .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                                }
                                catch (Exception ex) { }

                                if (childsDB == null)
                                {
                                    var yb = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);

                                    item.Length = item.Length == null || item.Length == "" ? 0.ToString() : item.Length;
                                    item.Height = item.Height == null || item.Height == "" ? 0.ToString() : item.Height;

                                    YarnBookingChild yarnBookingChild = new YarnBookingChild();
                                    yarnBookingChild = new YarnBookingChild();
                                    yarnBookingChild.IsNewObj = true;
                                    yarnBookingChild.YBChildID = 0;
                                    if (yb != null)
                                    {
                                        yarnBookingChild.YBookingID = yb.YBookingID;
                                    }


                                    yarnBookingChild.BookingChildID = item.BookingChildID;
                                    yarnBookingChild.ConsumptionID = item.ConsumptionID;
                                    yarnBookingChild.ItemMasterID = item.ItemMasterID;
                                    yarnBookingChild.YarnTypeID = 0;
                                    yarnBookingChild.YarnBrandID = item.YarnBrandID;
                                    yarnBookingChild.FUPartID = item.FUPartID;
                                    yarnBookingChild.BookingUnitID = item.BookingUnitID;
                                    yarnBookingChild.BookingQty = item.BookingQty;
                                    yarnBookingChild.FTechnicalName = "";
                                    yarnBookingChild.IsCompleteReceive = item.IsCompleteReceive;
                                    yarnBookingChild.LastDCDate = item.LastDCDate;
                                    yarnBookingChild.ClosingRemarks = item.ClosingRemarks;

                                    yarnBookingChild.GreyReqQty = item.GreyReqQty;
                                    yarnBookingChild.GreyLeftOverQty = item.GreyLeftOverQty;
                                    yarnBookingChild.GreyProdQty = item.GreyProdQty;
                                    yarnBookingChild.ReqFinishFabricQty = item.ReqFinishFabricQty;
                                    yarnBookingChild.FinishFabricUtilizationQty = item.FinishFabricUtilizationQty;

                                    yarnBookingChild.QtyInKG = (Convert.ToDecimal(item.Length) *
                                         Convert.ToDecimal(item.Height) *
                                         Convert.ToDecimal(0.045) *
                                         item.BookingQty) / 420;

                                    yarnBookingChild.ExcessPercentage = item.ExcessPercentage;
                                    yarnBookingChild.ExcessQty = item.ExcessQty;
                                    yarnBookingChild.ExcessQtyInKG = item.ExcessQtyInKG;
                                    yarnBookingChild.TotalQty = item.TotalQty;
                                    yarnBookingChild.TotalQtyInKG = (Convert.ToDecimal(item.Length) *
                                                    Convert.ToDecimal(item.Height) *
                                                    Convert.ToDecimal(0.045) *
                                                    item.TotalQty) / 420;
                                    yarnBookingChild.YarnAllowance = item.YarnAllowance;

                                    yarnBookingChild.EntityState = EntityState.Added;

                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs.Add(yarnBookingChild);

                                    childsDB = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                   .Childs.Find(x => x.BookingChildID == item.BookingChildID);
                                }
                                else
                                {
                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                  .Childs.Find(x => x.BookingChildID == item.BookingChildID).YarnAllowance = item.YarnAllowance;
                                    entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                   .Childs.Find(x => x.BookingChildID == item.BookingChildID).EntityState = EntityState.Modified;
                                }



                                var yBChild = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs.Find(x => x.BookingChildID == item.BookingChildID);
                                var ybMaster = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID);
                                if (yBChild.IsNotNull())
                                {
                                    if (ybMaster.SubGroupID != 1)
                                    {
                                        yBChild.FinishFabricUtilizationPopUpList = CommonFunction.DeepClone(CommonFunction.DeepClone(obj.FinishFabricUtilizationPopUpList));
                                        yBChild.GreyFabricUtilizationPopUpList = CommonFunction.DeepClone(CommonFunction.DeepClone(obj.GreyFabricUtilizationPopUpList));
                                        //ybc.FinishFabricUtilizationPopUpList = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).FinishFabricUtilizationPopUpList);
                                        //ybc.GreyFabricUtilizationPopUpList = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID).GreyFabricUtilizationPopUpList);
                                    }
                                    if (ybMaster.SubGroupID == 1)
                                    {
                                        YarnBookingChild yarnBookingChildList = new YarnBookingChild();

                                        yarnBookingChildList.FinishFabricUtilizationPopUpList = CommonFunction.DeepClone(obj.FinishFabricUtilizationPopUpList);
                                        yarnBookingChildList.GreyFabricUtilizationPopUpList = CommonFunction.DeepClone(obj.GreyFabricUtilizationPopUpList);

                                        yBChild.FinishFabricUtilizationPopUpList = CommonFunction.DeepClone(this.GetUtilizationPopUpList(yBChild, yarnBookingChildList, ybMaster.Childs, ybMaster.SubGroupID));
                                        yBChild.GreyFabricUtilizationPopUpList = CommonFunction.DeepClone(this.GetGreyUtilizationPopUpList(yBChild, yarnBookingChildList, ybMaster.Childs, ybMaster.SubGroupID));
                                    }


                                }

                                int indexChild = entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID)
                                                                   .Childs.FindIndex(x => x.BookingChildID == item.BookingChildID);

                                item.ChildItems.Where(x => x.EntityState != EntityState.Deleted).ToList().ForEach(childItemModel =>
                                {
                                    int indexCItem = childsDB.ChildItems.FindIndex(x => x.YBChildItemID == childItemModel.YBChildItemID);
                                    if (indexChild > -1 && indexCItem > -1)
                                    {
                                        childItemModel.EntityState = EntityState.Modified;
                                        childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                        entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems[indexCItem] = CommonFunction.DeepClone(childItemModel);
                                    }
                                    else
                                    {
                                        YarnBookingMaster masterObj = CommonFunction.DeepClone(entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID));
                                        YarnBookingChild childObj = CommonFunction.DeepClone(masterObj.Childs[indexChild]);
                                        childItemModel.YBookingID = masterObj.YBookingID;
                                        childItemModel.YBChildID = childObj.YBChildID;
                                        childItemModel.YarnCategory = CommonFunction.GetYarnShortForm(childItemModel.Segment1ValueDesc, childItemModel.Segment2ValueDesc, childItemModel.Segment3ValueDesc, childItemModel.Segment4ValueDesc, childItemModel.Segment5ValueDesc, childItemModel.Segment6ValueDesc, childItemModel.ShadeCode);
                                        childItemModel.EntityState = EntityState.Added;
                                        entity.YarnBookings.Find(x => x.SubGroupID == item.SubGroupID).Childs[indexChild].ChildItems.Add(CommonFunction.DeepClone(childItemModel));
                                    }
                                });
                            }
                        }

                        item.ChildDetails.ForEach(itemDetail =>
                        {
                            itemDetail.TechnicalNameId = obj.TechnicalNameId;
                            itemDetail.EntityState = EntityState.Modified;
                        });

                        #region Finishing Process Operations
                        var childIndexE = entity.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);
                        var childIndexM = model.FBookingChild.FindIndex(x => x.BookingChildID == item.BookingChildID);

                        if (childIndexE > -1 && childIndexM > -1)
                        {
                            entity.FBookingChild[childIndexE].PreFinishingProcessChilds.SetUnchanged();
                            entity.FBookingChild[childIndexE].PostFinishingProcessChilds.SetUnchanged();

                            model.FBookingChild[childIndexM].PreFinishingProcessChilds.ForEach(mChildFP =>
                            {
                                int indexChildFP = entity.FBookingChild[childIndexE].PreFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                                if (indexChildFP == -1)
                                {
                                    mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                    mChildFP.EntityState = EntityState.Added;
                                    entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Add(mChildFP);
                                }
                                else
                                {
                                    mChildFP.EntityState = EntityState.Modified;
                                    entity.FBookingChild[childIndexE].PreFinishingProcessChilds[indexChildFP] = mChildFP;
                                }
                            });

                            model.FBookingChild[childIndexM].PostFinishingProcessChilds.ForEach(mChildFP =>
                            {
                                int indexChildFP = entity.FBookingChild[childIndexE].PostFinishingProcessChilds.FindIndex(x => x.FPChildID == mChildFP.FPChildID);
                                if (indexChildFP == -1)
                                {
                                    mChildFP.BookingChildID = entity.FBookingChild[childIndexE].BookingChildID;
                                    mChildFP.EntityState = EntityState.Added;
                                    entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Add(mChildFP);
                                }
                                else
                                {
                                    mChildFP.EntityState = EntityState.Modified;
                                    entity.FBookingChild[childIndexE].PostFinishingProcessChilds[indexChildFP] = mChildFP;
                                }
                            });

                            entity.FBookingChild[childIndexE].PreFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                            entity.FBookingChild[childIndexE].PostFinishingProcessChilds.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                        }
                        #endregion
                    }
                });


                if (!isAddition)
                {
                    if (entity.YarnBookings.Count() > 0)
                    {
                        entity.HasYarnBooking = true;
                    }
                    if (entities.Count() > 0)
                    {
                        entities.ForEach(x =>
                        {
                            x.ExportOrderID = entity.ExportOrderID;
                            x.CompanyID = entity.CompanyID;
                            x.AddedBy = entity.AddedBy;
                            x.DateAdded = entity.DateAdded;
                            x.UpdatedBy = entity.UpdatedBy;
                            x.DateUpdated = entity.DateUpdated;
                        });
                    }
                }

                #region BBKI Revision Operation
                if (isReviseBBKI)
                {
                    entity.UserId = AppUser.UserCode;
                    entity.IsReviseBBKI = true;
                    entity.PreRevisionNoBBKI++;
                    entity.IsUnAcknowledge = false;
                    entity.IsCheckByKnittingHead = false;
                    entity.IsRejectByKnittingHead = false;
                    entity.IsUtilizationProposalSend = false;
                    entity.IsUtilizationProposalConfirmed = false;
                    entity.IsApprovedByProdHead = false;
                    entity.IsRejectByProdHead = false;
                    entity.IsApprovedByPMC = false;
                    entity.IsRejectByPMC = false;
                    entity.IsApprovedByAllowance = false;
                    entity.IsRejectByAllowance = false;
                    entity.RivisionReason = model.RivisionReason;

                    entities.Where(x => x.FBAckID != entity.FBAckID).ToList().ForEach(x =>
                    {
                        x.IsReviseBBKI = entity.IsReviseBBKI;
                        x.PreRevisionNoBBKI = entity.PreRevisionNoBBKI;
                        x.IsUnAcknowledge = false;
                        x.IsCheckByKnittingHead = false;
                        x.IsRejectByKnittingHead = false;
                        x.IsUtilizationProposalSend = false;
                        x.IsUtilizationProposalConfirmed = false;
                        x.IsApprovedByProdHead = false;
                        x.IsRejectByProdHead = false;
                        x.IsApprovedByPMC = false;
                        x.IsRejectByPMC = false;
                        x.IsApprovedByAllowance = false;
                        x.IsRejectByAllowance = false;
                    });
                }
                #endregion
            }

            if (model.IsYarnRevision)
            {

                foreach (YarnBookingMaster YBM in entity.YarnBookings)
                {
                    foreach (YarnBookingChild YBC in YBM.Childs)
                    {
                        YBC.ChildItemsRevision = GetYarnRevisedChildItems(YBC.ChildItems);
                    }
                }
                foreach (FBookingAcknowledgeChild FBAC in entity.FBookingChild)
                {

                    FBAC.ChildItemsRevision = GetYarnRevisedChildItems(FBAC.ChildItems);

                }

                string result = await _service.SaveAsyncBulkWithRevision(AppUser.UserCode, entity, entities, isAddition, mrChilds);

                if (isAddition) entity.YBookingNo = result;
            }
            else
            {
                string result = await _service.SaveAsyncBulk(AppUser.UserCode, entity, entities, isAddition, mrChilds);

                if (isAddition) entity.YBookingNo = result;
            }


            return Ok(entity);
        }
        [Route("bulk/approveProdHead")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> BulkApprove(FBookingAcknowledge model)
        {

            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<YarnBookingChild> yarnBookingChilds = new List<YarnBookingChild>();
            List<YarnBookingChildItem> yarnBookingChildItems = new List<YarnBookingChildItem>();
            List<FreeConceptMRChild> mrChilds = new List<FreeConceptMRChild>();

            if (model.FBAckID > 0 && model.BookingNo.IsNotNullOrEmpty())
            {
                mrChilds = await _fcMRService.GetMRChildByBookingNoWithRevision(model.BookingNo);
                mrChilds.SetModified();

                entities = await _service.GetFBAcknowledgeMasterBulkWithChildWithRevision(model.BookingNo);
                #region Extend Model
                var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);
                yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByBookingNoWithRevision(model.BookingNo);

                var collarListModel = model.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                var cuffListModel = model.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                var yarnItemsCollar = yarnBookingChildItems.Where(x => x.SubGroupId == 11).ToList();
                var yarnItemsCuff = yarnBookingChildItems.Where(x => x.SubGroupId == 12).ToList();

                List<FBookingAcknowledgeChild> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnItemsCollar, model);
                List<FBookingAcknowledgeChild> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnItemsCuff, model);

                model.FBookingChild = model.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                model.FBookingChild.AddRange(collarList);
                model.FBookingChild.AddRange(cuffList);

                model.FBookingChild = CommonFunction.DeepClone(model.FBookingChild);

                model.ChildItems = model.ChildItems.Where(x => x.SubGroupId == 1).ToList();
                collarList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));
                cuffList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));

                //model.YChilds.Where(x => x.SubGroupId != 1).ToList().ForEach(x =>
                //{
                //    x.ChildItems = model.ChildItems.Where(y => y.YBChildID == x.YBChildID).ToList();
                //});


                #endregion

                entities.ForEach(entity =>
                {
                    entity.EntityState = EntityState.Modified;
                    entity.FBookingChild.SetModified();

                    if (model.IsRejectByProdHead)
                    {
                        entity.IsApprovedByProdHead = false;
                        entity.ApprovedByProdHead = 0;
                        entity.ApprovedDateProdHead = null;

                        entity.IsRejectByProdHead = true;
                        entity.RejectByProdHead = AppUser.UserCode;
                        entity.RejectDateProdHead = DateTime.Now;
                        entity.RejectReasonProdHead = model.RejectReasonProdHead;
                    }
                    else
                    {
                        entity.IsApprovedByProdHead = true;
                        entity.ApprovedByProdHead = AppUser.UserCode;
                        entity.ApprovedDateProdHead = DateTime.Now;

                        entity.IsRejectByProdHead = false;
                        entity.RejectByProdHead = 0;
                        entity.RejectDateProdHead = null;

                        List<YarnBookingChild> tempYarnBookingChilds = new List<YarnBookingChild>();
                        List<YarnBookingChildItem> tempYarnBookingChildItems = new List<YarnBookingChildItem>();
                        foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
                        {
                            tempYarnBookingChildItems = new List<YarnBookingChildItem>();

                            FBookingAcknowledgeChild obj = model.FBookingChild.Find(x => x.BookingChildID == item.BookingChildID);

                            if (obj != null)
                            {
                                item.MachineTypeId = obj.MachineTypeId;
                                item.MachineType = obj.MachineType;
                                item.TechnicalNameId = obj.TechnicalNameId;
                                item.TechnicalName = obj.TechnicalName;
                                item.MachineGauge = obj.MachineGauge;
                                item.MachineDia = obj.MachineDia;
                                item.BrandID = obj.BrandID;
                                item.GreyReqQty = obj.GreyReqQty;
                                item.GreyLeftOverQty = obj.GreyLeftOverQty;
                                item.GreyProdQty = obj.GreyProdQty;
                                item.ReqFinishFabricQty = obj.ReqFinishFabricQty;
                                item.FinishFabricUtilizationQty = obj.FinishFabricUtilizationQty;
                                item.RefSourceID = obj.RefSourceID;
                                item.ReqFinishFabricQty = obj.ReqFinishFabricQty;
                                item.SourceConsumptionID = obj.SourceConsumptionID;
                                item.SourceItemMasterID = obj.SourceItemMasterID;

                                foreach (FBookingAcknowledgeChildDetails itemDetail in item.ChildDetails)
                                {
                                    itemDetail.TechnicalNameId = obj.TechnicalNameId;
                                    itemDetail.EntityState = EntityState.Modified;
                                }
                            }
                        }
                    }
                });
                // alamin
                List<YarnBookingMaster> yarnBookings = await _service.GetYarnBookingsBulkWithRevision(model.BookingNo);

                yarnBookings.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.Childs.ForEach(c =>
                    {
                        c.EntityState = EntityState.Unchanged;
                        c.ChildItems.SetUnchanged();
                    });
                });
                List<ItemMasterBomTemp> ItemList = new List<ItemMasterBomTemp>();
                ItemList = _yarnChildItemMasterRepository.GetItemMasterList(AppConstants.ITEM_SUB_GROUP_YARN_LIVE);

                model.YChilds.ForEach(c =>
                {
                    YarnBookingMaster yb = yarnBookings.Find(x => x.YBookingID == c.YBookingID);

                    if (yb.IsNotNull())
                    {
                        List<YarnBookingChild> ybcs = new List<YarnBookingChild>();
                        if (c.SubGroupId == 1)
                        {
                            ybcs = yb.Childs.Where(x => x.YBChildID == c.YBChildID).ToList();
                        }
                        else
                        {
                            ybcs = yb.Childs.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color).ToList();
                        }
                        int maxChildCount = ybcs.Count();
                        int loopIndex = 0;
                        //YarnBookingChild ybc = yb.Childs.Find(x => x.YBChildID == c.YBChildID);
                        while (loopIndex < maxChildCount)
                        {
                            var ybc = ybcs[loopIndex];
                            c.ChildItems = model.ChildItems.Where(y => y.YBChildID == ybc.YBChildID).ToList();

                            if (ybc == null)
                            {
                                ybc = CommonFunction.DeepClone(c);
                                ybc.EntityState = EntityState.Added;
                            }
                            else
                            {
                                ybc.EntityState = EntityState.Modified;
                                ybc.YarnAllowance = c.YarnAllowance;
                                List<YarnBookingChildItem> bookingChildItems = new List<YarnBookingChildItem>();

                                List<YarnBookingChild> bookingChildsForItemMasterID = new List<YarnBookingChild>();

                                c.ChildItems.ForEach(ci =>
                                {
                                    var childItem = ybc.ChildItems.Find(ybci => ybci.YBChildItemID == ci.YBChildItemID);
                                    if (childItem == null)
                                    {
                                        childItem = CommonFunction.DeepClone(ci);
                                        childItem.YBChildID = c.YBChildID;
                                        childItem.YBookingID = c.YBookingID;
                                        childItem.EntityState = EntityState.Added;
                                    }
                                    else
                                    {
                                        childItem.EntityState = EntityState.Modified;
                                    }
                                    childItem.Segment1ValueId = ci.Segment1ValueId;
                                    childItem.Segment2ValueId = ci.Segment2ValueId;
                                    childItem.Segment3ValueId = ci.Segment3ValueId;
                                    childItem.Segment4ValueId = ci.Segment4ValueId;
                                    childItem.Segment5ValueId = ci.Segment5ValueId;
                                    childItem.Segment6ValueId = ci.Segment6ValueId;
                                    childItem.Segment7ValueId = ci.Segment7ValueId;
                                    childItem.Segment8ValueId = ci.Segment8ValueId;
                                    childItem.Segment9ValueId = ci.Segment9ValueId;
                                    childItem.Segment10ValueId = ci.Segment10ValueId;

                                    childItem.Segment1ValueDesc = ci.Segment1ValueDesc;
                                    childItem.Segment2ValueDesc = ci.Segment2ValueDesc;
                                    childItem.Segment3ValueDesc = ci.Segment3ValueDesc;
                                    childItem.Segment4ValueDesc = ci.Segment4ValueDesc;
                                    childItem.Segment5ValueDesc = ci.Segment5ValueDesc;
                                    childItem.Segment6ValueDesc = ci.Segment6ValueDesc;
                                    childItem.Segment7ValueDesc = ci.Segment7ValueDesc;
                                    childItem.Segment8ValueDesc = ci.Segment8ValueDesc;
                                    childItem.Segment9ValueDesc = ci.Segment9ValueDesc;
                                    childItem.Segment10ValueDesc = ci.Segment10ValueDesc;
                                    //--
                                    childItem.ShadeCode = ci.ShadeCode;
                                    childItem.YarnLotNo = ci.YarnLotNo;
                                    childItem.Allowance = ci.Allowance;
                                    childItem.GreyAllowance = ci.GreyAllowance;
                                    childItem.YDAllowance = ci.YDAllowance;
                                    childItem.GreyYarnUtilizationQty = ci.GreyYarnUtilizationQty;
                                    childItem.DyedYarnUtilizationQty = ci.DyedYarnUtilizationQty;
                                    childItem.YarnReqQty = ci.YarnReqQty;
                                    childItem.NetYarnReqQty = ci.NetYarnReqQty;
                                    childItem.YarnBalanceQty = ci.YarnBalanceQty;
                                    childItem.Distribution = ci.Distribution;
                                    childItem.BookingQty = ci.BookingQty;
                                    //childItem.RequiredQty = ci.RequiredQty;
                                    childItem.Remarks = ci.Remarks;
                                    childItem.Specification = ci.Specification;
                                    childItem.YD = ci.YD;
                                    childItem.ItemMasterID = ci.YItemMasterID;
                                    childItem.YItemMasterID = ci.YItemMasterID;

                                    bookingChildItems.Add(childItem);

                                    mrChilds = this.SetMRChildValues(mrChilds, ci);
                                });

                                List<YarnBookingChildItem> childItemRecords = new List<YarnBookingChildItem>();
                                childItemRecords = bookingChildItems;
                                _yarnChildItemMasterRepository.GenerateItemWithYItem(AppConstants.ITEM_SUB_GROUP_YARN_LIVE, ref ItemList, ref childItemRecords);

                                bookingChildItems.ForEach(x =>
                                {
                                    var objItem = childItemRecords.Find(y => y.Segment1ValueId == x.Segment1ValueId && y.Segment2ValueId == x.Segment2ValueId
                                                                && y.Segment3ValueId == x.Segment3ValueId && y.Segment4ValueId == x.Segment4ValueId
                                                                && y.Segment5ValueId == x.Segment5ValueId && y.Segment6ValueId == x.Segment6ValueId
                                                                && y.Segment7ValueId == x.Segment7ValueId && y.Segment8ValueId == x.Segment8ValueId
                                                                && y.Segment9ValueId == x.Segment9ValueId && y.Segment10ValueId == x.Segment10ValueId);

                                    x.ItemMasterID = objItem.ItemMasterID;
                                    x.YItemMasterID = objItem.ItemMasterID;
                                    x.BookingChildID = c.BookingChildID;
                                    x.ConsumptionID = c.ConsumptionID;
                                    x.YarnCategory = CommonFunction.GetYarnShortForm(x.Segment1ValueDesc, x.Segment2ValueDesc, x.Segment3ValueDesc, x.Segment4ValueDesc, x.Segment5ValueDesc, x.Segment6ValueDesc, x.ShadeCode);
                                });
                                ybc.ChildItems.Where(cii => cii.EntityState == EntityState.Unchanged).ToList().ForEach(cii =>
                                {
                                    cii.EntityState = EntityState.Deleted;
                                    bookingChildItems.Add(cii);

                                    mrChilds = this.SetMRChildValues(mrChilds, cii);
                                });
                                ybc.ChildItems = CommonFunction.DeepClone(bookingChildItems);
                            }

                            if (yb.SubGroupID == 1)
                            {
                                ybc.FinishFabricUtilizationPopUpList = this.GetUtilizationPopUpList(ybc, c, yb.Childs, yb.SubGroupID);
                                ybc.GreyFabricUtilizationPopUpList = this.GetGreyUtilizationPopUpList(ybc, c, yb.Childs, yb.SubGroupID);
                            }
                            List<BulkBookingGreyYarnUtilization> greyYarnUTListSave = new List<BulkBookingGreyYarnUtilization>();
                            List<BulkBookingDyedYarnUtilization> dyedYarnUTListSave = new List<BulkBookingDyedYarnUtilization>();
                            c.ChildItems.ForEach(ci =>
                            {
                                List<BulkBookingGreyYarnUtilization> greyYarnUtilizationListFromDB = new List<BulkBookingGreyYarnUtilization>();
                                List<BulkBookingDyedYarnUtilization> dyedYarnUtilizationListFromDB = new List<BulkBookingDyedYarnUtilization>();
                                ybc.ChildItems.ForEach(gy =>
                                {

                                    var greyYarnUtilizationList = gy.GreyYarnUtilizationPopUpList.Where(xgy => xgy.YBChildItemID == ci.YBChildItemID).ToList();
                                    if (greyYarnUtilizationList.Count > 0)
                                    {
                                        greyYarnUtilizationListFromDB.AddRange(CommonFunction.DeepClone(greyYarnUtilizationList));
                                    }

                                    var dyedYarnUtilizationList = gy.DyedYarnUtilizationPopUpList.Where(xgy => xgy.YBChildItemID == ci.YBChildItemID).ToList();
                                    if (dyedYarnUtilizationList.Count > 0)
                                    {
                                        dyedYarnUtilizationListFromDB.AddRange(CommonFunction.DeepClone(dyedYarnUtilizationList));
                                    }


                                });
                                greyYarnUtilizationListFromDB.SetUnchanged();
                                List<BulkBookingGreyYarnUtilization> tempGreyYarnUTList = new List<BulkBookingGreyYarnUtilization>();

                                tempGreyYarnUTList = this.GetGreyYarnUtilizationPopUpList(greyYarnUtilizationListFromDB, ci, yb.SubGroupID, ybc.YBChildID, yb.YBookingID, ci.YBChildItemID);
                                greyYarnUTListSave.AddRange(CommonFunction.DeepClone(tempGreyYarnUTList));

                                dyedYarnUtilizationListFromDB.SetUnchanged();
                                List<BulkBookingDyedYarnUtilization> tempDyedYarnUTList = new List<BulkBookingDyedYarnUtilization>();

                                tempDyedYarnUTList = this.GetDyedYarnUtilizationPopUpList(dyedYarnUtilizationListFromDB, ci, yb.SubGroupID, ybc.YBChildID, yb.YBookingID, ci.YBChildItemID);
                                dyedYarnUTListSave.AddRange(CommonFunction.DeepClone(tempDyedYarnUTList));

                            });


                            ybc.ChildItems.ForEach(gy =>
                            {
                                var greyYarnUtilizationList = greyYarnUTListSave.Where(xgy => xgy.YBChildItemID == gy.YBChildItemID).ToList();
                                if (greyYarnUtilizationList.Count > 0)
                                    gy.GreyYarnUtilizationPopUpList = CommonFunction.DeepClone(greyYarnUtilizationList);

                                var dyedYarnUtilizationList = dyedYarnUTListSave.Where(xgy => xgy.YBChildItemID == gy.YBChildItemID).ToList();
                                if (dyedYarnUtilizationList.Count > 0)
                                    gy.DyedYarnUtilizationPopUpList = CommonFunction.DeepClone(dyedYarnUtilizationList);

                            });

                            if (yb.SubGroupID != 1)
                            {
                                var objU = CommonFunction.DeepClone(model.FBookingChild.Find(x => x.BookingChildID == ybc.BookingChildID));
                                if (objU.IsNotNull())
                                {
                                    ybc.FinishFabricUtilizationPopUpList = objU.FinishFabricUtilizationPopUpList;
                                    ybc.GreyFabricUtilizationPopUpList = objU.GreyFabricUtilizationPopUpList;
                                }
                            }
                            yarnBookingChilds.Add(ybc);
                            loopIndex++;
                        }
                    }
                });

            }
            if (model.IsYarnRevision)
            {
                List<YarnBookingChildItemRevision> yarnBookingChildItemsRevision = GetYarnRevisedChildItems(yarnBookingChildItems);
                List<YarnBookingChild> yarnBookingChildsRevision = GetYarnRevisedChilds(yarnBookingChilds);
                await _service.UpdateBulkStatusYarnRevision(AppUser.UserCode, entities, new List<YarnBookingChildItemRevision>(), new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            }
            else
            {
                await _service.UpdateBulkStatus2(AppUser.UserCode, entities, new List<YarnBookingChildItem>(), new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            }
            //await _service.UpdateBulkStatus2(entities, new List<YarnBookingChildItem>(), new List<YarnBookingMaster>(), yarnBookingChilds, mrChilds);
            return Ok(entities);
        }
        [Route("bulk/approvePMC")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> BulkApprovePMC(FBookingAcknowledge model)
        {
            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<YarnBookingMaster> yarnBookings = new List<YarnBookingMaster>();
            List<YarnBookingChildItem> yarnBookingChildItems = new List<YarnBookingChildItem>();
            List<FreeConceptMRChild> mrChilds = new List<FreeConceptMRChild>();
            bool isYarnRevised = false;

            if (model.FBAckID > 0 && model.BookingNo.IsNotNullOrEmpty())
            {
                DateTime? approvedDatePMC = null;

                entities = await _service.GetFBAcknowledgeMasterBulkWithRevision(model.BookingNo);
                entities.ForEach(entity =>
                {
                    entity.EntityState = EntityState.Modified;
                    entity.RevisionNoBBKI = entity.PreRevisionNoBBKI;
                    entity.IsReviseBBKI = false;
                    if (model.IsInternalRevise)
                    {
                        entity.IsApprovedByPMC = false;
                        entity.ApprovedByPMC = 0;
                        entity.ApprovedDatePMC = null;

                        entity.IsRejectByPMC = false;

                        entity.IsInternalRevise = true;
                        entity.InternalReviseBy = AppUser.UserCode;
                        entity.InternalReviseDate = DateTime.Now;
                        entity.InternalReviseReason = model.InternalReviseReason;
                    }
                    else if (model.IsRejectByPMC)
                    {
                        entity.IsApprovedByPMC = false;
                        entity.ApprovedByPMC = 0;
                        entity.ApprovedDatePMC = null;

                        entity.IsRejectByPMC = true;
                        entity.RejectByPMC = AppUser.UserCode;
                        entity.RejectDatePMC = DateTime.Now;
                        entity.RejectReasonPMC = model.RejectReasonPMC;

                        entity.IsInternalRevise = false;
                    }
                    else
                    {
                        entity.IsApprovedByPMC = true;
                        entity.ApprovedByPMC = AppUser.UserCode;

                        if (approvedDatePMC == null)
                        {
                            approvedDatePMC = DateTime.Now;
                        }
                        entity.ApprovedDatePMC = approvedDatePMC;

                        entity.IsRejectByPMC = false;
                        entity.RejectByPMC = 0;
                        entity.RejectDatePMC = null;
                    }

                    entity.YarnBookings.ForEach(yb =>
                    {
                        yb.Propose = true;
                        yb.ProposeDate = DateTime.Now;
                        yb.EntityState = EntityState.Modified;
                        if (entity.IsApprovedByPMC == true)
                        {
                            yb.PMCFinalApproveCount++;
                        }

                        if (yb.IsRevised && !isYarnRevised && yb.PMCFinalApproveCount > 1)
                        {
                            isYarnRevised = true;
                        }

                        if (model.IsYarnRevision == true && yb.PMCFinalApproveCount > 1)
                        {
                            yb.YarnBookingRevisionTypeID = (int)YarnBookingRevisionTypeEnum.YarnInternalRevision;
                        }
                        else if (model.IsYarnRevision == false && yb.PMCFinalApproveCount > 1)
                        {
                            yb.YarnBookingRevisionTypeID = (int)YarnBookingRevisionTypeEnum.FabricRevision;
                        }
                        //yb.Acknowledge = true;
                        //yb.AcknowledgeDate = DateTime.Now;

                        yb.IsRevised = false;
                        //yb.RevisionNo = yb.PreProcessRevNo; //Internal Yarn Revision

                        if (yb.YRequiredDate.IsNull()) yb.YRequiredDate = DateTime.Now;

                        if (yb.PMCFinalApproveCount == 1 && entity.IsApprovedByPMC == true)
                        {
                            yb.YBookingDate = DateTime.Now;
                        }

                        yarnBookings.Add(yb);
                    });
                });

                #region Extend Model
                var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);
                yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByBookingNoWithRevision(model.BookingNo);

                var collarListModel = model.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                var cuffListModel = model.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                var yarnItemsCollar = yarnBookingChildItems.Where(x => x.SubGroupId == 11).ToList();
                var yarnItemsCuff = yarnBookingChildItems.Where(x => x.SubGroupId == 12).ToList();

                List<FBookingAcknowledgeChild> collarList = this.GetExtendChilds(collarListModel, collarsEntity, yarnItemsCollar, model);
                List<FBookingAcknowledgeChild> cuffList = this.GetExtendChilds(cuffListModel, cuffsEntity, yarnItemsCuff, model);

                model.FBookingChild = model.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                model.FBookingChild.AddRange(collarList);
                model.FBookingChild.AddRange(cuffList);

                model.FBookingChild = CommonFunction.DeepClone(model.FBookingChild);

                model.ChildItems = model.ChildItems.Where(x => x.SubGroupId == 1).ToList();
                collarList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));
                cuffList.ForEach(x => model.ChildItems.AddRange(x.ChildItems));

                #endregion

                if (model.BookingNo.IsNotNullOrEmpty() && model.ChildItems.Count() > 0)
                {
                    mrChilds = await _fcMRService.GetMRChildByBookingNoWithRevision(model.BookingNo);
                    mrChilds.SetModified();

                    string sYBChildItemIDs = string.Join(",", model.ChildItems.Select(x => x.YBChildItemID));
                    yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsWithRevision(sYBChildItemIDs);
                    model.ChildItems.ForEach(ci =>
                    {
                        var objList = yarnBookingChildItems.Where(x => x.YBChildItemID == ci.YBChildItemID).ToList();
                        if (objList.Count > 0)
                        {
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).ItemMasterID = ci.YItemMasterID;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).YarnCategory = ci.YarnCategory;

                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).Remarks = ci.Remarks;
                            yarnBookingChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID).EntityState = EntityState.Modified;

                            mrChilds = this.SetMRChildValues(mrChilds, ci);
                        }
                    });
                }
                if (entities.FirstOrDefault().IsAllocationInternalRevise == true)
                {
                    yarnBookings.ForEach(m =>
                    {
                        m.IsForAllocationRevise = false;
                        m.PreProcessRevNo = m.PreProcessRevNo + 1;
                    });
                }
            }

            if (model.IsYarnRevision)
            {
                List<YarnBookingChildItemRevision> yarnBookingChildItemsRevision = GetYarnRevisedChildItems(yarnBookingChildItems);
                await _service.UpdateBulkStatusYarnRevision(AppUser.UserCode, entities, yarnBookingChildItemsRevision, yarnBookings, new List<YarnBookingChild>(), mrChilds, true, model.IsRejectByPMC);
            }
            else
            {
                await _service.UpdateBulkStatus(entities, yarnBookingChildItems, yarnBookings, new List<YarnBookingChild>(), mrChilds, isYarnRevised, entities.First().IsApprovedByPMC, entities.First().IsRejectByPMC, AppUser.UserCode);//AppUser.UserCode
            }
            Boolean IsSendMail = false;
            string BookingNo = model.BookingNo;
            string YBookingNo = model.YBookingNo;
            Boolean IsYarnRevision = model.IsYarnRevision;
            string BuyerName = model.BuyerName;
            string BuyerTeam = model.BuyerTeamName;
            int RevisionNo = model.RevisionNo;


            if (model.IsRejectByPMC)
            {

                List<FBookingAcknowledge> fbackDataList = await _service.GetItsSampleOrNot(BookingNo);

                bool WithoutOB = true;
                bool IsSample = true;
                IsSendMail = true;
                string SaveType = "UA";
                int BookingID = 0;
                if (fbackDataList.Count > 0)
                {
                    WithoutOB = CommonFunction.DeepClone(fbackDataList.First().WithoutOB);
                    IsSample = CommonFunction.DeepClone(fbackDataList.First().IsSample);
                    BookingID = CommonFunction.DeepClone(fbackDataList.First().BookingID);
                }
                List<BookingItemAcknowledge> saveBookingItemAcknowledgeList = new List<BookingItemAcknowledge>();

                List<BookingChild> updatedDataNew = new List<BookingChild>();

                //OFF FOR CORE//FBookingAcknowledgeController FBAckCtrl = new FBookingAcknowledgeController(_emailService, _reportingService, _fbaService, _fcService, _fcMRService, _commonService);

                if (BookingNo.IsNotNullOrEmpty())
                {
                    if (!WithoutOB)
                    {
                        updatedDataNew = await _fbaService.GetAllInHouseBookingByBookingNo(BookingNo);

                    }
                    else
                    {
                        updatedDataNew = await _fbaService.GetAllInHouseSampleBookingByBookingNo(BookingNo);

                    }
                }
                String selectedbookingID = String.Empty;
                var strArr = updatedDataNew.Select(i => i.BookingId.ToString()).Distinct().ToArray();
                selectedbookingID += string.Join(",", strArr.ToArray());

                String strSql1 = String.Format(@"Update FBookingAcknowledge Set IsUnAcknowledge=1,UnAcknowledgeBy={0},UnAcknowledgeDate=getdate(),UnAcknowledgeReason='{2}' Where  BookingID in ({1});
                                                Update FabricBookingAcknowledge set
                                                                Status = 0,
                                                                UnAcknowledgeBy ={0},
                                                                UnAcknowledgeDate = Cast(GETDATE() As date),
                                                                UnAcknowledge = 1
                                                Where BookingID in ({1});", AppUser.UserCode, selectedbookingID, model.RejectReasonPMC);
                await _fbaService.UnAckFabricBooking(strSql1);


                if (!WithoutOB)
                {
                    List<BookingMaster> bmList = new List<BookingMaster>();
                    BookingMaster bm = await _fbaService.GetAllBookingAsync(BookingID);
                    if (bm.IsNotNull())
                        bmList.Add(bm);

                    saveBookingItemAcknowledgeList = await _fbaService.GetAllBookingItemAcknowledgeByBookingNo(BookingNo);
                    //OFF FOR CORE//IsSendMail = await FBAckCtrl.SystemMailUnAck(saveBookingItemAcknowledgeList, bmList, IsSendMail, SaveType, false, "");
                }
                else
                {
                    List<SampleBookingMaster> bmList = new List<SampleBookingMaster>();
                    SampleBookingMaster bm = await _fbaService.GetAllAsync(BookingID);
                    if (bm.IsNotNull())
                        bmList.Add(bm);

                    saveBookingItemAcknowledgeList = await _fbaService.GetAllBookingItemAcknowledgeByBookingIDAndWithOutOB(selectedbookingID == "" ? "0" : selectedbookingID);
                    //OFF FOR CORE//IsSendMail = await FBAckCtrl.SystemMailForSampleUnAck(saveBookingItemAcknowledgeList, bmList, IsSendMail, SaveType, false, "");


                }
            }
            else
            {
                //OFF FOR CORE//IsSendMail = await SystemMail(BookingNo, YBookingNo, IsYarnRevision, BuyerName, BuyerTeam, RevisionNo, IsSendMail);
            }

            return Ok(entities);
        }
        #endregion


        [HttpGet]
        [Route("bulk/finishFabricUtilization")]
        //[Route("bulk/finishFabricUtilization/{GSM}/{Composition}")]
        public async Task<IActionResult> GetFinishFabricUtilizationByGSMAndComposition(string GSMId, string GSM, string CompositionId, string ConstructionId, string SubGroupID)
        {

            var paginationInfo = Request.GetPaginationInfo();
            List<BulkBookingFinishFabricUtilization> records = await _service.GetFinishFabricUtilizationByGSMAndCompositionAsync(GSMId, GSM, CompositionId, ConstructionId, SubGroupID, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));


            //BulkBookingFinishFabricUtilization data = await _service.GetFinishFabricUtilizationByGSMAndCompositionAsync(GSM, Composition);
            //return Ok(data);
        }

        [HttpGet]
        [Route("bulk/finishFabricUtilization")]
        public async Task<IActionResult> GetFinishFabricUtilizationByYBChildID(int YBChildID)
        {

            List<BulkBookingFinishFabricUtilization> records = await _service.GetFinishFabricUtilizationByYBChildID(YBChildID);
            return Ok(new TableResponseModel(records));


            //BulkBookingFinishFabricUtilization data = await _service.GetFinishFabricUtilizationByYBChildID(YBChildID);
            //return Ok(data);
        }




        [HttpGet]
        [Route("bulk/GetGreyFabricUtilizationItem")]
        public async Task<IActionResult> GetGreyFabricUtilizationItem(string GSMId, string GSM, string CompositionId, string ConstructionId, string SubGroupID)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetGreyFabricUtilizationItem(GSMId, GSM, CompositionId, ConstructionId, SubGroupID, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("bulk/GetDyedYarnUtilizationItem")]
        public async Task<IActionResult> GetDyedYarnUtilizationItem(string GSMId, string CompositionId, string ConstructionId, string SubGroupID)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetDyedYarnUtilizationItem(GSMId, CompositionId, ConstructionId, SubGroupID, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("bulk/GetGreyYarnUtilizationItem")]
        public async Task<IActionResult> GetGreyYarnUtilizationItem(string ItemMasterId)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetGreyYarnUtilizationItem(ItemMasterId, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }


        [HttpGet]
        [Route("ForAckRevisionPendingValidation/{BookingNo}/{ExportOrderID}")]
        public async Task<String> FBookingAckRevisionPendingValidation(string BookingNo, string ExportOrderID)
        {
            try
            {
                List<FBookingAcknowledge> ValidationList = new List<FBookingAcknowledge>();
                ValidationList = await _fbaService.GetRevMktAckAndRevisionAck(BookingNo, ExportOrderID);
                if (ValidationList.Count > 0)
                {
                    if (ValidationList.First().IsRevMktAck == "Y")
                    {
                        return "M&M acknowledge pedning.";
                    }
                    else if (ValidationList.First().IsRevisionAck == "Y")
                    {
                        return "Fabric booking revision acknowledge pending.";
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message + " Call Stack: " + ex.StackTrace);
            }
        }

        #region Bulk Addition

        [HttpGet]
        [Route("bulk/addition/{bookingNo}/{isSample}/{yBookingNo}/{isSavedAddition}/{isAllowYBookingNo}/{isFromYBAck}")]
        public async Task<IActionResult> GetDataByBookingNoAddition(string bookingNo, bool isSample, string yBookingNo, bool isSavedAddition, bool isAllowYBookingNo, bool isFromYBAck)
        {
            FBookingAcknowledge data = await _service.GetDataByYBookingNo(bookingNo, isSample, true, yBookingNo, isSavedAddition, isAllowYBookingNo, false, isFromYBAck);
            return Ok(data);
        }
        [Route("bulk/addition/approveOrReject")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> ApproveOrRejectBulkAddition(FBookingAcknowledge model)
        {
            bool isReject = model.IsReject;
            int paramTypeId = model.ParamTypeId;
            List<YarnBookingMaster> entityYarnBookings = await _serviceYB.GetYarnByNo(model.YBookingNo);
            YarnBookingMaster entityYB = entityYarnBookings.First();
            List<YarnBookingChildItem> yarnBookingChildItems = new List<YarnBookingChildItem>();
            entityYB.EntityState = EntityState.Modified;

            entityYB.Childs.ForEach(x =>
            {
                x.EntityState = EntityState.Unchanged;
                x.ChildItems.SetUnchanged();
                x.AdditionalReplacementPOPUPList.SetUnchanged();
            });

            bool isQtyMoreThan100 = false;
            if (!isReject)
            {//--
                #region Extend Model
                var bookingChildEntity = await _service.GetYBForBulkAsync(model.BookingNo);
                yarnBookingChildItems = await _serviceYB.GetYanBookingChildItemsByBookingNoWithRevision(model.BookingNo);

                var collarListModel = model.FBookingChild.Where(x => x.SubGroupID == 11).ToList();
                var cuffListModel = model.FBookingChild.Where(x => x.SubGroupID == 12).ToList();

                var collarsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 11).ToList();
                var cuffsEntity = bookingChildEntity.Childs.Where(x => x.SubGroupID == 12).ToList();

                List<FBookingAcknowledgeChild> collarList = this.GetExtendChildsAddition(collarListModel, collarsEntity, yarnBookingChildItems, model, true);
                List<FBookingAcknowledgeChild> cuffList = this.GetExtendChildsAddition(cuffListModel, cuffsEntity, yarnBookingChildItems, model, true);

                model.FBookingChild = model.FBookingChild.Where(x => x.SubGroupID == 1).ToList();
                model.FBookingChild.AddRange(collarList);
                model.FBookingChild.AddRange(cuffList);

                model.FBookingChild = CommonFunction.DeepClone(model.FBookingChild);

                #endregion

                model.FBookingChild.ForEach(fbc =>
                {
                    //int indexYBC = entityYB.Childs.FindIndex(c => c.BookingChildID == fbc.BookingChildID);
                    int indexYBC = entityYB.Childs.FindIndex(c => c.Construction == fbc.Construction && c.Composition == fbc.Composition && c.Color == fbc.Color);
                    YarnBookingChild entityYBChild = entityYB.Childs[indexYBC];
                    entityYBChild.EntityState = EntityState.Modified;

                    /*if (fbc.IsForFabric)
                    {
                        entityYBChild.IsForFabric = true;
                        //entityYBChild.ChildItems = new List<YarnBookingChildItem>();
                        entityYBChild.BookingQty = fbc.BookingQty;
                        if (fbc.BookingQty > 100) isQtyMoreThan100 = true;
                        entityYBChild.EntityState = EntityState.Modified;
                    }
                    else
                    {
                        entityYBChild.IsForFabric = false;

                    }*/
                    entityYBChild.YarnAllowance = fbc.YarnAllowance;
                    entityYBChild.IsForFabric = fbc.IsForFabric;
                    //entityYBChild.ChildItems = new List<YarnBookingChildItem>();
                    entityYBChild.BookingQty = fbc.BookingQty;
                    entityYBChild.QtyInKG = fbc.BookingQtyKG;
                    entityYBChild.GreyReqQty = fbc.GreyReqQty;
                    entityYBChild.GreyLeftOverQty = fbc.GreyLeftOverQty;
                    entityYBChild.GreyProdQty = fbc.GreyProdQty;
                    entityYBChild.FinishFabricUtilizationQty = fbc.FinishFabricUtilizationQty;
                    entityYBChild.ReqFinishFabricQty = fbc.ReqFinishFabricQty;
                    if (fbc.BookingQty > 100) isQtyMoreThan100 = true;

                    entityYBChild.FinishFabricUtilizationQty = fbc.FinishFabricUtilizationQty;
                    entityYBChild.GreyLeftOverQty = fbc.GreyLeftOverQty;
                    entityYBChild.GreyFabricUtilizationPopUpList = CommonFunction.DeepClone(this.GetAdditionGreyUtilizationPopUpListYBC(entityYBChild, fbc));
                    entityYBChild.FinishFabricUtilizationPopUpList = CommonFunction.DeepClone(this.GetAdditionUtilizationPopUpListYBC(entityYBChild, fbc));
                    entityYBChild.AdditionalReplacementPOPUPList = CommonFunction.DeepClone(this.GetAdditionReplacementPopUpListYBC(entityYBChild, fbc));

                    entityYBChild.EntityState = EntityState.Modified;

                    fbc.ChildItems.ForEach(ci =>
                    {
                        YarnBookingChildItem entityYBCI = entityYBChild.ChildItems.Find(x => x.YBChildItemID == ci.YBChildItemID);
                        if (entityYBCI == null)
                        {
                            entityYBCI = ci;
                            entityYBCI.GreyYarnUtilizationPopUpList = GetAdditionGreyYarnUtilizationPopUpList(new List<BulkBookingGreyYarnUtilization>(), ci, fbc.SubGroupID);
                            entityYBCI.DyedYarnUtilizationPopUpList = GetAdditionDyedYarnUtilizationPopUpList(new List<BulkBookingDyedYarnUtilization>(), ci, fbc.SubGroupID);
                            entityYBCI.AdditionalNetReqPOPUPList = CommonFunction.DeepClone(GetAdditionChildItemNetReqQTYPopUpList(new YarnBookingChildItem(), ci));
                            entityYBCI.EntityState = EntityState.Added;
                            entityYBChild.ChildItems.Add(entityYBCI);
                        }
                        else
                        {
                            int indexYBCI = entityYBChild.ChildItems.FindIndex(x => x.YBChildItemID == ci.YBChildItemID);
                            YarnBookingChildItem ChildItemDB = entityYBChild.ChildItems.Where(m => m.YBChildItemID == ci.YBChildItemID).FirstOrDefault();
                            entityYBCI.YarnReqQty = ci.YarnReqQty;
                            if (ci.YarnReqQty > 100) isQtyMoreThan100 = true;
                            entityYBCI.YarnLeftOverQty = ci.YarnLeftOverQty;
                            entityYBCI.NetYarnReqQty = ci.NetYarnReqQty;
                            entityYBCI.YarnBalanceQty = ci.YarnBalanceQty;
                            entityYBCI.Allowance = ci.Allowance;
                            entityYBCI.GreyAllowance = ci.GreyAllowance;
                            entityYBCI.YDAllowance = ci.YDAllowance;
                            entityYBCI.DyedYarnUtilizationQty = ci.DyedYarnUtilizationQty;
                            entityYBCI.GreyYarnUtilizationQty = ci.GreyYarnUtilizationQty;
                            entityYBCI.GreyYarnUtilizationPopUpList = GetAdditionGreyYarnUtilizationPopUpList(ChildItemDB.GreyYarnUtilizationPopUpList, ci, fbc.SubGroupID);
                            entityYBCI.DyedYarnUtilizationPopUpList = GetAdditionDyedYarnUtilizationPopUpList(ChildItemDB.DyedYarnUtilizationPopUpList, ci, fbc.SubGroupID);
                            entityYBCI.AdditionalNetReqPOPUPList = CommonFunction.DeepClone(GetAdditionChildItemNetReqQTYPopUpList(ChildItemDB, ci));
                            entityYBCI.EntityState = EntityState.Modified;

                            entityYBChild.ChildItems[indexYBCI] = entityYBCI;
                        }

                    });
                    entityYB.Childs[indexYBC] = entityYBChild;
                });

                entityYB.Childs.ForEach(yc =>
                {
                    if (yc.EntityState == EntityState.Unchanged)
                    {
                        yc.EntityState = EntityState.Deleted;
                        yc.ChildItems.SetDeleted();
                        yc.ChildItems.ForEach(ci =>
                        {
                            ci.GreyYarnUtilizationPopUpList.SetDeleted();
                            ci.DyedYarnUtilizationPopUpList.SetDeleted();
                        });
                        yc.FinishFabricUtilizationPopUpList.SetDeleted();
                        yc.GreyFabricUtilizationPopUpList.SetDeleted();
                        yc.AdditionalReplacementPOPUPList.SetDeleted();
                    }
                    else
                    {
                        yc.FinishFabricUtilizationPopUpList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                        yc.GreyFabricUtilizationPopUpList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                        yc.AdditionalReplacementPOPUPList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                        //yc.ChildItems.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                        yc.ChildItems.ForEach(ci =>
                        {
                            if (ci.EntityState == EntityState.Unchanged)
                            {
                                ci.EntityState = EntityState.Deleted;
                                ci.GreyYarnUtilizationPopUpList.SetDeleted();
                                ci.DyedYarnUtilizationPopUpList.SetDeleted();
                                ci.AdditionalNetReqPOPUPList.SetDeleted();
                            }
                            else
                            {
                                ci.GreyYarnUtilizationPopUpList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                                ci.DyedYarnUtilizationPopUpList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                                ci.AdditionalNetReqPOPUPList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                            }
                        });

                    }
                });
            }

            #region Status Update

            entityYarnBookings.ForEach(YB =>
            {
                YB.EntityState = EntityState.Modified;
                switch (paramTypeId)
                {
                    case (int)ParamTypeId.AYBQtyFinalizationPMC:
                        if (isReject)
                        {
                            YB.IsQtyFinalizationPMCReject = true;
                            YB.QtyFinalizationPMCRejectBy = AppUser.UserCode;
                            YB.QtyFinalizationPMCRejectDate = DateTime.Now;
                            YB.QtyFinalizationPMCRejectReason = model.QtyFinalizationPMCRejectReason;

                            YB.IsQtyFinalizationPMCApprove = false;
                            YB.QtyFinalizationPMCApproveBy = 0;
                            YB.QtyFinalizationPMCApproveDate = null;
                        }
                        else
                        {
                            YB.IsQtyFinalizationPMCApprove = true;
                            YB.QtyFinalizationPMCApproveBy = AppUser.UserCode;
                            YB.QtyFinalizationPMCApproveDate = DateTime.Now;

                            YB.IsQtyFinalizationPMCReject = false;
                            YB.QtyFinalizationPMCRejectBy = 0;
                            YB.QtyFinalizationPMCRejectDate = null;
                            YB.QtyFinalizationPMCRejectReason = "";
                        }
                        break;
                    case (int)ParamTypeId.AYBProdHeadApproval:
                        if (isReject)
                        {
                            YB.IsProdHeadReject = true;
                            YB.ProdHeadRejectBy = AppUser.UserCode;
                            YB.ProdHeadRejectDate = DateTime.Now;
                            YB.ProdHeadRejectReason = model.ProdHeadRejectReason;

                            YB.IsProdHeadApprove = false;
                            YB.ProdHeadApproveBy = 0;
                            YB.ProdHeadApproveDate = null;
                        }
                        else
                        {
                            YB.IsProdHeadApprove = true;
                            YB.ProdHeadApproveBy = AppUser.UserCode;
                            YB.ProdHeadApproveDate = DateTime.Now;

                            YB.IsProdHeadReject = false;
                            YB.ProdHeadRejectBy = 0;
                            YB.ProdHeadRejectDate = null;
                            YB.ProdHeadRejectReason = "";

                            if (!isQtyMoreThan100) //If Less than 100 then auti textile head approve
                            {
                                YB.IsTextileHeadApprove = true;
                            }
                        }
                        break;
                    case (int)ParamTypeId.AYBTextileHeadApproval:
                        if (isReject)
                        {
                            YB.IsTextileHeadReject = true;
                            YB.TextileHeadRejectBy = AppUser.UserCode;
                            YB.TextileHeadRejectDate = DateTime.Now;
                            YB.TextileHeadRejectReason = model.TextileHeadRejectReason;

                            YB.IsTextileHeadApprove = false;
                            YB.TextileHeadApproveBy = 0;
                            YB.TextileHeadApproveDate = null;
                        }
                        else
                        {
                            YB.IsTextileHeadApprove = true;
                            YB.TextileHeadApproveBy = AppUser.UserCode;
                            YB.TextileHeadApproveDate = DateTime.Now;

                            YB.IsTextileHeadReject = false;
                            YB.TextileHeadRejectBy = 0;
                            YB.TextileHeadRejectDate = null;
                            YB.TextileHeadRejectReason = "";
                        }
                        break;
                    case (int)ParamTypeId.AYBKnittingUtilization:
                        if (isReject)
                        {
                            YB.IsKnittingUtilizationReject = true;
                            YB.KnittingUtilizationRejectBy = AppUser.UserCode;
                            YB.KnittingUtilizationRejectDate = DateTime.Now;
                            YB.KnittingUtilizationRejectReason = model.KnittingUtilizationRejectReason;

                            YB.IsKnittingUtilizationApprove = false;
                            YB.KnittingUtilizationApproveBy = 0;
                            YB.KnittingUtilizationApproveDate = null;
                        }
                        else
                        {
                            YB.IsKnittingUtilizationApprove = true;
                            YB.KnittingUtilizationApproveBy = AppUser.UserCode;
                            YB.KnittingUtilizationApproveDate = DateTime.Now;

                            YB.IsKnittingUtilizationReject = false;
                            YB.KnittingUtilizationRejectBy = 0;
                            YB.KnittingUtilizationRejectDate = null;
                            YB.KnittingUtilizationRejectReason = "";

                        }
                        break;
                    case (int)ParamTypeId.AYBKnittingHeadApproval:
                        if (isReject)
                        {
                            YB.IsKnittingHeadReject = true;
                            YB.KnittingHeadRejectBy = AppUser.UserCode;
                            YB.KnittingHeadRejectDate = DateTime.Now;
                            YB.KnittingHeadRejectReason = model.KnittingHeadRejectReason;

                            YB.IsKnittingHeadApprove = false;
                            YB.KnittingHeadApproveBy = 0;
                            YB.KnittingHeadApproveDate = null;
                        }
                        else
                        {
                            YB.IsKnittingHeadApprove = true;
                            YB.KnittingHeadApproveBy = AppUser.UserCode;
                            YB.KnittingHeadApproveDate = DateTime.Now;

                            YB.IsKnittingHeadReject = false;
                            YB.KnittingHeadRejectBy = 0;
                            YB.KnittingHeadRejectDate = null;
                            YB.KnittingHeadRejectReason = "";

                            YB.IsOperationHeadApprove = true;
                            YB.OperationHeadApproveBy = AppUser.UserCode;
                            YB.OperationHeadApproveDate = DateTime.Now;
                        }
                        break;
                    case (int)ParamTypeId.AYBOperationHeadApproval:
                        if (isReject)
                        {
                            YB.IsOperationHeadReject = true;
                            YB.OperationHeadRejectBy = AppUser.UserCode;
                            YB.OperationHeadRejectDate = DateTime.Now;
                            YB.OperationHeadRejectReason = model.OperationHeadRejectReason;

                            YB.IsOperationHeadApprove = false;
                            YB.OperationHeadApproveBy = 0;
                            YB.OperationHeadApproveDate = null;
                        }
                        else
                        {
                            YB.IsOperationHeadApprove = true;
                            YB.OperationHeadApproveBy = AppUser.UserCode;
                            YB.OperationHeadApproveDate = DateTime.Now;

                            YB.IsOperationHeadReject = false;
                            YB.OperationHeadRejectBy = 0;
                            YB.OperationHeadRejectDate = null;
                            YB.OperationHeadRejectReason = "";
                        }
                        break;
                    default:
                        // code block
                        break;
                }
            });
            #endregion

            await _service.ApproveOrRejectBulkAddition(entityYB, entityYarnBookings);
            if (paramTypeId == (int)ParamTypeId.AYBOperationHeadApproval)
            {
                Boolean IsSendMail = false;
                string BookingNo = model.BookingNo;
                string YBookingNo = model.YBookingNo;
                Boolean IsYarnRevision = model.IsYarnRevision;
                string BuyerName = model.BuyerName;
                string BuyerTeam = model.BuyerTeamName;
                int RevisionNo = model.RevisionNo;

                //OFF FOR CORE//IsSendMail = await SystemMail(BookingNo, YBookingNo, IsYarnRevision, BuyerName, BuyerTeam, RevisionNo, IsSendMail);
            }
            return Ok();
        }
        #endregion

        #region Labdip
        [HttpGet]
        [Route("new/labDip/{bookingId}")]
        public async Task<IActionResult> GetNewLabDip(int bookingId)
        {
            FBookingAcknowledge data = await _service.GetNewAsyncLabdip(bookingId);
            return Ok(data);
        }
        [HttpGet]
        [Route("labDip/{bookingId}/{isRnD}")]
        public async Task<IActionResult> GetDataLabdipAsync(int bookingId, bool isRnD)
        {
            FBookingAcknowledge data = await _service.GetDataLabdipAsync(bookingId, isRnD);
            return Ok(data);
        }
        [HttpGet]
        [Route("labDip/acknowledgedData/{bookingId}/{isRnD}")]
        public async Task<IActionResult> GetDataLabdipAcknowledgedDataAsync(int bookingId, bool isRnD)
        {
            FBookingAcknowledge data = await _service.GetDataLabdipAcknowledgedDataAsync(bookingId, isRnD);
            return Ok(data);
        }
        [HttpGet]
        [Route("labDip/revision/{bookingId}")]
        public async Task<IActionResult> GetDataLabdipRevisionAsync(int bookingId)
        {
            FBookingAcknowledge data = await _service.GetDataLabdipRevisionAsync(bookingId);
            return Ok(data);
        }
        [Route("labDip/acknowledge")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> LabdipAcknowledge(FBookingAcknowledge modelDynamic)
        {
            FBookingAcknowledge model = modelDynamic;
            bool isLabdipAcknowledge = model.IsLabdipAcknowledge;
            bool isRevised = model.IsRevised;

            SampleBookingMaster sampleBooking = new SampleBookingMaster();
            if (model.BookingID > 0)
            {
                sampleBooking = await _service.GetSampleBooking(model.BookingID);
                if (isLabdipAcknowledge)
                {
                    sampleBooking.LabdipAcknowledge = true;
                    sampleBooking.LabdipAcknowledgeBY = AppUser.UserCode;
                    sampleBooking.LabdipAcknowledgeDate = DateTime.Now;

                    sampleBooking.LabdipUnAcknowledge = false;
                    sampleBooking.LabdipUnAcknowledgeBY = 0;
                    sampleBooking.LabdipUnAcknowledgeDate = null;
                }
                else
                {
                    sampleBooking.LabdipAcknowledge = false;
                    sampleBooking.LabdipAcknowledgeBY = 0;
                    sampleBooking.LabdipAcknowledgeDate = null;

                    sampleBooking.LabdipUnAcknowledge = true;
                    sampleBooking.LabdipUnAcknowledgeBY = AppUser.UserCode;
                    sampleBooking.LabdipUnAcknowledgeDate = DateTime.Now;
                    sampleBooking.LabdipUnAcknowledgeReason = model.LabdipUnAcknowledgeReason;
                }
                sampleBooking.LabdipRevisionNo = sampleBooking.RevisionNo;
                sampleBooking.EntityState = EntityState.Modified;

                if (isRevised)
                {
                    sampleBooking.RevisionNo = sampleBooking.RevisionNo + 1;
                    sampleBooking.RevisionDate = DateTime.Now;
                }
                //////////////////////////Start Check and update if revision pending///////////////////////

                //FBookingAcknowledge entity = new FBookingAcknowledge();
                //if (model.FBAckID > 0)
                //{
                //    entity = await _service.GetFBAcknowledgeWithChilds(model.FBAckID);
                //    if (entity.PreRevisionNoLabdip != sampleBooking.RevisionNo)
                //    {
                //        entity.EntityState = EntityState.Modified;

                //        entity.FBookingChild.SetUnchanged();


                //        entity.PreRevisionNoLabdip = sampleBooking.RevisionNo;
                //        entity.RevisionNoLabdip = entity.RevisionNoLabdip + 1;
                //        entity.RevisionDate = DateTime.Now;

                //        model.FBookingChild.ForEach(mChild =>
                //        {
                //            FBookingAcknowledgeChild eChild = entity.FBookingChild.Find(x => x.BookingChildID == mChild.BookingChildID);
                //            if (mChild.BookingChildID == 0 || eChild == null)
                //            {
                //                eChild = CommonFunction.DeepClone(mChild);
                //                eChild.EntityState = EntityState.Added;
                //                entity.FBookingChild.Add(eChild);
                //            }
                //            else
                //            {
                //                if (mChild.IsForFabric)
                //                {
                //                    eChild.BookingQty = mChild.BookingQty;
                //                    eChild.EntityState = EntityState.Modified;
                //                }
                //                else
                //                {
                //                    eChild.EntityState = EntityState.Deleted;
                //                }
                //            }
                //        });

                //        await _service.UpdateFBookingAck(entity);
                //    }
                //}
                //else
                //{
                //    String WithoutOB = modelDynamic.WithoutOB == true ? "1".ToString() : "0";
                //    String BookingID = modelDynamic.BookingID.ToString();
                //    String BookingNo = modelDynamic.BookingNo.ToString();
                //    List<BookingChild> updatedDataNew = new List<BookingChild>();
                //    List<BookingMaster> bookingMasters = new List<BookingMaster>();

                //    if (BookingNo.IsNotNullOrEmpty())
                //    {
                //        //if (WithoutOB == "0")
                //        //{
                //            updatedDataNew = await _FBAservice.GetAllInHouseBookingByBookingNo(BookingNo);
                //            bookingMasters = await _FBAservice.GetBookingMasterByNo(BookingNo);
                //        //}
                //        //else
                //        //{
                //        //    updatedDataNew = await _FBAservice.GetAllInHouseSampleBookingByBookingNo(BookingNo);
                //        //}
                //    }
                //    String selectedbookingID = String.Empty;
                //    var strArr = updatedDataNew.Select(i => i.BookingId.ToString()).Distinct().ToArray();
                //    selectedbookingID += string.Join(",", strArr.ToArray());
                //    FabricBookingAcknowledge savedFBA = await _FBAservice.GetAllSavedFBAcknowledgeByBookingID(selectedbookingID == "" ? "0" : selectedbookingID, isRevised);
                //    #region SaveFBookingAcknowledge
                //    List<FBookingAcknowledge> saveFBookingAcknowledge = savedFBA.FBookingAcknowledgeList;
                //    if (saveFBookingAcknowledge.IsNull())
                //        saveFBookingAcknowledge = new List<FBookingAcknowledge>();
                //    #region Delete Existing Data

                //    #endregion
                //    foreach (FBookingAcknowledge fba in modelDynamic.FBookingAcknowledgeList)
                //    {
                //        FBookingAcknowledge fbMaster = saveFBookingAcknowledge.Find(i => i.BookingID == fba.BookingID && i.ItemGroupId == fba.ItemGroupId && i.SubGroupID == fba.SubGroupID);
                //        fbMaster = await _service.GetFBAcknowledgeByBookingID(BookingID.ToInt());


                //            fbMaster = new FBookingAcknowledge();
                //            fbMaster.AddedBy = AppUser.UserCode;
                //            fbMaster.DateAdded = DateTime.Now;
                //            fbMaster.IsSample = model.IsSample;
                //            fbMaster.PreRevisionNo = bookingMasters != null && bookingMasters.Count() > 0 ? bookingMasters.First().RevisionNo : 0;
                //            fbMaster.RevisionNo = fbMaster.PreRevisionNo;
                //            saveFBookingAcknowledge.Add(fbMaster);

                //        List<FabricBookingAcknowledge> saveFabricBookingItemAcknowledge = savedFBA.FabricBookingAcknowledgeList;
                //        if (saveFabricBookingItemAcknowledge.IsNull())
                //            saveFabricBookingItemAcknowledge = new List<FabricBookingAcknowledge>();
                //        FabricBookingAcknowledge objFabricBookingItemAcknowledge = saveFabricBookingItemAcknowledge.Find(i => i.BookingID == fba.BookingID && i.ItemGroupID == fba.ItemGroupId && i.SubGroupID == fba.SubGroupID);

                //        if (!isRevised)
                //        {
                //            fbMaster.PreRevisionNo = objFabricBookingItemAcknowledge.IsNotNull() ? objFabricBookingItemAcknowledge.PreProcessRevNo : fbMaster.PreRevisionNo;
                //        }

                //        fbMaster.BookingID = fba.BookingID;
                //        fbMaster.BookingNo = modelDynamic.BookingNo;
                //        fbMaster.SLNo = modelDynamic.SLNo;
                //        fbMaster.BookingDate = modelDynamic.BookingDate;
                //        fbMaster.BuyerID = modelDynamic.BuyerID;
                //        fbMaster.BuyerTeamID = modelDynamic.BuyerTeamID;
                //        fbMaster.ExecutionCompanyID = modelDynamic.ExecutionCompanyID;
                //        fbMaster.SupplierID = modelDynamic.SupplierID;
                //        fbMaster.StyleMasterID = modelDynamic.StyleMasterID;
                //        fbMaster.StyleNo = modelDynamic.StyleNo;
                //        fbMaster.SeasonID = modelDynamic.SeasonID;
                //        fbMaster.FinancialYearID = modelDynamic.FinancialYearID;
                //        fbMaster.ExportOrderID = modelDynamic.ExportOrderID;
                //        fbMaster.BookingQty = modelDynamic.BookingQty;
                //        fbMaster.BomMasterId = fba.BomMasterId;
                //        fbMaster.SubGroupID = fba.SubGroupID;
                //        fbMaster.ItemGroupId = fba.ItemGroupId;
                //        fbMaster.MerchandiserID = AppUser.UserCode;
                //        fbMaster.IsUnAcknowledge = true;
                //        fbMaster.UnAcknowledgeBy = AppUser.UserCode;
                //        fbMaster.UnAcknowledgeDate = DateTime.Now;
                //        fbMaster.UnAcknowledgeReason = modelDynamic.UnAcknowledgeReason.Trim();
                //        fbMaster.PreRevisionNoLabdip = sampleBooking.RevisionNo;
                //        fbMaster.RevisionNoLabdip = 0;
                //        fbMaster.RevisionDate = DateTime.Now;

                //    }
                //    _FBAservice.SaveAsync(saveFBookingAcknowledge);
                //    #endregion
                //}
                //////////////////////////Start Check and update if revision pending///////////////////////
                sampleBooking.Childs.SetUnchanged();
                sampleBooking.Childs.ForEach(c =>
                {
                    c.EntityState = EntityState.Modified;
                    c.IsFabricReq = false;
                    c.ConsumptionQty = 0;

                    c.Childs.SetUnchanged();
                });

                foreach (FBookingAcknowledgeChild childObj in model.FBookingChild.Where(x => x.IsFabricReq == true))
                {
                    SampleBookingConsumption consumption = sampleBooking.Childs.Find(x => x.ConsumptionID == childObj.ConsumptionID && x.SubGroupID == childObj.SubGroupID);
                    if (consumption != null)
                    {
                        consumption.IsFabricReq = true;
                        consumption.ConsumptionQty = childObj.BookingQty;
                        consumption.EntityState = EntityState.Modified;

                        if (consumption.Childs.Count() > 0)
                        {
                            consumption.Childs.FirstOrDefault().ConsumptionQty = consumption.ConsumptionQty;
                            int subGroupID = consumption.Childs.FirstOrDefault().SubGroupID;
                            consumption.Childs.FirstOrDefault().RequiredUnitID = subGroupID == 1 ? 28 : 1;
                            consumption.Childs.FirstOrDefault().EntityState = EntityState.Modified;
                        }
                    }
                }
                await _service.UpdateSampleBookingAsync(sampleBooking);
            }
            return Ok(sampleBooking);
        }
        [Route("labDip/formulation/revision")] //revision From formulation menu
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> LabdipRevisionFormulation(FBookingAcknowledge modelDynamic)
        {
            FBookingAcknowledge model = modelDynamic;
            bool isFormulationRevision = modelDynamic.IsRevised;

            FBookingAcknowledge entity = new FBookingAcknowledge();
            if (model.FBAckID > 0)
            {
                entity = await _service.GetFBAcknowledgeWithChilds(model.FBAckID);
                entity.EntityState = EntityState.Modified;

                entity.FBookingChild.SetUnchanged();

                if (isFormulationRevision)
                {
                    entity.PreRevisionNoLabdip++;
                }
                else
                {
                    entity.RevisionNoLabdip = entity.PreRevisionNoLabdip;
                }
                model.FBookingChild.ForEach(mChild =>
                {
                    FBookingAcknowledgeChild eChild = entity.FBookingChild.Find(x => x.BookingChildID == mChild.BookingChildID);
                    if (mChild.BookingChildID == 0 || eChild == null)
                    {
                        eChild = CommonFunction.DeepClone(mChild);
                        eChild.EntityState = EntityState.Added;
                        entity.FBookingChild.Add(eChild);
                    }
                    else
                    {
                        if (mChild.IsForFabric)
                        {
                            eChild.BookingQty = mChild.BookingQty;
                            eChild.EntityState = EntityState.Modified;
                        }
                        else
                        {
                            eChild.EntityState = EntityState.Deleted;
                        }
                    }
                });

                await _service.UpdateFBookingAck(entity);
            }
            return Ok(entity);
        }
        #endregion

        [Route("Receive")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> Receive(SampleBookingMaster model)
        {
            SampleBookingMaster entity;
            entity = await _service.GetAllAsync(model.BookingID);
            entity.SwatchReceive = true;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsync(entity);

            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.SampleBooking, 0, "", entity.BookingID);
            return Ok();
        }

        [Route("Received")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> Received(FreeConceptMaster model)
        {
            FreeConceptMaster entity;
            entity = await _service.GetAllAsyncR(model.BookingID);
            entity.DeliveryComplete = true;
            entity.EntityState = EntityState.Modified;
            await _service.UpdateEntityAsyncR(entity);

            //await _commonService.UpdateFreeConceptStatus(InterfaceFrom.FreeConcept, 0, "", entity.BookingID);
            return Ok();
        }

        [Route("cancel-save")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> CancelSave(FBookingAcknowledge modelDynamic)
        {
            FBookingAcknowledge entity = modelDynamic;// models.FirstOrDefault();
            string grpConceptNo = modelDynamic.grpConceptNo;
            int isBDS = modelDynamic.IsBDS;
            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<BDSDependentTNACalander> BDCalander = new List<BDSDependentTNACalander>();

            var BDSTNAEvent = await _service.GetAllAsyncBDSTNAEvent_HK();

            entity.IsSample = true;
            entity.AddedBy = AppUser.UserCode;

            if (entity.IsUnAcknowledge)
            {
                entity.IsUnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.EmployeeCode;
                entity.UnAcknowledgeDate = DateTime.Now;
                entity.UnAcknowledgeReason = entity.UnAcknowledgeReason;
            }
            if (grpConceptNo != "" && grpConceptNo != null)
            {
                var a = await _fcService.GetDatasAsync(grpConceptNo);
                entities.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.FBookingChildCollor.SetUnchanged();
                    x.FBookingAcknowledgeChildAddProcess.SetUnchanged();
                    x.FBookingAcknowledgeChildGarmentPart.SetUnchanged();
                });
            }
            else
            {
                //FBookingAcknowledgeChild
                List<FBookingAcknowledgeChild> entityChilds = new List<FBookingAcknowledgeChild>();
                foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
                {
                    FBookingAcknowledgeChild ObjEntityChild = new FBookingAcknowledgeChild();
                    ObjEntityChild = item;
                    entityChilds.Add(ObjEntityChild);
                }
                //FBookingAcknowledgeChildAddProcess
                List<FBookingAcknowledgeChildAddProcess> entityChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
                foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
                {
                    FBookingAcknowledgeChildAddProcess ObjEntityChildAddProces = new FBookingAcknowledgeChildAddProcess();
                    ObjEntityChildAddProces.BookingCAddProcessID = 0;
                    ObjEntityChildAddProces.BookingChildID = item.BookingChildID;
                    ObjEntityChildAddProces.BookingID = item.BookingID;
                    ObjEntityChildAddProces.ConsumptionID = item.ConsumptionID;
                    ObjEntityChildAddProces.ProcessID = 0;
                    entityChildAddProcess.Add(ObjEntityChildAddProces);
                }
                //FBookingAcknowledgeChildDetails
                List<FBookingAcknowledgeChildDetails> entityChildDetails = new List<FBookingAcknowledgeChildDetails>();
                foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
                {
                    FBookingAcknowledgeChildDetails ObjEntityChildDetails = new FBookingAcknowledgeChildDetails();
                    ObjEntityChildDetails.BookingCDetailsID = 0;
                    ObjEntityChildDetails.BookingChildID = item.BookingChildID;
                    ObjEntityChildDetails.BookingID = item.BookingID;
                    ObjEntityChildDetails.ConsumptionID = item.ConsumptionID;
                    ObjEntityChildDetails.ItemGroupID = item.ItemGroupID;
                    ObjEntityChildDetails.SubGroupID = item.SubGroupID;
                    ObjEntityChildDetails.ItemMasterID = item.ItemMasterID;
                    ObjEntityChildDetails.OrderBankPOID = item.OrderBankPOID;
                    ObjEntityChildDetails.Color = item.Color;
                    ObjEntityChildDetails.ColorID = item.ColorID;
                    ObjEntityChildDetails.SizeID = item.SizeID;
                    ObjEntityChildDetails.TechPackID = item.TechPackID;
                    ObjEntityChildDetails.ConsumptionQty = item.ConsumptionQty;
                    ObjEntityChildDetails.BookingQty = item.BookingQty;
                    ObjEntityChildDetails.BookingUnitID = item.BookingUnitID;
                    ObjEntityChildDetails.RequisitionQty = item.RequisitionQty;
                    ObjEntityChildDetails.AddedBy = item.AddedBy;
                    ObjEntityChildDetails.ExecutionCompanyID = item.ExecutionCompanyID;
                    ObjEntityChildDetails.TechnicalNameId = item.TechnicalNameId;
                    ObjEntityChildDetails.TechnicalName = item.TechnicalName;
                    ObjEntityChildDetails.DateAdded = item.DateAdded;
                    entityChildDetails.Add(ObjEntityChildDetails);
                }
                //FBookingAcknowledgeChildGarmentPart
                List<FBookingAcknowledgeChildGarmentPart> entityChildsGpart = new List<FBookingAcknowledgeChildGarmentPart>();
                foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
                {
                    FBookingAcknowledgeChildGarmentPart ObjEntityChildsGpart = new FBookingAcknowledgeChildGarmentPart();
                    ObjEntityChildsGpart.BookingCGPID = 0;
                    ObjEntityChildsGpart.BookingChildID = item.BookingChildID;
                    ObjEntityChildsGpart.BookingID = item.BookingID;
                    ObjEntityChildsGpart.ConsumptionID = item.ConsumptionID;
                    ObjEntityChildsGpart.FUPartID = item.FUPartID;
                    entityChildsGpart.Add(ObjEntityChildsGpart);
                }
                //FBookingAcknowledgeChildProcess
                List<FBookingAcknowledgeChildProcess> entityChildsProcess = new List<FBookingAcknowledgeChildProcess>();
                foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
                {
                    FBookingAcknowledgeChildProcess ObjEntityChildsProcess = new FBookingAcknowledgeChildProcess();
                    ObjEntityChildsProcess.BookingCProcessID = 0;
                    ObjEntityChildsProcess.BookingChildID = item.BookingChildID;
                    ObjEntityChildsProcess.BookingID = item.BookingID;
                    ObjEntityChildsProcess.ConsumptionID = item.ConsumptionID;
                    ObjEntityChildsProcess.ProcessID = 0;
                    entityChildsProcess.Add(ObjEntityChildsProcess);
                }
                //FBookingAcknowledgeChildText
                List<FBookingAcknowledgeChildText> entityChildsText = new List<FBookingAcknowledgeChildText>();
                foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
                {
                    FBookingAcknowledgeChildText ObjEntityChildsText = new FBookingAcknowledgeChildText();
                    ObjEntityChildsText.TextID = 0;
                    ObjEntityChildsText.BookingChildID = item.BookingChildID;
                    ObjEntityChildsText.BookingID = item.BookingID;
                    ObjEntityChildsText.ConsumptionID = item.ConsumptionID;
                    entityChildsText.Add(ObjEntityChildsText);
                }
                //FBookingAcknowledgeChildDistribution
                List<FBookingAcknowledgeChildDistribution> entityChildsDistribution = new List<FBookingAcknowledgeChildDistribution>();
                foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
                {
                    FBookingAcknowledgeChildDistribution ObjEntityChildsDistribution = new FBookingAcknowledgeChildDistribution();
                    ObjEntityChildsDistribution.DistributionID = 0;
                    ObjEntityChildsDistribution.BookingChildID = item.BookingChildID;
                    ObjEntityChildsDistribution.BookingID = item.BookingID;
                    ObjEntityChildsDistribution.ConsumptionID = item.ConsumptionID;
                    entityChildsDistribution.Add(ObjEntityChildsDistribution);
                }

                //FBookingAcknowledgeChildYarnSubBrand
                List<FBookingAcknowledgeChildYarnSubBrand> entityChildsYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
                foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
                {
                    FBookingAcknowledgeChildYarnSubBrand ObjEntityChildsYarnSubBrand = new FBookingAcknowledgeChildYarnSubBrand();
                    ObjEntityChildsYarnSubBrand.BookingCYSubBrandID = 0;
                    ObjEntityChildsYarnSubBrand.BookingChildID = item.BookingChildID;
                    ObjEntityChildsYarnSubBrand.BookingID = item.BookingID;
                    ObjEntityChildsYarnSubBrand.ConsumptionID = item.ConsumptionID;
                    ObjEntityChildsYarnSubBrand.YarnSubBrandID = 0;
                    entityChildsYarnSubBrand.Add(ObjEntityChildsYarnSubBrand);
                }
                List<FBookingAcknowledgeImage> entityChildsImage = new List<FBookingAcknowledgeImage>();
                foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
                {
                    FBookingAcknowledgeImage ObjEntityChildsImage = new FBookingAcknowledgeImage();
                    ObjEntityChildsImage.ChildImgID = 0;
                    ObjEntityChildsImage.BookingID = item.BookingID;
                    ObjEntityChildsImage.ExportOrderID = item.ExportOrderID;
                    entityChildsImage.Add(ObjEntityChildsImage);
                }
                entityChildsImage = entityChildsImage.GroupBy(x => x.BookingID).Select(y => y.First()).ToList();

                var fbMaster = new FBookingAcknowledge();
                if (entity.FBAckID > 0)
                {
                    List<FBookingAcknowledgeChild> newEntityChilds = new List<FBookingAcknowledgeChild>();
                    List<FBookingAcknowledgeChildAddProcess> newEntityChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
                    List<FBookingAcknowledgeChildDetails> newEntityChildDetails = new List<FBookingAcknowledgeChildDetails>();
                    List<FBookingAcknowledgeChildGarmentPart> newEntityChildsGpart = new List<FBookingAcknowledgeChildGarmentPart>();
                    List<FBookingAcknowledgeChildProcess> newEntityChildsProcess = new List<FBookingAcknowledgeChildProcess>();
                    List<FBookingAcknowledgeChildText> newEntityChildsText = new List<FBookingAcknowledgeChildText>();
                    List<FBookingAcknowledgeChildDistribution> newEntityChildsDistribution = new List<FBookingAcknowledgeChildDistribution>();
                    List<FBookingAcknowledgeChildYarnSubBrand> newEntityChildsYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
                    List<FBAChildPlanning> newFBAChildPlanning = new List<FBAChildPlanning>();
                    List<BDSDependentTNACalander> newBDCalander = new List<BDSDependentTNACalander>();

                    fbMaster = await _service.GetFBAcknowledge(entity.FBAckID);
                    fbMaster.EntityState = EntityState.Unchanged;
                    fbMaster.FBookingChild.SetUnchanged();
                    fbMaster.FBookingChild.ForEach(x => x.FBAChildPlannings.SetUnchanged());
                    fbMaster.FBookingAcknowledgeChildAddProcess.SetUnchanged();
                    fbMaster.FBookingChildDetails.SetUnchanged();
                    fbMaster.FBookingAcknowledgeChildGarmentPart.SetUnchanged();
                    fbMaster.FBookingAcknowledgeChildProcess.SetUnchanged();
                    fbMaster.FBookingAcknowledgeChildText.SetUnchanged();
                    fbMaster.FBookingAcknowledgeChildDistribution.SetUnchanged();
                    fbMaster.FBookingAcknowledgeChildYarnSubBrand.SetUnchanged();
                    fbMaster.BDSDependentTNACalander.SetUnchanged();
                    fbMaster.FBookingAcknowledgeImage.SetUnchanged();

                    entityChilds.ForEach(bookingChild =>
                    {
                        bookingChild.ChildAddProcess = entityChildAddProcess.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.FBChildDetails = entityChildDetails.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.ChildsGpart = entityChildsGpart.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.ChildsProcess = entityChildsProcess.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.ChildsText = entityChildsText.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.ChildsDistribution = entityChildsDistribution.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.ChildsYarnSubBrand = entityChildsYarnSubBrand.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                        bookingChild.BDCalander = BDCalander.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
                    });
                    int newBookingChildId = 0;
                    foreach (FBookingAcknowledgeChild item in entityChilds)
                    {
                        newFBAChildPlanning = new List<FBAChildPlanning>();

                        item.AcknowledgeID = entity.FBAckID;
                        var fbChild = fbMaster.FBookingChild.Find(x => x.ItemMasterID == item.ItemMasterID && x.SubGroupID == item.SubGroupID);
                        if (fbChild != null)
                        {
                            int bookingChildID = fbChild.BookingChildID;
                            item.BookingChildID = bookingChildID;
                            item.EntityState = EntityState.Modified;
                            item.DateUpdated = DateTime.Now;

                            foreach (var obj in item.ChildAddProcess)
                            {
                                var tempObj = fbMaster.FBookingAcknowledgeChildAddProcess.Find(x => x.BookingChildID == bookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildAddProcess.Add(obj);
                                }
                                else
                                {
                                    var newObj = new FBookingAcknowledgeChildAddProcess();
                                    newObj.BookingCAddProcessID = 0;
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newEntityChildAddProcess.Add(newObj);
                                }
                            }
                            foreach (var obj in item.FBChildDetails)
                            {
                                var tempObj = fbMaster.FBookingChildDetails.Find(x => x.BookingChildID == bookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildDetails.Add(obj);
                                }
                                else
                                {
                                    var newObj = new FBookingAcknowledgeChildDetails();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newObj.ItemGroupID = item.ItemGroupID;
                                    newObj.SubGroupID = item.SubGroupID;
                                    newObj.ItemMasterID = item.ItemMasterID;
                                    newObj.OrderBankPOID = item.OrderBankPOID;
                                    newObj.ColorID = item.ColorID;
                                    newObj.SizeID = item.SizeID;
                                    newObj.TechPackID = item.TechPackID;
                                    newObj.ConsumptionQty = item.ConsumptionQty;
                                    newObj.BookingQty = item.BookingQty;
                                    newObj.BookingUnitID = item.BookingUnitID;
                                    newObj.RequisitionQty = item.RequisitionQty;
                                    newObj.ExecutionCompanyID = item.ExecutionCompanyID;
                                    newObj.TechnicalNameId = item.TechnicalNameId;

                                    newObj.AddedBy = entity.AddedBy;
                                    newObj.DateAdded = DateTime.Now;
                                    newEntityChildDetails.Add(newObj);
                                }
                            }
                            foreach (var obj in item.ChildsGpart)
                            {
                                var tempObj = fbMaster.FBookingAcknowledgeChildGarmentPart.Find(x => x.BookingChildID == bookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildsGpart.Add(obj);
                                }
                                else
                                {
                                    var newObj = new FBookingAcknowledgeChildGarmentPart();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newObj.FUPartID = item.FUPartID;
                                    newEntityChildsGpart.Add(newObj);
                                }
                            }
                            foreach (var obj in item.ChildsProcess)
                            {
                                var tempObj = fbMaster.FBookingAcknowledgeChildProcess.Find(x => x.BookingChildID == bookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildsProcess.Add(obj);
                                }
                                else
                                {
                                    var newObj = new FBookingAcknowledgeChildProcess();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newEntityChildsProcess.Add(newObj);
                                }
                            }
                            foreach (var obj in item.ChildsText)
                            {
                                var tempObj = fbMaster.FBookingAcknowledgeChildText.Find(x => x.BookingChildID == bookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildsText.Add(obj);
                                }
                                else
                                {
                                    var newObj = new FBookingAcknowledgeChildText();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newEntityChildsText.Add(newObj);
                                }
                            }
                            foreach (var obj in item.ChildsDistribution)
                            {
                                var tempObj = fbMaster.FBookingAcknowledgeChildDistribution.Find(x => x.BookingChildID == bookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildsDistribution.Add(obj);
                                }
                                else
                                {
                                    var newObj = new FBookingAcknowledgeChildDistribution();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newObj.DeliveryDate = obj.DeliveryDate;
                                    newObj.DistributionQty = obj.DistributionQty;
                                    newEntityChildsDistribution.Add(newObj);
                                }
                            }
                            foreach (var obj in item.ChildsYarnSubBrand)
                            {
                                var tempObj = fbMaster.FBookingAcknowledgeChildYarnSubBrand.Find(x => x.BookingChildID == bookingChildID);
                                if (tempObj != null)
                                {
                                    obj.BookingChildID = bookingChildID;
                                    obj.EntityState = EntityState.Modified;
                                    newEntityChildsYarnSubBrand.Add(obj);
                                }
                                else
                                {
                                    var newObj = new FBookingAcknowledgeChildYarnSubBrand();
                                    newObj.BookingChildID = bookingChildID;
                                    newObj.BookingID = item.BookingID;
                                    newObj.ConsumptionID = item.ConsumptionID;
                                    newEntityChildsYarnSubBrand.Add(newObj);
                                }
                            }
                            if (item.CriteriaIDs.IsNotNullOrEmpty())
                            {
                                string[] criteriaIds = item.CriteriaIDs.Distinct().ToString().Split(',');
                                if (criteriaIds.Length > 0 && !criteriaIds[0].Equals(""))
                                {
                                    foreach (string criteriaID in item.CriteriaIDs.Split(','))
                                    {
                                        if (criteriaID.IsNotNullOrEmpty())
                                        {
                                            var obj = fbChild.FBAChildPlannings.FirstOrDefault(x => x.CriteriaID == Convert.ToInt32(criteriaID));
                                            if (obj != null)
                                            {
                                                obj.BookingChildID = bookingChildID;
                                                obj.AcknowledgeID = item.AcknowledgeID;
                                                obj.CriteriaID = obj.CriteriaID;
                                                obj.EntityState = EntityState.Modified;
                                                newFBAChildPlanning.Add(obj);

                                                #region Delete Duplicate Criteria BookingChildID wise

                                                var fBACPlannings = fbChild.FBAChildPlannings.Where(x => x.CriteriaID == Convert.ToInt32(criteriaID)).ToList();
                                                if (fBACPlannings.Count() > 1)
                                                {
                                                    int count = 0;
                                                    fBACPlannings.ForEach(x =>
                                                    {
                                                        count++;
                                                        if (count > 1)
                                                        {
                                                            x.EntityState = EntityState.Deleted;
                                                            newFBAChildPlanning.Add(x);
                                                        }
                                                    });
                                                }

                                                #endregion Delete Duplicate Criteria BookingChildID wise
                                            }
                                            else
                                            {
                                                var newObj = new FBAChildPlanning();
                                                newObj.BookingChildID = bookingChildID;
                                                newObj.AcknowledgeID = item.AcknowledgeID;
                                                newObj.CriteriaID = Convert.ToInt32(criteriaID);
                                                newFBAChildPlanning.Add(newObj);
                                            }
                                        }
                                    }
                                    List<int> deletedFBACIds = new List<int>();
                                    fbChild.FBAChildPlannings.ForEach(x =>
                                    {
                                        if (!criteriaIds.Contains(x.CriteriaID.ToString()))
                                        {
                                            deletedFBACIds.Add(x.CriteriaID);
                                        }
                                    });
                                    deletedFBACIds.ForEach(criteriaID =>
                                    {
                                        var obj = fbChild.FBAChildPlannings.Find(x => x.CriteriaID == criteriaID);
                                        obj.EntityState = EntityState.Deleted;
                                        newFBAChildPlanning.Add(obj);
                                    });
                                }
                                item.FBAChildPlannings = newFBAChildPlanning;
                            }
                            else
                            {
                                #region Delete Duplicate Criteria BookingChildID wise

                                newFBAChildPlanning = new List<FBAChildPlanning>();
                                string[] criteriaIDs = string.Join(",", fbChild.FBAChildPlannings.Select(x => x.CriteriaID).Distinct()).Split(',');
                                foreach (string critentialId in criteriaIDs)
                                {
                                    var fBACPlannings = fbChild.FBAChildPlannings.Where(x => x.CriteriaID == Convert.ToInt32(critentialId)).ToList();
                                    if (fBACPlannings.Count() > 1)
                                    {
                                        int count = 0;
                                        fBACPlannings.ForEach(x =>
                                        {
                                            count++;
                                            if (count > 1)
                                            {
                                                x.EntityState = EntityState.Deleted;
                                                newFBAChildPlanning.Add(x);
                                            }
                                        });
                                    }
                                }
                                item.FBAChildPlannings = newFBAChildPlanning;

                                #endregion Delete Duplicate Criteria BookingChildID wise
                            }
                        }
                        else
                        {
                            newBookingChildId++;
                            item.BookingChildID = newBookingChildId;
                            item.EntityState = EntityState.Added;
                            item.DateAdded = DateTime.Now;
                            foreach (var ChildAddProces in item.ChildAddProcess)
                            {
                                var obj = CommonFunction.DeepClone(ChildAddProces);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.EntityState = EntityState.Added;
                                newEntityChildAddProcess.Add(obj);
                            }
                            foreach (var FBChildDetail in item.FBChildDetails)
                            {
                                var obj = CommonFunction.DeepClone(FBChildDetail);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.BookingQty = item.BookingQty;
                                obj.ItemGroupID = item.ItemGroupID;
                                obj.SubGroupID = item.SubGroupID;
                                obj.ItemMasterID = item.ItemMasterID;
                                obj.OrderBankPOID = item.OrderBankPOID;
                                obj.ColorID = item.ColorID;
                                obj.SizeID = item.SizeID;
                                obj.TechPackID = item.TechPackID;
                                obj.ConsumptionQty = item.ConsumptionQty;
                                obj.BookingUnitID = item.BookingUnitID;
                                obj.RequisitionQty = item.RequisitionQty;
                                obj.ExecutionCompanyID = item.ExecutionCompanyID;
                                obj.TechnicalNameId = item.TechnicalNameId;

                                obj.EntityState = EntityState.Added;
                                newEntityChildDetails.Add(obj);
                            }
                            foreach (var ChildsGp in item.ChildsGpart)
                            {
                                var obj = CommonFunction.DeepClone(ChildsGp);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.FUPartID = item.FUPartID;
                                obj.EntityState = EntityState.Added;
                                newEntityChildsGpart.Add(obj);
                            }
                            foreach (var ChildProc in item.ChildsProcess)
                            {
                                var obj = CommonFunction.DeepClone(ChildProc);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.EntityState = EntityState.Added;
                                newEntityChildsProcess.Add(obj);
                            }
                            foreach (var ChildTxt in item.ChildsText)
                            {
                                var obj = CommonFunction.DeepClone(ChildTxt);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.EntityState = EntityState.Added;
                                newEntityChildsText.Add(obj);
                            }
                            foreach (var ChildDis in item.ChildsDistribution)
                            {
                                var obj = CommonFunction.DeepClone(ChildDis);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.DeliveryDate = ChildDis.DeliveryDate;
                                obj.DistributionQty = ChildDis.DistributionQty;
                                obj.EntityState = EntityState.Added;
                                newEntityChildsDistribution.Add(obj);
                            }
                            foreach (var ChildYarnSubBrand in item.ChildsYarnSubBrand)
                            {
                                var obj = CommonFunction.DeepClone(ChildYarnSubBrand);
                                obj.BookingChildID = newBookingChildId;
                                obj.BookingID = item.BookingID;
                                obj.ConsumptionID = item.ConsumptionID;
                                obj.EntityState = EntityState.Added;
                                newEntityChildsYarnSubBrand.Add(obj);
                            }
                            if (item.CriteriaIDs.IsNotNullOrEmpty())
                            {
                                foreach (string criteriaID in item.CriteriaIDs.Split(',').Distinct())
                                {
                                    if (criteriaID.IsNotNullOrEmpty())
                                    {
                                        var obj = new FBAChildPlanning();
                                        obj.BookingChildID = newBookingChildId;
                                        obj.AcknowledgeID = item.AcknowledgeID;
                                        obj.CriteriaID = Convert.ToInt32(criteriaID);
                                        obj.EntityState = EntityState.Added;
                                        newFBAChildPlanning.Add(obj);
                                    }
                                }
                            }
                            item.FBAChildPlannings = newFBAChildPlanning;
                        }
                        newEntityChilds.Add(item);
                    }

                    fbMaster.FBookingChild.ForEach(x =>
                    {
                        var obj = newEntityChilds.Find(y => y.ItemMasterID == x.ItemMasterID && y.SubGroupID == x.SubGroupID && y.EntityState != EntityState.Deleted);
                        if (obj == null)
                        {
                            x.EntityState = EntityState.Deleted;
                            newEntityChilds.Add(x);

                            newEntityChildAddProcess.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                p.EntityState = EntityState.Deleted;
                            });

                            fbMaster.FBookingAcknowledgeChildAddProcess.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                var objP = newEntityChildAddProcess.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildAddProcess.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildAddProcess.Add(p);
                                }
                            });
                            fbMaster.FBookingChildDetails.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                var objP = newEntityChildDetails.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildDetails.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildDetails.Add(p);
                                }
                            });
                            fbMaster.FBookingAcknowledgeChildGarmentPart.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                var objP = newEntityChildsGpart.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildsGpart.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildsGpart.Add(p);
                                }
                            });
                            fbMaster.FBookingAcknowledgeChildProcess.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                var objP = newEntityChildsProcess.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildsProcess.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildsProcess.Add(p);
                                }
                            });
                            fbMaster.FBookingAcknowledgeChildText.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                var objP = newEntityChildsText.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildsText.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildsText.Add(p);
                                }
                            });
                            fbMaster.FBookingAcknowledgeChildDistribution.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                var objP = newEntityChildsDistribution.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildsDistribution.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildsDistribution.Add(p);
                                }
                            });
                            fbMaster.FBookingAcknowledgeChildYarnSubBrand.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                            {
                                var objP = newEntityChildsYarnSubBrand.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID);
                                if (objP != null)
                                {
                                    newEntityChildsYarnSubBrand.Find(pp => pp.BookingChildID == p.BookingChildID && pp.BookingID == p.BookingID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                                }
                                else
                                {
                                    p.EntityState = EntityState.Deleted;
                                    newEntityChildsYarnSubBrand.Add(p);
                                }
                            });
                            fbMaster.BDSDependentTNACalander.Where(p => p.BookingChildID == x.BookingChildID && p.BookingID == x.BookingID).ToList().ForEach(p =>
                            {
                                p.EntityState = EntityState.Deleted;
                                newBDCalander.Add(p);
                            });
                        }
                    });

                    entityChilds = newEntityChilds;
                    entityChildAddProcess = newEntityChildAddProcess;
                    entityChildDetails = newEntityChildDetails;
                    entityChildsGpart = newEntityChildsGpart;
                    entityChildsProcess = newEntityChildsProcess;
                    entityChildsText = newEntityChildsText;
                    entityChildsDistribution = newEntityChildsDistribution;
                    entityChildsYarnSubBrand = newEntityChildsYarnSubBrand;
                    BDCalander = newBDCalander;
                    fbMaster.FBookingAcknowledgeImage.ForEach(x => x.EntityState = EntityState.Modified);
                    entityChildsImage = fbMaster.FBookingAcknowledgeImage;
                }

                var fabricWastageGrids = await _commonService.GetFabricWastageGridAsync("BDS");
                foreach (FBookingAcknowledgeChild details in entityChilds.Where(x => x.EntityState != EntityState.Deleted))
                {
                    foreach (BDSTNAEvent_HK hk_details in BDSTNAEvent.BDSTNAEvent_HKNames)
                    {
                        BDSDependentTNACalander objCalender = new BDSDependentTNACalander();
                        objCalender.BookingChildID = details.BookingChildID;
                        objCalender.BookingID = details.BookingID;
                        objCalender.EventID = hk_details.EventID;
                        objCalender.SeqNo = hk_details.SeqNo;
                        objCalender.SystemEvent = hk_details.SystemEvent;
                        objCalender.HasDependent = hk_details.HasDependent;
                        objCalender.BookingDate = DateTime.Now;

                        if (entity.FBAckID > 0)
                        {
                            var obj = fbMaster.BDSDependentTNACalander.Find(x => x.BookingChildID == details.BookingChildID);
                            if (obj != null)
                            {
                                objCalender.EntityState = EntityState.Modified;
                            }
                        }

                        if (hk_details.EventID == 1)
                        {
                            objCalender.EventDate = DateTime.Now.Date.AddDays(details.StructureDays); //(objCalender.BookingDate).AddDays + details.StructureDays;
                            objCalender.CompleteDate = DateTime.Now;
                            objCalender.TNADays = details.StructureDays;
                        }
                        else if (hk_details.EventID == 2)
                        {
                            objCalender.EventDate = DateTime.Now.Date.AddDays(details.MaterialDays);
                            objCalender.TNADays = details.MaterialDays;
                        }
                        else if (hk_details.EventID == 3)
                        {
                            objCalender.EventDate = DateTime.Now.Date.AddDays(details.KnittingDays);
                            objCalender.TNADays = details.KnittingDays;
                        }
                        else if (hk_details.EventID == 4)
                        {
                            objCalender.EventDate = DateTime.Now.Date.AddDays(details.PreprocessDays);
                            objCalender.TNADays = details.PreprocessDays;
                        }
                        else if (hk_details.EventID == 5)
                        {
                            objCalender.EventDate = DateTime.Now.Date.AddDays(details.BatchPreparationDays);
                            objCalender.TNADays = details.BatchPreparationDays;
                        }
                        else if (hk_details.EventID == 6)
                        {
                            objCalender.EventDate = DateTime.Now.Date.AddDays(details.DyeingDays);
                            objCalender.TNADays = details.DyeingDays;
                        }
                        else if (hk_details.EventID == 7)
                        {
                            objCalender.EventDate = DateTime.Now.Date.AddDays(details.FinishingDays);
                            objCalender.TNADays = details.FinishingDays;
                        }
                        else if (hk_details.EventID == 8)
                        {
                            objCalender.EventDate = DateTime.Now.Date.AddDays(details.QualityDays);
                            objCalender.TNADays = details.QualityDays;
                        }
                        else if (hk_details.EventID == 9)
                        {
                            objCalender.EventDate = DateTime.Now.Date.AddDays(details.TestReportDays);
                            objCalender.TNADays = details.TestReportDays;
                        }
                        else
                        {
                            objCalender.EventDate = DateTime.Now.Date.AddDays(details.TotalDays);
                            objCalender.TNADays = details.TotalDays;
                        }
                        //objCalender.TNADays = 0;
                        objCalender.SystemEvent = false;
                        BDCalander.Add(objCalender);
                    }
                    //objCalender

                    if (entity.FBAckID == 0)
                    {
                        List<FBAChildPlanning> planningList = new List<FBAChildPlanning>();
                        string[] criteriaIds = details.CriteriaIDs.Split(',');
                        if (criteriaIds.Length > 0 && !criteriaIds[0].Equals(""))
                        {
                            foreach (string criteria in criteriaIds)
                            {
                                planningList.Add(new FBAChildPlanning { CriteriaID = Convert.ToInt32(criteria) });
                            }
                            details.FBAChildPlannings = planningList;
                        }
                        else
                        {
                            details.FBAChildPlannings = new List<FBAChildPlanning>();
                        }
                    }

                    #region Set FabricWastageGrid Values

                    var fabricWastageGrid = new FabricWastageGrid();
                    if (Convert.ToInt32(details.GSM) > 0 && details.SubGroupID == 1)
                    {
                        fabricWastageGrid = fabricWastageGrids.Where(x => x.IsFabric == true).ToList().Find(x => x.GSMFrom <= Convert.ToInt32(details.GSM)
                                                                            && Convert.ToInt32(details.GSM) <= x.GSMTo
                                                                            && x.BookingQtyFrom <= details.BookingQty
                                                                            && details.BookingQty <= x.BookingQtyTo);
                    }
                    else if (details.SubGroupID == 11 || details.SubGroupID == 12)
                    {
                        fabricWastageGrid = fabricWastageGrids.Where(x => x.IsFabric == false).ToList().Find(x => x.BookingQtyFrom <= details.BookingQty
                                                                            && details.BookingQty <= x.BookingQtyTo);
                    }
                    if (fabricWastageGrid != null)
                    {
                        if (fabricWastageGrid.FixedQty)
                        {
                            details.ExcessPercentage = 0;
                            if (details.BookingQty > 0)
                            {
                                details.ExcessQty = fabricWastageGrid.ExcessQty;
                                details.TotalQty = details.BookingQty + details.ExcessQty;
                            }
                            if (details.TotalQtyInKG > 0)
                            {
                                details.ExcessQtyInKG = fabricWastageGrid.ExcessQty;
                                details.TotalQtyInKG = details.TotalQtyInKG + details.ExcessQty;
                            }
                        }
                        else
                        {
                            details.ExcessPercentage = fabricWastageGrid.ExcessPercentage;
                            if (details.BookingQty > 0)
                            {
                                details.ExcessQty = Math.Floor(details.BookingQty * fabricWastageGrid.ExcessPercentage / 100);
                                details.TotalQty = details.BookingQty + details.ExcessQty;
                            }
                            if (details.TotalQtyInKG > 0)
                            {
                                details.ExcessQtyInKG = Math.Floor(details.BookingQty * fabricWastageGrid.ExcessPercentage / 100);
                                details.TotalQtyInKG = details.TotalQtyInKG + details.ExcessQtyInKG;
                            }
                        }
                    }

                    #endregion Set FabricWastageGrid Values
                }

                string ColorIDs = "";
                ColorIDs = string.Join(",", entityChilds.Select(x => x.Color)).ToString();

                entity.ColorCodeList = await _service.GetAllAsyncColorIDs(ColorIDs);

                var entityFreeConcepts = new List<FreeConceptMaster>();
                var freeConceptMRList = new List<FreeConceptMRMaster>();
                string groupConceptNo = entity.BookingNo;// entities.Count() > 0 ? entities[0].BookingNo : "";
                if (groupConceptNo.IsNotNullOrEmpty())
                {
                    entityFreeConcepts = await _fcService.GetDatasAsync(groupConceptNo);
                    entityFreeConcepts.ForEach(x =>
                    {
                        x.EntityState = EntityState.Unchanged;
                        x.ChildColors.SetUnchanged();

                        if (modelDynamic.PageName == "BulkBookingKnittingInfo")
                        {
                            var obj = modelDynamic.FBookingChild.Find(y => y.SubGroupID == x.SubGroupID && y.ConsumptionID == x.ConsumptionID);
                            if (obj.IsNotNull())
                            {
                                x.MCSubClassID = obj.MachineTypeId;
                                x.TechnicalNameId = obj.TechnicalNameId;
                                x.MachineGauge = obj.MachineGauge;
                                x.MachineDia = obj.MachineDia;
                                x.BrandID = obj.BrandID;
                                x.EntityState = EntityState.Modified;
                            }
                        }
                    });
                    if (isBDS == 2)
                    {
                        freeConceptMRList = await _serviceFreeConceptMR.GetByGroupConceptNo(groupConceptNo);
                        freeConceptMRList.ForEach(x =>
                        {
                            x.EntityState = EntityState.Unchanged;
                            x.Childs.SetUnchanged();
                        });
                    }
                }
                await _service.SaveAsync(entity, entityChilds, entityChildAddProcess, entityChildDetails, entityChildsGpart, entityChildsProcess, entityChildsText, entityChildsDistribution, entityChildsYarnSubBrand, entityChildsImage, BDCalander, isBDS, entityFreeConcepts, freeConceptMRList);

                bool isSendMail = true;
                if (entity.BookingNo.Contains("PB-") || entity.IsBDS == 2)
                {
                    isSendMail = false;
                }
                if (entity.BookingNo.Contains("LR-"))
                {
                    isSendMail = false;
                }
                /*//OFF FOR CORE//
                if (isSendMail)
                {
                    #region Send Mail

                    try
                    {
                        EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();

                        var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "BDS Cancel");
                        var uInfo = await _emailService.GetUserEmailInfoAsync(entity.BookingBy);
                        //var attachment = await _reportingService.GetPdfByte(990, AppUser.UserCode, entity.BookingNo);
                        var attachment = new byte[] { 0 };
                        String fromMailID = "";
                        String toMailID = "";
                        String ccMailID = "";
                        String bccMailID = "";
                        String password = "";

                        if (Request.Headers.Host.ToString().ToUpper()== "texerp.epylliongroup.com".ToUpper())
                        {
                            fromMailID = AppUser.Email;
                            password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
                            toMailID = uInfo.Email.IsNullOrEmpty() ? AppUser.Email : uInfo.Email;
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
                        string tddetail = "";

                        List<FBookingAcknowledgeChild> GetDataForAcknowledgColour = await _service.GetDataForAcknowledgColourAsync(entity.BookingID);
                        DateTime date = new DateTime();
                        string deleveryDate = "";
                        string subject = entity.IsUnAcknowledge == true ? $"Cancelation of Sample Booking No : {entity.BookingNo} for Garments Buyer {entity.BuyerName} {entity.BuyerTeamName} not accepted." : $"Cancelation of Sample Booking No:{entity.BookingNo} for Garments Buyer {entity.BuyerName} {entity.BuyerTeamName} accepted.";
                        string messageBody = "";

                        if (entity.IsUnAcknowledge)
                        {
                            messageBody = $"<center><u><b>Booking Cancelation Not Accepted</b></u> </center></br></br>" +
                                $"Dear Sir,</br>" +
                                $"Fabric Booking No: <b>{entity.BookingNo}</b>,Buyer Development Sample for the Garments Buyer {entity.BuyerName}" +
                                $"<b> cannot be acknowledged</b> for the below comments:</br>" +
                                $"</br>" +
                                $"<b>\"{entity.UnAcknowledgeReason}\"</b></br>" +
                                $"</br>" +
                                $"</br>" +
                                $"Thanks & Best Regards," +
                                $"</br>" +
                                $"{AppUser.EmployeeName}</br>" +
                                $"{AppUser.DepertmentDescription}</br></br>" +
                                $"This is system generated mail.";
                        }
                        else
                        {
                            messageBody = $"<center><u><b>Booking Cancelation Accepted</b></u> </center></br></br>" +
                                    $"Dear Sir,</br>" +
                                    $"Fabric Booking No: {entity.BookingNo}, Buyer Development Sample for the Garments Buyer {entity.BuyerName}" +
                                    $" has been acknowledged for further processing. Please note that the fabrics will be delivered from Textile Unit as per the following schedule:" +
                                    $"</br></br>" +
                                     $"<div style='float:center;padding:10px;font-family:Tahoma;'>" +
                                     $"<table border='1' style='color:black;font-size:10pt;font-family:Tahoma;'>" +
                                     $"<tr style='background-color:#dde9ed'>" +
                                     $"<th>Garments Color</th>" +
                                     $"<th>Delivery plan</th>" +
                                     $"</tr>";

                            for (int i = 0; i < GetDataForAcknowledgColour.Count(); i++)
                            {
                                date = (DateTime)GetDataForAcknowledgColour[i].DeliveryDate;
                                deleveryDate = date.ToString("dd/M/yyyy");
                                tddetail += $"<tr><td>{GetDataForAcknowledgColour[i].Color}</td>" +
                                               $"<td>{deleveryDate}</td></tr>";
                            }
                            messageBody += tddetail +
                                 $"</table>" +
                                 $"</div>" +
                                $"</br>" +
                                $"<div>" +
                                $"The Fabric Delivery plan has been generated by Textile ERP considering the shortest possible operational " +
                                $"timeframe based on fabric requirements on the booking. Please contact with concerns for any issue. </ br > " +
                                $"</div>" +
                                $"</br>" +
                                $"Thanks & Best Regards," +
                                $"</br>" +
                                $"{AppUser.EmployeeName}</br>" +
                                $"{AppUser.DepertmentDescription}</br></br>" +
                                $"This is system generated mail.";
                        }
                        String fileName = String.Empty;
                        //fileName = $"{entity.BookingNo}.pdf";
                        await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, subject, messageBody, fileName, attachment);
                    }
                    catch (Exception ex)
                    {
                    }

                    #endregion Send Mail
                }
                */
            }

            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.FBookingAcknowledge, 0, grpConceptNo, entity.BookingID);
            return Ok();
        }

        [HttpGet]
        [Route("get-yarn-revision-reason")]
        public async Task<IActionResult> GetYarnRevisionReason()
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetYarnRevisionReason(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        public List<YarnBookingChildItemRevision> GetYarnRevisedChildItems(List<YarnBookingChildItem> yarnBookingChildItems)
        {
            List<YarnBookingChildItemRevision> yarnBookingChildItemsRevision = new List<YarnBookingChildItemRevision>();
            yarnBookingChildItems.ForEach(c =>
            {
                YarnBookingChildItemRevision obj = new YarnBookingChildItemRevision();
                obj.YBChildItemID = c.YBChildItemID;
                obj.YBChildID = c.YBChildID;
                obj.YBookingID = c.YBookingID;
                obj.YItemMasterID = c.YItemMasterID;
                obj.UnitID = c.UnitID;
                obj.Blending = c.Blending;
                obj.YarnCategory = c.YarnCategory;
                obj.Distribution = c.Distribution;
                obj.BookingQty = c.BookingQty;
                obj.Allowance = c.Allowance;
                obj.RequiredQty = c.RequiredQty;
                obj.ShadeCode = c.ShadeCode;
                obj.Remarks = c.Remarks;
                obj.Specification = c.Specification;
                obj.YD = c.YD;
                obj.YDItem = c.YDItem;
                obj.StitchLength = c.StitchLength;
                obj.PhysicalCount = c.PhysicalCount;
                obj.BatchNo = c.BatchNo;
                obj.SpinnerId = c.SpinnerId;
                obj.YarnLotNo = c.YarnLotNo;
                obj.YarnReqQty = c.YarnReqQty;
                obj.YarnLeftOverQty = c.YarnLeftOverQty;
                obj.NetYarnReqQty = c.NetYarnReqQty;
                obj.YarnBalanceQty = c.YarnBalanceQty;
                obj.YarnPly = c.YarnPly;
                obj.GreyAllowance = c.GreyAllowance;
                obj.YDAllowance = c.YDAllowance;
                obj.GreyYarnUtilizationQty = c.GreyYarnUtilizationQty;
                obj.DyedYarnUtilizationQty = c.DyedYarnUtilizationQty;
                obj.AllowanceFM = c.AllowanceFM;
                obj.RequiredQtyFM = c.RequiredQtyFM;
                obj.SourcingRate = c.SourcingRate;
                obj.SourcingLandedCost = c.SourcingLandedCost;
                obj.TotalSourcingRate = c.TotalSourcingRate;
                obj.DyeingCostFM = c.DyeingCostFM;
                obj.EntityState = c.EntityState;
                yarnBookingChildItemsRevision.Add(obj);
            });
            return yarnBookingChildItemsRevision;
        }
        public List<YarnBookingChild> GetYarnRevisedChilds(List<YarnBookingChild> yarnBookingChilds)
        {
            yarnBookingChilds.ForEach(m =>
            {
                m.ChildItemsRevision = new List<YarnBookingChildItemRevision>();
                m.ChildItems.ForEach(c =>
                {
                    YarnBookingChildItemRevision obj = new YarnBookingChildItemRevision();
                    obj.YBChildItemID = c.YBChildItemID;
                    obj.YBChildID = c.YBChildID;
                    obj.YBookingID = c.YBookingID;
                    obj.YItemMasterID = c.YItemMasterID;
                    obj.UnitID = c.UnitID;
                    obj.Blending = c.Blending;
                    obj.YarnCategory = c.YarnCategory;
                    obj.Distribution = c.Distribution;
                    obj.BookingQty = c.BookingQty;
                    obj.Allowance = c.Allowance;
                    obj.RequiredQty = c.RequiredQty;
                    obj.ShadeCode = c.ShadeCode;
                    obj.Remarks = c.Remarks;
                    obj.Specification = c.Specification;
                    obj.YD = c.YD;
                    obj.YDItem = c.YDItem;
                    obj.StitchLength = c.StitchLength;
                    obj.PhysicalCount = c.PhysicalCount;
                    obj.BatchNo = c.BatchNo;
                    obj.SpinnerId = c.SpinnerId;
                    obj.YarnLotNo = c.YarnLotNo;
                    obj.YarnReqQty = c.YarnReqQty;
                    obj.YarnLeftOverQty = c.YarnLeftOverQty;
                    obj.NetYarnReqQty = c.NetYarnReqQty;
                    obj.YarnBalanceQty = c.YarnBalanceQty;
                    obj.YarnPly = c.YarnPly;
                    obj.GreyAllowance = c.GreyAllowance;
                    obj.YDAllowance = c.YDAllowance;
                    obj.GreyYarnUtilizationQty = c.GreyYarnUtilizationQty;
                    obj.DyedYarnUtilizationQty = c.DyedYarnUtilizationQty;
                    obj.AllowanceFM = c.AllowanceFM;
                    obj.RequiredQtyFM = c.RequiredQtyFM;
                    obj.SourcingRate = c.SourcingRate;
                    obj.SourcingLandedCost = c.SourcingLandedCost;
                    obj.TotalSourcingRate = c.TotalSourcingRate;
                    obj.DyeingCostFM = c.DyeingCostFM;
                    obj.EntityState = c.EntityState;
                    m.ChildItemsRevision.Add(obj);
                });
            });
            return yarnBookingChilds;
        }
        //public void SetBookingWeightKG(FBookingAcknowledge masterData)
        //{
        //    char[] delimiters = { 'X' };
        //    decimal perWeightCollar = Convert.ToDecimal(masterData.CollarWeightInGm) / (Convert.ToDecimal(masterData.CollarSizeID.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)[0]) * Convert.ToDecimal(masterData.CollarSizeID.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)[1]));
        //    decimal perWeightCuff = Convert.ToDecimal(masterData.CuffWeightInGm) / (Convert.ToDecimal(masterData.CuffSizeID.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)[0]) * Convert.ToDecimal(masterData.CuffSizeID.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)[1]));
        //    masterData.FBookingChild.Where(m => m.SubGroupID == 11).ToList().ForEach(x =>
        //    {
        //        //var Sizelist = masterData.AllCollarSizeList.Where(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color).ToList();
        //        //decimal BookingWeightGM = 0;
        //        //Sizelist.ForEach(z => {
        //        //    BookingWeightGM += Convert.ToDecimal(z.Length) * Convert.ToDecimal(z.Width) * perWeightCollar;
        //        //});
        //        var Size = masterData.AllCollarSizeList.Where(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color).FirstOrDefault();
        //        decimal BookingWeightGM = Convert.ToDecimal(Size.Length) * Convert.ToDecimal(Size.Width) * perWeightCollar;

        //        x.BookingQtyKG = (BookingWeightGM / 1000);
        //    });

        //    masterData.FBookingChild.Where(m => m.SubGroupID == 12).ToList().ForEach(x =>
        //    {
        //        ////var Sizelist = masterData.AllCuffSizeList.Where(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color);
        //        ////decimal BookingWeightGM = 0;
        //        ////Sizelist.ToList().ForEach(z => {
        //        ////    BookingWeightGM += Convert.ToDecimal(z.Length) * Convert.ToDecimal(z.Width) * perWeightCuff;
        //        ////});
        //        var Size = masterData.AllCuffSizeList.Where(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color).FirstOrDefault();
        //        decimal BookingWeightGM = Convert.ToDecimal(Size.Length) * Convert.ToDecimal(Size.Width) * perWeightCuff;

        //        x.BookingQtyKG = (BookingWeightGM / 1000);
        //    });

        //}


        /*//OFF FOR CORE//
        public async Task<bool> SystemMail(string BookingNo, string YBookingNo, bool IsYarnRevision, string BuyerName, string BuyerTeam, int RevisionNo, bool IsSendMail)
        {
            if (_isValidMailSending)
            {

                try
                {
                    String Salutation = "Dear Sir,";
                    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
                    Core.DTOs.ItemSubGroupMailSetupDTO isgDTO = new Core.DTOs.ItemSubGroupMailSetupDTO();
                    Core.DTOs.EmployeeMailSetupDTO emsDTO = new Core.DTOs.EmployeeMailSetupDTO();
                    isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "Textile Yarn Booking");


                    int userCode = AppUser.UserCode;

                    var uInfo = await _emailService.GetUserEmailInfoAsync(userCode);
                    var rInfo = uInfo;
                    emsDTO = await _emailService.GetEmployeeMailSetupAsync(userCode, "FBKACK,FBK-UNACK");

                    byte[] attachment = null;
                    String fromMailID = "";
                    String toMailID = "";
                    String ccMailID = "";
                    String bccMailID = "";
                    String password = "";
                    String filePath = "";

                    if (uInfo.IsNotNull() && rInfo.IsNotNull())
                    {
                        String Designation = "", Department = "", EmailID = "", ExtensionNo = "", MailSubject = "", MailBody = "", UnAcknowledgeReason = "";
                        String SenderList = "", ToMailList = "", CCMailList = "", BCCIDList = "";

                        if (uInfo.Designation != "" && uInfo.Designation != null)
                        {
                            Designation = "<BR>" + uInfo.Designation;
                        }
                        if (uInfo.Department != "" && uInfo.Department != null)
                        {
                            Department = "<BR>" + uInfo.Department;
                        }
                        if (uInfo.Email != "" && uInfo.Email != null)
                        {
                            EmailID = "<BR>" + uInfo.Email;
                        }
                        if (Request.Headers.Host.ToString().ToUpper()== "texerp.epylliongroup.com".ToUpper())
                        {
                            fromMailID = uInfo.Email;
                            password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
                            toMailID = rInfo.Email.IsNullOrEmpty() ? AppUser.Email : rInfo.Email;

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
                            if (emsDTO.IsNotNull())
                            {
                                toMailID += emsDTO.ToMailId.IsNullOrEmpty() ? "" : ";" + emsDTO.ToMailId;
                                ccMailID += emsDTO.CcMailId.IsNullOrEmpty() ? "" : ";" + emsDTO.CcMailId;
                                bccMailID += emsDTO.BccMailId.IsNullOrEmpty() ? "" : ";" + emsDTO.BccMailId;
                            }
                        }
                        else
                        {
                            MailBasicProps mailBasicProps = new MailBasicProps();
                            fromMailID = mailBasicProps.DefaultFromEmailId;
                            password = mailBasicProps.DefaultPassword;
                            toMailID = mailBasicProps.DefaultToEmailIds;
                            ccMailID = mailBasicProps.DefaultCCEmailIds;
                            bccMailID = mailBasicProps.DefaultBCCEmailIds;
                        }

                        string subForRevise = RevisionNo > 0 ? "Revise" : "";
                        String BKRevision = String.Empty;
                        BKRevision = RevisionNo > 0 ? " Rev-" + RevisionNo.ToString() : "";
                        string makeYBookingNo = YBookingNo + " " + BKRevision;


                        MailSubject = String.Format(@"{0} Yarn Booking [{1}] Generated for Garments Buyer {2} - {3}", subForRevise, makeYBookingNo, BuyerName, BuyerTeam);

                        MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                                  <BR><BR>{1} Yarn Booking Number  <b>{2}</b> has been generated by textile Operation[Textile] for garments buyer <b>{3} - {4}</b>. 
                                  <BR><BR> For more details please check in ERP. 
                                  <BR><BR>Any query please feel free to contact me.
                                  <BR><BR><BR>Thanks &amp; Best Regards,
                                  <BR><BR>{5}{6}{7}{8}
                                  <BR><BR><BR>This is ERP generated mail</span>", Salutation, subForRevise, makeYBookingNo, BuyerName, BuyerTeam,
                                  uInfo.Name,
                                  Designation, Department, ExtensionNo);



                        //MailSubject = "Test Mail Please Ignore - " + MailSubject;


                        //await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, MailSubject, MailBody, filePath, attachment);
                        await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, MailSubject, MailBody, filePath, attachment);
                    }
                    return IsSendMail;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return IsSendMail;
        }
        */
    }
}
