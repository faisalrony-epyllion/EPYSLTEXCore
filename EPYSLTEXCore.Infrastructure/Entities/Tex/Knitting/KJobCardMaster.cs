using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.KNITTING_JOB_CARD_Master)]
    public class KJobCardMaster : DapperBaseEntity
    {
        public KJobCardMaster()
        {
            IsSubContact = false;
            ExportOrderID = 0;
            BuyerID = 0;
            BuyerTeamID = 0;
            SubGroupID = 0;
            GroupID = 0;
            BookingQty = 0m;
            KJobCardQty = 0m;
            ContactID = 0;
            MachineKnittingTypeID = 0;
            MachineDia = 0;
            Machine = "";
            DateAdded = DateTime.Now;
            KJobCardDate = DateTime.Now;
            KRolls = new List<KRollMaster>();
            Childs = new List<KJobCardChild>();
            KnittingPlanChilds = new List<KnittingPlanChild>();
            KnittingPlans = new List<KnittingPlanMaster>();
            TotalQty = 0;
            PreProcessRevNo = 0;
            RevisionNo = 0;
            RevisionBy = 0;
            RevisionReason = "";
            IsRevision = false;
            ColorName = "";
        }
        #region Table Properties

        [ExplicitKey]
        public int KJobCardMasterID { get; set; }
        public string KJobCardNo { get; set; }
        public DateTime KJobCardDate { get; set; }
        public int KPChildID { get; set; }
        public int BAnalysisChildID { get; set; }
        public int ItemMasterID { get; set; }
        public bool IsSubContact { get; set; }
        public int ContactID { get; set; }
        public int MachineKnittingTypeID { get; set; }
        public int KnittingMachineID { get; set; }
        public int MachineGauge { get; set; }
        public int MachineDia { get; set; }
        public int BookingID { get; set; }
        public int ExportOrderID { get; set; }
        public int BuyerID { get; set; }
        public int BuyerTeamID { get; set; }
        public int SubGroupID { get; set; }
        public int UnitID { get; set; }
        public decimal BookingQty { get; set; }
        public decimal KJobCardQty { get; set; }
        public decimal ProdQty { get; set; }
        public int ProdQtyPcs { get; set; }
        public int ConceptID { get; set; }
        public string Remarks { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool ProdComplete { get; set; }
        public bool QualityComplete { get; set; }
        public int Width { get; set; }
        public int BrandID { get; set; }
        public bool Active { get; set; }
        public bool GrayFabricOK { get; set; }
        public int GrayGSM { get; set; }
        public int GrayWidth { get; set; }
        public int ProductionStatusId { get; set; }
        public bool NeedPreFinishingProcess { get; set; }
        public bool ColorWayDesignOk { get; set; }
        public int ActualTotalNeedle { get; set; }
        public int ActualTotalCourse { get; set; }
        public decimal ActualGreyHeight { get; set; }
        public decimal ActualGreyLength { get; set; }
        public decimal ActualNeedle { get; set; }
        public int ActualCPI { get; set; }
        public int Status { get; set; }
        public string Reason { get; set; }
        public int GroupID { get; set; }

        public string ColorName { get; set; }
        public int PreProcessRevNo { get; set; }
        public int RevisionNo { get; set; }
        public DateTime? RevisionDate { get; set; }
        public int RevisionBy { get; set; }
        public string RevisionReason { get; set; }

        #endregion Table Properties

        #region Additonal Fields

        [Write(false)]
        public List<KJobCardChild> Childs { get; set; }
        [Write(false)]
        public List<KRollMaster> KRolls { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public int TotalQty { get; set; }
        [Write(false)]
        public string Brand { get; set; }
        [Write(false)]
        public string Machine { get; set; }

        [Write(false)]
        public int MCSubClassID { get; set; }

        [Write(false)]
        public string MCSubClassName { get; set; }

        [Write(false)]
        public string Contact { get; set; }

        [Write(false)]
        public string KnittingMachineNo { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }

        [Write(false)]
        public DateTime BookingDate { get; set; }

        [Write(false)]
        public string SubGroup { get; set; }

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public int ConstructionID { get; set; }

        [Write(false)]
        public int CompositionID { get; set; }

        [Write(false)]
        public int FabricColorID { get; set; }

        [Write(false)]
        public decimal FabricGsm { get; set; }

        [Write(false)]
        public int FabricWidth { get; set; }

        [Write(false)]
        public string GSM { get; set; }
        [Write(false)]
        public int KnittingTypeID { get; set; }
        [Write(false)]
        public string MCSubClass { get; set; }
        [Write(false)]
        public int DyeingTypeID { get; set; }

        [Write(false)]
        public string FabricConstruction { get; set; }

        [Write(false)]
        public string FabricComposition { get; set; }
        [Write(false)]
        public string Composition { get; set; }
        [Write(false)]
        public string Size { get; set; }
        [Write(false)]
        public string DyeingType { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string MachineKnittingType { get; set; }

        [Write(false)]
        public string EWONo { get; set; }

        [Write(false)]
        public string Buyer { get; set; }

        [Write(false)]
        public string BuyerTeam { get; set; }

        [Write(false)]
        public decimal QCPassQty { get; set; }

        [Write(false)]
        public decimal QCFailQty { get; set; }

        [Write(false)]
        public string StatusInText { get; set; }

        [Write(false)]
        public DateTime ConceptDate { get; set; }

        [Write(false)]
        public string SubGroupName { get; set; }

        [Write(false)]
        public string MachineType { get; set; }

        [Write(false)]
        public string PlanNo { get; set; }

        [Write(false)]
        public int IsBDS { get; set; }

        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public bool IsRevision { get; set; }
        [Write(false)]
        public override bool IsModified => KJobCardMasterID > 0 || EntityState == System.Data.Entity.EntityState.Modified;

        [Write(false)]
        public IEnumerable<Select2OptionModel> OperatorList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ShiftList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> StatusList { get; set; }
        [Write(false)]
        public List<KnittingMachine> KnittingMachines { get; set; }
        [Write(false)]
        public List<KnittingPlanChild> KnittingPlanChilds { get; set; }
        [Write(false)]
        public List<KnittingPlanMaster> KnittingPlans { get; set; }
        [Write(false)]
        public List<KJobCardChild> KJChilds { get; set; }
        [Write(false)]
        public bool IsConfirm { get; set; }

        #endregion Additonal Fields
    }
    #region Validator
    //public class KJobCardMasterValidator : AbstractValidator<KJobCardMaster>
    //{
    //    public KJobCardMasterValidator()
    //    {
    //        RuleFor(x => x.KJobCardNo).NotEmpty();
    //        RuleFor(x => x.KJobCardDate).NotEmpty();
    //        RuleFor(x => x.ContactID).NotEmpty().WithMessage("Contact is required.");
    //        RuleFor(x => x.MachineKnittingTypeID).NotEmpty();
    //        RuleFor(x => x.MachineGauge).NotEmpty();
    //        RuleFor(x => x.MachineDia).NotEmpty();
    //        RuleFor(x => x.SubGroupID).NotEmpty();
    //        RuleFor(x => x.UnitID).NotEmpty();
    //        RuleFor(x => x.BookingQty).NotEmpty().WithMessage("Please enter Quantity");
    //        RuleFor(x => x.KJobCardQty).NotEmpty();
    //    }
    //}
    #endregion
}
