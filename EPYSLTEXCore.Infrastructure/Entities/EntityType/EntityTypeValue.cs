using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    [Table("EntityTypeValue")]
    public class EntityTypeValue : DapperBaseEntity
    {
        [ExplicitKey]
        public int ValueID { get; set; }
        public string ValueName { get; set; }
        public int EntityTypeID { get; set; }
        public bool IsUsed { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        /// <summary>
        /// Parent EntityType pointed by [EntityTypeValue].([EntityTypeId]) (FK_EntityTypeValue_EntityType)
        /// </summary>
        //  public virtual EntityType EntityType { get; set; }
        #region Additional Columns
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.EntityTypeID > 0;

        #endregion Additional Columns
        public EntityTypeValue()
        {
            IsUsed = false;
            DateAdded = DateTime.Now;
            UpdatedBy = 0;
            EntityState = EntityState.Added;
        }
    }
    #region Validator

    //public class EntityTypeValueValidator : AbstractValidator<EntityTypeValue>
    //{
    //    public EntityTypeValueValidator()
    //    {
    //        //RuleFor(x => x.ValueName).NotEmpty().MaximumLength(300);
    //    }
    //}

    #endregion Validator
}

