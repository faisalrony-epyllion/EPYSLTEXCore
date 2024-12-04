using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;


namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.YarnBookingMaster_New)]
    public class YarnBookingMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YBookingID { get; set; }
        public string YBookingNo { get; set; }
        public int? PreProcessRevNo { get; set; }
        public int? RevisionNo { get; set; }
        public DateTime? YBookingDate { get; set; }
        public int BuyerID { get; set; }
        public int BuyerTeamID { get; set; }
        public int CompanyID { get; set; }
        public int ExportOrderID { get; set; }
        public int BookingID { get; set; }
        public DateTime YInHouseDate { get; set; }
        public DateTime? YRequiredDate { get; set; }
        public int ContactPerson { get; set; }
        public bool Propose { get; set; }
        public DateTime? ProposeDate { get; set; }
        public bool Acknowledge { get; set; }
        public int AcknowledgeBy { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public string Remarks { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateRevised { get; set; }
        public bool? HasYPrice { get; set; }
        public int? YPRevisionNo { get; set; }
        public bool? ApproveYP { get; set; }
        public bool UnApproveYP { get; set; }
        public int YPPreProcessRevNo { get; set; }
        public int? FPRevisionNo { get; set; }
        public int? FPPreProcessRevNo { get; set; }
        public bool? PORevisionNeed { get; set; }
        public bool YBRevisionNeed { get; set; }
        public int AdditionalBooking { get; set; }
        public string FPRejectReason { get; set; }
        public bool HoldYP { get; set; }
        public int? HoldYPBy { get; set; }
        public DateTime? HoldYPDate { get; set; }
        public string HoldYPReason { get; set; }
        public bool IsCancel { get; set; }
        public int CancelReasonID { get; set; }
        public int CanceledBy { get; set; }
        public DateTime? DateCanceled { get; set; }
        public bool Exported { get; set; }
        public DateTime? DateExported { get; set; }
        public int ExportNo { get; set; }
        public bool WithoutOB { get; set; }
        public int SubGroupID { get; set; }
        public bool AllowForNextStep { get; set; }
        public int TNADays { get; set; }
        public DateTime? FabricInHouseDate { get; set; }
        public bool ApproveFP { get; set; }
        public string RevisionReason { get; set; }
        public bool IsYarnStock { get; set; }

        public bool UnAcknowledge { get; set; }
        public DateTime? UnAcknowledgeDate { get; set; }
        public int UnAcknowledgeBy { get; set; }
        public string UnAckReason { get; set; }

        public bool IsAddition { get; set; }
        public int AdditionNo { get; set; }


        public bool IsQtyFinalizationPMCApprove { get; set; }
        public int QtyFinalizationPMCApproveBy { get; set; }
        public DateTime? QtyFinalizationPMCApproveDate { get; set; }

        public bool IsQtyFinalizationPMCReject { get; set; }
        public int QtyFinalizationPMCRejectBy { get; set; }
        public DateTime? QtyFinalizationPMCRejectDate { get; set; }
        public string QtyFinalizationPMCRejectReason { get; set; }

        public bool IsProdHeadApprove { get; set; }
        public int ProdHeadApproveBy { get; set; }
        public DateTime? ProdHeadApproveDate { get; set; }

        public bool IsProdHeadReject { get; set; }
        public int ProdHeadRejectBy { get; set; }
        public DateTime? ProdHeadRejectDate { get; set; }
        public string ProdHeadRejectReason { get; set; }

        public bool IsTextileHeadApprove { get; set; }
        public int TextileHeadApproveBy { get; set; }
        public DateTime? TextileHeadApproveDate { get; set; }

        public bool IsTextileHeadReject { get; set; }
        public int TextileHeadRejectBy { get; set; }
        public DateTime? TextileHeadRejectDate { get; set; }
        public string TextileHeadRejectReason { get; set; }

        public bool IsKnittingUtilizationApprove { get; set; }
        public int KnittingUtilizationApproveBy { get; set; }
        public DateTime? KnittingUtilizationApproveDate { get; set; }

        public bool IsKnittingUtilizationReject { get; set; }
        public int KnittingUtilizationRejectBy { get; set; }
        public DateTime? KnittingUtilizationRejectDate { get; set; }
        public string KnittingUtilizationRejectReason { get; set; }

        public bool IsKnittingHeadApprove { get; set; }
        public int KnittingHeadApproveBy { get; set; }
        public DateTime? KnittingHeadApproveDate { get; set; }

        public bool IsKnittingHeadReject { get; set; }
        public int KnittingHeadRejectBy { get; set; }
        public DateTime? KnittingHeadRejectDate { get; set; }
        public string KnittingHeadRejectReason { get; set; }

        public bool IsOperationHeadApprove { get; set; }
        public int OperationHeadApproveBy { get; set; }
        public DateTime? OperationHeadApproveDate { get; set; }

        public bool IsOperationHeadReject { get; set; }
        public int OperationHeadRejectBy { get; set; }
        public DateTime? OperationHeadRejectDate { get; set; }
        public string OperationHeadRejectReason { get; set; }

        public bool IsRevised { get; set; }
        public int RevisedBy { get; set; }
        public DateTime? RevisedDate { get; set; }
        public int PMCFinalApproveCount { get; set; } = 0;
        public bool IsForAllocationRevise { get; set; } = false;

        public bool IsAllocationInternalRevise_Additional { get; set; } = false;
        public int AllocationInternalReviseBy_Additional { get; set; } = 0;
        public DateTime? AllocationInternalReviseDate_Additional { get; set; }
        public string AllocationInternalReviseReason_Additional { get; set; }

        public int YarnBookingRevisionTypeID { get; set; } = 0;
        public int AcknowledgeCount { get; set; } = 0;


        #region Additional Columns 
        [Write(false)]
        public int ContactID { get; set; } = 0;
        [Write(false)]
        public int IsFabric { get; set; }
        [Write(false)]
        public int IsCollar { get; set; }
        [Write(false)]
        public int IsCuff { get; set; }

        [Write(false)]
        public int BOMMasterID { get; set; } = 0;
        [Write(false)]
        public string AcknowledgeStatus { get; set; }
        [Write(false)]
        public string BookingNo { get; set; }
        [Write(false)]
        public DateTime BookingDate { get; set; }
        [Write(false)]
        public string ExportOrderNo { get; set; }
        [Write(false)]
        public string BuyerDepartment { get; set; }
        [Write(false)]
        public string MerchandiserName { get; set; }
        [Write(false)]
        public string ReferenceNo { get; set; }
        [Write(false)]
        public int IsCompleteDelivery { get; set; }
        [Write(false)]
        public List<YarnBookingMaster> YarnBookings { get; set; }
        //[Write(false)]
        //public int CalendarDays { get; set; }
        [Write(false)]
        public DateTime RequiredDate { get; set; }
        [Write(false)]
        public DateTime EmployeeName { get; set; }
        [Write(false)]
        public int PreRevisionNo { get; set; }
        [Write(false)]
        public bool IsSample { get; set; }
        [Write(false)]
        public int RevStatus { get; set; }
        [Write(false)]
        public string CancelReason { get; set; }
        [Write(false)]
        public string GroupName { get; set; }
        [Write(false)]
        public string ContactPersonName { get; set; }
        [Write(false)]
        public string Depertment { get; set; }
        [Write(false)]
        public int StyleMasterID { get; set; }
        [Write(false)]
        public string YearName { get; set; }
        [Write(false)]
        public bool HasFabric { get; set; }
        [Write(false)]
        public bool HasCollar { get; set; }
        [Write(false)]
        public bool HasCuff { get; set; }
        [Write(false)]
        public DateTime? RevisionDate { get; set; }
        [Write(false)]
        public string OwnerFactory { get; set; }
        [Write(false)]
        public DateTime? FirstInHouseDate { get; set; }
        [Write(false)]
        public DateTime? InHouseDate { get; set; }
        [Write(false)]

        // These properties below may not be required.
        public bool SwatchAttached { get; set; }
        [Write(false)]
        public string ImagePath { get; set; }
        [Write(false)]
        public int SupplierID { get; set; }
        [Write(false)]
        public string ReasonName { get; set; }
        [Write(false)]
        public bool Proposed { get; set; }
        [Write(false)]
        public bool IsRePurchase { get; set; }
        [Write(false)]
        public int RePurchaseQty { get; set; }
        [Write(false)]
        public string CancelReasonName { get; set; }
        [Write(false)]
        public int PriceReProposeReasonID { get; set; }
        [Write(false)]
        public bool PriceReSuggest { get; set; }
        [Write(false)]
        public DateTime? PriceReSuggestDate { get; set; }
        [Write(false)]
        public int PriceReSuggestBy { get; set; }
        [Write(false)]
        public DateTime? PriceReAgreeDate { get; set; }
        [Write(false)]
        public int PriceReAgreeBy { get; set; }
        [Write(false)]
        public string ReProposeAcceptReason { get; set; }
        [Write(false)]
        public string UnAcknowledgeReason { get; set; }
        [Write(false)]
        public string PriceReProposeReasonName { get; set; }
        [Write(false)]
        public bool RevisionNeed { get; set; }
        [Write(false)]
        public string SupplierName { get; set; }
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public int SampleID { get; set; }
        [Write(false)]
        public int ContactPersonID { get; set; }
        [Write(false)]
        public int BlockBookingID { get; set; }
        [Write(false)]
        public bool IsSizeLevel { get; set; }
        [Write(false)]
        public string BookingItemName { get; set; }
        [Write(false)]
        public string CalculateBy { get; set; }
        [Write(false)]
        public bool MustRevise { get; set; }
        [Write(false)]
        public bool SendForRevision { get; set; }
        [Write(false)]
        public bool RevisionAllowed { get; set; }
        [Write(false)]
        public int RevisionAllowRejectBy { get; set; }
        [Write(false)]
        public DateTime? RevisionAllowRejectDate { get; set; }
        [Write(false)]
        public bool PricePropose { get; set; }
        [Write(false)]
        public DateTime? PriceProposeDate { get; set; }
        [Write(false)]
        public DateTime? FirstPProposeDate { get; set; }
        [Write(false)]
        public string PriceProposeNo { get; set; }
        [Write(false)]
        public bool PriceAgree { get; set; }
        [Write(false)]
        public DateTime? PriceAgreeDate { get; set; }
        [Write(false)]
        public bool PriceAgreeBy { get; set; }
        [Write(false)]
        public bool PriceSuggest { get; set; }
        [Write(false)]
        public DateTime? PriceSuggestDate { get; set; }
        [Write(false)]
        public int PriceSuggestBy { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string CompanyName { get; set; }
        [Write(false)]
        public string BuyerTeamName { get; set; }
        [Write(false)]
        public string StyleNo { get; set; }
        [Write(false)]
        public bool AdditionalFlag { get; set; }

        [Write(false)]
        public bool CancelFlag { get; set; }

        [Write(false)]
        public int LastAdditionalBooking { get; set; }

        [Write(false)]
        public bool RevisionFlag { get; set; }

        [Write(false)]
        public string BOMStatus { get; set; }
        [Write(false)]
        public int ExecutionCompanyID { get; set; }
        [Write(false)]
        public decimal BookingQty { get; set; }
        [Write(false)]
        public string SLNo { get; set; }
        [Write(false)]
        public string SeasonName { get; set; }
        [Write(false)]
        public bool F_isSample { get; set; }
        [Write(false)]
        public string F_BookingNo { get; set; }
        [Write(false)]
        public string F_YBookingNo { get; set; }
        [Write(false)]
        public string F_ReasonStatus { get; set; }

        [Write(false)]
        public int F_YBookingID { get; set; }
        [Write(false)]
        public string F_Status { get; set; }
        [Write(false)]
        public bool IsRevice { get; set; }
        [Write(false)]
        public int FBAckID { get; set; }
        [Write(false)]
        public int BookingRevisionNo { get; set; } = 0;
        [Write(false)]
        public int TNACalendarDays { get; set; }
        [Write(false)]
        public DateTime? RequiredFabricDeliveryDate { get; set; }
        [Write(false)]
        public string TeamLeader { get; set; }
        [Write(false)]
        public string CollarSizeID { get; set; }
        [Write(false)]
        public decimal CollarWeightInGm { get; set; }
        [Write(false)]
        public string CuffSizeID { get; set; }
        [Write(false)]
        public decimal CuffWeightInGm { get; set; }
        [Write(false)]
        public bool IsYarnRevision { get; set; } = false;
        [Write(false)]
        public bool IsApprove { get; set; } = false;
        [Write(false)]
        public bool IsReject { get; set; } = false;
        [Write(false)]
        public int YBRTypeID { get; set; } = 0;
        [Write(false)]
        public List<FabricWastageGrid> FabricWastageGridList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> FiberPartList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnSubBrandList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TechnicalNameList { get; set; }
        [Write(false)]
        public int[] CompanyIDs { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> ContactPersonList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CancelReasonList { get; set; }
        [Write(false)]
        public List<YarnBookingChild> Childs { get; set; }
        [Write(false)]
        public List<YarnBookingChild> ChildsGroup { get; set; }
        [Write(false)]
        public List<YarnBookingChildItem> ChildItems { get; set; }
        [Write(false)]
        public List<YarnBookingChildItemRevision> ChildItemsRevision { get; set; } = new List<YarnBookingChildItemRevision>();
        [Write(false)]
        public List<YarnBookingChildItem> ChildItemsGroup { get; set; }
        [Write(false)]
        public List<YarnBookingReason> yarnBookingReason { get; set; }
        [Write(false)]
        public List<YarnBookingChildYarnSubBrand> yarnBookingChildYarnSubBrand { get; set; }
        [Write(false)]
        public List<YarnBookingChildGarmentPart> yarnBookingChildGarmentPart { get; set; }
        [Write(false)]
        public List<YarnItemPrice> yarnItemPrice { get; set; }
        [Write(false)]
        public List<YarnBookingChildItemYarnSubBrand> yarnBookingChildItemYarnSubBrand { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnShadeBooks { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> MCTypeForFabricList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SpinnerList { get; set; }
        [Write(false)]
        public List<KnittingMachine> KnittingMachines { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> MCTypeForOtherList { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YBookingID > 0;
        [Write(false)]
        public List<YarnBookingChild> Fabrics { get; set; }
        [Write(false)]
        public List<YarnBookingChild> Collars { get; set; }
        [Write(false)]
        public List<YarnBookingChild> Cuffs { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModelExtended> YarnSubProgramNews { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModelExtended> Certifications { get; set; }

        [Write(false)]
        public IEnumerable<string> FabricComponents { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> FabricComponentsNew { get; set; }
        [Write(false)]
        public List<Select2OptionModel> Spinners { get; set; }
        [Write(false)]
        public List<Select2OptionModel> GaugeList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> DiaList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> CollarSizeList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> CuffSizeList { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> AllCollarSizeList { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> AllCuffSizeList { get; set; }
        [Write(false)]
        public List<KnittingMachine> MachineBrandList { get; set; }
        [Write(false)]
        public List<KnittingMachine> CollarCuffBrandList { get; set; }
        [Write(false)]
        public List<YarnBookingMaster_New_RevisionReason> RevisionReasonList { get; set; } = new List<YarnBookingMaster_New_RevisionReason>();
        [Write(false)]
        public List<FabricComponentMappingSetup> FabricComponentMappingSetupList { get; set; } = new List<FabricComponentMappingSetup>();
        #region For Fabric Costing
        [Write(false)]
        public List<FabricItemPrice_New> FabricItemPriceList { get; set; } = new List<FabricItemPrice_New>();
        [Write(false)]
        public List<FabricItemPriceBD> FabricItemPriceBDList { get; set; } = new List<FabricItemPriceBD>();
        [Write(false)]
        public List<FabricItemPriceBDChild> FabricItemPriceBDChildList { get; set; } = new List<FabricItemPriceBDChild>();
        [Write(false)]
        public List<FabricItemPriceAddProcess_New> FabricItemPriceAddProcessList { get; set; } = new List<FabricItemPriceAddProcess_New>();
        [Write(false)]
        public List<YarnBookingChildItem> YarnBookingChildItemList { get; set; } = new List<YarnBookingChildItem>();

        #endregion

        #endregion Additional Columns
        public YarnBookingMaster()
        {
            Childs = new List<YarnBookingChild>();
            ChildsGroup = new List<YarnBookingChild>();
            ChildItems = new List<YarnBookingChildItem>();
            ChildItemsGroup = new List<YarnBookingChildItem>();
            yarnBookingReason = new List<YarnBookingReason>();
            yarnBookingChildYarnSubBrand = new List<YarnBookingChildYarnSubBrand>();
            yarnBookingChildGarmentPart = new List<YarnBookingChildGarmentPart>();
            yarnItemPrice = new List<YarnItemPrice>();
            SpinnerList = new List<Select2OptionModel>();
            yarnBookingChildItemYarnSubBrand = new List<YarnBookingChildItemYarnSubBrand>();
            FabricWastageGridList = new List<FabricWastageGrid>();
            KnittingMachines = new List<KnittingMachine>();
            Fabrics = new List<YarnBookingChild>();
            Collars = new List<YarnBookingChild>();
            Cuffs = new List<YarnBookingChild>();
            YarnBookings = new List<YarnBookingMaster>();
            Spinners = new List<Select2OptionModel>();
            GaugeList = new List<Select2OptionModel>();
            DiaList = new List<Select2OptionModel>();
            CollarSizeList = new List<Select2OptionModel>();
            CuffSizeList = new List<Select2OptionModel>();
            AllCollarSizeList = new List<FBookingAcknowledgeChild>();
            AllCuffSizeList = new List<FBookingAcknowledgeChild>();
            MachineBrandList = new List<KnittingMachine>();
            CollarCuffBrandList = new List<KnittingMachine>();

            BuyerID = 0;
            BuyerTeamID = 0;
            PreProcessRevNo = 0;
            RevisionNo = 0;
            CompanyID = 0;
            HasYPrice = false;
            YPRevisionNo = 0;
            ApproveYP = false;
            FPRevisionNo = 0;
            FPPreProcessRevNo = 0;
            PORevisionNeed = false;
            HoldYPBy = 0;
            Propose = false;
            Acknowledge = false;
            AcknowledgeBy = 0;
            UnApproveYP = false;
            YPPreProcessRevNo = 0;
            YBRevisionNeed = false;
            AdditionalBooking = 0;
            HoldYP = false;
            IsCancel = false;
            CancelReasonID = 0;
            CanceledBy = 0;
            Exported = false;
            ExportNo = 0;
            WithoutOB = false;
            SubGroupID = 0;
            AllowForNextStep = false;
            TNADays = 0;
            ApproveFP = false;
            YBookingNo = AppConstants.NEW;
            RequiredDate = DateTime.Now;
            YInHouseDate = DateTime.Now;
            DateAdded = DateTime.Now;
            PreRevisionNo = 0;
            FBAckID = 0;
            UnAcknowledge = false;
            UnAcknowledgeBy = 0;
            UnAckReason = "";
            IsAddition = false;
            AdditionNo = 0;

            IsQtyFinalizationPMCApprove = false;
            QtyFinalizationPMCApproveBy = 0;
            IsQtyFinalizationPMCReject = false;
            QtyFinalizationPMCRejectBy = 0;
            IsProdHeadApprove = false;
            ProdHeadApproveBy = 0;
            IsProdHeadReject = false;
            ProdHeadRejectBy = 0;
            IsTextileHeadApprove = false;
            TextileHeadApproveBy = 0;
            IsTextileHeadReject = false;
            TextileHeadRejectBy = 0;
            IsKnittingUtilizationApprove = false;
            KnittingUtilizationApproveBy = 0;
            IsKnittingUtilizationReject = false;
            KnittingUtilizationRejectBy = 0;
            IsKnittingHeadApprove = false;
            KnittingHeadApproveBy = 0;
            IsKnittingHeadReject = false;
            KnittingHeadRejectBy = 0;
            IsOperationHeadApprove = false;
            OperationHeadApproveBy = 0;
            IsOperationHeadReject = false;
            OperationHeadRejectBy = 0;
            IsRevised = false;
            RevisedBy = 0;

            TNACalendarDays = 0;
            BookingQty = 0;
            CollarWeightInGm = 0;
            CuffWeightInGm = 0;
        }
    }
    //public class YarnBookingMasterValidator : AbstractValidator<YarnBookingMaster>
    //{
    //    public YarnBookingMasterValidator()
    //    {
    //        RuleForEach(x => x.Childs).SetValidator(new YarnBookingChildValidator());
    //    }
    //}

    public class LastAdditionalBookingList
    {
        public int LastAdditionalBooking { get; set; }
    }
}
