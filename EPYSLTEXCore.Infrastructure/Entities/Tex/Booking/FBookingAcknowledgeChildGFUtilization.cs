using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.FBookingAcknowledgeChildGFUtilization)]
    public class FBookingAcknowledgeChildGFUtilization : DapperBaseEntity
    {
        [ExplicitKey]
        public int GFUtilizationID { get; set; }
        public int YBChildID { get; set; }
        public int ExportOrderID { get; set; }
        public int ItemMasterID { get; set; }
        public int SubGroupID { get; set; }
        public int GSMID { get; set; }
        public int ColorID { get; set; }
        public int BuyerID { get; set; }
        public int FabricTypeID { get; set; }
        public int CompositionID { get; set; }
        public string GSM { get; set; }
        public string FabricStyle { get; set; }
        public decimal GreyFabricUtilizationQTYinkg { get; set; }


        #region Additional Properties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || GFUtilizationID > 0;

        [Write(false)]
        public int RowNumber { get; set; }

        [Write(false)]
        public string ExportOrderNo { get; set; }

        [Write(false)]
        public string Buyer { get; set; }
        [Write(false)]
        public string FabricType { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public int BookingChildID { get; set; }
        [Write(false)]
        public int ConsumptionID { get; set; }
        [Write(false)]
        public string PhysicalCount { get; set; } = "";
        [Write(false)]
        public decimal DyedYarnUtilizationQty { get; set; } = 0M;
        #endregion Additional Properties

        public FBookingAcknowledgeChildGFUtilization()
        {
            GFUtilizationID = 0;
            YBChildID = 0;
            ExportOrderID = 0;
            ItemMasterID = 0;
            SubGroupID = 0;
            GSMID = 0;
            ColorID = 0;
            BuyerID = 0;
            FabricTypeID = 0;
            CompositionID = 0;
            GSM = "";
            FabricStyle = "";
            GreyFabricUtilizationQTYinkg = 0;

            RowNumber = 0;
            ExportOrderNo = "";
            Buyer = "";
            FabricType = "";
            ColorName = "";
            Composition = "";
        }
    }
}
