using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnPOChildOrder)]
    public class YarnPOChildOrder : DapperBaseEntity
    {
        [ExplicitKey]
        public int YPOChildOrderID { get; set; } = 0;

        public int YPOChildID { get; set; } = 0;

        public int YPOMasterID { get; set; } = 0;

        public int ExportOrderId { get; set; } = 0;

        public string EWONo { get; set; } = "";

        public int BuyerID { get; set; } = 0;

        public int BuyerTeamID { get; set; } = 0;

        public string BuyerName { get; set; } = "";

        public int Qty { get; set; } = 0;

        public bool IsSample { get; set; } = false;

        #region Additional

        [Write(false)]
        public string BuyerTeam { get; set; } = "";

        [Write(false)]
        public string StyleNo { get; set; } = "";

        [Write(false)]
        public decimal ReceiveQty { get; set; } = 0;

        [Write(false)]
        public int CompanyId { get; set; } = 0;

        [Write(false)]
        public string CompanyName { get; set; } = "";

        [Write(false)]
        public int SupplierId { get; set; } = 0;

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YPOChildOrderID > 0;

        #endregion Additional
    }
}
