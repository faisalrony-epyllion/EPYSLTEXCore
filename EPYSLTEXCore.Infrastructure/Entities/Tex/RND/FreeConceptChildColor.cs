using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_FREE_CONCEPT_CHILD_COLOR)]
    public class FreeConceptChildColor : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int CCColorID { get; set; }

        public int ConceptID { get; set; }

        public int ColorId { get; set; }

        public string ColorCode { get; set; }

        public string ColorName { get; set; }

        public bool RequestRecipe { get; set; }

        public int RequestBy { get; set; }

        public DateTime? RequestDate { get; set; }

        public bool RequestAck { get; set; }

        public int RequestAckBy { get; set; }

        public int DPID { get; set; }

        public string DPProcessInfo { get; set; }

        public DateTime? RequestAckDate { get; set; }

        public bool GrayFabricOK { get; set; }

        public string Remarks { get; set; }

        public bool IsFirm { get; set; }

        public bool IsLive { get; set; }

        #endregion Table Properties

        #region Additional Properties

        [Write(false)]
        public string ConceptNo { get; set; } = "";

        [Write(false)]
        public DateTime ConceptDate { get; set; }

        [Write(false)]
        public int TrialNo { get; set; } = 0;

        [Write(false)]
        public DateTime? TrialDate { get; set; }

        [Write(false)]
        public int ConceptFor { get; set; } = 0;

        [Write(false)]
        public int KnittingTypeId { get; set; } = 0;

        [Write(false)]
        public int ConstructionId { get; set; } = 0;

        [Write(false)]
        public int CompositionId { get; set; } = 0;

        [Write(false)]
        public int GSMId { get; set; } = 0;

        [Write(false)]
        public decimal Qty { get; set; } = 0;

        [Write(false)]
        public int ConceptStatusId { get; set; } = 0;

        [Write(false)]
        public string? RGBOrHex { get; set; }

        [Write(false)]
        public string KnittingType { get; set; } = "";

        [Write(false)]
        public string Composition { get; set; } = "";

        [Write(false)]
        public string Construction { get; set; } = "";

        [Write(false)]
        public string TechnicalName { get; set; } = "";

        [Write(false)]
        public string Gsm { get; set; } = "";

        [Write(false)]
        public string DPName { get; set; } = "";

        [Write(false)]
        public string ColorSource { get; set; } = "";
        [Write(false)]
        public bool IsRecipeDone { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || CCColorID > 0;

        [Write(false)]
        public List<RecipeDefinitionDyeingInfo> RecipeDefinitionDyeingInfos { get; set; } = new List<RecipeDefinitionDyeingInfo>();

        [Write(false)]
        public IEnumerable<Select2OptionModel> FiberPartList { get; set; } = [];

        [Write(false)]
        public string UserName { get; set; } = "";

        #endregion Additional Properties

        public FreeConceptChildColor()
        {
            DPID = 0;
            RecipeDefinitionDyeingInfos = new List<RecipeDefinitionDyeingInfo>();
            IsRecipeDone = false;
            ConceptID = 0;
            ColorId = 0;
            ColorCode = "";
            ColorName = "";
            RequestRecipe = false;
            RequestBy = 0;
            RequestDate = DateTime.Now;
            RequestAck= false;
            RequestAckBy = 0;
            DPProcessInfo ="";
            RequestAckDate = DateTime.Now;
            GrayFabricOK= false;
            Remarks = "";
            IsFirm= false;
            IsLive= false;

        }
    }

    #region Validator

    //public class FreeConceptChildColorValidator : AbstractValidator<FreeConceptChildColor>
    //{
    //    public FreeConceptChildColorValidator()
    //    {
    //        RuleFor(x => x.ColorId).NotEmpty();
    //    }
    //}

    #endregion Validator
}
