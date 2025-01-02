using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASSReceiveMaster")]
    public class CDASSReceiveMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int SSReceiveMasterID { get; set; }
        
        ///<summary>
        /// SupplierID
        ///</summary>
        public int SupplierID { get; set; }
        ///<summary>
        /// SSReceiveNo (length: 50)
        ///</summary>
        public string SSReceiveNo { get; set; }

        ///<summary>
        /// SSReceivedBy
        ///</summary>
        public int SSReceivedBy { get; set; }

        ///<summary>
        /// SSReceiveDate
        ///</summary>
        public System.DateTime SSReceiveDate { get; set; }

        ///<summary>
        /// SSReqMasterID
        ///</summary>
        public int SSReqMasterID { get; set; }

        ///<summary>
        /// SSIssueMasterID
        ///</summary>
        public int SSIssueMasterID { get; set; }

        ///<summary>
        /// SubGroupID
        ///</summary>
        public int SubGroupID { get; set; }

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
        /// CompanyID
        ///</summary>
        public int CompanyID { get; set; }

        ///<summary>
        /// Remarks (length: 250)
        ///</summary>
        public string Remarks { get; set; }

        #region Additional Columns
        [Write(false)]
        public int CompanyId { get; set; }
        [Write(false)]
        public int SubGroupId { get; set; }
        [Write(false)]
        public int SupplierId { get; set; }
        [Write(false)]
        public int SSIssueMasterId { get; set; }
        [Write(false)]
        public string SSReceivedByUser { get; set; }
        [Write(false)]
        public string SSIssueNo { get; set; }
        [Write(false)]
        public DateTime SSIssueDate { get; set; }
        [Write(false)]
        public string SSIssueByUser { get; set; }
        [Write(false)]
        public int SSReqMasterId { get; set; }
        [Write(false)]
        public string ReqByUser { get; set; }
        [Write(false)]
        public string SSReqNo { get; set; }
        [Write(false)]
        public DateTime SSReqDate { get; set; }
        [Write(false)]
        public string SSReqFor { get; set; }
        [Write(false)]
        public int ReqQty { get; set; }
        [Write(false)]
        public int IssueQty { get; set; }
        [Write(false)]
        public int ReceiveQty { get; set; }
        [Write(false)]
        public List<CDASSReceiveChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSReceiveMasterID > 0;

        #endregion Additional Columns
        
        public CDASSReceiveMaster()
        {
            SSReceiveDate = DateTime.Now;
            SSReceiveNo = AppConstants.NEW;
            Childs = new  List<CDASSReceiveChild>();
        }
    }

}

