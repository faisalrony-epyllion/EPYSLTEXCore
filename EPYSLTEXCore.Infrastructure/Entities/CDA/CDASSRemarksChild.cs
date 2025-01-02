using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASSRemarksChild")]
    public class CDASSRemarksChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int SSRemarksChildID { get; set; }

        ///<summary>
        /// SSRemarksMasterID
        ///</summary>
        public int SSRemarksMasterID { get; set; }

        ///<summary>
        /// BatchNo (length: 50)
        ///</summary>
        public string BatchNo { get; set; }

        ///<summary>
        /// Rate
        ///</summary>
        public decimal Rate { get; set; }

        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// Remarks (length: 100)
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
        public System.DateTime? ApproveDate { get; set; }

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
        public System.DateTime? RejectDate { get; set; }

        ///<summary>
        /// ReTest
        ///</summary>
        public bool ReTest { get; set; }

        ///<summary>
        /// ReTestBy
        ///</summary>
        public int? ReTestBy { get; set; }

        ///<summary>
        /// ReTestDate
        ///</summary>
        public System.DateTime? ReTestDate { get; set; }

        ///<summary>
        /// ReqQty
        ///</summary>
        public int ReqQty { get; set; }

        ///<summary>
        /// IssueQty
        ///</summary>
        public int IssueQty { get; set; }

        ///<summary>
        /// ReceiveQty
        ///</summary>
        public int ReceiveQty { get; set; }

        #region Additional Columns
        [Write(false)]
        public int SSReceiveMasterId { get; set; }
        [Write(false)]
        public int SubGroupId { get; set; }
        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string AgentName { get; set; }
        [Write(false)]
        public DateTime SSRemarksDate { get; set; }
        [Write(false)]
        public int SSReqMasterId { get; set; }
        [Write(false)]
        public int SSIssueMasterId { get; set; }
        [Write(false)]
        public int CompanyId { get; set; }
        [Write(false)]
        public int SupplierId { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSRemarksChildID > 0;

        #endregion Additional Columns

        public CDASSRemarksChild()
        {
            SSReqMasterId = 0;
            SSIssueMasterId = 0;
            CompanyId = 0;
            SupplierId = 0;
            SSRemarksDate = DateTime.Now;
            Remarks = "";
            Approve = false;
            Reject = false;
            ReTest = false;
        }
    }

}

