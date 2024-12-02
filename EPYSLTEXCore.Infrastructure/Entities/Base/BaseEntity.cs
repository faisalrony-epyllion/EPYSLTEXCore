using Dapper.Contrib.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    public abstract class BaseEntity : IBaseEntity
    {
        /// <summary>
        /// Id
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// EntityState
        /// </summary>
        [NotMapped]
        [Write(false)]
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public BaseEntity()
        {
            EntityState = EntityState.Added;
        }
    }
}