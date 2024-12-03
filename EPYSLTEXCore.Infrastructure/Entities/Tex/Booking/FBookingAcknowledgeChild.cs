using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD)]
    public class FBookingAcknowledgeChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int BookingChildID { get; set; }

        [ExplicitKey]
        public int AcknowledgeID { get; set; }

        [ExplicitKey]
        public int BookingID { get; set; }

        public int ConsumptionChildID { get; set; } = 0;

        public int ConsumptionID { get; set; } = 0;

        public int BOMMasterID { get; set; } = 0;

        public int ExportOrderID { get; set; } = 0;

        public int ItemGroupID { get; set; } = 0;

        public int SubGroupID { get; set; } = 0;

        public int ItemMasterID { get; set; } = 0;

        public int OrderBankPOID { get; set; } = 0;

        public int ColorID { get; set; } = 0;

        public int SizeID { get; set; } = 0;

        public int TechPackID { get; set; } = 0;

        public decimal ConsumptionQty { get; set; } = 0;

        public decimal BookingQty { get; set; } = 0;

        public int BookingUnitID { get; set; } = 0;

        public decimal RequisitionQty { get; set; } = 0;

        public bool ISourcing { get; set; } = false;

        public string Remarks { get; set; }

        public int LengthYds { get; set; } = 0;

        public decimal LengthInch { get; set; } = 0;

        public int FUPartID { get; set; } = 0;

        public int A1ValueID { get; set; } = 0;

        public int YarnBrandID { get; set; } = 0;

        public int ContactID { get; set; } = 0;

        public string LabDipNo { get; set; }

        public int AddedBy { get; set; } = 0;

        public DateTime DateAdded { get; set; }

        public int UpdatedBy { get; set; } = 0;

        public DateTime? DateUpdated { get; set; }

        public int ExecutionCompanyID { get; set; } = 0;

        public decimal BlockBookingQty { get; set; } = 0;

        public decimal AdjustQty { get; set; } = 0;

        public bool AutoAgree { get; set; } = false;

        public decimal Price { get; set; } = 0;

        public decimal SuggestedPrice { get; set; }

        public DateTime? LabdipUpdateDate { get; set; }

        public bool IsCompleteReceive { get; set; } = false;

        public bool IsCompleteDelivery { get; set; } = false;

        public DateTime? LastDCDate { get; set; }

        public string ClosingRemarks { get; set; }

        public int ToItemMasterId { get; set; } = 0;

        public int TechnicalNameId { get; set; } = 0;

        public int MachineTypeId { get; set; } = 0;
        public int MachineGauge { get; set; } = 0;
        public int MachineDia { get; set; } = 0;
        public int BrandID { get; set; } = 0;
        public bool IsSubContact { get; set; } = false;
        public decimal PreviousBookingQty { get; set; } = 0;
        public decimal CurrentBookingQty { get; set; } = 0;
        public decimal LiabilitiesBookingQty { get; set; } = 0;
        public decimal ActualBookingQty { get; set; } = 0;
        public bool SendToMktAck { get; set; } = false;
        public bool IsMktAck { get; set; } = false;
        public bool IsMktUnAck { get; set; } = false;
        public DateTime? AcknowledgeDate { get; set; }
        public int AcknowledgeBy { get; set; } = 0;
        public string AcknowledgeReason { get; set; }
        public bool SendToTxtAck { get; set; } = false;
        public bool IsTxtAck { get; set; } = false;
        public bool IsTxtUnAck { get; set; } = false;
        public DateTime? TxtAcknowledgeDate { get; set; }
        public int TxtAcknowledgeBy { get; set; } = 0;
        public string TxtAcknowledgeReason { get; set; }
        public int TotalDays { get; set; } = 0;
        public DateTime? DeliveryDate { get; set; }
        public int TestReportDays { get; set; } = 0;
        public int FinishingDays { get; set; } = 0;
        public int DyeingDays { get; set; } = 0;
        public int BatchPreparationDays { get; set; } = 0;
        public int KnittingDays { get; set; } = 0;
        public int MaterialDays { get; set; } = 0;
        public int StructureDays { get; set; } = 0;
        public int ReferenceSourceID { get; set; } = 0;
        public string ReferenceNo { get; set; }
        public string ColorReferenceNo { get; set; }
        public int YarnSourceID { get; set; } = 0;

        public decimal ExcessPercentage { get; set; } = 0;
        public decimal ExcessQty { get; set; } = 0;
        public decimal ExcessQtyInKG { get; set; } = 0;
        public decimal TotalQty { get; set; } = 0;
        public decimal TotalQtyInKG { get; set; } = 0;
        public decimal GreyReqQty { get; set; } = 0;
        public decimal GreyLeftOverQty { get; set; } = 0;
        public decimal GreyProdQty { get; set; } = 0;
        public decimal GreyProdQtyPCS { get; set; } = 0;
        public int RefSourceID { get; set; } = 0;
        public string RefSourceNo { get; set; }
        public int SourceConsumptionID { get; set; } = 0;
        public int SourceItemMasterID { get; set; } = 0;
        public bool IsFabricReq { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public decimal BookingQtyKG { get; set; } = 0;
        public string CareCode { get; set; } = "";

        public int RevisionNoWhenDeleted { get; set; } = -1;
        public int RevisionByWhenDeleted { get; set; } = 0;
        public DateTime? RevisionDateWhenDeleted { get; set; } = null;
        public int DayValidDurationId { get; set; } = 0;

        #region Additional Properties
        [Write(false)]
        public decimal TotalFinishFabricStockQty { get; set; } = 0;
        [Write(false)]
        public decimal DeliveredQtyForLiability { get; set; } = 0;

        [Write(false)]
        public string PartName { get; set; } = "";
        [Write(false)]
        public string Construction { get; set; } = "";

        [Write(false)]
        public string Composition { get; set; } = "";

        [Write(false)]
        public string Color { get; set; } = "";

        [Write(false)]
        public string ColorCode { get; set; } = "";

        [Write(false)]
        public string GSM { get; set; } = "";

        [Write(false)]
        public string FabricWidth { get; set; } = "";

        [Write(false)]
        public string TechnicalName { get; set; } = "";

        [Write(false)]
        public string MachineType { get; set; } = "";

        [Write(false)]
        public string KnittingType { get; set; } = "";

        [Write(false)]
        public string ConceptTypeID { get; set; } = "";

        [Write(false)]
        public string BookingNo { get; set; } = "";

        [Write(false)]
        public int ConstructionId { get; set; } = 0;

        [Write(false)]
        public int CompositionId { get; set; } = 0;

        [Write(false)]
        public int KnittingTypeId { get; set; } = 0;

        [Write(false)]
        public int KTypeId { get; set; } = 0;

        [Write(false)]
        public int GSMId { get; set; } = 0;

        [Write(false)]
        public string YarnType { get; set; } = "";

        [Write(false)]
        public string YarnProgram { get; set; } = "";

        [Write(false)]
        public string DyeingType { get; set; } = "";

        [Write(false)]
        public string BookingUOM { get; set; } = "";
        [Write(false)]
        public int DayDuration { get; set; } = 0;
        [Write(false)]
        public string DayValidDurationName { get; set; } = "";
        [Write(false)]
        public string Instruction { get; set; } = "";
        [Write(false)]
        public string RefSourceName { get; set; } = "";
        [Write(false)]
        public string ForBDSStyleNo { get; set; } = "";
        [Write(false)]
        public bool Blending { get; set; } = false;
        [Write(false)]
        public string YarnCategory { get; set; } = "";
        [Write(false)]
        public decimal Distribution { get; set; } = 0;
        [Write(false)]
        public decimal Allowance { get; set; } = 0;
        [Write(false)]
        public decimal RequiredQty { get; set; } = 0;
        [Write(false)]
        public string ShadeCode { get; set; } = "";
        [Write(false)]
        public string Specification { get; set; } = "";
        [Write(false)]
        public bool YD { get; set; } = false;
        [Write(false)]
        public bool YDItem { get; set; } = false;
        [Write(false)]
        public int SpinnerID { get; set; } = 0;
        [Write(false)]
        public string LotNo { get; set; } = "";
        [Write(false)]
        public decimal PreviousConsumptionQty { get; set; } = 0;
        [Write(false)]
        public int PreviousBookingUnitID { get; set; } = 0;
        [Write(false)]
        public decimal PreviousRequisitionQty { get; set; } = 0;
        [Write(false)]
        public decimal PreviousPrice { get; set; } = 0;
        [Write(false)]
        public decimal PreviousSuggestedPrice { get; set; } = 0;
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BookingChildID > 0;

        [Write(false)]
        public string CriteriaName { get; set; } = "";
        [Write(false)]
        public decimal LFDDeliveredQty { get; set; } = 0;
        [Write(false)]
        public decimal LFDAckQty { get; set; } = 0;
        [Write(false)]
        public decimal LFDDeliveredQtyInKG { get; set; } = 0;
        [Write(false)]
        public decimal LFDAckQtyInKG { get; set; } = 0;
        [Write(false)]
        public int ConceptID { get; set; } = 0;
        [Write(false)]
        public string ExportOrderNo { get; set; } = "";
        [Write(false)]
        public int BuyerID { get; set; } = 0;
        [Write(false)]
        public string BuyerName { get; set; } = "";
        [Write(false)]
        public int FabricTypeID { get; set; } = 0;
        [Write(false)]
        public string FabricType { get; set; } = "";
        [Write(false)]
        public int EWOSeries { get; set; } = 0;
        [Write(false)]
        public string FabricStyle { get; set; } = "";
        [Write(false)]
        public decimal StockQtyKg { get; set; } = 0;
        [Write(false)]
        public int RowNumber { get; set; } = 0;
        [Write(false)]
        public int TexBookingChildID { get; set; } = 0;
        [Write(false)]
        public List<FBookingAcknowledgeChild> CriteriaNames { get; set; } = new List<FBookingAcknowledgeChild>();

        [Write(false)]
        public List<FBAChildPlanning> FBAChildPlannings { get; set; } = new List<FBAChildPlanning>();

        [Write(false)]
        public List<FBAChildPlanning> FBAChildPlanningsWithIds { get; set; } = new List<FBAChildPlanning>();

        [Write(false)]
        public List<YarnBookingChildItem> ChildItems { get; set; } = new List<YarnBookingChildItem>();
        [Write(false)]
        public List<YarnBookingChildItemRevision> ChildItemsRevision { get; set; } = new List<YarnBookingChildItemRevision>();
        [Write(false)]
        public List<FBookingAcknowledgeChildDetails> ChildDetails { get; set; } = new List<FBookingAcknowledgeChildDetails>();

        [Write(false)]
        public List<FBookingAcknowledgementLiabilityDistribution> ChildAckLiabilityDetails { get; set; } = new List<FBookingAcknowledgementLiabilityDistribution>();

        [Write(false)]
        public List<FBookingAcknowledgementYarnLiability> ChildAckYarnLiabilityDetails { get; set; } = new List<FBookingAcknowledgementYarnLiability>();

        [Write(false)]
        public List<dynamic> Results { get; set; } = new List<dynamic>();

        [Write(false)]
        public string CriteriaIDs { get; set; } = "";

        [Write(false)]
        public int CriteriaTime { get; set; } = 0;

        [Write(false)]
        public int TechnicalTime { get; set; } = 0;
        [Write(false)]
        public int TotalTime { get; set; } = 0;
        [Write(false)]
        public int FinishingTime { get; set; } = 0;
        [Write(false)]
        public int DyeingTime { get; set; } = 0;
        [Write(false)]
        public int MaterialTime { get; set; } = 0;
        [Write(false)]
        public int KnittingTime { get; set; } = 0;
        [Write(false)]
        public int TestReportTime { get; set; } = 0;
        [Write(false)]
        public int PreprocessTime { get; set; } = 0;
        [Write(false)]
        public int TestingTime { get; set; } = 0;
        [Write(false)]
        public int PreprocessDays { get; set; } = 0;
        [Write(false)]
        public int batchPreparationTime { get; set; } = 0;
        [Write(false)]
        public string ReferenceSourceName { get; set; } = "";
        [Write(false)]
        public string Height { get; set; } = "";
        [Write(false)]
        public string Length { get; set; } = "";
        [Write(false)]
        public string Description { get; set; } = "";
        [Write(false)]
        public int QualityDays { get; set; } = 0;
        [Write(false)]
        public bool DelivereyComplete { get; set; } = false;
        [Write(false)]
        public int DeliveredQty { get; set; } = 0;
        [Write(false)]
        public string Brand { get; set; } = "";
        [Write(false)]
        public int TextileCompanyID { get; set; } = 0;
        [Write(false)]
        public int Segment1ValueID { get; set; } = 0;
        [Write(false)]
        public int Segment2ValueID { get; set; } = 0;
        [Write(false)]
        public int Segment3ValueID { get; set; } = 0;
        [Write(false)]
        public int Segment4ValueID { get; set; } = 0;
        [Write(false)]
        public int Segment5ValueID { get; set; } = 0;
        [Write(false)]
        public int Segment6ValueID { get; set; } = 0;
        [Write(false)]
        public int Segment7ValueID { get; set; } = 0;
        [Write(false)]
        public string Segment1Desc { get; set; } = "";
        [Write(false)]
        public string Segment2Desc { get; set; } = "";
        [Write(false)]
        public string Segment3Desc { get; set; } = "";
        [Write(false)]
        public string Segment4Desc { get; set; } = "";
        [Write(false)]
        public string Segment5Desc { get; set; } = "";
        [Write(false)]
        public string Segment6Desc { get; set; } = "";
        [Write(false)]
        public string Segment7Desc { get; set; } = "";
        [Write(false)]
        public int BRefDetailsID { get; set; } = 0;
        [Write(false)]
        public int YBChildItemID { get; set; } = 0;
        [Write(false)]
        public bool IsForFabric { get; set; } = false;
        [Write(false)]
        public decimal ReplacementQty { get; set; } = 0;
        [Write(false)]
        public bool IsDeletedItem { get; set; } = false;
        [Write(false)]
        public string Status { get; set; } = "";
        [Write(false)]
        public decimal YarnAllowance { get; set; } = 0;
        [Write(false)]
        public decimal FinishFabricUtilizationQty { get; set; } = 0;
        [Write(false)]
        public decimal ReqFinishFabricQty { get; set; } = 0;
        [Write(false)]
        public string Width { get; set; } = "";
        [Write(false)]
        public decimal ExistingYarnAllowance { get; set; } = 0;
        [Write(false)]
        public decimal TotalYarnAllowance { get; set; } = 0;
        [Write(false)]
        public List<FBookingAcknowledgeChildAddProcess> ChildAddProcess { get; set; } = new List<FBookingAcknowledgeChildAddProcess>();
        [Write(false)]
        public List<FBookingAcknowledgeChildDetails> FBChildDetails { get; set; } = new List<FBookingAcknowledgeChildDetails>();
        [Write(false)]
        public List<FBookingAcknowledgeChildGarmentPart> ChildsGpart { get; set; } = new List<FBookingAcknowledgeChildGarmentPart>();
        [Write(false)]
        public List<FBookingAcknowledgeChildProcess> ChildsProcess { get; set; } = new List<FBookingAcknowledgeChildProcess>();
        [Write(false)]
        public List<FBookingAcknowledgeChildText> ChildsText { get; set; } = new List<FBookingAcknowledgeChildText>();
        [Write(false)]
        public List<FBookingAcknowledgeChildDistribution> ChildsDistribution { get; set; } = new List<FBookingAcknowledgeChildDistribution>();
        [Write(false)]
        public List<FBookingAcknowledgeChildYarnSubBrand> ChildsYarnSubBrand { get; set; } = new List<FBookingAcknowledgeChildYarnSubBrand>();
        [Write(false)]
        public List<BDSDependentTNACalander> BDCalander { get; set; } = new List<BDSDependentTNACalander>();
        [Write(false)]
        public List<Select2OptionModel> YarnShadeBooks { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> YarnSubBrandList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> Spinners { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<FBookingAckChildFinishingProcess> PreFinishingProcessChilds { get; set; } = new List<FBookingAckChildFinishingProcess>();

        [Write(false)]
        public List<FBookingAckChildFinishingProcess> PostFinishingProcessChilds { get; set; } = new List<FBookingAckChildFinishingProcess>();

        [Write(false)]
        public List<FBookingAcknowledgeChildGFUtilization> ChildsGFUtilizationList { get; set; } = new List<FBookingAcknowledgeChildGFUtilization>();
        [Write(false)]
        public List<BulkBookingFinishFabricUtilization> FinishFabricUtilizationPopUpList { get; set; } = new List<BulkBookingFinishFabricUtilization>();
        [Write(false)]
        public List<FBookingAcknowledgeChildGFUtilization> GreyFabricUtilizationPopUpList { get; set; } = new List<FBookingAcknowledgeChildGFUtilization>();

        [Write(false)]
        public List<FBookingAcknowledgeChildReplacement> AdditionalReplacementPOPUPList { get; set; } = new List<FBookingAcknowledgeChildReplacement>();
        [Write(false)]
        public List<FBookingAcknowledgeChildItemNetReqQTY> AdditionalNetReqPOPUPList { get; set; } = new List<FBookingAcknowledgeChildItemNetReqQTY>();

        #endregion Additional Properties

        #region Off Constructor 
        //public FBookingAcknowledgeChild()
        //{
        //    DateAdded = DateTime.Now;
        //    AcknowledgeID = 0;
        //    BookingID = 0;
        //    ConsumptionChildID = 0;
        //    ConsumptionID = 0;
        //    BOMMasterID = 0;
        //    ExportOrderID = 0;
        //    ItemGroupID = 0;
        //    SubGroupID = 0;
        //    ItemMasterID = 0;
        //    OrderBankPOID = 0;
        //    ColorID = 0;
        //    SizeID = 0;
        //    TechPackID = 0;
        //    ConsumptionQty = 0m;
        //    BookingQty = 0m;
        //    BookingUnitID = 0;
        //    RequisitionQty = 0m;
        //    ISourcing = false;
        //    LengthYds = 0;
        //    LengthInch = 0m;
        //    FUPartID = 0;
        //    A1ValueID = 0;
        //    YarnBrandID = 0;
        //    ContactID = 0;
        //    AddedBy = 0;
        //    UpdatedBy = 0;
        //    ExecutionCompanyID = 0;
        //    BlockBookingQty = 0m;
        //    AdjustQty = 0m;
        //    TechnicalNameId = 0;
        //    Remarks = "";
        //    LabDipNo = "";
        //    AutoAgree = false;
        //    Price = 0;
        //    SuggestedPrice = 0;
        //    IsCompleteReceive = false;
        //    IsCompleteDelivery = false;
        //    ClosingRemarks = "";
        //    MachineGauge = 0;
        //    MachineDia = 0;
        //    BrandID = 0;
        //    MachineTypeId = 0;
        //    TotalDays = 0;
        //    TestReportDays = 0;
        //    FinishingDays = 0;
        //    DyeingDays = 0;
        //    BatchPreparationDays = 0;
        //    KnittingDays = 0;
        //    MaterialDays = 0;
        //    StructureDays = 0;
        //    ReferenceSourceID = 0;
        //    ReferenceNo = "";
        //    ColorReferenceNo = "";
        //    YarnSourceID = 0;
        //    TextileCompanyID = 0;

        //    GreyReqQty = 0;
        //    GreyLeftOverQty = 0;
        //    GreyProdQty = 0;

        //    RefSourceID = 0;
        //    RefSourceNo = "";
        //    SourceConsumptionID = 0;
        //    SourceItemMasterID = 0;

        //    IsFabricReq = false;
        //    IsDeleted = false;

        //    IsForFabric = false;
        //    ReplacementQty = 0;

        //    ExcessPercentage = 0;
        //    ExcessQty = 0;
        //    ExcessQtyInKG = 0;
        //    TotalQty = 0;
        //    TotalQtyInKG = 0;
        //    Segment1ValueID = 0;
        //    Segment2ValueID = 0;
        //    Segment3ValueID = 0;
        //    Segment4ValueID = 0;
        //    Segment5ValueID = 0;
        //    Segment6ValueID = 0;
        //    Segment7ValueID = 0;

        //    Segment1Desc = "";
        //    Segment2Desc = "";
        //    Segment3Desc = "";
        //    Segment4Desc = "";
        //    Segment5Desc = "";
        //    Segment6Desc = "";
        //    Segment7Desc = "";


        //    BookingUOM = "";
        //    CriteriaIDs = "";
        //    MachineType = "Empty";
        //    TechnicalName = "Empty";

        //    Blending = false;
        //    YarnCategory = "";
        //    Distribution = 0;
        //    Allowance = 0;
        //    Distribution = 0;
        //    RequiredQty = 0;
        //    ShadeCode = "";
        //    Specification = "";
        //    YD = false;
        //    YDItem = false;
        //    BRefDetailsID = 0;
        //    YBChildItemID = 0;

        //    LFDDeliveredQty = 0;
        //    LFDAckQty = 0;
        //    LFDDeliveredQtyInKG = 0;
        //    LFDAckQtyInKG = 0;

        //    Status = "";

        //    YarnAllowance = 0;
        //    FinishFabricUtilizationQty = 0;
        //    ReqFinishFabricQty = 0;

        //    FBAChildPlannings = new List<FBAChildPlanning>();
        //    CriteriaNames = new List<FBookingAcknowledgeChild>();
        //    FBAChildPlanningsWithIds = new List<FBAChildPlanning>();
        //    ChildItems = new List<YarnBookingChildItem>();
        //    Results = new List<dynamic>();
        //    ChildDetails = new List<FBookingAcknowledgeChildDetails>();
        //    ChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
        //    FBChildDetails = new List<FBookingAcknowledgeChildDetails>();
        //    ChildsGpart = new List<FBookingAcknowledgeChildGarmentPart>();
        //    ChildsProcess = new List<FBookingAcknowledgeChildProcess>();
        //    ChildsText = new List<FBookingAcknowledgeChildText>();
        //    ChildsDistribution = new List<FBookingAcknowledgeChildDistribution>();
        //    ChildsYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
        //    ChildAckLiabilityDetails = new List<FBookingAcknowledgementLiabilityDistribution>();
        //    BDCalander = new List<BDSDependentTNACalander>();
        //    YarnShadeBooks = new List<Select2OptionModel>();
        //    YarnSubBrandList = new List<Select2OptionModel>();
        //    Spinners = new List<Select2OptionModel>();
        //    FinishFabricUtilizationPopUpList = new List<BulkBookingFinishFabricUtilization>();
        //    GreyFabricUtilizationPopUpList = new List<FBookingAcknowledgeChildGFUtilization>();

        //    Height = "";
        //    Length = "";
        //    Description = "";
        //    DelivereyComplete = false;
        //    DeliveredQty = 0;
        //    Brand = "Empty";

        //    SpinnerID = 0;
        //    LotNo = "";

        //    BookingQtyKG = 0;

        //    PreFinishingProcessChilds = new List<FBookingAckChildFinishingProcess>();
        //    PostFinishingProcessChilds = new List<FBookingAckChildFinishingProcess>();
        //    ChildsGFUtilizationList = new List<FBookingAcknowledgeChildGFUtilization>();
        //}

        #endregion
    }
}
