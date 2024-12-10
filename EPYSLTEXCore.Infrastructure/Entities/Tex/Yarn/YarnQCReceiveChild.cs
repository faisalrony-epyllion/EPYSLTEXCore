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
    [Table(TableNames.YARN_QC_RECEIVE_CHILD)]
    public class YarnQCReceiveChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReceiveChildID { get; set; }
        public int QCReceiveMasterID { get; set; }
        public string LotNo { get; set; }
        public int ItemMasterID { get; set; }
        public decimal ReqQty { get; set; }
        public int ReqQtyCarton { get; set; }
        public int ReqQtyCone { get; set; }
        public int UnitID { get; set; }
        public decimal IssueQty { get; set; }
        public int IssueQtyCarton { get; set; }
        public int IssueQtyCone { get; set; }
        public decimal ReceiveQty { get; set; }
        public int ReceiveQtyCarton { get; set; }
        public int ReceiveQtyCone { get; set; }
        public decimal Rate { get; set; }
        public int YarnProgramId { get; set; }
        public string ChallanCount { get; set; }
        public string POCount { get; set; }
        public string PhysicalCount { get; set; }
        public string YarnCategory { get; set; }
        public int NoOfThread { get; set; }
        public string ShadeCode { get; set; }
        public int QCIssueChildID { get; set; }
        public int ReceiveChildID { get; set; }
        public string ChallanLot { get; set; }


        #region AdditionalFields

        [Write(false)]
        public string Uom { get; set; }

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
        public string YarnColor { get; set; }
        [Write(false)]
        public int MachineTypeId { get; set; }
        [Write(false)]
        public int TechnicalNameId { get; set; }
        [Write(false)]
        public int BuyerID { get; set; }
        [Write(false)]
        public string MachineType { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        public string QCReqRemarks { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCReceiveChildID > 0;
        #endregion Additional Columns
        public YarnQCReceiveChild()
        {
            UnitID = 1;
            Uom = "Pcs";
            MachineTypeId = 0;
            TechnicalNameId = 0;
            BuyerID = 0;
            MachineType = "";
            TechnicalName = "";
            QCIssueChildID = 0;
            ReceiveChildID = 0;
            ChallanLot = "";

            QCReqRemarks = "";
            POCount = "";
        }
    }
}
