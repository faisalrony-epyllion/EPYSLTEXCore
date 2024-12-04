using EPYSLTEXCore.Infrastructure.Entities.Gmt.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.Security
{
    public class LoginUserAttachedWithGroupUser : IBaseEntity
    {
        /// <summary>
        /// We need this for using EfRepository
        /// </summary>
        [NotMapped]
        public int Id { get; set; }

        ///<summary>
        /// UserCode (Primary key)
        ///</summary>
        public int UserCode { get; set; }

        ///<summary>
        /// GroupCode (Primary key)
        ///</summary>
        public int GroupCode { get; set; }

        [NotMapped]
        public EntityState EntityState { get; set; }
        public int AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Parent GroupUser pointed by [LoginUserAttachedWithGroupUser].([GroupCode]) (FK_LoginUserAttachedWithGroupUser_GroupUser)
        /// </summary>
        public virtual GroupUser GroupUser { get; set; }

        public LoginUserAttachedWithGroupUser()
        {
            EntityState = EntityState.Added;
        }
    }
}
