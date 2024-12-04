using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces.Booking;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.CountEntities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Entity;



namespace EPYSLTEXCore.API.Contollers.Booking
{

    [Route("api/fab-acknowledge")]
    public class FBookingAcknowledgeController : ApiBaseController
    {
        bool _isValidMailSending = true;
        private readonly IFBookingAcknowledgeService _service;
        private readonly IFreeConceptService _fcService;

        //private readonly IEmailService _emailService;// OFF Report Mail in CORE
        //private readonly IReportingService _reportingService;// OFF Report Mail in CORE

        //private static Logger _logger;
        //private readonly ICommonService _commonService;

        //private readonly IFreeConceptMRService _serviceFreeConceptMR;

        public FBookingAcknowledgeController(IUserService userService
            //IEmailService emailService // OFF Report Mail in CORE
            //, IReportingService reportingService// OFF Report Mail in CORE
            , IFBookingAcknowledgeService KnittingProgramBDSService
            , IFreeConceptService FreeConceptService
            //, IFreeConceptMRService serviceFreeConceptMR
        //, ICommonService commonService
            ) : base(userService)
        {
            _service = KnittingProgramBDSService;
            _fcService = FreeConceptService;
            //_emailService = emailService;// OFF Report Mail in CORE
            //_reportingService = reportingService;// OFF Report Mail in CORE
            //_commonService = commonService;
            //_serviceFreeConceptMR = serviceFreeConceptMR;
            //_logger = LogManager.GetCurrentClassLogger();
        }

        [Route("bulkfabric/list")]
        public async Task<IActionResult> GetBulkFabricList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<FBookingAcknowledge> records = await _service.GetBulkFabricAckPagedAsync(status, paginationInfo, AppUser);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("bulk/fabric-booking-acknowledge/get-list-count")]
        public async Task<IActionResult> GetListCount()
        {
            CountListItem data = await _service.GetListCount();
            return Ok(data);
        }

        [HttpGet]
        [Route("bulk/new/{bookingNo}")]
        public async Task<IActionResult> GetNewBulk(string bookingNo)
        {
            FBookingAcknowledge data = await _service.GetNewBulkFAsync(bookingNo);
            return Ok(data);
        }
        [HttpGet]
        [Route("bulk/slist/{bookingNo}/{withoutOB}/{isRevised}")]
        public async Task<IActionResult> GetNewBulkFabric(string bookingNo, int withoutOB, bool isRevised)
        {
            List<BookingChild> updatedDataNew = new List<BookingChild>();
            if (bookingNo.IsNotNullOrEmpty())
            {
                if (withoutOB == 0)
                {
                    updatedDataNew = await _service.GetAllInHouseBookingByBookingNo(bookingNo);
                }
                else
                {
                    updatedDataNew = await _service.GetAllInHouseSampleBookingByBookingNo(bookingNo);
                }
            }
            String selectedbookingID = String.Empty;
            var strArr = updatedDataNew.Select(i => i.BookingId.ToString()).Distinct().ToArray();
            selectedbookingID += string.Join(",", strArr.ToArray());

            FBookingAcknowledge data = new FBookingAcknowledge();
            if (isRevised)
            {
                data = await _service.GetSavedBulkFabricRevisionAsync(selectedbookingID == "" ? "0" : selectedbookingID);
            }
            else
            {
                data = await _service.GetSavedBulkFabricAsync(selectedbookingID == "" ? "0" : selectedbookingID);
                //data.FBookingChild = data.FBookingChild.Where(x => x.BookingQty > 0).ToList();
                //data.FBookingChildCollor = data.FBookingChildCollor.Where(x => x.BookingQty > 0).ToList();
                //data.FBookingChildCuff = data.FBookingChildCuff.Where(x => x.BookingQty > 0).ToList();
            }
            return Ok(data);
        }

        [HttpGet]
        [Route("bulk/smail/{bookingNo}/{withoutOB}/{saveType}/{listTypeMasterGrid}")]
        public async Task<IActionResult> SendMail(string bookingNo, int withoutOB, string saveType, string listTypeMasterGrid)
        {
            List<BookingChild> updatedDataNew = new List<BookingChild>();
            if (bookingNo.IsNotNullOrEmpty())
            {
                if (withoutOB == 0)
                {
                    updatedDataNew = await _service.GetAllInHouseBookingByBookingNo(bookingNo);
                }
                else
                {
                    updatedDataNew = await _service.GetAllInHouseSampleBookingByBookingNo(bookingNo);
                }
            }
            String selectedbookingID = String.Empty;
            var strArr = updatedDataNew.Select(i => i.BookingId.ToString()).Distinct().ToArray();
            selectedbookingID += string.Join(",", strArr.ToArray());
            FabricBookingAcknowledge savedFBA = await _service.GetAllSavedFBAcknowledgeByBookingID(selectedbookingID == "" ? "0" : selectedbookingID);
            bool hasLiabilities = false;
            if (savedFBA.FBookingChild.Count > 0)
            {
                if (savedFBA.FBookingChild.Max(i => Convert.ToInt32(i.SendToMktAck)) == 1)
                {
                    hasLiabilities = true;
                }
            }


            Boolean IsSendMail = false;

            int unAckBy = 0;

            if (withoutOB == 0)
            {
                List<BookingMaster> bmList = new List<BookingMaster>();
                if (updatedDataNew.Count > 0)
                {
                    BookingMaster bm = await _service.GetAllBookingAsync(updatedDataNew[0].BookingId);
                    if (bm.IsNotNull())
                    {
                        bmList.Add(bm);
                        unAckBy = bm.OrderBankMasterID;
                    }
                    // OFF FOR CORE //IsSendMail = await SystemMail(savedFBA.FabricBookingAcknowledgeList, bmList, IsSendMail, saveType, hasLiabilities, unAckBy, listTypeMasterGrid);
                }
            }
            else
            {
                List<SampleBookingMaster> bmList = new List<SampleBookingMaster>();
                if (savedFBA.FabricBookingAcknowledgeList.Count > 0)
                {
                    SampleBookingMaster bm = await _service.GetAllAsync(savedFBA.FabricBookingAcknowledgeList[0].BookingID);
                    if (bm.IsNotNull())
                    {
                        bmList.Add(bm);
                        unAckBy = bm.LabdipUnAcknowledgeBY;
                    }
                    // OFF FOR CORE //IsSendMail = await SystemMailForSample(savedFBA.FabricBookingAcknowledgeList, bmList, IsSendMail, saveType, hasLiabilities, unAckBy, listTypeMasterGrid);
                }
            }
            return Ok(IsSendMail);
        }

