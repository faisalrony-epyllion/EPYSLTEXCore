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
    [Table("EntityType")]
    public class EntityType : DapperBaseEntity
    {
        [ExplicitKey]
        public int EntityTypeID { get; set; }
        public string EntityTypeName { get; set; }
        public bool IntegerValue { get; set; }
        public bool IsUsed { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        /// <summary>
        /// Child EntityTypeValues where [EntityTypeValue].[EntityTypeID] point to this entity (FK_EntityTypeValue_EntityType)
        /// </summary>
        //public virtual ICollection<EntityTypeValue> EntityTypeValues { get; set; } 
        #region Additional Columns

        [Write(false)]
        public List<EntityTypeValue> Childs { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.EntityTypeID > 0;

        #endregion Additional Columns
        public EntityType()
        {
            //EntityState = EntityState.Added;
            IntegerValue = false;
            IsUsed = false;
            DateAdded = DateTime.Now;
            UpdatedBy = 0;
            Childs = new List<EntityTypeValue>();
        }
    }
    #region Validator

    //public class EntityTypeValidator : AbstractValidator<EntityType>
    //{
    //    public EntityTypeValidator()
    //    {
    //        // RuleFor(x => x.EntityTypeName).NotEmpty().MaximumLength(100);
    //    }
    //}

    #endregion Validator
}
