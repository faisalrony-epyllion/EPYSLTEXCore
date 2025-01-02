using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASSRemarksMaster")]
    public class CDASSRemarksMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int SSRemarksMasterID { get; set; }

        ///<summary>
        /// SupplierID
        ///</summary>
        public int SupplierID { get; set; }
        ///<summary>
        /// SubGroupID
        ///</summary>
        public int SubGroupID { get; set; }

        ///<summary>
        /// SSReqMasterID
        ///</summary>
        public int SSReqMasterID { get; set; }

        ///<summary>
        /// SSIssueMasterID
        ///</summary>
        public int SSIssueMasterID { get; set; }

        ///<summary>
        /// SSReceiveMasterID
        ///</summary>
        public int SSReceiveMasterID { get; set; }

        ///<summary>
        /// SSRemarksNo (length: 50)
        ///</summary>
        public string SSRemarksNo { get; set; }

        ///<summary>
        /// SSRemarksBy
        ///</summary>
        public int SSRemarksBy { get; set; }

        ///<summary>
        /// SSRemarksDate
        ///</summary>
        public System.DateTime SSRemarksDate { get; set; }

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

        #region Additional Columns

        [Write(false)]
        public int CompanyId { get; set; }
        [Write(false)]
        public int SubGroupId { get; set; }
        [Write(false)]
        public int SupplierId { get; set; }
        [Write(false)]
        public string SSRemarksdByUser { get; set; }
        [Write(false)]
        public string SSReceiveNo { get; set; }
        [Write(false)]
        public DateTime SSReceiveDate { get; set; }
        [Write(false)]
        public string SSReceivedByUser { get; set; }
        [Write(false)]
        public int SSReqMasterId { get; set; }
        [Write(false)]
        public int SSIssueMasterId { get; set; }
        [Write(false)]
        public string SSIssueNo { get; set; }
        [Write(false)]
        public DateTime SSIssueDate { get; set; }
        [Write(false)]
        public string SSIssueByUser { get; set; }
        [Write(false)]
        public string SSReqFor { get; set; }
        [Write(false)]
        public string SSReqNo { get; set; }
        [Write(false)]
        public DateTime SSReqDate { get; set; }
        [Write(false)]
        public int ReqQty { get; set; }
        [Write(false)]
        public int IssueQty { get; set; }
        [Write(false)]
        public int ReceiveQty { get; set; }
        [Write(false)]
        public decimal Rate { get; set; }

        [Write(false)]
        public List<CDASSRemarksChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSRemarksMasterID > 0;

        #endregion Additional Columns

        public CDASSRemarksMaster()
        {
            SSRemarksDate = DateTime.Now;
            SSRemarksNo = AppConstants.NEW;
            Childs = new List<CDASSRemarksChild>();
        }
    }

}

