using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex
{
    [Table(TableNames.YARN_RECEIVE_CHILD_BUYER)]
    public class YarnReceiveChildBuyer : DapperBaseEntity
    {
        public YarnReceiveChildBuyer()
        {
            YRChildBuyerID = 0;
            ReceiveChildID = 0;
            BuyerID = 0;
        }

        [ExplicitKey]
        public int YRChildBuyerID { get; set; }
        public int ReceiveChildID { get; set; }
        public int BuyerID { get; set; }

        #region Additional Columns
        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YRChildBuyerID > 0;

        #endregion Additional Columns
    }
}
