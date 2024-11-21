using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Entities;
using EPYSLTEXCore.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace EPYSLTEXCore.Application.Entities
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