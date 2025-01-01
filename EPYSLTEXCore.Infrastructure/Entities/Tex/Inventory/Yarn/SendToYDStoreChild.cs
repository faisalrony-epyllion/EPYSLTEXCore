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
    [Table(TableNames.SendToYDStoreChild)]
    public class SendToYDStoreChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int SendToYDStoreChildID { get; set; }
        ///<summary>
        /// YDQCMasterID
        ///</summary>
        public int SendToYDStoreMasterID { get; set; }

        public int YDBItemReqID { get; set; } = 0;

        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// YarnProgramID
        ///</summary>
        public int YarnProgramID { get; set; }

        ///<summary>
        /// BookingQty
        ///</summary>
        public decimal BookingQty { get; set; } = 0;

        ///<summary>
        /// ProducedQty
        ///</summary>
        public decimal SendQty { get; set; } = 0;
        public int SendConeQty { get; set; } = 0;
        public int SendPacketQty { get; set; } = 0;

        ///<summary>
        /// Remarks (length: 200)
        ///</summary>
        public string Remarks { get; set; }
        public int YDQCChildID { get; set; } = 0;
        public int YDRICRBId { get; set; } = 0;

        #region Additional Columns

        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string YarnType { get; set; }
        [Write(false)]
        public string YarnCount { get; set; }
        [Write(false)]
        public string YarnComposition { get; set; }
        [Write(false)]
        public string YarnShade { get; set; }
        [Write(false)]
        public string YarnColor { get; set; }
        [Write(false)]
        public int YDBatchID { get; set; } = 0;
        [Write(false)]
        public int YDBookingChildID { get; set; } = 0;
        [Write(false)]
        public int YDBookingMasterID { get; set; } = 0;
        [Write(false)]
        public string ShadeCode { get; set; } = "";
        [Write(false)]
        public int NoOfThread { get; set; } = 0;
        [Write(false)]
        public int ColorID { get; set; } = 0;
        [Write(false)]
        public string YarnCategory { get; set; } = "";
        [Write(false)]
        public decimal QCQty { get; set; } = 0;
        [Write(false)]
        public int QCCone { get; set; } = 0;
        [Write(false)]
        public int QCPacket { get; set; } = 0;
        [Write(false)]
        public decimal BalanceSendQty { get; set; } = 0;
        [Write(false)]
        public int BalanceSendConeQty { get; set; } = 0;
        [Write(false)]
        public int BalanceSendPacketQty { get; set; } = 0;
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || SendToYDStoreChildID > 0;

        #endregion Additional Columns

        public SendToYDStoreChild()
        {
            BookingQty = 0;
        }
    }
}
