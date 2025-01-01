using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_DYEING_BATCH_MASTER)]
    public class YDDyeingBatchMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDDBatchID { get; set; }
        public string YDDBatchNo { get; set; }
        public DateTime YDDBatchDate { get; set; }
        public int YDRecipeID { get; set; }
        public int ColorID { get; set; }
        public int CCColorID { get; set; }
        public decimal BatchWeightKG { get; set; }
        public decimal BatchQtyPcs { get; set; }
        public int DMID { get; set; }
        public decimal MachineLoading { get; set; }
        public int DyeingNozzleQty { get; set; }
        public int DyeingMcCapacity { get; set; }
        public int ShiftID { get; set; }
        public int UnloadShiftID { get; set; }
        public int OperatorID { get; set; }
        public int UnloadOperatorID { get; set; }
        public DateTime? BatchStartTime { get; set; }
        public DateTime? BatchEndTime { get; set; }
        public DateTime? PlanBatchStartTime { get; set; }
        public DateTime? PlanBatchEndTime { get; set; }
        public DateTime? ProductionDate { get; set; }
        public bool IsApproved { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Remarks { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }
        public bool PreProductionComplete { get; set; }
        public bool PostProductionComplete { get; set; }
        public int BatchStatus { get; set; }
        public bool IsNewBatch { get; set; }
        public bool IsNewRecipe { get; set; }
        public bool IsRedyeingBatch { get; set; }


        #region Additional Property 
        [Write(false)]
        public List<YDDyeingBatchRework> Childs { get; set; }
        [Write(false)]
        public List<YDDyeingBatchItem> YDDyeingBatchItems { get; set; }
        [Write(false)]
        public List<YDDyeingBatchWithBatchMaster> YDDyeingBatchWithBatchMasters { get; set; }
        [Write(false)]
        public List<YDDyeingBatchRecipe> YDDyeingBatchRecipes { get; set; }
        [Write(false)]
        public List<RollFinishingInfo> RollFinishingInfoChilds { get; set; }
        [Write(false)]
        public List<YDDyeingBatchChildFinishingProcess> YDDyeingBatchChildFinishingProcesses { get; set; }
        [Write(false)]
        public List<YDDyeingBatchMergeBatch> YDDyeingBatchMergeBatchs { get; set; }
        [Write(false)]
        public List<YDDyeingBatchMaster> YDDyeingBatches { get; set; }
        [Write(false)]
        public string YDRecipeNo { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string BuyerTeamName { get; set; }
        [Write(false)]
        public string SLNo { get; set; }
        [Write(false)]
        public DateTime? YDRecipeDate { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string ShiftName { get; set; }
        [Write(false)]
        public string UnloadShiftName { get; set; }
        [Write(false)]
        public string OperatorName { get; set; }
        [Write(false)]
        public string UnloadOperatorName { get; set; }
        [Write(false)]
        public int YDBatchID { get; set; } = 0;
        [Write(false)]
        public string YDBatchNo { get; set; }
        [Write(false)]
        public string DMNo { get; set; }
        [Write(false)]
        public DateTime YDBatchDate { get; set; }
        [Write(false)]
        public int YDRecipeFor { get; set; }
        [Write(false)]
        public string YDRecipeForName { get; set; }
        [Write(false)]
        public decimal TotalBatchUseQtyKG { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public int ConceptID { get; set; } = 0;
        [Write(false)]
        public int BookingID { get; set; } = 0;
        [Write(false)]
        public int SubGroupID { get; set; } = 0;
        [Write(false)]
        public int DBIID { get; set; } = 0;
        [Write(false)]
        public int ItemStatus1 { get; set; } = 0;
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public string KnittingType { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public string Composition { get; set; }
        [Write(false)]
        public string Gsm { get; set; }
        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public int FUPartID { get; set; }
        [Write(false)]
        public string FUPartName { get; set; }
        [Write(false)]
        public int KnittingTypeID { get; set; }
        [Write(false)]
        public int TechnicalNameID { get; set; }
        [Write(false)]
        public decimal Length { get; set; }
        [Write(false)]
        public decimal Width { get; set; }
        [Write(false)]
        public int CompositionID { get; set; }
        [Write(false)]
        public int GSMID { get; set; }
        [Write(false)]
        public string Unit { get; set; }
        [Write(false)]
        public bool IsRedyeingProcess { get; set; }
        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        public string ExistingDBatchNo { get; set; }
        [Write(false)]
        public bool IsParentDBatch { get; set; }
        [Write(false)]
        public bool IsRework { get; set; }
        [Write(false)]
        public decimal BatchWeightPcs { get; set; } = 0;
        [Write(false)]
        public int ExportOrderID { get; set; } = 0;
        [Write(false)]
        public int BuyerID { get; set; } = 0;
        [Write(false)]
        public int BuyerTeamID { get; set; } = 0;
        [Write(false)]
        public List<YDDyeingBatchItemRoll> YDDyeingBatchItemRolls { get; set; }
        [Write(false)]
        public List<Select2OptionModel> ShiftList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> UnloadShiftList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> OperatorList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> UnloadOperatorList { get; set; }
        [Write(false)]
        public List<KnittingProduction> KnittingProductions { get; set; }
        [Write(false)]
        public List<Select2OptionModel> UnitList { get; set; }
        [Write(false)]
        public List<FinishingMachineConfigurationChild> FinishingMachineConfigurationChildList { get; set; }
        [Write(false)]
        public List<YDRecipeDefinitionChild> DefChilds { get; set; } = new List<YDRecipeDefinitionChild>();
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDDBatchID > 0;

        #endregion Additional Property

        public YDDyeingBatchMaster()
        {
            DateAdded = DateTime.Now;
            Childs = new List<YDDyeingBatchRework>();
            YDDyeingBatchItems = new List<YDDyeingBatchItem>();
            YDDyeingBatchWithBatchMasters = new List<YDDyeingBatchWithBatchMaster>();
            YDDyeingBatchRecipes = new List<YDDyeingBatchRecipe>();
            RollFinishingInfoChilds = new List<RollFinishingInfo>();
            YDDyeingBatchChildFinishingProcesses = new List<YDDyeingBatchChildFinishingProcess>();
            YDDyeingBatchMergeBatchs = new List<YDDyeingBatchMergeBatch>();
            YDDyeingBatchItemRolls = new List<YDDyeingBatchItemRoll>();
            YDDyeingBatches = new List<YDDyeingBatchMaster>();
            YDDBatchNo = AppConstants.NEW;
            YDDBatchDate = DateTime.Now;
            PlanBatchEndTime = DateTime.Now;
            PlanBatchStartTime = DateTime.Now;
            ConceptID = 0;
            BatchStatus = 0;
            IsNewBatch = false;
            IsNewRecipe = false;
            IsRedyeingBatch = false;

            IsRedyeingProcess = false;
            IsParentDBatch = false;
            IsRework = false;
        }
    }
}
