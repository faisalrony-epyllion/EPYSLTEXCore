using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_PROCESS)]
    public class FBookingAcknowledgeChildProcess : DapperBaseEntity
    {
        [ExplicitKey]
        public int BookingCProcessID { get; set; }

        [ExplicitKey]
        public int BookingChildID { get; set; }

        [ExplicitKey]
        public int BookingID { get; set; }
        ///<summary>
        /// ConsumptionID
        ///</summary>
        public int ConsumptionID { get; set; }
        ///<summary>
        /// ProcessID
        ///</summary>
        public int ProcessID { get; set; }
        #region Additional Properties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BookingCProcessID > 0;

        #endregion Additional Properties
        public FBookingAcknowledgeChildProcess()
        {
            BookingChildID = 0;
            BookingID = 0;
            ProcessID = 0;
        }
    }
}
