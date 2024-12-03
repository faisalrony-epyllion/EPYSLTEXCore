using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.PriceRequestBuyer)]
    public class PriceRequestBuyer : DapperBaseEntity
    {
        #region Table properties

        [ExplicitKey]
        public int PRBID { get; set; }
        public int LPPRID { get; set; }
        public int BuyerID { get; set; }
        public decimal Price { get; set; }
        public string PRRemarks { get; set; }
        public string Remarks { get; set; }
        #endregion Table properties

        #region Additional Columns
        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || PRBID > 0;

        #endregion Additional Columns

        public PriceRequestBuyer()
        {
            PRBID = 0;
            LPPRID = 0;
            BuyerID = 0;
            Price = 0;
            PRRemarks = "";
            Remarks = "";

            BuyerName = "";
        }

    }
}
