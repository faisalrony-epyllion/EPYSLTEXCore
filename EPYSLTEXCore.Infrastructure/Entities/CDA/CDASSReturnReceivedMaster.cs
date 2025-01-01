using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASSReturnReceivedMaster")]
    public class CDASSReturnReceivedMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int SSReturnReceivedMasterID { get; set; }
        public int SubGroupID { get; set; }
        public DateTime SSReturnReceivedDate { get; set; }
        public int SSReturnMasterID { get; set; }
        public int SSRemarksMasterID { get; set; }
        public bool IsApprove { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int? ApproveBy { get; set; }
        public bool? IsAcknowledge { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public int? AcknowledgeBy { get; set; }
        public int AddedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int CompanyID { get; set; }
        public string SSReturnReceivedNo { get; set; }
        public int SupplierID { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; } 

        #region Additional Columns

        [Write(false)]
        public string SSReturnNo { get; set; }
        [Write(false)]
        public int SSReturnBy { get; set; }
        [Write(false)]
        public DateTime SSReturnDate { get; set; }
        [Write(false)]
        public string SSReqFor { get; set; }
        [Write(false)]
        public DateTime SSReceiveDate { get; set; }
        [Write(false)]
        public string SSReceiveNo { get; set; } 

        [Write(false)]
        public int SSReqMasterID { get; set; }
        [Write(false)]
        public string SSReqNo { get; set; }
        [Write(false)]
        public DateTime SSReqDate { get; set; }
        [Write(false)]
        public int SSReqQty { get; set; }
        [Write(false)]
        public int SSReturnQty { get; set; }
        [Write(false)]
        public string SSReturnByUser { get; set; }
        [Write(false)]
        public int ReceiveQty { get; set; }
        [Write(false)]
        public int ReturnQty { get; set; }
        [Write(false)]
        public string ReturnReceivedByUser { get; set; } 
        
        [Write(false)]
        public List<CDASSReturnReceivedChild> Childs { get; set; } 
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSReturnReceivedMasterID > 0;

        #endregion Additional Columns
        public CDASSReturnReceivedMaster()
        {
            IsAcknowledge = false;
            SSReturnReceivedDate = DateTime.Now;
            Childs = new List<CDASSReturnReceivedChild>();
        }
    }
    
}
 
