using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking
{
    [Table(TableNames.BOOKING_ITEM_ACKNOWLEDGE)]
    public class BookingItemAcknowledge : DapperBaseEntity
    {
        #region Table properties

        [ExplicitKey]
        public int AcknowledgeID { get; set; }

        public int PreProcessRevNo { get; set; }

        public int RevisionNo { get; set; }

        public int BookingID { get; set; }

        public int BOMMasterID { get; set; }

        public int ItemGroupID { get; set; }

        public int SubGroupID { get; set; }

        public bool Status { get; set; }

        public DateTime? AcknowledgeDate { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        public bool WithoutOB { get; set; }
        public int UnAcknowledgeBy { get; set; }
        public DateTime? UnAcknowledgeDate { get; set; }
        public bool UnAcknowledge { get; set; }

        #endregion Table properties

        #region Additional Columns

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || AcknowledgeID > 0;

        #endregion Additional Columns

        public BookingItemAcknowledge()
        {
            UnAcknowledgeBy = 0;
            UnAcknowledge = false;
        }

    }
}
