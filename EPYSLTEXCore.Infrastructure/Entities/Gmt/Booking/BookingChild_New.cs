using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking
{
    [Table(TableNames.BOOKING_CHILD)]
    public class BookingChild_New : DapperBaseEntity
    {
        #region Table property
        [ExplicitKey]
        public int BookingChildID { get; set; } = 0;
        public int BookingID { get; set; } = 0;
        public int ConsumptionChildID { get; set; } = 0;
        public int ConsumptionID { get; set; } = 0;
        public int BOMMasterID { get; set; } = 0;
        public int ExportOrderID { get; set; } = 0;
        public int ItemGroupID { get; set; } = 0;
        public int SubGroupID { get; set; } = 0;
        public int ItemMasterID { get; set; } = 0;
        public int OrderBankPOID { get; set; } = 0;
        public int ColorID { get; set; } = 0;
        public int SizeID { get; set; } = 0;
        public int TechPackID { get; set; } = 0;
        public decimal ConsumptionQty { get; set; } = 0;
        public decimal BookingQty { get; set; } = 0;
        public int BookingUnitID { get; set; } = 0;
        public decimal RequisitionQty { get; set; } = 0;
        public bool ISourcing { get; set; } = false;
        public string Remarks { get; set; } = "";
        public int LengthYds { get; set; } = 0;
        public decimal LengthInch { get; set; } = 0;
        public int FUPartID { get; set; } = 0;
        public int A1ValueID { get; set; } = 0;
        public int YarnBrandID { get; set; } = 0;
        public int ContactID { get; set; } = 0;
        public string LabDipNo { get; set; } = "";
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }
        public int ExecutionCompanyID { get; set; } = 0;
        public decimal BlockBookingQty { get; set; } = 0;
        public decimal AdjustQty { get; set; } = 0;
        public bool AutoAgree { get; set; } = false;
        public decimal Price { get; set; } = 0;
        public decimal SuggestedPrice { get; set; } = 0;
        public DateTime? LabdipUpdateDate { get; set; }
        public bool IsCompleteReceive { get; set; } = false;
        public bool IsCompleteDelivery { get; set; } = false;
        public DateTime? LastDCDate { get; set; }
        public string ClosingRemarks { get; set; } = "";
        public int ToItemMasterID { get; set; } = 0;
        public string CareCode { get; set; } = "";
        #endregion Table property


        #region Additional props

        [Write(false)]
        public string BookingUOM { get; set; } = "";
        [Write(false)]
        public string BookingNo { get; set; } = "";
        [Write(false)]
        public string YarnBrandName { get; set; } = "";

        [Write(false)]
        public string A1Desc { get; set; } = "";

        [Write(false)]
        public string YarnSubBrandName { get; set; } = "";

        [Write(false)]
        public string PartName { get; set; } = "";

        [Write(false)]
        public string SubGroupName { get; set; } = "";


        #region Segment 
        [Write(false)]
        public string Segment1ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment2ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment3ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment4ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment5ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment6ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment7ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment8ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment9ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment10ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment11ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment12ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment13ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment14ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment15ValueDesc { get; set; } = "";

        #endregion

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BookingChildID > 0;


        #endregion
    }
}
