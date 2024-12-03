using EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.SupplyChain;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item
{
    public class ItemMaster : IBaseEntity
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

        /// <summary>
        /// EntityState
        /// </summary>
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Child BookingChilds where [BookingChild].[ItemMasterID] point to this entity (FK_BookingChild_ItemMaster)
        /// </summary>
        public virtual ICollection<BookingChild> BookingChilds { get; set; }
        /// <summary>
        /// Child BookingChildDetails where [BookingChildDetails].[ItemMasterID] point to this entity (FK_BookingChildDetails_ItemMaster)
        /// </summary>
        public virtual ICollection<BookingChildDetail> BookingChildDetails { get; set; }
        /// <summary>
        /// Child PiChilds where [PIChild].[ItemMasterID] point to this entity (FK_PIChild_ItemMaster)
        /// </summary>
        public virtual ICollection<PiChild> PiChilds { get; set; }
        /// <summary>
        /// Child SpoChilds where [SPOChild].[ItemMasterID] point to this entity (FK_SPOChild_ItemMaster)
        /// </summary>
        public virtual ICollection<SpoChild> SpoChilds { get; set; }
        /// <summary>
        /// Child SpoChildDetails where [SPOChildDetails].[ItemMasterID] point to this entity (FK_SPOChildDetails_ItemMaster)
        /// </summary>
        public virtual ICollection<SpoChildDetail> SpoChildDetails { get; set; }

        /// <summary>
        /// Parent ItemGroup pointed by [ItemMaster].([ItemGroupId]) (FK_ItemMaster_ItemGroup)
        /// </summary>
        public virtual ItemGroup ItemGroup { get; set; }

        /// <summary>
        /// Parent ItemSubGroup pointed by [ItemMaster].([SubGroupId]) (FK_ItemMaster_ItemSubGroup)
        /// </summary>
        public virtual ItemSubGroup ItemSubGroup { get; set; }

        /// <summary>
        /// Parent Unit pointed by [ItemMaster].([DefaultReportUnitId]) (FK_ItemMaster_Unit1)
        /// </summary>
        public virtual Unit DefaultReportUnit { get; set; }

        /// <summary>
        /// Parent Unit pointed by [ItemMaster].([DefaultTranUnitId]) (FK_ItemMaster_Unit)
        /// </summary>
        public virtual Unit DefaultTranUnit { get; set; }

        public ItemMaster()
        {
            EntityState = EntityState.Added;
            BookingChilds = new List<BookingChild>();
            BookingChildDetails = new List<BookingChildDetail>();
            PiChilds = new List<PiChild>();
            SpoChilds = new List<SpoChild>();
            SpoChildDetails = new List<SpoChildDetail>();
        }
    }
}
