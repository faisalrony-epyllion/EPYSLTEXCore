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
        public YarnPRMaster()
        {
            Remarks = "";
            SendForApproval = false;
            SendForCPRApproval = false;
            Approve = false;
            ApproveBy = 0;
            Reject = false;
            RejectBy = 0;
            RejectReason = "";
            TriggerPointID = 0;
            UpdatedBy = 0;
            DateAdded = DateTime.Now;
            YarnPRDate = DateTime.Now;
            YarnPRRequiredDate = DateTime.Now;
            YarnPRNo = AppConstants.NEW;
            YarnPRFromID = 0;
            AdditionalNo = 0;
            Status = Status.Pending;
            Childs = new List<YarnPRChild>();
            IsAdditional = false;
            YarnPRName = "";
            YarnPRFromMasterId = 0;
            YarnPRFromTableId = 0;
            Childs = new List<YarnPRChild>();
        }

        [ExplicitKey]
        public int YarnPRMasterID { get; set; }
        public int YarnPRFromID { get; set; }
        public DateTime YarnPRDate { get; set; }
        public string YarnPRNo { get; set; }
        public DateTime YarnPRRequiredDate { get; set; }
        public int YarnPRBy { get; set; }
        public int TriggerPointID { get; set; }
        public int SubGroupID { get; set; }
        public string Remarks { get; set; }
        public bool SendForApproval { get; set; }
        public bool SendForCPRApproval { get; set; }
        public bool? Approve { get; set; }
        public int? ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public bool? IsCPR { get; set; }
        public int? CPRBy { get; set; }
        public DateTime? CPRDate { get; set; }
        public bool? IsFPR { get; set; }
        public int? FPRBy { get; set; }
        public DateTime? FPRDate { get; set; }
        public bool? Reject { get; set; }
        public int? RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; }
        public bool IsRNDPR { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string ConceptNo { get; set; }
        public string BookingNo { get; set; }
        public string YDMaterialRequirementNo { get; set; }
        public int CompanyID { get; set; }
        public int PreProcessRevNo { get; set; }
        public int RevisionNo { get; set; }
        public DateTime? RevisionDate { get; set; }
        public int? RevisionBy { get; set; }
        public string RevisionReason { get; set; }
        public bool NeedRevision { get; set; }
        public int AdditionalNo { get; set; }
        public int YarnPRFromTableId { get; set; }
        public int YarnPRFromMasterId { get; set; }

        #region Additional Property

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YarnPRMasterID > 0;
        [Write(false)]
        public bool IsAdditional { get; set; }
        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public List<YarnPRChild> Childs { get; set; }
        [Write(false)]
        public List<YarnPRCompany> YarnPRCompanies { get; set; }
        [Write(false)]
        public string FiberType { get; set; }
        [Write(false)]
        public string YarnPRByUser { get; set; }
        [Write(false)]
        public string YpApproveBy { get; set; }
        [Write(false)]
        public string YpRejectBy { get; set; }
        [Write(false)]
        public string TriggerPoint { get; set; }
        [Write(false)]
        public string Name { get; set; }
        [Write(false)]
        public int FiberTypeID { get; set; }
        [Write(false)]
        public string RevisionStatus { get; set; }
        [Write(false)]
        public int ConceptStatus { get; set; }
        [Write(false)]
        public string PRStatus { get; set; }
        [Write(false)]
        public string YarnPRName { get; set; }
        [Write(false)]
        public Status Status { get; set; }
        [Write(false)]
        public bool IsCheckDVD { get; set; } = true;
        [Write(false)]
        public IEnumerable<Select2OptionModel> DayValidDurations { get; set; }
        [Write(false)]
        public List<YarnPOMaster> YarnPOMasters { get; set; } = new List<YarnPOMaster>();

        #region Material Requirement

        [Write(false)]
        public int FCMRMasterID { get; set; }

        [Write(false)]
        public int ConceptID { get; set; }

        [Write(false)]
        public DateTime? ConceptDate { get; set; }

        [Write(false)]
        public string ConceptForName { get; set; }

        [Write(false)]
        public string TrialNo { get; set; }

        [Write(false)]
        public string ItemSubGroup { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string GSM { get; set; }

        [Write(false)]
        public decimal Qty { get; set; }

        #endregion Material Requirement

        #region Booking

        [Write(false)]
        public int BookingID { get; set; }

        [Write(false)]
        public DateTime? BookingDate { get; set; }

        [Write(false)]
        public string Buyer { get; set; }

        [Write(false)]
        public string BuyerTeam { get; set; }

        [Write(false)]
        public string Source { get; set; }

        #endregion Booking

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnPRByList { get; set; }
        [Write(false)]
        public IEnumerable<YarnProductSetupChildProgramDTO> ChildProgramList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TechnicalParameterList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> TriggerPointList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> FiberTypeList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> RefSpinnerList { get; set; }
        [Write(false)]
        public string YarnPRByName { get; set; }
        [Write(false)]
        public string CreateBy { get; set; }
        #endregion Additional Property
    }
}
