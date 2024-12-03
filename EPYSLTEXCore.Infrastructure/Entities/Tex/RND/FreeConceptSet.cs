using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table("T_FreeConceptSet")]
    public class FreeConceptSet : DapperBaseEntity
    {
        [ExplicitKey]
        public int FCSetID { get; set; }

        public int ConceptID { get; set; }

        public int SubGroupID { get; set; }

        public decimal Ratio { get; set; }

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
