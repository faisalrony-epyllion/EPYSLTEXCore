using Dapper.Contrib.Extensions;

using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using FluentValidation;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_FBookingAcknowledge")]
    public class FBookingAcknowledge : DapperBaseEntity
    {
        [ExplicitKey]
        public int FBAckID { get; set; }
        public int BookingID { get; set; }
        public string BookingNo { get; set; }
        public string SLNo { get; set; }
        public System.DateTime BookingDate { get; set; }
        public int BuyerID { get; set; }
        public int BuyerTeamID { get; set; }
        public int ExecutionCompanyID { get; set; }
        public int SupplierID { get; set; }
        public int StyleMasterID { get; set; }
        public string StyleNo { get; set; }
        public int SubGroupID { get; set; }
        public int ExportOrderID { get; set; }
        public decimal BookingQty { get; set; } = 0;
        public int AddedBy { get; set; } = 0;
        public System.DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public System.DateTime? DateUpdated { get; set; }
        public bool IsSample { get; set; }
        public bool IsUnAcknowledge { get; set; }
        public DateTime? UnAcknowledgeDate { get; set; }
        public int? UnAcknowledgeBy { get; set; }
        public string UnAcknowledgeReason { get; set; }
        public int BomMasterId { get; set; } = 0;
        public int ItemGroupId { get; set; }
        public bool Status { get; set; }
        public bool WithoutOB { get; set; }
        public int PreRevisionNo { get; set; }
        public int RevisionNo { get; set; }
        public DateTime? RevisionDate { get; set; }
        public bool IsKnittingComplete { get; set; }
        public int KnittingRevisionNo { get; set; }
        public bool IsCheckByKnittingHead { get; set; }
        public int CheckByKnittingHead { get; set; }
        public DateTime? CheckDateKnittingHead { get; set; }
        public bool IsRejectByKnittingHead { get; set; }
        public int RejectByKnittingHead { get; set; }
        public DateTime? RejectDateKnittingHead { get; set; }
        public string RejectReasonKnittingHead { get; set; }
        public bool IsApprovedByProdHead { get; set; }
        public int ApprovedByProdHead { get; set; }
        public DateTime? ApprovedDateProdHead { get; set; }
        public bool IsRejectByProdHead { get; set; }
        public int RejectByProdHead { get; set; }
        public DateTime? RejectDateProdHead { get; set; }
        public string RejectReasonProdHead { get; set; }
        public bool IsApprovedByPMC { get; set; }
        public int ApprovedByPMC { get; set; }
        public DateTime? ApprovedDatePMC { get; set; }
        public bool IsRejectByPMC { get; set; }
        public int RejectByPMC { get; set; }
        public DateTime? RejectDatePMC { get; set; }
        public string RejectReasonPMC { get; set; }
        public bool IsApprovedByAllowance { get; set; }
        public int ApprovedByAllowance { get; set; }
        public DateTime? ApprovedDateAllowance { get; set; }
        public bool IsRejectByAllowance { get; set; }
        public int RejectByAllowance { get; set; }
        public DateTime? RejectDateAllowance { get; set; }
        public string RejectReasonAllowance { get; set; }
        public int MerchandiserID { get; set; }
        public int FinancialYearID { get; set; }
        public int SeasonID { get; set; }
        public int PreRevisionNoLabdip { get; set; }
        public int RevisionNoLabdip { get; set; }
        public int PreRevisionNoBBKI { get; set; }
        public int RevisionNoBBKI { get; set; }
        public bool IsReviseBBKI { get; set; }
        public bool IsUtilizationProposalConfirmed { get; set; }
        public int UtilizationProposalConfirmedBy { get; set; }
        public DateTime? UtilizationProposalConfirmedDate { get; set; }
        public bool IsUtilizationProposalSend { get; set; }
        public int UtilizationProposalSendBy { get; set; }
        public DateTime? UtilizationProposalSendDate { get; set; }
        public string RivisionReason { get; set; }
        public string CollarSizeID { get; set; }
        public decimal CollarWeightInGm { get; set; }
        public string CuffSizeID { get; set; }
        public decimal CuffWeightInGm { get; set; }
        public bool IsInternalRevise { get; set; }
        public int InternalReviseBy { get; set; }
        public string InternalReviseReason { get; set; }
        public DateTime? InternalReviseDate { get; set; }
        public int BaseTypeId { get; set; }
        public int KnittingCompleteBy { get; set; } = 0;
        public DateTime? KnittingCompleteDate { get; set; }
        public bool IsAllocationInternalRevise { get; set; }
        public int AllocationInternalReviseBy { get; set; }
        public DateTime? AllocationInternalReviseDate { get; set; }
        public string AllocationInternalReviseReason { get; set; }
        public bool IsValidForYarnBookingAcknowledge { get; set; } = false;


        #region Additional Properties
        [Write(false)]
        public string FabricBookingType { get; set; } = "";
        [Write(false)]
        public string IsRevMktAck { get; set; }
        [Write(false)]
        public string IsRevisionAck { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public int TechnicalNameId { get; set; }
        [Write(false)]
        public string BuyerTeamName { get; set; }
        [Write(false)]
        public string CompanyName { get; set; }
        [Write(false)]
        public string SupplierName { get; set; }
        [Write(false)]
        public string Remarks { get; set; }
        [Write(false)]
        public string SeasonName { get; set; }
        [Write(false)]
        public int YBookingID { get; set; }
        [Write(false)]
        public string YBookingNo { get; set; }
        [Write(false)]
        public string ExportOrderNo { get; set; }
        [Write(false)]
        public string EmployeeName { get; set; }
        [Write(false)]
        public decimal OrderQty { get; set; }
        [Write(false)]
        public string ImagePath { get; set; }
        [Write(false)]
        public string ImagePath1 { get; set; }
        [Write(false)]
        public string AckByName { get; set; }
        [Write(false)]
        public string UnAckByName { get; set; }
        [Write(false)]
        public int TextileCompanyID { get; set; }
        [Write(false)]
        public int CompanyID { get; set; }
        [Write(false)]
        public string PageName { get; set; }
        [Write(false)]
        public int TNACalendarDays { get; set; }
        [Write(false)]
        public bool IsBulkBooking { get; set; }
        [Write(false)]
        public string ActionStatus { get; set; }
        [Write(false)]
        public string BOMStatus { get; set; }
        [Write(false)]
        public string RevNoValue { get; set; }
        [Write(false)]
        public string BBStatus { get; set; }
        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public string SaveType { get; set; }
        [Write(false)]
        public bool HasYarnBooking { get; set; }
        [Write(false)]
        public DateTime? FBAcknowledgeDate { get; set; }
        [Write(false)]
        public DateTime? BKAcknowledgeDate { get; set; }
        [Write(false)]
        public string RejectReason { get; set; }
        [Write(false)]
        public bool IsReject { get; set; }
        [Write(false)]
        public bool IsApprove { get; set; } = false;
        [Write(false)]
        public bool LabdipAcknowledge { get; set; }
        [Write(false)]
        public int LabdipAcknowledgeBY { get; set; }
        [Write(false)]
        public DateTime? LabdipAcknowledgeDate { get; set; }
        [Write(false)]
        public string LabdipUnAcknowledgeReason { get; set; }
        [Write(false)]
        public bool IsAddition { get; set; }
        [Write(false)]
        public bool IsUpdateAddition { get; set; }
        [Write(false)]
        public string ParentYBookingNo { get; set; }
        [Write(false)]
        public bool IsRevisionValid { get; set; }
        [Write(false)]
        public bool IsInvalidBooking { get; set; }
        [Write(false)]
        public int YarnPreRevisionNo { get; set; }
        [Write(false)]
        public int UserId { get; set; }
        [Write(false)]
        public int LFDMasterID { get; set; }
        [Write(false)]
        public int LFDChildID { get; set; }
        [Write(false)]
        public decimal RequiredQty { get; set; }
        [Write(false)]
        public decimal StockQty { get; set; }
        [Write(false)]
        public decimal DeliveredQty { get; set; }
        [Write(false)]
        public DateTime? RequiredFabricDeliveryDate { get; set; }
        [Write(false)]
        public DateTime? ApproveDate { get; set; }
        [Write(false)]
        public string RevisionReason { get; set; }
        [Write(false)]
        public string BookingStatus { get; set; }
        [Write(false)]
        public string YarnBookingRevisionDate { get; set; }
        [Write(false)]
        public int CalendarDays { get; set; }
        [Write(false)]
        public DateTime? FirstShipmentDate { get; set; }
        [Write(false)]
        public DateTime? YarnBookingDate { get; set; }
        [Write(false)]
        public DateTime? AddYarnBookingDate { get; set; }
        [Write(false)]
        public string BookingType { get; set; }
        [Write(false)]
        public DateTime? FabricStartDate { get; set; }
        [Write(false)]
        public DateTime? FabricEndDate { get; set; }
        [Write(false)]
        public decimal OrderQtyKG { get; set; }
        [Write(false)]
        public string PMCApprovedBy { get; set; }
        [Write(false)]
        public string PMCRejectedBy { get; set; }
        [Write(false)]
        public DateTime? ApproveRejectDatePMC { get; set; }
        [Write(false)]
        public string PMCApprovedRejectedBy { get; set; }
        [Write(false)]
        public string TeamLeader { get; set; }
        [Write(false)]
        public int EWOStatusID { get; set; }
        [Write(false)]
        public int OrderBankMasterID { get; set; }
        [Write(false)]
        public int MenuId { get; set; }
        [Write(false)]
        public string InternalRivisionReason { get; set; }
        [Write(false)]
        public Status GridStatus { get; set; }
        [Write(false)]
        public bool IsMktRevisionPending { get; set; } = false;
        [Write(false)]
        public string BtnId { get; set; }
        [Write(false)]
        public string BookingByName { get; set; }
        [Write(false)]
        public string YarnBookingByName { get; set; }
        [Write(false)]
        public string MerchandisingTeam { get; set; }
        [Write(false)]
        public string YarnProjectionReference { get; set; }
        [Write(false)]
        public DateTime FabricRequireDate { get; set; }
        [Write(false)]
        public DateTime FabricRequireDateEnd { get; set; }
        [Write(false)]
        public DateTime? GarmentsShipmentDate { get; set; }
        [Write(false)]
        public DateTime YarnRequiredDate { get; set; }
        [Write(false)]
        public bool IsYarnRevision { get; set; } = false;
        [Write(false)]
        public int YarnRevisionNo { get; set; } = 0;
        [Write(false)]
        public DateTime? YarnRevisedDate { get; set; }
        [Write(false)]
        public string YarnAcknowledgeBy { get; set; } = "";
        [Write(false)]
        public DateTime? YarnAcknowledgeDate { get; set; }
        [Write(false)]
        public string YarnUnAcknowledgeBy { get; set; } = "";
        [Write(false)]
        public DateTime? YarnUnAcknowledgeDate { get; set; }
        [Write(false)]
        public bool IsIncreaseRevisionNo { get; set; } = false;
        [Write(false)]
        public int PMCFinalApproveCount { get; set; } = 0;

        [Write(false)]
        public bool IsQtyFinalizationPMCReject { get; set; } = false;
        [Write(false)]
        public string QtyFinalizationPMCRejectReason { get; set; } = "";
        [Write(false)]
        public bool IsProdHeadReject { get; set; } = false;
        [Write(false)]
        public string ProdHeadRejectReason { get; set; } = "";
        [Write(false)]
        public bool IsTextileHeadReject { get; set; } = false;
        [Write(false)]
        public string TextileHeadRejectReason { get; set; } = "";
        [Write(false)]
        public bool IsKnittingUtilizationReject { get; set; } = false;
        [Write(false)]
        public string KnittingUtilizationRejectReason { get; set; } = "";
        [Write(false)]
        public bool IsKnittingHeadReject { get; set; } = false;
        [Write(false)]
        public string KnittingHeadRejectReason { get; set; } = "";
        [Write(false)]
        public bool IsOperationHeadReject { get; set; } = false;
        [Write(false)]
        public string OperationHeadRejectReason { get; set; } = "";
        [Write(false)]
        public bool IsAdditionalRevise { get; set; } = false;
        [Write(false)]
        public int YarnBookingRevisionTypeID { get; set; } = 0;
        [Write(false)]
        public List<FBookingAcknowledgeChild> FBookingChild { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> FBookingChildCollor { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> FBookingChildCuff { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> FBookingChildDetailsgroup { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> FBChilds { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> ChangesChilds { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildDetails> FBookingChildDetails { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildDetails> FBookingChildDetailsCollar { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildDetails> FBookingChildDetailsCuff { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildAddProcess> FBookingAcknowledgeChildAddProcess { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildGarmentPart> FBookingAcknowledgeChildGarmentPart { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildProcess> FBookingAcknowledgeChildProcess { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildText> FBookingAcknowledgeChildText { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildDistribution> FBookingAcknowledgeChildDistribution { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildYarnSubBrand> FBookingAcknowledgeChildYarnSubBrand { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeImage> FBookingAcknowledgeImage { get; set; }
        [Write(false)]
        public List<BDSDependentTNACalander> BDSDependentTNACalander { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> Childs { get; set; }
        [Write(false)]
        public List<KnittingMachine> KnittingMachines { get; set; }
        [Write(false)]
        public List<FreeConceptMaster> FreeConcepts { get; set; }
        [Write(false)]
        public List<FreeConceptMaster> FreeConceptsCollar { get; set; }
        [Write(false)]
        public List<FreeConceptMaster> FreeConceptsCuff { get; set; }
        [Write(false)]
        public List<FBAChildPlanning> AllChildPlannings { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TechnicalNameList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnSourceNameList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> MCTypeForFabricList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> MCTypeForOtherList { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChildColor> ColorCodeList { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgementLiabilityDistribution> FBookingAckLiabilityDistributionList { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgementYarnLiability> FBookingAcknowledgementYarnLiabilityList { get; set; }
        [Write(false)]
        public List<FabricBookingAcknowledge> FabricBookingAcknowledgeList { get; set; }
        [Write(false)]
        public List<FBookingAcknowledge> FBookingAcknowledgeList { get; set; }
        [Write(false)]
        public List<YarnBookingMaster> YarnBookings { get; set; }
        [Write(false)]
        public List<BulkBookingFinishFabricUtilization> FinishFabricUtilizationPopUpList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SpinnerList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> GaugeList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> DiaList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnShadeBooks { get; set; }
        [Write(false)]
        public List<YarnBookingChild> YChilds { get; set; }
        [Write(false)]
        public List<YarnBookingChildItem> ChildItems { get; set; }
        [Write(false)]
        public List<YarnBookingChildItemRevision> ChildItemsRevision { get; set; } = new List<YarnBookingChildItemRevision>();
        [Write(false)]
        public List<SFDChild> DeliveryChilds { get; set; }
        [Write(false)]
        public List<SFDChildRoll> DeliveryChildItems { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnSubProgramNews { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModelExtended> Certifications { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> FabricComponentsNew { get; set; }
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
        public IEnumerable<Select2OptionModel> DayValidDurations { get; set; }
        [Write(false)]
        public YarnPRMaster PRMaster { get; set; }
        [Write(false)]
        public List<FabricComponentMappingSetup> FabricComponentMappingSetupList { get; set; } = new List<FabricComponentMappingSetup>();

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FBAckID > 0;
        [Write(false)]
        public bool HasFabric { get; set; }
        [Write(false)]
        public bool HasCollar { get; set; }
        [Write(false)]
        public bool HasCuff { get; set; }
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public int BookingBy { get; set; }
        [Write(false)]
        public string PendingRevision { get; set; }
        [Write(false)]
        public string ColorCode { get; set; }
        [Write(false)]
        public string grpConceptNo { get; set; }
        [Write(false)]
        public int IsBDS { get; set; }
        [Write(false)]
        public bool IsRevised { get; set; }
        [Write(false)]
        public bool PreProcessRevNo { get; set; }
        [Write(false)]
        public string CreatedByName { get; set; }
        [Write(false)]
        public string FabricBookingStatus { get; set; } = "";
        [Write(false)]
        public string StatusText { get; set; }
        [Write(false)]
        public string Reason { get; set; }
        [Write(false)]
        public bool IsLabdipAcknowledge { get; set; }
        [Write(false)]
        public int ParamTypeId { get; set; }
        [Write(false)]
        public string DeliveryNo { get; set; }
        [Write(false)]
        public bool IsLabdip { get; set; }
        [Write(false)]
        public string PMCApproveBy { get; set; }
        [Write(false)]
        public int GmtQtyPcs { get; set; }
        [Write(false)]
        public string UnAcknowledgeByName { get; set; }
        [Write(false)]
        public DateTime? AcknowledgeDate { get; set; }
        [Write(false)]
        public string AcknowledgeByName { get; set; }
        [Write(false)]
        public bool IsRevisedYarn { get; set; }
        [Write(false)]
        public string Ageing { get; set; } = "";
        [Write(false)]
        public DateTime? StartDate { get; set; } = null;
        [Write(false)]
        public DateTime? BookingDateFR { get; set; } = null;


        [Write(false)]
        public DateTime? YarnBookingDateActual { get; set; }
        [Write(false)]
        public DateTime? YarnBookingDateFR { get; set; }
        [Write(false)]
        public string YarnBookingStatus { get; set; } = "";
        [Write(false)]
        public string YarnBookingCreatedBy { get; set; } = "";
        [Write(false)]
        public string YarnProjectionRef { get; set; } = "";
        [Write(false)]
        public DateTime? YarnRequiredDateFR { get; set; }
        [Write(false)]
        public DateTime? YarnRequiredDateBOYB { get; set; }
        [Write(false)]
        public string Merchandiser { get; set; } = "";
        [Write(false)]
        public int TNACalender { get; set; } = 0;
        [Write(false)]
        public string ListTypeMasterGrid { get; set; } = "";
        //FabricRevisionDate
        [Write(false)]
        public bool Acknowledge { get; set; } = false;





        [Write(false)]
        public List<Select2OptionModel> AdditionalYarnBookingReason { get; set; }
        [Write(false)]
        public List<Select2OptionModel> AdditionalEFLCompanyList { get; set; }
        [Write(false)]
        public List<YarnBookingMaster_New_RevisionReason> RevisionReasonList { get; set; } = new List<YarnBookingMaster_New_RevisionReason>();
        [Write(false)]
        public List<FreeConceptMRMaster> MRMasters { get; set; } = new List<FreeConceptMRMaster>();

        #endregion Additional Properties

        public FBookingAcknowledge()
        {
            IsSample = false;
            DateAdded = System.DateTime.Now;
            SLNo = "";
            BuyerID = 0;
            BuyerTeamID = 0;
            ExecutionCompanyID = 0;
            SupplierID = 0;
            StyleMasterID = 0;
            StyleNo = "";
            SubGroupID = 0;
            ExportOrderID = 0;
            BookingBy = 0;
            TextileCompanyID = 0;
            IsUnAcknowledge = false;
            RevisionDate = null;
            AckByName = "";
            UnAckByName = "";
            PageName = "";
            BOMStatus = "";
            UnAcknowledgeReason = "";
            GroupConceptNo = "";
            IsKnittingComplete = false;
            KnittingRevisionNo = 0;
            HasYarnBooking = false;
            IsBulkBooking = false;
            MerchandiserID = 0;
            FinancialYearID = 0;
            SeasonID = 0;
            PreRevisionNoLabdip = 0;
            RevisionNoLabdip = 0;
            PreRevisionNoBBKI = 0;
            YarnPreRevisionNo = 0;
            RevisionNoBBKI = 0;
            RivisionReason = "";
            IsInternalRevise = false;
            InternalReviseBy = 0;
            InternalReviseReason = "";
            BaseTypeId = (int)EnumBaseType.None;

            IsCheckByKnittingHead = false;
            CheckByKnittingHead = 0;
            IsRejectByKnittingHead = false;
            RejectByKnittingHead = 0;
            RejectReasonKnittingHead = "";

            IsApprovedByProdHead = false;
            ApprovedByProdHead = 0;
            IsRejectByProdHead = false;
            RejectByProdHead = 0;
            RejectReasonProdHead = "";

            IsApprovedByPMC = false;
            ApprovedByPMC = 0;
            IsRejectByPMC = false;
            RejectByPMC = 0;
            RejectReasonPMC = "";

            IsApprovedByAllowance = false;
            ApprovedByAllowance = 0;
            IsRejectByAllowance = false;
            RejectByAllowance = 0;
            RejectReasonAllowance = "";

            IsReject = false;
            OrderQty = 0;

            CreatedByName = "";
            StatusText = "";
            Reason = "";
            CompanyID = 0;

            IsLabdipAcknowledge = false;

            IsAddition = false;
            ParentYBookingNo = "";

            ParamTypeId = 0;
            IsRevisionValid = false;
            IsReviseBBKI = false;

            UserId = 0;
            TNACalendarDays = 0;

            LFDMasterID = 0;
            LFDChildID = 0;

            IsLabdip = false;

            IsInvalidBooking = false;
            PMCApproveBy = "";
            GmtQtyPcs = 0;
            AcknowledgeByName = "";
            UnAcknowledgeByName = "";

            CalendarDays = 0;
            OrderQtyKG = 0;

            UtilizationProposalConfirmedBy = 0;
            IsUtilizationProposalConfirmed = false;
            UtilizationProposalSendBy = 0;
            IsUtilizationProposalSend = false;

            GridStatus = EPYSLTEXCore.Infrastructure.Statics.Status.None;
            BtnId = "";

            CollarWeightInGm = 0;
            CuffWeightInGm = 0;

            CollarSizeID = "";
            CuffSizeID = "";

            IsRevisedYarn = false;
            GarmentsShipmentDate = DateTime.Now;

            YarnRequiredDate = DateTime.Now;
            FabricRequireDate = DateTime.Now;

            EWOStatusID = 0;
            OrderBankMasterID = 0;
            MenuId = 0;

            FBookingChild = new List<FBookingAcknowledgeChild>();
            FBookingChildDetails = new List<FBookingAcknowledgeChildDetails>();
            FBookingAcknowledgeChildAddProcess = new List<FBookingAcknowledgeChildAddProcess>();
            FBookingAcknowledgeChildGarmentPart = new List<FBookingAcknowledgeChildGarmentPart>();
            FBookingAcknowledgeChildProcess = new List<FBookingAcknowledgeChildProcess>();
            FBookingAcknowledgeChildText = new List<FBookingAcknowledgeChildText>();
            FBookingAcknowledgeChildDistribution = new List<FBookingAcknowledgeChildDistribution>();
            FBookingAcknowledgeChildYarnSubBrand = new List<FBookingAcknowledgeChildYarnSubBrand>();
            FBookingAcknowledgeImage = new List<FBookingAcknowledgeImage>();
            BDSDependentTNACalander = new List<BDSDependentTNACalander>();
            Childs = new List<FBookingAcknowledgeChild>();
            FreeConcepts = new List<FreeConceptMaster>();
            TechnicalNameList = new List<Select2OptionModel>();
            ColorCodeList = new List<FBookingAcknowledgeChildColor>();
            KnittingMachines = new List<KnittingMachine>();
            AllChildPlannings = new List<FBAChildPlanning>();
            FBookingAckLiabilityDistributionList = new List<FBookingAcknowledgementLiabilityDistribution>();
            FabricBookingAcknowledgeList = new List<FabricBookingAcknowledge>();
            FBookingAcknowledgeList = new List<FBookingAcknowledge>();
            YarnBookings = new List<YarnBookingMaster>();
            FBChilds = new List<FBookingAcknowledgeChild>();
            SpinnerList = new List<Select2OptionModel>();
            YChilds = new List<YarnBookingChild>();
            ChildItems = new List<YarnBookingChildItem>();
            GaugeList = new List<Select2OptionModel>();
            DiaList = new List<Select2OptionModel>();
            DeliveryChilds = new List<SFDChild>();
            DeliveryChildItems = new List<SFDChildRoll>();
            ChangesChilds = new List<FBookingAcknowledgeChild>();
            AllCollarSizeList = new List<FBookingAcknowledgeChild>();
            AllCuffSizeList = new List<FBookingAcknowledgeChild>();
            MachineBrandList = new List<KnittingMachine>();
            CollarCuffBrandList = new List<KnittingMachine>();
            FinishFabricUtilizationPopUpList = new List<BulkBookingFinishFabricUtilization>();
        }
    }
    #region Validator

    public class FBookingAcknowledgeValidator : AbstractValidator<FBookingAcknowledge>
    {
        public FBookingAcknowledgeValidator()
        {
            //RuleFor(x => x.AddedBy).GreaterThan(0).When(x => x.EntityState == EntityState.Added).WithMessage("Added by missing - FBookingAcknowledge");
            //RuleFor(x => x.UpdatedBy).GreaterThan(0).When(x => x.EntityState == EntityState.Modified).WithMessage("Updated by missing - FBookingAcknowledge");
            //RuleFor(x => x.KnittingTypeID).NotEmpty().When(x => x.KnittingTypeID == 0).WithMessage("Machine Type can not be empty!");
            //RuleFor(x => x.ConceptDate).NotEmpty();
            // RuleFor(x => x.Qty).GreaterThan(0).LessThanOrEqualTo(50);
            //// RuleFor(x => x.TechnicalNameId).NotEmpty();
            //RuleFor(x => x.GSMId).NotEmpty();
            // RuleFor(x => x.SubGroupID).NotEmpty();
            // RuleFor(x => x.MCSubClassID).NotEmpty();
            //RuleFor(x => x.ChildColors).NotEmpty().When(x => x.ConceptFor == EntityTypeConstants.CONCEPT_FOR_COLOR_BASE).WithMessage("For color base concept you must add at least one Color Item.");
            //When(x => x.ChildColors.Any(), () =>
            //{
            //    RuleForEach(x => x.ChildColors).SetValidator(new FreeConceptChildColorValidator());
            //});
        }
    }

    #endregion Validator
    public class FBookingAcknowledgeChildColor
    {
        public string ColorCode { get; set; }
        public string ColorName { get; set; }
    }
    public enum ParamTypeId
    {
        BDSAcknowledge = 0,

        BulkBookingAck = 1,

        Projection = 2,

        BulkBookingCheck = 3,
        BulkBookingApprove = 4,
        BulkBookingFinalApprove = 5,
        BulkBookingYarnAllowance = 6,

        LabdipBookingAcknowledge = 7,
        LabdipBookingAcknowledgeRnD = 8,

        AdditionalYarnBooking = 9,
        AYBQtyFinalizationPMC = 10,
        AYBProdHeadApproval = 11,
        AYBTextileHeadApproval = 12,
        AYBKnittingUtilization = 13,
        AYBKnittingHeadApproval = 14,
        AYBOperationHeadApproval = 15,

        BulkBookingUtilizationProposal = 16,
        BulkBookingUtilizationConfirmation = 17,
        YarnBookingAcknowledge = 18
    }
}
