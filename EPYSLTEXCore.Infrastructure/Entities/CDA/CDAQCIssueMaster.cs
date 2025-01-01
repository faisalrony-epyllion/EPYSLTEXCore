using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAQCIssueMaster")]
    public class CDAQCIssueMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCIssueMasterID { get; set; }
        
        public string QCIssueNo { get; set; }

        public int ReceiveID { get; set; }

        public int SupplierID { get; set; }

        public int CompanyID { get; set; }

        public int RCompanyID { get; set; }

        public DateTime QCIssueDate { get; set; }

        public int QCIssueBy { get; set; }

        public int QCReqMasterId { get; set; }

        public bool Approve { get; set; }

        public DateTime? ApproveDate { get; set; }

        public int? ApproveBy { get; set; }

        public bool Reject { get; set; }

        public DateTime? RejectDate { get; set; }

        public int? RejectBy { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        #region Additional Columns
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || QCIssueMasterID > 0;

        [Write(false)]
        public List<CDAQCIssueChild> Childs { get; set; }

        [Write(false)]
        public List<CDAQCReceiveMaster> CDAQCReceiveMasters { get; set; }

        [Write(false)]
        public string QCIssueByUser { get; set; }

        [Write(false)]
        public string QCReqNo { get; set; }

        [Write(false)]
        public DateTime QCReqDate { get; set; }

        [Write(false)]
        public string QCReqFor { get; set; }

        [Write(false)]
        public string QCReqByUser { get; set; }

        [Write(false)]
        public int ReqQty { get; set; }

        [Write(false)]
        public int IssueQty { get; set; }

        #endregion

        public CDAQCIssueMaster()
        {
            DateAdded = DateTime.Now;
            Childs = new List<CDAQCIssueChild>();
            CDAQCReceiveMasters = new List<CDAQCReceiveMaster>();
        }
    }
}