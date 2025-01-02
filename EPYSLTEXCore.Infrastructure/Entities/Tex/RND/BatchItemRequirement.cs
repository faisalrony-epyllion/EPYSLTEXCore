using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.BATCH_ITEM_REQUIREMENT)]
    public class BatchItemRequirement : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int BItemReqID { get; set; }
        public int BatchID { get; set; }
        public int RecipeItemInfoID { get; set; }
        public int ItemMasterID { get; set; }
        public int ConceptID { get; set; }
        public int Pcs { get; set; }
        public decimal Qty { get; set; }

        public bool IsFloorRequistion { get; set; }

        #endregion Table Properties

        #region Additional Fields

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BItemReqID > 0;

        [Write(false)]
        public List<BatchChild> BatchChilds { get; set; }

        [Write(false)]
        public int ConsumptionUnitID { get; set; }

        [Write(false)]
        public decimal ConsumptionQty { get; set; }

        [Write(false)]
        public int RequiredUnitID { get; set; }

        [Write(false)]
        public decimal RequiredQty { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string ConsumptionUnit { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string FabricComposition { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string FabricGsm { get; set; }

        [Write(false)]
        public int SubGroupID { get; set; }

        [Write(false)]
        public string SubGroup { get; set; }

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
        #endregion Additional Fields

        public BatchItemRequirement()
        {
            BatchChilds = new List<BatchChild>();
            EntityState = System.Data.Entity.EntityState.Added;
            ConceptID = 0;

            ConceptOrSampleQtyKg = 0;
            ConceptOrSampleQtyPcs = 0;
            Qty = 0; //BatchQtyKg
            Pcs = 0; //BatchQtyPcs
            PlannedBatchQtyKg = 0;
            PlannedBatchQtyPcs = 0;
            ConceptNo = "";
            RecipeNo = "";
            ColorName = "";
        }
    }

    #region Validators

    public class BatchItemRequirementValidator : AbstractValidator<BatchItemRequirement>
    {
        public BatchItemRequirementValidator()
        {
            //RuleFor(x => x.RequiredUnitID).NotEmpty();
            //RuleFor(x => x.RequiredQty).NotEmpty();
        }
    }

    #endregion Validators
}