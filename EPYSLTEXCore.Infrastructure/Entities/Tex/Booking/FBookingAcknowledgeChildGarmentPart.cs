using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_FBookingAcknowledgeChildGarmentPart")]
    public class FBookingAcknowledgeChildGarmentPart : DapperBaseEntity
    {

        [ExplicitKey]
        public int BookingCGPID { get; set; }

        [ExplicitKey]
        public int BookingChildID { get; set; }

        [ExplicitKey]
        public int BookingID { get; set; }
        ///<summary>
        /// ConsumptionID
        ///</summary>
        public int ConsumptionID { get; set; }
        ///<summary>
        /// FUPartID
        ///</summary>
        public int FUPartID { get; set; }

        #region Additional Properties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BookingCGPID > 0;

        #endregion Additional Properties
        public FBookingAcknowledgeChildGarmentPart()
        {
            BookingChildID = 0;
            BookingID = 0;
            FUPartID = 0;
        }
    }
}
