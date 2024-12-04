using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item
{
    /// <summary>
    /// ItemSubGroup
    /// </summary>
    public class ItemSubGroup : IBaseEntity
    {
        ///<summary>
        /// SubGroupID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// DisplaySubGrupID (length: 10)
        ///</summary>
        public string DisplaySubGrupId { get; set; }

        ///<summary>
        /// SubGroupName (length: 50)
        ///</summary>
        public string SubGroupName { get; set; }

        ///<summary>
        /// ItemPrefix (length: 5)
        ///</summary>
        public string ItemPrefix { get; set; }

        ///<summary>
        /// ItemGroupID
        ///</summary>
        public int ItemGroupId { get; set; }

        ///<summary>
        /// SeqNo
        ///</summary>
        public int SeqNo { get; set; }

        ///<summary>
        /// IsUsed
        ///</summary>
        public bool IsUsed { get; set; }

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; }

        ///<summary>
        /// DateAdded
        ///</summary>
        public DateTime DateAdded { get; set; }

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; }

        ///<summary>
        /// DateUpdated
        ///</summary>
        public DateTime? DateUpdated { get; set; }

        public EntityState EntityState { get; set; }

        /// <summary>
        /// Child ItemMasters where [ItemMaster].[SubGroupID] point to this entity (FK_ItemMaster_ItemSubGroup)
        /// </summary>
        public virtual ICollection<ItemMaster> ItemMasters { get; set; }

        /// <summary>
        /// Parent ItemGroup pointed by [ItemSubGroup].([ItemGroupId]) (FK_ItemSubGroup_ItemGroup)
        /// </summary>
        public virtual ItemGroup ItemGroup { get; set; }

        public ItemSubGroup()
        {
            EntityState = EntityState.Added;
            DateAdded = DateTime.Now;
            SeqNo = 0;
            IsUsed = false;
            ItemMasters = new List<ItemMaster>();
        }
    }
}
