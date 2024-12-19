using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnPIReceiveDeductionValue)]
    public class YarnPIReceiveDeductionValue : DapperBaseEntity
    {
        [ExplicitKey]
        public int YPIReceiveDeductionID { get; set; }

        public int YPIReceiveMasterID { get; set; }

        public int DeductionValueID { get; set; }

        public decimal DeductionValue { get; set; }

        #region Additional

        [Write(false)]
        public string DeductionValueName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YPIReceiveDeductionID > 0;

        #endregion Additional
    }
}