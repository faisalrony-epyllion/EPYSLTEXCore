using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAFloorReturnMaster")]
    public class CDAFloorReturnMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int FloorReturnMasterID { get; set; }

        public int SupplierID { get; set; }
        
        public string FloorReturnNo { get; set; }

        public int FloorReturnBy { get; set; }

        public DateTime FloorReturnDate { get; set; }

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

        public int SubGroupID { get; set; }


        #region Additional
        [Write(false)]
        public List<CDAFloorReturnChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FloorReturnMasterID > 0;

        [Write(false)]
        public DateTime FloorReturnReceivedDate { get; set; }

        [Write(false)]
        public int FloorReqMasterID { get; set; }

        [Write(false)]
        public string FloorReqFor { get; set; }

        [Write(false)]
        public DateTime FloorReceiveDate { get; set; }

        [Write(false)]
        public string FloorReceiveNo { get; set; }

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

        #endregion

        public CDAFloorReturnMaster()
        {
            Childs = new List<CDAFloorReturnChild>();
        }
    }




}
