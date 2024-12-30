using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory
{
    [Table(TableNames.YARN_STOCK_ADJUSTMENT_MASTER)]
    public class YarnStockAdjustmentMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YSAMasterId { get; set; } = 0;
        public int YarnStockSetId { get; set; } = 0;
        public string AdjustmentNo { get; set; } = AppConstants.NEW;
        public string ReferanceNo { get; set; } = "";
        public DateTime AdjustmentDate { get; set; }
        public string ManualTransectionNo { get; set; } = "";
        public int AdjustmentReasonId { get; set; } = 0;
        public int AddedBy { get; set; } = 0;
        public DateTime AddedDate { get; set; }
        public int UpdatedBy { get; set; } = 0;
        public DateTime? UpdatedDate { get; set; }
        public bool IsSendForApproval { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public int ApprovedBy { get; set; } = 0;
        public DateTime? ApprovedDate { get; set; }
        public bool IsReject { get; set; } = false;
        public int RejectedBy { get; set; } = 0;
        public DateTime? RejectedDate { get; set; }
        public string RejectedReason { get; set; } = "";

        #region additional props
        [Write(false)]
        public bool IsPipelineRecord { get; set; } = false;
        [Write(false)]
        public string YarnCategory { get; set; } = "";
        [Write(false)]
        public string Reason { get; set; } = "";
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
        public List<Select2OptionModel> SupplierList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> SpinnerList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> ShadeCodes { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> AdjustmentReasonList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> AdjustmentTypeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<YarnStockAdjustmentChild> Childs { get; set; } = new List<YarnStockAdjustmentChild>();
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YSAMasterId > 0;

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
        public int SupplierId { get; set; } = 0;
        [Write(false)]
        public int SpinnerId { get; set; } = 0;
        [Write(false)]
        public string SupplierName { get; set; } = "";
        [Write(false)]
        public string SpinnerName { get; set; } = "";
        [Write(false)]
        public string PhysicalCount { get; set; } = "";
        [Write(false)]
        public string YarnLotNo { get; set; } = "";
        [Write(false)]
        public string ShadeCode { get; set; } = "";

        [Write(false)]
        public bool IsInvalidItem { get; set; } = false;
        [Write(false)]
        public string InvalidItem_St => this.IsInvalidItem ? "Invalid item" : "Valid item";
        [Write(false)]
        public string Note { get; set; } = "";


        [Write(false)]
        public decimal PipelineStockQty { get; set; } = 0;
        [Write(false)]
        public decimal QuarantineStockQty { get; set; } = 0;
        [Write(false)]
        public decimal TotalIssueQty { get; set; } = 0;
        [Write(false)]
        public decimal AdvanceStockQty { get; set; } = 0;
        [Write(false)]
        public decimal AllocatedStockQty { get; set; } = 0;
        [Write(false)]
        public decimal SampleStockQty { get; set; } = 0;
        [Write(false)]
        public decimal LeftoverStockQty { get; set; } = 0;
        [Write(false)]
        public decimal LiabilitiesStockQty { get; set; } = 0;
        [Write(false)]
        public decimal UnusableStockQty { get; set; } = 0;

        [Write(false)]
        public decimal BlockPipelineStockQty { get; set; } = 0;
        [Write(false)]
        public decimal BlockAdvanceStockQty { get; set; } = 0;
        [Write(false)]
        public decimal BlockSampleStockQty { get; set; } = 0;
        [Write(false)]
        public decimal BlockLeftoverStockQty { get; set; } = 0;
        [Write(false)]
        public decimal BlockLiabilitiesStockQty { get; set; } = 0;
        [Write(false)]
        public decimal BlockAllocatedStockQty { get; set; } = 0;


        [Write(false)]
        public decimal PipelineStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal QuarantineStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal TotalIssueQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal AdvanceStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal AllocatedStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal SampleStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal LeftoverStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal LiabilitiesStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal UnusableStockQtyNew { get; set; } = 0;

        [Write(false)]
        public decimal BlockPipelineStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal BlockAdvanceStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal BlockSampleStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal BlockLeftoverStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal BlockLiabilitiesStockQtyNew { get; set; } = 0;
        [Write(false)]
        public decimal BlockAllocatedStockQtyNew { get; set; } = 0;

        [Write(false)]
        public string PipelineStockQtySt => CommonFunction.GetNumberDisplayValue(this.PipelineStockQty);
        [Write(false)]
        public string QuarantineStockQtySt => CommonFunction.GetNumberDisplayValue(this.QuarantineStockQty);
        [Write(false)]
        public string TotalIssueQtySt => CommonFunction.GetNumberDisplayValue(this.TotalIssueQty);
        [Write(false)]
        public string AdvanceStockQtySt => CommonFunction.GetNumberDisplayValue(this.AdvanceStockQty);
        [Write(false)]
        public string AllocatedStockQtySt => CommonFunction.GetNumberDisplayValue(this.AllocatedStockQty);
        [Write(false)]
        public string SampleStockQtySt => CommonFunction.GetNumberDisplayValue(this.SampleStockQty);
        [Write(false)]
        public string LeftoverStockQtySt => CommonFunction.GetNumberDisplayValue(this.LeftoverStockQty);
        [Write(false)]
        public string LiabilitiesStockQtySt => CommonFunction.GetNumberDisplayValue(this.LiabilitiesStockQty);
        [Write(false)]
        public string UnusableStockQtySt => CommonFunction.GetNumberDisplayValue(this.UnusableStockQty);

        [Write(false)]
        public string BlockPipelineStockQtySt => CommonFunction.GetNumberDisplayValue(this.BlockPipelineStockQty);
        [Write(false)]
        public string BlockAdvanceStockQtySt => CommonFunction.GetNumberDisplayValue(this.BlockAdvanceStockQty);
        [Write(false)]
        public string BlockSampleStockQtySt => CommonFunction.GetNumberDisplayValue(this.BlockSampleStockQty);
        [Write(false)]
        public string BlockLeftoverStockQtySt => CommonFunction.GetNumberDisplayValue(this.BlockLeftoverStockQty);
        [Write(false)]
        public string BlockLiabilitiesStockQtySt => CommonFunction.GetNumberDisplayValue(this.BlockLiabilitiesStockQty);
        [Write(false)]
        public string BlockAllocatedStockQtySt => CommonFunction.GetNumberDisplayValue(this.BlockAllocatedStockQty);
        #endregion
    }

}
