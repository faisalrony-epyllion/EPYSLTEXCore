using Dapper.Contrib.Extensions;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("DyeingBatchWithBatchMaster")]
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