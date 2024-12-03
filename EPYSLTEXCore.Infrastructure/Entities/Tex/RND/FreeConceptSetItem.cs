using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_FREE_CONCEPT_SET_ITEM)]
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
