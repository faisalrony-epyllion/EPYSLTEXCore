using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_FBookingAcknowledgeChildYarnSubBrand")]
    public class FBookingAcknowledgeChildYarnSubBrand : DapperBaseEntity
    {
        [ExplicitKey]
        public int BookingCYSubBrandID { get; set; }

        [ExplicitKey]
        public int BookingChildID { get; set; }

        [ExplicitKey]
        public int BookingID { get; set; }
        ///<summary>
        /// ConsumptionID
        ///</summary>
        public int ConsumptionID { get; set; }
        ///<summary>
        /// YarnSubBrandID
        ///</summary>
        public int YarnSubBrandID { get; set; }

        #region Additional Properties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BookingCYSubBrandID > 0;

        #endregion Additional Properties
        public FBookingAcknowledgeChildYarnSubBrand()
        {
            BookingChildID = 0;
            BookingID = 0;
            YarnSubBrandID = 0;
        }
    }
}
