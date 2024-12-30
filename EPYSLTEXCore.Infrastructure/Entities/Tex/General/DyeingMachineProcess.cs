using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table(TableNames.DYEING_MACHINE_PROCESS)]
    public class DyeingMachineProcess : IDapperBaseEntity
    {
        public DyeingMachineProcess()
        {
            EntityState = EntityState.Added;
        }
        [ExplicitKey]
        public int DMProcessID { get; set; }
        public int DMID { get; set; }

        public int DyeProcessID { get; set; }

        //public virtual DyeingMachine DyeingMachine { get; set; }

        #region Additional Fields
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public string DyeProcess { get; set; }

        #endregion Additional Fields
    }
}
