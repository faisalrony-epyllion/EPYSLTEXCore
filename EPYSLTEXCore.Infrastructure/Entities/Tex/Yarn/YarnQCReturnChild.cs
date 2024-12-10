using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table("YarnQCReturnChild")]
    public class YarnQCReturnChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReturnChildID { get; set; } = 0;
        public int QCReturnMasterID { get; set; } = 0;
        public int QCRemarksChildID { get; set; } = 0;
        public int QCReceiveChildID { get; set; } = 0;
        public int ReceiveChildID { get; set; } = 0;

        public int ReceiveQtyCarton { get; set; } = 0;
        public int ReceiveQtyCone { get; set; } = 0;
        public int ReturnQtyCarton { get; set; } = 0;
        public int ReturnQtyCone { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public decimal ReceiveQty { get; set; } = 0;
        public decimal ReturnQty { get; set; } = 0;

        #region Additional Columns
        [Write(false)]
        public string QCReceiveNo { get; set; }
        [Write(false)]
        public string QCIssueNo { get; set; }
        [Write(false)]
        public string ReceiveNo { get; set; }
        [Write(false)]
        public string QCRemarksNo { get; set; }

        [Write(false)]
        public string ChallanLot { get; set; }
        [Write(false)]
        public string PhysicalCount { get; set; }
        [Write(false)]
        public string LotNo { get; set; }
        [Write(false)]
        public string Spinner { get; set; }

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
        public int QCReceiveMasterId { get; set; }
        [Write(false)]
        public string YarnCategory { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || QCReturnChildID > 0;

        #endregion Additional Columns

        public YarnQCReturnChild()
        {
            Uom = "Pcs";
        }
    }
}
