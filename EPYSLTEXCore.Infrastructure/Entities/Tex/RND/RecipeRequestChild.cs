using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;


namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_RECIPE_REQ_CHILD)]
    public class RecipeRequestChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int RecipeReqChildID { get; set; }

        public int RecipeReqMasterID { get; set; }

        public int ConceptID { get; set; }

        public int BookingID { get; set; }
        public bool RecipeOn { get; set; }
        public int SubGroupID { get; set; }

        public int ItemMasterID { get; set; }

        public int CCColorID { get; set; }

        #region Additional

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || RecipeReqChildID > 0;

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public string Construction { get; set; }

        [Write(false)]
        public string SubGroup { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string GSM { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        #endregion Additional
    }
}