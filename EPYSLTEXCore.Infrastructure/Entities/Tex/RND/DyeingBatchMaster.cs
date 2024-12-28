using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Statics;
using System;
using System.Collections.Generic;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("DyeingBatchMaster")]
    public class DyeingBatchMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int DBatchID { get; set; }
        public string DBatchNo { get; set; }
        public DateTime DBatchDate { get; set; }
        public int RecipeID { get; set; }
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
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool PreProductionComplete { get; set; }
        public bool PostProductionComplete { get; set; }
        public int BatchStatus { get; set; }
        public bool IsNewBatch { get; set; }
        public bool IsNewRecipe { get; set; }
        public bool IsRedyeingBatch { get; set; }


        #region Additional Property 
        [Write(false)]
        public List<DyeingBatchRework> Childs { get; set; }
        [Write(false)]
        public List<DyeingBatchItem> DyeingBatchItems { get; set; }
        [Write(false)]
        public List<DyeingBatchWithBatchMaster> DyeingBatchWithBatchMasters { get; set; }
        [Write(false)]
        public List<DyeingBatchRecipe> DyeingBatchRecipes { get; set; }
        [Write(false)]
        public List<RollFinishingInfo> RollFinishingInfoChilds { get; set; }
        [Write(false)]
        public List<DyeingBatchChildFinishingProcess> DyeingBatchChildFinishingProcesses { get; set; }
        [Write(false)]
        public List<DyeingBatchMergeBatch> DyeingBatchMergeBatchs { get; set; }
        [Write(false)]
        public List<DyeingBatchMaster> DyeingBatches { get; set; }
        [Write(false)]
        public string RecipeNo { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string BuyerTeamName { get; set; }
        [Write(false)]
        public string SLNo { get; set; }
        [Write(false)]
        public DateTime? RecipeDate { get; set; }
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
        public int BatchID { get; set; }
        [Write(false)]
        public string BatchNo { get; set; }
        [Write(false)]
        public string DMNo { get; set; }
        [Write(false)]
        public DateTime BatchDate { get; set; }
        [Write(false)]
        public int RecipeFor { get; set; }
        [Write(false)]
        public string RecipeForName { get; set; }
        [Write(false)]
        public decimal TotalBatchUseQtyKG { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public int ConceptID { get; set; }
        [Write(false)]
        public int SubGroupID { get; set; }
        [Write(false)]
        public int DBIID { get; set; }
        [Write(false)]
        public int ItemStatus1 { get; set; }
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
        public List<DyeingBatchItemRoll> DyeingBatchItemRolls { get; set; }
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
        public List<RecipeDefinitionChild> DefChilds { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || DBatchID > 0;

        #endregion Additional Property

        public DyeingBatchMaster()
        {
            DateAdded = DateTime.Now;
            Childs = new List<DyeingBatchRework>();
            DyeingBatchItems = new List<DyeingBatchItem>();
            DyeingBatchWithBatchMasters = new List<DyeingBatchWithBatchMaster>();
            DyeingBatchRecipes = new List<DyeingBatchRecipe>();
            RollFinishingInfoChilds = new List<RollFinishingInfo>();
            DyeingBatchChildFinishingProcesses = new List<DyeingBatchChildFinishingProcess>();
            DyeingBatchMergeBatchs = new List<DyeingBatchMergeBatch>();
            DyeingBatchItemRolls = new List<DyeingBatchItemRoll>();
            DyeingBatches = new List<DyeingBatchMaster>();
            DBatchNo = AppConstants.NEW;
            DBatchDate = DateTime.Now;
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