using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Fabric
{
    [Table("T_FabricItemPrice")]
    public class FabricItemPrice_New : DapperBaseEntity
    {
        #region Table property
        [ExplicitKey]
        public int FIPriceID { get; set; }

        public int RevisionNo { get; set; }

        public int YBookingID { get; set; }

        public int ConsumptionID { get; set; }

        public int ItemMasterID { get; set; }

        public int UnitID { get; set; }

        public decimal BookingQty { get; set; }

        public decimal Price { get; set; }

        public decimal PriceInPcs { get; set; }

        public decimal SuggestedPrice { get; set; }

        public int SuggestedStatus { get; set; }

        public int NoOfChange { get; set; }

        public bool SendToApproval { get; set; }

        public bool Approve { get; set; }

        public bool UnApproveFP { get; set; }

        public bool Agree { get; set; }

        public bool IsOldFormat { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        public string Remarks { get; set; }

        public bool IsTextileERP { get; set; }

        #endregion Table property

        #region Additional Columns

        [Write(false)]
        public int BookingID { get; set; } = 0;
        [Write(false)]
        public int YBChildID { get; set; } = 0;
        [Write(false)]
        public int ItemGroupID { get; set; } = 0;
        [Write(false)]
        public int SubGroupID { get; set; } = 0;
        [Write(false)]
        public string SubGroupName { get; set; } = "";

        #region Fabric Cost Pupup obj
        #region YarnCost
        [Write(false)]
        public decimal YarnCostBeforePercent { get; set; } = 0;
        [Write(false)]
        public decimal YarnCostPercent { get; set; } = 0;
        [Write(false)]
        public decimal YarnCost { get; set; } = 0;

        #endregion YarnCost

        #region ProcessCost
        [Write(false)]
        public decimal TotalProcessCostBeforePercent { get; set; } = 0;
        [Write(false)]
        public decimal TotalProcessCostPercent { get; set; } = 0;
        [Write(false)]
        public decimal TotalProcessCost { get; set; } = 0;

        #region KnittingCost
        [Write(false)]
        public decimal KnittingCost { get; set; } = 0;
        [Write(false)]
        public decimal KnittingCostPercent { get; set; } = 0;
        [Write(false)]
        public decimal KnittingCostAfterPercent { get; set; } = 0;
        #endregion KnittingCost
        #region DyeingCost
        [Write(false)]
        public decimal DyeingCost { get; set; } = 0;
        [Write(false)]
        public decimal DyeingCostPercent { get; set; } = 0;
        [Write(false)]
        public decimal DyeingCostAfterPercent { get; set; } = 0;
        #endregion
        #region DyeingCost YD
        [Write(false)]
        public decimal YDCost { get; set; } = 0;
        [Write(false)]
        public decimal YDCostPercent { get; set; } = 0;
        [Write(false)]
        public decimal YDCostAfterPercent { get; set; } = 0;
        #endregion

        #region FinishingAndCompactingCost
        [Write(false)]
        public decimal FinishingCost { get; set; } = 0;
        [Write(false)]
        public decimal FinishingCostPercent { get; set; } = 0;
        [Write(false)]
        public decimal FinishingCostAfterPercent { get; set; } = 0;

        #endregion FinishingAndCompactingCost 
        #region AdditionalProcessCost
        [Write(false)]
        public decimal AddProcessCost { get; set; } = 0;
        [Write(false)]
        public decimal AddProcessCostPercent { get; set; } = 0;
        [Write(false)]
        public decimal AddProcessCostAfterPercent { get; set; } = 0;
        #endregion AdditionalProcessCost

        #endregion ProcessCost

        #region TotalVariableCost

        [Write(false)]
        public decimal TVariableCost { get; set; } = 0;

        #endregion TotalVariableCost

        #region FixedCost

        [Write(false)]
        public decimal FixedCost { get; set; } = 0.75M;

        #endregion FixedCost

        #region TotalCost

        [Write(false)]
        public decimal TotalCost { get; set; } = 0;

        #endregion TotalCost

        #region Markup

        [Write(false)]
        public decimal TotalMarkupPercent { get; set; } = 0;
        [Write(false)]
        public decimal TotalMarkup { get; set; } = 0;

        #endregion Markup

        #region TotalFabricCost

        [Write(false)]
        public decimal QFabricPrice { get; set; } = 0;

        #endregion TotalFabricCost

        #region  Yarn Cost Details Footer
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
        #endregion


        #endregion Fabric Cost Pupup obj

        [Write(false)]
        public List<FabricItemPriceBD> oFabricItemPriceBDList { get; set; } = new List<FabricItemPriceBD>();
        [Write(false)]
        public List<FabricItemPriceBDChild> oFabricItemPriceBDChildList { get; set; } = new List<FabricItemPriceBDChild>();
        [Write(false)]
        public List<FabricItemPriceAddProcess_New> oFabricItemPriceAddProcessList { get; set; } = new List<FabricItemPriceAddProcess_New>();
        [Write(false)]
        public List<YarnItemPrice> oYarnItemPriceList { get; set; } = new List<YarnItemPrice>();



        [Write(false)]
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
        public int Segment12ValueId { get; set; }

        [Write(false)]
        public int YarnTypeID { get; set; } = 0;
        [Write(false)]
        public int YarnBrandID { get; set; } = 0;



        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FIPriceID > 0;

        #endregion Additional Columns
    }
}
