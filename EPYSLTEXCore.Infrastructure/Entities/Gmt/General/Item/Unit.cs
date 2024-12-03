using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item
{
    /// <summary>
    /// Unit
    /// </summary>
    public class Unit : IBaseEntity
    {
        ///<summary>
        /// UnitID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// UnitDesc (length: 30)
        ///</summary>
        public string UnitDesc { get; set; }

        ///<summary>
        /// DisplayUnitDesc (length: 10)
        ///</summary>
        public string DisplayUnitDesc { get; set; }

        ///<summary>
        /// RelativeFactor
        ///</summary>
        public decimal RelativeFactor { get; set; }

        ///<summary>
        /// UnitSetID
        ///</summary>
        public int UnitSetId { get; set; }

        ///<summary>
        /// NoFraction
        ///</summary>
        public bool NoFraction { get; set; }

        ///<summary>
        /// ExternalCode (length: 10)
        ///</summary>
        public string ExternalCode { get; set; }

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
        /// Child ItemGroups where [ItemGroup].[DefaultUnitID] point to this entity (FK_ItemGroup_Unit)
        /// </summary>
        public virtual ICollection<ItemGroup> ItemGroups { get; set; }
        /// <summary>
        /// Child ItemMasters where [ItemMaster].[DefaultReportUnitID] point to this entity (FK_ItemMaster_Unit1)
        /// </summary>
        public virtual ICollection<ItemMaster> ItemMasters_DefaultReportUnitId { get; set; }
        /// <summary>
        /// Child ItemMasters where [ItemMaster].[DefaultTranUnitID] point to this entity (FK_ItemMaster_Unit)
        /// </summary>
        public virtual ICollection<ItemMaster> ItemMasters_DefaultTranUnitId { get; set; }

        public Unit()
        {
            DateAdded = DateTime.Now;
            EntityState = EntityState.Added;
            RelativeFactor = 0m;
            NoFraction = false;
            IsUsed = false;
            ItemGroups = new List<ItemGroup>();
            ItemMasters_DefaultReportUnitId = new List<ItemMaster>();
            ItemMasters_DefaultTranUnitId = new List<ItemMaster>();
        }
    }
}
