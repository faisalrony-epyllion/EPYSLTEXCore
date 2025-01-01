using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAQCRemarksMaster")]
    public class CDAQCRemarksMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCRemarksMasterID { get; set; }

        ///<summary>
        /// QCReqMasterID
        ///</summary>
        public int QCReqMasterId { get; set; }
        public int ReceiveID { get; set; }
        public int SupplierID { get; set; }
        public int CompanyID { get; set; }
        public int RCompanyID { get; set; }

        ///<summary>
        /// QCIssueMasterID
        ///</summary>
        public int QCIssueMasterId { get; set; }

        ///<summary>
        /// QCReceiveMasterID
        ///</summary>
        public int QCReceiveMasterId { get; set; }

        ///<summary>
        /// QCRemarksNo (length: 50)
        ///</summary>
        public string QCRemarksNo { get; set; }

        ///<summary>
        /// QCRemarksBy
        ///</summary>
        public int QCRemarksBy { get; set; }

        ///<summary>
        /// QCRemarksDate
        ///</summary>
        public DateTime QCRemarksDate { get; set; }

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; }

        ///<summary>
        /// DateAdded
        ///</summary>
        public DateTime DateAdded { get; set; }

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; }

        ///<summary>
        /// DateUpdated
        ///</summary>
        public DateTime? DateUpdated { get; set; }
        #region Additional Columns

        [Write(false)]
        public int RCompanyId { get; set; }
        [Write(false)]
        public int CompanyId { get; set; }
        [Write(false)]
        public int SupplierId { get; set; }
        [Write(false)]
        public string QCRemarksdByUser { get; set; }
        [Write(false)]
        public string QCReceiveNo { get; set; }
        [Write(false)]
        public DateTime QCReceiveDate { get; set; }
        [Write(false)]
        public string QCReceivedByUser { get; set; }
        [Write(false)]
        public string QCIssueNo { get; set; }
        [Write(false)]
        public DateTime QCIssueDate { get; set; }
        [Write(false)]
        public string QCIssueByUser { get; set; }
        [Write(false)]
        public string QCReqFor { get; set; }
        [Write(false)]
        public string QCReqNo { get; set; }
        [Write(false)]
        public DateTime QCReqDate { get; set; }
        [Write(false)]
        public int ReqQty { get; set; }
        [Write(false)]
        public int IssueQty { get; set; }
        [Write(false)]
        public int ReceiveQty { get; set; }

        [Write(false)]
        public List<CDAQCRemarksChild> CDAQCRemarksChilds { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCRemarksMasterID > 0;

        #endregion Additional Columns

        public CDAQCRemarksMaster()
        {
            DateAdded = DateTime.Now;
            QCRemarksBy = 0;
            UpdatedBy = 0;
            QCRemarksDate = DateTime.Now;
            QCRemarksNo = AppConstants.NEW;
            CDAQCRemarksChilds = new List<CDAQCRemarksChild>();
        }
    }

}

