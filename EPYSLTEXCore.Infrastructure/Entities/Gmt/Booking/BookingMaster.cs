using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.Booking
{
    [Table(TableNames.BOOKING_MASTER)]
    public class BookingMaster : DapperBaseEntity
    {
        #region Table properties

        [ExplicitKey]
        public int BookingID { get; set; } = 0;

        public string BookingNo { get; set; } = "";

        public int PreProcessRevNo { get; set; } = 0;

        public int RevisionNo { get; set; } = 0;

        public DateTime BookingDate { get; set; }

        public DateTime? RevisionDate { get; set; }

        public int BuyerID { get; set; } = 0;

        public int BuyerTeamID { get; set; } = 0;

        public int CompanyID { get; set; } = 0;

        public int ExportOrderID { get; set; } = 0;

        public int SubGroupID { get; set; } = 0;

        public bool OwnerFactory { get; set; } = false;

        public DateTime? FirstInHouseDate { get; set; }

        public DateTime InHouseDate { get; set; }

        public bool SwatchAttached { get; set; } = false;

        public string ImagePath { get; set; } = "";

        public int SupplierID { get; set; } = 0;

        public int ContactPersonID { get; set; } = 0;

        public int AdditionalBooking { get; set; } = 0;

        public string Remarks { get; set; } = "";

        public string ReferenceNo { get; set; } = "";

        public int ReasonID { get; set; } = 0;

        public bool Proposed { get; set; } = false;

        public bool IsRePurchase { get; set; } = false;

        public int RePurchaseQty { get; set; } = 0;

        public bool IsCancel { get; set; } = false;

        public int CancelReasonID { get; set; } = 0;

        public bool RevisionNeed { get; set; } = false;

        public bool IsSample { get; set; } = false;

        public int SampleID { get; set; } = 0;

        public string ExportOrderNo { get; set; } = "";

        public int AddedBy { get; set; } = 0;

        public DateTime DateAdded { get; set; }

        public int UpdatedBy { get; set; } = 0;

        public DateTime? DateUpdated { get; set; }

        public int CanceledBy { get; set; } = 0;

        public DateTime? DateCanceled { get; set; }

        public bool IsSizeLevel { get; set; } = false;

        public string BookingItemName { get; set; } = "";

        public string CalculateBy { get; set; } = "";

        public bool MustRevise { get; set; } = false;

        public int BlockBookingID { get; set; } = 0;

        public bool SendForRevision { get; set; } = false;

        public bool RevisionAllowed { get; set; } = false;

        public int RevisionAllowRejectBy { get; set; } = 0;

        public DateTime? RevisionAllowRejectDate { get; set; }

        public bool PricePropose { get; set; } = false;

        public DateTime? PriceProposeDate { get; set; }

        public DateTime? FirstPProposeDate { get; set; }

        public int PriceProposeNo { get; set; } = 0;

        public bool PriceAgree { get; set; } = false;

        public DateTime? PriceAgreeDate { get; set; }

        public int PriceAgreeBy { get; set; } = 0;

        public bool PriceSuggest { get; set; } = false;

        public DateTime? PriceSuggestDate { get; set; }

        public int PriceSuggestBy { get; set; } = 0;

        public DateTime? ProposeDate { get; set; }

        public string RevisionReason { get; set; } = "";

        public int PriceReProposeReasonID { get; set; } = 0;

        public bool PriceReSuggest { get; set; } = false;

        public DateTime? PriceReSuggestDate { get; set; }

        public int PriceReSuggestBy { get; set; } = 0;

        public DateTime? PriceReAgreeDate { get; set; }

        public int PriceReAgreeBy { get; set; } = 0;

        public string ReProposeAcceptReason { get; set; } = "";

        public string UnAcknowledgeReason { get; set; } = "";


        #endregion Table properties

        #region Additional Columns


        [Write(false)]
        public int OrderBankMasterID { get; set; } = 0;
        [Write(false)]
        public int YBookingID { get; set; } = 0;
        [Write(false)]
        public string FCStatus { get; set; } = "";
        [Write(false)]
        public string YBookingNo { get; set; } = "";
        [Write(false)]
        public bool IsAcceptPrice { get; set; } = false;
        [Write(false)]
        public bool SendSuggestPrice { get; set; } = false;
        [Write(false)]
        public int HasYarnBooking { get; set; }

        [Write(false)]
        public string ReasonName { get; set; } = "";

        [Write(false)]
        public string CancelReasonName { get; set; } = "";

        [Write(false)]
        public string PriceReProposeReasonName { get; set; } = "";

        [Write(false)]
        public string SupplierName { get; set; } = "";

        [Write(false)]
        public string ContactPerson { get; set; } = "";

        [Write(false)]
        public int IsAcknowledged { get; set; } = 0;

        [Write(false)]
        public string BuyerName { get; set; } = "";

        [Write(false)]
        public string BuyerTeamName { get; set; } = "";

        [Write(false)]
        public string SubGroupName { get; set; } = "";

        [Write(false)]
        public int BOMMasterID { get; set; } = 0;

        [Write(false)]
        public int ItemGroupID { get; set; } = 0;

        [Write(false)]
        public int StyleMasterID { get; set; } = 0;

        [Write(false)]
        public int ContactID { get; set; } = 0;

        [Write(false)]
        public int CategoryTeamID { get; set; } = 0;

        [Write(false)]
        public int? AcknowledgeID { get; set; } = 0;

        [Write(false)]
        public int? ManagementID { get; set; } = 0;

        [Write(false)]
        public int SeasonID { get; set; }

        #region For Fabric Price Acceptance

        [Write(false)]
        public string IsAllowRePropose { get; set; } = "";
        [Write(false)]
        public string SaveType { get; set; } = "";
        [Write(false)]
        public string PriceStatus { get; set; } = "";

        [Write(false)]
        public string PriceProposeDateTime { get; set; } = "";

        [Write(false)]
        public string PriceSuggestDateTime { get; set; } = "";

        [Write(false)]
        public string StyleNo { get; set; } = "";

        [Write(false)]
        public string BookingType { get; set; } = "";

        [Write(false)]
        public bool WithoutOB { get; set; } = false;

        [Write(false)]
        public int EWOStatusID { get; set; } = 0;

        [Write(false)]
        public List<BookingChild_New> BookingChildDetailsFabric { get; set; }

        [Write(false)]
        public List<BookingChild_New> BookingChildDetailsCollar { get; set; }

        [Write(false)]
        public List<BookingChild_New> BookingChildDetailsCuff { get; set; }

        [Write(false)]
        public List<GridColumnInfo> oGridColumnInfoFabricList { get; set; } = new List<GridColumnInfo>();
        [Write(false)]
        public List<GridColumnInfo> oGridColumnInfoCollarList { get; set; } = new List<GridColumnInfo>();
        [Write(false)]
        public List<GridColumnInfo> oGridColumnInfoCuffList { get; set; } = new List<GridColumnInfo>();
        [Write(false)]
        public List<GridColumnInfo> oGridColumnInfoYarnCountsList { get; set; } = new List<GridColumnInfo>();
        [Write(false)]
        public List<YarnItemPrice> BookingDetailsInfoList { get; set; } = new List<YarnItemPrice>();
        [Write(false)]
        public List<FabricItemPriceBD> FabricItemPriceBDData { get; set; } = new List<FabricItemPriceBD>();
        [Write(false)]
        public List<FabricItemPriceBDChild> FabricItemPriceBDChildData { get; set; } = new List<FabricItemPriceBDChild>();
        [Write(false)]
        public List<YarnItemPrice> CollarBookingDetailsInfoList { get; set; } = new List<YarnItemPrice>();
        [Write(false)]
        public List<YarnItemPrice> CuffBookingDetailsInfoList { get; set; } = new List<YarnItemPrice>();

        [Write(false)]
        public List<SampleBookingMaster> oSampleBookingMasterList { get; set; } = new List<SampleBookingMaster>();

        #endregion


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || SubGroupID > 0;

        #endregion Additional Columns

        public BookingMaster()
        {
            SeasonID = 0;
            HasYarnBooking = 0;
            BookingChildDetailsFabric = new List<BookingChild_New>();
            BookingChildDetailsCollar = new List<BookingChild_New>();
            BookingChildDetailsCuff = new List<BookingChild_New>();
        }
    }
}
