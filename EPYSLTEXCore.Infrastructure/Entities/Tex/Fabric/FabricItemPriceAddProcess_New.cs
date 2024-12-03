using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric
{
    [Table("T_FabricItemPriceAddProcess")]
    public class FabricItemPriceAddProcess_New : DapperBaseEntity
    {
        #region Table property
        [ExplicitKey]
        public int BDPChildID { get; set; }

        public int BDID { get; set; } = 0;

        public int ProcessID { get; set; } = 0;

        public decimal Cost { get; set; } = 0;
        public int YBChildID { get; set; } = 0;

        public bool IsTextileERP { get; set; } = true;


        #endregion Table property

        #region Additional Columns
        [Write(false)]
        public int ConsumptionID { get; set; } = 0;
        [Write(false)]
        public int YBookingID { get; set; } = 0;

        [Write(false)]
        public int ItemMasterID { get; set; } = 0;
        [Write(false)]
        public int CostID { get; set; } = 0;
        [Write(false)]
        public decimal FixedValue { get; set; } = 0;
        [Write(false)]
        public string CostName { get; set; } = "";


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BDPChildID > 0;

        #endregion Additional Columns
    }
}
