using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item
{
    /// <summary>
    /// ItemGroup
    /// </summary>
    public class ItemGroup : IBaseEntity
    {
        ///<summary>
        /// ItemGroupID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// DisplayItemGroupID (length: 10)
        ///</summary>
        public string DisplayItemGroupId { get; set; }

        ///<summary>
        /// GroupName (length: 50)
        ///</summary>
        public string GroupName { get; set; }

        ///<summary>
        /// HasSubGroup
        ///</summary>
        public bool HasSubGroup { get; set; }

        ///<summary>
        /// UnitSetID
        ///</summary>
        public int UnitSetId { get; set; }

        ///<summary>
        /// DefaultUnitID
        ///</summary>
        public int DefaultUnitId { get; set; }

        ///<summary>
        /// HasSecondaryUnit
        ///</summary>
        public bool HasSecondaryUnit { get; set; }

        ///<summary>
        /// SUnitSetID
        ///</summary>
        public int? SUnitSetId { get; set; }

        ///<summary>
        /// SUnitID
        ///</summary>
        public int? SUnitId { get; set; }

        ///<summary>
        /// SUnitFirst
        ///</summary>
        public bool SUnitFirst { get; set; }

        ///<summary>
        /// CostingTypeID
        ///</summary>
        public int CostingTypeId { get; set; }

        ///<summary>
        /// GroupSubTypeID
        ///</summary>
        public int GroupSubTypeId { get; set; }

        ///<summary>
        /// HasUD
        ///</summary>
        public int HasUd { get; set; }

        ///<summary>
        /// UnitInColumn
        ///</summary>
        public bool UnitInColumn { get; set; }

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

        ///<summary>
        /// HasMeasurementUnit
        ///</summary>
        public bool HasMeasurementUnit { get; set; }

        ///<summary>
        /// MeasurementUnitSetID
        ///</summary>
        public int MeasurementUnitSetId { get; set; }

        ///<summary>
        /// ItemWiseBooking
        ///</summary>
        public bool ItemWiseBooking { get; set; }

        public EntityState EntityState { get; set; }

        /// <summary>
        /// Child ItemMasters where [ItemMaster].[ItemGroupID] point to this entity (FK_ItemMaster_ItemGroup)
        /// </summary>
        public virtual ICollection<ItemMaster> ItemMasters { get; set; }
        /// <summary>
        /// Child ItemSubGroups where [ItemSubGroup].[ItemGroupID] point to this entity (FK_ItemSubGroup_ItemGroup)
        /// </summary>
        public virtual ICollection<ItemSubGroup> ItemSubGroups { get; set; }

        // Foreign keys

        /// <summary>
        /// Parent Unit pointed by [ItemGroup].([DefaultUnitId]) (FK_ItemGroup_Unit)
        /// </summary>
        public virtual Unit Unit { get; set; }

        public ItemGroup()
        {
            EntityState = EntityState.Added;
            HasSubGroup = false;
            HasSecondaryUnit = false;
            SUnitFirst = false;
            HasUd = 0;
            UnitInColumn = false;
            IsUsed = false;
            HasMeasurementUnit = false;
            MeasurementUnitSetId = 0;
            ItemWiseBooking = false;
            ItemMasters = new List<ItemMaster>();
            ItemSubGroups = new List<ItemSubGroup>();
        }
    }
}
