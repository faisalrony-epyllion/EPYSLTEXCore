using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Booking
{
    [Table(TableNames.YarnBookingChildItem_New_Revision)]
    public class YarnBookingChildItemRevision : BaseChildItemMaster, IDapperBaseEntity//: DapperBaseEntity 
    {
        [ExplicitKey]
        public int YBChildItemID { get; set; }
        public int YBChildID { get; set; }
        public int YBookingID { get; set; }
        public int YItemMasterID { get; set; }
        public int UnitID { get; set; }
        public bool Blending { get; set; }
        public string YarnCategory { get; set; }
        public decimal Distribution { get; set; }
        public decimal BookingQty { get; set; }
        public decimal Allowance { get; set; }
        public decimal RequiredQty { get; set; }
        public string ShadeCode { get; set; }
        public string Remarks { get; set; }
        public string Specification { get; set; }
        public bool YD { get; set; }
        public bool YDItem { get; set; }
        public decimal StitchLength { get; set; }
        public string PhysicalCount { get; set; }
        public string BatchNo { get; set; }
        public int SpinnerId { get; set; }
        public string YarnLotNo { get; set; }
        public decimal YarnReqQty { get; set; }
        public decimal YarnLeftOverQty { get; set; }
        public decimal NetYarnReqQty { get; set; }
        public decimal YarnBalanceQty { get; set; }
        public int YarnPly { get; set; }
        public decimal GreyAllowance { get; set; }
        public decimal YDAllowance { get; set; }
        public decimal GreyYarnUtilizationQty { get; set; }
        public decimal DyedYarnUtilizationQty { get; set; }

        #region For Fabric Cost Menu Yarn Cost Details Grid

        public decimal AllowanceFM { get; set; } = 0;
        public decimal RequiredQtyFM { get; set; } = 0;
        public decimal SourcingRate { get; set; } = 0;
        public decimal SourcingLandedCost { get; set; } = 0;
        public decimal TotalSourcingRate { get; set; } = 0;
        public decimal DyeingCostFM { get; set; } = 0;

        #endregion For Fabric Cost Menu Yarn Cost Details Grid

        #region Additional Columns 
        //Start :: Default Assign

        #region Yarn Cost footar attribute
        [Write(false)]
        public decimal YarnInKg { get; set; } = 0;
        [Write(false)]
        public decimal YarnValue { get; set; } = 0;
        [Write(false)]
        public decimal FBKInKg { get; set; } = 0;
        [Write(false)]
        public decimal YarnRate { get; set; } = 0;
        [Write(false)]
        public decimal YarnAllowance { get; set; } = 0;
        [Write(false)]
        public int FCMRChildID { get; set; } = 0;

        #endregion Yarn Cost footar attribute




        [Write(false)]
        public string YarnSubProgram { get; set; } = "";

        [Write(false)]
        public decimal CostedAllowance { get; set; } = 0;
        [Write(false)]
        public decimal CostedQty { get; set; } = 0;
        [Write(false)]
        public decimal CostedRate { get; set; } = 0;

        [Write(false)]
        public string SourcingModeName { get; set; } = "";

        [Write(false)]
        public decimal Marineinsurance { get; set; } = 0;

        [Write(false)]
        public decimal CarryingCost { get; set; } = 0;

        [Write(false)]
        public decimal CnFCost { get; set; } = 0;

        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        //End :: Default Assign

        [Write(false)]
        public int[] YarnSubBrandIDs { get; set; }

        [Write(false)]
        public string YarnSubBrandName { get; set; }
        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public string YarnSubBrandID { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        /*[Write(false)]
        public int SubGroupId { get; set; }*/
        [Write(false)]
        public int BookingChildID { get; set; }
        [Write(false)]
        public int ConsumptionID { get; set; }
        [Write(false)]
        public int YBChildGroupID { get; set; }
        //[Write(false)]
        //public int SubGroupID { get; set; }
        [Write(false)]
        public int BookingID { get; set; }
        [Write(false)]
        public string SubGroupName { get; set; }
        /* [Write(false)]
         public int Segment1ValueId { get; set; }
         [Write(false)]
         public int Segment2ValueId { get; set; }
         [Write(false)]
         public int Segment3ValueId { get; set; }
         [Write(false)]
         public int Segment4ValueId { get; set; }
         [Write(false)]
         public int Segment5ValueId { get; set; }
         [Write(false)]
         public int Segment6ValueId { get; set; }
         [Write(false)]
         public int Segment7ValueId { get; set; }
         [Write(false)]
         public int Segment8ValueId { get; set; }
         [Write(false)]
         public int Segment9ValueId { get; set; }
         [Write(false)]
         public int Segment10ValueId { get; set; }
         [Write(false)]
         public int Segment11ValueId { get; set; }
         [Write(false)]
         public int Segment12ValueId { get; set; }
         [Write(false)]
         public int Segment13ValueId { get; set; }
         [Write(false)]
         public int Segment14ValueId { get; set; }
         [Write(false)]
         public int Segment15ValueId { get; set; }
         [Write(false)]
         public string Segment1ValueDesc { get; set; }
         [Write(false)]
         public string Segment2ValueDesc { get; set; }
         [Write(false)]
         public string Segment3ValueDesc { get; set; }
         [Write(false)]
         public string Segment4ValueDesc { get; set; }
         [Write(false)]
         public string Segment5ValueDesc { get; set; }
         [Write(false)]
         public string Segment6ValueDesc { get; set; }
         [Write(false)]
         public string Segment7ValueDesc { get; set; }
         [Write(false)]
         public string Segment8ValueDesc { get; set; }
         [Write(false)]
         public string Segment9ValueDesc { get; set; }
         [Write(false)]
         public string Segment10ValueDesc { get; set; }
         [Write(false)]
         public string Segment11ValueDesc { get; set; }
         [Write(false)]
         public string Segment12ValueDesc { get; set; }
         [Write(false)]
         public string Segment13ValueDesc { get; set; }
         [Write(false)]
         public string Segment14ValueDesc { get; set; }
         [Write(false)]
         public string Segment15ValueDesc { get; set; }*/
        [Write(false)]
        public string ShadeName { get; set; }
        /*[Write(false)]
        public int ItemMasterID { get; set; }*/
        [Write(false)]
        public string Width { get; set; }
        [Write(false)]
        public string Length { get; set; }
        [Write(false)]
        public int YDProductionMasterID { get; set; }
        [Write(false)]
        public List<YarnItemPrice> yarnItemPrice { get; set; }
        [Write(false)]
        public List<YarnBookingChildItemYarnSubBrand> yarnBookingChildItemYarnSubBrand { get; set; }
        [Write(false)]
        public FabricItemPrice_New FabricItemPriceBD { get; set; } = new FabricItemPrice_New();
        [Write(false)]
        public List<FabricItemPrice_New> FabricItemPriceBDList { get; set; } = new List<FabricItemPrice_New>();
        [Write(false)]
        public List<FabricItemPriceAddProcess_New> fabricItemPriceAddProcessList { get; set; } = new List<FabricItemPriceAddProcess_New>();


        #endregion Additional Columns 

        public YarnBookingChildItemRevision()
        {
            Blending = false;
            Distribution = 0m;
            BookingQty = 0m;
            Allowance = 0m;
            RequiredQty = 0m;
            YD = false;
            YDItem = false;
            UnitID = 28;
            YItemMasterID = 0;
            Width = "";
            Length = "";
            BookingChildID = 0;
            ConsumptionID = 0;
            BookingID = 0;
            Spinner = "";
            //Segment1ValueId = 0;
            //Segment2ValueId = 0; 
            //Segment3ValueId = 0; 
            //Segment4ValueId = 0; 
            //Segment5ValueId = 0; 
            //Segment6ValueId = 0; 
            //Segment7ValueId = 0; 
            //Segment8ValueId = 0;
            Segment1ValueDesc = "";
            Segment2ValueDesc = "";
            Segment3ValueDesc = "";
            Segment4ValueDesc = "";
            Segment5ValueDesc = "";
            Segment6ValueDesc = "";
            Segment7ValueDesc = "";
            Segment8ValueDesc = "";
            Segment9ValueDesc = "";
            Segment10ValueDesc = "";
            Segment11ValueDesc = "";
            Segment12ValueDesc = "";
            Segment13ValueDesc = "";
            Segment14ValueDesc = "";
            Segment15ValueDesc = "";
            SubGroupId = 0;
            YarnReqQty = 0;
            YarnLotNo = "";
            YarnLeftOverQty = 0;

            NetYarnReqQty = 0;
            YarnBalanceQty = 0;

            YarnPly = 0;
            GreyAllowance = 0;
            YDAllowance = 0;
            GreyYarnUtilizationQty = 0;
            DyedYarnUtilizationQty = 0;
            StitchLength = 0;
            PhysicalCount = "";
            BatchNo = "";
            SpinnerId = 0;

            YarnSubBrandName = "";
            DisplayUnitDesc = "";
            YBChildGroupID = 0;
            SubGroupName = "";
            ShadeName = "";
            YDProductionMasterID = 0;
            ShadeCode = "";
            Specification = "";

            EntityState = EntityState.Added;
            yarnItemPrice = new List<YarnItemPrice>();
            yarnBookingChildItemYarnSubBrand = new List<YarnBookingChildItemYarnSubBrand>();
        }
    }
}
