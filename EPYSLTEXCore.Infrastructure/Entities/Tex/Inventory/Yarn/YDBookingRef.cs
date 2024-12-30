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
    [Table(TableNames.YDBookingChild)]
    public class YDBookingRef : DapperBaseEntity
    {
        [ExplicitKey]
        public int YaDBookingRefID { get; set; }

        public int YDBookingMasterID { get; set; }

        public int BookingID { get; set; }

        public string BookingNo { get; set; }

        public int ExportOrderID { get; set; }

        #region Additional Fields

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YaDBookingRefID > 0;

        #endregion Additional Fields
    }
}
