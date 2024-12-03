using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking
{
    /// <summary>
    /// BookingChildDetail
    /// </summary>
    public class BookingChildDetail : IBaseEntity
    {
        ///<summary>
        /// BookingCDetailsID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// BookingChildID
        ///</summary>
        public int BookingChildId { get; set; }

        ///<summary>
        /// BookingID
        ///</summary>
        public int BookingId { get; set; }

        ///<summary>
        /// ItemGroupID
        ///</summary>
        public int ItemGroupId { get; set; }

        ///<summary>
        /// SubGroupID
        ///</summary>
        public int SubGroupId { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// OrderBankPOID
        ///</summary>
        public int OrderBankPoid { get; set; }

        ///<summary>
        /// ColorID
        ///</summary>
        public int ColorId { get; set; }

        ///<summary>
        /// SizeID
        ///</summary>
        public int SizeId { get; set; }

        ///<summary>
        /// TechPackID
        ///</summary>
        public int TechPackId { get; set; }

        ///<summary>
        /// ConsumptionQty
        ///</summary>
        public decimal ConsumptionQty { get; set; }

        ///<summary>
        /// BookingQty
        ///</summary>
        public decimal BookingQty { get; set; }

        ///<summary>
        /// BookingUnitID
        ///</summary>
        public int BookingUnitId { get; set; }

        ///<summary>
        /// RequisitionQty
        ///</summary>
        public decimal RequisitionQty { get; set; }

        ///<summary>
        /// OrderQty
        ///</summary>
        public int OrderQty { get; set; }

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
        /// ExecutionCompanyID
        ///</summary>
        public int ExecutionCompanyId { get; set; }

        ///<summary>
        /// DeliveryStart
        ///</summary>
        public DateTime? DeliveryStart { get; set; }

        ///<summary>
        /// DeliveryEnd
        ///</summary>
        public DateTime? DeliveryEnd { get; set; }

        ///<summary>
        /// SecondarySizeID
        ///</summary>
        public int SecondarySizeId { get; set; }

        ///<summary>
        /// DestinationGroup
        ///</summary>
        public int DestinationGroup { get; set; }

        /// <summary>
        /// EntityState
        /// </summary>
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Parent ItemMaster pointed by [BookingChildDetails].([ItemMasterID]) (FK_BookingChildDetails_ItemMaster)
        /// </summary>
        public virtual ItemMaster ItemMaster { get; set; }

        public BookingChildDetail()
        {
            EntityState = EntityState.Added;
            DateAdded = DateTime.Now;
            BookingId = 0;
            TechPackId = 0;
            ConsumptionQty = 0m;
            BookingQty = 0m;
            RequisitionQty = 0m;
            OrderQty = 0;
            ExecutionCompanyId = 0;
            SecondarySizeId = 0;
            DestinationGroup = 0;
        }
    }
}
