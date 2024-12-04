using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_FREE_CONCEPT_MASTER)]
    public class FreeConceptMaster : DapperBaseEntity
    {
        public List<Select2OptionModel> FormList;

        #region Table Properties

        [ExplicitKey]
        public int ConceptID { get; set; }

        public string ConceptNo { get; set; }

        public DateTime ConceptDate { get; set; }

        public int TrialNo { get; set; }

        public DateTime? TrialDate { get; set; }

        public int ConceptFor { get; set; }

        public int MCSubClassID { get; set; }

        public int KnittingTypeID { get; set; }

        public int SubGroupID { get; set; }

        public int ConstructionId { get; set; }

        public int TechnicalNameId { get; set; }

        public int CompositionId { get; set; }

        public int GSMId { get; set; }

        public decimal Qty { get; set; }
        public decimal QtyInKG { get; set; }

        public int ConceptStatusId { get; set; }

        public string Remarks { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        public bool ProdStart { get; set; }
        public bool ProdComplete { get; set; }
        public bool RevisionPending { get; set; }
        public DateTime? RevisionPendingDate { get; set; }
        public int CompanyID { get; set; }
        public bool Active { get; set; }
        public int IsBDS { get; set; }
        public int ItemMasterID { get; set; }
        public string GroupConceptNo { get; set; }
        public int ConceptTypeID { get; set; }
        public int FUPartID { get; set; }
        public int? ConsumptionID { get; set; }
        public bool IsYD { get; set; }
        public int MachineGauge { get; set; }
        public int MachineDia { get; set; }
        public int BrandID { get; set; }
        public decimal Length { get; set; }

        public decimal Width { get; set; }

        public int BookingID { get; set; }

        public int BookingChildID { get; set; }
        public int PreProcessRevNo { get; set; }

        public int RevisionNo { get; set; }

        public DateTime? RevisionDate { get; set; }

        public int RevisionBy { get; set; }

        public string RevisionReason { get; set; }
        public bool DeliveryComplete { get; set; }
        public string StatusRemarks { get; set; }

        public int ExportOrderID { get; set; }
        public int BuyerID { get; set; }
        public int BuyerTeamID { get; set; }
        public decimal ExcessPercentage { get; set; }
        public decimal ExcessQty { get; set; }
        public decimal ExcessQtyInKG { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalQtyInKG { get; set; }
        public bool IsActive { get; set; }
        public decimal ProduceKnittingQty { get; set; }

        #endregion Table Properties

        #region Additional Properties

        [Write(false)]
        public List<FreeConceptChildColor> ChildColors { get; set; }

        [Write(false)]
        public List<FreeConceptSet> FreeConceptSets { get; set; }
        [Write(false)]
        public List<FreeConceptMaster> FabricItems { get; set; }

        [Write(false)]
        public List<FreeConceptMaster> OtherItems { get; set; }

        [Write(false)]
        public List<ConceptStatus> ConceptStatusList { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string ConcepTypeName { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }

        //[Write(false)]
        //public int ColorId { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public int ColorID { get; set; }

        [Write(false)]
        public string Color { get; set; }

        [Write(false)]
        public string ItemSubGroup { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string Construction { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string SubClassName { get; set; }

        [Write(false)]
        public string GSM { get; set; }

        [Write(false)]
        public string ConceptStatus { get; set; }

        [Write(false)]
        public string ConceptForName { get; set; }

        [Write(false)]
        public string UserName { get; set; }

        [Write(false)]
        public string FUPartName { get; set; }

        [Write(false)]
        public string MCSubClassName { get; set; }

        [Write(false)]
        public decimal BookingQty { get; set; }

        [Write(false)]
        public string LiveStatus { get; set; }

        [Write(false)]
        public string RevisionPendingInString => RevisionPending ? "Need Revision" : "Running";

        [Write(false)]
        public IEnumerable<Select2OptionModel> KnittingTypeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> SubGroupList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ConstructionList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompositionList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> TechnicalNameList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> MCSubClassList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> GSMList { get; set; }

        [Write(false)]
        public IEnumerable<string> FabricComponents { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> MachineGaugeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> OtherMCSubClassList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> OtherTechnicalNameList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ConceptID > 0;

        [Write(false)]
        public IEnumerable<Select2OptionModel> FabricUsedPartList { get; set; }
        [Write(false)]
        public bool NeedRevision { get; set; }
        [Write(false)]
        public string grpConceptNo { get; set; }
        [Write(false)]
        public int conceptType { get; set; }
        [Write(false)]
        public List<FreeConceptMRMaster> MRList { get; set; }
        #endregion Additional Properties

        public FreeConceptMaster()
        {
            ConceptID = 0;
            CompanyID = CompnayIDConstants.EFL;
            ConceptFor = 1093;
            ConceptNo = AppConstants.NEW;
            GroupConceptNo = AppConstants.NEW;
            ConsumptionID = 0;
            ChildColors = new List<FreeConceptChildColor>();
            FreeConceptSets = new List<FreeConceptSet>();
            ConceptStatusList = new List<ConceptStatus>();
            MRList = new List<FreeConceptMRMaster>();
            FabricItems = new List<FreeConceptMaster>();
            OtherItems = new List<FreeConceptMaster>();
            DateAdded = DateTime.Now;
            ConceptDate = DateTime.Now;
            IsBDS = 0;
            ItemMasterID = 0;
            MCSubClassID = 0;
            NeedRevision = false;
            IsActive = false;
            LiveStatus = "";
            StatusRemarks = "";
            ExportOrderID = 0;
            BuyerID = 0;
            BuyerTeamID = 0;
            ExcessPercentage = 0;
            ExcessQty = 0;
            ExcessQtyInKG = 0;
            TotalQty = 0;
            TotalQtyInKG = 0;
            MachineGauge = 0;
            MachineDia = 0;
            BrandID = 0;
            ProduceKnittingQty = 0;
        }

        public static implicit operator FreeConceptMaster(FinishFabricStockForm v)
        {
            throw new NotImplementedException();
        }
    }

    #region Validator

    //public class FreeConceptMasterValidator : AbstractValidator<FreeConceptMaster>
    //{
    //    public FreeConceptMasterValidator()
    //    {
    //        //RuleFor(x => x.AddedBy).NotEmpty().When(x => x.AddedBy == 0).WithMessage("Added by missing - FreeConceptMaster");
    //        //RuleFor(x => x.UpdatedBy).NotEmpty().When(x => x.UpdatedBy == 0 && x.EntityState==System.Data.Entity.EntityState.Modified).WithMessage("Updated by missing - FreeConceptMaster");
    //        //RuleFor(x => x.KnittingTypeID).NotEmpty().When(x => x.KnittingTypeID == 0).WithMessage("Machine Type can not be empty!");
    //        //RuleFor(x => x.ConceptDate).NotEmpty();
    //        // RuleFor(x => x.Qty).GreaterThan(0).LessThanOrEqualTo(50);
    //        //// RuleFor(x => x.TechnicalNameId).NotEmpty();
    //        //RuleFor(x => x.GSMId).NotEmpty();
    //        // RuleFor(x => x.SubGroupID).NotEmpty();
    //        // RuleFor(x => x.MCSubClassID).NotEmpty();
    //        //RuleFor(x => x.ChildColors).NotEmpty().When(x => x.ConceptFor == EntityTypeConstants.CONCEPT_FOR_COLOR_BASE).WithMessage("For color base concept you must add at least one Color Item.");
    //        //When(x => x.ChildColors.Any(), () =>
    //        //{
    //        //    RuleForEach(x => x.ChildColors).SetValidator(new FreeConceptChildColorValidator());
    //        //});
    //    }
    //}

    #endregion Validator
}
