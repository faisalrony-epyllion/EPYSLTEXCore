using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table("T_RecipeDefinitionDyeingInfo")]
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

        public int RecipeReqMasterID { get; set; }

        public int CCColorID { get; set; }

        public int RecipeID { get; set; }

        public int FiberPartID { get; set; }

        public int ColorID { get; set; }

        public string ColorCode { get; set; }

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
