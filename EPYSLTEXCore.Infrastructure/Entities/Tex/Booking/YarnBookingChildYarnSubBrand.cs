using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.YarnBookingChildYarnSubBrand_New)]
    public class YarnBookingChildYarnSubBrand : IDapperBaseEntity
    {
        [ExplicitKey]
        public int YBookingCYSubBrandID { get; set; }
        public int YBChildID { get; set; }
        public int YarnSubBrandID { get; set; }

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
        public string YarnSubBrandName { get; set; }
        [Write(false)]
        public int IsSaveFlag { get; set; }


        [Write(false)]
        public int YBookingID { get; set; } = 0;




        #endregion Additional Columns
        public YarnBookingChildYarnSubBrand()
        {
            EntityState = EntityState.Added;
        }
    }
}
