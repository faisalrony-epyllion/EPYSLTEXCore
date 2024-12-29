using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{

    [Table(TableNames.DYEING_BATCH_RECIPE)]
    public class DyeingBatchRecipe : DapperBaseEntity
    {
        public DyeingBatchRecipe()
        {
            Unit = "";
            DefChilds = new List<RecipeDefinitionChild>();
        }
        [ExplicitKey]
        public int DBRID { get; set; }

        public int DBatchID { get; set; }

        public int RecipeID { get; set; }

        public int RecipeChildID { get; set; }

        public int ProcessID { get; set; }

        public int ParticularsID { get; set; }

        public int RawItemID { get; set; }

        public decimal Qty { get; set; }

        public int UnitID { get; set; }

        public decimal TempIn { get; set; }

        public decimal TempOut { get; set; }

        public decimal ProcessTime { get; set; }

        #region Additional

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
        public int RecipeDInfoID { get; set; }
        [Write(false)]
        public List<RecipeDefinitionChild> DefChilds { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || DBRID > 0;

        #endregion Additional
    }
}