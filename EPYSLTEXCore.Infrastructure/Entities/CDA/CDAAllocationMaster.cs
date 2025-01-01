using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAAllocationMaster")]
    public class CDAAllocationMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int AllocationID { get; set; }

        public DateTime AllocationDate { get; set; }

        public string AllocationNo { get; set; }

        public string BookingNo { get; set; }

        public int? BuyerTeamID { get; set; }

        public int? ExportOrderID { get; set; }

        public int? BuyerID { get; set; }

        public string EWONo { get; set; }

        public DateTime? FabricDeliveryDate { get; set; }

        public int? LocationID { get; set; }

        public int? TNA { get; set; }

        public bool Propose { get; set; }

        public int? ProposeBy { get; set; }

        public DateTime? ProposeDate { get; set; }

        public bool Approve { get; set; }

        public int? ApproveBy { get; set; }

        public DateTime? ApproveDate { get; set; }

        public bool Reject { get; set; }

        public int? RejectBy { get; set; }

        public DateTime? RejectDate { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        public int BAnalysisID { get; set; }

        public string BAnalysisNo { get; set; }

        #region Additional
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || AllocationID > 0;

        [Write(false)]
        public List<CDAAllocationChild> Childs { get; set; }

        [Write(false)]
        public List<Select2OptionModel> LocationList { get; set; }

        [Write(false)]
        public string Buyer { get; set; }

        [Write(false)]
        public string BuyerTeam { get; set; }

        [Write(false)]
        public string Location { get; set; }

        [Write(false)]
        public DateTime? BAnalysisDate { get; set; }


        #endregion

        public CDAAllocationMaster()
        {
            BuyerTeamID = 0;
            BuyerID = 0;
            TNA = 0;
            Propose = false;
            Approve = false;
            ApproveBy = 0;
            Reject = false;
            RejectBy = 0;
            BAnalysisID = 0;
            BAnalysisNo = "";
            Childs = new List<CDAAllocationChild>();
        }
    }

}

