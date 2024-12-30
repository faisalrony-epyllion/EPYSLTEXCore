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
    [Table(TableNames.YD_BOOKING_CHILD_USES_IN)]
    public class YDBookingChildUsesIn : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDBCUsesInID { get; set; }

        public int YDBookingChildID { get; set; }

        public int YDBookingMasterID { get; set; }

        public int UsesIn { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDBCUsesInID > 0;

        [Write(false)]
        public string UsesInName { get; set; }
    }
}
