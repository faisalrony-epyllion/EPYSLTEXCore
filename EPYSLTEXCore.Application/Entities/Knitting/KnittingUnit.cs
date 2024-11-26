using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;
using System.Runtime.Serialization;

namespace EPYSLTEXCore.Application.Entities
{
    [Table("KnittingUnit")]
    public class KnittingUnit : DapperBaseEntity
    {
        [ExplicitKey]

        public int KnittingUnitID { get; set; }
        public string ContactId { get; set; }

        public string UnitName { get; set; }

        public string ShortName { get; set; }

       
        public string IsKnitting { get; set; }
        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified;
        #endregion Additional Properties
    }
}
