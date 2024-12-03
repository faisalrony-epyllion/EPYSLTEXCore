using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.YarnBookingReason_New)]
    public class YarnBookingReason : DapperBaseEntity
    {
        [ExplicitKey]
        public int YBKReasonID { get; set; }
        public int YBookingID { get; set; }
        public int RevisionNo { get; set; }
        public int ReasonID { get; set; }
        public DateTime DateAdded { get; set; }

        #region Additional Columns 
        [Write(false)]
        public string ReasonName { get; set; }
        [Write(false)]
        public bool IsAddition { get; set; }
        [Write(false)]
        public bool IsRevision { get; set; }
        [Write(false)]
        public bool UseInBooking { get; set; }
        [Write(false)]
        public bool UseInYBooking { get; set; }
        [Write(false)]
        public string Remarks { get; set; }
        [Write(false)]
        public bool Selected { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YBKReasonID > 0;

        #endregion Additional Columns 

        public YarnBookingReason()
        {
            YBookingID = 0;
            RevisionNo = 0;
            ReasonID = 0;
            DateAdded = DateTime.Now;
        }
    }
}
