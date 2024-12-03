using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_FBookingAcknowledgeChildItemNetReqQTY")]
    public class FBookingAcknowledgeChildItemNetReqQTY : DapperBaseEntity
    {
        #region Table Propertise
        [ExplicitKey]
        public int ReplacementID { get; set; }

        public int YBChildItemID { get; set; } = 0;

        public int ReasonID { get; set; } = 0;

        public int DepertmentID { get; set; } = 0;

        public decimal ReplacementQTY { get; set; } = 0M;

        public string Remarks { get; set; } = "";

        public int AddedBy { get; set; } = 0;

        public DateTime DateAdded { get; set; }

        public int UpdatedBy { get; set; } = 0;

        public DateTime? DateUpdated { get; set; }

        #endregion

        #region Additional Info

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ReplacementID > 0;
        #endregion
    }
}
