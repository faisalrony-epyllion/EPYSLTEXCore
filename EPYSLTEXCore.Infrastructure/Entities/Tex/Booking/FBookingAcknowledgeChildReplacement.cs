using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.FBookingAcknowledgeChildReplacement)]
    public class FBookingAcknowledgeChildReplacement : DapperBaseEntity
    {
        #region Table Propertise
        [ExplicitKey]
        public int ReplacementID { get; set; }

        public int YBChildID { get; set; } = 0;

        public int BookingChildID { get; set; } = 0;

        public int ReasonID { get; set; } = 0;

        public int DepertmentID { get; set; } = 0;
        public decimal ReplacementQTY { get; set; } = 0M;

        public int AddedBy { get; set; } = 0;

        public DateTime DateAdded { get; set; }

        public int UpdatedBy { get; set; } = 0;

        public DateTime? DateUpdated { get; set; }

        public string Remarks { get; set; } = "";

        #endregion Table Propertise


        #region Additional Info

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ReplacementID > 0;
        #endregion
    }
}
