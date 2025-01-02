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
    [Table(TableNames.YD_RECIPE_DEFINITION_DYEING_INFO)]
    public class YDRecipeDefinitionDyeingInfo : DapperBaseEntity
    {
        public YDRecipeDefinitionDyeingInfo()
        {
            Temperature = 0;
            ProcessTime = 0;
            RecipeOn = false;
        }

        [ExplicitKey]
        public int YDRecipeDInfoID { get; set; }

        public int YDRecipeReqMasterID { get; set; }

        public int CCColorID { get; set; }

        public int YDRecipeID { get; set; }

        public int FiberPartID { get; set; }

        public int ColorID { get; set; }

        public string ColorCode { get; set; }

        public decimal Temperature { get; set; }

        public decimal ProcessTime { get; set; }
        public bool RecipeOn { get; set; }
        #region Additional

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDRecipeDInfoID > 0;

        [Write(false)]
        public string FiberPart { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        #endregion Additional
    }
}
