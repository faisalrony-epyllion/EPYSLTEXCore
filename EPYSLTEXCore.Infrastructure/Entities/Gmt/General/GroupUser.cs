
using EPYSLTEXCore.Infrastructure.Entities.Gmt.Security;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General
{
    public class GroupUser : BaseEntity
    {
        ///<summary>
        /// GroupName (length: 250)
        ///</summary>
        public string GroupName { get; set; }

        ///<summary>
        /// GroupDescription (length: 500)
        ///</summary>
        public string GroupDescription { get; set; }

        ///<summary>
        /// DefaultApplicationID
        ///</summary>
        public int DefaultApplicationId { get; set; }

        public int AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Child GroupUserSecurityRules where [GroupUserSecurityRule].[GroupCode] point to this entity (FK_GroupUserSecurityRule_GroupUser)
        /// </summary>
        public virtual ICollection<GroupUserSecurityRule> GroupUserSecurityRules { get; set; }

        /// <summary>
        /// Child LoginUserAttachedWithGroupUsers where [LoginUserAttachedWithGroupUser].[GroupCode] point to this entity (FK_LoginUserAttachedWithGroupUser_GroupUser)
        /// </summary>
        public virtual ICollection<LoginUserAttachedWithGroupUser> LoginUserAttachedWithGroupUsers { get; set; }

        public GroupUser()
        {
            EntityState = EntityState.Added;
            DefaultApplicationId = 0;
            GroupUserSecurityRules = new List<GroupUserSecurityRule>();
            LoginUserAttachedWithGroupUsers = new List<LoginUserAttachedWithGroupUser>();
        }
    }
}
