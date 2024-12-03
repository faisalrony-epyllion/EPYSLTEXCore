using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using FluentValidation;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_FREE_CONCEPT_MR_MASTER)]
    public class FreeConceptMRMaster : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int FCMRMasterID { get; set; }
        public DateTime ReqDate { get; set; }
        public int ConceptID { get; set; }
        public int TrialNo { get; set; }
        public string Remarks { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool HasYD { get; set; }
        public int IsBDS { get; set; }
        public int FabricID { get; set; }
        public int PreProcessRevNo { get; set; }
        public int RevisionNo { get; set; }
        public DateTime? RevisionDate { get; set; }
        public int RevisionBy { get; set; }
        public string RevisionReason { get; set; }
        public bool IsComplete { get; set; }
        public bool IsAcknowledge { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public int AcknowledgedBy { get; set; }
        public bool IsNeedRevision { get; set; } = false;
        public int ItemRevisionNo { get; set; } = 0;

        #endregion Table Properties

        #region Additional Properties
        [Write(false)]
        public List<FreeConceptMRChild> Childs { get; set; }

        [Write(false)]
        public List<BDSDependentTNACalander> BDSDependentTNACalanders { get; set; }

        [Write(false)]
        public List<FreeConceptChildColor> ChildColors { get; set; }

        [Write(false)]
        public List<FreeConceptMRMaster> FabricItems { get; set; }

        [Write(false)]
        public List<FreeConceptMRMaster> OtherItems { get; set; }

        [Write(false)]
        public List<ConceptStatus> ConceptStatusList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> DayValidDurations { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }

        [Write(false)]
        public string YBookingNo { get; set; }

        [Write(false)]
        public DateTime BookingDate { get; set; }

        [Write(false)]
        public bool HasFabric { get; set; }

        [Write(false)]
        public bool HasCollar { get; set; }

        [Write(false)]
        public bool HasCuff { get; set; }

        [Write(false)]
        public int FBAckID { get; set; }
        [Write(false)]
        public int SubGroupID { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public string BuyerDepartment { get; set; }

        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public string Name { get; set; }

        [Write(false)]
        public string MaterialRequirmentBy { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModelExtended> YarnSubProgramNews { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModelExtended> Certifications { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnShadeBooks { get; set; }

        [Write(false)]
        public IEnumerable<string> FabricComponents { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> FabricComponentsNew { get; set; }
        [Write(false)]
        public string RnDReqNo { get; set; }

        [Write(false)]
        public DateTime RnDReqDate { get; set; }

        [Write(false)]
        public int ConceptFor { get; set; }

        [Write(false)]
        public string ConceptForName { get; set; }

        [Write(false)]
        public string ItemSubGroup { get; set; }

        [Write(false)]
        public int KnittingTypeID { get; set; }

        [Write(false)]
        public int ConstructionId { get; set; }

        [Write(false)]
        public int TechnicalNameId { get; set; }

        [Write(false)]
        public int CompositionId { get; set; }
        [Write(false)]
        public int GSMId { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public DateTime ConceptDate { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string Construction { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string GSM { get; set; }

        [Write(false)]
        public string Color { get; set; }

        [Write(false)]
        public string DyeingType { get; set; }

        [Write(false)]
        public string ConceptStatus { get; set; }

        [Write(false)]
        public decimal Qty { get; set; }

        [Write(false)]
        public int BookingID { get; set; }

        [Write(false)]
        public int GroupID { get; set; }
        [Write(false)]
        public int BookingChildID { get; set; }
        [Write(false)]
        public bool IsCheckDVD { get; set; } = true;
        [Write(false)]
        public int ItemGroupID { get; set; } = 0;
        [Write(false)]
        public int ItemMasterID { get; set; } = 0;
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public int BookingUnitID { get; set; } = 0;
        [Write(false)]
        public int YarnBrandID { get; set; } = 0;
        [Write(false)]
        public int YarnTypeID { get; set; } = 0;
        [Write(false)]
        public string YarnBrand { get; set; }
        [Write(false)]
        public string PartName { get; set; }
        [Write(false)]
        public int BuyerID { get; set; } = 0;
        [Write(false)]
        public int BuyerTeamID { get; set; } = 0;
        [Write(false)]
        public int ExportOrderID { get; set; } = 0;
        [Write(false)]
        public int ConsumptionID { get; set; } = 0;
        [Write(false)]
        public int MachineTypeId { get; set; } = 0;
        [Write(false)]
        public int TechnicalNameID { get; set; } = 0;
        [Write(false)]
        public string BookingUOM { get; set; }
        [Write(false)]
        public int RequisitionQty { get; set; } = 0;
        [Write(false)]
        public string ForTechPack { get; set; }
        [Write(false)]
        public int ISourcing { get; set; } = 0;
        [Write(false)]
        public string ISourcingName { get; set; }
        [Write(false)]
        public string ContactName { get; set; }
        [Write(false)]
        public int ContactID { get; set; } = 0;
        [Write(false)]
        public int BlockBookingQty { get; set; } = 0;
        [Write(false)]
        public int AdjustQty { get; set; } = 0;
        [Write(false)]
        public int AutoAgree { get; set; } = 0;
        [Write(false)]
        public int Price { get; set; } = 0;
        [Write(false)]
        public int SuggestedPrice { get; set; } = 0;
        [Write(false)]
        public bool IsCompleteReceive { get; set; } = false;
        [Write(false)]
        public bool IsCompleteDelivery { get; set; } = false;
        [Write(false)]
        public int ColorID { get; set; } = 0;
        [Write(false)]
        public int ConstructionID { get; set; } = 0;
        [Write(false)]
        public int BookingQty { get; set; } = 0;
        [Write(false)]
        public int RefSourceID { get; set; } = 0;
        [Write(false)]
        public string RefSourceNo { get; set; }
        [Write(false)]
        public int SourceConsumptionID { get; set; } = 0;
        [Write(false)]
        public int SourceItemMasterID { get; set; } = 0;
        [Write(false)]
        public string MenuParam { get; set; }
        [Write(false)]
        public List<FabricComponentMappingSetup> FabricComponentMappingSetupList { get; set; } = new List<FabricComponentMappingSetup>();
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FCMRMasterID > 0;

        [Write(false)]
        public string GroupConceptNo { get; set; }

        [Write(false)]
        public int ConceptTypeID { get; set; }

        [Write(false)]
        public int FUPartID { get; set; }

        [Write(false)]
        public bool IsYD { get; set; }

        [Write(false)]
        public int MachineGauge { get; set; }

        [Write(false)]
        public decimal Length { get; set; }

        [Write(false)]
        public decimal Width { get; set; }

        [Write(false)]
        public string ConcepTypeName { get; set; }

        [Write(false)]
        public string FUPartName { get; set; }

        [Write(false)]
        public string MCSubClassName { get; set; }

        [Write(false)]
        public string UserName { get; set; }

        [Write(false)]
        public bool IsUsed { get; set; }

        [Write(false)]
        public bool NeedRevision { get; set; }

        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        public string MachineType { get; set; }

        [Write(false)]
        public bool IsSubContact { get; set; }

        [Write(false)]
        public DateTime? DeliveryDate { get; set; }

        [Write(false)]
        public string FabricWidth { get; set; }

        [Write(false)]
        public string YarnType { get; set; }

        [Write(false)]
        public string YarnProgram { get; set; }

        [Write(false)]
        public string YarnSubProgram { get; set; }

        [Write(false)]
        public string ReferenceSource { get; set; }

        [Write(false)]
        public string ReferenceNo { get; set; }

        [Write(false)]
        public string ColorReferenceNo { get; set; }

        [Write(false)]
        public decimal LengthYds { get; set; }

        [Write(false)]
        public decimal LengthInch { get; set; }

        [Write(false)]
        public string Instruction { get; set; }

        [Write(false)]
        public string LabDipNo { get; set; }

        [Write(false)]
        public string StyleNo { get; set; }

        [Write(false)]
        public decimal ConsumptionQty { get; set; }

        [Write(false)]
        public decimal ExcessPercentage { get; set; }

        [Write(false)]
        public decimal ExcessQty { get; set; }

        [Write(false)]
        public decimal ExcessQtyInKG { get; set; }

        [Write(false)]
        public decimal TotalQty { get; set; }

        [Write(false)]
        public decimal TotalQtyInKG { get; set; }

        [Write(false)]
        public decimal QtyInKG { get; set; }
        [Write(false)]
        public int YDProductionMasterID { get; set; }
        [Write(false)]
        public bool Modify { get; set; }
        [Write(false)]
        public bool IsOwnRevise { get; set; }
        [Write(false)]
        public string AddDate { get; set; }
        [Write(false)]
        public int FreeConceptRevisionNo { get; set; }
        [Write(false)]
        public bool IsNeedRevisionTemp { get; set; } = false;
        [Write(false)]
        public int DayValidDurationId { get; set; } = 0;
        [Write(false)]
        public int DayDuration { get; set; } = 0;
        [Write(false)]
        public string DayValidDurationName { get; set; } = "";
        [Write(false)]
        public string CollarSizeID { get; set; }
        [Write(false)]
        public decimal CollarWeightInGm { get; set; } = 0;
        [Write(false)]
        public string CuffSizeID { get; set; }
        [Write(false)]
        public decimal CuffWeightInGm { get; set; } = 0;
        [Write(false)]
        public List<Select2OptionModel> CollarSizeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> CuffSizeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<FBookingAcknowledgeChild> AllCollarSizeList { get; set; } = new List<FBookingAcknowledgeChild>();
        [Write(false)]
        public List<FBookingAcknowledgeChild> AllCuffSizeList { get; set; } = new List<FBookingAcknowledgeChild>();
        #endregion Additional Properties

        public FreeConceptMRMaster()
        {
            Childs = new List<FreeConceptMRChild>();
            BDSDependentTNACalanders = new List<BDSDependentTNACalander>();
            DateAdded = DateTime.Now;
            ReqDate = DateTime.Now;
            IsUsed = false;
            NeedRevision = false;
            FreeConceptRevisionNo = 0;
            GroupID = 0;
            SubGroupID = 0;
        }
    }

    #region Validator

    public class FreeConceptMRMasterValidator : AbstractValidator<FreeConceptMRMaster>
    {
        public FreeConceptMRMasterValidator()
        {
            //When(x => x.ChildColors.Any(), () =>
            //{
            //    RuleForEach(x => x.ChildColors).SetValidator(new FreeConceptChildColorValidator());
            //});
        }
    }

    #endregion Validator
}
