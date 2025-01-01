using Dapper.Contrib.Extensions;
using System;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("CDAQCRemarksChild")]
    public class CDAQCRemarksChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCRemarksChildID { get; set; } 

        ///<summary>
        /// QCRemarksMasterID
        ///</summary>
        public int QCRemarksMasterId { get; set; }

        ///<summary>
        /// LotNo (length: 50)
        ///</summary>
        public string LotNo { get; set; }
        ///<summary>
        /// Rate
        ///</summary>
        public decimal Rate { get; set; }

        ///<summary>
        /// ReqQtyCone. No of Cone
        ///</summary>
        public int ReqQty { get; set; }

        ///<summary>
        /// IssueQtyCarton
        ///</summary>
        public int IssueQty { get; set; }
        ///<summary>
        /// ReceiveQtyCarton
        ///</summary>
        public int ReceiveQty { get; set; }
        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// Remarks (length: 500)
        ///</summary>
        public string Remarks { get; set; }

        ///<summary>
        /// Approve
        ///</summary>
        public bool Approve { get; set; }

        ///<summary>
        /// ApproveBy
        ///</summary>
        public int ApproveBy { get; set; }

        ///<summary>
        /// ApproveDate
        ///</summary>
        public DateTime? ApproveDate { get; set; }

        ///<summary>
        /// Reject
        ///</summary>
        public bool Reject { get; set; }

        ///<summary>
        /// RejectBy
        ///</summary>
        public int RejectBy { get; set; }

        ///<summary>
        /// RejectDate
        ///</summary>
        public DateTime? RejectDate { get; set; }

        ///<summary>
        /// ReTest
        ///</summary>
        public bool ReTest { get; set; }

        ///<summary>
        /// ReTestBy
        ///</summary>
        public int ReTestBy { get; set; }

        ///<summary>
        /// ReTestDate
        ///</summary>
        public DateTime? ReTestDate { get; set; }

        #region Additional Columns

        [Write(false)]
        public int ReceiveID { get; set; }
        [Write(false)]
        public int RCompanyId { get; set; }
        [Write(false)]
        public int CompanyId { get; set; }
        [Write(false)]
        public int SupplierId { get; set; }
        [Write(false)]
        public DateTime QCRemarksDate { get; set; }
        [Write(false)]
        public int QCReqMasterId { get; set; }
        [Write(false)]
        public int QCIssueMasterId { get; set; }
        [Write(false)]
        public int QCReceiveMasterId { get; set; }
        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string AgentName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCRemarksChildID > 0;

        #endregion Additional Columns

        public CDAQCRemarksChild()
        {
            UnitID = 28;
            ItemMasterID = 0;
            Remarks = "";
        }
    }

}

