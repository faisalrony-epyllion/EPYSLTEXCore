using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_FREE_CONCEPT_STATUS)]
    public class ConceptStatus : DapperBaseEntity
    {
        public ConceptStatus()
        {
            FCSID = 0;
            ConceptID = 0;
            CPSID = 0;
            IsApplicable = false;
            SeqNo = 0;
            Status = false;
            Remarks = "";
        }

        [ExplicitKey]
        public int FCSID { get; set; }

        public int ConceptID { get; set; }
        public int CPSID { get; set; }
        public bool IsApplicable { get; set; }
        public int SeqNo { get; set; }
        public bool Status { get; set; }
        public string Remarks { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FCSID > 0;
    }
}
