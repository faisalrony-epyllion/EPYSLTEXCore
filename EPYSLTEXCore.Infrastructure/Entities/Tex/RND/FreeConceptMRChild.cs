using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_FREE_CONCEPT_MR_CHILD)]
    public class FreeConceptMRChild : YarnItemMaster, IDapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int FCMRChildID { get; set; } = 0;
        public int FCMRMasterID { get; set; } = 0;
        public string YarnCategory { get; set; } = "";
        public bool YD { get; set; } = false;
        public decimal ReqQty { get; set; } = 0;
        public decimal ReqCone { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public bool IsPR { get; set; } = false;
        public string ShadeCode { get; set; } = "";
        public bool Acknowledge { get; set; } = false;
        public int AcknowledgeBy { get; set; } = 0;
        public DateTime? AcknowledgeDate { get; set; }
        public bool Reject { get; set; } = false;
        public int RejectBy { get; set; } = 0;
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; } = "";

        public decimal Distribution { get; set; } = 0;
        public decimal BookingQty { get; set; } = 0;
        public decimal Allowance { get; set; } = 0;
        public bool YDItem { get; set; } = false;
        public int YBChildItemID { get; set; } = 0;
        public int YarnStockSetId { get; set; } = 0;
        public int DayValidDurationId { get; set; } = 0;
        public bool IsInActive { get; set; } = false;

        #endregion Table Properties

        #region Additional Properties
        [Write(false)]
        public int ConceptID { get; set; } = 0;
        [Write(false)]
        public decimal StockQty { get; set; } = 0;
        [Write(false)]
        public int YDProductionMasterID { get; set; } = 0;
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public int TotalRows { get; set; }
        [Write(false)]
        public decimal TotalQty { get; set; } = 0;
        [Write(false)]
        public string DisplayUnitDesc { get; set; } = "";
        [Write(false)]
        public string BookingNo { get; set; } = "";
        [Write(false)]
        public string YarnProgram { get; set; } = "";
        [Write(false)]
        public string YarnSubProgram { get; set; } = "";
        [Write(false)]
        public decimal ExcessPercentage { get; set; } = 0;
        [Write(false)]
        public decimal ExcessQty { get; set; } = 0;
        [Write(false)]
        public decimal ExcessQtyInKG { get; set; } = 0;
        [Write(false)]
        public decimal TotalQtyInKG { get; set; } = 0;
        [Write(false)]
        public string CFValue { get; set; } = "";
        [Write(false)]
        public int FinalCount { get; set; } = 0;
        [Write(false)]
        public int FeederRepeat { get; set; } = 0;
        [Write(false)]
        public int TotalFeeder { get; set; } = 0;
        [Write(false)]
        public int ConsumptionPer { get; set; } = 0;

        [Write(false)]
        public int SupplierId { get; set; } = 0;
        [Write(false)]
        public int SpinnerId { get; set; } = 0;
        [Write(false)]
        public string PhysicalCount { get; set; } = "";
        [Write(false)]
        public string YarnLotNo { get; set; } = "";
        [Write(false)]
        public string SpinnerName { get; set; } = "";
        [Write(false)]
        public decimal SampleStockQty { get; set; } = 0;
        [Write(false)]
        public decimal AdvanceStockQty { get; set; } = 0;
        [Write(false)]
        public int BookingChildID { get; set; } = 0;
        [Write(false)]
        public int SubGroupId { get; set; } = 0;

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        #endregion Additional Properties

        public FreeConceptMRChild()
        {
            BookingNo = "";
            DisplayUnitDesc = "";
            EntityState = EntityState.Added;
            YarnProgram = "";
            YarnSubProgram = "";
            YarnCategory = "";
            YBChildItemID = 0;
        }
    }

    #region Validator

    //public class FreeConceptMRChildValidator : AbstractValidator<FreeConceptMRChild>
    //{
    //    public FreeConceptMRChildValidator()
    //    {
    //        RuleFor(x => x.ReqQty).GreaterThan(0);
    //        RuleFor(x => x.ReqCone).GreaterThan(0);
    //    }
    //}

    #endregion Validator
}
