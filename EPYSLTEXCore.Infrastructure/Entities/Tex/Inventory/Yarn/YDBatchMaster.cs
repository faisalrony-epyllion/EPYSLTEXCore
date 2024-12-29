using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_BATCH_MASTER)]
    public class YDBatchMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDBatchID { get; set; }

        public string YDBatchNo { get; set; }
        public System.DateTime YDBatchDate { get; set; }
        public int YDRecipeID { get; set; } = 0;
        public int ConceptID { get; set; } = 0;
        public int YDBookingMasterID { get; set; } = 0;
        public int ExportOrderID { get; set; } = 0;
        public int BuyerID { get; set; } = 0;
        public int BuyerTeamID { get; set; } = 0;
        public int ColorID { get; set; } = 0;
        public decimal BatchWeightKG { get; set; } = 0;
        public decimal BatchWeightPcs { get; set; } = 0;
        public int ShiftID { get; set; } = 0;
        public int OperatorID { get; set; } = 0;
        public System.DateTime? BatchStartTime { get; set; }
        public System.DateTime? BatchEndTime { get; set; }
        public System.DateTime? ProductionDate { get; set; }
        public bool IsApproved { get; set; } = false;
        public int ApprovedBy { get; set; } = 0;
        public System.DateTime? ApprovedDate { get; set; }
        public string Remarks { get; set; }
        public int AddedBy { get; set; } = 0;
        public System.DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; } = 0;
        public System.DateTime? DateUpdated { get; set; }
        public int CCColorID { get; set; } = 0;
        public int DMID { get; set; } = 0;
        public int DyeingNozzleQty { get; set; } = 0;
        public decimal MachineLoading { get; set; } = 0;
        public int DyeingMcCapacity { get; set; } = 0;
        public bool QCPass { get; set; } = false;
        public bool QCFail { get; set; } = false;

        #region Additional Columns

        [Write(false)]
        public string RecipeForName { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        //[Write(false)]
        //public string YDRecipeNo { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public string YDBookingNo { get; set; }

        [Write(false)]
        public string RecipeFor { get; set; }

        //[Write(false)]
        //public DateTime? YDRecipeDate { get; set; }

        [Write(false)]
        public string ShiftName { get; set; }

        [Write(false)]
        public string OperatorName { get; set; }

        [Write(false)]
        public string DyeingMcStatus { get; set; }

        [Write(false)]
        public string Company { get; set; }

        [Write(false)]
        public string DyeingMcName { get; set; }

        [Write(false)]
        public string DyeingMcBrand { get; set; }

        [Write(false)]
        public string DMNo { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public decimal BookingQty { get; set; } = 0;
        [Write(false)]
        public decimal BatchPlannedQty { get; set; } = 0;
        [Write(false)]
        public int IsBDS { get; set; } = 0;
        [Write(false)]
        public string BookingByUser { get; set; }
        [Write(false)]
        public int ItemMasterID { get; set; } = 0;

        [Write(false)]
        public List<Select2OptionModel> UnitList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ShiftList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> OperatorList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> MachineList { get; set; }

        [Write(false)]
        public List<YDBatchItemRequirement> YDBatchItemRequirements { get; set; }

        [Write(false)]
        public List<YDBatchWiseRecipeChild> YDBatchWiseRecipeChilds { get; set; }
        [Write(false)]
        public List<YDBatchItemRequirement> YDBatchOtherItemRequirements { get; set; }

        [Write(false)]
        public string RecipeNo { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }

        [Write(false)]
        public DateTime? RecipeDate { get; set; }

        [Write(false)]
        public string SLNo { get; set; }

        [Write(false)]
        public string BuyerTeamName { get; set; }

        [Write(false)]
        public List<KnittingProduction> KnittingProductions { get; set; }
        [Write(false)]
        public int TotalQty { get; set; } = 0;
        [Write(false)]
        public List<YDRecipeDefinitionChild> YDDefChilds { get; set; }


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDBatchID > 0;

        #endregion Additional Columns

        public YDBatchMaster()
        {
            YDBatchNo = AppConstants.NEW;
            YDBatchDate = DateTime.Now;
            YDBatchItemRequirements = new List<YDBatchItemRequirement>();
            YDBatchWiseRecipeChilds = new List<YDBatchWiseRecipeChild>();

            ExportOrderID = 0;
            BuyerID = 0;
            BuyerTeamID = 0;
            TotalQty = 0;
            BatchWeightKG = 0m;
            DateAdded = DateTime.Now;
            ProductionDate = DateTime.Now;
            BuyerName = "";
            BuyerTeamName = "";
            EntityState = System.Data.Entity.EntityState.Added;
            YDBatchItemRequirements = new List<YDBatchItemRequirement>();
            YDBatchOtherItemRequirements = new List<YDBatchItemRequirement>();
            YDBatchWiseRecipeChilds = new List<YDBatchWiseRecipeChild>();
            SLNo = "";
        }
    }
}
