using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item
{
    public class ItemMasterBomTemp : IBaseEntity
    {
        ///<summary>
        /// ItemMasterID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// DisplayItemID (length: 20)
        ///</summary>
        public string DisplayItemId { get; set; }

        ///<summary>
        /// ItemName (length: 200)
        ///</summary>
        public string ItemName { get; set; }

        ///<summary>
        /// DisplayItemName (length: 200)
        ///</summary>
        public string DisplayItemName { get; set; }

        ///<summary>
        /// ItemGroupID
        ///</summary>
        public int ItemGroupId { get; set; }

        ///<summary>
        /// SubGroupID
        ///</summary>
        public int SubGroupId { get; set; }

        ///<summary>
        /// UnitSetID
        ///</summary>
        public int UnitSetId { get; set; }

        ///<summary>
        /// DefaultTranUnitID
        ///</summary>
        public int DefaultTranUnitId { get; set; }

        ///<summary>
        /// DefaultReportUnitID
        ///</summary>
        public int DefaultReportUnitId { get; set; }

        ///<summary>
        /// Segment1ValueID
        ///</summary>
        public int Segment1ValueId { get; set; }

        ///<summary>
        /// Segment2ValueID
        ///</summary>
        public int Segment2ValueId { get; set; }

        ///<summary>
        /// Segment3ValueID
        ///</summary>
        public int Segment3ValueId { get; set; }

        ///<summary>
        /// Segment4ValueID
        ///</summary>
        public int Segment4ValueId { get; set; }

        ///<summary>
        /// Segment5ValueID
        ///</summary>
        public int Segment5ValueId { get; set; }

        ///<summary>
        /// Segment6ValueID
        ///</summary>
        public int Segment6ValueId { get; set; }

        ///<summary>
        /// Segment7ValueID
        ///</summary>
        public int Segment7ValueId { get; set; }

        ///<summary>
        /// Segment8ValueID
        ///</summary>
        public int Segment8ValueId { get; set; }

        ///<summary>
        /// Segment9ValueID
        ///</summary>
        public int Segment9ValueId { get; set; }

        ///<summary>
        /// Segment10ValueID
        ///</summary>
        public int Segment10ValueId { get; set; }

        ///<summary>
        /// Segment11ValueID
        ///</summary>
        public int Segment11ValueId { get; set; }

        ///<summary>
        /// Segment12ValueID
        ///</summary>
        public int Segment12ValueId { get; set; }

        ///<summary>
        /// Segment13ValueID
        ///</summary>
        public int Segment13ValueId { get; set; }

        ///<summary>
        /// Segment14ValueID
        ///</summary>
        public int Segment14ValueId { get; set; }

        ///<summary>
        /// Segment15ValueID
        ///</summary>
        public int Segment15ValueId { get; set; }

        public EntityState EntityState { get; set; }

        public ItemMasterBomTemp()
        {
            EntityState = EntityState.Added;
        }
    }
}
