using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Knitting
{
    public class KnittingPlanBookingChildDetailsDTO
    {
        /// <summary>
        /// Knitting Plan Master Id
        /// </summary>
        public int Id { get; set; }
        public int YBookingID { get; set; }
        public int ConsumptionID { get; set; }
        public int SubGroupID { get; set; }
        public int BookingID { get; set; }
        public int ItemMasterID { get; set; }
        public decimal BookingQty { get; set; }
        public string Unit { get; set; }
        public string ItemName { get; set; }
        public bool KnittingPlanned { get; set; }
        public int TotalRows { get; set; }
    }
}