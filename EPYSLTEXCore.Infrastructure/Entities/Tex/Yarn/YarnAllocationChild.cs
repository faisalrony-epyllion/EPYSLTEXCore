using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table(TableNames.YARN_ALLOCATION_CHILD)]
    public class YarnAllocationChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int AllocationChildID { get; set; }
        public int AllocationID { get; set; } = 0;
        public int ItemMasterID { get; set; } = 0;
        public int YBChildItemID { get; set; } = 0;
        public string YBookingNo { get; set; } = "";
        public int CCID1 { get; set; } = 0;
        public int CCID2 { get; set; } = 0;
        public int CCID3 { get; set; } = 0;
        public int CCID4 { get; set; } = 0;
        public int CCID5 { get; set; } = 0;
        public int RCCID1 { get; set; } = 0;
        public int RCCID2 { get; set; } = 0;
        public int RCCID3 { get; set; } = 0;
        public int RCCID4 { get; set; } = 0;
        public int RCCID5 { get; set; } = 0;
        public int UnitID { get; set; } = 0;
        public decimal RelativeFactor { get; set; } = 0;
        public int SUnitID { get; set; } = 0;
        public string LotNo { get; set; } = "";
        public decimal ReqQty { get; set; } = 0;
        /*public decimal ApproveStockQty { get; set; }
        public decimal DiagnosticStockQty { get; set; }
        public decimal PipelineStockQty { get; set; }*/
        public decimal AdvanceStockAllocationQty { get; set; } = 0;
        public decimal PipelineStockAllocationQty { get; set; } = 0;
        public decimal QtyForPO { get; set; } = 0;
        public DateTime? InhouseStartDate { get; set; }
        public DateTime? InhouseEndDate { get; set; }
        public decimal TotalAllocationQty { get; set; } = 0;
        /*public decimal PRQty { get; set; }
        public bool IsPR { get; set; }*/

        public int PreRevisionNo { get; set; } = 0;
        public int RevisionNo { get; set; } = 0;
        public int RevisionBy { get; set; } = 0;
        public DateTime? RevisionDate { get; set; }
        public decimal QtyFromAutoGenerate { get; set; } = 0;

        #region Additional Columns    
        [Write(false)]
        public string BookingNo { get; set; }
        [Write(false)]
        public int CompanyId { get; set; } = 0;
        [Write(false)]
        public string ShadeCode { get; set; }
        [Write(false)]
        public int YBChildID { get; set; } = 0;
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string BuyerTeamName { get; set; }
        [Write(false)]
        public string SeasonName { get; set; }
        [Write(false)]
        public int YarnPly { get; set; } = 0;
        [Write(false)]
        public int NumericCount { get; set; } = 0;
        [Write(false)]
        public string YarnCategory { get; set; }
        [Write(false)]
        public string SpinnerName { get; set; }
        [Write(false)]
        public decimal YarnReqQty { get; set; } = 0;
        [Write(false)]
        public decimal YarnStockQty { get; set; } = 0;
        [Write(false)]
        public bool YD { get; set; } = false;
        [Write(false)]
        public string YarnCertification { get; set; } = "";
        [Write(false)]
        public string BookingStatus { get; set; } = "";
        [Write(false)]
        public string FabricType { get; set; } = "";
        [Write(false)]
        public decimal FabricQty { get; set; } = 0;
        [Write(false)]
        public string Location { get; set; } = "";
        [Write(false)]
        public string Rack { get; set; } = "";
        [Write(false)]
        public decimal StockQty { get; set; } = 0;
        [Write(false)]
        public int StockCone { get; set; } = 0;
        [Write(false)]
        public int StockQtyCarton { get; set; } = 0;
        [Write(false)]
        public decimal IssueQty { get; set; } = 0;
        [Write(false)]
        public int IssueCone { get; set; } = 0;
        [Write(false)]
        public int IssueQtyCarton { get; set; } = 0;
        [Write(false)]
        public decimal AllocatedQty { get; set; } = 0;
        [Write(false)]
        public decimal PlannedPOQty { get; set; } = 0;
        [Write(false)]
        public decimal PlannedPipelineQty { get; set; } = 0;
        [Write(false)]
        public decimal YarnUtilizationQty { get; set; } = 0;
        [Write(false)]
        public decimal Allowance { get; set; } = 0;
        [Write(false)]
        public decimal YDAllowance { get; set; } = 0;
        [Write(false)]
        public decimal NetYarnReqQty { get; set; } = 0;
        [Write(false)]
        public string PhysicalCount { get; set; }
        [Write(false)]
        public int UpdatedBy { get; set; } = 0;
        [Write(false)]
        public decimal PrevNetYarnReqQty { get; set; } = 0;
        [Write(false)]
        public decimal TotalAllocatedQty { get; set; } = 0;
        [Write(false)]
        public decimal PendingAllocationQty { get; set; } = 0;
        [Write(false)]
        public decimal BalancePOQty { get; set; } = 0;
        [Write(false)]
        public int BookingChildID { get; set; } = 0;
        [Write(false)]
        public DateTime? YarnReqDate { get; set; }
        [Write(false)]
        public string YarnCount { get; set; } = "";
        [Write(false)]
        public string FabricShade { get; set; } = "";
        [Write(false)]
        public int FCMRChildID { get; set; } = 0;
        [Write(false)]
        public int ConceptID { get; set; } = 0;
        [Write(false)]
        public int SpinnerId { get; set; } = 0;
        [Write(false)]
        public List<YarnAllocationChildItem> ChildItems { get; set; }
        [Write(false)]
        public List<YarnAllocationChildPipelineItem> ChildPipelineItems { get; set; }
        [Write(false)]
        public List<BulkBookingGreyYarnUtilization> GreyYarnUtilizationPopUpList { get; set; } = new List<BulkBookingGreyYarnUtilization>();
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;
        [Write(false)]
        public int TotalRows { get; set; }

        #endregion Additional Columns


        public YarnAllocationChild()
        {
            EntityState = EntityState.Added;

            YBookingNo = "";
            BuyerName = "";
            BuyerTeamName = "";
            SeasonName = "";
            YarnPly = 0;
            NumericCount = 0;
            YarnCategory = "";
            SpinnerName = "";
            YarnReqQty = 0;
            YarnStockQty = 0;
            YD = false;
            YarnCertification = "";
            BookingStatus = "";
            FabricType = "";
            FabricQty = 0;
            YBChildItemID = 0;
            YBChildID = 0;
            ItemMasterID = 0;
            AdvanceStockAllocationQty = 0;
            PipelineStockAllocationQty = 0;
            TotalAllocationQty = 0;
            AllocationID = 0;
            CompanyId = 0;
            ShadeCode = "";
            BookingNo = "";
            UpdatedBy = 0;
            ChildItems = new List<YarnAllocationChildItem>();
            ChildPipelineItems = new List<YarnAllocationChildPipelineItem>();
        }
    }
}
