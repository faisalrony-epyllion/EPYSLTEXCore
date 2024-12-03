using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table(TableNames.YARN_ALLOCATION_CHILD_ITEM)]
    public class YarnAllocationChildItem : IDapperBaseEntity
    {
        [ExplicitKey]
        public int AllocationChildItemID { get; set; }
        public int AllocationChildID { get; set; } = 0;
        public string YarnCategory { get; set; } = "";
        public int YarnStockSetId { get; set; } = 0;
        public decimal QuarantineAllocationQty { get; set; } = 0;
        public decimal AdvanceAllocationQty { get; set; } = 0;
        public decimal SampleAllocationQty { get; set; } = 0;
        public decimal LiabilitiesAllocationQty { get; set; } = 0;
        public decimal LeftoverAllocationQty { get; set; } = 0;
        public decimal TotalAllocationQty { get; set; } = 0;
        public bool Acknowledge { get; set; } = false;
        public int AcknowledgeBy { get; set; } = 0;
        public DateTime? AcknowledgeDate { get; set; }
        public bool UnAcknowledge { get; set; } = false;
        public int UnAcknowledgeBy { get; set; } = 0;
        public DateTime? UnAcknowledgeDate { get; set; }
        public string UnAcknowledgeReason { get; set; } = "";
        public string Remarks { get; set; } = "";
        public bool IsReAllocation { get; set; } = false;
        public int ReAllocationNo { get; set; } = 0;
        public int ReAllocationBy { get; set; } = 0;
        public DateTime? ReAllocationDate { get; set; }
        public bool IsUnAckRevise { get; set; } = false;
        public int UnAckReviseNo { get; set; } = 0;
        public int UnAckReviseBy { get; set; } = 0;
        public DateTime? UnAckReviseDate { get; set; }
        public int PreProcessRevNo { get; set; } = 0;
        public int RevisionNo { get; set; } = 0;
        public bool IsRevised { get; set; } = false;
        public int RevisionBy { get; set; } = 0;
        public DateTime? RevisionDate { get; set; }
        public int OperationType { get; set; } = (int)EnumOperationTypes.None;

        #region Additional Columns

        [Write(false)]
        public string NumericCount { get; set; } = "";
        [Write(false)]
        public string Spinner { get; set; } = "";
        [Write(false)]
        public string PhysicalCount { get; set; } = "";
        [Write(false)]
        public string PhysicalLot { get; set; } = "";
        [Write(false)]
        public int YarnAge { get; set; } = 0;
        [Write(false)]
        public decimal POPrice { get; set; } = 0;
        [Write(false)]
        public string TestResult { get; set; } = "";
        [Write(false)]
        public string TestResultComments { get; set; } = "";
        [Write(false)]
        public decimal AdvanceStockQty { get; set; } = 0;
        [Write(false)]
        public decimal SampleStockQty { get; set; } = 0;
        [Write(false)]
        public decimal LeftoverStockQty { get; set; } = 0;
        [Write(false)]
        public decimal LiabilitiesStockQty { get; set; } = 0;
        [Write(false)]
        public string BookingNo { get; set; } = "";
        [Write(false)]
        public string YBookingNo { get; set; } = "";
        [Write(false)]
        public string ShadeCode { get; set; } = "";
        [Write(false)]
        public string AllocatedBy { get; set; } = "";
        [Write(false)]
        public string BuyerName { get; set; } = "";
        [Write(false)]
        public string FabricType { get; set; } = "";
        [Write(false)]
        public string LotRef { get; set; } = "";
        [Write(false)]
        public int ItemMasterId { get; set; } = 0;
        [Write(false)]
        public int YItemMasterId { get; set; } = 0;
        [Write(false)]
        public decimal NetYarnReqQty { get; set; } = 0;
        [Write(false)]
        public decimal QtyForPO { get; set; } = 0;
        [Write(false)]
        public decimal PipelineAllocationQty { get; set; } = 0;
        [Write(false)]
        public string Status { get; set; } = "";
        [Write(false)]
        public bool IsDifferentItem { get; set; } = false;
        [Write(false)]
        public string YarnAllocationNo { get; set; } = "";
        [Write(false)]
        public string YarnCount { get; set; } = "";
        [Write(false)]
        public string ImagePath { get; set; } = "";
        [Write(false)]
        public string ReqCount { get; set; } = "";
        [Write(false)]
        public string ReqYarnDetails { get; set; } = "";
        [Write(false)]
        public string AllocatedYarnDetails { get; set; } = "";
        [Write(false)]
        public string AllocatedCount { get; set; } = "";
        [Write(false)]
        public string FabricColor { get; set; } = "";
        [Write(false)]
        public decimal AllocatedQty { get; set; } = 0;
        [Write(false)]
        public DateTime? ApproveDate { get; set; } = null;
        [Write(false)]
        public bool IsAllocationInternalRevise { get; set; } = false;
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;
        [Write(false)]
        public int TotalRows { get; set; }


        [Write(false)]
        public int ExportOrderID { get; set; } = 0;
        [Write(false)]
        public string ExportOrderNo { get; set; } = "";
        [Write(false)]
        public int EventID { get; set; } = 0;
        [Write(false)]
        public int CDays { get; set; } = 0;
        [Write(false)]
        public int TNADays { get; set; } = 0;
        [Write(false)]
        public DateTime? EventDate { get; set; } = null;
        [Write(false)]
        public int EventDescriptionId { get; set; } = 0;
        [Write(false)]
        public decimal StockQty { get; set; } = 0;
        [Write(false)]
        public int YBChildItemID { get; set; } = 0;

        [Write(false)]
        public int BuyerId { get; set; } = 0;
        [Write(false)]
        public int BuyerTeamId { get; set; } = 0;
        [Write(false)]
        public int YBookingId { get; set; } = 0;
        [Write(false)]
        public int CountId { get; set; } = 0;
        [Write(false)]
        public int FabricShadeId { get; set; } = 0;
        [Write(false)]
        public string FabricShade { get; set; } = "";
        [Write(false)]
        public int FabricTypeId { get; set; } = 0;
        [Write(false)]
        public int YarnTypeId { get; set; } = 0;
        [Write(false)]
        public string YarnType { get; set; } = "";
        [Write(false)]
        public string YarnRequisitionType { get; set; } = "";
        [Write(false)]
        public int FabricGSMId { get; set; } = 0;
        [Write(false)]
        public string FabricGSM { get; set; } = "";

        [Write(false)]
        public string BuyerTeamName { get; set; } = "";
        [Write(false)]
        public int CountItem { get; set; } = 0;

        #endregion Additional Columns

        public YarnAllocationChildItem()
        {
            EntityState = EntityState.Added;
        }
    }
}
