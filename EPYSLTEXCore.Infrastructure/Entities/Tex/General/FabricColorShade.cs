using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table("FabricColorShadeFactor")]
    public class FabricColorShade : DapperBaseEntity
    {
        [ExplicitKey]
        public int FCSFID { get; set; }        
        public int ShadeID { get; set; }
        public int CountAdd { get; set; }
        public string ShadeName { get; set; }
        public decimal SLCount { get; set; }
        //public EntityState EntityState { get; set; }
        #region Additional Columns
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.FCSFID > 0;
        #endregion Additional Columns
    }
}
