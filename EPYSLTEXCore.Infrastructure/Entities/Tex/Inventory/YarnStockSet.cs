using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory
{
    
    [Table(TableNames.YarnStockSet)]
    public class YarnStockSet : DapperBaseEntity
    {
        [ExplicitKey]
        public int YarnStockSetId { get; set; } = 0;

        public int ItemMasterId { get; set; } = 0;

        public int SupplierId { get; set; } = 0;
                          
        public int SpinnerId { get; set; } = 0;

        public string YarnLotNo { get; set; } = "";

        public string ShadeCode { get; set; } = "";
        public string PhysicalCount { get; set; } = "";
        public string YarnCategory { get; set; } = "";
        public DateTime YarnApprovedDate { get; set; }
        public int SerialNo { get; set; } = 0;

        public int EnumFromMenuType { get; set; } = 0;

        public int SerialNo_SS { get; set; } = 0;

        public bool IsInvalidItem { get; set; } = false;

        public string Note { get; set; } = "";

        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YarnStockSetId > 0;

    }
}
