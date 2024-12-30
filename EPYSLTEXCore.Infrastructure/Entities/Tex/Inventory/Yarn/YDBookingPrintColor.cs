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
    [Table(TableNames.YD_BOOKING_PRINT_COLOR)]
    public class YDBookingPrintColor : DapperBaseEntity
    {
        [ExplicitKey]
        public int PrintColorID { get; set; }

        public int ColorID { get; set; }
        public string ColorCode { get; set; }
        public bool IsMajor { get; set; }
        public int YDBookingChildID { get; set; }
        public int YDBookingMasterID { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == EntityState.Modified || PrintColorID > 0;
    }
}
