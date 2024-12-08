using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnPOChildBuyer)]
    public class YarnPOChildBuyer : DapperBaseEntity
    {
        public YarnPOChildBuyer()
        {
            YPOChildBuyerID = 0;
            YPOChildID = 0;
            YPOMasterID = 0;
            BuyerId = 0;
            BuyerName = "";
        }

        [ExplicitKey]
        public int YPOChildBuyerID { get; set; } = 0;
        public int YPOChildID { get; set; } = 0;
        public int YPOMasterID { get; set; } = 0;
        public int BuyerId { get; set; } = 0;

        #region Additional Fields
        [Write(false)]
        public string BuyerName { get; set; } = "";
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YPOChildBuyerID > 0;

        #endregion Additional Fields
    }
}
