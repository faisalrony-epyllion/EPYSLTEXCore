using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASTSReqMaster")]
    public class CDASTSReqMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int STSReqMasterID { get; set; }
        public string STSReqNo { get; set; }
        public int STSReqBy { get; set; }
        public DateTime STSReqDate { get; set; }
        public int SubGroupID { get; set; } 
        public bool IsSendForApprove { get; set; }
        public int? SendForApproveBy { get; set; }
        public DateTime? SendForApproveDate { get; set; } 
        public bool IsApprove { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int? ApproveBy { get; set; }
        public bool IsAcknowledge { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public int? AcknowledgeBy { get; set; }
        public int AddedBy { get; set; }
        public bool IsReject { get; set; }
        public int? RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; }
        public int? UpdatedBy { get; set; }
        public string Remarks { get; set; }
        public int CompanyID { get; set; }
        public int RCompanyID { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }

        #region Additional Columns

        [Write(false)]
        public string RequisitionByUser { get; set; }
        [Write(false)]
        public string ApproveByUser { get; set; }
        [Write(false)]
        public string AcknowledgeByUser { get; set; }
        [Write(false)]
        public int TotalQty { get; set; }
        [Write(false)]
        public int TotalCount { get; set; }
        [Write(false)] 
        public string STSReqByUser { get; set; }
        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        //public IList<Select2OptionModel> ItemList { get; set; }
        public IEnumerable<Select2OptionModel> ItemList { get; set; } 
        [Write(false)]
        public IEnumerable<Select2OptionModel> Segment1ValueList { get; set; }
       
        [Write(false)]
        public IList<Select2OptionModel> AgentList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> RCompanyList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> STSReqByList { get; set; }
        [Write(false)]
        public List<CDASTSReqChild> Childs { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.STSReqMasterID > 0;

        #endregion Additional Columns
        public CDASTSReqMaster()
        {
            IsReject = false; 
            STSReqDate = DateTime.Now;
            STSReqNo = AppConstants.NEW;
            SendForApproveDate = DateTime.Now;
            IsApprove = false;
            IsAcknowledge = false;
            Childs = new List<CDASTSReqChild>(); 
        }
    }

}
 
