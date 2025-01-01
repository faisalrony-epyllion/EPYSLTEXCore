using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_RECIPE_REQ_MASTER)]
    public class YDRecipeRequestMaster : DapperBaseEntity
    {
        public YDRecipeRequestMaster()
        {
            BuyerID = 0;
            RecipeReqNo = AppConstants.NEW;
            Approved = false;
            YDBookingNo = "";
            YDBookingDate = DateTime.Now;
            CCColorID = 0;
            ColorID = 0;
            ColorName = "";
            GroupConceptNo = "";
            Buyer = "";
            BuyerTeam = "";
            ColorCode = "";
            IsBDS = 0;
            UpdatedBy = 0;
            YDBookingChildID = 0;
            YDDyeingBatchList = new List<YDDyeingBatchMaster>();
            YDRecipeRequestChilds = new List<YDRecipeRequestChild>();
            YDRecipeDefinitions = new List<YDRecipeDefinitionMaster>();
            YDDyeingBatchReworkRecipe = new YDDyeingBatchReworkRecipe();
            YDRecipeDefinitionItemInfos = new List<YDRecipeDefinitionItemInfo>();
            IsRework = false;
        }

        [ExplicitKey]
        public int YDRecipeReqMasterID { get; set; }

        public string RecipeReqNo { get; set; }

        public DateTime RecipeReqDate { get; set; }

        public string GroupConceptNo { get; set; }

        public int PreProcessRevNo { get; set; }

        public int RevisionNo { get; set; }

        public int YDDBatchID { get; set; }

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
        public int YDBookingChildID { get; set; }


        #region Additional
        [Write(false)]
        public List<YDRecipeRequestChild> YDRecipeRequestChilds { get; set; }
        [Write(false)]
        public List<YDRecipeDefinitionDyeingInfo> YDRecipeDefinitionDyeingInfos { get; set; }
        [Write(false)]
        public List<YDRecipeDefinitionMaster> YDRecipeDefinitions { get; set; }
        [Write(false)]
        public List<FreeConceptMaster> Concepts { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> DPList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> FiberPartList { get; set; }
        [Write(false)]
        public IList<Select2OptionModel> FiberPartListCC { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDRecipeReqMasterID > 0;

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
        public int YDRecipeID { get; set; }

        [Write(false)]
        public string LabDipNo { get; set; }

        [Write(false)]
        public string Buyer { get; set; }

        [Write(false)]
        public string BuyerTeam { get; set; } //

        [Write(false)]
        public List<YDDyeingBatchMaster> YDDyeingBatchList { get; set; }
        [Write(false)]
        public YDDyeingBatchReworkRecipe YDDyeingBatchReworkRecipe { get; set; }

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
        public string YDBookingNo { get; set; }
        [Write(false)]
        public DateTime YDBookingDate { get; set; }
        [Write(false)]
        public List<YDRecipeDefinitionItemInfo> YDRecipeDefinitionItemInfos { get; set; }
        #endregion Additional
    }
}
