using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table(TableNames.YARN_QC_REMARKS_CHILD)]
    public class YarnQCRemarksChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCRemarksChildID { get; set; }
        public int QCRemarksMasterId { get; set; }
        public string LotNo { get; set; }
        public int ItemMasterID { get; set; }
        public int UnitID { get; set; }
        public decimal ReceiveQty { get; set; }
        public int ReceiveQtyCarton { get; set; }
        public int ReceiveQtyCone { get; set; }
        public decimal Rate { get; set; }
        public string Remarks { get; set; }
        public int YarnProgramId { get; set; }
        public string ChallanCount { get; set; }
        public string POCount { get; set; }
        public string PhysicalCount { get; set; }
        public string YarnCategory { get; set; }
        public int NoOfThread { get; set; }
        public bool Approve { get; set; }
        public int ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public bool Reject { get; set; }
        public int RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public bool ReTest { get; set; }
        public int ReTestBy { get; set; }
        public DateTime? ReTestDate { get; set; }
        public bool Diagnostic { get; set; }
        public int DiagnosticBy { get; set; }
        public DateTime? DiagnosticDate { get; set; }
        public bool CommerciallyApprove { get; set; }
        public int CommerciallyApproveBy { get; set; }
        public DateTime? CommerciallyApproveDate { get; set; }
        public string ShadeCode { get; set; }
        public int QCReceiveChildID { get; set; }
        public int YarnStatusID { get; set; }
        public int ReceiveChildID { get; set; }
        public string ChallanLot { get; set; }
        public int TechnicalNameID { get; set; }

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
        public DateTime QCRemarksDate { get; set; }
        [Write(false)]
        public int QCReqMasterId { get; set; }
        [Write(false)]
        public int QCIssueMasterId { get; set; }
        [Write(false)]
        public int QCReceiveMasterId { get; set; }

        [Write(false)]
        public int LocationId { get; set; }
        [Write(false)]
        public int CompanyId { get; set; }
        [Write(false)]
        public int RCompanyId { get; set; }
        [Write(false)]
        public int SupplierId { get; set; }
        [Write(false)]
        public int SpinnerId { get; set; }
        [Write(false)]
        public string id { get; set; }
        [Write(false)]
        public string text { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public string PhysicalLot { get; set; }
        [Write(false)]
        public string YarnDetail { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public DateTime ReceiveDate { get; set; }
        [Write(false)]
        public int NoOfCartoon { get; set; }
        [Write(false)]
        public int NoOfCone { get; set; }
        [Write(false)]
        public decimal ReceiveQtyYS { get; set; }
        [Write(false)]
        public List<YarnQCRemarksChildResult> YarnQCRemarksChildResults { get; set; }
        [Write(false)]
        public List<YarnQCRemarksChildFiber> YarnQCRemarksChildFibers { get; set; }

        [Write(false)]


        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || QCRemarksChildID > 0;

        #endregion Additional Columns

        public YarnQCRemarksChild()
        {
            UnitID = 0;
            ItemMasterID = 0;
            ReTest = false;
            ReTestBy = 0;
            QCReceiveChildID = 0;
            Approve = false;
            ApproveBy = 0;
            Reject = false;
            RejectBy = 0;
            Diagnostic = false;
            DiagnosticBy = 0;
            ReceiveChildID = 0;
            YarnQCRemarksChildResults = new List<YarnQCRemarksChildResult>();
            YarnQCRemarksChildFibers = new List<YarnQCRemarksChildFiber>();
            TechnicalNameID = 0;

            NoOfCartoon = 0;
            NoOfCone = 0;
            ReceiveQtyYS = 0;
        }
    }
}
