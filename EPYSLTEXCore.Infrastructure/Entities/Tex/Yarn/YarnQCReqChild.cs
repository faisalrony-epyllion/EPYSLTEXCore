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
    [Table(TableNames.YARN_QC_REQ_CHILD)]
    public class YarnQCReqChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReqChildID { get; set; }
        public int QCReqMasterID { get; set; }
        public int ReceiveID { get; set; }
        public string ReceiveNo { get; set; }
        public string LotNo { get; set; }
        public int ItemMasterID { get; set; }
        public decimal ReqQty { get; set; }
        public decimal ReqCone { get; set; }
        public int UnitID { get; set; }
        public decimal Rate { get; set; }
        public int YarnProgramId { get; set; }
        public string ChallanCount { get; set; }
        public string POCount { get; set; }
        public string PhysicalCount { get; set; }
        public string YarnCategory { get; set; }
        public int NoOfThread { get; set; }
        public string ShadeCode { get; set; }
        public int ReceiveChildID { get; set; }
        public int MachineTypeId { get; set; }
        public int TechnicalNameId { get; set; }
        public int BuyerID { get; set; }
        public int ReqBagPcs { get; set; }
        public string ChallanLot { get; set; }
        public string QCReqRemarks { get; set; }

        #region Additional Columns

        [Write(false)]
        public decimal ReceiveQty { get; set; }
        [Write(false)]
        public int ReqQtyCone { get; set; }
        [Write(false)]
        public int ReqQtyCarton { get; set; }
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
        public string SupplierName { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string ImportCategory { get; set; }
        [Write(false)]
        public string EWO { get; set; }
        [Write(false)]
        public string POFor { get; set; }
        [Write(false)]
        public string PhysicalLot { get; set; }
        [Write(false)]
        public string YarnDetail { get; set; }
        [Write(false)]
        public decimal ReceivedQtyInKg { get; set; }
        [Write(false)]
        public DateTime ReceiveDate { get; set; }
        [Write(false)]
        public string MachineType { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public int NoOfCartoon { get; set; }
        [Write(false)]
        public int NoOfCone { get; set; }
        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public bool HasPrevQCReq { get; set; } = false;
        [Write(false)]
        public string TagWithPrevReq { get; set; } = "Tag";
        [Write(false)]
        public string QCReqNo { get; set; } = "";
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCReqChildID > 0;

        #endregion Additional Columns
        public YarnQCReqChild()
        {
            UnitID = 1;
            Uom = "Pcs";
            ReqQty = 0;
            ReqCone = 0;
            ReceiveChildID = 0;
            ReqBagPcs = 0;
            MachineTypeId = 0;
            TechnicalNameId = 0;
            BuyerID = 0;
            MachineType = "";
            TechnicalName = "";
        }
    }
}
