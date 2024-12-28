using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.HouseKeeping
{
    [Table("LaundryCareLable_HK")]
    public class LaundryCareLable_HK : DapperBaseEntity
    {
        #region Table Properties
        [ExplicitKey]
        public int LCareLableID { get; set; }
        public string CareType { get; set; }
        public string CareName { get; set; }
        public string ImagePath { get; set; }
        public int SeqNo { get; set; }
        #endregion

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LCareLableID > 0;
        #endregion
        public LaundryCareLable_HK()
        {
        }
    }
}
