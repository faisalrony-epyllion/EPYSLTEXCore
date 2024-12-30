using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YARN_DYEING_BOOKING_CHILD_TWISTING)]
    public class YDBookingChildTwisting : YarnItemMaster, IDapperBaseEntity
    {
        public YDBookingChildTwisting()
        {
            EntityState = EntityState.Added;
            TPI = 0;
            YDBookingChildTwistingColors = new List<YDBookingChildTwistingColor>();
            YDBCTwistingUsesIns = new List<YDBookingChildTwistingUsesIn>();
        }

        #region Table Properties

        [ExplicitKey]
        public int YDBCTwistingID { get; set; }

        public int YDBookingMasterID { get; set; }

        public string Remarks { get; set; }

        public string YarnCategory { get; set; }

        public int NoOfThread { get; set; }

        public int YarnDyedColorID { get; set; }

        public decimal BookingQty { get; set; }

        public int YarnProgramID { get; set; }

        public int FCMRChildID { get; set; }

        public int YBChildItemID { get; set; }

        public string ProgramName { get; set; }

        public int ColorID { get; set; }

        public string ColorCode { get; set; }

        public string TwistedColor { get; set; }

        public bool RequestRecipe { get; set; }

        public int? RequestBy { get; set; }

        public System.DateTime? RequestDate { get; set; }

        public bool RequestAck { get; set; }

        public int? RequestAckBy { get; set; }

        public System.DateTime? RequestAckDate { get; set; }

        public int DPID { get; set; }

        public string DPProcessInfo { get; set; }

        public int NoOfCone { get; set; }

        public bool IsTwisting { get; set; }

        public bool IsWaxing { get; set; }

        public int UsesIn { get; set; }

        public string ShadeCode { get; set; }

        public string PhysicalCount { get; set; }

        public string PrintedDensity { get; set; }

        public decimal TPI { get; set; }

        #endregion Table Properties

        #region Additional Properties

        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public string DisplayUnitDesc { get; set; }

        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string ColorIDs { get; set; }
        [Write(false)]
        public string TwistedColors { get; set; }
        [Write(false)]
        public string UsesInIDs { get; set; }
        [Write(false)]
        public string UsesIns { get; set; }
        [Write(false)]
        public string TwistedSelectedColorIDs { get; set; }
        [Write(false)]
        public int YDProductionMasterID { get; set; }
        [Write(false)]
        public int YDBookingChildID { get; set; }
        [Write(false)]
        public List<YDBookingChildTwistingColor> YDBookingChildTwistingColors { get; set; }
        [Write(false)]
        public List<YDBookingChildTwistingColor> TwistingList { get; set; }
        [Write(false)]
        public List<YDBookingChildTwistingUsesIn> YDBCTwistingUsesIns { get; set; }
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        #endregion Additional Properties
    }
}
