using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;


namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory
{
    //[Table(TableNames.YarnStockChild)]
    public class YarnStockChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int YarnStockChildId { get; set; } = 0;
        public int YarnStockSetId { get; set; } = 0;
        public int LocationId { get; set; } = 0;
        public int BuyerId { get; set; } = 0;
        public int CompanyId { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public int TransectionTypeId { get; set; } = 0;
        public int UnitId { get; set; } = 0;
        public int MOTId { get; set; } = 0;
        public int ImportCategoryId { get; set; } = 0;
        public int StockTypeId { get; set; } = 0;
        public int RackID { get; set; } = 0;
        public decimal Qty { get; set; } = 0;
        public decimal Cone { get; set; } = 0;
        public decimal Cartoon { get; set; } = 0;
        public string TransectionDate { get; set; } = ""; //it's a string property
        public int ParentYarnStockChildId { get; set; } = 0;
        public bool IsInactive { get; set; } = false;
        public int StockFromTableId { get; set; } = 0;
        public int StockFromPKId { get; set; } = 0;
        public decimal BlockAdvanceStockQty { get; set; } = 0;
        public decimal BlockSampleStockQty { get; set; } = 0;
        public decimal BlockLiabilitiesStockQty { get; set; } = 0;
        public decimal BlockLeftoverStockQty { get; set; } = 0;
        public decimal BlockPipelineStockQty { get; set; } = 0;
        public decimal BlockAllocatedStockQty { get; set; } = 0;
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; }
        public string Note { get; set; } = "";
        public bool IsForOpening { get; set; } = false;
        public bool IsAdjustedQty { get; set; } = false;
        public bool IsAvoidForExactQty { get; set; } = false;
        public bool IsUnblock { get; set; } = false;

        public override bool IsModified => false;

        #region Additional Props
        [Write(false)]
        public string StockFromMenu { get; set; } = "";
        [Write(false)]
        public string StockFromMasterType { get; set; } = "";
        [Write(false)]
        public string StockFromMasterNo { get; set; } = "";
        [Write(false)]
        public string Location { get; set; } = "";
        [Write(false)]
        public string Buyer { get; set; } = "";
        [Write(false)]
        public string Company { get; set; } = "";
        [Write(false)]
        public string TransectionTypeName { get; set; } = "";
        [Write(false)]
        public string StockTypeName { get; set; } = "";
        [Write(false)]
        public string RackNo { get; set; } = "";
        [Write(false)]
        public string TransectionByName { get; set; } = "";
        #endregion
    }
}
