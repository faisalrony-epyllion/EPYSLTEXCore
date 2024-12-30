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
    [Table(TableNames.YARN_DYEING_BOOKING_CHILD_TWISTING_COLOR)]
    public class YDBookingChildTwistingColor : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDBCTwistingColorID { get; set; }

        public int YDBCTwistingID { get; set; }

        public int YDBookingMasterID { get; set; }

        public int YDBookingChildID { get; set; }

        public int ColorID { get; set; }

        public string ColorCode { get; set; }

        public decimal TwistingColorQty { get; set; }


        #region Additional Property
        [Write(false)]
        public int PrimaryTwistingColorID { get; set; }
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
        public string ColorName { get; set; }
        [Write(false)]
        public int ItemMasterID { get; set; }
        [Write(false)]
        public int FCMRChildID { get; set; }
        [Write(false)]
        public string PhysicalCount { get; set; }
        [Write(false)]
        public string LotNo { get; set; }
        [Write(false)]
        public int BookingFor { get; set; }
        [Write(false)]
        public string BookingForName { get; set; }
        [Write(false)]
        public decimal AssignQty { get; set; }
        [Write(false)]
        public int TPI { get; set; }
        [Write(false)]
        public decimal BookingQty { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDBCTwistingColorID > 0;

        #endregion Additional Property
    }
}
