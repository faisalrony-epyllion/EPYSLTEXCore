using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_FBookingAcknowledgeImage")]
    public class FBookingAcknowledgeImage : DapperBaseEntity
    {
        [ExplicitKey]
        public int ChildImgID { get; set; }

        ///<summary>
        /// BookingID
        ///</summary>
        public int BookingID { get; set; }

        ///<summary>
        /// ExportOrderID
        ///</summary>
        public int ExportOrderID { get; set; }

        ///<summary>
        /// ImagePath (length: 500)
        ///</summary>
        public string ImagePath { get; set; }

        #region Additional Properties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ChildImgID > 0;

        #endregion Additional Properties
        public FBookingAcknowledgeImage()
        {
            BookingID = 0;
            ExportOrderID = 0;
        }
    }
}
