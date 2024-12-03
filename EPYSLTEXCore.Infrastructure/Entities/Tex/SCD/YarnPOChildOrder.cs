using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnPOChildOrder)]
    public class YarnPOChildOrder : DapperBaseEntity
    {
        [ExplicitKey]
        public int YPOChildOrderID { get; set; }

        public int YPOChildID { get; set; }

        public int YPOMasterID { get; set; }

        public int ExportOrderId { get; set; }

        public string EWONo { get; set; }

        public int BuyerID { get; set; }

        public int BuyerTeamID { get; set; }

        public string BuyerName { get; set; }

        public int Qty { get; set; }

        public bool IsSample { get; set; }

        #region Additional

        [Write(false)]
        public string BuyerTeam { get; set; }

        [Write(false)]
        public string StyleNo { get; set; }

        [Write(false)]
        public decimal ReceiveQty { get; set; }

        [Write(false)]
        public int CompanyId { get; set; }

        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public int SupplierId { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YPOChildOrderID > 0;

        #endregion Additional
    }
}
