using EPYSLTEXCore.Infrastructure.Entities.Gmt.General.Item;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking
{
    /// <summary>
    /// BookingChild
    /// </summary>
    public class BookingChild : IBaseEntity
    {
        ///<summary>
        /// BookingChildID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// BookingID
        ///</summary>
        public int BookingId { get; set; }

        ///<summary>
        /// ConsumptionChildID
        ///</summary>
        public int ConsumptionChildId { get; set; }

        ///<summary>
        /// ConsumptionID
        ///</summary>
        public int ConsumptionId { get; set; }

        ///<summary>
        /// BOMMasterID
        ///</summary>
        public int BomMasterId { get; set; }

        ///<summary>
        /// ExportOrderID
        ///</summary>
        public int ExportOrderId { get; set; }

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
        /// ISourcing
        ///</summary>
        public bool ISourcing { get; set; }

        ///<summary>
        /// Remarks (length: 500)
        ///</summary>
        public string Remarks { get; set; }

        ///<summary>
        /// LengthYds
        ///</summary>
        public int LengthYds { get; set; }

        ///<summary>
        /// LengthInch
        ///</summary>
        public decimal LengthInch { get; set; }

        ///<summary>
        /// FUPartID
        ///</summary>
        public int FuPartId { get; set; }

        ///<summary>
        /// A1ValueID
        ///</summary>
        public int A1ValueId { get; set; }

        ///<summary>
        /// YarnBrandID
        ///</summary>
        public int YarnBrandId { get; set; }

        ///<summary>
        /// ContactID
        ///</summary>
        public int ContactId { get; set; }

        ///<summary>
        /// LabDipNo (length: 100)
        ///</summary>
        public string LabDipNo { get; set; }

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
        /// BlockBookingQty
        ///</summary>
        public decimal BlockBookingQty { get; set; }

        ///<summary>
        /// AdjustQty
        ///</summary>
        public decimal AdjustQty { get; set; }

        /// <summary>
        /// EntityState
        /// </summary>
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Parent ItemMaster pointed by [BookingChild].([ItemMasterID]) (FK_BookingChild_ItemMaster)
        /// </summary>
        public virtual ItemMaster ItemMaster { get; set; }

        #region Additional Properties
        //[Write(false)]
        //public int RevisionNo { get; set; } //Can't Take any field cz if do so have to migrate 
        #endregion

        public BookingChild()
        {
            EntityState = EntityState.Added;
            ExportOrderId = 0;
            TechPackId = 0;
            ConsumptionQty = 0m;
            BookingQty = 0m;
            RequisitionQty = 0m;
            ISourcing = false;
            LengthYds = 0;
            LengthInch = 0m;
            FuPartId = 0;
            A1ValueId = 0;
            YarnBrandId = 0;
            ContactId = 0;
            ExecutionCompanyId = 0;
            BlockBookingQty = 0m;
            AdjustQty = 0m;

            //RevisionNo = 0;
        }
    }
}
