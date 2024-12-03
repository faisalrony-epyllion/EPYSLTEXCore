using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using FluentValidation;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.YarnBookingChild_New)]
    public class YarnBookingChild : BaseItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int YBChildID { get; set; }
        public int YBookingID { get; set; }
        public int ConsumptionID { get; set; }
        public int YarnTypeID { get; set; }
        public int YarnBrandID { get; set; }
        public int FUPartID { get; set; }
        public int BookingUnitID { get; set; }
        public int BookingChildID { get; set; }
        public decimal BookingQty { get; set; }
        public string FTechnicalName { get; set; }
        public bool IsCompleteReceive { get; set; }
        public bool IsCompleteDelivery { get; set; }
        public DateTime? LastDCDate { get; set; }
        public string ClosingRemarks { get; set; }

        public decimal ExcessPercentage { get; set; }
        public decimal ExcessQty { get; set; }
        public decimal ExcessQtyInKG { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalQtyInKG { get; set; }

        public decimal GreyReqQty { get; set; }
        public decimal GreyLeftOverQty { get; set; }
        public decimal GreyProdQty { get; set; }

        public bool IsForFabric { get; set; }
        public decimal YarnAllowance { get; set; }
        public decimal FinishFabricUtilizationQty { get; set; }
        public decimal ReqFinishFabricQty { get; set; }

        #region Additional Columns 

        [Write(false)]
        public List<BulkBookingFinishFabricUtilization> FinishFabricUtilizationPopUpList { get; set; }

        [Write(false)]
        public List<FBookingAcknowledgeChildGFUtilization> GreyFabricUtilizationPopUpList { get; set; }

        [Write(false)]
        public List<FBookingAcknowledgeChildReplacement> AdditionalReplacementPOPUPList { get; set; } = new List<FBookingAcknowledgeChildReplacement>();

        public decimal QtyInKG { get; set; }
        [Write(false)]
        public string Construction { get; set; }
        [Write(false)]
        public string Composition { get; set; }
        [Write(false)]
        public string Color { get; set; }
        [Write(false)]
        public string GSM { get; set; }
        [Write(false)]
        public string Width { get; set; }
        [Write(false)]
        public string Length { get; set; }
        [Write(false)]
        public string KnittingType { get; set; }
        [Write(false)]

        public string YarnType { get; set; }
        [Write(false)]
        public string DyeingType { get; set; }
        [Write(false)]
        public string BodyMeasurement { get; set; }
        [Write(false)]
        public int YBChildGroupID { get; set; }

        [Write(false)]
        public int BookingID { get; set; }

        [Write(false)]
        public int ConsumptionChildID { get; set; }

        [Write(false)]
        public int BOMMasterID { get; set; } = 0;

        [Write(false)]
        public int ExportOrderID { get; set; }

        [Write(false)]
        public int ItemGroupID { get; set; }

        //[Write(false)]
        //public int SubGroupId { get; set; } 

        [Write(false)]
        public string SubGroupName { get; set; }

        [Write(false)]
        public int OrderBankPOID { get; set; }

        [Write(false)]
        public int ColorID { get; set; }

        [Write(false)]
        public int SizeID { get; set; }

        [Write(false)]
        public int TechPackID { get; set; }

        [Write(false)]
        public int ExecutionCompanyID { get; set; }

        [Write(false)]
        public decimal ConsumptionQty { get; set; }

        [Write(false)]
        public decimal RequisitionQty { get; set; }

        [Write(false)]
        public string Remarks { get; set; }

        [Write(false)]
        public string ForGarmentColors { get; set; }

        [Write(false)]
        public string ForGarmentSizes { get; set; }

        [Write(false)]
        public string BookingUOM { get; set; }

        [Write(false)]
        public int LengthYds { get; set; }

        [Write(false)]
        public int LengthInch { get; set; }

        [Write(false)]
        public int A1ValueID { get; set; }

        [Write(false)]
        public string A1Desc { get; set; }

        [Write(false)]
        public string YarnBrand { get; set; }

        [Write(false)]
        public string PartName { get; set; }

        [Write(false)]
        public string ForTechPack { get; set; }

        [Write(false)]
        public bool ISourcing { get; set; }

        [Write(false)]
        public string ISourcingName { get; set; }

        [Write(false)]
        public string ContactName { get; set; }

        [Write(false)]
        public int ContactID { get; set; }

        [Write(false)]
        public string LabDipNo { get; set; }

        [Write(false)]
        public decimal BlockBookingQty { get; set; }

        [Write(false)]
        public decimal AdjustQty { get; set; }

        [Write(false)]
        public bool AutoAgree { get; set; }
        [Write(false)]
        public new int UnitID { get; set; }

        [Write(false)]
        public decimal Price { get; set; }

        [Write(false)]
        public decimal SuggestedPrice { get; set; }

        [Write(false)]
        public DateTime LabdipUpdateDate { get; set; }

        [Write(false)]
        public int ToItemMasterID { get; set; }
        [Write(false)]
        public string FUPart { get; set; }
        [Write(false)]
        public int[] FUPartIDs { get; set; }
        [Write(false)]
        public string YarnSubBrandID { get; set; }
        [Write(false)]
        public string YarnSubBrandName { get; set; }
        [Write(false)]
        public int[] YarnSubBrandIDs { get; set; }
        [Write(false)]
        public int KnittingTypeID { get; set; }
        [Write(false)]
        public int CompositionID { get; set; }
        [Write(false)]
        public int ConstructionID { get; set; }
        [Write(false)]
        public int GSMID { get; set; }
        [Write(false)]
        public int MachineTypeId { get; set; }
        [Write(false)]
        public string MachineType { get; set; }
        [Write(false)]
        public int TechnicalNameId { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public string YarnProgram { get; set; }
        [Write(false)]
        public string FabricWidth { get; set; }
        [Write(false)]
        public int StitchLength { get; set; }
        [Write(false)]
        public int BrandID { get; set; }
        [Write(false)]
        public string Brand { get; set; }
        [Write(false)]
        public bool IsNewObj { get; set; }
        [Write(false)]
        public string Instruction { get; set; }
        [Write(false)]
        public int Segment1ValueID { get; set; }
        [Write(false)]
        public int Segment2ValueID { get; set; }
        [Write(false)]
        public int Segment3ValueID { get; set; }
        [Write(false)]
        public int Segment4ValueID { get; set; }
        [Write(false)]
        public int Segment5ValueID { get; set; }
        [Write(false)]
        public int Segment6ValueID { get; set; }
        [Write(false)]
        public int Segment7ValueID { get; set; }
        [Write(false)]
        public int Segment8ValueID { get; set; }
        [Write(false)]
        public string Segment1ValueDesc { get; set; }
        [Write(false)]
        public string Segment2ValueDesc { get; set; }
        [Write(false)]
        public string Segment3ValueDesc { get; set; }
        [Write(false)]
        public string Segment4ValueDesc { get; set; }
        [Write(false)]
        public string Segment5ValueDesc { get; set; }
        [Write(false)]
        public string Segment6ValueDesc { get; set; } //Count Ne
        [Write(false)]
        public string Segment7ValueDesc { get; set; }
        [Write(false)]
        public string Segment8ValueDesc { get; set; }
        [Write(false)]
        public decimal ExistingYarnAllowance { get; set; } = 0;
        [Write(false)]
        public decimal TotalYarnAllowance { get; set; } = 0;
        [Write(false)]
        public List<YarnBookingChildItem> ChildItems { get; set; }
        [Write(false)]
        public List<YarnBookingChildItemRevision> ChildItemsRevision { get; set; } = new List<YarnBookingChildItemRevision>();
        [Write(false)]
        public List<YarnBookingChildItem> ChildItemsGroup { get; set; }
        [Write(false)]
        public List<YarnBookingChildYarnSubBrand> yarnBookingChildYarnSubBrand { get; set; }
        [Write(false)]
        public List<YarnBookingChildGarmentPart> yarnBookingChildGarmentPart { get; set; }
        [Write(false)]
        public List<FBookingAcknowledgeChild> CriteriaNames { get; set; }
        [Write(false)]
        public List<FBAChildPlanning> FBAChildPlannings { get; set; }
        [Write(false)]
        public List<FBAChildPlanning> FBAChildPlanningsWithIds { get; set; }
        [Write(false)]
        public List<Select2OptionModel> YarnShadeBooks { get; set; }
        [Write(false)]
        public List<Select2OptionModel> YarnSubBrandList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> Spinners { get; set; }
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }
        [Write(false)]
        public int RefSourceID { get; set; }
        [Write(false)]
        public string RefSourceNo { get; set; }
        [Write(false)]
        public int SourceConsumptionID { get; set; }
        [Write(false)]
        public int SourceItemMasterID { get; set; }
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public decimal BookingQtyKG { get; set; }

        #endregion Additional Columns
        public YarnBookingChild()
        {

            YarnSubBrandID = "";
            BookingQty = 0m;
            IsCompleteReceive = false;
            IsCompleteDelivery = false;
            ChildItems = new List<YarnBookingChildItem>();
            ChildItemsGroup = new List<YarnBookingChildItem>();
            yarnBookingChildYarnSubBrand = new List<YarnBookingChildYarnSubBrand>();
            yarnBookingChildGarmentPart = new List<YarnBookingChildGarmentPart>();
            YarnShadeBooks = new List<Select2OptionModel>();
            YarnSubBrandList = new List<Select2OptionModel>();
            Spinners = new List<Select2OptionModel>();
            CriteriaNames = new List<FBookingAcknowledgeChild>();
            FBAChildPlannings = new List<FBAChildPlanning>();
            FBAChildPlanningsWithIds = new List<FBAChildPlanning>();
            FinishFabricUtilizationPopUpList = new List<BulkBookingFinishFabricUtilization>();
            GreyFabricUtilizationPopUpList = new List<FBookingAcknowledgeChildGFUtilization>();
            BookingQty = 0m;
            IsCompleteReceive = false;
            IsCompleteDelivery = false;
            SubGroupId = 102;
            EntityState = EntityState.Added;
            MachineType = "Empty";
            TechnicalName = "Empty";
            StitchLength = 0;
            BrandID = 0;
            Brand = "";
            ConsumptionID = 0;
            YarnTypeID = 0;
            YarnBrandID = 0;
            FUPartID = 0;
            GreyReqQty = 0;
            GreyLeftOverQty = 0;
            GreyProdQty = 0;
            IsForFabric = false;

            RefSourceID = 0;
            RefSourceNo = "";
            SourceConsumptionID = 0;
            SourceItemMasterID = 0;
            ToItemMasterID = 0;
            IsNewObj = false;

            YarnAllowance = 0;
            FinishFabricUtilizationQty = 0;
            ReqFinishFabricQty = 0;

            Instruction = "";
            BookingQtyKG = 0;
        }
    }
    public class YarnBookingChildValidator : AbstractValidator<YarnBookingChild>
    {
        public YarnBookingChildValidator()
        {
            RuleFor(x => x.BookingQty).NotEmpty().WithMessage("Booking Qty is required.");
        }
    }
}
