using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Data.Entity;

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
        public int TotalRows { get; set; } = 0;

        [Write(false)]
        public abstract bool IsModified { get; }

        [Write(false)]
        public virtual bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public string Status { get; set; } = "";

        /// <summary>
        /// Ctor
        /// </summary>
        public DapperBaseEntity()
        {
            EntityState = EntityState.Added;
        }
    }
}
