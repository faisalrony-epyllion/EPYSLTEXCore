using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.Knitting_Plan_Master)]
    public class KnittingPlanMaster : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int KPMasterID { get; set; } = 0;

        public int BAnalysisID { get; set; } = 0;

        public DateTime? ReqDeliveryDate { get; set; }

        public string BookingNo { get; set; } = "";

        public string YBookingNo { get; set; } = "";

        public int BuyerID { get; set; } = 0;

        public int BuyerTeamID { get; set; } = 0;

        public int CompanyID { get; set; } = 0;

        public int ExportOrderID { get; set; } = 0;

        public int MerchandiserTeamID { get; set; } = 0;

        public string StyleNo { get; set; } = "";

        public int SeasonID { get; set; } = 0;

        public int ContactID { get; set; } = 0;

        public bool Active { get; set; } = true;

        public int AddedBy { get; set; } = 0;

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public int UpdatedBy { get; set; } = 0;

        public DateTime? DateUpdated { get; set; }

        public int Status { get; set; } = 0;

        public int ConceptID { get; set; } = 0;

        public bool RevisionPending { get; set; } = false;

        public DateTime? RevisionPendingDate { get; set; }

        public int PlanNo { get; set; } = 0;
        public bool GrayFabricOK { get; set; } = false;
        public bool ColorWayDesignOk { get; set; } = false;
        public int ActualTotalNeedle { get; set; } = 0;
        public int ActualTotalCourse { get; set; } = 0;
        public decimal ActualGreyHeight { get; set; } = 0;
        public decimal ActualGreyLength { get; set; } = 0;
        public decimal ActualNeedle { get; set; } = 0;
        public int ActualCPI { get; set; } = 0;
        public int ProductionStatusId { get; set; } = 0;

        public bool IsConfirm { get; set; } = false;

        public string ConfirmationRemarks { get; set; } = "";

        public bool NeedPreFinishingProcess { get; set; } = false;

        public int GrayGSM { get; set; } = 0;

        public int GrayWidth { get; set; } = 0;

        public decimal PlanQty { get; set; } = 0;

        public int MCSubClassID { get; set; } = 0;

        public int PreProcessRevNo { get; set; } = 0;

        public int RevisionNo { get; set; } = 0;

        public DateTime? RevisionDate { get; set; }

        public int RevisionBy { get; set; } = 0;

        public string RevisionReason { get; set; } = "";

        public int IsBDS { get; set; } = 0;

        public string FilePath { get; set; } = "";

        public string AttachmentPreviewTemplate { get; set; } = "";

        public bool IsSubContact { get; set; } = false;

        #endregion Table Properties

        #region Additional Properties

        [Write(false)]
        public decimal TotalQty { get; set; } = 0;

        [Write(false)]
        public List<KnittingPlanChild> Childs { get; set; } = new List<KnittingPlanChild>();

        [Write(false)]
        public List<KnittingPlanYarn> Yarns { get; set; } = new List<KnittingPlanYarn>();

        [Write(false)]
        public string ConceptNo { get; set; } = "";

        [Write(false)]
        public DateTime? ConceptDate { get; set; }

        [Write(false)]
        public int SubGroupID { get; set; } = 0;

        [Write(false)]
        public int BookingChildID { get; set; } = 0;

        [Write(false)]
        public int BookingID { get; set; } = 0;

        [Write(false)]
        public int ConsumptionID { get; set; } = 0;

        [Write(false)]
        public string SubGroupName { get; set; } = "";

        [Write(false)]
        public int ConstructionID { get; set; } = 0;

        [Write(false)]
        public int TechnicalNameId { get; set; } = 0;

        [Write(false)]
        public int CompositionID { get; set; } = 0;

        [Write(false)]
        public int GSMID { get; set; } = 0;

        [Write(false)]
        public decimal Qty { get; set; } = 0;

        [Write(false)]
        public decimal BookingQty { get; set; } = 0;

        [Write(false)]
        public string Remarks { get; set; } = "";

        [Write(false)]
        public string KnittingType { get; set; } = "";

        [Write(false)]
        public string Composition { get; set; } = "";

        [Write(false)]
        public string Construction { get; set; } = "";

        [Write(false)]
        public string TechnicalName { get; set; } = "";

        [Write(false)]
        public string ColorName { get; set; } = "";

        [Write(false)]
        public string GSM { get; set; } = "";

        [Write(false)]
        public string ConceptStatus { get; set; } = "";

        [Write(false)]
        public string ConceptForName { get; set; } = "";

        [Write(false)]
        public string BAnalysisNo { get; set; } = "";

        [Write(false)]
        public DateTime? BAnalysisDate { get; set; }

        [Write(false)]
        public string Buyer { get; set; } = "";

        [Write(false)]
        public string BuyerTeam { get; set; } = "";

        [Write(false)]
        public string Company { get; set; } = "";

        [Write(false)]
        public string EWONo { get; set; } = "";
        [Write(false)]
        public string Uom { get; set; } = "";
        [Write(false)]
        public string MerchandiserTeam { get; set; } = "";

        [Write(false)]
        public string SeasonName { get; set; } = "";

        [Write(false)]
        public string AddedByUser { get; set; } = "";

        [Write(false)]
        public string UpdatedByUser { get; set; } = "";

        [Write(false)]
        public DateTime? BookingDate { get; set; }

        [Write(false)]
        public int ApprovedBy { get; set; } = 0;

        [Write(false)]
        public DateTime? ApprovedDate { get; set; }

        [Write(false)]
        public string ApprovedByUser { get; set; } = "";

        [Write(false)]
        public string StatusDesc { get; set; } = "";

        [Write(false)]
        public string CompletionStatus { get; set; } = "";

        [Write(false)]
        public decimal TotalPlanedQty { get; set; } = 0;
        [Write(false)]
        public decimal ProduceKnittingQty { get; set; } = 0;

        [Write(false)]
        public int ConceptTypeID { get; set; } = 0;

        [Write(false)]
        public int FUPartID { get; set; } = 0;

        [Write(false)]
        public string FUPartName { get; set; } = "";

        [Write(false)]
        public bool IsYD { get; set; } = false;
        [Write(false)]
        public bool IsRevise { get; set; } = false;

        [Write(false)]
        public int MachineGauge { get; set; } = 0;
        [Write(false)]
        public int MachineDia { get; set; } = 0;
        [Write(false)]
        public int BrandID { get; set; } = 0;
        [Write(false)]
        public string Brand { get; set; } = "";
        [Write(false)]
        public DateTime? StartDate { get; set; }
        [Write(false)]
        public DateTime? EndDate { get; set; }

        [Write(false)]
        public decimal Length { get; set; } = 0;

        [Write(false)]
        public decimal Width { get; set; } = 0;

        [Write(false)]
        public int ItemMasterID { get; set; } = 0;

        [Write(false)]
        public string RevisionPendingStatus => RevisionPending ? "Need Revision" : "New";

        [Write(false)]
        public string MCSubClass { get; set; } = "";

        [Write(false)]
        public string Size { get; set; } = "";

        [Write(false)]
        public int KnittingTypeID { get; set; } = 0;

        [Write(false)]
        public int ProcessTime { get; set; } = 0;

        [Write(false)]
        public string GroupConceptNo { get; set; } = "";

        [Write(false)]
        public decimal MaxQty { get; set; } = 0;
        [Write(false)]
        public decimal RemainingPlanQty { get; set; } = 0;
        [Write(false)]
        public KnittingProgramType KnittingProgramType { get; set; } = new KnittingProgramType();

        [Write(false)]
        public IEnumerable<Select2OptionModel> MCSubClassList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> OtherMCSubClassList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> KnittingTypeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> MachineTypeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ProductionStatusList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ColorList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnBrandList { get; set; }

        [Write(false)]
        public List<FreeConceptChildColor> ChildColors { get; set; } = new List<FreeConceptChildColor>();

        [Write(false)]
        public List<KnittingMachine> KnittingMachines { get; set; } = new List<KnittingMachine>();
        [Write(false)]
        public List<KnittingMachine> KnittingSubContracts { get; set; } = new List<KnittingMachine>();
        [Write(false)]
        public KnittingPlanGroup KPGroup { get; set; } = new KnittingPlanGroup();
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || KPMasterID > 0;

        [Write(false)]
        public string UserName { get; set; } = "";

        [Write(false)]
        public string KJobCardNo { get; set; } = "";

        [Write(false)]
        public bool WithoutOB { get; set; } = false;
        [Write(false)]
        public string BuyerName { get; set; } = "";
        [Write(false)]
        public string BuyerTeamName { get; set; } = "";
        [Write(false)]
        public int TotalNeedle { get; set; } = 0;
        [Write(false)]
        public int TotalCourse { get; set; } = 0;
        [Write(false)]
        public string UsesIn { get; set; } = "";
        [Write(false)]
        public string Contact { get; set; } = "";

        #endregion Additional Properties
    }
    #region Validator

    //public class KnittingPlanMasterValidator : AbstractValidator<KnittingPlanMaster>
    //{
    //    public KnittingPlanMasterValidator()
    //    {
    //        RuleFor(x => x.ReqDeliveryDate).NotEmpty();
    //        //RuleFor(x => x.Childs).Must(list => list.Count == 0).WithMessage("You must add at least one Child Item.");
    //        //RuleFor(x => x.Yarns).Must(list => list.Count == 0).WithMessage("You must add at least one Yarn Item.");
    //        //When(x => x.Childs.Any(), () =>
    //        //{
    //        //    RuleForEach(x => x.Childs).SetValidator(new KnittingPlanChildValidator());
    //        //});
    //    }
    //}

    #endregion Validator
}
