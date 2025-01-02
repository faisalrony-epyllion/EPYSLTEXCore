using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_REQ_MASTER)]
    public class YDReqMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDReqMasterID { get; set; }
        public string YDReqNo { get; set; }
        public int YDReqBy { get; set; }
        public DateTime YDReqDate { get; set; }
        public int YDBookingMasterID { get; set; }
        public int BuyerID { get; set; }
        public int CompanyId { get; set; }
        public string Remarks { get; set; }
        public bool IsApprove { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int ApproveBy { get; set; }
        public bool IsReject { get; set; }
        public DateTime? RejectDate { get; set; }
        public int RejectBy { get; set; }
        public string RejectReason { get; set; }
        public bool IsAcknowledge { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public int AcknowledgeBy { get; set; }
        public bool IsUnAcknowledge { get; set; }
        public DateTime? UnAcknowledgeDate { get; set; }
        public int UnAcknowledgeBy { get; set; }
        public string UnAcknowledgeReason { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        #region Additional Columns

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YDReqMasterID > 0;

        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public string ReqByUser { get; set; }

        [Write(false)]
        public string YDBookingNo { get; set; }

        [Write(false)]
        public string BookingByUser { get; set; }

        [Write(false)]
        public DateTime YDBookingDate { get; set; }

        [Write(false)]
        public string RefBatchNo { get; set; }

        [Write(false)]
        public string SampleRefNo { get; set; }

        [Write(false)]
        public int BookingQty { get; set; }

        [Write(false)]
        public decimal ReqQty { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public int YDReqIssueMasterID { get; set; }
        [Write(false)]
        public string YDReqIssueNo { get; set; }
        [Write(false)]
        public DateTime YDReqIssueDate { get; set; }
        [Write(false)]
        public int YDReqIssueBy { get; set; }
        [Write(false)]
        public bool IsSendForApprove { get; set; }
        [Write(false)]
        public string SendForApproveName { get; set; }
        [Write(false)]
        public DateTime? SendForApproveDate { get; set; }
        [Write(false)]
        public string AcknowledgeName { get; set; }
        [Write(false)]
        public int IsBDS { get; set; } = 0;
        [Write(false)]
        public int ConceptID { get; set; } = 0;
        [Write(false)]
        public decimal RequestedQty { get; set; } = 0;
        [Write(false)]
        public List<YDReqChild> Childs { get; set; }
        [Write(false)]
        public List<Select2OptionModel> StockTypeList { get; set; } = new List<Select2OptionModel>();

        #endregion Additional Columns

        public YDReqMaster()
        {
            YDReqDate = DateTime.Now;
            YDReqNo = AppConstants.NEW;
            DateAdded = DateTime.Now;
            YDBookingMasterID = 0;
            AcknowledgeBy = 0;
            UnAcknowledgeBy = 0;
            ApproveBy = 0;
            RejectBy = 0;
            BuyerID = 0;
            UpdatedBy = 0;
            IsReject = false;
            Childs = new List<YDReqChild>();
        }
    }
}