        [HttpGet]
        [Route("bulk/list/{ExportOrderNo}/{SubGroupID}")]
        public async Task<IActionResult> CheckRevisionStatus(string ExportOrderNo, string SubGroupID)
        {
            FBookingAcknowledge data = await _service.GetAllRevisionStatusByExportOrderIDAndSubGroupID(ExportOrderNo, SubGroupID);
            if (data.IsNull())
                data = new FBookingAcknowledge();
            return Ok(data);
        }
        [HttpGet]
        [Route("{fbAckId}")]
        public async Task<IActionResult> GetData(int fbAckId)
        {
            FBookingAcknowledge data = await _service.GetDataAsync(fbAckId);
            return Ok(data);
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(FBookingAcknowledge modelDynamic)
        {
            FBookingAcknowledge entity = modelDynamic;// models.FirstOrDefault();
            string grpConceptNo = modelDynamic.grpConceptNo;
            int isBDS = modelDynamic.IsBDS;

            bool isRevised = modelDynamic.IsRevised;
            int preRevisionNo = modelDynamic.PreRevisionNo;
            string ActionStatus = modelDynamic.ActionStatus;
            List<FBookingAcknowledge> entities = new List<FBookingAcknowledge>();
            List<BDSDependentTNACalander> BDCalander = new List<BDSDependentTNACalander>();
            var fbMaster = new FBookingAcknowledge();
            fbMaster = await _service.GetFBAcknowledgeByBookingID(entity.BookingID);
            if (fbMaster.IsNull())
                fbMaster = new FBookingAcknowledge();
            //var BDSTNAEvent = await _service.GetAllAsyncBDSTNAEvent_HK();

            entity.IsSample = false;
            entity.AddedBy = AppUser.UserCode;

            if (entity.IsUnAcknowledge)
            {
                entity.IsUnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.EmployeeCode;
                entity.UnAcknowledgeDate = DateTime.Now;
                entity.UnAcknowledgeReason = entity.UnAcknowledgeReason;
            }

            //FBookingAcknowledgeChild
            List<FBookingAcknowledgeChild> entityChilds = new List<FBookingAcknowledgeChild>();
            foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
            {
                FBookingAcknowledgeChild ObjEntityChild = new FBookingAcknowledgeChild(); ;
                ObjEntityChild.BookingChildID = item.BookingChildID;
                ObjEntityChild.ConsumptionChildID = item.BookingChildID;
                ObjEntityChild.BookingID = item.BookingID;
                ObjEntityChild.IsTxtAck = true;
                if (ActionStatus == "30" || ActionStatus == "3")
                {
                    ObjEntityChild.IsTxtAck = true;
                    ObjEntityChild.TxtAcknowledgeBy = AppUser.UserCode;
                    ObjEntityChild.TxtAcknowledgeDate = DateTime.Now;
                }
                else if (ActionStatus == "10")
                {
                    ObjEntityChild.SendToMktAck = true;
                    ObjEntityChild.AcknowledgeBy = AppUser.UserCode;
                    ObjEntityChild.AcknowledgeDate = DateTime.Now;
                }
                ObjEntityChild.AcknowledgeID = item.AcknowledgeID;
                ObjEntityChild.ConsumptionID = item.ConsumptionID;
                ObjEntityChild.SubGroupID = item.SubGroupID;
                ObjEntityChild.ItemGroupID = item.ItemGroupID;
                ObjEntityChild.ItemMasterID = item.ItemMasterID;
                ObjEntityChild.OrderBankPOID = item.OrderBankPOID;
                ObjEntityChild.ColorID = item.ColorID;
                ObjEntityChild.TechPackID = item.TechPackID;
                ObjEntityChild.BookingUnitID = item.BookingUnitID;
                ObjEntityChild.PreviousBookingQty = item.PreviousBookingQty;
                ObjEntityChild.ActualBookingQty = item.ActualBookingQty;
                ObjEntityChild.LiabilitiesBookingQty = item.LiabilitiesBookingQty;
                ObjEntityChild.CurrentBookingQty = item.BookingQty;
                ObjEntityChild.BookingQty = item.BookingQty;
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
                foreach (FBookingAcknowledgeChildDistribution dis in item.ChildsDistribution)
                {
                    FBookingAcknowledgeChildDistribution obj = new FBookingAcknowledgeChildDistribution();
                    obj.DistributionID = 0;
                    obj.BookingChildID = item.BookingChildID;
                    obj.BookingID = item.BookingID;
                    obj.ConsumptionID = item.ConsumptionID;
                    obj.DeliveryDate = dis.DeliveryDate;
                    obj.DistributionQty = dis.DistributionQty;
                    entityChildsDistribution.Add(obj);
                }
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
            //FBookingAcknowledgementLiabilityDistribution
            List<FBookingAcknowledgementLiabilityDistribution> entityChildsLiabilitiesDistribution = new List<FBookingAcknowledgementLiabilityDistribution>();
            foreach (FBookingAcknowledgeChild item in entity.FBookingChild)
            {
                foreach (FBookingAcknowledgementLiabilityDistribution dis in item.ChildAckLiabilityDetails)
                {
                    if (dis.LiabilityQty > 0)
                    {
                        FBookingAcknowledgementLiabilityDistribution obj = new FBookingAcknowledgementLiabilityDistribution();
                        obj.LChildID = 0;
                        obj.BookingChildID = item.BookingChildID;
                        obj.ConsumptionID = item.ConsumptionID;
                        obj.BookingID = item.BookingID;
                        obj.AcknowledgeID = item.AcknowledgeID;
                        obj.UnitID = item.BookingUnitID;
                        obj.LiabilitiesProcessID = dis.LiabilitiesProcessID;
                        obj.LiabilityQty = dis.LiabilityQty;
                        obj.ConsumedQty = dis.ConsumedQty;
                        entityChildsLiabilitiesDistribution.Add(obj);
                    }
                }
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
            List<FabricBookingAcknowledge> entityFBA = modelDynamic.FabricBookingAcknowledgeList;
            var fbaMaster = entityFBA.Find(i => i.BookingID == modelDynamic.BookingID);
            if (fbaMaster.IsNull())
            {
                fbaMaster = new FabricBookingAcknowledge();
                fbaMaster.BookingID = modelDynamic.BookingID;
                fbaMaster.SubGroupID = modelDynamic.SubGroupID;
                fbaMaster.ItemGroupID = modelDynamic.ItemGroupId;
                fbaMaster.PreProcessRevNo = modelDynamic.RevisionNo;
                fbaMaster.RevisionNo = modelDynamic.RevisionNo;
                fbaMaster.BOMMasterID = modelDynamic.BomMasterId;
                fbaMaster.Status = true;
                fbaMaster.WithoutOB = modelDynamic.WithoutOB;
                fbaMaster.AcknowledgeDate = DateTime.Now;
                fbaMaster.AddedBy = AppUser.UserCode;
                fbaMaster.DateAdded = DateTime.Now;
                fbaMaster.EntityState = EntityState.Added;
                if (entity.IsUnAcknowledge)
                {
                    fbaMaster.UnAcknowledge = true;
                    fbaMaster.UnAcknowledgeBy = AppUser.UserCode;
                    fbaMaster.UnAcknowledgeDate = DateTime.Now;
                }
                entityFBA.Add(fbaMaster);
            }
            else
            {
                fbaMaster.RevisionNo = modelDynamic.RevisionNo + 1;
                fbaMaster.PreProcessRevNo = fbaMaster.RevisionNo;
                fbaMaster.EntityState = EntityState.Modified;
            }



            List<FBookingAcknowledgeChild> newEntityChilds = new List<FBookingAcknowledgeChild>();
            List<FBookingAcknowledgeChildAddProcess> newEntityChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
            List<FBookingAcknowledgeChildDetails> newEntityChildDetails = new List<FBookingAcknowledgeChildDetails>();
            List<FBookingAcknowledgeChildGarmentPart> newEntityChildsGpart = new List<FBookingAcknowledgeChildGarmentPart>();
            List<FBookingAcknowledgeChildProcess> newEntityChildsProcess = new List<FBookingAcknowledgeChildProcess>();
            List<FBookingAcknowledgeChildText> newEntityChildsText = new List<FBookingAcknowledgeChildText>();
            List<FBookingAcknowledgeChildDistribution> newEntityChildsDistribution = new List<FBookingAcknowledgeChildDistribution>();
            List<FBookingAcknowledgeChildYarnSubBrand> newEntityChildsYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
            List<FBAChildPlanning> newFBAChildPlanning = new List<FBAChildPlanning>();
            List<FBookingAcknowledgementLiabilityDistribution> entityChildAckLiabilityDetails = new List<FBookingAcknowledgementLiabilityDistribution>();


            if (fbMaster.IsNull())
                fbMaster = new FBookingAcknowledge();
            fbMaster.BookingID = modelDynamic.BookingID;
            fbMaster.BookingNo = modelDynamic.BookingNo;
            fbMaster.SLNo = modelDynamic.SLNo;
            fbMaster.BookingDate = modelDynamic.BookingDate;
            fbMaster.BuyerID = modelDynamic.BuyerID;
            fbMaster.BuyerTeamID = modelDynamic.BuyerTeamID;
            fbMaster.ExecutionCompanyID = modelDynamic.ExecutionCompanyID;
            fbMaster.SupplierID = modelDynamic.SupplierID;
            fbMaster.StyleMasterID = modelDynamic.StyleMasterID;
            fbMaster.StyleNo = modelDynamic.StyleNo;
            fbMaster.FinancialYearID = modelDynamic.FinancialYearID;
            fbMaster.SeasonID = modelDynamic.SeasonID;
            fbMaster.SubGroupID = 1;
            fbMaster.ExportOrderID = modelDynamic.ExportOrderID;
            fbMaster.BookingQty = modelDynamic.BookingQty;
            fbMaster.BomMasterId = modelDynamic.BomMasterId;
            fbMaster.SubGroupID = modelDynamic.SubGroupID;
            fbMaster.ItemGroupId = modelDynamic.ItemGroupId;
            fbMaster.AddedBy = AppUser.UserCode;
            fbMaster.MerchandiserID = AppUser.UserCode;
            fbMaster.IsSample = false;
            fbMaster.EntityState = EntityState.Added;
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
                bookingChild.ChildAckLiabilityDetails = entityChildAckLiabilityDetails.Where(x => x.BookingChildID == bookingChild.BookingChildID).ToList();
            });
            int newBookingChildId = 0;
            foreach (FBookingAcknowledgeChild item in entityChilds)
            {
                newFBAChildPlanning = new List<FBAChildPlanning>();

                item.AcknowledgeID = entity.FBAckID;
                FBookingAcknowledgeChild fbChild = fbMaster.FBookingChild.Find(x => x.ItemMasterID == item.ItemMasterID && x.SubGroupID == item.SubGroupID);
                if (fbChild != null)
                {
                    int bookingChildID = fbChild.BookingChildID;
                    item.BookingChildID = bookingChildID;
                    item.DateUpdated = DateTime.Now;
                    item.EntityState = EntityState.Modified;

                    foreach (FBookingAcknowledgeChildAddProcess obj in item.ChildAddProcess)
                    {
                        FBookingAcknowledgeChildAddProcess tempObj = fbMaster.FBookingAcknowledgeChildAddProcess.Find(x => x.BookingCAddProcessID == obj.BookingCAddProcessID);
                        if (tempObj != null)
                        {
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
                    }
                    foreach (FBookingAcknowledgeChildDetails obj in item.FBChildDetails)
                    {
                        FBookingAcknowledgeChildDetails tempObj = fbMaster.FBookingChildDetails.Find(x => x.BookingCDetailsID == obj.BookingCDetailsID);
                        if (tempObj != null)
                        {
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
                            newObj.DateAdded = DateTime.Now;
                            newEntityChildDetails.Add(newObj);
                        }
                    }
                    foreach (FBookingAcknowledgeChildGarmentPart obj in item.ChildsGpart)
                    {
                        FBookingAcknowledgeChildGarmentPart tempObj = fbMaster.FBookingAcknowledgeChildGarmentPart.Find(x => x.BookingCGPID == obj.BookingCGPID);
                        if (tempObj != null)
                        {
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
                    }
                    foreach (FBookingAcknowledgeChildProcess obj in item.ChildsProcess)
                    {
                        FBookingAcknowledgeChildProcess tempObj = fbMaster.FBookingAcknowledgeChildProcess.Find(x => x.BookingCProcessID == obj.BookingCProcessID);
                        if (tempObj != null)
                        {
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
                    }
                    foreach (FBookingAcknowledgeChildText obj in item.ChildsText)
                    {
                        FBookingAcknowledgeChildText tempObj = fbMaster.FBookingAcknowledgeChildText.Find(x => x.TextID == obj.TextID);
                        if (tempObj != null)
                        {
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
                    }
                    foreach (FBookingAcknowledgeChildDistribution obj in item.ChildsDistribution)
                    {
                        FBookingAcknowledgeChildDistribution tempObj = fbMaster.FBookingAcknowledgeChildDistribution.Find(x => x.DistributionID == obj.DistributionID);
                        if (tempObj != null)
                        {
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
                    }
                    foreach (FBookingAcknowledgeChildYarnSubBrand obj in item.ChildsYarnSubBrand)
                    {
                        FBookingAcknowledgeChildYarnSubBrand tempObj = fbMaster.FBookingAcknowledgeChildYarnSubBrand.Find(x => x.BookingCYSubBrandID == obj.BookingCYSubBrandID);
                        if (tempObj != null)
                        {
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
                    newBookingChildId = item.BookingChildID;
                    item.BookingChildID = newBookingChildId;
                    item.EntityState = EntityState.Added;
                    item.DateAdded = DateTime.Now;
                    foreach (FBookingAcknowledgeChildAddProcess childAddProc in item.ChildAddProcess)
                    {
                        var obj = CommonFunction.DeepClone(childAddProc);
                        obj.BookingChildID = newBookingChildId;
                        obj.BookingID = item.BookingID;
                        obj.ConsumptionID = item.ConsumptionID;
                        obj.EntityState = EntityState.Added;
                        newEntityChildAddProcess.Add(obj);
                    }
                    foreach (FBookingAcknowledgeChildDetails ChildDetail in item.FBChildDetails)
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
                    }
                    foreach (FBookingAcknowledgeChildGarmentPart ChildsGp in item.ChildsGpart)
                    {
                        FBookingAcknowledgeChildGarmentPart obj = CommonFunction.DeepClone(ChildsGp);
                        obj.BookingChildID = newBookingChildId;
                        obj.BookingID = item.BookingID;
                        obj.ConsumptionID = item.ConsumptionID;
                        obj.FUPartID = item.FUPartID;
                        obj.EntityState = EntityState.Added;
                        newEntityChildsGpart.Add(obj);
                    }
                    foreach (FBookingAcknowledgeChildProcess ChildsProc in item.ChildsProcess)
                    {
                        FBookingAcknowledgeChildProcess obj = CommonFunction.DeepClone(ChildsProc);
                        obj.BookingChildID = newBookingChildId;
                        obj.BookingID = item.BookingID;
                        obj.ConsumptionID = item.ConsumptionID;
                        obj.EntityState = EntityState.Added;
                        newEntityChildsProcess.Add(obj);
                    }
                    foreach (FBookingAcknowledgeChildText ChildTxt in item.ChildsText)
                    {
                        FBookingAcknowledgeChildText obj = CommonFunction.DeepClone(ChildTxt);
                        obj.BookingChildID = newBookingChildId;
                        obj.BookingID = item.BookingID;
                        obj.ConsumptionID = item.ConsumptionID;
                        obj.EntityState = EntityState.Added;
                        newEntityChildsText.Add(obj);
                    }
                    foreach (FBookingAcknowledgeChildDistribution ChildDis in item.ChildsDistribution)
                    {
                        FBookingAcknowledgeChildDistribution obj = CommonFunction.DeepClone(ChildDis);
                        obj.BookingChildID = newBookingChildId;
                        obj.BookingID = item.BookingID;
                        obj.ConsumptionID = item.ConsumptionID;
                        obj.DeliveryDate = ChildDis.DeliveryDate;
                        obj.DistributionQty = ChildDis.DistributionQty;
                        obj.EntityState = EntityState.Added;
                        newEntityChildsDistribution.Add(obj);
                    }
                    foreach (FBookingAcknowledgeChildYarnSubBrand ChildYarnSubBrand in item.ChildsYarnSubBrand)
                    {
                        FBookingAcknowledgeChildYarnSubBrand obj = CommonFunction.DeepClone(ChildYarnSubBrand);
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


                fbMaster.FBookingChild.ForEach(x =>
                {
                    FBookingAcknowledgeChild obj = newEntityChilds.Find(y => y.ItemMasterID == x.ItemMasterID && y.SubGroupID == x.SubGroupID && y.EntityState != EntityState.Deleted);
                    if (obj == null)
                    {
                        x.EntityState = EntityState.Deleted;
                        newEntityChilds.Add(x);

                        newEntityChildAddProcess.Where(p => p.BookingChildID == x.BookingChildID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                        {
                            p.EntityState = EntityState.Deleted;
                        });

                        fbMaster.FBookingAcknowledgeChildAddProcess.Where(p => p.BookingChildID == x.BookingChildID && p.ConsumptionID == x.ConsumptionID).ToList().ForEach(p =>
                        {
                            FBookingAcknowledgeChildAddProcess objP = newEntityChildAddProcess.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID);
                            if (objP != null)
                            {
                                newEntityChildAddProcess.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                            }
                            else
                            {
                                p.EntityState = EntityState.Deleted;
                                newEntityChildAddProcess.Add(p);
                            }
                        });
                        fbMaster.FBookingChildDetails.Where(p => p.BookingChildID == x.BookingChildID).ToList().ForEach(p =>
                        {
                            FBookingAcknowledgeChildDetails objP = newEntityChildDetails.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID);
                            if (objP != null)
                            {
                                newEntityChildDetails.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                            }
                            else
                            {
                                p.EntityState = EntityState.Deleted;
                                newEntityChildDetails.Add(p);
                            }
                        });
                        fbMaster.FBookingAcknowledgeChildGarmentPart.Where(p => p.BookingChildID == x.BookingChildID).ToList().ForEach(p =>
                        {
                            FBookingAcknowledgeChildGarmentPart objP = newEntityChildsGpart.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID);
                            if (objP != null)
                            {
                                newEntityChildsGpart.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                            }
                            else
                            {
                                p.EntityState = EntityState.Deleted;
                                newEntityChildsGpart.Add(p);
                            }
                        });
                        fbMaster.FBookingAcknowledgeChildProcess.Where(p => p.BookingChildID == x.BookingChildID).ToList().ForEach(p =>
                        {
                            FBookingAcknowledgeChildProcess objP = newEntityChildsProcess.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID);
                            if (objP != null)
                            {
                                newEntityChildsProcess.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                            }
                            else
                            {
                                p.EntityState = EntityState.Deleted;
                                newEntityChildsProcess.Add(p);
                            }
                        });
                        fbMaster.FBookingAcknowledgeChildText.Where(p => p.BookingChildID == x.BookingChildID).ToList().ForEach(p =>
                        {
                            FBookingAcknowledgeChildText objP = newEntityChildsText.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID);
                            if (objP != null)
                            {
                                newEntityChildsText.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                            }
                            else
                            {
                                p.EntityState = EntityState.Deleted;
                                newEntityChildsText.Add(p);
                            }
                        });
                        fbMaster.FBookingAcknowledgeChildDistribution.Where(p => p.BookingChildID == x.BookingChildID).ToList().ForEach(p =>
                        {
                            FBookingAcknowledgeChildDistribution objP = newEntityChildsDistribution.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID);
                            if (objP != null)
                            {
                                newEntityChildsDistribution.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                            }
                            else
                            {
                                p.EntityState = EntityState.Deleted;
                                newEntityChildsDistribution.Add(p);
                            }
                        });
                        fbMaster.FBookingAcknowledgeChildYarnSubBrand.Where(p => p.BookingChildID == x.BookingChildID).ToList().ForEach(p =>
                        {
                            FBookingAcknowledgeChildYarnSubBrand objP = newEntityChildsYarnSubBrand.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID);
                            if (objP != null)
                            {
                                newEntityChildsYarnSubBrand.Find(pp => p.BookingChildID == p.BookingChildID && pp.ConsumptionID == p.ConsumptionID).EntityState = EntityState.Deleted;
                            }
                            else
                            {
                                p.EntityState = EntityState.Deleted;
                                newEntityChildsYarnSubBrand.Add(p);
                            }
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
                //entityChildAckLiabilityDetails = new;
                fbMaster.FBookingAcknowledgeImage.ForEach(x => x.EntityState = EntityState.Modified);
                entityChildsImage = fbMaster.FBookingAcknowledgeImage;
            }

            var fabricWastageGrids = await _service.GetFabricWastageGridAsync("BDS");
            foreach (FBookingAcknowledgeChild details in entityChilds.Where(x => x.EntityState != EntityState.Deleted))
            {


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

            List<FBookingAcknowledgementYarnLiability> entityFBYL = new List<FBookingAcknowledgementYarnLiability>();
            if (isRevised && !entity.IsUnAcknowledge)
            {
                entity.FBAckID = fbMaster.FBAckID;
                entityChilds.ForEach(x => x.AcknowledgeID = entity.FBAckID);

                entity.PreRevisionNo = preRevisionNo;
                entity.RevisionNo = entity.RevisionNo + 1;
                entity.RevisionDate = DateTime.Now;

                foreach (FBookingAcknowledgementYarnLiability YL in entity.FBookingAcknowledgementYarnLiabilityList)
                {
                    FBookingAcknowledgementYarnLiability objYL = new FBookingAcknowledgementYarnLiability();
                    objYL.ItemMasterID = YL.ItemMasterID;
                    objYL.UnitID = YL.UnitID;
                    objYL.LiabilityQty = YL.LiabilityQty;
                    objYL.BookingID = entity.BookingID;
                    entityFBYL.Add(objYL);
                }
            }
            List<FreeConceptMaster> entityFreeConcepts = new List<FreeConceptMaster>();
            List<FreeConceptMRMaster> entityFreeMR = new List<FreeConceptMRMaster>();
            await _service.SaveAsync(AppUser.UserCode, entity, entityChilds, entityChildAddProcess, entityChildDetails, entityChildsGpart, entityChildsProcess, entityChildsText, entityChildsDistribution, entityChildsYarnSubBrand, entityChildsImage, BDCalander, isBDS, entityFreeConcepts, entityFreeMR, entityChildsLiabilitiesDistribution, entityFBA, entityFBYL);

            //await _commonService.UpdateFreeConceptStatus(InterfaceFrom.FBookingAcknowledge, 0, grpConceptNo, entity.BookingID);
            return Ok();
        }

        private FabricBookingAcknowledge GetFabricBookingAck(FabricBookingAcknowledge fba, BookingMaster bookingMaster, SampleBookingMaster sampleBookingMaster, bool isUnAcknowledge)
        {
            int revisionNo = 0;
            if (bookingMaster.IsNull())
            {
                bookingMaster = new BookingMaster();
            }
            else
            {
                revisionNo = bookingMaster.RevisionNo;
            }

            if (sampleBookingMaster.IsNull())
            {
                sampleBookingMaster = new SampleBookingMaster();
            }
            else
            {
                revisionNo = sampleBookingMaster.RevisionNo;
            }


            bool isSample = bookingMaster.BookingID > 0 ? false : true;

            if (fba.IsNull())
            {
                fba = new FabricBookingAcknowledge();
            }

            fba.BookingID = isSample ? sampleBookingMaster.BookingID : bookingMaster.BookingID;
            fba.BOMMasterID = isSample ? 0 : bookingMaster.BOMMasterID;

            fba.ItemGroupID = isSample ? fba.ItemGroupID : bookingMaster.ItemGroupID;
            fba.SubGroupID = isSample ? fba.SubGroupID : bookingMaster.SubGroupID;
            fba.WithoutOB = isSample ? true : false;

            if (fba.AcknowledgeID == 0)
            {
                fba.AddedBy = AppUser.UserCode;
                fba.DateAdded = DateTime.Now;
                fba.EntityState = EntityState.Added;
            }
            else
            {
                fba.UpdatedBy = AppUser.UserCode;
                fba.DateUpdated = DateTime.Now;
                fba.EntityState = EntityState.Modified;
            }

            if (isUnAcknowledge)
            {
                fba.Status = false;
                fba.UnAcknowledgeBy = AppUser.UserCode;
                fba.UnAcknowledgeDate = DateTime.Now;
                fba.UnAcknowledge = true;
                if (revisionNo > 0)
                {
                    fba.PreProcessRevNo = revisionNo - 1;
                }
                else
                {
                    fba.PreProcessRevNo = revisionNo;
                }
            }
            else
            {
                fba.Status = true;
                fba.UnAcknowledgeBy = 0;
                fba.UnAcknowledgeDate = null;
                fba.UnAcknowledge = false;
                fba.AcknowledgeDate = DateTime.Now;
            }
            return fba;
        }
        private FBookingAcknowledge GetFBookingAck(FBookingAcknowledge fba, BookingMaster bookingMaster, SampleBookingMaster sampleBookingMaster, bool isUnAcknowledge)
        {
            bool isSample = bookingMaster.IsNotNull() ? false : true;

            if (fba.IsNull())
            {
                fba = new FBookingAcknowledge();
            }

            fba.BookingID = isSample ? sampleBookingMaster.BookingID : bookingMaster.BookingID;
            fba.BookingNo = isSample ? sampleBookingMaster.BookingNo : bookingMaster.BookingNo;
            fba.SLNo = isSample ? sampleBookingMaster.SLNo : "";
            fba.BookingDate = isSample ? sampleBookingMaster.BookingDate : bookingMaster.BookingDate;
            fba.BuyerID = isSample ? (int)sampleBookingMaster.BuyerID : bookingMaster.BuyerID;
            fba.BuyerTeamID = isSample ? (int)sampleBookingMaster.BuyerTeamID : bookingMaster.BuyerTeamID;
            fba.ExecutionCompanyID = isSample ? sampleBookingMaster.ExecutionCompanyID : bookingMaster.CompanyID;
            fba.SupplierID = isSample ? (int)sampleBookingMaster.SupplierID : bookingMaster.SupplierID;
            fba.StyleMasterID = isSample ? sampleBookingMaster.StyleMasterID : bookingMaster.StyleMasterID;
            fba.StyleNo = isSample ? sampleBookingMaster.StyleNo : "";
            fba.ExportOrderID = isSample ? sampleBookingMaster.ExportOrderID : bookingMaster.ExportOrderID;
            fba.BookingQty = isSample ? sampleBookingMaster.OrderQty : 0;
            fba.PreRevisionNo = isSample ? sampleBookingMaster.RevisionNo : bookingMaster.RevisionNo;
            //fba.RevisionNo = fba.RevisionNo;
            //fba.RevisionDate = fba.RevisionDate;
            fba.BomMasterId = isSample ? 0 : bookingMaster.BOMMasterID;
            fba.ItemGroupId = isSample ? sampleBookingMaster.ItemGroupID : bookingMaster.ItemGroupID;
            fba.SubGroupID = isSample ? sampleBookingMaster.SubGroupID : bookingMaster.SubGroupID;
            fba.StyleMasterID = isSample ? sampleBookingMaster.StyleMasterID : bookingMaster.StyleMasterID;
            fba.WithoutOB = isSample ? true : false;
            fba.IsSample = isSample;

            fba.SeasonID = isSample ? sampleBookingMaster.SeasonID : bookingMaster.SeasonID;


            if (fba.FBAckID == 0)
            {
                fba.AddedBy = AppUser.UserCode;
                fba.DateAdded = DateTime.Now;
                fba.EntityState = EntityState.Added;
            }
            else
            {
                fba.UpdatedBy = AppUser.UserCode;
                fba.DateUpdated = DateTime.Now;
                fba.EntityState = EntityState.Modified;
            }

            if (isUnAcknowledge)
            {
                fba.Status = false;
                fba.IsUnAcknowledge = true;
                fba.UnAcknowledgeBy = AppUser.UserCode;
                fba.UnAcknowledgeDate = DateTime.Now;
                fba.UnAcknowledgeReason = isSample ? sampleBookingMaster.UnAcknowledgeReason : bookingMaster.UnAcknowledgeReason;
                fba.AcknowledgeDate = null;
            }
            else
            {
                fba.Status = true;
                fba.IsUnAcknowledge = false;
                fba.UnAcknowledgeBy = 0;
                fba.UnAcknowledgeDate = null;
                fba.AcknowledgeDate = DateTime.Now;
            }
            return fba;
        }

        [Route("acknowledge")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Acknowledge(FBookingAcknowledge modelDynamic)
        {
            await _service.CheckIsBookingApprovedAsync(modelDynamic.BookingNo);

            FBookingAcknowledge model = CommonFunction.DeepClone(modelDynamic);
            FBookingAcknowledge entity = modelDynamic;
            int menuId = modelDynamic.MenuId;
            bool isRevised = modelDynamic.IsRevised;
            bool isUnAcknowledge = modelDynamic.IsUnAcknowledge;
            string unAcknowledgeReason = modelDynamic.UnAcknowledgeReason;
            int styleMasterId = 0;
            int bomMasterId = 0;
            int userCode = AppUser.UserCode;
            string listTypeMasterGrid = modelDynamic.ListTypeMasterGrid;
            DateTime currentDate = DateTime.Now;

            bool SendMailTrueOrFalse = false;

            String WithoutOB = modelDynamic.IsSample == true ? "1".ToString() : "0";
            String EditType = modelDynamic.ActionStatus == "30" ? "N" : modelDynamic.ActionStatus == "10" ? "R" : "";

            String BookingID = modelDynamic.BookingID.ToString();
            String BookingNo = modelDynamic.BookingNo.ToString();
            String SaveType = modelDynamic.SaveType.ToString();
            List<BookingChild> updatedDataNew = new List<BookingChild>();
            List<BookingMaster> bookingMasters = new List<BookingMaster>();
            List<SampleBookingMaster> sampleBookingMasters = new List<SampleBookingMaster>();

            if (BookingNo.IsNotNullOrEmpty())
            {
                if (WithoutOB == "0")
                {
                    updatedDataNew = await _service.GetAllInHouseBookingByBookingNo(BookingNo);
                    bookingMasters = await _service.GetBookingMasterByNo(BookingNo);
                }
                else
                {
                    updatedDataNew = await _service.GetAllInHouseSampleBookingByBookingNo(BookingNo); //SampleBookingChild Return in BookingChild
                    sampleBookingMasters = await _service.GetBookingMasterByNoSample(BookingNo);
                }
            }
            String selectedbookingID = String.Empty;
            var strArr = updatedDataNew.Select(i => i.BookingId.ToString()).Distinct().ToArray();
            selectedbookingID += string.Join(",", strArr.ToArray());
            FabricBookingAcknowledge savedFBA = await _service.GetAllSavedFBAcknowledgeByBookingID(selectedbookingID == "" ? "0" : selectedbookingID, isRevised);

            #region Save fabric booking ack from textile
            bookingMasters.ForEach(x =>
            {
                FabricBookingAcknowledge fba = savedFBA.FabricBookingAcknowledgeList.Find(y => y.BookingID == x.BookingID && y.SubGroupID == x.SubGroupID);

                int revisonNo = updatedDataNew.Find(z => z.BookingId == x.BookingID).TechPackId;
                var objChild = updatedDataNew.Find(z => z.BookingId == x.BookingID && z.SubGroupId == x.SubGroupID);
                if (objChild.IsNull()) objChild = new BookingChild();
                if (fba.IsNull())
                {
                    fba = new FabricBookingAcknowledge();
                    fba.EntityState = EntityState.Added;


                    int itemGroupID = objChild.ItemGroupId;
                    int nBOMMasterID = objChild.BomMasterId;
                    x.ItemGroupID = itemGroupID;
                    x.BOMMasterID = nBOMMasterID;

                    fba.SubGroupID = x.SubGroupID;
                    fba.ItemGroupID = x.ItemGroupID;
                    fba.BOMMasterID = x.BOMMasterID;
                    fba.AcknowledgeDate = currentDate;
                    //fba.PreProcessRevNo = revisonNo;

                    ////if (isRevised) fba.RevisionNo = fba.RevisionNo + 1;
                    //fba.RevisionNo = revisonNo;
                    if (SaveType != "UA")
                    {
                        fba.PreProcessRevNo = revisonNo;
                        fba.RevisionNo = revisonNo;
                    }
                    else
                    {
                        fba.PreProcessRevNo = revisonNo - 1;
                    }
                    savedFBA.FabricBookingAcknowledgeList.Add(this.GetFabricBookingAck(fba, x, null, isUnAcknowledge));
                }
                else
                {
                    int nBOMMasterID = objChild.BomMasterId;
                    //fba.PreProcessRevNo = revisonNo;
                    fba.BOMMasterID = nBOMMasterID;//x.BOMMasterID;

                    //if (isRevised) fba.RevisionNo = revisonNo;//fba.RevisionNo + 1;
                    if (SaveType != "UA")
                    {
                        fba.PreProcessRevNo = revisonNo;
                        if (isRevised)
                        {
                            fba.RevisionNo = revisonNo;//fba.RevisionNo + 1;
                        }
                    }
                    else
                    {
                        fba.PreProcessRevNo = revisonNo - 1;
                    }
                    fba.EntityState = EntityState.Modified;
                }

                x.SeasonID = objChild.LengthYds;

            });
            sampleBookingMasters.ForEach(x =>
            {
                updatedDataNew.ForEach(c =>
                {
                    FabricBookingAcknowledge fba = savedFBA.FabricBookingAcknowledgeList.Find(y => y.BookingID == x.BookingID && y.SubGroupID == c.SubGroupId);
                    var objChild = updatedDataNew.Find(z => z.BookingId == x.BookingID && z.SubGroupId == x.SubGroupID);
                    if (objChild.IsNull()) objChild = new BookingChild();
                    if (fba.IsNull())
                    {
                        fba = new FabricBookingAcknowledge();
                        fba.EntityState = EntityState.Added;

                        fba.SubGroupID = c.SubGroupId;
                        fba.ItemGroupID = c.ItemGroupId;

                        //if (isRevised) fba.RevisionNo = fba.RevisionNo + 1;
                        if (SaveType != "UA")
                        {
                            fba.PreProcessRevNo = c.TechPackId;
                            fba.RevisionNo = c.TechPackId;
                        }
                        else
                        {
                            if (c.TechPackId > 0)
                            {
                                fba.PreProcessRevNo = c.TechPackId - 1;
                            }
                            else
                            {
                                fba.PreProcessRevNo = c.TechPackId;
                            }
                        }
                        savedFBA.FabricBookingAcknowledgeList.Add(this.GetFabricBookingAck(fba, null, x, isUnAcknowledge));
                    }
                    else
                    {
                        if (SaveType != "UA")
                        {
                            fba.PreProcessRevNo = c.TechPackId;
                            if (isRevised)
                            {
                                fba.RevisionNo = c.TechPackId;//fba.RevisionNo + 1;
                            }
                        }
                        else
                        {
                            if (c.TechPackId > 0)
                            {
                                fba.PreProcessRevNo = c.TechPackId - 1;
                            }
                            else
                            {
                                fba.PreProcessRevNo = c.TechPackId;
                            }
                        }

                        fba.EntityState = EntityState.Modified;
                    }

                    x.SeasonID = objChild.LengthYds;
                });
            });
            #endregion

            //modelDynamic.FBookingAcknowledgeList
            #region Save fbooking ack from textile
            if (modelDynamic.FBookingAcknowledgeList.Count() == 0 || savedFBA.FabricBookingAcknowledgeList.Count() != savedFBA.FBookingAcknowledgeList.Count())
            {
                bookingMasters.ForEach(x =>
                {
                    int index = savedFBA.FBookingAcknowledgeList.FindIndex(y => y.BookingID == x.BookingID);
                    if (index == -1)
                    {
                        FBookingAcknowledge fba = savedFBA.FBookingAcknowledgeList.Find(y => y.BookingID == x.BookingID);
                        if (fba.IsNull()) fba = new FBookingAcknowledge();
                        savedFBA.FBookingAcknowledgeList.Add(this.GetFBookingAck(fba, x, null, isUnAcknowledge));
                    }
                });
                sampleBookingMasters.ForEach(x =>
                {
                    int index = savedFBA.FBookingAcknowledgeList.FindIndex(y => y.BookingID == x.BookingID);
                    if (index == -1)
                    {
                        FBookingAcknowledge fba = savedFBA.FBookingAcknowledgeList.Find(y => y.BookingID == x.BookingID);
                        if (fba.IsNull()) fba = new FBookingAcknowledge();
                        savedFBA.FBookingAcknowledgeList.Add(this.GetFBookingAck(fba, null, x, isUnAcknowledge));
                    }
                });

                if (SaveType != "UA")
                {
                    savedFBA.FBookingAcknowledgeList.ForEach(x =>
                    {
                        FabricBookingAcknowledge fba = savedFBA.FabricBookingAcknowledgeList.Find(y => y.BookingID == x.BookingID && y.SubGroupID == x.SubGroupID);
                        if (fba.IsNotNull())
                        {
                            x.PreRevisionNo = fba.RevisionNo;
                        }
                    });
                }
            }

            #endregion

            if (SaveType == "UA")
            {
                List<BookingItemAcknowledge> saveBookingItemAcknowledgeList = new List<BookingItemAcknowledge>();
                if (WithoutOB == "0")
                {
                    saveBookingItemAcknowledgeList = await _service.GetAllBookingItemAcknowledgeByBookingNo(BookingNo);
                }
                else
                {
                    saveBookingItemAcknowledgeList = await _service.GetAllBookingItemAcknowledgeByBookingIDAndWithOutOB(selectedbookingID == "" ? "0" : selectedbookingID);
                }
                if (EditType == "N")
                {
                    saveBookingItemAcknowledgeList = new List<BookingItemAcknowledge>();
                    foreach (BookingChild bc in updatedDataNew)
                    {
                        FabricBookingAcknowledge objFabricBookingItemAcknowledge = savedFBA.FabricBookingAcknowledgeList.Find(i => i.BookingID == bc.BookingId && i.SubGroupID == bc.SubGroupId);

                        if (objFabricBookingItemAcknowledge.IsNull())
                        {
                            objFabricBookingItemAcknowledge = new FabricBookingAcknowledge();
                            objFabricBookingItemAcknowledge.AddedBy = AppUser.AddedBy;
                            objFabricBookingItemAcknowledge.DateAdded = currentDate;
                            savedFBA.FabricBookingAcknowledgeList.Add(objFabricBookingItemAcknowledge);
                        }

                        objFabricBookingItemAcknowledge.BookingID = bc.BookingId;
                        objFabricBookingItemAcknowledge.BOMMasterID = bc.BomMasterId;

                        objFabricBookingItemAcknowledge.ItemGroupID = bc.ItemGroupId;
                        objFabricBookingItemAcknowledge.SubGroupID = bc.SubGroupId;

                        objFabricBookingItemAcknowledge.Status = !isUnAcknowledge;

                        objFabricBookingItemAcknowledge.WithoutOB = WithoutOB == "1" ? true : false;

                        if (isUnAcknowledge)
                        {
                            objFabricBookingItemAcknowledge.UnAcknowledgeDate = currentDate;
                            objFabricBookingItemAcknowledge.UnAcknowledge = true;
                            objFabricBookingItemAcknowledge.UnAcknowledgeBy = AppUser.UserCode;
                            objFabricBookingItemAcknowledge.UnAcknowledgeReason = unAcknowledgeReason;

                            objFabricBookingItemAcknowledge.AcknowledgeDate = currentDate;
                        }
                        else
                        {
                            objFabricBookingItemAcknowledge.AcknowledgeDate = currentDate;
                            objFabricBookingItemAcknowledge.UnAcknowledge = false;
                            objFabricBookingItemAcknowledge.UnAcknowledgeBy = 0;
                            objFabricBookingItemAcknowledge.RevisionNo = 0;
                            objFabricBookingItemAcknowledge.PreProcessRevNo = 0;
                        }
                    }
                }
                foreach (BookingItemAcknowledge bc in saveBookingItemAcknowledgeList)
                {
                    //bc.RevisionNo = bc.RevisionNo - 1;
                    //bc.RevisionNo = bc.RevisionNo < 0 ? 0 : bc.RevisionNo;
                    FabricBookingAcknowledge objFabricBookingItemAcknowledge = savedFBA.FabricBookingAcknowledgeList.Find(i => i.BookingID == bc.BookingID && i.SubGroupID == bc.SubGroupID);
                    //if (objFabricBookingItemAcknowledge.IsNotNull())
                    //{
                    //    objFabricBookingItemAcknowledge.PreProcessRevNo = bc.RevisionNo;
                    //}

                    if (isUnAcknowledge)
                    {
                        bc.UnAcknowledge = true;
                        bc.UnAcknowledgeBy = AppUser.UserCode;
                        bc.UnAcknowledgeDate = currentDate;

                        bc.AcknowledgeDate = null;
                    }
                    else
                    {
                        bc.UnAcknowledge = false;
                        bc.UnAcknowledgeBy = 0;
                        bc.UnAcknowledgeDate = null;

                        bc.AcknowledgeDate = currentDate;
                    }
                }

                foreach (FabricBookingAcknowledge bc in savedFBA.FabricBookingAcknowledgeList)
                {
                    //bc.RevisionNo = bc.RevisionNo - 1;
                    //bc.RevisionNo = bc.RevisionNo < 0 ? 0 : bc.RevisionNo;

                    bc.UnAcknowledge = true;
                    bc.UnAcknowledgeBy = AppUser.UserCode;
                    bc.UnAcknowledgeDate = currentDate;
                    bc.Status = !isUnAcknowledge;
                }

                #region SaveFBookingAcknowledge
                List<FBookingAcknowledge> saveFBookingAcknowledge = savedFBA.FBookingAcknowledgeList;
                if (saveFBookingAcknowledge.IsNull())
                    saveFBookingAcknowledge = new List<FBookingAcknowledge>();
                #region Delete Existing Data

                #endregion

                if (sampleBookingMasters != null && sampleBookingMasters.Count() > 0)
                {
                    modelDynamic.FBookingAcknowledgeList = modelDynamic.FBookingAcknowledgeList.OrderBy(item => item.SubGroupID).Take(1).ToList();
                }
                foreach (FBookingAcknowledge fba in modelDynamic.FBookingAcknowledgeList)
                {
                    FBookingAcknowledge fbMaster = saveFBookingAcknowledge.Find(i => i.BookingID == fba.BookingID && i.SubGroupID == fba.SubGroupID);
                    //fbMaster = await _service.GetFBAcknowledgeByBookingID(BookingID.ToInt());

                    int preRevisionNo = 0;
                    if (bookingMasters != null && bookingMasters.Count() > 0) preRevisionNo = bookingMasters.First().RevisionNo;
                    else if (sampleBookingMasters != null && sampleBookingMasters.Count() > 0) preRevisionNo = sampleBookingMasters.First().RevisionNo;

                    if (fbMaster.IsNull())
                    {
                        fbMaster = new FBookingAcknowledge();
                        fbMaster.AddedBy = AppUser.UserCode;
                        fbMaster.DateAdded = currentDate;
                        fbMaster.IsSample = model.IsSample;
                        //fbMaster.PreRevisionNo = preRevisionNo;
                        //fbMaster.RevisionNo = fbMaster.PreRevisionNo;
                        fbMaster.RevisionDate = currentDate;
                        saveFBookingAcknowledge.Add(fbMaster);
                    }
                    else
                    {
                        fbMaster.UpdatedBy = AppUser.UserCode;
                        fbMaster.DateUpdated = currentDate;
                        //fbMaster.PreRevisionNo = preRevisionNo;
                        //fbMaster.RevisionNo = fbMaster.PreRevisionNo;
                        fbMaster.RevisionDate = currentDate;
                        fbMaster.EntityState = EntityState.Modified;
                    }
                    List<FabricBookingAcknowledge> saveFabricBookingItemAcknowledge = savedFBA.FabricBookingAcknowledgeList;
                    if (saveFabricBookingItemAcknowledge.IsNull())
                        saveFabricBookingItemAcknowledge = new List<FabricBookingAcknowledge>();
                    FabricBookingAcknowledge objFabricBookingItemAcknowledge = saveFabricBookingItemAcknowledge.Find(i => i.BookingID == fba.BookingID && i.SubGroupID == fba.SubGroupID);

                    if (!isRevised)
                    {
                        fbMaster.PreRevisionNo = objFabricBookingItemAcknowledge.IsNotNull() ? objFabricBookingItemAcknowledge.PreProcessRevNo : fbMaster.PreRevisionNo;
                    }

                    fbMaster.BookingID = fba.BookingID;
                    fbMaster.BookingNo = modelDynamic.BookingNo;
                    fbMaster.SLNo = modelDynamic.SLNo;
                    fbMaster.BookingDate = modelDynamic.BookingDate;
                    fbMaster.BuyerID = modelDynamic.BuyerID;
                    fbMaster.BuyerTeamID = modelDynamic.BuyerTeamID;
                    fbMaster.ExecutionCompanyID = modelDynamic.ExecutionCompanyID;
                    fbMaster.SupplierID = modelDynamic.SupplierID;
                    fbMaster.StyleMasterID = modelDynamic.StyleMasterID;
                    fbMaster.StyleNo = modelDynamic.StyleNo;
                    fbMaster.SeasonID = modelDynamic.SeasonID;
                    fbMaster.FinancialYearID = modelDynamic.FinancialYearID;
                    fbMaster.ExportOrderID = modelDynamic.ExportOrderID;
                    fbMaster.BookingQty = modelDynamic.BookingQty;
                    fbMaster.BomMasterId = fba.BomMasterId;
                    fbMaster.SubGroupID = fba.SubGroupID;
                    fbMaster.ItemGroupId = fba.ItemGroupId;
                    fbMaster.MerchandiserID = AppUser.UserCode;
                    fbMaster.IsUnAcknowledge = true;
                    fbMaster.UnAcknowledgeBy = AppUser.UserCode;
                    fbMaster.UnAcknowledgeDate = currentDate;
                    fbMaster.UnAcknowledgeReason = modelDynamic.UnAcknowledgeReason.Trim();
                }
                #endregion


                bool hasLiabilities = false;

                if (WithoutOB == "0")
                {
                    List<BookingMaster> saveBookingMasterList = new List<BookingMaster>();
                    saveBookingMasterList = await _service.GetAllBookingMasterByIDAsync(selectedbookingID == "" ? "0" : selectedbookingID);
                    foreach (BookingMaster bc in saveBookingMasterList)
                    {
                        bc.UnAcknowledgeReason = modelDynamic.UnAcknowledgeReason.Trim();
                    }
                    saveFBookingAcknowledge.ForEach(x =>
                    {
                        if (isUnAcknowledge)
                        {
                            x.UnAcknowledgeReason = unAcknowledgeReason;
                        }
                        else
                        {
                            x.IsUnAcknowledge = false;
                            x.UnAcknowledgeBy = 0;
                            x.UnAcknowledgeDate = null;
                            x.UnAcknowledgeReason = "";
                        }
                    });
                    savedFBA.FabricBookingAcknowledgeList.ForEach(x => x.MenuId = menuId);

                    await _service.SaveAsync(AppUser.UserCode, saveBookingItemAcknowledgeList, savedFBA.FabricBookingAcknowledgeList, saveFBookingAcknowledge, saveBookingMasterList, WithoutOB, isRevised, SaveType);

                    #region RollBack Booking
                    //if (BookingNo.IsNotNullOrEmpty())
                    //{
                    //    await _service.RollBackFabricBookingData(BookingNo, WithoutOB);
                    //}
                    #endregion

                    if (SaveType == "UA")
                    {
                        Boolean IsSendMail = true;
                        saveBookingItemAcknowledgeList = new List<BookingItemAcknowledge>();
                        if (WithoutOB == "0")
                        {
                            List<BookingMaster> bmList = new List<BookingMaster>();
                            if (savedFBA.FabricBookingAcknowledgeList.Count > 0)
                            {
                                BookingMaster bm = await _service.GetAllBookingAsync(savedFBA.FabricBookingAcknowledgeList[0].BookingID);
                                if (bm.IsNotNull())
                                    bmList.Add(bm);

                                if (isUnAcknowledge)
                                {
                                    saveBookingItemAcknowledgeList = await _service.GetAllBookingItemAcknowledgeByBookingNo(BookingNo);
                                    // OFF FOR CORE //IsSendMail = await SystemMailUnAck(saveBookingItemAcknowledgeList, bmList, IsSendMail, SaveType, false, listTypeMasterGrid);
                                }
                                else
                                {
                                    // OFF FOR CORE //IsSendMail = await SystemMail(savedFBA.FabricBookingAcknowledgeList, bmList, IsSendMail, SaveType, hasLiabilities, 0, listTypeMasterGrid);
                                }
                            }
                        }
                        else
                        {
                            List<SampleBookingMaster> bmList = new List<SampleBookingMaster>();
                            if (savedFBA.FabricBookingAcknowledgeList.Count > 0)
                            {
                                SampleBookingMaster bm = await _service.GetAllAsync(savedFBA.FabricBookingAcknowledgeList[0].BookingID);
                                if (bm.IsNotNull())
                                    bmList.Add(bm);

                                if (isUnAcknowledge)
                                {
                                    saveBookingItemAcknowledgeList = await _service.GetAllBookingItemAcknowledgeByBookingIDAndWithOutOB(selectedbookingID == "" ? "0" : selectedbookingID);
                                    // OFF FOR CORE //IsSendMail = await SystemMailForSampleUnAck(saveBookingItemAcknowledgeList, bmList, IsSendMail, SaveType, false, listTypeMasterGrid);
                                }
                                else
                                {
                                    // OFF FOR CORE //IsSendMail = await SystemMailForSample(savedFBA.FabricBookingAcknowledgeList, bmList, IsSendMail, SaveType, hasLiabilities, 0, listTypeMasterGrid);
                                }
                            }
                        }

                        SendMailTrueOrFalse = IsSendMail;
                    }
                }
                else
                {
                    List<SampleBookingMaster> saveSampleBookingMasterList = new List<SampleBookingMaster>();
                    saveSampleBookingMasterList = await _service.GetAllSampleBookingByIDAsync(selectedbookingID == "" ? "0" : selectedbookingID);
                    foreach (SampleBookingMaster bc in saveSampleBookingMasterList)
                    {
                        bc.UnAcknowledge = true;
                        bc.UnAcknowledgeReason = modelDynamic.UnAcknowledgeReason.Trim();
                    }
                    saveFBookingAcknowledge.ForEach(x =>
                    {
                        if (isUnAcknowledge)
                        {
                            x.UnAcknowledgeReason = unAcknowledgeReason;
                        }
                        else
                        {
                            x.IsUnAcknowledge = false;
                            x.UnAcknowledgeBy = 0;
                            x.UnAcknowledgeDate = null;
                            x.UnAcknowledgeReason = "";
                        }
                    });
                    savedFBA.FabricBookingAcknowledgeList.ForEach(x => x.MenuId = menuId);
                    await _service.SaveAsync(AppUser.UserCode, saveBookingItemAcknowledgeList, savedFBA.FabricBookingAcknowledgeList, saveFBookingAcknowledge, saveSampleBookingMasterList, isRevised, SaveType);

                    #region System Mail

                    if (SaveType == "UA")
                    {
                        Boolean IsSendMail = true;
                        saveBookingItemAcknowledgeList = new List<BookingItemAcknowledge>();
                        if (WithoutOB == "0")
                        {
                            List<BookingMaster> bmList = new List<BookingMaster>();
                            if (savedFBA.FabricBookingAcknowledgeList.Count > 0)
                            {
                                BookingMaster bm = await _service.GetAllBookingAsync(savedFBA.FabricBookingAcknowledgeList[0].BookingID);
                                if (bm.IsNotNull())
                                    bmList.Add(bm);

                                if (isUnAcknowledge)
                                {
                                    saveBookingItemAcknowledgeList = await _service.GetAllBookingItemAcknowledgeByBookingNo(BookingNo);
                                    // OFF FOR CORE //IsSendMail = await SystemMailUnAck(saveBookingItemAcknowledgeList, bmList, IsSendMail, SaveType, false, listTypeMasterGrid);
                                }
                                else
                                {
                                    // OFF FOR CORE //IsSendMail = await SystemMail(savedFBA.FabricBookingAcknowledgeList, bmList, IsSendMail, SaveType, hasLiabilities, 0, listTypeMasterGrid);
                                }
                            }
                        }
                        else
                        {
                            List<SampleBookingMaster> bmList = new List<SampleBookingMaster>();
                            if (savedFBA.FabricBookingAcknowledgeList.Count > 0)
                            {
                                SampleBookingMaster bm = await _service.GetAllAsync(savedFBA.FabricBookingAcknowledgeList[0].BookingID);
                                if (bm.IsNotNull())
                                    bmList.Add(bm);

                                if (isUnAcknowledge)
                                {
                                    saveBookingItemAcknowledgeList = await _service.GetAllBookingItemAcknowledgeByBookingIDAndWithOutOB(selectedbookingID == "" ? "0" : selectedbookingID);
                                    // OFF FOR CORE //IsSendMail = await SystemMailForSampleUnAck(saveBookingItemAcknowledgeList, bmList, IsSendMail, SaveType, false, listTypeMasterGrid);
                                }
                                else
                                {
                                    // OFF FOR CORE //IsSendMail = await SystemMailForSample(savedFBA.FabricBookingAcknowledgeList, bmList, IsSendMail, SaveType, hasLiabilities, 0, listTypeMasterGrid);
                                }
                            }
                        }
                        SendMailTrueOrFalse = IsSendMail;

                    }
                    else
                    {
                        Boolean IsSendMail = true;
                        saveBookingItemAcknowledgeList = new List<BookingItemAcknowledge>();
                        if (WithoutOB == "0")
                        {
                            List<BookingMaster> bmList = new List<BookingMaster>();
                            if (savedFBA.FabricBookingAcknowledgeList.Count > 0)
                            {
                                BookingMaster bm = await _service.GetAllBookingAsync(savedFBA.FabricBookingAcknowledgeList[0].BookingID);
                                if (bm.IsNotNull())
                                    bmList.Add(bm);

                                if (isUnAcknowledge)
                                {
                                    saveBookingItemAcknowledgeList = await _service.GetAllBookingItemAcknowledgeByBookingNo(BookingNo);
                                    // OFF FOR CORE //IsSendMail = await SystemMailUnAck(saveBookingItemAcknowledgeList, bmList, IsSendMail, SaveType, false, listTypeMasterGrid);
                                }
                                else
                                {
                                    // OFF FOR CORE //IsSendMail = await SystemMail(savedFBA.FabricBookingAcknowledgeList, bmList, IsSendMail, SaveType, hasLiabilities, 0, listTypeMasterGrid);
                                }
                            }
                        }
                        else
                        {
                            List<SampleBookingMaster> bmList = new List<SampleBookingMaster>();
                            if (savedFBA.FabricBookingAcknowledgeList.Count > 0)
                            {
                                SampleBookingMaster bm = await _service.GetAllAsync(savedFBA.FabricBookingAcknowledgeList[0].BookingID);
                                if (bm.IsNotNull())
                                    bmList.Add(bm);

                                if (isUnAcknowledge)
                                {
                                    saveBookingItemAcknowledgeList = await _service.GetAllBookingItemAcknowledgeByBookingIDAndWithOutOB(selectedbookingID == "" ? "0" : selectedbookingID);
                                    // OFF FOR CORE //IsSendMail = await SystemMailForSampleUnAck(saveBookingItemAcknowledgeList, bmList, IsSendMail, SaveType, false, listTypeMasterGrid);
                                }
                                else
                                {
                                    // OFF FOR CORE //IsSendMail = await SystemMailForSample(savedFBA.FabricBookingAcknowledgeList, bmList, IsSendMail, SaveType, hasLiabilities, 0, listTypeMasterGrid);
                                }
                            }
                        }

                        SendMailTrueOrFalse = IsSendMail;
                    }
                    #endregion
                }


            }
            else
            {
                #region SaveFabricItemAcknowledgement
                List<FabricBookingAcknowledge> saveFabricBookingItemAcknowledge = savedFBA.FabricBookingAcknowledgeList;
                if (saveFabricBookingItemAcknowledge.IsNull())
                    saveFabricBookingItemAcknowledge = new List<FabricBookingAcknowledge>();

                #region Add Item In List
                foreach (FabricBookingAcknowledge bc in modelDynamic.FabricBookingAcknowledgeList)
                {
                    FabricBookingAcknowledge objFabricBookingItemAcknowledge = saveFabricBookingItemAcknowledge.Find(i => i.BookingID == bc.BookingID && i.ItemGroupID == bc.ItemGroupID && i.SubGroupID == bc.SubGroupID);
                    int preProcessRevNo = 0;
                    if (bookingMasters != null && bookingMasters.Count() > 0) preProcessRevNo = bookingMasters.First().RevisionNo;
                    else if (sampleBookingMasters != null && sampleBookingMasters.Count() > 0) preProcessRevNo = sampleBookingMasters.First().RevisionNo;

                    if (objFabricBookingItemAcknowledge.IsNull())
                    {
                        objFabricBookingItemAcknowledge = new FabricBookingAcknowledge();
                        objFabricBookingItemAcknowledge.DateAdded = currentDate;
                        objFabricBookingItemAcknowledge.AddedBy = AppUser.UserCode;
                        objFabricBookingItemAcknowledge.PreProcessRevNo = preProcessRevNo;
                        objFabricBookingItemAcknowledge.RevisionNo = objFabricBookingItemAcknowledge.PreProcessRevNo;
                        saveFabricBookingItemAcknowledge.Add(objFabricBookingItemAcknowledge);
                    }
                    else
                    {
                        objFabricBookingItemAcknowledge.PreProcessRevNo = preProcessRevNo;
                        objFabricBookingItemAcknowledge.RevisionNo = objFabricBookingItemAcknowledge.PreProcessRevNo;
                        objFabricBookingItemAcknowledge.EntityState = EntityState.Modified;
                    }
                    if (objFabricBookingItemAcknowledge.EntityState != EntityState.Added)
                    {
                        objFabricBookingItemAcknowledge.UpdatedBy = AppUser.UserCode;
                        objFabricBookingItemAcknowledge.DateUpdated = currentDate;
                    }
                    objFabricBookingItemAcknowledge.BookingID = bc.BookingID;
                    objFabricBookingItemAcknowledge.BOMMasterID = bc.BOMMasterID;
                    bomMasterId = bc.BOMMasterID;

                    objFabricBookingItemAcknowledge.ItemGroupID = bc.ItemGroupID;
                    objFabricBookingItemAcknowledge.SubGroupID = bc.SubGroupID;

                    objFabricBookingItemAcknowledge.Status = !isUnAcknowledge;
                    objFabricBookingItemAcknowledge.AcknowledgeDate = currentDate;
                    objFabricBookingItemAcknowledge.RevisionNo = EditType == "Revise" || EditType == "R" ? objFabricBookingItemAcknowledge.RevisionNo + 1 : objFabricBookingItemAcknowledge.RevisionNo;

                    objFabricBookingItemAcknowledge.WithoutOB = WithoutOB == "1" ? true : false;
                    objFabricBookingItemAcknowledge.UnAcknowledge = false;
                    objFabricBookingItemAcknowledge.UnAcknowledgeBy = 0;

                }
                #endregion
                #endregion
                #region SaveFBookingAcknowledge
                List<FBookingAcknowledge> saveFBookingAcknowledge = savedFBA.FBookingAcknowledgeList;
                if (saveFBookingAcknowledge.IsNull())
                    saveFBookingAcknowledge = new List<FBookingAcknowledge>();

                if (sampleBookingMasters != null && sampleBookingMasters.Count() > 0)
                {
                    modelDynamic.FBookingAcknowledgeList = modelDynamic.FBookingAcknowledgeList.OrderBy(item => item.SubGroupID).Take(1).ToList();
                }
                foreach (FBookingAcknowledge fba in modelDynamic.FBookingAcknowledgeList)
                {
                    FBookingAcknowledge fbMaster = saveFBookingAcknowledge.Find(i => i.BookingID == fba.BookingID && i.SubGroupID == fba.SubGroupID);
                    //fbMaster = await _service.GetFBAcknowledgeByBookingID(BookingID.ToInt());

                    int preRevisionNo = 0;
                    if (bookingMasters != null && bookingMasters.Count() > 0) preRevisionNo = bookingMasters.First().RevisionNo;
                    else if (sampleBookingMasters != null && sampleBookingMasters.Count() > 0) preRevisionNo = sampleBookingMasters.First().RevisionNo;

                    if (fbMaster.IsNull())
                    {
                        fbMaster = new FBookingAcknowledge();
                        fbMaster.AddedBy = AppUser.UserCode;
                        fbMaster.DateAdded = currentDate;
                        fbMaster.IsSample = model.IsSample;
                        fbMaster.PreRevisionNo = preRevisionNo;
                        //fbMaster.RevisionNo = fbMaster.PreRevisionNo;
                        fbMaster.RevisionDate = currentDate;

                        int indexF = saveFBookingAcknowledge.FindIndex(x => x.SubGroupID == fba.SubGroupID);
                        if (indexF > -1) saveFBookingAcknowledge.RemoveAt(indexF);

                        saveFBookingAcknowledge.Add(fbMaster);
                    }
                    else
                    {
                        fbMaster.UpdatedBy = AppUser.UserCode;
                        fbMaster.DateUpdated = currentDate;
                        fbMaster.PreRevisionNo = preRevisionNo;
                        //fbMaster.RevisionNo = fbMaster.PreRevisionNo;
                        fbMaster.RevisionDate = currentDate;
                        fbMaster.EntityState = EntityState.Modified;
                    }

                    FabricBookingAcknowledge objFabricBookingItemAcknowledge = saveFabricBookingItemAcknowledge.Find(i => i.BookingID == fba.BookingID && i.ItemGroupID == fba.ItemGroupId && i.SubGroupID == fba.SubGroupID);

                    if (!isRevised)
                    {
                        fbMaster.PreRevisionNo = objFabricBookingItemAcknowledge.IsNotNull() ? objFabricBookingItemAcknowledge.PreProcessRevNo : fbMaster.PreRevisionNo;
                    }

                    fbMaster.BookingID = fba.BookingID;
                    fbMaster.BookingNo = modelDynamic.BookingNo;
                    fbMaster.SLNo = modelDynamic.SLNo;
                    fbMaster.BookingDate = modelDynamic.BookingDate;
                    fbMaster.BuyerID = modelDynamic.BuyerID;
                    fbMaster.BuyerTeamID = modelDynamic.BuyerTeamID;
                    fbMaster.ExecutionCompanyID = modelDynamic.ExecutionCompanyID;
                    fbMaster.SupplierID = modelDynamic.SupplierID;
                    fbMaster.StyleMasterID = modelDynamic.StyleMasterID;
                    fbMaster.StyleNo = modelDynamic.StyleNo;
                    fbMaster.SeasonID = modelDynamic.SeasonID;
                    fbMaster.FinancialYearID = modelDynamic.FinancialYearID;
                    fbMaster.ExportOrderID = modelDynamic.ExportOrderID;
                    fbMaster.BookingQty = modelDynamic.BookingQty;
                    fbMaster.BomMasterId = fba.BomMasterId;
                    fbMaster.SubGroupID = fba.SubGroupID;
                    fbMaster.ItemGroupId = fba.ItemGroupId;
                    fbMaster.MerchandiserID = AppUser.UserCode;

                    bomMasterId = fbMaster.BomMasterId;
                    styleMasterId = fbMaster.StyleMasterID;
                }
                #endregion
                #region SaveFBookingAcknowledgeChild
                List<FBookingAcknowledgeChild> saveFBookingAcknowledgeChild = savedFBA.FBookingChild;
                saveFBookingAcknowledgeChild.SetUnchanged();
                if (saveFBookingAcknowledgeChild.IsNull())
                    saveFBookingAcknowledgeChild = new List<FBookingAcknowledgeChild>();
                #region Delete Existing Data
                foreach (FBookingAcknowledgeChild item in saveFBookingAcknowledgeChild)
                {
                    //if (item.BookingChildID == 774600)
                    //{

                    //}
                    //List<FBookingAcknowledgeChild> objItem = modelDynamic.FBookingChild.FindAll(i => i.BookingID == item.BookingID && i.ItemMasterID == item.ItemMasterID && i.BookingChildID == item.BookingChildID);
                    List<FBookingAcknowledgeChild> objItem = modelDynamic.FBookingChild.FindAll(i => i.BookingID == item.BookingID && i.ItemMasterID == item.ItemMasterID && i.ConsumptionID == item.ConsumptionID);
                    if (objItem.IsNull())
                        item.EntityState = EntityState.Deleted;

                }

                #endregion
                bool hasLiabilities = false;
                foreach (FBookingAcknowledgeChild item in modelDynamic.FBookingChild)
                {
                    int iBookingID = item.BookingID;
                    int iItemMasterID = item.ItemMasterID;
                    int iConsumptionID = item.ConsumptionID;
                    //if (item.BookingChildID == 774600)
                    //{

                    //}

                    //var aaa = saveFBookingAcknowledgeChild.Where(i => i.BookingID == item.BookingID && i.ItemMasterID == item.ItemMasterID && i.ConsumptionID == item.ConsumptionID);

                    FBookingAcknowledgeChild ObjEntityChild = saveFBookingAcknowledgeChild.Find(i => i.BookingID == item.BookingID && i.ItemMasterID == item.ItemMasterID && i.ConsumptionID == item.ConsumptionID && i.RevisionNoWhenDeleted == -1);

                    if (ObjEntityChild.IsNull())
                    {
                        ObjEntityChild = new FBookingAcknowledgeChild();
                        ObjEntityChild.AddedBy = AppUser.UserCode;
                        ObjEntityChild.DateAdded = currentDate;
                        saveFBookingAcknowledgeChild.Add(ObjEntityChild);
                    }
                    else
                    {
                        ObjEntityChild.UpdatedBy = AppUser.UserCode;
                        ObjEntityChild.DateUpdated = currentDate;
                        ObjEntityChild.EntityState = EntityState.Modified;
                    }
                    ObjEntityChild.ConsumptionChildID = sampleBookingMasters.Count() > 0 ? item.ConsumptionChildID : item.BookingChildID;
                    ObjEntityChild.BookingID = item.BookingID;
                    ObjEntityChild.ConsumptionID = item.ConsumptionID;
                    ObjEntityChild.SubGroupID = item.SubGroupID;
                    ObjEntityChild.ItemGroupID = item.ItemGroupID;
                    ObjEntityChild.ItemMasterID = item.ItemMasterID;
                    ObjEntityChild.BOMMasterID = item.BOMMasterID;
                    ObjEntityChild.OrderBankPOID = item.OrderBankPOID;
                    ObjEntityChild.ColorID = item.ColorID;
                    ObjEntityChild.TechPackID = 0;
                    ObjEntityChild.YarnBrandID = item.YarnBrandID;
                    ObjEntityChild.A1ValueID = item.A1ValueID;
                    ObjEntityChild.BookingUnitID = item.BookingUnitID;
                    ObjEntityChild.BookingQty = item.BookingQty;
                    ObjEntityChild.PreviousBookingQty = item.PreviousBookingQty;
                    ObjEntityChild.ActualBookingQty = item.ActualBookingQty;
                    ObjEntityChild.LiabilitiesBookingQty = item.LiabilitiesBookingQty;
                    ObjEntityChild.CurrentBookingQty = item.BookingQty;
                    ObjEntityChild.LengthInch = item.LengthInch;
                    ObjEntityChild.LengthYds = item.LengthYds;
                    ObjEntityChild.Remarks = item.Remarks;
                    ObjEntityChild.FUPartID = item.FUPartID;
                    ObjEntityChild.ConsumptionQty = item.ConsumptionQty;
                    ObjEntityChild.LabDipNo = item.LabDipNo;
                    ObjEntityChild.Price = item.Price;
                    ObjEntityChild.SuggestedPrice = item.SuggestedPrice;
                    ObjEntityChild.RequisitionQty = item.RequisitionQty;

                    bomMasterId = ObjEntityChild.BOMMasterID;

                    if (EditType == "N")
                    {
                        ObjEntityChild.IsTxtAck = true;
                        ObjEntityChild.TxtAcknowledgeBy = AppUser.UserCode;
                        ObjEntityChild.TxtAcknowledgeDate = currentDate;
                    }
                    if (EditType == "R" && ObjEntityChild.LiabilitiesBookingQty > 0)
                    {
                        ObjEntityChild.SendToMktAck = true;
                        ObjEntityChild.IsMktAck = false;

                        ObjEntityChild.AcknowledgeBy = AppUser.UserCode;
                        ObjEntityChild.AcknowledgeDate = currentDate;

                        FabricBookingAcknowledge objFabricBookingItemAcknowledge = saveFabricBookingItemAcknowledge.Find(i => i.BookingID == ObjEntityChild.BookingID);
                        if (objFabricBookingItemAcknowledge.IsNotNull())
                        {
                            objFabricBookingItemAcknowledge.Status = !isUnAcknowledge;
                        }
                    }
                    else
                    {
                        ObjEntityChild.IsTxtAck = true;
                        ObjEntityChild.TxtAcknowledgeBy = AppUser.UserCode;
                        ObjEntityChild.TxtAcknowledgeDate = currentDate;
                    }
                }

                if (saveFBookingAcknowledgeChild.Max(i => Convert.ToInt32(i.SendToMktAck)) == 1)
                {
                    foreach (FabricBookingAcknowledge item in saveFabricBookingItemAcknowledge)
                    {
                        item.Status = !isUnAcknowledge;
                    }
                    foreach (FBookingAcknowledgeChild item in saveFBookingAcknowledgeChild)
                    {
                        item.SendToMktAck = true;
                        item.IsTxtAck = false;
                        item.IsTxtUnAck = false;
                        item.SendToTxtAck = false;
                        item.IsMktAck = false;
                    }
                }
                if (saveFBookingAcknowledgeChild.Min(i => Convert.ToInt32(i.SendToMktAck)) == 0)
                {
                    foreach (FabricBookingAcknowledge item in saveFabricBookingItemAcknowledge)
                    {
                        item.Status = !isUnAcknowledge;
                    }
                    foreach (FBookingAcknowledgeChild item in saveFBookingAcknowledgeChild)
                    {
                        item.SendToMktAck = false;
                        item.IsTxtAck = true;
                        item.IsTxtUnAck = false;
                        item.SendToTxtAck = true;
                    }
                }
                saveFBookingAcknowledgeChild.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x =>
                {
                    //if (x.BookingChildID == 774600)
                    //{

                    //}


                    x.IsDeleted = true;
                    x.EntityState = EntityState.Modified;
                });

                #endregion

                #region SaveFBookingAcknowledgementLiabilityDistribution
                List<FBookingAcknowledgementLiabilityDistribution> modelDistributions = modelDynamic.FBookingAckLiabilityDistributionList;
                List<FBookingAcknowledgementLiabilityDistribution> entityDistributions = savedFBA.FBookingAckLiabilityDistributionList;

                entityDistributions.SetUnchanged();
                if (!isUnAcknowledge)
                {
                    modelDistributions.ForEach(md =>
                    {
                        if (md.LiabilityQty > 0)
                        {
                            var distributionObj = entityDistributions.Find(x => x.LiabilitiesProcessID == md.LiabilitiesProcessID && x.BookingChildID == md.BookingChildID && x.BookingID == md.BookingID);
                            if (distributionObj == null)
                            {
                                distributionObj = CommonFunction.DeepClone(md);
                                distributionObj.EntityState = EntityState.Added;
                                distributionObj.UnitID = 28;
                                entityDistributions.Add(distributionObj);
                            }
                            else
                            {
                                distributionObj.LiabilityQty = md.LiabilityQty;
                                distributionObj.ConsumedQty = md.ConsumedQty;
                                distributionObj.Rate = md.Rate;
                                distributionObj.BookingChildID = md.BookingChildID;
                                distributionObj.ConsumptionID = md.ConsumptionID;
                                distributionObj.UnitID = 28;
                                distributionObj.EntityState = EntityState.Modified;

                                int indexF = entityDistributions.FindIndex(x => x.LiabilitiesProcessID == md.LiabilitiesProcessID && x.BookingChildID == md.BookingChildID && x.BookingID == md.BookingID);
                                entityDistributions[indexF] = distributionObj;
                            }
                        }
                    });
                }
                entityDistributions.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();

                #endregion

                #region FBookingAcknowledgementYarnLiability
                List<FBookingAcknowledgementYarnLiability> modelYarnLiabilities = modelDynamic.FBookingAcknowledgementYarnLiabilityList;
                List<FBookingAcknowledgementYarnLiability> entityYarnLiabilities = savedFBA.FBookingAcknowledgementYarnLiabilityList;

                entityYarnLiabilities.SetUnchanged();
                if (!isUnAcknowledge)
                {
                    modelYarnLiabilities.ForEach(md =>
                    {
                        if (md.LiabilityQty > 0)
                        {
                            if (md.YLChildID == 0)
                            {
                                var yarnLiabilityObj1 = CommonFunction.DeepClone(md);
                                yarnLiabilityObj1.EntityState = EntityState.Added;
                                entityYarnLiabilities.Add(yarnLiabilityObj1);
                            }
                            else
                            {
                                var yarnLiabilityObj = entityYarnLiabilities.Find(x => x.YLChildID == md.YLChildID);
                                if (yarnLiabilityObj != null)
                                {
                                    yarnLiabilityObj.LiabilityQty = md.LiabilityQty;
                                    yarnLiabilityObj.Rate = md.Rate;
                                    yarnLiabilityObj.YBChildItemID = md.YBChildItemID;
                                    yarnLiabilityObj.AllocationChildID = md.AllocationChildID;
                                    yarnLiabilityObj.BookingChildID = md.BookingChildID;
                                    yarnLiabilityObj.ConsumptionID = md.ConsumptionID;
                                    yarnLiabilityObj.EntityState = EntityState.Modified;

                                    int indexF = entityYarnLiabilities.FindIndex(x => x.YLChildID == md.YLChildID);
                                    entityYarnLiabilities[indexF] = yarnLiabilityObj;
                                }
                            }
                        }
                    });
                }
                entityYarnLiabilities.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();

                #endregion

                if (modelDistributions.Count() > 0 || modelYarnLiabilities.Count() > 0)
                {
                    hasLiabilities = true;
                }
                //updatedDataNew
                if (updatedDataNew.Count() > 0)
                {
                    int revisionNo = updatedDataNew.First().TechPackId; //Can't Take field named RevisionNo cz if do so have to migrate. That's why use TechPackId for RevisionNo
                    saveFabricBookingItemAcknowledge.ForEach(x =>
                    {
                        x.PreProcessRevNo = revisionNo;
                        x.RevisionNo = revisionNo;
                    });
                    saveFBookingAcknowledge.ForEach(x =>
                    {
                        x.PreRevisionNo = revisionNo;
                        //x.RevisionNo = revisionNo;
                        if (isRevised)
                        {
                            x.RevisionNo = x.RevisionNo + 1;
                        }
                        x.RevisionDate = currentDate;
                    });

                    revisionNo = saveFBookingAcknowledge.Max(x => x.RevisionNo);

                    if (isRevised)
                    {
                        saveFBookingAcknowledgeChild.Where(x => x.BookingQty == 0 && x.RevisionNoWhenDeleted == -1).ToList().ForEach(x =>
                        {
                            //if (x.BookingChildID == 774600)
                            //{

                            //}


                            x.RevisionNoWhenDeleted = revisionNo;
                            x.RevisionByWhenDeleted = AppUser.UserCode;
                            x.RevisionDateWhenDeleted = currentDate;

                            x.IsDeleted = true;
                            x.EntityState = EntityState.Modified;
                        });
                    }
                }

                saveFBookingAcknowledge.ForEach(x =>
                {
                    if (isUnAcknowledge)
                    {
                        x.UnAcknowledgeReason = unAcknowledgeReason;
                    }
                    else
                    {
                        x.IsUnAcknowledge = false;
                        x.UnAcknowledgeBy = 0;
                        x.UnAcknowledgeDate = null;
                        x.UnAcknowledgeReason = "";

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
                    }
                });
                saveFabricBookingItemAcknowledge.ForEach(x => x.MenuId = menuId);

                await _service.SaveAsync(AppUser.UserCode, EditType, BookingNo, saveFabricBookingItemAcknowledge, saveFBookingAcknowledge, saveFBookingAcknowledgeChild, entityDistributions, entityYarnLiabilities, isRevised, WithoutOB, styleMasterId, bomMasterId, userCode, SaveType);

                Boolean IsSendMail = true;

                if (isRevised && hasLiabilities)
                {
                    IsSendMail = true;
                }

                if (IsSendMail)
                {
                    List<BookingItemAcknowledge> saveBookingItemAcknowledgeList = new List<BookingItemAcknowledge>();
                    if (WithoutOB == "0")
                    {
                        List<BookingMaster> bmList = new List<BookingMaster>();
                        if (savedFBA.FabricBookingAcknowledgeList.Count > 0)
                        {
                            BookingMaster bm = await _service.GetAllBookingAsync(savedFBA.FabricBookingAcknowledgeList[0].BookingID);
                            if (bm.IsNotNull())
                                bmList.Add(bm);

                            if (isUnAcknowledge)
                            {
                                saveBookingItemAcknowledgeList = await _service.GetAllBookingItemAcknowledgeByBookingNo(BookingNo);
                                // OFF FOR CORE //IsSendMail = await SystemMailUnAck(saveBookingItemAcknowledgeList, bmList, IsSendMail, SaveType, false, listTypeMasterGrid);
                            }
                            else
                            {
                                // OFF FOR CORE //IsSendMail = await SystemMail(savedFBA.FabricBookingAcknowledgeList, bmList, IsSendMail, SaveType, hasLiabilities, 0, listTypeMasterGrid);
                            }
                        }
                    }
                    else
                    {
                        List<SampleBookingMaster> bmList = new List<SampleBookingMaster>();
                        if (savedFBA.FabricBookingAcknowledgeList.Count > 0)
                        {
                            SampleBookingMaster bm = await _service.GetAllAsync(savedFBA.FabricBookingAcknowledgeList[0].BookingID);
                            if (bm.IsNotNull())
                                bmList.Add(bm);

                            if (isUnAcknowledge)
                            {
                                saveBookingItemAcknowledgeList = await _service.GetAllBookingItemAcknowledgeByBookingIDAndWithOutOB(selectedbookingID == "" ? "0" : selectedbookingID);
                                // OFF FOR CORE //IsSendMail = await SystemMailForSampleUnAck(saveBookingItemAcknowledgeList, bmList, IsSendMail, SaveType, false, listTypeMasterGrid);
                            }
                            else
                            {
                                // OFF FOR CORE //IsSendMail = await SystemMailForSample(savedFBA.FabricBookingAcknowledgeList, bmList, IsSendMail, SaveType, hasLiabilities, 0, listTypeMasterGrid);
                            }
                        }
                    }

                    SendMailTrueOrFalse = IsSendMail;
                }
            }

            return Ok(SendMailTrueOrFalse);
        }
        /* //OFF FOR CORE
public async Task<bool> SystemMail(List<FabricBookingAcknowledge> saveFabricBookingItemAcknowledgeList, List<BookingMaster> bmList, Boolean IsSendMail, String SaveType, bool HasLiabilities, int unAckBy, string listTypeMasterGrid)
{
    if (_isValidMailSending)
    {
        try
        {
            int revisionNo = 0;
            var bookings = await _service.GetBookingByBookingNo(bmList.First().BookingNo);
            if (bookings.IsNotNull() && bookings.Count() > 0)
            {
                revisionNo = bookings.Max(x => x.RevisionNo);
            }

            String BuyerName = "", BuyerTeam = "", Salutation = "Dear Sir,";
            EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
            ItemSubGroupMailSetupDTO isgDTO = new ItemSubGroupMailSetupDTO();
            EmployeeMailSetupDTO emsDTO = new EmployeeMailSetupDTO();
            if (SaveType != "UA")
            {
                isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Fabric", "FBKACK"); 
            }
            else
            {
                isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Fabric", "FBK-UNACK");
            }
            //var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "BDS Ack");
            int addedBy = 0;
            int updatedBy = 0;
            int userCode = AppUser.UserCode;
            if (saveFabricBookingItemAcknowledgeList.Count > 0)
            {
                addedBy = saveFabricBookingItemAcknowledgeList.First().AddedBy;
                if (saveFabricBookingItemAcknowledgeList.First().UpdatedBy.IsNotNull())
                {
                    updatedBy = (int)saveFabricBookingItemAcknowledgeList.First().UpdatedBy;
                }
            }
            var uInfo = await _emailService.GetUserEmailInfoAsync(updatedBy > 0 ? updatedBy : addedBy); 
            var rInfo = await _emailService.GetUserEmailInfoAsync(updatedBy > 0 ? updatedBy : addedBy); 
            emsDTO = await _emailService.GetEmployeeMailSetupAsync(updatedBy > 0 ? updatedBy : addedBy, "FBKACK,FBK-UNACK");

            var ReceiverDetails = await _emailService.GetUserEmailInfoAsync(bmList[0].UpdatedBy == 0 ? bmList[0].AddedBy : bmList[0].UpdatedBy);
            var ReceiverDetails1 = await _emailService.GetUserEmailInfoAsync(unAckBy); 

            //String EditType = bmList[0].RevisionNo > 0 ? "Revise " : "";
            String EditType = revisionNo > 0 ? "Revise " : "";

            String BKRevision = String.Empty;
            //BKRevision = bmList[0].RevisionNo > 0 ? " Rev-" + bmList[0].RevisionNo.ToString() : "";
            BKRevision = revisionNo > 0 ? " Rev-" + revisionNo.ToString() : "";
            BuyerName = bmList[0].BuyerName;
            BuyerTeam = bmList[0].BuyerTeamName == bmList[0].BuyerName ? "" : " " + bmList[0].BuyerTeamName;

            var attachment = new byte[] { 0 };
            String fromMailID = "";
            String toMailID = "";
            String ccMailID = "";
            String bccMailID = "";
            String password = "";
            String filePath = "";
            if (SaveType != "UA")
            {
                #region Get Report Attachment 

                filePath = String.Format(@"{0} {1} {2}.PDF", EditType, bmList[0].BookingNo, BKRevision);
                Dictionary<String, String> paraList = new Dictionary<String, String>();
                paraList.Add("BookingNo", bmList[0].BookingNo);

                //attachment = await _reportingService.GetPdfByte(1663, AppUser.UserCode, paraList);  //For local & Live // OFF FOR CORE
                //attachment = await _reportingService.GetPdfByte(1328, UserId, paraList);  //Old mail

                #endregion
            }

            if (uInfo.IsNotNull() && rInfo.IsNotNull())
            {
                List<FabricBookingAcknowledge> fbaList = await _service.GetAllBuyerTeamHeadByBOMMasterID(saveFabricBookingItemAcknowledgeList[0].BOMMasterID.ToString());
                String TeamHeadEmail = fbaList.Count > 0 ? fbaList[0].EmailID : "";

                List<FabricBookingAcknowledge> EmployeeMailSetupList = await _service.GetAllEmployeeMailSetupByUserCodeAndSetupForName(bmList[0].UpdatedBy == 0 ? bmList[0].AddedBy.ToString() : bmList[0].UpdatedBy.ToString(), "FBKACK");

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

                if (HttpContext.Request.Host.Host.ToUpper()== "texerp.epylliongroup.com".ToUpper())//--
                {
                    if (EmployeeMailSetupList.Count > 0)
                    {
                        toMailID += EmployeeMailSetupList[0].ToMailID != "" ? EmployeeMailSetupList[0].ToMailID + ";" : "";
                        ccMailID += EmployeeMailSetupList[0].CCMailID != "" ? EmployeeMailSetupList[0].CCMailID + ";" : "";
                        bccMailID += EmployeeMailSetupList[0].BCCMailID != "" ? EmployeeMailSetupList[0].BCCMailID + ";" : "";
                    }

                    fromMailID = uInfo.Email;
                    password = objEncription.Decrypt(uInfo.EmailPassword, uInfo.UserName);
                    //password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);//alamin
                    toMailID = ReceiverDetails.Email.IsNullOrEmpty() ? "" : ReceiverDetails.Email;//alamin
                    if (ReceiverDetails1.IsNotNull())
                    {
                        toMailID += ReceiverDetails1.Email.IsNotNullOrEmpty() ? ";" + ReceiverDetails1.Email : "";
                    }

                    if (TeamHeadEmail.IsNotNullOrEmpty())
                        toMailID += ";" + TeamHeadEmail;
                    if (ccMailID.IsNullOrEmpty())
                        ccMailID = AppUser.Email;
                    else
                    {
                        ccMailID = ccMailID + ";";
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
                if (HasLiabilities && SaveType != "UA")
                {
                    String selectedbookingID = String.Empty;
                    var strArr = saveFabricBookingItemAcknowledgeList.Select(i => i.BookingID.ToString()).Distinct().ToArray();
                    selectedbookingID += string.Join(",", strArr.ToArray());
                    List<FBookingAcknowledgementLiabilityDistribution> curLiabList = await _service.GetAllFBookingAckLiabilityByIDAsync(selectedbookingID == "" ? "0" : selectedbookingID);
                    List<FBookingAcknowledgementYarnLiability> curYLiabList = await _service.GetAllFBookingAckYarnLiabilityByIDAsync(selectedbookingID == "" ? "0" : selectedbookingID);

                    String colData = "";
                    foreach (FBookingAcknowledgementLiabilityDistribution item in curLiabList)
                    {
                        string cellData = $@"<td>{item.SubGroupName}</td><td>{item.LiabilitiesName}</td><td>{item.LiabilityQty}</td><td>{item.TotalValue}</td><td>{item.UOM}</td>";
                        colData += $@"<tr>{cellData}</tr>";
                    }
                    String tblLD = $@"<table border='1' style='color:black;font-size:10pt;font-family:Tahoma;'><tr style='background-color:#dde9ed'><th>Item</th><th>Liabilities Name</th><th>Liability Qty</th><th>Value</th><th>UOM</th></tr>{colData}</table>";

                    String colYarnData = "";
                    foreach (FBookingAcknowledgementYarnLiability item in curYLiabList)
                    {
                        string cellYarnData = $@"<td>{item._segment1ValueDesc}</td><td>{item._segment2ValueDesc}</td><td>{item._segment3ValueDesc}</td><td>{item._segment4ValueDesc}</td><td>{item._segment5ValueDesc}</td><td>{item._segment6ValueDesc}</td><td>{item.ShadeCode}</td><td>{item.DisplayUnitDesc}</td><td>{item.LiabilityQty}</td><td>{item.TotalValue}</td>";
                        colYarnData += $@"<tr>{cellYarnData}</tr>";
                    }
                    String tblYarnLD = $@"<table border='1' style='color:black;font-size:10pt;font-family:Tahoma;'><tr style='background-color:#dde9ed'><th>Composition</th><th>Yarn Type</th><th>Manufacturing Process</th><th>Sub Process</th><th>Quality Parameter</th><th>Count</th><th>Shade Code</th><th>UOM</th><th>Liability Qty</th><th>Value</th></tr>{colYarnData}</table>";
                    if (colYarnData == "")
                    {
                        tblYarnLD = "";
                    }
                    else
                    {
                        tblYarnLD = $@"<BR><BR>Yarn Liabilities Here-<BR><BR>{tblYarnLD}";
                    }

                    MailSubject = String.Format(@"{0}Fabric Booking [{1}{2}] Liabilities for Garments Buyer {3},{4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

                    MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                          <BR><BR>{1}Booking Number <b>{2}{3}</b> garments buyer: <b>{4}</b>, <b>{5}</b> has been sent to you for liability acceptance. 
                           <BR><BR>Please accept it or stay as previous booking.
                          <BR><BR>Liabilities Here-
                          <BR><BR>
                           {10}
                           {11}
                          <BR><BR>Any query please feel free to contact me.
                          <BR><BR><BR>Thanks &amp; Best Regards,
                          <BR><BR>{6}{7}{8}{9}
                          <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                              uInfo.Name,
                              Designation, Department, ExtensionNo, tblLD, tblYarnLD);
                }
                else
                {
                    if (SaveType != "UA")
                    {
                        MailSubject = String.Format(@"{0}Fabric Booking [{1}{2}] Acknowledged for Garments Buyer {3} {4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

                        MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                          <BR><BR>{1}Booking Number <b>{2}{3}</b> has been acknowledged by textile PMC for garments buyer <b>{4}</b> <b>{5}</b>.
                          <BR><BR>Any query please feel free to contact me.
                          <BR><BR><BR>Thanks &amp; Best Regards,
                          <BR><BR>{6}{7}{8}{9}
                          <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                                  uInfo.Name,
                                  Designation, Department, ExtensionNo);
                    }
                    else
                    {
                        if (bmList[0].UnAcknowledgeReason.Length > 1)
                        {
                            UnAcknowledgeReason = String.Format(@"<BR>UnAcknowledge Reason: {0}", bmList[0].UnAcknowledgeReason);
                        }

                        MailSubject = String.Format(@"Fabric Booking [{1}{2}] UnAcknowledged for Garments Buyer {3} {4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

                        MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                          <BR><BR>Booking Number <b>{2}{3}</b> has been unacknowledged by textile PMC for garments buyer <b>{4}</b> <b>{5}</b>.{10}
                          <BR><BR>Any query please feel free to contact me.
                          <BR><BR><BR>Thanks &amp; Best Regards,
                          <BR><BR>{6}{7}{8}{9}
                          <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                                  uInfo.Name,
                                  Designation, Department, ExtensionNo, UnAcknowledgeReason);
                    }
                }

                //MailSubject = "Test Mail Please Ignore - " + MailSubject;
                await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, MailSubject, MailBody, filePath, attachment);
            }

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    return IsSendMail;
}

public async Task<bool> SystemMailUnAck(List<BookingItemAcknowledge> saveFabricBookingItemAcknowledgeList, List<BookingMaster> bmList, Boolean IsSendMail, String SaveType, bool HasLiabilities, string listTypeMasterGrid)
{
if (_isValidMailSending)
{
try
{
    int revisionNo = 0;
    var bookings = await _service.GetBookingByBookingNo(bmList.First().BookingNo);
    if (bookings.IsNotNull() && bookings.Count() > 0)
    {
        revisionNo = bookings.Max(x => x.RevisionNo);
        if (listTypeMasterGrid == "RAL")
        {
            revisionNo = revisionNo + 1;
        }
    }

    String BuyerName = "", BuyerTeam = "", Salutation = "Dear Sir,";
    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
    ItemSubGroupMailSetupDTO isgDTO = new ItemSubGroupMailSetupDTO();
    EmployeeMailSetupDTO emsDTO = new EmployeeMailSetupDTO();
    if (SaveType != "UA")
    {
        isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Fabric", "FBKACK"); 
    }
    else
    {
        isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Fabric", "FBK-UNACK"); 
    }
    //var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "BDS Ack");

    int UnAcknowledgeBy = 0;
    int updatedBy = 0;
    int userCode = AppUser.UserCode;
    if (saveFabricBookingItemAcknowledgeList.Count > 0)
    {
        UnAcknowledgeBy = saveFabricBookingItemAcknowledgeList.First().UnAcknowledgeBy;

    }
    var uInfo = await _emailService.GetUserEmailInfoAsync(UnAcknowledgeBy); 
    var rInfo = await _emailService.GetUserEmailInfoAsync(UnAcknowledgeBy); 
    emsDTO = await _emailService.GetEmployeeMailSetupAsync(UnAcknowledgeBy, "FBKACK,FBK-UNACK"); 
    var ReceiverDetails = await _emailService.GetUserEmailInfoAsync(bmList[0].UpdatedBy == 0 ? bmList[0].AddedBy : bmList[0].UpdatedBy); 

    //String EditType = bmList[0].RevisionNo > 0 ? "Revise " : "";
    String EditType = revisionNo > 0 ? "Revise " : "";

    String BKRevision = String.Empty;
    //BKRevision = bmList[0].RevisionNo > 0 ? " Rev-" + bmList[0].RevisionNo.ToString() : "";

    BKRevision = revisionNo > 0 ? " Rev-" + revisionNo.ToString() : "";
    BuyerName = bmList[0].BuyerName;
    BuyerTeam = bmList[0].BuyerTeamName == bmList[0].BuyerName ? "" : " " + bmList[0].BuyerTeamName;


    var attachment = new byte[] { 0 };
    String fromMailID = "";
    String toMailID = "";
    String ccMailID = "";
    String bccMailID = "";
    String password = "";
    String filePath = "";
    if (SaveType != "UA")
    {
        #region Get Report Attachment 

        filePath = String.Format(@"{0} {1} {2}.PDF", EditType, bmList[0].BookingNo, BKRevision);
        Dictionary<String, String> paraList = new Dictionary<String, String>();
        paraList.Add("BookingNo", bmList[0].BookingNo);
        //attachment = await _reportingService.GetPdfByte(1663, AppUser.UserCode, paraList);  //For local & Live // OFF For CORE
                                                                                  //attachment = await _reportingService.GetPdfByte(1328, UserId, paraList);  //Old mail

        #endregion
    }

    if (uInfo.IsNotNull() && rInfo.IsNotNull())
    {
        List<FabricBookingAcknowledge> fbaList = await _service.GetAllBuyerTeamHeadByBOMMasterID(saveFabricBookingItemAcknowledgeList[0].BOMMasterID.ToString());
        String TeamHeadEmail = fbaList.Count > 0 ? fbaList[0].EmailID : "";

        List<FabricBookingAcknowledge> EmployeeMailSetupList = await _service.GetAllEmployeeMailSetupByUserCodeAndSetupForName(bmList[0].UpdatedBy == 0 ? bmList[0].AddedBy.ToString() : bmList[0].UpdatedBy.ToString(), "FBKACK");


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
        if (HttpContext.Request.Host.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
        {
            if (EmployeeMailSetupList.Count > 0)
            {
                toMailID += EmployeeMailSetupList[0].ToMailID != "" ? EmployeeMailSetupList[0].ToMailID + ";" : "";
                ccMailID += EmployeeMailSetupList[0].CCMailID != "" ? EmployeeMailSetupList[0].CCMailID + ";" : "";
                bccMailID += EmployeeMailSetupList[0].BCCMailID != "" ? EmployeeMailSetupList[0].BCCMailID + ";" : "";
            }
            fromMailID = uInfo.Email;
            password = objEncription.Decrypt(uInfo.EmailPassword, uInfo.UserName);
            //password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
            //toMailID = rInfo.Email.IsNullOrEmpty() ? AppUser.Email : rInfo.Email; //alamin
            toMailID = ReceiverDetails.Email.IsNullOrEmpty() ? "" : ReceiverDetails.Email; //alamin

            if (TeamHeadEmail.IsNotNullOrEmpty())
                toMailID += ";" + TeamHeadEmail;
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
        if (HasLiabilities && SaveType != "UA")
        {
            String selectedbookingID = String.Empty;
            var strArr = saveFabricBookingItemAcknowledgeList.Select(i => i.BookingID.ToString()).Distinct().ToArray();
            selectedbookingID += string.Join(",", strArr.ToArray());
            List<FBookingAcknowledgementLiabilityDistribution> curLiabList = await _service.GetAllFBookingAckLiabilityByIDAsync(selectedbookingID == "" ? "0" : selectedbookingID);
            List<FBookingAcknowledgementYarnLiability> curYLiabList = await _service.GetAllFBookingAckYarnLiabilityByIDAsync(selectedbookingID == "" ? "0" : selectedbookingID);
            String colData = "";
            foreach (FBookingAcknowledgementLiabilityDistribution item in curLiabList)
            {
                string cellData = $@"<td>{item.SubGroupName}</td><td>{item.LiabilitiesName}</td><td>{item.LiabilityQty}</td><td>{item.TotalValue}</td><td>{item.UOM}</td>";
                colData += $@"<tr>{cellData}</tr>";
            }
            String tblLD = $@"<table border='1' style='color:black;font-size:10pt;font-family:Tahoma;'><tr style='background-color:#dde9ed'><th>Item</th><th>Liabilities Name</th><th>Liability Qty</th><th>Value</th><th>UOM</th></tr>{colData}</table>";

            String colYarnData = "";
            foreach (FBookingAcknowledgementYarnLiability item in curYLiabList)
            {
                string cellYarnData = $@"<td>{item._segment1ValueDesc}</td><td>{item._segment2ValueDesc}</td><td>{item._segment3ValueDesc}</td><td>{item._segment4ValueDesc}</td><td>{item._segment5ValueDesc}</td><td>{item._segment6ValueDesc}</td><td>{item.ShadeCode}</td><td>{item.DisplayUnitDesc}</td><td>{item.LiabilityQty}</td><td>{item.TotalValue}</td>";
                colYarnData += $@"<tr>{cellYarnData}</tr>";
            }
            String tblYarnLD = $@"<table border='1' style='color:black;font-size:10pt;font-family:Tahoma;'><tr style='background-color:#dde9ed'><th>Composition</th><th>Yarn Type</th><th>Manufacturing Process</th><th>Sub Process</th><th>Quality Parameter</th><th>Count</th><th>Shade Code</th><th>UOM</th><th>Liability Qty</th><th>Value</th></tr>{colYarnData}</table>";
            if (colYarnData == "")
            {
                tblYarnLD = "";
            }
            else
            {
                tblYarnLD = $@"<BR><BR>Yarn Liabilities Here-<BR><BR>{tblYarnLD}";
            }

            MailSubject = String.Format(@"{0}Fabric Booking [{1}{2}] Liabilities for Garments Buyer {3},{4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

            MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                  <BR><BR>{1}Booking Number <b>{2}{3}</b> garments buyer: <b>{4}<b>, <b>{5}<b> has been sent to you for liability acceptance. 
                       Please accept it or stay as previous booking.
                  <BR><BR>Liabilities Here-
                  <BR><BR>
                   {10}
                   {11}
                  <BR><BR>Any query please feel free to contact me.
                  <BR><BR><BR>Thanks &amp; Best Regards,
                  <BR><BR>{6}{7}{8}{9}
                  <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                      uInfo.Name,
                      Designation, Department, ExtensionNo, tblLD, tblYarnLD);
        }
        else
        {
            if (SaveType != "UA")
            {
                MailSubject = String.Format(@"{0}Fabric Booking [{1}{2}] Acknowledged for Garments Buyer {3} {4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

                MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                  <BR><BR>{1}Booking Number <b>{2}{3}</b> has been acknowledged by textile PMC for garments buyer <b>{4}</b> <b>{5}</b>.
                  <BR><BR>Any query please feel free to contact me.
                  <BR><BR><BR>Thanks &amp; Best Regards,
                  <BR><BR>{6}{7}{8}{9}
                  <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                          uInfo.Name,
                          Designation, Department, ExtensionNo);
            }
            else
            {
                if (bmList[0].UnAcknowledgeReason.Length > 1)
                {
                    UnAcknowledgeReason = String.Format(@"<BR>UnAcknowledge Reason: {0}", bmList[0].UnAcknowledgeReason);
                }

                MailSubject = String.Format(@"Fabric Booking [{1}{2}] UnAcknowledged for Garments Buyer {3} {4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

                MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                  <BR><BR>Booking Number <b>{2}{3}</b> has been unacknowledged by textile PMC for garments buyer <b>{4}</b> <b>{5}</b>.{10}
                  <BR><BR>Any query please feel free to contact me.
                  <BR><BR><BR>Thanks &amp; Best Regards,
                  <BR><BR>{6}{7}{8}{9}
                  <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                          uInfo.Name,
                          Designation, Department, ExtensionNo, UnAcknowledgeReason);
            }
        }
        //MailSubject = "Test Mail Please Ignore - " + MailSubject;
        await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, MailSubject, MailBody, filePath, attachment);
    }

    return true;
}
catch (Exception ex)
{
    return false;
}
}
return IsSendMail;
}

public async Task<bool> SystemMailForSample(List<FabricBookingAcknowledge> saveFabricBookingItemAcknowledgeList, List<SampleBookingMaster> bmList, Boolean IsSendMail, String SaveType, bool HasLiabilities, int unAckBy, string listTypeMasterGrid)
{
if (_isValidMailSending)
{
try
{
    int revisionNo = 0;
    var bookings = await _service.GetBookingByBookingNo(bmList.First().BookingNo);
    if (bookings.IsNotNull() && bookings.Count() > 0)
    {
        revisionNo = bookings.Max(x => x.RevisionNo);
    }


    String BuyerName = "", BuyerTeam = "", Salutation = "Dear Sir,";
    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
    ItemSubGroupMailSetupDTO isgDTO = new ItemSubGroupMailSetupDTO();
    EmployeeMailSetupDTO emsDTO = new EmployeeMailSetupDTO();
    if (SaveType != "UA")
    {
        isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Fabric", "FBKACK");
    }
    else
    {
        isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Fabric", "FBK-UNACK");
    }
    //var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "BDS Ack");
    int addedBy = 0;
    int updatedBy = 0;
    int userCode = AppUser.UserCode;
    if (saveFabricBookingItemAcknowledgeList.Count > 0)
    {
        addedBy = saveFabricBookingItemAcknowledgeList.First().AddedBy;
        if (saveFabricBookingItemAcknowledgeList.First().UpdatedBy.IsNotNull())
        {
            updatedBy = (int)saveFabricBookingItemAcknowledgeList.First().UpdatedBy;
        }
    }
    var uInfo = await _emailService.GetUserEmailInfoAsync(updatedBy > 0 ? updatedBy : addedBy);
    var rInfo = await _emailService.GetUserEmailInfoAsync(updatedBy > 0 ? updatedBy : addedBy);
    emsDTO = await _emailService.GetEmployeeMailSetupAsync(updatedBy > 0 ? updatedBy : addedBy, "FBKACK,FBK-UNACK");
    var ReceiverDetails = await _emailService.GetUserEmailInfoAsync(bmList[0].UpdatedBy == 0 ? bmList[0].AddedBy : bmList[0].UpdatedBy);
    var ReceiverDetails1 = await _emailService.GetUserEmailInfoAsync(unAckBy);

    //var uInfo = await _emailService.GetUserEmailInfoAsync(saveFabricBookingItemAcknowledgeList.Count > 0 ? saveFabricBookingItemAcknowledgeList[0].UpdatedBy.IsNotNull() ? saveFabricBookingItemAcknowledgeList[0].UpdatedBy.ToString().ToInt() : saveFabricBookingItemAcknowledgeList[0].AddedBy : AppUser.UserCode);
    //var rInfo = await _emailService.GetUserEmailInfoAsync(bmList[0].UpdatedBy > 0 ? bmList[0].UpdatedBy : bmList[0].AddedBy);
    //emsDTO = await _emailService.GetEmployeeMailSetupAsync(bmList[0].UpdatedBy > 0 ? bmList[0].UpdatedBy : bmList[0].AddedBy, "FBKACK,FBK-UNACK");

    //String EditType = bmList[0].RevisionNo > 0 ? "Revise " : "";
    String EditType = revisionNo > 0 ? "Revise " : "";

    String BKRevision = String.Empty;
    //BKRevision = bmList[0].RevisionNo > 0 ? " Rev-" + bmList[0].RevisionNo.ToString() : "";
    BKRevision = revisionNo > 0 ? " Rev-" + revisionNo.ToString() : "";

    BuyerName = bmList[0].BuyerName;
    BuyerTeam = bmList[0].BuyerTeamName == bmList[0].BuyerName ? "" : " " + bmList[0].BuyerTeamName;

    var attachment = new byte[] { 0 };
    String fromMailID = "";
    String toMailID = "";
    String ccMailID = "";
    String bccMailID = "";
    String password = "";
    String filePath = "";
    if (SaveType != "UA")
    {
        #region Get Report Attachment 

        filePath = String.Format(@"{0} {1} {2}.PDF", EditType, bmList[0].BookingNo, BKRevision);
        Dictionary<String, String> paraList = new Dictionary<String, String>();
        paraList.Add("BookingNo", bmList[0].BookingNo);
        //attachment = await _reportingService.GetPdfByte(1663, UserId, paraList);  //For local & Live// OFF For CORE
        //attachment = await _reportingService.GetPdfByte(406, UserId, paraList);  //Old mail

        #endregion
    }
    if (uInfo.IsNotNull() && rInfo.IsNotNull())
    {
        List<FabricBookingAcknowledge> EmployeeMailSetupList = await _service.GetAllEmployeeMailSetupByUserCodeAndSetupForName(bmList[0].UpdatedBy == 0 ? bmList[0].AddedBy.ToString() : bmList[0].UpdatedBy.ToString(), "FBKACK");

        List<FabricBookingAcknowledge> fbaList = await _service.GetAllBuyerTeamHeadByBOMMasterID(saveFabricBookingItemAcknowledgeList[0].BOMMasterID.ToString());
        String TeamHeadEmail = fbaList.Count > 0 ? fbaList[0].EmailID : "";

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
        if (HttpContext.Request.Host.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
        {
            if (EmployeeMailSetupList.Count > 0)
            {
                toMailID += EmployeeMailSetupList[0].ToMailID != "" ? EmployeeMailSetupList[0].ToMailID + ";" : "";
                ccMailID += EmployeeMailSetupList[0].CCMailID != "" ? EmployeeMailSetupList[0].CCMailID + ";" : "";
                bccMailID += EmployeeMailSetupList[0].BCCMailID != "" ? EmployeeMailSetupList[0].BCCMailID + ";" : "";
            }
            fromMailID = uInfo.Email;
            password = objEncription.Decrypt(uInfo.EmailPassword, uInfo.UserName);
            //password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
            //toMailID = rInfo.Email.IsNullOrEmpty() ? AppUser.Email : rInfo.Email; //alamin
            toMailID = ReceiverDetails.Email.IsNullOrEmpty() ? "" : ReceiverDetails.Email; //alamin
            if (ReceiverDetails1.IsNotNull())
            {
                toMailID += ReceiverDetails1.Email.IsNotNullOrEmpty() ? ";" + ReceiverDetails1.Email : "";//alamin
            }

            if (TeamHeadEmail.IsNotNullOrEmpty())
                toMailID += ";" + TeamHeadEmail;
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
        if (HasLiabilities && SaveType != "UA")
        {
            String selectedbookingID = String.Empty;
            var strArr = saveFabricBookingItemAcknowledgeList.Select(i => i.BookingID.ToString()).Distinct().ToArray();
            selectedbookingID += string.Join(",", strArr.ToArray());
            List<FBookingAcknowledgementLiabilityDistribution> curLiabList = await _service.GetAllFBookingAckLiabilityByIDAsync(selectedbookingID == "" ? "0" : selectedbookingID);
            List<FBookingAcknowledgementYarnLiability> curYLiabList = await _service.GetAllFBookingAckYarnLiabilityByIDAsync(selectedbookingID == "" ? "0" : selectedbookingID);
            String colData = "";
            foreach (FBookingAcknowledgementLiabilityDistribution item in curLiabList)
            {
                string cellData = $@"<td>{item.SubGroupName}</td><td>{item.LiabilitiesName}</td><td>{item.LiabilityQty}</td><td>{item.UOM}</td>";
                colData += $@"<tr>{cellData}</tr>";
            }
            String tblLD = $@"<table border='1' style='color:black;font-size:10pt;font-family:Tahoma;'><tr style='background-color:#dde9ed'><th>Item</th><th>Liabilities Name</th><th>Liability Qty</th><th>UOM</th></tr>{colData}</table>";

            String colYarnData = "";
            foreach (FBookingAcknowledgementYarnLiability item in curYLiabList)
            {
                string cellYarnData = $@"<td>{item._segment1ValueDesc}</td><td>{item._segment2ValueDesc}</td><td>{item._segment3ValueDesc}</td><td>{item._segment4ValueDesc}</td><td>{item._segment5ValueDesc}</td><td>{item._segment6ValueDesc}</td><td>{item.ShadeCode}</td><td>{item.DisplayUnitDesc}</td><td>{item.LiabilityQty}</td>";
                colYarnData += $@"<tr>{cellYarnData}</tr>";
            }
            String tblYarnLD = $@"<table border='1' style='color:black;font-size:10pt;font-family:Tahoma;'><tr style='background-color:#dde9ed'><th>Composition</th><th>Yarn Type</th><th>Manufacturing Process</th><th>Sub Process</th><th>Quality Parameter</th><th>Count</th><th>Shade Code</th><th>UOM</th><th>Liability Qty</th></tr>{colYarnData}</table>";
            if (colYarnData == "")
            {
                tblYarnLD = "";
            }
            else
            {
                tblYarnLD = $@"<BR><BR>Yarn Liabilities Here-<BR><BR>{tblYarnLD}";
            }

            MailSubject = String.Format(@"{0}Sample Fabric Booking [{1}{2}] Liabilities for Garments Buyer {3} {4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

            MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                      <BR><BR>{1}Sample Booking Number <b>{2}{3}</b> garments buyer: <b>{4}<b>, <b>{5}<b> has been sent to you for liability acceptance. 
                       Please accept it or stay as previous booking.
                      <BR><BR>Liabilities Here-
                      <BR><BR>
                       {10}
                       {11}
                      <BR><BR>Any query please feel free to contact me.
                      <BR><BR><BR>Thanks &amp; Best Regards,
                      <BR><BR>{6}{7}{8}{9}
                      <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                      uInfo.Name,
                      Designation, Department, ExtensionNo, tblLD, tblYarnLD);
        }
        else
        {
            if (SaveType != "UA")
            {
                MailSubject = String.Format(@"{0}Sample Fabric Booking [{1}{2}] Acknowledged for Garments Buyer {3} {4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

                MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                      <BR><BR>{1}Sample Booking Number <b>{2}{3}</b> has been acknowledged by textile PMC for garments buyer <b>{4}</b> <b>{5}</b>.
                      <BR><BR>Any query please feel free to contact me.
                      <BR><BR><BR>Thanks &amp; Best Regards,
                      <BR><BR>{6}{7}{8}{9}
                      <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                          uInfo.Name,
                          Designation, Department, ExtensionNo);
            }
            else
            {
                if (bmList[0].UnAcknowledgeReason.Length > 1)
                {
                    UnAcknowledgeReason = String.Format(@"<BR>UnAcknowledge Reason: {0}", bmList[0].UnAcknowledgeReason);
                }

                MailSubject = String.Format(@"{0}Sample Fabric Booking [{1}{2}] UnAcknowledged for Garments Buyer {3} {4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

                MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                      <BR><BR>{1}Sample Booking Number <b>{2}{3}</b> has been unacknowledged by textile PMC for garments buyer <b>{4}</b> <b>{5}</b>.{10}
                      <BR><BR>Any query please feel free to contact me.
                      <BR><BR><BR>Thanks &amp; Best Regards,
                      <BR><BR>{6}{7}{8}{9}
                      <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                          uInfo.Name,
                          Designation, Department, ExtensionNo, UnAcknowledgeReason);
            }
        }
        //MailSubject = "Test Mail Please Ignore - " + MailSubject;

        toMailID = this.ReplaceMailInvalidChar(toMailID);
        ccMailID = this.ReplaceMailInvalidChar(ccMailID);
        bccMailID = this.ReplaceMailInvalidChar(bccMailID);

        await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, MailSubject, MailBody, filePath, attachment);
    }

    return true;
}
catch (Exception ex)
{
    return false;
}
}
return IsSendMail;
}

public async Task<bool> SystemMailForSampleUnAck(List<BookingItemAcknowledge> saveFabricBookingItemAcknowledgeList, List<SampleBookingMaster> bmList, Boolean IsSendMail, String SaveType, bool HasLiabilities, string listTypeMasterGrid)
{
if (_isValidMailSending)
{
try
{
    int revisionNo = 0;
    var bookings = await _service.GetBookingByBookingNo(bmList.First().BookingNo);
    if (bookings.IsNotNull() && bookings.Count() > 0)
    {
        revisionNo = bookings.Max(x => x.RevisionNo);
        if (listTypeMasterGrid == "RAL")
        {
            revisionNo = revisionNo + 1;
        }
    }

    String BuyerName = "", BuyerTeam = "", Salutation = "Dear Sir,";
    EPYSL.Encription.Encryption objEncription = new EPYSL.Encription.Encryption();
    ItemSubGroupMailSetupDTO isgDTO = new ItemSubGroupMailSetupDTO();
    EmployeeMailSetupDTO emsDTO = new EmployeeMailSetupDTO();
    if (SaveType != "UA")
    {
        isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Fabric", "FBKACK");
    }
    else
    {
        isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Fabric", "FBK-UNACK");
    }
    //var isgDTO = await _emailService.GetItemSubGroupMailSetupAsync("Yarn New", "BDS Ack");
    int UnAcknowledgeBy = 0;
    int updatedBy = 0;
    int userCode = AppUser.UserCode;
    if (saveFabricBookingItemAcknowledgeList.Count > 0)
    {
        UnAcknowledgeBy = saveFabricBookingItemAcknowledgeList.First().UnAcknowledgeBy;
        //addedBy = saveFabricBookingItemAcknowledgeList.First().AddedBy;
        //if (saveFabricBookingItemAcknowledgeList.First().UpdatedBy.IsNotNull())
        //{
        //    updatedBy = (int)saveFabricBookingItemAcknowledgeList.First().UpdatedBy;
        //}
    }
    var uInfo = await _emailService.GetUserEmailInfoAsync(UnAcknowledgeBy);
    var rInfo = await _emailService.GetUserEmailInfoAsync(UnAcknowledgeBy);
    emsDTO = await _emailService.GetEmployeeMailSetupAsync(UnAcknowledgeBy, "FBKACK,FBK-UNACK");
    var ReceiverDetails = await _emailService.GetUserEmailInfoAsync(bmList[0].UpdatedBy == 0 ? bmList[0].AddedBy : bmList[0].UpdatedBy);

    //var uInfo = await _emailService.GetUserEmailInfoAsync(saveFabricBookingItemAcknowledgeList.Count > 0 ? saveFabricBookingItemAcknowledgeList[0].UpdatedBy.IsNotNull() ? saveFabricBookingItemAcknowledgeList[0].UpdatedBy.ToString().ToInt() : saveFabricBookingItemAcknowledgeList[0].AddedBy : AppUser.UserCode);
    //var rInfo = await _emailService.GetUserEmailInfoAsync(bmList[0].UpdatedBy > 0 ? bmList[0].UpdatedBy : bmList[0].AddedBy);
    //emsDTO = await _emailService.GetEmployeeMailSetupAsync(bmList[0].UpdatedBy > 0 ? bmList[0].UpdatedBy : bmList[0].AddedBy, "FBKACK,FBK-UNACK");

    //String EditType = bmList[0].RevisionNo > 0 ? "Revise " : "";
    String EditType = revisionNo > 0 ? "Revise " : "";

    String BKRevision = String.Empty;
    //BKRevision = bmList[0].RevisionNo > 0 ? " Rev-" + bmList[0].RevisionNo.ToString() : "";
    BKRevision = revisionNo > 0 ? " Rev-" + revisionNo.ToString() : "";

    BuyerName = bmList[0].BuyerName;
    BuyerTeam = bmList[0].BuyerTeamName == bmList[0].BuyerName ? "" : " " + bmList[0].BuyerTeamName;

    var attachment = new byte[] { 0 };
    String fromMailID = "";
    String toMailID = "";
    String ccMailID = "";
    String bccMailID = "";
    String password = "";
    String filePath = "";
    if (SaveType != "UA")
    {
        #region Get Report Attachment 

        filePath = String.Format(@"{0} {1} {2}.PDF", EditType, bmList[0].BookingNo, BKRevision);
        Dictionary<String, String> paraList = new Dictionary<String, String>();
        paraList.Add("BookingNo", bmList[0].BookingNo);

        //attachment = await _reportingService.GetPdfByte(1663, UserId, paraList); //For local & Live // OFF For CORE
        //attachment = await _reportingService.GetPdfByte(406, UserId, paraList); //Old mail

        #endregion
    }
    if (uInfo.IsNotNull() && rInfo.IsNotNull())
    {
        List<FabricBookingAcknowledge> EmployeeMailSetupList = await _service.GetAllEmployeeMailSetupByUserCodeAndSetupForName(bmList[0].UpdatedBy == 0 ? bmList[0].AddedBy.ToString() : bmList[0].UpdatedBy.ToString(), "FBKACK");

        List<FabricBookingAcknowledge> fbaList = await _service.GetAllBuyerTeamHeadByBOMMasterID(saveFabricBookingItemAcknowledgeList[0].BOMMasterID.ToString());
        String TeamHeadEmail = fbaList.Count > 0 ? fbaList[0].EmailID : "";


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
        if (HttpContext.Request.Host.Host.ToUpper() == "texerp.epylliongroup.com".ToUpper())
        {
            if (EmployeeMailSetupList.Count > 0)
            {
                toMailID += EmployeeMailSetupList[0].ToMailID != "" ? EmployeeMailSetupList[0].ToMailID + ";" : "";
                ccMailID += EmployeeMailSetupList[0].CCMailID != "" ? EmployeeMailSetupList[0].CCMailID + ";" : "";
                bccMailID += EmployeeMailSetupList[0].BCCMailID != "" ? EmployeeMailSetupList[0].BCCMailID + ";" : "";
            }
            fromMailID = uInfo.Email;
            password = objEncription.Decrypt(uInfo.EmailPassword, uInfo.UserName);
            //password = objEncription.Decrypt(AppUser.EmailPassword, AppUser.UserName);
            //toMailID = rInfo.Email.IsNullOrEmpty() ? AppUser.Email : rInfo.Email; //alamin
            toMailID = ReceiverDetails.Email.IsNullOrEmpty() ? "" : ReceiverDetails.Email; //alamin

            if (TeamHeadEmail.IsNotNullOrEmpty())
                toMailID += ";" + TeamHeadEmail;
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
                bccMailID += isgDTO.CCMailID.IsNullOrEmpty() ? "" : ";" + isgDTO.CCMailID;
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
        if (HasLiabilities && SaveType != "UA")
        {
            String selectedbookingID = String.Empty;
            var strArr = saveFabricBookingItemAcknowledgeList.Select(i => i.BookingID.ToString()).Distinct().ToArray();
            selectedbookingID += string.Join(",", strArr.ToArray());
            List<FBookingAcknowledgementLiabilityDistribution> curLiabList = await _service.GetAllFBookingAckLiabilityByIDAsync(selectedbookingID == "" ? "0" : selectedbookingID);
            List<FBookingAcknowledgementYarnLiability> curYLiabList = await _service.GetAllFBookingAckYarnLiabilityByIDAsync(selectedbookingID == "" ? "0" : selectedbookingID);
            String colData = "";
            foreach (FBookingAcknowledgementLiabilityDistribution item in curLiabList)
            {
                string cellData = $@"<td>{item.SubGroupName}</td><td>{item.LiabilitiesName}</td><td>{item.LiabilityQty}</td><td>{item.UOM}</td>";
                colData += $@"<tr>{cellData}</tr>";
            }
            String tblLD = $@"<table border='1' style='color:black;font-size:10pt;font-family:Tahoma;'><tr style='background-color:#dde9ed'><th>Item</th><th>Liabilities Name</th><th>Liability Qty</th><th>UOM</th></tr>{colData}</table>";

            String colYarnData = "";
            foreach (FBookingAcknowledgementYarnLiability item in curYLiabList)
            {
                string cellYarnData = $@"<td>{item._segment1ValueDesc}</td><td>{item._segment2ValueDesc}</td><td>{item._segment3ValueDesc}</td><td>{item._segment4ValueDesc}</td><td>{item._segment5ValueDesc}</td><td>{item._segment6ValueDesc}</td><td>{item.ShadeCode}</td><td>{item.DisplayUnitDesc}</td><td>{item.LiabilityQty}</td>";
                colYarnData += $@"<tr>{cellYarnData}</tr>";
            }
            String tblYarnLD = $@"<table border='1' style='color:black;font-size:10pt;font-family:Tahoma;'><tr style='background-color:#dde9ed'><th>Composition</th><th>Yarn Type</th><th>Manufacturing Process</th><th>Sub Process</th><th>Quality Parameter</th><th>Count</th><th>Shade Code</th><th>UOM</th><th>Liability Qty</th></tr>{colYarnData}</table>";
            if (colYarnData == "")
            {
                tblYarnLD = "";
            }
            else
            {
                tblYarnLD = $@"<BR><BR>Yarn Liabilities Here-<BR><BR>{tblYarnLD}";
            }

            MailSubject = String.Format(@"{0}Sample Fabric Booking [{1}{2}] Liabilities for Garments Buyer {3} {4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

            MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                      <BR><BR>{1}Sample Booking Number <b>{2}{3}</b> garments buyer: <b>{4}<b>, <b>{5}<b> has been sent to you for liability acceptance. 
                       Please accept it or stay as previous booking..
                      <BR><BR>Liabilities Here-
                      <BR><BR>
                       {10}
                       {11}
                      <BR><BR>Any query please feel free to contact me.
                      <BR><BR><BR>Thanks &amp; Best Regards,
                      <BR><BR>{6}{7}{8}{9}
                      <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                      uInfo.Name,
                      Designation, Department, ExtensionNo, tblLD, tblYarnLD);
        }
        else
        {
            if (SaveType != "UA")
            {
                MailSubject = String.Format(@"{0}Sample Fabric Booking [{1}{2}] Acknowledged for Garments Buyer {3} {4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

                MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                      <BR><BR>{1}Sample Booking Number <b>{2}{3}</b> has been acknowledged by textile PMC for garments buyer <b>{4}</b> <b>{5}</b>.
                      <BR><BR>Any query please feel free to contact me.
                      <BR><BR><BR>Thanks &amp; Best Regards,
                      <BR><BR>{6}{7}{8}{9}
                      <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                          uInfo.Name,
                          Designation, Department, ExtensionNo);
            }
            else
            {
                if (bmList[0].UnAcknowledgeReason.Length > 1)
                {
                    UnAcknowledgeReason = String.Format(@"<BR>UnAcknowledge Reason: {0}", bmList[0].UnAcknowledgeReason);
                }

                MailSubject = String.Format(@"{0}Sample Fabric Booking [{1}{2}] UnAcknowledged for Garments Buyer {3} {4}", EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam);

                MailBody = String.Format(@"<span style='font-size:10pt;font-family:Tahoma;'>{0}
                      <BR><BR>{1}Sample Booking Number <b>{2}{3}</b> has been unacknowledged by textile PMC for garments buyer <b>{4}</b> <b>{5}</b>.{10}
                      <BR><BR>Any query please feel free to contact me.
                      <BR><BR><BR>Thanks &amp; Best Regards,
                      <BR><BR>{6}{7}{8}{9}
                      <BR><BR><BR>This is ERP generated mail</span>", Salutation, EditType, bmList[0].BookingNo, BKRevision, BuyerName, BuyerTeam,
                          uInfo.Name,
                          Designation, Department, ExtensionNo, UnAcknowledgeReason);
            }
        }
        //MailSubject = "Test Mail Please Ignore - " + MailSubject;
        await _emailService.SendAutoEmailAsync(fromMailID, password, toMailID, ccMailID, bccMailID, MailSubject, MailBody, filePath, attachment);
    }

    return true;
}
catch (Exception ex)
{
    return false;
}
}
return IsSendMail;
}
*/
        private string ReplaceMailInvalidChar(string pValue)
        {
            pValue = pValue.Replace(";;;", ";").Replace(";;", ";").Replace(" ", "");
            return pValue;
        }
    }
}
