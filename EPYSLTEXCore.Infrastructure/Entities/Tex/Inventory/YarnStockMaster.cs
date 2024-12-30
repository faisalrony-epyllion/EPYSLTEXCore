using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;


namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory
{
    [Table(TableNames.YarnStockMaster)]
    public class YarnStockMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YarnStockSetId { get; set; } = 0;
        public int ItemMasterId { get; set; } = 0;
        public int SupplierId { get; set; } = 0;
        public int SpinnerId { get; set; } = 0;
        public string YarnLotNo { get; set; } = "";
        public string ShadeCode { get; set; } = "";
        public string PhysicalCount { get; set; } = "";
        public string YarnCategory { get; set; } = "";
        public DateTime? YarnApprovedDate { get; set; }

        public int YarnStockMasterId { get; set; } = 0;
        public int UnitId { get; set; } = 0;
        public decimal OpeningPipelineQty { get; set; } = 0;
        public decimal OpeningQuarantineQty { get; set; } = 0;
        public decimal OpeningIssueQty { get; set; } = 0;
        public decimal OpeningAdvanceQty { get; set; } = 0;
        public decimal OpeningAllocatedQty { get; set; } = 0;
        public decimal OpeningSampleQty { get; set; } = 0;
        public decimal OpeningLeftoverQty { get; set; } = 0;
        public decimal OpeningLiabilitiesQty { get; set; } = 0;
        public decimal OpeningUnusableQty { get; set; } = 0;
        public decimal OpeningValue { get; set; } = 0;
        public decimal CurrentReceiveQty { get; set; } = 0;
        public decimal CurrentIssueQty { get; set; } = 0;
        public decimal CurrentReturnQty { get; set; } = 0;
        public decimal CurrentBlockQty { get; set; } = 0;
        public int UpdatedBy { get; set; } = 0;
        public DateTime DateUpdated { get; set; }
        public decimal PipelineStockQty { get; set; } = 0;
        public decimal QuarantineStockQty { get; set; } = 0;
        public decimal TotalIssueQty { get; set; } = 0;
        public decimal AdvanceStockQty { get; set; } = 0;
        public decimal AllocatedStockQty { get; set; } = 0;
        public decimal SampleStockQty { get; set; } = 0;
        public decimal LeftoverStockQty { get; set; } = 0;
        public decimal LiabilitiesStockQty { get; set; } = 0;
        public decimal UnusableStockQty { get; set; } = 0;
        public decimal BlockPipelineStockQty { get; set; } = 0;
        public decimal BlockAdvanceStockQty { get; set; } = 0;
        public decimal BlockSampleStockQty { get; set; } = 0;
        public decimal BlockLeftoverStockQty { get; set; } = 0;
        public decimal BlockLiabilitiesStockQty { get; set; } = 0;
        public decimal BlockAllocatedStockQty { get; set; } = 0;
        public decimal TotalExectStockQty { get; set; } = 0;
        public decimal? TotalCurrentStockQty { get; set; } = 0;
        public decimal? TotalCurrentPipelineStockQty { get; set; } = 0;
        public DateTime? DateAdded { get; set; }
        public int AddedBy { get; set; } = 0;

        #region Additional Props
        [Write(false)]
        public bool IsPipelineRecord { get; set; } = false;
        public List<YarnStockChild> Childs { get; set; } = new List<YarnStockChild>();
        [Write(false)]
        public string Composition { get; set; } = ""; //Segment1Value
        [Write(false)]
        public string YarnType { get; set; } = ""; //Segment2Value
        [Write(false)]
        public string ManufacturingProcess { get; set; } = ""; //Segment3Value
        [Write(false)]
        public string SubProcess { get; set; } = ""; //Segment4Value
        [Write(false)]
        public string QualityParameter { get; set; } = ""; //Segment5Value
        [Write(false)]
        public string Count { get; set; } = ""; //Segment6Value

        [Write(false)]
        public string Supplier { get; set; } = "";
        [Write(false)]
        public string Spinner { get; set; } = "";
        #endregion

        public override bool IsModified => false;
    }
}
