using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.Knitting_Plan_Child)]
    public class KnittingPlanChild : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int KPChildID { get; set; } = 0;
        public int KPMasterID { get; set; } = 0;
        public int BAnalysisChildID { get; set; } = 0;
        public int ItemMasterID { get; set; } = 0;
        public int MachineGauge { get; set; } = 0;
        public int MachineDia { get; set; } = 0;
        public int YBookingID { get; set; } = 0;
        public int SubGroupID { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ActualStartDate { get; set; }
        public DateTime ActualEndDate { get; set; }
        public int UnitID { get; set; } = 0;
        public decimal BookingQty { get; set; } = 0;
        public decimal KJobCardQty { get; set; } = 0;
        public int CCColorID { get; set; } = 0;
        public int FabricGsm { get; set; } = 0;
        public int FabricWidth { get; set; } = 0;
        public int MCSubClassID { get; set; } = 0;
        public int KnittingTypeID { get; set; }
        public string Remarks { get; set; } = "";
        public decimal Needle { get; set; } = 0;
        public int CPI { get; set; } = 0;
        public int TotalNeedle { get; set; } = 0;
        public int TotalCourse { get; set; } = 0;
        public int PlanNo { get; set; } = 0;

        #endregion Table Properties

        #region Additional Fields

        [Write(false)]
        public List<KJobCardMaster> KJobCardMasters { get; set; }

        [Write(false)]
        public string MCSubClassName { get; set; } = "";

        [Write(false)]
        public string BookingNo { get; set; } = "";

        [Write(false)]
        public DateTime? BookingDate { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; } = "";

        [Write(false)]
        public string SubGroup { get; set; } = "";

        [Write(false)]
        public DateTime? ConceptDate { get; set; }

        [Write(false)]
        public string Uom { get; set; } = "";

        [Write(false)]
        public int BAnalysisID { get; set; } = 0;

        [Write(false)]
        public int ConstructionID { get; set; } = 0;

        [Write(false)]
        public int TechnicalNameId { get; set; } = 0;

        [Write(false)]
        public int CompositionID { get; set; } = 0;

        [Write(false)]
        public int FabricColorID { get; set; } = 0;

        [Write(false)]
        public string KnittingType { get; set; } = "";

        [Write(false)]
        public int DyeingTypeID { get; set; } = 0;
        [Write(false)]
        public string FabricConstruction { get; set; } = "";
        [Write(false)]
        public string FabricComposition { get; set; } = "";
        [Write(false)]
        public string DyeingType { get; set; } = "";
        [Write(false)]
        public int ExportOrderID { get; set; } = 0;
        [Write(false)]
        public int BuyerID { get; set; } = 0;
        [Write(false)]
        public int BuyerTeamID { get; set; } = 0;
        [Write(false)]
        public string TechnicalName { get; set; } = "";
        [Write(false)]
        public string ItemName { get; set; } = "";
        [Write(false)]
        public string Contact { get; set; } = "";
        [Write(false)]
        public string KnittingMachineNo { get; set; } = "";
        [Write(false)]
        public string Brand { get; set; } = "";
        [Write(false)]
        public string KJobCardNo { get; set; } = "";
        [Write(false)]
        public string FUPartName { get; set; } = "";
        [Write(false)]
        public decimal Length { get; set; } = 0;
        [Write(false)]
        public decimal Width { get; set; } = 0;
        [Write(false)]
        public IEnumerable<Select2OptionModel> MachineKnittingTypes { get; set; }

        [Write(false)]
        public bool IsSubContact { get; set; } = false;

        [Write(false)]
        public int BrandID { get; set; } = 0;
        [Write(false)]
        public int KnittingMachineID { get; set; } = 0;
        [Write(false)]
        public int ContactID { get; set; } = 0;
        [Write(false)]
        public decimal Rate { get; set; } = 0;
        [Write(false)]
        public int KJobCardMasterID { get; set; } = 0;
        [Write(false)]
        public decimal ProdQty { get; set; } = 0;
        [Write(false)]
        public int ProdQtyPcs { get; set; } = 0;
        [Write(false)]
        public string Composition { get; set; } = "";
        [Write(false)]
        public string ColorName { get; set; } = "";
        [Write(false)]
        public string GSM { get; set; } = "";
        [Write(false)]
        public string Size { get; set; } = "";
        [Write(false)]
        public decimal MaxQty { get; set; } = 0;
        [Write(false)]
        public decimal MaxQtyKg { get; set; } = 0;
        [Write(false)]
        public decimal MaxQtyPcs { get; set; } = 0;
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || KPChildID > 0;

        #endregion Additional Fields
    }
    #region Validator

    //public class KnittingPlanChildValidator : AbstractValidator<KnittingPlanChild>
    //{
    //    public KnittingPlanChildValidator()
    //    {
    //        RuleFor(x => x.UnitID).NotEmpty().WithMessage("Please select unit!");
    //        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Please select start date!");
    //        RuleFor(x => x.EndDate).NotEmpty().WithMessage("Please select end date!");
    //        RuleFor(x => x.BookingQty).NotEmpty().WithMessage("Please enter Quantity");
    //        RuleFor(x => x.MCSubClassID).NotEmpty().WithMessage("Please select sub-class!");
    //        RuleFor(x => x.MachineGauge).NotEmpty().WithMessage("Please select Machine Gauge!");
    //        RuleFor(x => x.BrandID).NotEmpty().When(x => x.IsSubContact == false).WithMessage("Please select brand");
    //        RuleFor(x => x.KnittingMachineID).NotEmpty().When(x => x.IsSubContact == false).WithMessage("Please select Machine");
    //        RuleFor(x => x.ContactID).NotEmpty().WithMessage("Please select Floor/Sub-Contractor!");
    //        RuleFor(x => x.MachineDia).NotEmpty().WithMessage("Please select Machine Dia!");
    //    }
    //}

    #endregion Validator
}
