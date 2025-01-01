using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_REQ_ISSUE_MASTER)]
    public class YDReqIssueMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDReqIssueMasterID { get; set; }

        public string YDReqIssueNo { get; set; }
        public DateTime YDReqIssueDate { get; set; }
        public int YDReqIssueBy { get; set; }
        public int YDReqMasterID { get; set; }

        public string ChallanNo { get; set; }
        public DateTime ChallanDate { get; set; }
        public string GPNo { get; set; }
        public DateTime GPDate { get; set; }
        public int TransportTypeID { get; set; }
        public int TransportAgencyID { get; set; }
        public string VehicleNo { get; set; }
        public string DriverName { get; set; }
        public string ContactNo { get; set; }
        public bool IsCompleted { get; set; }

        public bool IsSendForApprove { get; set; }
        public int SendForApproveBy { get; set; }
        public DateTime SendForApproveDate { get; set; }
        public bool IsAcknowledge { get; set; }
        public int AcknowledgeBy { get; set; } = 0;
        public DateTime? AcknowledgeDate { get; set; }
        public bool IsApprove { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int ApproveBy { get; set; } = 0;
        public bool IsReject { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; }
        public int RejectBy { get; set; } = 0;
        public bool IsGPApprove { get; set; }
        public DateTime? GPApproveDate { get; set; }
        public int GPApproveBy { get; set; } = 0;
        public int AddedBy { get; set; }
        public int UpdatedBy { get; set; } = 0;
        public int BuyerID { get; set; }
        public int LocationID { get; set; }
        public int CompanyID { get; set; }
        public int RCompanyID { get; set; }
        public int SupplierID { get; set; }
        public int SpinnerID { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string Remarks { get; set; }

        #region Additional Columns

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDReqIssueMasterID > 0;

        [Write(false)]
        public IEnumerable<Select2OptionModel> BuyerList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> LocationList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> RCompanyList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> SupplierList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnBrandList { get; set; }


        [Write(false)]
        public List<YDReqIssueChild> Childs { get; set; }

        [Write(false)]
        public string YDReqNo { get; set; }

        [Write(false)]
        public string YDBookingNo { get; set; }
        [Write(false)]
        public DateTime? YDBookingDate { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public string ReqByUser { get; set; }

        [Write(false)]
        public DateTime YDReqDate { get; set; }

        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public decimal ReqQty { get; set; }

        [Write(false)]
        public string SendForApproveName { get; set; }

        [Write(false)]
        public string ApprovedName { get; set; }

        [Write(false)]
        public string AcknowledgeName { get; set; }

        [Write(false)]
        public string RejectName { get; set; }
        [Write(false)]
        public string ReqType { get; set; }
        [Write(false)]
        public string ProgramName { get; set; }


        #endregion Additional Columns

        public YDReqIssueMaster()
        {
            YDReqIssueNo = AppConstants.NEW;
            BuyerID = 0;
            LocationID = 0;
            CompanyID = 0;
            RCompanyID = 0;
            SupplierID = 0;
            SpinnerID = 0;
            GPApproveBy = 0;
            YDReqIssueBy = 0;
            YDReqMasterID = 0;
            TransportTypeID = 0;
            TransportAgencyID = 0;
            IsCompleted = false;
            IsSendForApprove = false;
            SendForApproveBy = 0;
            IsAcknowledge = false;
            AcknowledgeBy = 0;
            IsApprove = false;
            ApproveBy = 0;
            ApproveDate = DateTime.Now;
            IsReject = false;
            RejectBy = 0;
            IsGPApprove = false;
            GPApproveBy = 0;
            AddedBy = 0;
            UpdatedBy = 0;
            GPApproveDate = DateTime.Now; ;
            RejectDate = DateTime.Now;
            AcknowledgeDate = DateTime.Now;
            DateAdded = DateTime.Now;
            YDReqIssueDate = DateTime.Now;
            SendForApproveDate = DateTime.Now;
            ChallanDate = DateTime.Now;
            GPDate = DateTime.Now;
            DateUpdated = DateTime.Now;
            Childs = new List<YDReqIssueChild>();
            ChallanNo = "";
            GPNo = "";
            VehicleNo = "";
            DriverName = "";
            ContactNo = "";
            RejectReason = "";
            Remarks = "";
        }
    }
}
