using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Production;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.CONCEPT_PENDING_STATUS_HK)]
    public class ConceptPendingStatus_HK : DapperBaseEntity
    {
        public ConceptPendingStatus_HK()
        {
            CPSID = 0;
            StatusName = "";
            SeqNo = 0;
            IsApplicable = false;
        }

        [ExplicitKey]
        public int CPSID { get; set; }

        public string StatusName { get; set; }
        public int SeqNo { get; set; }
        public bool IsApplicable { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || CPSID > 0;
    }
}
