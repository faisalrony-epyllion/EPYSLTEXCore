using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table("T_FreeConceptSetItem")]
    public class FreeConceptSetItem : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int FCSetItemID { get; set; }

        public int FCSetID { get; set; }

        public int ConceptID { get; set; }

        public int ItemMasterID { get; set; }

        public decimal Qty { get; set; }

        #endregion Table Properties

        #region Additional Properties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FCSetItemID > 0;

        #endregion Additional Properties
    }
}
