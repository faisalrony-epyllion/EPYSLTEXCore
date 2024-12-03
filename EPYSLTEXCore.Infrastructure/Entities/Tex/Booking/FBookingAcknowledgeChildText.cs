using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_FBookingAcknowledgeChildText")]
    public class FBookingAcknowledgeChildText : DapperBaseEntity
    {
        [ExplicitKey]
        public int TextID { get; set; }

        [ExplicitKey]
        public int BookingChildID { get; set; }

        [ExplicitKey]
        public int BookingID { get; set; }
        ///<summary>
        /// ConsumptionID
        ///</summary>
        public int ConsumptionID { get; set; }
        ///<summary>
        /// UsesIn (length: 5000)
        ///</summary>
        public string UsesIn { get; set; }

        ///<summary>
        /// AdditionalProcess (length: 5000)
        ///</summary>
        public string AdditionalProcess { get; set; }

        ///<summary>
        /// ApplicableProcess (length: 5000)
        ///</summary>
        public string ApplicableProcess { get; set; }

        ///<summary>
        /// GmtColor (length: 5000)
        ///</summary>
        public string GmtColor { get; set; }
        public string WashType { get; set; } = "";
        public string RepeatColor { get; set; } = "";

        ///<summary>
        /// YarnSubProgram (length: 5000)
        ///</summary>
        public string YarnSubProgram { get; set; }
        #region Additional Properties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || TextID > 0;

        #endregion Additional Properties
        public FBookingAcknowledgeChildText()
        {
            BookingChildID = 0;
            BookingID = 0;
        }
    }
}
