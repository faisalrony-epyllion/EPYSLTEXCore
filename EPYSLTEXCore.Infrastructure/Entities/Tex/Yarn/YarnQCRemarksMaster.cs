using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;


namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table("YarnQCRemarksMaster")]
    public class YarnQCRemarksMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCRemarksMasterID { get; set; }
        public int QCReqMasterID { get; set; }
        public int QCIssueMasterID { get; set; }
        public bool PreTest { get; set; }
        public int PreTestBy { get; set; }
        public DateTime? PreTestDate { get; set; }
        public int QCReceiveMasterID { get; set; }
        public string QCRemarksNo { get; set; }
        public int QCRemarksBy { get; set; }
        public DateTime QCRemarksDate { get; set; }
        public int LocationId { get; set; }
        public int ReceiveID { get; set; }
        public int CompanyId { get; set; }
        public int RCompanyId { get; set; }
        public int SupplierId { get; set; }
        public int SpinnerId { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsSendForApproval { get; set; }
        public int SendForApprovalBy { get; set; }
        public DateTime? SendForApprovalDate { get; set; }
        public bool IsApproved { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsRetest { get; set; }
        public int RetestBy { get; set; }
        public DateTime? RetestDate { get; set; }
        public bool IsRetestForRequisition { get; set; } = false;
        public int RetestForRequisitionBy { get; set; } = 0;
        public DateTime? RetestForRequisitionDate { get; set; }

        #region Additional Columns
        [Write(false)]
        public string Segment1ValueDesc { get; set; }
        [Write(false)]
        public string Segment2ValueDesc { get; set; }
        [Write(false)]
        public string Segment3ValueDesc { get; set; }
        [Write(false)]
        public string Segment4ValueDesc { get; set; }
        [Write(false)]
        public string Segment5ValueDesc { get; set; }
        [Write(false)]
        public string Segment6ValueDesc { get; set; }
        [Write(false)]
        public string Segment7ValueDesc { get; set; }
        [Write(false)]
        public string QCRemarksByUser { get; set; }
        [Write(false)]
        public string ReceiveNo { get; set; }
        [Write(false)]
        public string QCReceiveNo { get; set; }
        [Write(false)]
        public DateTime QCReceiveDate { get; set; }
        [Write(false)]
        public string QCReceivedByUser { get; set; }
        [Write(false)]
        public string QCReqFor { get; set; }
        [Write(false)]
        public string QCReqNo { get; set; }

        [Write(false)]
        public DateTime QCReqDate { get; set; }

        [Write(false)]
        public string QCIssueNo { get; set; }
        [Write(false)]
        public DateTime QCIssueDate { get; set; }

        [Write(false)]
        public int ReqQtyCone { get; set; }
        [Write(false)]
        public int ReceiveQty { get; set; }
        [Write(false)]
        public int ReceiveQtyCone { get; set; }
        [Write(false)]
        public int ReceiveQtyCarton { get; set; }
        [Write(false)]
        public int QCReceiveChildID { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public bool ReTest { get; set; }
        [Write(false)]
        public string YarnStatus { get; set; }
        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        public int QCRemarksChildID { get; set; }
        [Write(false)]
        public string LotNo { get; set; }
        [Write(false)]
        public string YarnDetail { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string Remarks { get; set; }
        [Write(false)]
        public DateTime? ReceiveDate { get; set; }
        [Write(false)]
        public string RetestReqBy { get; set; }
        [Write(false)]
        public string RetestReason { get; set; }
        [Write(false)]
        public List<YarnQCRemarksChild> YarnQCRemarksChilds { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnAssessmentZoneList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnAssessmentStatusList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TechnicalNameList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> DPList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> BuyerList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> TestParamSetList { get; set; }
        [Write(false)]
        public List<YarnQCRemarksChildResult> YarnQCRemarksChildResults { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> FabricComponents { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || QCRemarksMasterID > 0;

        #endregion Additional Columns

        public YarnQCRemarksMaster()
        {
            QCRemarksDate = DateTime.Now;
            QCRemarksNo = AppConstants.NEW;
            DateAdded = DateTime.Now;
            QCRemarksBy = 0;
            UpdatedBy = 0;
            PreTest = false;
            PreTestBy = 0;
            ReTest = false;
            QCReceiveChildID = 0;
            QCRemarksChildID = 0;
            IsSendForApproval = false;
            SendForApprovalBy = 0;
            IsRetest = false;
            RetestBy = 0;
            IsApproved = false;
            ApprovedBy = 0;
            YarnQCRemarksChilds = new List<YarnQCRemarksChild>();
        }
    }
}
