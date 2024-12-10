using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table(TableNames.YARN_QC_ISSUE_MASTER)]
    public class YarnQCIssueMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCIssueMasterID { get; set; }
        public string QCIssueNo { get; set; }
        public DateTime QCIssueDate { get; set; }
        public int QCIssueBy { get; set; }
        public int QCReqMasterID { get; set; }
        public int LocationId { get; set; }
        public int ReceiveID { get; set; }
        public int CompanyId { get; set; }
        public int RCompanyId { get; set; }
        public int SupplierId { get; set; }
        public int SpinnerId { get; set; }
        public bool Approve { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int? ApproveBy { get; set; }
        public bool Reject { get; set; }
        public DateTime? RejectDate { get; set; }
        public int? RejectBy { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        #region Additional Columns

        [Write(false)]
        public string QCIssueByUser { get; set; }
        [Write(false)]
        public string QCReqNo { get; set; }
        [Write(false)]
        public DateTime QCReqDate { get; set; }
        [Write(false)]
        public string QCReqFor { get; set; }
        [Write(false)]
        public string QCReqByUser { get; set; }
        [Write(false)]
        public int ReqQtyCone { get; set; }
        [Write(false)]
        public int IssueQtyCone { get; set; }
        [Write(false)]
        public int IssueQtyCarton { get; set; }
        [Write(false)]
        public string ReceiveNo { get; set; }
        [Write(false)]
        public DateTime? ReceiveDate { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public bool IsMRIRCompleted { get; set; } = false;
        [Write(false)]
        public List<YarnQCIssueChild> YarnQCIssueChilds { get; set; }
        [Write(false)]
        public List<YarnQCReceiveMaster> YarnQCReceiveMasters { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCIssueMasterID > 0;

        #endregion Additional Columns

        public YarnQCIssueMaster()
        {
            DateAdded = DateTime.Now;
            YarnQCIssueChilds = new List<YarnQCIssueChild>();
            YarnQCReceiveMasters = new List<YarnQCReceiveMaster>();
            QCIssueDate = DateTime.Now;
            QCIssueNo = AppConstants.NEW;
            Approve = false;
            Reject = false;
        }
    }
}
