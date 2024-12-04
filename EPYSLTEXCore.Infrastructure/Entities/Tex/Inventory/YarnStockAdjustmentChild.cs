using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory
{
    [Table(TableNames.YARN_STOCK_ADJUSTMENT_CHILD)]
    public class YarnStockAdjustmentChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int YSAChildId { get; set; } = 0;
        public int YSAMasterId { get; set; } = 0;
        public int UnitId { get; set; } = 0;
        public int StockTypeId { get; set; } = 0;
        public string ControlNo { get; set; } = "";
        public int LocationId { get; set; } = 0;
        public int AdjustmentTypeId { get; set; } = 0;
        public decimal AdjustmentQty { get; set; } = 0;

        #region additional props
        [Write(false)]
        public string YarnCategory { get; set; } = "";
        [Write(false)]
        public string UnitName { get; set; } = "";
        [Write(false)]
        public int ItemMasterID { get; set; } = 0;
        [Write(false)]
        public int Segment1ValueId { get; set; } = 0;
        [Write(false)]
        public string Segment1ValueDesc { get; set; } = "";
        [Write(false)]
        public int Segment2ValueId { get; set; } = 0;
        [Write(false)]
        public string Segment2ValueDesc { get; set; } = "";
        [Write(false)]
        public int Segment3ValueId { get; set; } = 0;
        [Write(false)]
        public string Segment3ValueDesc { get; set; } = "";
        [Write(false)]
        public int Segment4ValueId { get; set; } = 0;
        [Write(false)]
        public string Segment4ValueDesc { get; set; } = "";
        [Write(false)]
        public int Segment5ValueId { get; set; } = 0;
        [Write(false)]
        public string Segment5ValueDesc { get; set; } = "";
        [Write(false)]
        public int Segment6ValueId { get; set; } = 0;
        [Write(false)]
        public string Segment6ValueDesc { get; set; } = "";

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
        public string ShadeCode { get; set; } = "";

        [Write(false)]
        public List<YarnStockAdjustmentChildItem> ChildItems { get; set; } = new List<YarnStockAdjustmentChildItem>();
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YSAChildId > 0;
        #endregion
    }

}
