using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table("YarnPIReceiveAdditionalValue")]
    public class YarnPIReceiveAdditionalValue : DapperBaseEntity
    {
        [ExplicitKey]
        public int YPIReceiveAdditionalID { get; set; }

        public int YPIReceiveMasterID { get; set; }

        public int AdditionalValueID { get; set; }

        public decimal AdditionalValue { get; set; }

        #region Additional

        [Write(false)]
        public string AdditionalValueName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YPIReceiveAdditionalID > 0;

        #endregion Additional
    }
}