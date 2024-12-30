using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.HouseKeeping
{
    [Table("LaundryCareLableBuyerCode")]
    public class LaundryCareLableBuyerCode : DapperBaseEntity
    {
        public LaundryCareLableBuyerCode()
        {
            LCLBuyerID = 0;
            BuyerID = 0;
            LCareLableID = 0;
            CareLableCode = "";
            GroupCode = "";
            SeqNo = 0;
        }
        [ExplicitKey]
        public int LCLBuyerID { get; set; }
        public int BuyerID { get; set; }
        public int LCareLableID { get; set; }
        public string CareLableCode { get; set; }
        public string GroupCode { get; set; }
        public int SeqNo { get; set; }

        #region Additional Properties
        [Write(false)]
        public string CareType { get; set; }
        [Write(false)]
        public string CareName { get; set; }
        [Write(false)]
        public string ImagePath { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LCLBuyerID > 0;
        #endregion
    }
}
