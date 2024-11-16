using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Data
{
    public abstract class DapperBaseEntity : IDapperBaseEntity
    {
        /// <summary>
        /// EntityState
        /// </summary>
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public abstract bool IsModified { get; }

        [Write(false)]
        public virtual bool IsNew => EntityState == EntityState.Added;

        /// <summary>
        /// Ctor
        /// </summary>
        public DapperBaseEntity()
        {
            EntityState = EntityState.Added;
        }
    }
}
