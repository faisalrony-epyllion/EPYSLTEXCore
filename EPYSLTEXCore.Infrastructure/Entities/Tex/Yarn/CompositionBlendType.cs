using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex
{
    [Table(TableNames.CompositionBlendType)]
    public class CompositionBlendType : DapperBaseEntity
    {
        [ExplicitKey]
        public int CompositionID { get; set; } = 0;
        public string BlendTypeName { get; set; } = "";
        public string ProgramTypeName { get; set; } = "";

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || CompositionID > 0;
        [Write(false)]
        public EntityState EntityState { get; set; }
    }
}
