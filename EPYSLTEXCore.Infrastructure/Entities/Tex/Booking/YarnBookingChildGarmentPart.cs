using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.YarnBookingChildGarmentPart_New)]
    public class YarnBookingChildGarmentPart : IDapperBaseEntity
    {
        [ExplicitKey]
        public int YBookingCGPID { get; set; }
        public int YBChildID { get; set; }
        public int FUPartID { get; set; }

        #region Additional Columns 
        //Start :: Default Assign
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        //End :: Default Assign

        [Write(false)]
        public int BookingID { get; set; }
        [Write(false)]
        public int ConsumptionID { get; set; }
        [Write(false)]
        public int ItemMasterID { get; set; }

        [Write(false)]
        public string PartName { get; set; }

        [Write(false)]
        public int IsSaveFlag { get; set; }

        #endregion Additional Columns 
        public YarnBookingChildGarmentPart()
        {
            EntityState = EntityState.Added;
        }
    }
}
