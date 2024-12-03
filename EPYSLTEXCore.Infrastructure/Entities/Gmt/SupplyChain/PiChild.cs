using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.SupplyChain
{
    public class PiChild : IBaseEntity
    {
        ///<summary>
        /// PIChildID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// PIMasterID
        ///</summary>
        public int PiMasterId { get; set; }

        ///<summary>
        /// SPOChildID
        ///</summary>
        public int? SpoChildId { get; set; }

        ///<summary>
        /// BookingID
        ///</summary>
        public int BookingId { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// PIQty
        ///</summary>
        public decimal PiQty { get; set; }

        ///<summary>
        /// Rate
        ///</summary>
        public decimal Rate { get; set; }

        ///<summary>
        /// ActualRate
        ///</summary>
        public decimal ActualRate { get; set; }

        ///<summary>
        /// PIUnitID
        ///</summary>
        public int PiUnitId { get; set; }

        ///<summary>
        /// QtyInKg
        ///</summary>
        public decimal QtyInKg { get; set; }

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

        /// <summary>
        /// EntityState
        /// </summary>
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Parent ItemMaster pointed by [PIChild].([ItemMasterID]) (FK_PIChild_ItemMaster)
        /// </summary>
        public virtual ItemMaster ItemMaster { get; set; }

        public PiChild()
        {
            EntityState = EntityState.Added;
            DateAdded = DateTime.Now;
            PiQty = 0m;
            Rate = 0m;
            ActualRate = 0m;
            QtyInKg = 0m;
        }
    }
}
