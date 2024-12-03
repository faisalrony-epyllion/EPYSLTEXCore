using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table("MachineGaugeSetup")]
    public class MachineGaugeSetup : DapperBaseEntity
    {
        [ExplicitKey]
        public int MGSID { get; set; }
        public int ConstructionID { get; set; }
        public int YCFrom { get; set; }
        public int YCTo { get; set; }
        public int MachineGauge { get; set; } 
        
        #region Additional Columns

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.MGSID > 0;
        [Write(false)]
        public string ConstructionName { get; set; }
        #endregion Additional Columns
    }
}
