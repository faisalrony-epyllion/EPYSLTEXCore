using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;


namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_RECIPE_REQ_MASTER)]
    public class RecipeRequestMaster : DapperBaseEntity
    {
        public RecipeRequestMaster()
        {
            BuyerID = 0;
            RecipeReqNo = AppConstants.NEW;
            Approved = false;
            DyeingBatchList = new List<DyeingBatchMaster>();
            RecipeRequestChilds = new List<RecipeRequestChild>();
            RecipeDefinitions = new List<RecipeDefinitionMaster>();
            DyeingBatchReworkRecipe = new DyeingBatchReworkRecipe();
            RecipeDefinitionItemInfos = new List<RecipeDefinitionItemInfo>();
            IsRework = false;
        }

        [ExplicitKey]
        public int RecipeReqMasterID { get; set; }

        public string RecipeReqNo { get; set; }

        public DateTime RecipeReqDate { get; set; }

        public string GroupConceptNo { get; set; }

        public int PreProcessRevNo { get; set; }

        public int RevisionNo { get; set; }

        public int DBatchID { get; set; }

        public DateTime? RevisionDate { get; set; }

        public int RevisionBy { get; set; }

        public string RevisionReason { get; set; }

        public int CCColorID { get; set; }

        public int ColorID { get; set; }

        public int DPID { get; set; }

        public string DPProcessInfo { get; set; }

        public string Remarks { get; set; }

        public int IsBDS { get; set; }

        public bool IsBulkBooking { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        public bool Acknowledge { get; set; }

        public int AcknowledgeBy { get; set; }

        public DateTime? AcknowledgeDate { get; set; }

        public bool UnAcknowledge { get; set; }

        public int UnAcknowledgeBy { get; set; }

        public DateTime? UnAcknowledgeDate { get; set; }

        public bool Approved { get; set; }

        public string RecipeFor { get; set; }

        public string UnAcknowledgeReason { get; set; }


        #region Additional
        [Write(false)]
        public List<RecipeRequestChild> RecipeRequestChilds { get; set; }
        [Write(false)]
        public List<RecipeDefinitionDyeingInfo> RecipeDefinitionDyeingInfos { get; set; }
        [Write(false)]
        public List<RecipeDefinitionMaster> RecipeDefinitions { get; set; }
        [Write(false)]
        public List<FreeConceptMaster> Concepts { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> DPList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> FiberPartList { get; set; }
        [Write(false)]
        public IList<Select2OptionModel> FiberPartListCC { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || RecipeReqMasterID > 0;

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string ColorCode { get; set; }

        [Write(false)]
        public int ConceptID { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public DateTime ConceptDate { get; set; }

        [Write(false)]
        public int ConceptFor { get; set; }

        [Write(false)]
        public int KnittingTypeID { get; set; }

        [Write(false)]
        public int ConstructionID { get; set; }

        [Write(false)]
        public string Construction { get; set; }

        [Write(false)]
        public int CompositionID { get; set; }

        [Write(false)]
        public int GSMID { get; set; }

        [Write(false)]
        public decimal Qty { get; set; }

        [Write(false)]
        public int ConceptStatusID { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string Gsm { get; set; }

        [Write(false)]
        public string UserName { get; set; }

        [Write(false)]
        public string DPName { get; set; }

        [Write(false)]
        public int RecipeID { get; set; }

        [Write(false)]
        public string LabDipNo { get; set; }

        [Write(false)]
        public string Buyer { get; set; }

        [Write(false)]
        public string BuyerTeam { get; set; } //

        [Write(false)]
        public List<DyeingBatchMaster> DyeingBatchList { get; set; }
        [Write(false)]
        public DyeingBatchReworkRecipe DyeingBatchReworkRecipe { get; set; }

        [Write(false)]
        public string RecipeReqTime { get; set; }
        [Write(false)]
        public string RecipeStatus { get; set; }
        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        public string DBatchNo { get; set; }
        [Write(false)]
        public string ExistingDBatchNo { get; set; }
        [Write(false)]
        public bool IsRework { get; set; }
        [Write(false)]
        public int BuyerID { get; set; }
        [Write(false)]
        public List<RecipeDefinitionItemInfo> RecipeDefinitionItemInfos { get; set; }
        #endregion Additional
    }
}