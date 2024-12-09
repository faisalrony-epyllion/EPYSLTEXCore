using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.PROJECTION_YARN_BOOKING_MASTER)]
    public class ProjectionYarnBookingMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int PYBookingID { get; set; } = 0;
        public string PYBookingNo { get; set; } = AppConstants.NEW;
        public DateTime PYBookingDate { get; set; }
        public int PreProcessRevNo { get; set; } = 0;
        public int RevisionNo { get; set; } = 0;
        public DateTime? RevisionDate { get; set; }
        public int RevisionBy { get; set; } = 0;
        public string RevisionReason { get; set; } = "";
        public DateTime RequiredDate { get; set; }

        public int BookingByID { get; set; } = 0;

        public int RequiredByID { get; set; } = 0;

        public int BuyerID { get; set; } = 0;
        public int CompanyID { get; set; } = 0;
        public int DepartmentID { get; set; } = 0;
        public int BuyerTeamID { get; set; } = 0;

        public bool SendToApprover { get; set; } = false;

        public bool IsApprove { get; set; } = false;

        public int ApproveBy { get; set; } = 0;

        public DateTime? ApproveDate { get; set; }

        public bool IsReject { get; set; } = false;

        public string RejectReason { get; set; } = "";

        public bool IsAcknowledged { get; set; } = false;

        public int AcknowledgedBy { get; set; } = 0;

        public DateTime? AcknowledgedDate { get; set; }

        public bool IsCancel { get; set; } = false;

        public string CancelReason { get; set; } = "";

        public bool IsCancelAccept { get; set; } = false;

        public bool IsCancelReject { get; set; } = false;

        public string CancelRejectReason { get; set; } = "";
        public bool isMarketingFlag { get; set; } = false;
        public string Remarks { get; set; } = "";
        public int AddedBy { get; set; } = 0;

        public int UpdatedBy { get; set; } = 0;

        public System.DateTime DateAdded { get; set; }

        public DateTime? DateUpdated { get; set; }
        public DateTime? FabricBookingStartMonth { get; set; }
        public DateTime? FabricBookingEndMonth { get; set; }

        public int SeasonID { get; set; } = 0;
        public int FinancialYearID { get; set; } = 0;

        public bool IsUnacknowledge { get; set; } = false;

        public int UnacknowledgeBy { get; set; } = 0;

        public DateTime? UnacknowledgeDate { get; set; }

        public string UnacknowledgeReason { get; set; } = "";

        public int PBookingType { get; set; } = 0;
        public int BaseTypeId { get; set; } = 0;

        #region Additional Columns

        [Write(false)]
        public List<ProjectionYarnBookingItemChild> ProjectionYarnBookingItemChilds { get; set; } = new List<ProjectionYarnBookingItemChild>();

        [Write(false)]
        public List<ProjectionYarnBookingItemChildDetails> PYBItemChildDetails { get; set; } = new List<ProjectionYarnBookingItemChildDetails>();

        [Write(false)]
        public List<PYBookingBuyerAndBuyerTeam> PYBookingBuyerAndBuyerTeams { get; set; } = new List<PYBookingBuyerAndBuyerTeam>();

        [Write(false)]
        public string Buyer { get; set; } = "";
        [Write(false)]
        public string BuyerTeam { get; set; } = "";

        [Write(false)]
        public string BookingByName { get; set; } = "";

        //[Write(false)]
        // public string UserId { get; set; }

        [Write(false)]
        public string RequiredByName { get; set; } = "";

        [Write(false)]
        public string DepertmentDescription { get; set; } = "";

        [Write(false)]
        public IEnumerable<Select2OptionModel> BuyerList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> BuyerTeamList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> BookingByList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> RequiredByList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> DepartmentList { get; set; }
        [Write(false)]
        public List<FabricComponentMappingSetup> FabricComponentMappingSetupList { get; set; } = new List<FabricComponentMappingSetup>();

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.PYBookingID > 0;

        [Write(false)]
        public string Status { get; set; } = "";

        [Write(false)]
        public bool IsSuperUser { get; set; } = false;

        [Write(false)]
        public string BuyerIDsList { get; set; } = "";

        [Write(false)]
        public string BuyerTeamIDsList { get; set; } = "";

        [Write(false)]
        public string BuyerName { get; set; } = "";
        [Write(false)]
        public bool IsCheckDVD { get; set; } = true;

        //[Write(false)]
        //public string BuyerIDsList { get; set; }
        [Write(false)]
        public string BuyerTeamName { get; set; } = "";

        //[Write(false)]
        //public string BuyerTeamIDsList { get; set; } //
        [Write(false)]
        public string RevisionStatus { get; set; } = "";
        //[Write(false)]
        //public bool isMarketingFlag { get; set; }


        [Write(false)]
        public YarnPRMaster YarnPR { get; set; } = new YarnPRMaster();
        [Write(false)]
        public IEnumerable<Select2OptionModel> SeasonList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> FinancialYearList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnShadeBooks { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> DayValidDurations { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnSubProgramNews { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModelExtended> Certifications { get; set; }

        [Write(false)]
        public IEnumerable<string> FabricComponents { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> FabricComponentsNew { get; set; }

        #endregion Additional Columns

        public ProjectionYarnBookingMaster()
        {
            PreProcessRevNo = 0;
            RevisionNo = 0;
            RevisionBy = 0;
            CompanyID = 0;
            SeasonID = 0;
            FinancialYearID = 0;
            isMarketingFlag = false;
            //BookingByName = UserId;
            DateAdded = DateTime.Now;
            PYBookingDate = DateTime.Now;
            //RevisionDate = DateTime.Now;
            RequiredDate = DateTime.Now;
            FabricBookingStartMonth = DateTime.Now;
            FabricBookingEndMonth = DateTime.Now;
            PYBookingNo = AppConstants.NEW;
            BuyerIDsList = "";
            BuyerTeamIDsList = "";
            PBookingType = 0;
            BaseTypeId = 0;
            ProjectionYarnBookingItemChilds = new List<ProjectionYarnBookingItemChild>();
            PYBItemChildDetails = new List<ProjectionYarnBookingItemChildDetails>();
            PYBookingBuyerAndBuyerTeams = new List<PYBookingBuyerAndBuyerTeam>();
            YarnPR = new YarnPRMaster();
        }
    }

    #region Validators
    /*
    public class ProjectionYarnBookingMasterValidator : AbstractValidator<ProjectionYarnBookingMaster>
    {
        public ProjectionYarnBookingMasterValidator()
        {
            RuleFor(x => x.PYBookingDate).NotEmpty().WithMessage("Booking date is required.");
            // RuleForEach(x => x.ProjectionYarnBookingItemChilds).SetValidator(new ProjectionYarnBookingItemChildValidator());
        }
    }
    */
    #endregion Validators
}
