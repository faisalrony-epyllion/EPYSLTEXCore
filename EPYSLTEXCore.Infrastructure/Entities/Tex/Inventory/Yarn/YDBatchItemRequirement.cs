using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_BATCH_ITEM_REQUIREMENT)]
    public class YDBatchItemRequirement : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDBItemReqID { get; set; }
        public int YDBatchID { get; set; }
        public int YDRecipeItemInfoID { get; set; }
        public int ItemMasterID { get; set; }
        public int ConceptID { get; set; }
        public int YDBookingChildID { get; set; } = 0;
        public int Pcs { get; set; }
        public decimal Qty { get; set; }
        public bool IsFloorRequistion { get; set; }
        public int UnitID { get; set; }
        public string ProgramName { get; set; }
        public int ColorId { get; set; }
        public string ColorCode { get; set; }
        public string ShadeCode { get; set; }

        #region Additional Columns
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YDBItemReqID > 0;

        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string Composition { get; set; }
        [Write(false)]
        public string YarnType { get; set; }
        [Write(false)]
        public string YarnCategory { get; set; }
        [Write(false)]
        public string Process { get; set; }
        [Write(false)]
        public string SubProcess { get; set; }
        [Write(false)]
        public string QualityParameter { get; set; }
        [Write(false)]
        public string Count { get; set; }
        [Write(false)]
        public string NoofPly { get; set; }
        [Write(false)]
        public string KnittingType { get; set; }
        [Write(false)]
        public string FabricConstruction { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public string FabricComposition { get; set; }
        [Write(false)]
        public string FabricGsm { get; set; }
        [Write(false)]
        public int SubGroupID { get; set; }
        [Write(false)]
        public string SubGroup { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public int NoOfThread { get; set; }
        [Write(false)]
        public decimal FRecipeQty { get; set; }
        [Write(false)]
        public decimal SavedQty { get; set; }
        [Write(false)]
        public decimal YDRecipeQty { get; set; }

        [Write(false)]
        public List<YDBatchChild> YDBatchChilds { get; set; } = new List<YDBatchChild>();

        [Write(false)]
        public int ConsumptionUnitID { get; set; }

        [Write(false)]
        public decimal ConsumptionQty { get; set; }

        [Write(false)]
        public int RequiredUnitID { get; set; }

        [Write(false)]
        public decimal RequiredQty { get; set; }

        [Write(false)]
        public string ConsumptionUnit { get; set; }


        [Write(false)]
        public string Length { get; set; }

        [Write(false)]
        public string FUPartName { get; set; }

        [Write(false)]
        public string Width { get; set; }

        [Write(false)]
        public decimal ConceptOrSampleQty { get; set; }

        [Write(false)]
        public decimal ProdQty { get; set; }

        [Write(false)]
        public int ProdQtyPcs { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public string RecipeNo { get; set; }
        [Write(false)]
        public string BatchNo { get; set; }
        [Write(false)]
        public DateTime? RecipeDate { get; set; }
        [Write(false)]
        public decimal ConceptOrSampleQtyKg { get; set; }
        [Write(false)]
        public int ConceptOrSampleQtyPcs { get; set; }
        [Write(false)]
        public decimal PlannedBatchQtyKg { get; set; }
        [Write(false)]
        public int PlannedBatchQtyPcs { get; set; }
        #endregion Additional Columns
    }
}
