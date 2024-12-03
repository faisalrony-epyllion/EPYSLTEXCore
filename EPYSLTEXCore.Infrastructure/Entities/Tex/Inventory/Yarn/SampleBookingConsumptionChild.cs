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
    [Table(TableNames.SAMPLE_BOOKING_CONSUMPTION_CHILD)]
    public class SampleBookingConsumptionChild : DapperBaseEntity
    {
        public SampleBookingConsumptionChild()
        {

        }
        [ExplicitKey]
        public int ConsumptionChildID { get; set; }

        public int ConsumptionID { get; set; }

        public int BookingID { get; set; }

        public int ItemGroupID { get; set; }

        public int SubGroupID { get; set; }

        public int ItemMasterID { get; set; }

        public decimal RequiredQty { get; set; }

        public decimal ConsumptionQty { get; set; }

        public int RequiredUnitID { get; set; }

        public decimal MaxRequiredQty { get; set; }

        public int StatusID { get; set; }

        #region Additional Columns

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.ConsumptionChildID > 0;

        #endregion Additional Columns
    }
}
