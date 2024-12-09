using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item
{
    public class ItemSegmentValue : DapperBaseEntity
    {
        ///<summary>
        /// SegmentValueID (Primary key)
        ///</summary> 
        public int Id { get; set; }

        ///<summary>
        /// SegmentValue (length: 250)
        ///</summary>
        public string SegmentValue { get; set; }

        ///<summary>
        /// SegmentNameID
        ///</summary>
        public int SegmentNameId { get; set; }

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
        /// Parent ItemSegmentName pointed by [ItemSegmentValue].([SegmentNameId]) (FK_ItemSegmentValue_ItemSegmentName)
        /// </summary>
        public virtual ItemSegmentName ItemSegmentName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified;

        public ItemSegmentValue()
        {
            DateAdded = DateTime.Now;
            IsUsed = false;
            EntityState = EntityState.Added;
        }
    }
}
