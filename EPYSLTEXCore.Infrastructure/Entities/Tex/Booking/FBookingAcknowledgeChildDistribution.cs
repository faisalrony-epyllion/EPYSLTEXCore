using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.FBBOOKING_ACKNOWLEDGE_CHILD_DISTRIBUTION)]
    public class FBookingAcknowledgeChildDistribution : DapperBaseEntity
    {
        [ExplicitKey]
        public int DistributionID { get; set; }
        public int BookingChildID { get; set; }
        public int ConsumptionID { get; set; }
        public int BookingID { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal DistributionQty { get; set; }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || DistributionID > 0;
        #endregion
        public FBookingAcknowledgeChildDistribution()
        {
            DistributionID = 0;
            BookingChildID = 0;
            ConsumptionID = 0;
            BookingID = 0;
            DeliveryDate = null;
            DistributionQty = 0;
        }
    }
}
