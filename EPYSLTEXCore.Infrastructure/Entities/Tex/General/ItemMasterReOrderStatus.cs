using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table(TableNames.ItemMasterReOrderStatus)]
    public class ItemMasterReOrderStatus : DapperBaseEntity
    {
        [ExplicitKey]
        public int ROSID { get; set; }
        public int ItemMasterID { get; set; } = 0;
        public int SubGroupID { get; set; } = 0;
        public int CompanyID { get; set; } = 0;
        public int CCID1 { get; set; } = 0;
        public int CCID2 { get; set; } = 0;
        public int CCID3 { get; set; } = 0;
        public int CCID4 { get; set; } = 0;
        public int CCID5 { get; set; } = 0;
        public int TSID1 { get; set; } = 0;
        public int TSID2 { get; set; } = 0;
        public int TSID3 { get; set; } = 0;
        public int TSID4 { get; set; } = 0;
        public int TSID5 { get; set; } = 0;
        public decimal MonthlyAvgConsumptionLP { get; set; } = 0;
        public decimal MonthlyAvgConsumptionFP { get; set; } = 0;
        public decimal ROLLocalPurchase { get; set; } = 0;
        public decimal ROLForeignPurchase { get; set; } = 0;
        public decimal ReOrderQty { get; set; } = 0;
        public decimal MaximumPRQtyLP { get; set; } = 0;
        public decimal MaximumPRQtyFP { get; set; } = 0;
        public decimal MOQ { get; set; } = 0;
        public DateTime? ValidDate { get; set; }
        public int UnitID { get; set; } = 0;
        public int LeadTimeDays { get; set; } = 0;
        public int SafetyStockDays { get; set; } = 0;
        public int MonthlyWorkingDays { get; set; } = 0;
        public int PackSize { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public string LeadTimeRemarks { get; set; } = "";
        public bool Propose { get; set; } = false;
        public int ProposeBy { get; set; } = 0;
        public DateTime? ProposeDate { get; set; }
        public bool Acknowledge { get; set; } = false;
        public int AcknowledgeBy { get; set; } = 0;
        public DateTime? AcknowledgeDate { get; set; }
        public bool Approve { get; set; } = false;
        public int ApproveBy { get; set; } = 0;
        public DateTime? ApproveDate { get; set; }
        public bool UnApprove { get; set; } = false;
        public int UnApproveBy { get; set; } = 0;
        public DateTime? UnApproveDate { get; set; }
        public int? ReasonID { get; set; }
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public decimal SuggestedMonthlyConsumption { get; set; } = 0;
        public int SuggestedLeadTimeDays { get; set; } = 0;
        public bool LeadTimePropose { get; set; } = false;
        public int LeadTimeProposeBy { get; set; } = 0;
        public DateTime? LeadTimeProposeDate { get; set; }
        public bool LeadTimeApprove { get; set; } = false;
        public int LeadTimeApproveBy { get; set; } = 0;
        public DateTime? LeadTimeApproveDate { get; set; }
        public bool LeadTimeUnApprove { get; set; } = false;
        public int LeadTimeUnApproveBy { get; set; } = 0;
        public DateTime? LeadTimeUnApproveDate { get; set; }
        public string FeedBackRemarks { get; set; } = "";
        public string LeadTimeFeedBackRemarks { get; set; } = "";

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ROSID > 0;
        [Write(false)]
        public string ItemName { get; set; } = "";
        [Write(false)]
        public string SubGroupName { get; set; } = "";
        [Write(false)]
        public string CompanyName { get; set; } = "";
        [Write(false)]
        public List<Select2OptionModel> ItemMasterList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> CompanyList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> SubGroupList { get; set; } = new List<Select2OptionModel>();
        #endregion
    }
}
