using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_Left_Over_Return_CHILD)]
    public class YDLeftOverReturnChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDLOReturnChildID { get; set; }

        public int YDLOReturnMasterID { get; set; } = 0;
        public decimal IssueQty { get; set; } = 0;
        public decimal IssueQtyCarton { get; set; } = 0;
        public int IssueCone { get; set; } = 0;
        public decimal UseableReturnQtyKG { get; set; } = 0;
        public int UseableReturnQtyCone { get; set; } = 0;
        public int UseableReturnQtyBag { get; set; } = 0;
        public decimal UnuseableReturnQtyKG { get; set; } = 0;
        public int UnuseableReturnQtyCone { get; set; } = 0;
        public int UnuseableReturnQtyBag { get; set; } = 0;
        public int UnitID { get; set; } = 0;
        public int ItemMasterID { get; set; } = 0;
        public string Remarks { get; set; }

        public string YarnCategory { get; set; }

        public int NoOfThread { get; set; } = 0;

        public string LotNo { get; set; }

        public int YarnProgramID { get; set; } = 0;
        public int YDReqIssueChildID { get; set; } = 0;

        public decimal Rate { get; set; } = 0;

        #region Additional

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public string YarnType { get; set; }

        [Write(false)]
        public string YarnCount { get; set; }

        [Write(false)]
        public string YarnComposition { get; set; }

        [Write(false)]
        public string Shade { get; set; }

        [Write(false)]
        public string YarnColor { get; set; }
        [Write(false)]
        public int YDReqIssueMasterID { get; set; } = 0;
        [Write(false)]
        public string YDReqIssueNo { get; set; }
        [Write(false)]
        public string YarnDetails { get; set; }
        [Write(false)]
        public string PhysicalCount { get; set; }

        [Write(false)]
        public string BatchNo { get; set; }
        [Write(false)]
        public string BookingNo { get; set; }
        [Write(false)]
        public string Floor { get; set; }
        [Write(false)]
        public string ExportOrderNo { get; set; }
        [Write(false)]
        public decimal BalanceQuantity { get; set; } = 0;
        [Write(false)]
        public int YDBookingMasterID { get; set; } = 0;
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YDLOReturnChildID > 0;
        #endregion Additional

        public YDLeftOverReturnChild()
        {
            NoOfThread = 0;
            UnitID = 28;
            Uom = "Kg";
        }
    }
}
