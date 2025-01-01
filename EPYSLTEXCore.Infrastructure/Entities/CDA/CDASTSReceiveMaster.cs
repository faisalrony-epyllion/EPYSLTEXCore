using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASTSReceiveMaster")]
    public class CDASTSReceiveMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int STSReceiveMasterID { get; set; }
        public string STSReceiveNo { get; set; }
        public int STSReceivedBy { get; set; }
        public DateTime STSReceiveDate { get; set; }
        public int STSReqMasterID { get; set; }
        public int STSIssueMasterID { get; set; }
        public int SubGroupID { get; set; }
        public int AddedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int CompanyID { get; set; }
        public int RCompanyID { get; set; }
        public string Remarks { get; set; }
        public int SupplierID { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }

        #region Additional Columns 
        [Write(false)]
        public string CompanyName { get; set; }
        [Write(false)]
        public string RCompanyName { get; set; }
        [Write(false)]
        public string STSReceivedByUser { get; set; }
        [Write(false)]
        public string STSIssueNo { get; set; }
        [Write(false)]
        public DateTime STSIssueDate { get; set; }
        [Write(false)]
        public string STSIssueByUser { get; set; }
        [Write(false)]
        public string STSReqByUser { get; set; }
        [Write(false)]
        public DateTime STSReqDate { get; set; }
        [Write(false)]
        public string STSReqFor { get; set; }
        [Write(false)]
        public string STSReqNo { get; set; }
        [Write(false)]
        public int ReqQty { get; set; }
        [Write(false)]
        public int IssueQty { get; set; }
        [Write(false)]
        public int ReceiveQty { get; set; } 
        [Write(false)]
        public List<CDASTSReceiveChild> Childs { get; set; } 

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.STSReceiveMasterID > 0;

        #endregion Additional Columns 
        public CDASTSReceiveMaster()
        {
            STSReceiveDate = DateTime.Now;
            STSReceiveNo = AppConstants.NEW;
            Childs = new List<CDASTSReceiveChild>();
        }
    }
     
}
 
