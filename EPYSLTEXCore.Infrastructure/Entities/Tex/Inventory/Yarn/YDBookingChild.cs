using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YDBookingChild)]
    public class YDBookingChild : YarnItemMaster, IDapperBaseEntity
    {
        public YDBookingChild()
        {
            EntityState = EntityState.Added;
            PrintColors = new List<YDBookingPrintColor>();
            YDBookingChildUsesIns = new List<YDBookingChildUsesIn>();
            SpinnerID = 0;
            YD = false;
            YDItem = false;
            AllocationChildItemID = 0;
        }

        #region Table Properties

        [ExplicitKey]
        public int YDBookingChildID { get; set; }

        public int YDBookingMasterID { get; set; }

        public int YarnProgramID { get; set; }

        public string Remarks { get; set; }

        public string YarnCategory { get; set; }
        public int NoOfThread { get; set; }
        public bool YD { get; set; }

        public int YarnDyedColorID { get; set; }

        public decimal BookingQty { get; set; }

        public int NoOfCone { get; set; }
        public decimal PerConeKG { get; set; } = 0;

        public int FCMRChildID { get; set; }

        public int YBChildItemID { get; set; }

        public string ProgramName { get; set; }

        public int ColorID { get; set; }

        public string ColorCode { get; set; }

        public bool RequestRecipe { get; set; }

        public int? RequestBy { get; set; }

        public System.DateTime? RequestDate { get; set; }

        public bool RequestAck { get; set; }

        public int? RequestAckBy { get; set; }

        public System.DateTime? RequestAckDate { get; set; }

        public int DPID { get; set; }

        public string DPProcessInfo { get; set; }

        public int BookingFor { get; set; }

        public bool IsTwisting { get; set; }

        public bool IsWaxing { get; set; }

        public int UsesIn { get; set; }

        public string ShadeCode { get; set; }

        public bool IsAdditionalItem { get; set; }

        public string PrintedDensity { get; set; }

        public int SpinnerID { get; set; }
        public string LotNo { get; set; }
        public string PhysicalCount { get; set; }
        public bool YDItem { get; set; }
        public int AllocationChildItemID { get; set; }
        public string ColorBatchRef { get; set; } = "";
        public int ColorBatchRefID { get; set; } = 0;

        #endregion Table Properties

        #region Additional Items
        [Write(false)]
        public List<YDBookingChildUsesIn> YDBookingChildUsesIns { get; set; }

        [Write(false)]
        public string UsesIns { get; set; }

        [Write(false)]
        public string BookingForName { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string DisplayUnitDesc { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified || YDBookingChildID > 0;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public int BuyerID { get; set; }

        [Write(false)]
        public decimal SavedQty { get; set; }

        [Write(false)]
        public decimal YDBookingQty { get; set; }

        [Write(false)]
        public decimal ReqQty { get; set; }

        [Write(false)]
        public decimal FBookingQty { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public int ExportOrderID { get; set; }

        [Write(false)]
        public List<YDBookingPrintColor> PrintColors { get; set; }

        [Write(false)]
        public string SpinnerName { get; set; }

        [Write(false)]
        public string UsesInName { get; set; }

        [Write(false)]
        public int YDBookingChild_DemoID { get; set; }

        [Write(false)]
        public string PrintColorIDs { get; set; }

        [Write(false)]
        public string UsesInIDs { get; set; }
        [Write(false)]
        public int YDProductionMasterID { get; set; }
        [Write(false)]
        public int YDBCTwistingID { get; set; }
        #endregion Additional Items
    }
}
