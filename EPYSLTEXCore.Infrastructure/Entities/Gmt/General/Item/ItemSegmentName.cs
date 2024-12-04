using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item
{
    public class ItemSegmentName : IBaseEntity
    {
        ///<summary>
        /// SegmentNameID (Primary key)
        ///</summary>
        public int Id { get; set; }
        public string SegmentName { get; set; }
        public string DisplayName { get; set; }
        public bool ItemSegment { get; set; }
        public bool IsUsed { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        public EntityState EntityState { get; set; }

        /// <summary>
        /// Child ItemSegmentValues where [ItemSegmentValue].[SegmentNameID] point to this entity (FK_ItemSegmentValue_ItemSegmentName)
        /// </summary>
        public virtual ICollection<ItemSegmentValue> ItemSegmentValues { get; set; }

        public ItemSegmentName()
        {
            DateAdded = DateTime.Now;
            ItemSegment = false;
            IsUsed = false;
            EntityState = EntityState.Added;
            ItemSegmentValues = new List<ItemSegmentValue>();
        }
    }
}
