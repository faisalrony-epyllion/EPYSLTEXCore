using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    public class Select2OptionModel:DapperBaseEntity
    {
        public string id { get; set; }
        public string text { get; set; }
        public string desc { get; set; }
        public string additionalValue { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified;

    }
}
