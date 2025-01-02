using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Knitting
{
    public class KnittingPlanBookingChildDTO
    {
        /// <summary>
        /// KnittingPlanChild Id
        /// </summary>
        public int Id { get; set; }
        public int YBookingID { get; set; }
        public int ConsumptionID { get; set; }
        public int BookingID { get; set; }
        public int SubGroupID { get; set; }
        public string BookingNo { get; set; }
        public string RevisionNo { get; set; }
        public DateTime BookingDate { get; set; }
        public string SubGroupName { get; set; }
        public string BookingQty { get; set; }
        public string Unit { get; set; }
        public int TotalRows { get; set; }
    }
}
