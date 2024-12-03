using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.YarnBookingChildItemYarnSubBrand_New)]
    public class YarnBookingChildItemYarnSubBrand : IDapperBaseEntity
    {
        [ExplicitKey]
        public int YBCItemYSubBrandID { get; set; }
        public int YBChildItemID { get; set; }
        public int YarnSubBrandID { get; set; }

        #region Additional Fields
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public int TotalRows { get; set; }
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public string DisplayUnitDesc { get; set; }

        [Write(false)]
        public int YBChildID { get; set; }
        [Write(false)]
        public int YBookingID { get; set; }
        [Write(false)]
        public string YarnSubBrandName { get; set; }
        [Write(false)]
        public int[] YarnSubBrandIDs { get; set; }
        [Write(false)]
        public int YItemMasterID { get; set; }
        [Write(false)]
        public int IsSaveFlag { get; set; }

        #endregion Additional Fields

        public YarnBookingChildItemYarnSubBrand()
        {
            EntityState = EntityState.Added;
        }
    }
}
