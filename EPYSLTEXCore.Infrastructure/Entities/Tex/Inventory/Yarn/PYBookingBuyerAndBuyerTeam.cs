using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.PROJECTION_YARN_BOOKING_BUYER_AND_BUYERTEAM)]
    public class PYBookingBuyerAndBuyerTeam : DapperBaseEntity
    {
        [ExplicitKey]
        public int PYBookingBuyerAndBuyerTeamID { get; set; } = 0;
        public int PYBookingID { get; set; } = 0;
        public int BuyerID { get; set; } = 0;
        public int BuyerTeamID { get; set; } = 0;

        #region Additional Columns
        [Write(false)]
        public string BuyerIDsList { get; set; } = "";
        [Write(false)]
        public string BuyerTeamIDsList { get; set; } = "";
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.PYBookingBuyerAndBuyerTeamID > 0;

        #endregion Additional Columns

        public PYBookingBuyerAndBuyerTeam()
        {


        }
    }
}
