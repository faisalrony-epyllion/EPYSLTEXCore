using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDAPRMaster")]
    public class CDAPRMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int CDAPRMasterID { get; set; }
        public int SubGroupID { get; set; }
        public int CompanyID { get; set; }
        public string Remarks { get; set; }
        public bool SendForApproval { get; set; }
        public int SendForApproveBy { get; set; }
        public DateTime? SendForApproveDate { get; set; }
        public bool? Approve { get; set; }
        public int? ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public bool? Acknowledge { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public int? AcknowledgeBy { get; set; }
        public bool? Reject { get; set; }
        public int? RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; }
        public int AddedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int TriggerPointID { get; set; }
        public string CDAPRNo { get; set; }
        public DateTime CDAPRDate { get; set; }
        public DateTime CDAPRRequiredDate { get; set; }
        public int CDAPRBy { get; set; }
        public bool? IsCPR { get; set; }
        public int? CPRBy { get; set; }
        public DateTime? CPRDate { get; set; }
        public bool? IsFPR { get; set; }
        public int? FPRBy { get; set; }
        public DateTime? FPRDate { get; set; }


        public bool IsSendForCheck { get; set; }
        public int SendForCheckBy { get; set; }
        public DateTime? SendForCheckDate { get; set; }

        public bool IsCheck { get; set; }
        public int CheckBy { get; set; }
        public DateTime? CheckDate { get; set; }

        public bool IsCheckReject { get; set; }
        public int CheckRejectBy { get; set; }
        public DateTime? CheckRejectDate { get; set; }
        public string CheckRejectReason { get; set; }

        #region Additional Columns 
        [Write(false)]
        public string CDAPRByUser { get; set; }
        [Write(false)]
        public string TriggerPoint { get; set; }

        [Write(false)]
        public string IndentNo { get; set; }
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CDAPRByList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TriggerPointList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CDAItemList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> CDAAgentList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> SubGroupList { get; set; }

        [Write(false)]
        public List<CDAPRChild> Childs { get; set; }

        [Write(false)]
        public List<CDAPRChild> ChildsItemSegments { get; set; }

        [Write(false)]
        public List<CDAPRChild> ChildsItemSegmentsPost { get; set; }
        [Write(false)]
        public List<CDAPRCompany> CDAPRCompanies { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.CDAPRMasterID > 0;

        #endregion Additional Columns 

        public CDAPRMaster()
        {
            CompanyID = 0;
            Remarks = "";
            SendForApproval = false;
            SendForApproveBy = 0;
            Approve = false;
            ApproveBy = 0;
            Acknowledge = false;
            AcknowledgeBy = 0;
            Reject = false;
            RejectBy = 0;
            RejectReason = "";
            UpdatedBy = 0;
            DateAdded = DateTime.Now;
            CDAPRNo = AppConstants.NEW;
            CDAPRDate = System.DateTime.Now;
            CDAPRRequiredDate = DateTime.Now;
            SendForApproveDate = DateTime.Now;

            IsSendForCheck = false;
            SendForCheckBy = 0;
            SendForCheckDate = null;
            IsCheck = false;
            CheckBy = 0;
            CheckDate = null;
            IsCheckReject = false;
            CheckRejectBy = 0;
            CheckRejectDate = null;
            CheckRejectReason = "";

            Childs = new System.Collections.Generic.List<CDAPRChild>();
            ChildsItemSegments = new System.Collections.Generic.List<CDAPRChild>();
            ChildsItemSegmentsPost = new System.Collections.Generic.List<CDAPRChild>();
        }
    }

}

