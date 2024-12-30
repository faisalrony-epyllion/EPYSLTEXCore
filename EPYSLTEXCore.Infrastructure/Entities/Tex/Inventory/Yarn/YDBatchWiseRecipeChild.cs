using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_BATCH_WISE_RECIPE_CHILD)]
    public class YDBatchWiseRecipeChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDBRecipeChildID { get; set; }
        public int YDBatchID { get; set; }
        public int YDRecipeChildID { get; set; }
        public int ProcessID { get; set; }
        public int ParticularsID { get; set; }
        public int RawItemID { get; set; }
        public decimal Qty { get; set; }
        public int UnitID { get; set; }
        public decimal TempIn { get; set; }
        public decimal TempOut { get; set; }
        public decimal ProcessTime { get; set; }

        #region Additional Columns

        [Write(false)]
        public string ProcessName { get; set; }

        [Write(false)]
        public string ParticularsName { get; set; }

        [Write(false)]
        public string RawItemName { get; set; }

        [Write(false)]
        public string Unit { get; set; }

        [Write(false)]
        public string FiberPart { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string Temperature { get; set; }

        [Write(false)]
        public int RecipeDInfoID { get; set; }
        [Write(false)]
        public int RecipeID { get; set; }

        [Write(false)]
        public List<YDRecipeDefinitionChild> YDDefChilds { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDRecipeChildID > 0;

        #endregion Additional Columns
    }
}
