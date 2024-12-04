using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory
{
    [Table(TableNames.YARN_STOCK_ADJUSTMENT_CHILD_ITEM)]
    public class YarnStockAdjustmentChildItem : DapperBaseEntity
    {
        [ExplicitKey]
        public int YSAChildItemId { get; set; } = 0;
        public int YSAChildId { get; set; } = 0;
        public int ChildRackBinID { get; set; } = 0;
        public decimal AdjustCartoon { get; set; } = 0;
        public decimal AdjustCone { get; set; } = 0;
        public decimal AdjustQtyKg { get; set; } = 0;

        #region additional props
        [Write(false)]
        public string YarnCategory { get; set; } = "";
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YSAChildItemId > 0;
        #endregion
    }
}
