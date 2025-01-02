using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDASTSIssueMaster")]
    public class CDASTSIssueMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int STSIssueMasterID { get; set; }
        public string STSIssueNo { get; set; }
        public DateTime STSIssueDate { get; set; }
        public int STSIssueBy { get; set; }
        public int STSReqMasterID { get; set; }
        public int CompanyID { get; set; }
        public int RCompanyID { get; set; }
        public int SubGroupID { get; set; }
        public int SupplierID { get; set; }
        public string Remarks { get; set; }
        public bool IsSendForApprove { get; set; }
        public int SendForApproveBy { get; set; }
        public DateTime SendForApproveDate { get; set; }
        public bool IsApprove { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int? ApproveBy { get; set; }
        public bool IsReject { get; set; }
        public int? RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; }
        public int AddedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; } 

        #region Additional Columns

        [Write(false)]
        public string CompanyName { get; set; }
        [Write(false)]
        public string RCompanyName { get; set; }
        [Write(false)]
        public string STSIssueByUser { get; set; }
        [Write(false)]
        public string STSReqNo { get; set; }
        [Write(false)]
        public DateTime STSReqDate { get; set; }
        [Write(false)]
        public string ReqByUser { get; set; }
        [Write(false)]
        public int ReqQty { get; set; }
        [Write(false)]
        public int IssueQty { get; set; } 
        [Write(false)]
        public string Status { get; set; } 



        [Write(false)] 
        public List<CDASTSIssueChild> Childs { get; set; } 
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.STSIssueMasterID > 0;

        #endregion Additional Columns
        public CDASTSIssueMaster()
        { 
            STSIssueDate = DateTime.Now;
            STSIssueNo = AppConstants.NEW;
            IsSendForApprove = false;
            SendForApproveDate = DateTime.Now;
            CompanyID = 0;
            IsApprove = false;
            IsReject = false;
            Childs = new List<CDASTSIssueChild>();
        }
    }

}
 
