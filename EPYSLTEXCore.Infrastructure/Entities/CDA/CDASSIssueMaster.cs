using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASSIssueMaster")]
    public class CDASSIssueMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int SSIssueMasterID { get; set; }
        ///<summary>
        /// SupplierID
        ///</summary>
        public int SupplierID { get; set; }
        ///<summary>
        /// SSIssueNo (length: 50)
        ///</summary>
        public string SSIssueNo { get; set; }

        ///<summary>
        /// SSIssueDate
        ///</summary>
        public System.DateTime SSIssueDate { get; set; }

        ///<summary>
        /// SSIssueBy
        ///</summary>
        public int SSIssueBy { get; set; }

        ///<summary>
        /// SSReqMasterID
        ///</summary>
        public int SSReqMasterID { get; set; }

        ///<summary>
        /// CompanyID
        ///</summary>
        public int CompanyID { get; set; }

        ///<summary>
        /// SubGroupID
        ///</summary>
        public int SubGroupID { get; set; }

        ///<summary>
        /// Approve
        ///</summary>
        public bool Approve { get; set; }

        ///<summary>
        /// ApproveDate
        ///</summary>
        public System.DateTime? ApproveDate { get; set; }

        ///<summary>
        /// ApproveBy
        ///</summary>
        public int? ApproveBy { get; set; }

        ///<summary>
        /// Reject
        ///</summary>
        public bool Reject { get; set; }

        ///<summary>
        /// RejectDate
        ///</summary>
        public System.DateTime? RejectDate { get; set; }

        ///<summary>
        /// RejectBy
        ///</summary>
        public int? RejectBy { get; set; }

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; }

        ///<summary>
        /// DateAdded
        ///</summary>
        public System.DateTime DateAdded { get; set; }

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; }

        ///<summary>
        /// DateUpdated
        ///</summary>
        public System.DateTime? DateUpdated { get; set; }

        ///<summary>
        /// Remarks (length: 250)
        ///</summary>
        public string Remarks { get; set; }

        #region Additional Columns

        [Write(false)]
        public int CompanyId { get; set; }
        [Write(false)]
        public int SupplierId { get; set; }
        [Write(false)]
        public int SubGroupId { get; set; }
        [Write(false)]
        public int SSReqMasterId { get; set; }
        [Write(false)]
        public IList<Select2OptionModel> SupplierList { get; set; }
        [Write(false)]
        public string SSIssueByUser { get; set; }
        [Write(false)]
        public string SSReqNo { get; set; }
        [Write(false)]
        public DateTime SSReqDate { get; set; }
        [Write(false)]
        public string ReqByUser { get; set; }
        [Write(false)]
        public int ReqQty { get; set; }
        [Write(false)]
        public int IssueQty { get; set; }
        [Write(false)]
        public List<CDASSIssueChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSIssueMasterID > 0;

        #endregion Additional Columns
       
        public CDASSIssueMaster()
        {
            SSIssueDate = DateTime.Now;
            SSIssueNo = AppConstants.NEW;
            Approve = false;
            Reject = false;
            Childs = new  List<CDASSIssueChild>();
        }
    }

}

