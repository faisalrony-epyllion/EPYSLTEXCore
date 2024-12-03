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
        public int YPOChildBuyerID { get; set; }
        public int YPOChildID { get; set; }
        public int YPOMasterID { get; set; }
        public int BuyerId { get; set; }

        #region Additional Fields
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YPOChildBuyerID > 0;

        #endregion Additional Fields
    }
}
