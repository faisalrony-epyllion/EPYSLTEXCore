using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAFloorRemarksMaster")]
    public class CDAFloorRemarksMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int FloorRemarksMasterID { get; set; }

        public int SupplierID { get; set; }

        public int SubGroupID { get; set; }

        public int FloorReqMasterID { get; set; }

        public int FloorIssueMasterID { get; set; }

        public int FloorReceiveMasterID { get; set; }

        public string FloorRemarksNo { get; set; }

        public int FloorRemarksBy { get; set; }

        public DateTime FloorRemarksDate { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        public int CompanyID { get; set; }

        #region Additional

        [Write(false)]
        public List<CDAFloorRemarksChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FloorRemarksMasterID > 0;

        [Write(false)]
        public string FloorRemarksdByUser { get; set; }

        [Write(false)]
        public string FloorReceiveNo { get; set; }

        [Write(false)]
        public DateTime FloorReceiveDate { get; set; }

        [Write(false)]
        public string FloorReceivedByUser { get; set; }

        [Write(false)]
        public string FloorIssueNo { get; set; }

        [Write(false)]
        public DateTime FloorIssueDate { get; set; }

        [Write(false)]
        public string FloorIssueByUser { get; set; }

        [Write(false)]
        public string FloorReqFor { get; set; }

        [Write(false)]
        public string FloorReqNo { get; set; }

        [Write(false)]
        public DateTime FloorReqDate { get; set; }

        [Write(false)]
        public int ReqQty { get; set; }

        [Write(false)]
        public int IssueQty { get; set; }

        [Write(false)]
        public int ReceiveQty { get; set; }

        #endregion Additional

        public CDAFloorRemarksMaster()
        {
            Childs = new List<CDAFloorRemarksChild>();
        }
    }
}

// </auto-generated>