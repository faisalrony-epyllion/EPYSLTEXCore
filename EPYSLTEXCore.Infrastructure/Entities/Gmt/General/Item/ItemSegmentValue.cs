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
        [Write(false)]
        public int Id { get; set; }
        [ExplicitKey]
        public int SegmentValueID { get; set; }
        ///<summary>
        /// SegmentValue (length: 250)
        ///</summary>
        public string SegmentValue { get; set; }

        ///<summary>
        /// SegmentNameID
        ///</summary>
        public int SegmentNameID { get; set; }

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
        [Write(false)]
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Parent ItemSegmentName pointed by [ItemSegmentValue].([SegmentNameId]) (FK_ItemSegmentValue_ItemSegmentName)
        /// </summary>
       [Write(false)]
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

    public class TempItemSegmentValue
    {
        public ItemSegmentValue ItemSegmentValue { get; set; }
        public string BlendTypeName { get; set; } = "";
        public string ProgramTypeName { get; set; } = "";
    }
}
