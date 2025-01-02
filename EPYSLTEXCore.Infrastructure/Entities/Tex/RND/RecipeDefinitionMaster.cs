using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;


namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_RECIPE_DEFINITION_MASTER)]
    public class RecipeDefinitionMaster : DapperBaseEntity
    {
        public RecipeDefinitionMaster()
        {
            RecipeNo = AppConstants.NEW;
            RecipeDate = DateTime.Now;
            Childs = new List<RecipeDefinitionChild>();
            AllChilds = new List<RecipeDefinitionChild>();
            RecipeDefinitionItemInfos = new List<RecipeDefinitionItemInfo>();
            RecipeDefinitionDyeingInfos = new List<RecipeDefinitionDyeingInfo>();
            Temperature = 0;
            ProcessTime = 0;
            IsActive = true;
            Status = "";
            RecipeStatus = "";
            RecipeFor = 0;
        }

        [ExplicitKey]
        public int RecipeID { get; set; }
        public string RecipeNo { get; set; }
        public DateTime RecipeDate { get; set; }
        public int RecipeReqMasterID { get; set; }
        public int ConceptID { get; set; }
        public int BookingID { get; set; }
        public int ExportOrderID { get; set; }
        public int BuyerID { get; set; }
        public int BuyerTeamID { get; set; }
        public int ItemMasterID { get; set; }
        public int CCColorID { get; set; }
        public int ColorID { get; set; }
        public int RecipeFor { get; set; }
        public decimal BatchWeightKG { get; set; }
        public bool IsApproved { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool RecipeSend { get; set; }
        public int SendBy { get; set; }
        public DateTime? SendDate { get; set; }
        public bool Acknowledged { get; set; }
        public int AcknowledgedBy { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
        public string Remarks { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public decimal Temperature { get; set; }
        public decimal ProcessTime { get; set; }
        public int DPID { get; set; }
        public string DPProcessInfo { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchive { get; set; }

        #region Additional Columns
        [Write(false)]
        public List<RecipeDefinitionChild> Childs { get; set; }
        [Write(false)]
        public List<RecipeDefinitionChild> AllChilds { get; set; }
        [Write(false)]
        public List<RecipeDefinitionItemInfo> RecipeDefinitionItemInfos { get; set; }
        [Write(false)]
        public string RecipeReqNo { get; set; }
        [Write(false)]
        public string DPName { get; set; }
        [Write(false)]
        public string RecipeForName { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string ColorCode { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public string RecipeStatus { get; set; }
        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        public DateTime ConceptDate { get; set; }
        [Write(false)]
        public DateTime RequestAckDate { get; set; }
        [Write(false)]
        public string JobCardNo { get; set; }
        [Write(false)]
        public string KnittingType { get; set; }
        [Write(false)]
        public string Composition { get; set; }
        //[Write(false)]
        //public string Gsm { get; set; }
        [Write(false)]
        public string TechnicalName { get; set; }
        [Write(false)]
        public DateTime JobCardDate { get; set; }
        [Write(false)]
        public string Construction { get; set; }
        [Write(false)]
        public int ConstructionID { get; set; }
        [Write(false)]
        public string GSM { get; set; }
        [Write(false)]
        public string ConceptForName { get; set; }
        [Write(false)]
        public int TechnicalNameId { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public int SubGroupID { get; set; }
        [Write(false)]
        public string SubGroup { get; set; }
        [Write(false)]
        public int DBatchID { get; set; }
        [Write(false)]
        public string LabDipNo { get; set; }
        [Write(false)]
        public string Buyer { get; set; }
        [Write(false)]
        public string BuyerTeam { get; set; }
        [Write(false)]
        public int IsBDS { get; set; }
        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> ProcessList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> RecipeForList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> ParticularsList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> UOMList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> RawItemList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TempRawItemList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> DPList { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || RecipeID > 0;
        [Write(false)]
        public List<RecipeDefinitionDyeingInfo> RecipeDefinitionDyeingInfos { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> FiberPartList { get; set; }
        [Write(false)]
        public List<RecipeDefinitionChild> DefChilds { get; set; }
        #endregion Additional Columns
    }

    //public class RecipeDefinitionMasterValidator : AbstractValidator<RecipeDefinitionMaster>
    //{
    //    public RecipeDefinitionMasterValidator()
    //    {
    //        RuleFor(x => x.RecipeDate).NotEmpty();
    //        RuleFor(x => x.RecipeFor).NotEmpty();
    //        //RuleFor(x => x.Childs).NotEmpty().WithMessage("You have must add one process Item.");
    //        When(x => x.Childs.Any(), () =>
    //        {
    //            RuleForEach(x => x.Childs).SetValidator(new RecipeDefinitionChildValidator());
    //        });
    //    }
    //}
}