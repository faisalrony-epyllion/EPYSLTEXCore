using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YDStoreReceiveChild)]
    public class YDStoreReceiveChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDStoreReceiveChildID { get; set; }
        public int YDStoreReceiveMasterID { get; set; } = 0;
        public int SendToYDStoreChildID { get; set; } = 0;
        public int YDBItemReqID { get; set; } = 0;
        public int ItemMasterID { get; set; } = 0;
        public int CompanyID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public int SpinnerID { get; set; } = 0;
        public int UnitID { get; set; } = 0;
        public string LotNo { get; set; } = "";
        public string PhysicalCount { get; set; } = "";
        public string ShadeCode { get; set; } = "";
        public int LocationID { get; set; } = 0;
        public int BookingID { get; set; } = 0;
        public decimal ReceiveQty { get; set; }
        public int ReceiveCone { get; set; }
        public int ReceiveCarton { get; set; }
        public string Remarks { get; set; }
        public string YarnCategory { get; set; }

        #region Additional Columns
        [Write(false)]
        public string Unit { get; set; }
        [Write(false)]
        public decimal SendQty { get; set; } = 0;
        [Write(false)]
        public int SendConeQty { get; set; } = 0;
        [Write(false)]
        public int SendPacketQty { get; set; } = 0;
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
        public string Segment8ValueDesc { get; set; }
        [Write(false)]
        public string YarnDetail { get; set; } = "";
        [Write(false)]
        public List<YDStoreReceiveChildRackBin> YDStoreReceiveChildRackBins { get; set; } = new List<YDStoreReceiveChildRackBin>();
        [Write(false)]
        public List<YarnReceiveChildSubProgram> SubPrograms { get; set; } = new List<YarnReceiveChildSubProgram>();
        [Write(false)]
        public List<YarnReceiveChildOrder> YarnReceiveChildOrders { get; set; } = new List<YarnReceiveChildOrder>();
        [Write(false)]
        public List<YarnReceiveChildBuyer> YarnReceiveChildBuyers { get; set; } = new List<YarnReceiveChildBuyer>();

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDStoreReceiveChildID > 0;

        #endregion Additional Columns

        public YDStoreReceiveChild()
        {
            ItemMasterID = 0;
            UnitID = 0;
            ReceiveQty = 0;
            ReceiveCone = 0;
            ReceiveCarton = 0;
            Remarks = "";
            YarnCategory = "";

            Unit = "";
        }
    }
}
