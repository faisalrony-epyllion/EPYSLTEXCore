using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDASSReqMaster")]
    public class CDASSReqMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int SSReqMasterID { get; set; }
        
        ///<summary>
        /// SSReqNo (length: 50)
        ///</summary>
        public string SSReqNo { get; set; }

        ///<summary>
        /// SSReqBy
        ///</summary>
        public int SSReqBy { get; set; }

        ///<summary>
        /// SSReqDate
        ///</summary>
        public System.DateTime SSReqDate { get; set; }

        ///<summary>
        /// SubGroupID
        ///</summary>
        public int SubGroupID { get; set; }

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
        /// Reject
        ///</summary>
        public bool? Reject { get; set; }

        ///<summary>
        /// RejectBy
        ///</summary>
        public int? RejectBy { get; set; }

        ///<summary>
        /// RejectDate
        ///</summary>
        public System.DateTime? RejectDate { get; set; }

        ///<summary>
        /// RejectReason (length: 250)
        ///</summary>
        public string RejectReason { get; set; }

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

        ///<summary>
        /// CompanyID
        ///</summary>
        public int CompanyID { get; set; }

        #region Additional Columns

        [Write(false)]
        public int CompanyId { get; set; }
        [Write(false)]
        public string RequisitionByUser { get; set; }
        [Write(false)]
        public string ApproveByUser { get; set; }
        [Write(false)]
        public string AcknowledgeByUser { get; set; }
        [Write(false)]
        public int TotalQty { get; set; }
        [Write(false)]
        public int SubGroupId { get; set; }
        [Write(false)]
        public int RCompanyId { get; set; }
        [Write(false)]
        public string SSReqByUser { get; set; }
        [Write(false)]
        public IList<Select2OptionModel> ItemList { get; set; }
        [Write(false)]
        public IList<Select2OptionModel> AgentList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> SSReqByList { get; set; }

        [Write(false)]
        public List<CDASSReqChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSReqMasterID > 0;

        #endregion Additional Columns
        
        public CDASSReqMaster()
        {
            SSReqDate = DateTime.Now;
            SSReqNo = AppConstants.NEW;
            IsApprove = false;
            IsAcknowledge = false;
            Reject = false;
            Childs = new  List<CDASSReqChild>();
        }
    }

}

