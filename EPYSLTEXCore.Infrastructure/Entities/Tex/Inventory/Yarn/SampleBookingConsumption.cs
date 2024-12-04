using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.SAMPLE_BOOKING_CONSUMPTION)]
    public class SampleBookingConsumption : DapperBaseEntity
    {
        public SampleBookingConsumption()
        {
            Childs = new List<SampleBookingConsumptionChild>();
        }

        [ExplicitKey]
        public int ConsumptionID { get; set; }

        public int BookingID { get; set; }

        public int ItemGroupID { get; set; }

        public int SubGroupID { get; set; }

        public int Segment1ValueID { get; set; }

        public string Segment1Desc { get; set; }

        public int Segment2ValueID { get; set; }

        public string Segment2Desc { get; set; }

        public int Segment3ValueID { get; set; }

        public string Segment3Desc { get; set; }

        public int Segment4ValueID { get; set; }

        public string Segment4Desc { get; set; }

        public int Segment5ValueID { get; set; }

        public string Segment5Desc { get; set; }

        public int Segment6ValueID { get; set; }

        public string Segment6Desc { get; set; }

        public int Segment7ValueID { get; set; }

        public string Segment7Desc { get; set; }

        public int Segment8ValueID { get; set; }

        public string Segment8Desc { get; set; }

        public int Segment9ValueID { get; set; }

        public string Segment9Desc { get; set; }

        public int Segment10ValueID { get; set; }

        public string Segment10Desc { get; set; }

        public int Segment11ValueID { get; set; }

        public string Segment11Desc { get; set; }

        public int Segment12ValueID { get; set; }

        public string Segment12Desc { get; set; }

        public int Segment13ValueID { get; set; }

        public string Segment13Desc { get; set; }

        public int Segment14ValueID { get; set; }

        public string Segment14Desc { get; set; }

        public int Segment15ValueID { get; set; }

        public string Segment15Desc { get; set; }

        public int LengthYds { get; set; }

        public decimal LengthInch { get; set; }

        public string ForFabAddProcess { get; set; }

        public int FUPartID { get; set; }

        public decimal ConsumptionQty { get; set; }

        public int ConsumptionUnitID { get; set; }

        public int PerGarment { get; set; }

        public int PerGarmentUnitID { get; set; }

        public decimal Wastage { get; set; }

        public int OrderUnitID { get; set; }

        public string ForProcess { get; set; }

        public int A1ValueID { get; set; }

        public int YarnBrandID { get; set; }

        public string YarnSubBrandName { get; set; }

        public string LabDipNo { get; set; }

        public string Remarks { get; set; }

        public int MeasurementUnitID { get; set; }

        public decimal RequiredQty { get; set; }

        public string ForBDSStyleNo { get; set; }

        public int BDSOrderQty { get; set; }

        public string PartName { get; set; }

        public decimal YarnFormInKG { get; set; }

        public decimal GreyFabricInKG { get; set; }

        public bool AutoAgree { get; set; }

        public decimal Price { get; set; }

        public decimal SuggestedPrice { get; set; }

        public string ForGarmentColors { get; set; }

        public DateTime? LabdipUpdateDate { get; set; }

        public int ReferenceSourceID { get; set; }

        public string ReferenceNo { get; set; }

        public string ColorReferenceNo { get; set; }

        public int YarnSourceID { get; set; }

        public string CareCode { get; set; }

        public bool IsFabricReq { get; set; }


        #region Additional Columns
        [Write(false)]
        public int ItemMasterID { get; set; } = 0;
        [Write(false)]
        public List<SampleBookingConsumptionChild> Childs { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.ConsumptionID > 0;
        #endregion Additional Columns

    }
}
