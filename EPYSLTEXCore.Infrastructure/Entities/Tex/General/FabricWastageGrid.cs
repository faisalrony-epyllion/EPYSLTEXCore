using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table(TableNames.FABRIC_WASTAGE_GRID)]
    public class FabricWastageGrid : DapperBaseEntity
    {
        [ExplicitKey]
        public int FWGID { get; set; }

        public string WastageFor { get; set; }
        public bool IsFabric { get; set; }
        public int GSMFrom { get; set; }
        public int GSMTo { get; set; }
        public decimal BookingQtyFrom { get; set; }
        public decimal BookingQtyTo { get; set; }
        public bool FixedQty { get; set; }
        public decimal ExcessQty { get; set; }
        public decimal ExcessPercentage { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.FWGID > 0;
    }
}
