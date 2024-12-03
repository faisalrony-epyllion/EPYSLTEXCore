using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table("FabricTechnicalNameOthers")]
    public class FabricTechnicalNameOther : DapperBaseEntity
    {
        [ExplicitKey]
        public int FTNOID { get; set; }
        public int ConstructionId { get; set; }
        public int Gsm { get; set; }
        public string YarnCount { get; set; }
        public int MachineGauge { get; set; }
        public int MachineDia { get; set; }
        public decimal StitchLength { get; set; }
        // public EntityState EntityState { get; set; } 
        #region Additional Columns


        //Additional Fields-
        [Write(false)]
        public string ConstructionName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.FTNOID > 0;
        #endregion Additional Columns

        public FabricTechnicalNameOther()
        {
            EntityState = EntityState.Added;
        }
    }
}
