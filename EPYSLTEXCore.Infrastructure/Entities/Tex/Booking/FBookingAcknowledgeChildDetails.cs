using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_FBookingAcknowledgeChildDetails")]
    public class FBookingAcknowledgeChildDetails : DapperBaseEntity
    {
        [ExplicitKey]
        public int BookingCDetailsID { get; set; }

        [ExplicitKey]
        public int BookingChildID { get; set; }
        [ExplicitKey]
        public int BookingID { get; set; }
        public int ConsumptionID { get; set; }
        public int ItemGroupID { get; set; }
        public int SubGroupID { get; set; }
        public int ItemMasterID { get; set; }
        public int OrderBankPOID { get; set; }
        public int ColorID { get; set; }
        public int SizeID { get; set; }
        public int TechPackID { get; set; }
        public decimal ConsumptionQty { get; set; }
        public decimal BookingQty { get; set; }
        public int BookingUnitID { get; set; }
        public decimal RequisitionQty { get; set; }
        public int OrderQty { get; set; }
        public int AddedBy { get; set; }
        public System.DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public System.DateTime? DateUpdated { get; set; }
        public int ExecutionCompanyID { get; set; }
        public System.DateTime? DeliveryStart { get; set; }
        public System.DateTime? DeliveryEnd { get; set; }
        public int SecondarySizeID { get; set; }
        public int DestinationGroup { get; set; }
        public int TechnicalNameId { get; set; }
        public decimal StitchLength { get; set; }

        #region Additional Properties
        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public decimal YarnReqQty { get; set; }
        [Write(false)]
        public decimal YarnLeftOverQty { get; set; }
        [Write(false)]
        public decimal YarnBalanceQty { get; set; }
        [Write(false)]
        public string Color { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BookingCDetailsID > 0;

        #endregion Additional Properties
        public FBookingAcknowledgeChildDetails()
        {
            DateAdded = System.DateTime.Now;
            DateUpdated = null;
            DeliveryStart = null;
            DeliveryEnd = null;
            BookingChildID = 0;
            BookingID = 0;
            ItemGroupID = 0;
            SubGroupID = 0;
            ItemMasterID = 0;
            OrderBankPOID = 0;
            ColorID = 0;
            SizeID = 0;
            TechPackID = 0;
            ConsumptionQty = 0m;
            BookingQty = 0m;
            BookingUnitID = 0;
            RequisitionQty = 0m;
            OrderQty = 0;
            AddedBy = 0;
            UpdatedBy = 0;
            ExecutionCompanyID = 0;
            SecondarySizeID = 0;
            DestinationGroup = 0;
            TechnicalNameId = 0;
            StitchLength = 0;
            YarnReqQty = 0;
            YarnLeftOverQty = 0;
            YarnBalanceQty = 0;
        }
    }
}
