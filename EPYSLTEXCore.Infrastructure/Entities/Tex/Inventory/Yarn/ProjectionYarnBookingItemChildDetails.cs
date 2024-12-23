﻿using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.PROJECTION_YARN_BOOKING_ITEM_CHILDDETAILS)]
    public class ProjectionYarnBookingItemChildDetails : DapperBaseEntity
    {
        [ExplicitKey]
        public int PYBBookingChildDetailsID { get; set; } = 0;
        public int PYBBookingChildID { get; set; } = 0;
        public int PYBookingID { get; set; } = 0;
        public DateTime BookingDate { get; set; }
        public decimal DetailsQTY { get; set; } = 0;


        #region Additional Columns 

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.PYBBookingChildDetailsID > 0;

        #endregion Additional Columns 
    }
}
