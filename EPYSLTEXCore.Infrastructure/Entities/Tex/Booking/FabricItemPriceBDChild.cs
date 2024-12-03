using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    //[Table("FabricItemPriceBD")]
    [Table(TableNames.FabricItemPriceBDChild)]
    public class FabricItemPriceBDChild : DapperBaseEntity
    {
        #region Table properties
        [ExplicitKey]
        public int BDChildID { get; set; }
        public int BDID { get; set; } = 0;
        public int YBookingID { get; set; } = 0;
        public int CostID { get; set; } = 0;
        public decimal Value { get; set; } = 0;
        public decimal Cost { get; set; } = 0;
        public int SpecialCostID { get; set; } = 0;

        #endregion Table properties

        #region Additional Columns
        [Write(false)]
        public string CostName { get; set; } = "";
        [Write(false)]
        public string PercentSymbol { get; set; } = "%";
        [Write(false)]
        public int ItemMasterID { get; set; } = 0;

        [Write(false)]
        public int ConsumptionID { get; set; } = 0;
        [Write(false)]
        public decimal FixedValue { get; set; } = 0;
        [Write(false)]
        public int FromValue { get; set; } = 0;
        [Write(false)]
        public int ToValue { get; set; } = 0;
        [Write(false)]
        public string SpecialCostName { get; set; } = "";
        [Write(false)]
        public string SetupName { get; set; } = "";
        [Write(false)]
        public string ProcessType { get; set; } = "";
        [Write(false)]
        public bool IsBuried { get; set; } = false;
        [Write(false)]
        public bool ApplyFixed { get; set; } = false;
        [Write(false)]
        public bool IsCalculated { get; set; } = false;
        [Write(false)]
        public bool HasChild { get; set; } = false;
        [Write(false)]
        public bool HasParent { get; set; } = false;
        [Write(false)]
        public bool IsMarkup { get; set; } = false;
        [Write(false)]
        public bool IsAPSelected { get; set; } = false;
        [Write(false)]
        public bool HasSpecialType { get; set; } = false;

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BDChildID > 0;


        #endregion Additional Columns
    }
}
