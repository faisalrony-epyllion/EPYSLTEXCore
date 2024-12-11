using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YARN_ALLOCATION_MASTER)]
    public class YarnAllocationMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YarnAllocationID { get; set; }
        public string YarnAllocationNo { get; set; }
        public DateTime YarnAllocationDate { get; set; }
        public int ExportOrderID { get; set; }
        public bool Approve { get; set; }
        public int ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public bool Acknowledge { get; set; }
        public int AcknowledgeBy { get; set; }
        public DateTime? AcknowledgeDate { get; set; } = null;
        public bool UnAcknowledge { get; set; }
        public int UnAcknowledgeBy { get; set; }
        public DateTime? UnAcknowledgeDate { get; set; } = null;
        public string UnAcknowledgeReason { get; set; }
        public int AddedBy { get; set; }
        public int UpdatedBy { get; set; }
        public string BookingNo { get; set; }
        public int BuyerID { get; set; }
        public int BuyerTeamID { get; set; }
        public string EWONo { get; set; }
        public DateTime FabricDeliveryDate { get; set; }
        public int TNA { get; set; }
        public int BAnalysisID { get; set; }
        public string BAnalysisNo { get; set; }
        public bool Propose { get; set; }
        public int ProposeBy { get; set; }
        public DateTime? ProposeDate { get; set; } = null;
        public bool Reject { get; set; }
        public int RejectBy { get; set; }
        public string RejectReason { get; set; }
        public DateTime? RejectDate { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int RevisionNo { get; set; } = 0;
        public bool IsAutoGenerate { get; set; } = false;
        public int MRIRChildID { get; set; } = 0;

        #region Additional Columns

        [Write(false)]
        public DateTime? FabricBookingDate { get; set; }

        [Write(false)]
        public DateTime? ActualYarnBookingDate { get; set; }

        [Write(false)]
        public DateTime? YarnBookingDate { get; set; }

        [Write(false)]
        public DateTime? FabricsDeliveryStartDate { get; set; }

        [Write(false)]
        public DateTime? FabricsDeliveryEndDate { get; set; }

        [Write(false)]
        public DateTime? YarnInhouseStartDate { get; set; }

        [Write(false)]
        public DateTime? YarnInhouseEndDate { get; set; }

        [Write(false)]
        public DateTime? KnittingStartDate4P { get; set; }

        [Write(false)]
        public DateTime? KnittingEndDate4P { get; set; }

        [Write(false)]
        public int TNACalendarDays { get; set; }

        [Write(false)]
        public string RevisionReason { get; set; }

        [Write(false)]
        public string FabricShade { get; set; }

        [Write(false)]
        public string FabricGSM { get; set; }

        [Write(false)]
        public string YarnType { get; set; }
        [Write(false)]
        public string YarnRequisitionType { get; set; }

        [Write(false)]
        public decimal RequiredYarnQuantityKG { get; set; }

        [Write(false)]
        public decimal AllocationBalanceQTYKG { get; set; }

        [Write(false)]
        public string YDST { get; set; }
        [Write(false)]
        public int YBChildItemID { get; set; }

        [Write(false)]
        public int YBChildID { get; set; }
        [Write(false)]
        public string YBookingNo { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string BuyerTeamName { get; set; }
        [Write(false)]
        public string SeasonName { get; set; }
        [Write(false)]
        public int YarnPly { get; set; }
        [Write(false)]
        public string NumericCount { get; set; }
        [Write(false)]
        public string YarnCategory { get; set; }
        [Write(false)]
        public string YarnLotNo { get; set; }
        [Write(false)]
        public string SpinnerName { get; set; }
        [Write(false)]
        public decimal YarnReqQty { get; set; }
        [Write(false)]
        public decimal YarnStockQty { get; set; }
        [Write(false)]
        public bool IsRevise { get; set; } = false;
        [Write(false)]
        public bool YD { get; set; }
        [Write(false)]
        public string YarnCertification { get; set; }
        [Write(false)]
        public string BookingStatus { get; set; }
        [Write(false)]
        public string FabricType { get; set; }
        [Write(false)]
        public string ImagePath { get; set; } = "";
        [Write(false)]
        public decimal FabricQty { get; set; }
        [Write(false)]
        public decimal AllocationQty { get; set; }
        [Write(false)]
        public decimal StockQty { get; set; }
        [Write(false)]
        public decimal AllocatedQty { get; set; }
        [Write(false)]
        public decimal OtherAllocatedQty { get; set; } = 0;
        [Write(false)]
        public decimal YarnUtilizationQty { get; set; }
        [Write(false)]
        public decimal PipelineQty { get; set; }
        [Write(false)]
        public decimal QtyForPO { get; set; }
        [Write(false)]
        public string AllocationNo { get; set; }
        [Write(false)]
        public decimal TotalAllocatedQty { get; set; }
        [Write(false)]
        public string AllocatedBy { get; set; }
        [Write(false)]
        public int YarnStockSetId { get; set; }
        [Write(false)]
        public int AllocationChildID { get; set; }
        [Write(false)]
        public int AllocationChildItemID { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public bool IsReAllocation { get; set; }
        [Write(false)]
        public bool IsUnAckRevise { get; set; }
        [Write(false)]
        public bool IsRevisionPending { get; set; } = false;
        [Write(false)]
        public bool IsAllocationInternalRevise { get; set; }
        [Write(false)]
        public int AllocationInternalReviseBy { get; set; }
        [Write(false)]
        public DateTime? AllocationInternalReviseDate { get; set; }
        [Write(false)]
        public string AllocationInternalReviseReason { get; set; }
        [Write(false)]
        public bool IsInValidOperation { get; set; } = false;
        [Write(false)]
        public bool IsAddition { get; set; } = false;
        [Write(false)]
        public YarnPRMaster YarnPRMaster { get; set; } = new YarnPRMaster();
        [Write(false)]
        public List<YarnAllocationChild> Childs { get; set; }
        [Write(false)]
        public List<YarnAllocationChild> RevisedChilds { get; set; }
        [Write(false)]
        public List<YarnAllocationChildItem> ChildItems { get; set; }
        [Write(false)]
        public List<YarnAllocationChild> SummaryChilds { get; set; }
        [Write(false)]
        public List<Select2OptionModel> LocationList { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YarnAllocationID > 0;


        [Write(false)]
        public string FabricBookingDate_St => CommonFunction.GetDateInString(FabricBookingDate);
        [Write(false)]
        public string YarnBookingDate_St => CommonFunction.GetDateInString(YarnBookingDate);
        [Write(false)]
        public string ActualYarnBookingDate_St => CommonFunction.GetDateInString(ActualYarnBookingDate);
        [Write(false)]
        public string YarnInhouseStartDate_St => CommonFunction.GetDateInString(YarnInhouseStartDate);
        [Write(false)]
        public string YarnInhouseEndDate_St => CommonFunction.GetDateInString(YarnInhouseEndDate);
        [Write(false)]
        public string KnittingStartDate4P_St => CommonFunction.GetDateInString(KnittingStartDate4P);
        [Write(false)]
        public string KnittingEndDate4P_St => CommonFunction.GetDateInString(KnittingEndDate4P);
        [Write(false)]
        public string FabricsDeliveryStartDate_St => CommonFunction.GetDateInString(FabricsDeliveryStartDate);
        [Write(false)]
        public string FabricsDeliveryEndDate_St => CommonFunction.GetDateInString(FabricsDeliveryEndDate);

        [Write(false)]
        public int YBookingId { get; set; } = 0;
        [Write(false)]
        public int CountId { get; set; } = 0;
        [Write(false)]
        public int FabricShadeId { get; set; } = 0;
        [Write(false)]
        public int FabricTypeId { get; set; } = 0;
        [Write(false)]
        public int YarnTypeId { get; set; } = 0;
        [Write(false)]
        public int FabricGSMId { get; set; } = 0;
        [Write(false)]
        public int YItemMasterId { get; set; } = 0;

        #endregion Additional Columns

        public YarnAllocationMaster()
        {
            YarnAllocationID = 0;
            YarnAllocationNo = AppConstants.NEW;

            BuyerID = 0;
            BuyerTeamID = 0;
            TNA = 0;
            Propose = false;
            ProposeBy = 0;
            Approve = false;
            ApproveBy = 0;
            Reject = false;
            RejectBy = 0;
            Acknowledge = false;
            AcknowledgeBy = 0;
            UnAcknowledge = false;
            UnAcknowledgeBy = 0;
            UnAcknowledgeReason = "";
            DateAdded = DateTime.Now;
            FabricDeliveryDate = DateTime.Now;
            Childs = new List<YarnAllocationChild>();
            SummaryChilds = new List<YarnAllocationChild>();

            BookingNo = "";
            YBookingNo = "";
            BuyerName = "";
            BuyerTeamName = "";
            SeasonName = "";
            YarnPly = 0;
            NumericCount = "";
            YarnCategory = "";
            YarnLotNo = "";
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
            AllocationQty = 0;
            StockQty = 0;
            AllocatedQty = 0;
            YarnUtilizationQty = 0;
            PipelineQty = 0;
            QtyForPO = 0;
            AllocationNo = "";
            TotalAllocatedQty = 0;

            TNACalendarDays = 0;
            RevisionReason = "";
            FabricShade = "";
            FabricGSM = "";
            YarnType = "";
            YarnRequisitionType = "";
            RequiredYarnQuantityKG = 0;
            AllocationBalanceQTYKG = 0;
            YDST = "NO";

            YarnStockSetId = 0;
            AllocationChildID = 0; //Used only for reallocation
            AllocationChildItemID = 0; //Used only for reallocation

            YarnPRMaster = new YarnPRMaster();
        }
    }
}
