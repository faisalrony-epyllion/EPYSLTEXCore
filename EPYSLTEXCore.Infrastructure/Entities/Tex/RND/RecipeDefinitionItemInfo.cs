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
    [Table(TableNames.RND_RECIPE_DEFINITION_ITEM_INFO)]
    public class RecipeDefinitionItemInfo : DapperBaseEntity
    {
        [ExplicitKey]
        public int RecipeItemInfoID { get; set; }

        public int RecipeID { get; set; }

        public int RecipeReqChildID { get; set; }

        public int BookingID { get; set; }

        public int ItemMasterID { get; set; }

        public int ConceptID { get; set; }

        public int SubGroupID { get; set; }

        public int? Pcs { get; set; }

        public decimal Qty { get; set; }

        #region Additional Property

        [Write(false)]
        public int ConstructionID { get; set; }

        [Write(false)]
        public int CompositionID { get; set; }

        [Write(false)]
        public int FabricColorID { get; set; }

        [Write(false)]
        public string FabricGsm { get; set; }

        [Write(false)]
        public string FabricWidth { get; set; }

        [Write(false)]
        public int KnittingTypeID { get; set; }

        [Write(false)]
        public int DyeingTypeID { get; set; }

        [Write(false)]
        public string FabricConstruction { get; set; }

        [Write(false)]
        public string FabricComposition { get; set; }

        [Write(false)]
        public string FabricColor { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string DyeingType { get; set; }

        [Write(false)]
        public string SubGroup { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public int TechnicalNameId { get; set; }

        [Write(false)]
        public bool RecipeOn { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || RecipeItemInfoID > 0;

        #endregion Additional Property

    }
}