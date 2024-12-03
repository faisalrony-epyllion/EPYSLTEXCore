using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_FabricItemPriceBD")]
    public class FabricItemPriceBD : DapperBaseEntity
    {
        #region Table properties
        [ExplicitKey]
        public int BDID { get; set; }
        public int FIPriceID { get; set; } = 0;
        public int YBookingID { get; set; } = 0;
        public decimal YarnCost { get; set; } = 0;
        public decimal CDACost { get; set; } = 0;
        public decimal TRMCost { get; set; } = 0;
        public decimal YDCost { get; set; } = 0;
        public decimal AOPCost { get; set; } = 0;
        public decimal FAddPCost { get; set; } = 0;
        public decimal TProcessCost { get; set; } = 0;
        public decimal ManufactureCost { get; set; } = 0;
        public decimal BuriedCost { get; set; } = 0;
        public decimal TFDirectCost { get; set; } = 0;
        public decimal TFInDirectCost { get; set; } = 0;
        public decimal TFactoryCost { get; set; } = 0;
        public decimal TOtherCost { get; set; } = 0;
        public decimal TCost { get; set; } = 0;
        public decimal MarkUp { get; set; } = 0;
        public decimal MarkUpCost { get; set; } = 0;
        public decimal FabricCost { get; set; } = 0;
        public decimal MCost { get; set; } = 0;
        public decimal Margin { get; set; } = 0;
        public decimal Adjustment { get; set; } = 0;
        public decimal AdjustmentCost { get; set; } = 0;
        public decimal QuotedFabricCost { get; set; } = 0;
        public int KTypeID { get; set; } = 0;
        public int DTypeID { get; set; } = 0;
        public int APTypeID { get; set; } = 0;
        public int APWTypeID { get; set; } = 0;
        public decimal DesignedFabricsCost { get; set; } = 0;
        public decimal LandedCost { get; set; } = 0;
        public decimal TVariableCost { get; set; } = 0;
        public decimal FixedCost { get; set; } = 0;
        public decimal TProcessCostPercent { get; set; } = 0;
        public decimal TProcessCostBeforePercent { get; set; } = 0;
        public decimal MarkUpPercent { get; set; } = 0;
        public decimal YarnCostPercent { get; set; } = 0;
        public decimal YDCostPercent { get; set; } = 0;
        public decimal YDCostAfterPercent { get; set; } = 0;
        public decimal KnittingCost { get; set; } = 0;
        public decimal KnittingCostPercent { get; set; } = 0;
        public decimal KnittingCostAfterPercent { get; set; } = 0;
        public decimal DyeingCost { get; set; } = 0;
        public decimal DyeingCostPercent { get; set; } = 0;
        public decimal DyeingCostAfterPercent { get; set; } = 0;
        public decimal FinishingCost { get; set; } = 0;
        public decimal FinishingCostPercent { get; set; } = 0;
        public decimal FinishingCostAfterPercent { get; set; } = 0;
        public decimal FAddPCostPercent { get; set; } = 0;
        public decimal FAddPCostAfterPercent { get; set; } = 0;
        public decimal YarnCostBeforePercent { get; set; } = 0;
        public bool IsTextileERP { get; set; } = true;

        public decimal YarnInKg { get; set; } = 0;
        public decimal YarnValue { get; set; } = 0;
        public decimal FBKInKg { get; set; } = 0;
        public decimal YarnRate { get; set; } = 0;
        public decimal YarnAllowance { get; set; } = 0;
        public int YBChildID { get; set; } = 0;









        #endregion Table properties

        #region Additional Columns
        [Write(false)]
        public int ConsumptionID { get; set; } = 0;
        [Write(false)]
        public int ItemMasterID { get; set; } = 0;
        [Write(false)]
        public string PercentSymbol { get; set; } = "%";
        [Write(false)]
        public string CostingHeadName { get; set; } = "";
        [Write(false)]
        public decimal Value { get; set; } = 0;

        [Write(false)]
        public decimal Cost { get; set; } = 0;

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BDID > 0;

        #endregion Additional Columns
    }
}
