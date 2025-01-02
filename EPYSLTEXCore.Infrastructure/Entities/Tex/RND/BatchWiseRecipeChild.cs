using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.BATCH_WISE_RECIPE_CHILD)]
    public class BatchWiseRecipeChild : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int BRecipeChildID { get; set; }

        public int BatchID { get; set; }

        public int RecipeChildID { get; set; }

        public int ProcessID { get; set; }

        public int ParticularsID { get; set; }

        public int RawItemID { get; set; }

        public decimal Qty { get; set; }

        public int UnitID { get; set; }

        public decimal TempIn { get; set; }

        public decimal TempOut { get; set; }

        public decimal ProcessTime { get; set; }

        #endregion Table Properties

        #region Additional Properties

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
        public List<RecipeDefinitionChild> DefChilds { get; set; } = new List<RecipeDefinitionChild>();

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BRecipeChildID > 0;

        #endregion Additional Properties
    }
}