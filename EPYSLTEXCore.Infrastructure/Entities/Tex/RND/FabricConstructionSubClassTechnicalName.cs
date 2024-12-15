using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.FabricConstructionSubClassTechnicalName)]
    public class FabricConstructionSubClassTechnicalName : DapperBaseEntity
    {
        public FabricConstructionSubClassTechnicalName()
        {
            ConstructionID = 0;
            SubClassID = 0;
            TechnicalNameID = 0;
            TechnicalNameID_Elastane = 0;
        }
        [ExplicitKey]
        public int ConstructionID { get; set; }
        public int SubClassID { get; set; }
        public int TechnicalNameID { get; set; }
        public int TechnicalNameID_Elastane { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.ConstructionID > 0;
    }
}
