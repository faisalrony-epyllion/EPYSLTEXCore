using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn.Knitting
{
    [Table(TableNames.KNITTING_SUB_CONTRACT_ISSUE_MASTER)]
    public class KnittingSubContractIssueMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int KSCIssueMasterID { get; set; } = 0;
        public string KSCIssueNo { get; set; } = AppConstants.NEW;
        public DateTime KSCIssueDate { get; set; } = DateTime.Now;
        public int KSCIssueByID { get; set; } = 0;
        public int KSCMasterID { get; set; } = 0;
        public int KSCReqMasterID { get; set; } = 0;
        public string ChallanNo { get; set; } = AppConstants.NEW;
        public DateTime ChallanDate { get; set; } = DateTime.Now;
        public string GPNo { get; set; } = AppConstants.NEW;
        public DateTime GPDate { get; set; } = DateTime.Now;
        public int TransportTypeID { get; set; } = 0;
        public int TransportAgencyID { get; set; } = 0;
        public string VehicleNo { get; set; } = "";
        public string DriverName { get; set; } = "";
        public string ContactNo { get; set; } = "";
        public bool IsCompleted { get; set; } = false;
        public string Remarks { get; set; } = "";
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; } = 0;
        public DateTime DateUpdated { get; set; } = DateTime.Now;
        public bool IsSendForApprove { get; set; } = false;
        public int SendForApproveBy { get; set; } = 0;
        public DateTime SendForApproveDate { get; set; } = DateTime.Now;
        public bool IsAcknowledge { get; set; } = false;
        public int AcknowledgeBy { get; set; } = 0;
        public DateTime? AcknowledgeDate { get; set; } = DateTime.Now;
        public bool IsApprove { get; set; } = false;
        public DateTime? ApproveDate { get; set; } = DateTime.Now;
        public int ApproveBy { get; set; } = 0;
        public bool IsReject { get; set; } = false;
        public DateTime? RejectDate { get; set; } = DateTime.Now;
        public string RejectReason { get; set; } = "";
        public int RejectBy { get; set; } = 0;
        public bool IsGPApprove { get; set; } = false;
        public DateTime GPApproveDate { get; set; } = DateTime.Now;
        public int GPApproveBy { get; set; } = 0;

        #region Additional Columns

        [Write(false)]
        public string KSCReqNo { get; set; } = "";
        [Write(false)]
        public DateTime KSCReqDate { get; set; } = DateTime.Now;
        [Write(false)]
        public string KSCNo { get; set; } = "";
        [Write(false)]
        public string YBookingNo { get; set; } = "";
        [Write(false)]
        public string SubContractor { get; set; } = "";
        [Write(false)]
        public string BookingNo { get; set; } = "";
        [Write(false)]
        public string ExportOrderNo { get; set; } = "";
        [Write(false)]
        public string KSCIssueByUser { get; set; } = "";
        [Write(false)]
        public decimal BookingQty { get; set; } = 0;
        [Write(false)]
        public decimal SCQty { get; set; } = 0;
        [Write(false)]
        public decimal ReqQty { get; set; } = 0;
        [Write(false)]
        public decimal Rate { get; set; } = 0;
        [Write(false)]
        public string ConceptNo { get; set; } = "";
        [Write(false)]
        public string ProgramName { get; set; } = "";
        [Write(false)]
        public string ReqType { get; set; } = "";
        [Write(false)]
        public string CorBookingNo { get; set; } = "";
        [Write(false)]
        public int SubGroupID { get; set; } = 0;
        [Write(false)]
        public string SubGroupName { get; set; } = "";
        [Write(false)]
        public string KnittingType { get; set; } = "";
        [Write(false)]
        public string TechnicalName { get; set; } = "";
        [Write(false)]
        public string Composition { get; set; } = "";
        [Write(false)]
        public int Gsm { get; set; } = 0;
        [Write(false)]
        public decimal Length { get; set; } = 0;
        [Write(false)]
        public decimal Width { get; set; } = 0;
        [Write(false)]
        public string ReqByUser { get; set; } = "";
        [Write(false)]
        public string SCByUser { get; set; } = "";
        [Write(false)]
        public string Company { get; set; } = "";
        [Write(false)]
        public string ServiceProvider { get; set; } = "";
        [Write(false)]
        public string KSCUnit { get; set; } = "";
        [Write(false)]
        public IEnumerable<Select2OptionModel> SpinnerList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TransportTypeList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TransportAgencyList { get; set; }
        [Write(false)]
        public List<KnittingSubContractIssueChild> Childs { get; set; } = new List<KnittingSubContractIssueChild>();

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.KSCIssueMasterID > 0;

        #endregion Additional Columns

        public KnittingSubContractIssueMaster()
        {

        }
    }
}
