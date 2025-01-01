using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAAllocationProposalMaster")]
    public class CDAAllocationProposalMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YAPMasterID { get; set; }
        
        public string ProposalNo { get; set; }

        public DateTime ProposalDate { get; set; }

        public int AllocationID { get; set; }

        public string AllocationNo { get; set; }

        public string BookingNo { get; set; }

        public int? ExportOrderID { get; set; }

        public int? BuyerID { get; set; }

        public int? BuyerTeamID { get; set; }

        public string EWONo { get; set; }

        public DateTime? FabricDeliveryDate { get; set; }

        public int? LocationID { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        #region Additional
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YAPMasterID > 0;

        [Write(false)]
        public List<CDAAllocationProposalChild> Childs { get; set; }

        [Write(false)]
        public string Buyer { get; set; }

        [Write(false)]
        public string BuyerTeam { get; set; }

        [Write(false)]
        public string Location { get; set; }

        [Write(false)]
        public DateTime? AllocationDate { get; set; }

        [Write(false)]
        public string BAnalysisNo { get; set; }
        #endregion

        public CDAAllocationProposalMaster()
        {
            BuyerID = 0;
            BuyerTeamID = 0;
            Childs = new List<CDAAllocationProposalChild>();
        }
    }

}

