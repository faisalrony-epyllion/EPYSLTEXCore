using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.SupplyChain
{
    /// <summary>
    /// SpoChildDetail
    /// </summary>
    public class SpoChildDetail : IBaseEntity
    {
        ///<summary>
        /// SPOCDetailsID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// SPOChildID
        ///</summary>
        public int SpoChildId { get; set; }

        ///<summary>
        /// SPOMasterID
        ///</summary>
        public int SpoMasterId { get; set; }

        ///<summary>
        /// BOMMasterID
        ///</summary>
        public int BomMasterId { get; set; }

        ///<summary>
        /// ConsumptionID
        ///</summary>
        public int ConsumptionId { get; set; }

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
        /// IsAdjust
        ///</summary>
        public bool IsAdjust { get; set; }

        ///<summary>
        /// POQty
        ///</summary>
        public decimal PoQty { get; set; }

        ///<summary>
        /// Rate
        ///</summary>
        public decimal Rate { get; set; }

        ///<summary>
        /// POUnitID
        ///</summary>
        public int PoUnitId { get; set; }

        ///<summary>
        /// BookingQty
        ///</summary>
        public decimal BookingQty { get; set; }

        ///<summary>
        /// BookingUnitID
        ///</summary>
        public int BookingUnitId { get; set; }

        ///<summary>
        /// QtyInKg
        ///</summary>
        public decimal QtyInKg { get; set; }

        ///<summary>
        /// ExecutionCompanyID
        ///</summary>
        public int ExecutionCompanyId { get; set; }

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
        /// RateSM
        ///</summary>
        public decimal RateSm { get; set; }

        ///<summary>
        /// RateFromGrade
        ///</summary>
        public decimal RateFromGrade { get; set; }

        ///<summary>
        /// RateFromAW
        ///</summary>
        public decimal RateFromAw { get; set; }

        ///<summary>
        /// GradeID
        ///</summary>
        public int GradeId { get; set; }

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

        public EntityState EntityState { get; set; }

        /// <summary>
        /// Parent ItemMaster pointed by [SPOChildDetails].([ItemMasterID]) (FK_SPOChildDetails_ItemMaster)
        /// </summary>
        public virtual ItemMaster ItemMaster { get; set; }

        public SpoChildDetail()
        {
            DateAdded = DateTime.Now;
            EntityState = EntityState.Added;
            SpoChildId = 0;
            SpoMasterId = 0;
            BomMasterId = 0;
            ConsumptionId = 0;
            ItemMasterID = 0;
            OrderBankPoid = 0;
            ColorId = 0;
            SizeId = 0;
            TechPackId = 0;
            IsAdjust = false;
            PoQty = 0m;
            Rate = 0m;
            PoUnitId = 0;
            BookingQty = 0m;
            BookingUnitId = 0;
            QtyInKg = 0m;
            ExecutionCompanyId = 0;
            RateSm = 0m;
            RateFromGrade = 0m;
            RateFromAw = 0m;
            GradeId = 0;
            SecondarySizeId = 0;
            DestinationGroup = 0;
        }
    }
}
