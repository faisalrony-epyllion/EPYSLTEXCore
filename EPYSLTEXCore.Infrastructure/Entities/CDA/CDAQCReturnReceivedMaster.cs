using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAQCReturnReceivedMaster")]
    public class CDAQCReturnReceivedMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReturnReceivedMasterID { get; set; }
        public int ReceiveID { get; set; }
        public int SupplierID { get; set; }
        public int CompanyID { get; set; }
        public int RCompanyID { get; set; }
        public DateTime QCReturnReceivedDate { get; set; }

        ///<summary>
        /// QCReturnReceivedBy
        ///</summary>
        public int QCReturnReceivedBy { get; set; }

        ///<summary>
        /// QCReturnMasterID
        ///</summary>
        public int QCReturnMasterId { get; set; }

        ///<summary>
        /// QCReturnNo (length: 50)
        ///</summary>
        public string QCReturnNo { get; set; }

        ///<summary>
        /// QCReturnBy
        ///</summary>
        public int QCReturnBy { get; set; }

        ///<summary>
        /// QCReturnDate
        ///</summary>
        public DateTime QCReturnDate { get; set; }

        ///<summary>
        /// QCReqMasterID
        ///</summary>
        public int QCReqMasterId { get; set; }

        ///<summary>
        /// QCRemarksMasterID
        ///</summary>
        public int QCRemarksMasterId { get; set; }

        ///<summary>
        /// IsApprove
        ///</summary>
        public bool IsApprove { get; set; }

        ///<summary>
        /// ApproveDate
        ///</summary>
        public DateTime? ApproveDate { get; set; }

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
        public DateTime? AcknowledgeDate { get; set; }

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
        public int QCReturnMasterID { get; set; }
        [Write(false)]
        public int QCRemarksMasterID { get; set; }
        [Write(false)]
        public string QCReqFor { get; set; }
        [Write(false)]
        public DateTime QCReceiveDate { get; set; }
        [Write(false)]
        public string QCReceiveNo { get; set; }
        [Write(false)]
        public int QCReqMasterID { get; set; }
        [Write(false)]
        public string QCReqNo { get; set; }
        [Write(false)]
        public DateTime QCReqDate { get; set; }
        [Write(false)]
        public int QCReqQty { get; set; }
        [Write(false)]
        public int QCReturnQty { get; set; }
        [Write(false)]
        public string QCReturnByUser { get; set; }
        [Write(false)]
        public int ReceiveQty { get; set; }
        [Write(false)]
        public int ReturnQty { get; set; }
        [Write(false)]
        public string ReturnReceivedByUser { get; set; }

        [Write(false)]
        public List<CDAQCReturnReceivedChild> CDAQCReturnReceivedChilds { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCReturnReceivedMasterID > 0;

        #endregion Additional Columns

        public CDAQCReturnReceivedMaster()
        {
            QCReturnReceivedDate = DateTime.Now;
            IsApprove = false;
            IsAcknowledge = false;
            CDAQCReturnReceivedChilds = new List<CDAQCReturnReceivedChild>();
        }
    }
   
}
