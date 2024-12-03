using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_BulkBookingDyedYarnUtilization")]
    public class BulkBookingDyedYarnUtilization : DapperBaseEntity
    {
        #region properties
        [ExplicitKey]
        public int BBDyedYarnUtilizationID { get; set; }
        public int YBChildItemID { get; set; } = 0;
        public int ExportOrderID { get; set; } = 0;
        public int BuyerID { get; set; } = 0;
        public int ColorID { get; set; } = 0;
        public string PhysicalCount { get; set; } = "";
        public string ColorName { get; set; } = "";
        public decimal DyedYarnUtilizationQty { get; set; } = 0M;
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }

        #endregion properties

        #region Additional properties
        [Write(false)]
        public int YBookingID { get; set; } = 0;
        [Write(false)]
        public int YBChildID { get; set; } = 0;
        [Write(false)]
        public int SubGroupID { get; set; } = 0;
        [Write(false)]
        public int ItemMasterId { get; set; } = 0;
        [Write(false)]
        public string ExportOrderNo { get; set; } = "";
        [Write(false)]
        public string Buyer { get; set; } = "";
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BBDyedYarnUtilizationID > 0;
        #endregion Additional properties
    }
}
