using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_FBookingAcknowledgementLiabilityDistribution")]
    public class FBookingAcknowledgementLiabilityDistribution : DapperBaseEntity
    {
        [ExplicitKey]
        public int LChildID { get; set; }
        public int BookingChildID { get; set; }
        public int ConsumptionID { get; set; } = 0;
        public int AcknowledgeID { get; set; }
        public int BookingID { get; set; }
        public int UnitID { get; set; }
        public int LiabilitiesProcessID { get; set; }
        public decimal LiabilityQty { get; set; }
        public decimal ConsumedQty { get; set; }
        public decimal SuggestedLiabilityQty { get; set; }
        public decimal Rate { get; set; }

        #region Additional Properties
        [Write(false)]
        public string LiabilitiesName { get; set; } = "";
        [Write(false)]
        public string SubGroupName { get; set; } = "";
        [Write(false)]
        public string UOM { get; set; } = "";
        [Write(false)]
        public decimal TotalValue { get; set; } = 0;
        [Write(false)]
        public decimal MaxFinishQtyLiability { get; set; } = 0;
        [Write(false)]
        public decimal TillLiabilityQty => this.LiabilityQty - this.ConsumedQty;
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LChildID > 0;

        #endregion Additional Properties
        public FBookingAcknowledgementLiabilityDistribution()
        {
            LChildID = 0;
            BookingChildID = 0;
            BookingID = 0;
            LiabilitiesProcessID = 0;
            LiabilityQty = 0;
            ConsumedQty = 0;
            SuggestedLiabilityQty = 0;
            Rate = 0;
            AcknowledgeID = 0;
            UnitID = 0;
        }
        public FBookingAcknowledgementLiabilityDistribution(FBookingAcknowledgementLiabilityDistribution x)
        {
            AcknowledgeID = 0;
            BookingChildID = 0;
            BookingID = 0;
            LChildID = 0;
            LiabilityQty = 0;
            ConsumedQty = 0;
            SuggestedLiabilityQty = 0;
            Rate = 0;
            LiabilitiesProcessID = x.LiabilitiesProcessID;
            LiabilitiesName = x.LiabilitiesName;
        }
    }
}
