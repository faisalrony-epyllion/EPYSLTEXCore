using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    [Table("Signature")]
    public class Signatures : DapperBaseEntity
    {
        [ExplicitKey]
        public string Field { get; set; }

        [ExplicitKey]
        public DateTime Dates { get; set; }

        public decimal LastNumber { get; set; }

        [ExplicitKey]
        public string CompanyId { get; set; }

        [ExplicitKey]
        public string SiteId { get; set; }

        public Signatures()
        {
            LastNumber = 1m;
            CompanyId = "1";
            SiteId = "1";
        }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || Field != "";
        #endregion Additional Properties

    }
}
