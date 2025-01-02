using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.BATCH_MASTER)]
    public class BatchMaster : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int BatchID { get; set; }

        public string BatchNo { get; set; }

        public DateTime BatchDate { get; set; }

        public int RecipeID { get; set; }
        public string GroupConceptNo { get; set; }
        //public int ConceptID { get; set; }

        public int BookingID { get; set; }

        public int ExportOrderID { get; set; }

        public int BuyerID { get; set; }

        public int BuyerTeamID { get; set; }

        public int CCColorID { get; set; }

        public int ColorID { get; set; }

        public decimal BatchWeightKG { get; set; }

        public int BatchWeightPcs { get; set; }

        public int ShiftID { get; set; }

        public int OperatorID { get; set; }

        public DateTime? BatchStartTime { get; set; }

        public DateTime? BatchEndTime { get; set; }

        public DateTime? ProductionDate { get; set; }

        public bool IsApproved { get; set; }

        public int ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public string Remarks { get; set; }

        public int AddedBy { get; set; }

        public int DMID { get; set; }

        public int DyeingNozzleQty { get; set; }

        public int DyeingMcCapacity { get; set; }

        public decimal MachineLoading { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        #endregion Table Properties

        #region Additional Properties

        /// <summary>
        /// This field should be removed and used from BatchItemRequirements
        /// </summary>

        [Write(false)]
        public List<BatchItemRequirement> BatchItemRequirements { get; set; }

        [Write(false)]
        public List<BatchItemRequirement> BatchOtherItemRequirements { get; set; }

        [Write(false)]
        public List<BatchWiseRecipeChild> BatchWiseRecipeChilds { get; set; }

        [Write(false)]
        public string RecipeForName { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string RecipeNo { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }

        [Write(false)]
        public string RecipeFor { get; set; }

        [Write(false)]
        public DateTime? RecipeDate { get; set; }

        [Write(false)]
        public string ShiftName { get; set; }

        [Write(false)]
        public string OperatorName { get; set; }

        [Write(false)]
        public string DyeingMcStatus { get; set; }

        [Write(false)]
        public string Company { get; set; }

        [Write(false)]
        public string DyeingMcName { get; set; }

        [Write(false)]
        public string DMNo { get; set; }

        [Write(false)]
        public string DyeingMcBrand { get; set; }

        [Write(false)]
        public int IsBDS { get; set; }
        [Write(false)]
        public string SLNo { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public string BuyerTeamName { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> UnitList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ShiftList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> OperatorList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> MachineList { get; set; }

        [Write(false)]
        public List<KnittingProduction> KnittingProductions { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BatchID > 0;

        [Write(false)]
        public int ItemMasterID { get; set; }
        [Write(false)]
        public int TotalQty { get; set; }
        [Write(false)]
        public List<RecipeDefinitionChild> DefChilds { get; set; }

        #endregion Additional Properties

        public BatchMaster()
        {
            BatchNo = AppConstants.NEW;
            GroupConceptNo = "";
            ExportOrderID = 0;
            BuyerID = 0;
            BuyerTeamID = 0;
            TotalQty = 0;
            BatchWeightKG = 0m;
            DateAdded = DateTime.Now;
            BatchDate = DateTime.Now;
            ProductionDate = DateTime.Now;
            BuyerName = "";
            BuyerTeamName = "";
            EntityState = System.Data.Entity.EntityState.Added;
            BatchItemRequirements = new List<BatchItemRequirement>();
            BatchOtherItemRequirements = new List<BatchItemRequirement>();
            BatchWiseRecipeChilds = new List<BatchWiseRecipeChild>();
            SLNo = "";
        }
    }

    //#region Validator

    //public class BatchMasterValidator : AbstractValidator<BatchMaster>
    //{
    //    public BatchMasterValidator()
    //    {
    //        RuleFor(x => x.BatchDate).NotEmpty();
    //        RuleFor(x => x.DMID).NotEmpty().WithMessage("Please select machine!");
    //    }
    //}

    //#endregion Validator
}