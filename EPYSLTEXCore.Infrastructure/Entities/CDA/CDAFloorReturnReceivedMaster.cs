using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAFloorReturnReceivedMaster")]
    public class CDAFloorReturnReceivedMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int FloorReturnReceivedMasterID { get; set; }

        public int SubGroupID { get; set; }

        public int SupplierID { get; set; }

        public DateTime FloorReturnReceivedDate { get; set; }

        public string FloorReturnReceivedNo { get; set; }

        public int FloorReturnMasterID { get; set; }

        public int FloorRemarksMasterID { get; set; }

        public bool IsApprove { get; set; }

        public DateTime? ApproveDate { get; set; }

        public int? ApproveBy { get; set; }

        public bool IsAcknowledge { get; set; }

        public DateTime? AcknowledgeDate { get; set; }

        public int? AcknowledgeBy { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        public int CompanyID { get; set; }

        #region Additional

        [Write(false)]
        public List<CDAFloorReturnReceivedChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FloorReturnReceivedMasterID > 0;

        [Write(false)]
        public string FloorReturnNo { get; set; }

        [Write(false)]
        public int FloorReturnBy { get; set; }

        [Write(false)]
        public DateTime FloorReturnDate { get; set; }

        [Write(false)]
        public string FloorReqFor { get; set; }

        [Write(false)]
        public DateTime FloorReceiveDate { get; set; }

        [Write(false)]
        public string FloorReceiveNo { get; set; }

        [Write(false)]
        public int FloorReqMasterID { get; set; }

        [Write(false)]
        public string FloorReqNo { get; set; }

        [Write(false)]
        public DateTime FloorReqDate { get; set; }

        [Write(false)]
        public int FloorReqQty { get; set; }

        [Write(false)]
        public int FloorReturnQty { get; set; }

        [Write(false)]
        public string FloorReturnByUser { get; set; }

        [Write(false)]
        public int ReceiveQty { get; set; }

        [Write(false)]
        public int ReturnQty { get; set; }

        [Write(false)]
        public string ReturnReceivedByUser { get; set; }

        #endregion Additional

        public CDAFloorReturnReceivedMaster()
        {
            Childs = new List<CDAFloorReturnReceivedChild>();
        }
    }

  

}
