using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.BulkBookingGreyYarnUtilization)]
    public class BulkBookingGreyYarnUtilization : DapperBaseEntity
    {
        #region properties
        [ExplicitKey]
        public int BBGreyYarnUtilizationID { get; set; }
        public int YBChildItemID { get; set; } = 0;
        public int YarnStockSetID { get; set; } = 0;
        public decimal UtilizationSampleStock { get; set; } = 0;
        public decimal UtilizationLiabilitiesStock { get; set; } = 0;
        public decimal UtilizationUnusableStock { get; set; } = 0;
        public decimal UtilizationLeftoverStock { get; set; } = 0;
        public decimal TotalUtilization { get; set; } = 0;
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }


        #endregion

        #region Additional properties
        [Write(false)]
        public int YBookingID { get; set; } = 0;
        [Write(false)]
        public int YBChildID { get; set; } = 0;
        [Write(false)]
        public int SubGroupID { get; set; } = 0;
        [Write(false)]
        public int ItemMasterID { get; set; } = 0;

        [Write(false)]
        public int SpinnerID { get; set; } = 0;
        [Write(false)]
        public string Spinner { get; set; } = "";
        [Write(false)]
        public string PhysicalLot { get; set; } = "";
        [Write(false)]
        public string PhysicalCount { get; set; } = "";
        [Write(false)]
        public string Composition { get; set; } = "";
        [Write(false)]
        public string NumaricCount { get; set; } = "";
        [Write(false)]
        public string YarnDetails { get; set; } = "";
        [Write(false)]
        public decimal SampleStockQty { get; set; } = 0M;
        [Write(false)]
        public decimal LiabilitiesStockQty { get; set; } = 0M;
        [Write(false)]
        public decimal UnusableStockQty { get; set; } = 0M;
        [Write(false)]
        public decimal LeftoverStockQty { get; set; } = 0M;

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BBGreyYarnUtilizationID > 0;


        #endregion
    }
}
