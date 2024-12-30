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
    [Table(TableNames.YARN_DYEING_BOOKING_CHILD_TWISTING_USES_IN)]
    public class YDBookingChildTwistingUsesIn : DapperBaseEntity
    {
        public YDBookingChildTwistingUsesIn()
        {
            YDBCTwistingUsesInID = 0;
            YDBCTwistingID = 0;
            YDBookingMasterID = 0;
            UsesIn = 0;
        }

        [ExplicitKey]
        public int YDBCTwistingUsesInID { get; set; }
        public int YDBCTwistingID { get; set; }
        public int YDBookingMasterID { get; set; }
        public int UsesIn { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDBCTwistingUsesInID > 0;
        [Write(false)]
        public string UsesInName { get; set; }
    }
}
