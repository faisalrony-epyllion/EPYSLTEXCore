using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_DYEING_BATCH_RECIPE)]
    public class YDDyeingBatchRecipe : DapperBaseEntity
    {
        public YDDyeingBatchRecipe()
        {
            Unit = "";
            DefChilds = new List<YDRecipeDefinitionChild>();
        }
        [ExplicitKey]
        public int YDDBRID { get; set; }

        public int YDDBatchID { get; set; }

        public int YDRecipeID { get; set; }

        public int YDRecipeChildID { get; set; }

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
        public int YDRecipeDInfoID { get; set; }
        [Write(false)]
        public int YDBatchID { get; set; } = 0;
        [Write(false)]
        public List<YDRecipeDefinitionChild> DefChilds { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDDBRID > 0;

        #endregion Additional
    }
}
