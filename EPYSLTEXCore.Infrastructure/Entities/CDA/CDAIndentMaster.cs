using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAIndentMaster")]
    public class CDAIndentMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int CDAIndentMasterID { get; set; }
        public string IndentNo { get; set; }
        public DateTime IndentDate { get; set; }
        public int SubGroupID { get; set; }
        public int TriggerPointID { get; set; }
        public DateTime? IndentStartMonth { get; set; }
        public DateTime? IndentEndMonth { get; set; }
        public string Remarks { get; set; }
        public bool SendForApproval { get; set; }
        public int SendForApproveBy { get; set; }
        public DateTime? SendForApproveDate { get; set; }
        public bool SendForAcknowledge { get; set; }
        public int SendForAcknowledgeBy { get; set; }
        public DateTime? SendForAcknowledgeDate { get; set; }
        public bool Approve { get; set; }
        public int ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public bool Acknowledge { get; set; }
        public int AcknowledgeBy { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public bool TexAcknowledge { get; set; }
        public int TexAcknowledgeBy { get; set; }
        public DateTime? TexAcknowledgeDate { get; set; }
        public bool SendForCheck { get; set; }
        public int SendForCheckBy { get; set; }
        public DateTime? SendForCheckDate { get; set; }
        public bool IsCheck { get; set; }
        public int CheckBy { get; set; }
        public DateTime? CheckDate { get; set; }

        public bool Reject { get; set; }
        public int RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; }
        public bool IsCIndent { get; set; }
        public int? CIndentBy { get; set; }
        public DateTime? CIndentDate { get; set; }
        public int AddedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }

        #region Additional Columns

        [Write(false)]
        public string CDAIndentByUser { get; set; }
        [Write(false)]
        public string TriggerPoint { get; set; }
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public bool IsSendForApprove { get; set; }
        [Write(false)]
        public bool IsApporve { get; set; }
        [Write(false)]
        public bool IsAck { get; set; }
        [Write(false)]
        public bool IsTexAck { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CIndentByList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TriggerPointList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CDAItemList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CDAAgentList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> SubGroupList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }
        [Write(false)]
        public List<CDAIndentChild> Childs { get; set; }
        [Write(false)] 
        public List<CDAIndentChildDetails> ChildItems { get; set; }
        [Write(false)]
        public List<CDAIndentChild> ChildsItemSegments { get; set; }
        [Write(false)]
        public List<CDAIndentChild> ChildsItemSegmentsPost { get; set; }
        [Write(false)]
        public List<CDAIndentChildCompany> CDAIndentCompanies { get; set; }
        
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.CDAIndentMasterID > 0;

        #endregion Additional Columns

        public CDAIndentMaster()
        {
            IndentStartMonth = DateTime.Now;
            IndentEndMonth = DateTime.Now;
            Remarks = "";
            SendForApproval = false;
            SendForApproveBy = 0;
            SendForAcknowledge = false;
            SendForAcknowledgeBy = 0;
            Approve = false;
            ApproveBy = 0;
            Acknowledge = false;
            AcknowledgeBy = 0;
            TexAcknowledge = false;
            TexAcknowledgeBy = 0;
            SendForCheck = false;
            SendForCheckBy = 0;
            IsCheck = false;
            TexAcknowledgeBy = 0;
            Reject = false;
            RejectBy = 0;
            RejectReason = "";
            IsCIndent = false;
            CIndentBy = 0;
            UpdatedBy = 0;
            DateAdded = DateTime.Now;
            SendForApproveDate = DateTime.Now;
            IsSendForApprove = false;
            IsApporve = false;
            IsAck = false;
            IsTexAck = false;
            IndentNo = AppConstants.NEW;
            IndentDate = System.DateTime.Now;
            TriggerPointID = 0;
            Childs = new List<CDAIndentChild>();
            ChildsItemSegments = new List<CDAIndentChild>();
            ChildsItemSegmentsPost = new List<CDAIndentChild>();
            ChildItems = new List<CDAIndentChildDetails>();
        }
    } 
}