using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    
    [Table(TableNames.DYEING_BATCH_WITH_BATCH_MASTER)]
    public class DyeingBatchWithBatchMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int DBBMID { get; set; }

        public int DBatchID { get; set; }

        public int BatchID { get; set; }

        public decimal BatchUseQtyKG { get; set; }

        public int BatchUseQtyPcs { get; set; }

        #region Addtional

        [Write(false)]
        public string BatchNo { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || DBBMID > 0;

        #endregion Addtional
    }
}