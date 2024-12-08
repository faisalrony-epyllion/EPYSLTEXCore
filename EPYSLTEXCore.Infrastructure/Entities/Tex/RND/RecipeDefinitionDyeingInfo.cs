using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_RECIPE_DEFINITION_DYEING_INFO)]
    public class RecipeDefinitionDyeingInfo : DapperBaseEntity
    {
        public RecipeDefinitionDyeingInfo()
        {
            Temperature = 0;
            ProcessTime = 0;
            RecipeOn = false;
        }

        [ExplicitKey]
        public int RecipeDInfoID { get; set; }

        public int RecipeReqMasterID { get; set; } = 0;

        public int CCColorID { get; set; } = 0;

        public int RecipeID { get; set; } = 0;

        public int FiberPartID { get; set; } = 0;

        public int ColorID { get; set; } = 0;

        public string ColorCode { get; set; } = "";

        public decimal Temperature { get; set; }

        public decimal ProcessTime { get; set; }
        public bool RecipeOn { get; set; }
        #region Additional

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || RecipeDInfoID > 0;

        [Write(false)]
        public string FiberPart { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        #endregion Additional
    }
}
