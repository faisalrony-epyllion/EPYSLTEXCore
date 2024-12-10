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
    [Table(TableNames.YARN_QC_RETURN_MASTER)]
    public class YarnQCReturnMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReturnMasterID { get; set; } = 0;
        public string QCReturnNo { get; set; }= AppConstants.NEW;
        public int QCReturnBy { get; set; } = 0;
        public DateTime QCReturnDate { get; set; }
        public int QCReqMasterID { get; set; } = 0;
        public int QCRemarksMasterID { get; set; } = 0;
        public bool IsApprove { get; set; } = false;
        public DateTime? ApproveDate { get; set; }
        public int ApproveBy { get; set; } = 0;
        public bool IsAcknowledge { get; set; } = false;
        public DateTime? AcknowledgeDate { get; set; }
        public int AcknowledgeBy { get; set; } = 0;
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }

        #region Additional Columns
        [Write(false)]
        public int QCReceiveChildID { get; set; } = 0;
        [Write(false)]
        public string YarnCategory { get; set; } = "";
        [Write(false)]
        public string PhysicalCount { get; set; } = "";
        [Write(false)]
        public string LotNo { get; set; } = "";
        [Write(false)]
        public string Spinner { get; set; } = "";
        [Write(false)]
        public string ShadeCode { get; set; } = "";
        [Write(false)]
        public string QCIssueNo { get; set; } = "";


        [Write(false)]
        public string QCReqFor { get; set; } = "";
        [Write(false)]
        public DateTime QCReceiveDate { get; set; }
        [Write(false)]
        public string QCReceiveNo { get; set; } = "";

        [Write(false)]
        public string QCReqNo { get; set; } = "";
        [Write(false)]
        public DateTime QCReqDate { get; set; }
        [Write(false)]
        public int QCReqQty { get; set; } = 0;
        [Write(false)]
        public int QCReturnQty { get; set; } = 0;
        [Write(false)]
        public string QCReturnByUser { get; set; } = "";
        [Write(false)]
        public int ReceiveQtyCarton { get; set; } = 0;
        [Write(false)]
        public int ReceiveQtyCone { get; set; } = 0;
        [Write(false)]
        public int ReturnQtyCarton { get; set; } = 0;
        [Write(false)]
        public int ReturnQtyCone { get; set; } = 0;
        [Write(false)]
        public DateTime? QCReturnReceivedDate { get; set; }
        [Write(false)]
        public string QCRemarksNo { get; set; } = "";
        [Write(false)]
        public DateTime? QCRemarksDate { get; set; }
        [Write(false)]
        public int QCReceiveMasterID { get; set; } = 0;
        [Write(false)]
        public string ReceiveNo { get; set; } = "";
        [Write(false)]
        public List<YarnQCReturnChild> YarnQCReturnChilds { get; set; } = new List<YarnQCReturnChild>();

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || QCReturnMasterID > 0;

        #endregion Additional Columns
    }
}
