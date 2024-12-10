using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YARN_PR_MASTER)]
    public class YarnPRMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YarnPRMasterID { get; set; } = 0;
        public int YarnPRFromID { get; set; } = 0;
        public DateTime YarnPRDate { get; set; }
        public string YarnPRNo { get; set; } = AppConstants.NEW;
        public DateTime YarnPRRequiredDate { get; set; }
        public int YarnPRBy { get; set; } = 0;
        public int TriggerPointID { get; set; } = 0;
        public int SubGroupID { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public bool SendForApproval { get; set; } = false;
        public bool SendForCPRApproval { get; set; } = false;
        public bool? Approve { get; set; } = false;
        public int? ApproveBy { get; set; } = 0;
        public DateTime? ApproveDate { get; set; }
        public bool? IsCPR { get; set; } = false;
        public int? CPRBy { get; set; } = 0;
        public DateTime? CPRDate { get; set; }
        public bool? IsFPR { get; set; } = false;
        public int? FPRBy { get; set; } = 0;
        public DateTime? FPRDate { get; set; }
        public bool? Reject { get; set; } = false;
        public int? RejectBy { get; set; } = 0;
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; } = "";
        public bool IsRNDPR { get; set; } = false;
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }
        public string ConceptNo { get; set; } = "";
        public string BookingNo { get; set; } = "";
        public string YDMaterialRequirementNo { get; set; } = "";
        public int CompanyID { get; set; } = 0;
        public int PreProcessRevNo { get; set; } = 0;
        public int RevisionNo { get; set; } = 0;
        public DateTime? RevisionDate { get; set; }
        public int? RevisionBy { get; set; } = 0;
        public string RevisionReason { get; set; } = "";
        public bool NeedRevision { get; set; } = false;
        public int AdditionalNo { get; set; } = 0;
        public int YarnPRFromTableId { get; set; } = 0;
        public int YarnPRFromMasterId { get; set; } = 0;

        #region Additional Property

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YarnPRMasterID > 0;
        [Write(false)]
        public bool IsAdditional { get; set; } = false;
        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public List<YarnPRChild> Childs { get; set; } = new List<YarnPRChild>();
        [Write(false)]
        public List<YarnPRCompany> YarnPRCompanies { get; set; } = new List<YarnPRCompany>();
        [Write(false)]
        public string FiberType { get; set; }
        [Write(false)]
        public string YarnPRByUser { get; set; } = "";
        [Write(false)]
        public string YpApproveBy { get; set; }
        [Write(false)]
        public string YpRejectBy { get; set; } = "";
        [Write(false)]
        public string TriggerPoint { get; set; } = "";
        [Write(false)]
        public string Name { get; set; } = "";
        [Write(false)]
        public int FiberTypeID { get; set; } = 0;
        [Write(false)]
        public string RevisionStatus { get; set; } = "";
        [Write(false)]
        public int ConceptStatus { get; set; } = 0;
        [Write(false)]
        public string PRStatus { get; set; }
        [Write(false)]
        public string YarnPRName { get; set; } = "";
        [Write(false)]
        public Status Status { get; set; } = Statics.Status.Pending;
        [Write(false)]
        public bool IsCheckDVD { get; set; } = true;
        [Write(false)]
        public IEnumerable<Select2OptionModel> DayValidDurations { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public List<YarnPOMaster> YarnPOMasters { get; set; } = new List<YarnPOMaster>();

        #region Material Requirement

        [Write(false)]
        public int FCMRMasterID { get; set; } = 0;

        [Write(false)]
        public int ConceptID { get; set; } = 0;

        [Write(false)]
        public DateTime? ConceptDate { get; set; }

        [Write(false)]
        public string ConceptForName { get; set; } = "";

        [Write(false)]
        public string TrialNo { get; set; } = "";

        [Write(false)]
        public string ItemSubGroup { get; set; } = "";

        [Write(false)]
        public string KnittingType { get; set; } = "";

        [Write(false)]
        public string TechnicalName { get; set; } = "";

        [Write(false)]
        public string Composition { get; set; } = "";

        [Write(false)]
        public string GSM { get; set; } = "";

        [Write(false)]
        public decimal Qty { get; set; } = 0;

        #endregion Material Requirement

        #region Booking

        [Write(false)]
        public int BookingID { get; set; } = 0;

        [Write(false)]
        public DateTime? BookingDate { get; set; }

        [Write(false)]
        public string Buyer { get; set; } = "";

        [Write(false)]
        public string BuyerTeam { get; set; } = "";

        [Write(false)]
        public string Source { get; set; } = "";

        #endregion Booking

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnPRByList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<YarnProductSetupChildProgramDTO> ChildProgramList { get; set; } = Enumerable.Empty<YarnProductSetupChildProgramDTO>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> TechnicalParameterList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> TriggerPointList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> FiberTypeList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> RefSpinnerList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public string YarnPRByName { get; set; }
        [Write(false)]
        public string CreateBy { get; set; } 
        [Write(false)]
        public int MaxValue { get; set; }
        #endregion Additional Property
    }
}
