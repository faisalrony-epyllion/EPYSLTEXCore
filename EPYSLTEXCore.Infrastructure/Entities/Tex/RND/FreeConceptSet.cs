using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_FREE_CONCEPT_SET)]
    public class FreeConceptSet : DapperBaseEntity
    {
        [ExplicitKey]
        public int FCSetID { get; set; }

        public int ConceptID { get; set; } = 0;

        public int SubGroupID { get; set; }= 0;

        public decimal Ratio { get; set; }= decimal.Zero;

        #region Additional Fields

        [Write(false)]
        public List<FreeConceptSetItem> FreeConceptSetItems { get; set; }

        public string SubGroup { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FCSetID > 0;

        #endregion Additional Fields

        public FreeConceptSet()
        {
            FreeConceptSetItems = new List<FreeConceptSetItem>();
        }
    }
}
