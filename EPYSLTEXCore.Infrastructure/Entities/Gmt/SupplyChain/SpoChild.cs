using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.SupplyChain
{
    /// <summary>
    /// SpoChild
    /// </summary>
    public class SpoChild : IBaseEntity
    {
        ///<summary>
        /// SPOChildID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// SPOMasterID
        ///</summary>
        public int SpoMasterId { get; set; }

        ///<summary>
        /// BookingID
        ///</summary>
        public int BookingId { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

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
        /// QtyInKg
        ///</summary>
        public decimal QtyInKg { get; set; }

        ///<summary>
        /// BookingQty
        ///</summary>
        public decimal BookingQty { get; set; }

        ///<summary>
        /// BookingUnitID
        ///</summary>
        public int BookingUnitId { get; set; }

        ///<summary>
        /// ForExportOrder (length: 3)
        ///</summary>
        public string ForExportOrder { get; set; }

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
        /// CustomerID
        ///</summary>
        public int CustomerId { get; set; }

        ///<summary>
        /// MerchandisingRate
        ///</summary>
        public decimal MerchandisingRate { get; set; }

        /// <summary>
        /// EntityState
        /// </summary>
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Parent ItemMaster pointed by [SPOChild].([ItemMasterID]) (FK_SPOChild_ItemMaster)
        /// </summary>
        public virtual ItemMaster ItemMaster { get; set; }

        public SpoChild()
        {
            EntityState = EntityState.Added;
            DateAdded = DateTime.Now;
            BookingId = 0;
            PoQty = 0m;
            Rate = 0m;
            QtyInKg = 0m;
            BookingQty = 0m;
            BookingUnitId = 0;
            ForExportOrder = "";
            ExecutionCompanyId = 0;
            RateSm = 0m;
            RateFromGrade = 0m;
            RateFromAw = 0m;
            GradeId = 0;
            CustomerId = 0;
            MerchandisingRate = 0m;
        }
    }
}
