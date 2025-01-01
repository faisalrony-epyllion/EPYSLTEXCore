using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASSReturnMaster")]
    public class CDASSReturnMaster : DapperBaseEntity
    {
        
        [ExplicitKey]
        public int SSReturnMasterID { get; set; }

        ///<summary>
        /// SupplierID
        ///</summary>
        public int SupplierID { get; set; }
        ///<summary>
        /// SubGroupID
        ///</summary>
        public int SubGroupID { get; set; }

        ///<summary>
        /// SSReturnNo (length: 50)
        ///</summary>
        public string SSReturnNo { get; set; }

        ///<summary>
        /// SSReturnBy
        ///</summary>
        public int SSReturnBy { get; set; }

        ///<summary>
        /// SSReturnDate
        ///</summary>
        public System.DateTime SSReturnDate { get; set; }

        ///<summary>
        /// SSRemarksMasterID
        ///</summary>
        public int SSRemarksMasterID { get; set; }

        ///<summary>
        /// IsApprove
        ///</summary>
        public bool IsApprove { get; set; }

        ///<summary>
        /// ApproveDate
        ///</summary>
        public System.DateTime? ApproveDate { get; set; }

        ///<summary>
        /// ApproveBy
        ///</summary>
        public int? ApproveBy { get; set; }

        ///<summary>
        /// IsAcknowledge
        ///</summary>
        public bool IsAcknowledge { get; set; }

        ///<summary>
        /// AcknowledgeDate
        ///</summary>
        public System.DateTime? AcknowledgeDate { get; set; }

        ///<summary>
        /// AcknowledgeBy
        ///</summary>
        public int? AcknowledgeBy { get; set; }

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
        public string SSRemarksNo { get; set; }
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
        public DateTime SSReturnReceivedDate { get; set; }
        [Write(false)]
        public List<CDASSReturnChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSReturnMasterID > 0;

        #endregion Additional Columns

        public CDASSReturnMaster()
        {
            SSReturnDate = DateTime.Now;
            SSReturnNo = AppConstants.NEW;
            Childs = new  List<CDASSReturnChild>();
        }
    }
   
}

