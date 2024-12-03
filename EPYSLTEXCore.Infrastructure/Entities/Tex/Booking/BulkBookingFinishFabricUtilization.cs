using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table("T_BulkBookingFinishFabricUtilization")]
    public class BulkBookingFinishFabricUtilization : DapperBaseEntity
    {
        #region Main property
        [ExplicitKey]
        public int BBFFUtilizationID { get; set; }
        public int YBChildID { get; set; }
        public int ExportOrderID { get; set; }
        public int ItemMasterID { get; set; }
        public int SubGroupID { get; set; }
        public int GSM { get; set; }
        public int ColorID { get; set; }
        public int BuyerID { get; set; }
        public string Width { get; set; }
        public string BatchNo { get; set; }
        public decimal FinishFabricUtilizationQTYinkg { get; set; }
        public string WeightSheetNo { get; set; }
        public int GSMID { get; set; }
        public int CompositionID { get; set; }

        public decimal FinishFabricExcessQtyKg { get; set; } = 0;
        public decimal FinishFabricRejectQtyKg { get; set; } = 0;
        public decimal FinishFabricBookingQtyDecreasedbyMerchantQtyKg { get; set; } = 0;
        public decimal FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg { get; set; } = 0;

        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }

        #endregion  Main property

        #region Additional Info
        [Write(false)]
        public string UserName { get; set; }

        [Write(false)]
        public string ExportOrderNo { get; set; }

        [Write(false)]
        public int BookingChildID { get; set; }

        [Write(false)]
        public int ConsumptionID { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string Buyer { get; set; }
        [Write(false)]
        public string FabricConstruction { get; set; }
        [Write(false)]
        public decimal TotalStockQtyinkg { get; set; }
        [Write(false)]
        public decimal ExcessQtyKg { get; set; } = 0;
        [Write(false)]
        public decimal RejectQtyKg { get; set; } = 0;
        [Write(false)]
        public decimal BookingQtyDecreasedbyMerchantQtyKg { get; set; } = 0;
        [Write(false)]
        public decimal AfterProductionOrderCancelledbyMerchantQtyKg { get; set; } = 0;


        [Write(false)]
        public int R_No_New { get; set; }


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BBFFUtilizationID > 0;
        #endregion

        public BulkBookingFinishFabricUtilization()
        {
            BookingChildID = 0;
            ExportOrderNo = "";
            ColorName = "";
            Buyer = "";
            FabricConstruction = "";
            BatchNo = "";
            Width = "";
            TotalStockQtyinkg = 0;
            FinishFabricUtilizationQTYinkg = 0;
            BuyerID = 0;
            ColorID = 0;
            GSM = 0;
            ExportOrderID = 0;
            YBChildID = 0;
            BBFFUtilizationID = 0;
            SubGroupID = 0;
            ItemMasterID = 0;
            WeightSheetNo = "";
            GSMID = 0;
            CompositionID = 0;
        }
    }
}
