using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table("T_YarnItemPrice")]
    public class YarnItemPrice : YarnItemMaster, IDapperBaseEntity//AuditFields, IBaseEntity
    {
        [ExplicitKey]
        public int YIPriceID { get; set; }
        public int RevisionNo { get; set; }
        public int YBookingID { get; set; }
        public int YBChildItemID { get; set; }
        public decimal RequiredQty { get; set; }
        public decimal CostedAllowance { get; set; }
        public decimal CostedQty { get; set; }
        public decimal Rate { get; set; }
        public bool NeedApproval { get; set; }
        public bool PriceApprove { get; set; }
        public int ReasonID { get; set; }
        public bool Approve { get; set; }
        public int YTAReasonID { get; set; }
        public string Remarks { get; set; }
        public int SetupID { get; set; }
        public decimal LandedCost { get; set; }
        public decimal TotalRate { get; set; }
        public decimal CostedRate { get; set; }
        public bool SourcingMode { get; set; }
        public int SupplierID { get; set; }
        public string LotNo { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }
        public string YarnCategory { get; set; }
        public string ShadeCode { get; set; }
        public string Specification { get; set; }
        public decimal SourcingRate { get; set; }
        public decimal SourcingLandedCost { get; set; }
        public decimal TotalSourcingRate { get; set; }
        public decimal AllowanceFM { get; set; }
        public decimal RequiredQtyFM { get; set; }
        public bool YDItem { get; set; }
        public bool YD { get; set; }
        public decimal DyeingCost { get; set; }
        public decimal DyeingCostFM { get; set; }
        public bool IsTextileERP { get; set; } = true;

        #region Additional Fields

        [Write(false)]
        public int YBChildID { get; set; } = 0;
        [Write(false)]
        public int YarnBrandID { get; set; } = 0;
        [Write(false)]
        public int YarnTypeID { get; set; } = 0;
        [Write(false)]
        public int SuggestedStatus { get; set; } = 0;
        [Write(false)]
        public bool SendToApproval { get; set; } = false;
        [Write(false)]
        public bool UnApproveFP { get; set; } = false;
        [Write(false)]
        public bool Agree { get; set; } = false;
        [Write(false)]
        public int ItemGroupID { get; set; } = 0;
        [Write(false)]
        public int FIPriceID { get; set; } = 0;
        [Write(false)]
        public int BookingID { get; set; } = 0;
        [Write(false)]
        public int SubGroupID { get; set; } = 0;
        [Write(false)]
        public int StyleMasterID { get; set; } = 0;
        [Write(false)]
        public string FCStatus { get; set; } = "";
        [Write(false)]
        public string ExportOrderNo { get; set; } = "";
        [Write(false)]
        public string BookingNo { get; set; } = "";
        [Write(false)]
        public string YBookingNo { get; set; } = "";
        [Write(false)]
        public string StyleNo { get; set; } = "";
        [Write(false)]
        public string BuyerName { get; set; } = "";
        [Write(false)]
        public string BuyerTeam { get; set; } = "";
        [Write(false)]
        public string BusinessUnitShortName { get; set; } = "";
        [Write(false)]
        public bool IsAgreed { get; set; } = false;
        [Write(false)]
        public bool WithoutOB { get; set; } = false;

        [Write(false)]
        public DateTime? AcknowledgeDate { get; set; }
        [Write(false)]
        public DateTime? TNADate { get; set; }
        [Write(false)]
        public DateTime? FabricDeliveryStartDate { get; set; }
        [Write(false)]
        public decimal Price { get; set; } = 0;
        [Write(false)]
        public string FBBookingUOM { get; set; } = "";
        [Write(false)]
        public int LengthYds { get; set; } = 0;
        [Write(false)]
        public decimal LengthInch { get; set; } = 0;
        [Write(false)]
        public string A1Desc { get; set; } = "";
        [Write(false)]
        public string YarnBrandName { get; set; } = "";
        [Write(false)]
        public decimal BookingQty { get; set; } = 0;
        [Write(false)]
        public string BookingUOM { get; set; } = "";
        [Write(false)]
        public decimal FBBookingQty { get; set; } = 0;
        [Write(false)]
        public decimal PORate { get; set; } = 0;
        [Write(false)]
        public decimal PriceInPcs { get; set; } = 0;
        [Write(false)]
        public decimal CostedPrice { get; set; } = 0;
        [Write(false)]
        public string PriceStatus { get; set; } = "";



        [Write(false)]
        public decimal SuggestedPrice { get; set; } = 0;
        [Write(false)]
        public int ConsumptionID { get; set; } = 0;
        [Write(false)]
        public string YarnSubProgram { get; set; } = "";
        [Write(false)]
        public bool ISFullAOP { get; set; } = false;
        [Write(false)]
        public string SeasonName { get; set; } = "";
        [Write(false)]
        public string SubGroupName { get; set; } = "";

        [Write(false)]
        public List<FabricItemPriceBD> FabricItemPriceBDList { get; set; } = new List<FabricItemPriceBD>();

        [Write(false)]
        public List<FabricItemPriceBDChild> FabricItemPriceBDChildList { get; set; } = new List<FabricItemPriceBDChild>();

        [Write(false)]
        public List<YarnItemPrice> CollarCuffList { get; set; } = new List<YarnItemPrice>();

        [Write(false)]
        public List<YarnBookingChildItem> YarnCostDetailsList { get; set; } = new List<YarnBookingChildItem>();
        [Write(false)]
        public List<FabricItemPriceAddProcess_New> AdditionalProcessCostDetails { get; set; } = new List<FabricItemPriceAddProcess_New>();

        [Write(false)]
        public FabricItemPrice_New FabricCostDetailsList { get; set; } = new FabricItemPrice_New();

        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public int TotalRows { get; set; }
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public int IsSaveFlag { get; set; }
        [Write(false)]
        public YarnItemApprovalReason YarnItemApprovalReason { get; set; }
        #endregion Additional Fields 
        public YarnItemPrice()
        {
            RevisionNo = 0;
            RequiredQty = 0m;
            CostedAllowance = 0m;
            CostedQty = 0m;
            Rate = 0m;
            NeedApproval = false;
            PriceApprove = false;
            ReasonID = 0;
            Approve = false;
            YTAReasonID = 0;
            SetupID = 0;
            YD = false;
            YDItem = false;
            LandedCost = 0m;
            TotalRate = 0m;
            CostedRate = 0m;
            SourcingMode = false;
            SupplierID = 0;
            LotNo = "N/A";
            DateAdded = DateTime.Now;
            DateUpdated = DateTime.Now;
            EntityState = EntityState.Added;
        }
    }
}
