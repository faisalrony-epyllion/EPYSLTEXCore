using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.YarnBookingMaster_New_RevisionReason)]
    public class YarnBookingMaster_New_RevisionReason : DapperBaseEntity
    {
        [ExplicitKey]
        public int YBRReasonID { get; set; }
        public string YBookingNo { get; set; }
        public int ReasonID { get; set; } = 0;
        public string ReasonName { get; set; } = "";


        #region Additional Columns 

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YBRReasonID > 0;


        #endregion Additional Columns
        public YarnBookingMaster_New_RevisionReason()
        {

        }
    }
}
