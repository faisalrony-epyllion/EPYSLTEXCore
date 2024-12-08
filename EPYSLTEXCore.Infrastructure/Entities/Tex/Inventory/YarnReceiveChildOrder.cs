using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex
{
    [Table(TableNames.YARN_RECEIVE_CHILD_ORDER)]
    public class YarnReceiveChildOrder : DapperBaseEntity
    {
        public YarnReceiveChildOrder()
        {
            YRChildOrderID = 0;
            ReceiveChildID = 0;
            BuyerID = 0;
            ExportOrderID = 0;
            EWONo = "";
            Qty = 0;
            BuyerName = "";
            BuyerTeamID = 0;
            IsSample = false;
        }

        [ExplicitKey]
        public int YRChildOrderID { get; set; }
        public int ReceiveChildID { get; set; }
        public int BuyerID { get; set; }
        public int ExportOrderID { get; set; }
        public string EWONo { get; set; }
        public int Qty { get; set; }
        public string BuyerName { get; set; }
        public int BuyerTeamID { get; set; }
        public bool IsSample { get; set; }

        #region Additional Columns
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YRChildOrderID > 0;

        #endregion Additional Columns
    }
}
