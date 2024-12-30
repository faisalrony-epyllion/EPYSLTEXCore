using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.KY_Req_Master)]
    public class KYReqMaster: DapperBaseEntity
    {
        [ExplicitKey]
        public int KYReqMasterID { get; set; }
        ///<summary>
        /// KYReqNo (length: 20)
        ///</summary>
        public string KYReqNo { get; set; }

        ///<summary>
        /// KYReqDate
        ///</summary>
        public System.DateTime KYReqDate { get; set; }

        ///<summary>
        /// KYReqBy
        ///</summary>
        public int KYReqBy { get; set; }


        ///<summary>
        /// SupplierID
        ///</summary>
        public int SupplierID { get; set; }

        ///<summary>
        /// SpinnerID
        ///</summary>
        public int SpinnerID { get; set; }

        ///<summary>
        /// Remarks (length: 500)
        ///</summary>
        public string Remarks { get; set; }

        ///<summary>
        /// Approve
        ///</summary>
        public bool Approve { get; set; }

        ///<summary>
        /// ApproveBy
        ///</summary>
        public int ApproveBy { get; set; }

        ///<summary>
        /// ApproveDate
        ///</summary>
        public System.DateTime? ApproveDate { get; set; }

        ///<summary>
        /// Reject
        ///</summary>
        public bool Reject { get; set; }

        ///<summary>
        /// RejectBy
        ///</summary>
        public int RejectBy { get; set; }

        ///<summary>
        /// RejectDate
        ///</summary>
        public System.DateTime? RejectDate { get; set; }

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; }

        ///<summary>
        /// DateAdded
        ///</summary>
        public System.DateTime DateAdded { get; set; }

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int UpdatedBy { get; set; }

        ///<summary>
        /// DateUpdated
        ///</summary>
        public System.DateTime? DateUpdated { get; set; }

        ///<summary>
        /// RnD for buyer
        ///</summary>
        public int BuyerID { get; set; }
        ///<summary>
        /// IsAcknowledge
        ///</summary>
        public bool Acknowledge { get; set; }

        ///<summary>
        /// AcknowledgeDate
        ///</summary>
        public DateTime? AcknowledgeDate { get; set; }

        ///<summary>
        /// AcknowledgeBy
        ///</summary>
        public int AcknowledgeBy { get; set; }
        public int RCompanyID { get; set; }
        public int OCompanyID { get; set; }
        public int LocationID { get; set; }
        public bool IsReqForYD { get; set; }
        public string ConceptNo { get; set; }
        public int FloorID { get; set; }
        public bool UnAcknowledge { get; set; }
        public DateTime? UnAcknowledgeDate { get; set; }
        public int UnAcknowledgeBy { get; set; }
        public string UnAcknowledgeReason { get; set; }
        public int PreProcessRevNo { get; set; }
        public int RevisionNo { get; set; }
        public DateTime? RevisionDate { get; set; }
        public int RevisionBy { get; set; }
        public string RevisionReason { get; set; }
        public bool IsAdditional { get; set; }
        public bool IsWOKnittingInfo { get; set; }
        #region Additional
        [Write(false)]
        public string KYReqByUser { get; set; }
        [Write(false)]
        public string BAnalysisNo { get; set; }
        [Write(false)]
        public DateTime BAnalysisDate { get; set; }
        [Write(false)]
        public DateTime YInHouseDate { get; set; }
        [Write(false)]
        public DateTime YRequiredDate { get; set; }
        [Write(false)]
        public string BookingNo { get; set; }
        [Write(false)]
        public DateTime BookingDate { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public string ApproveByUser { get; set; }
        [Write(false)]
        public string RejectByUser { get; set; }
        [Write(false)]
        public string AddedByUser { get; set; }
        [Write(false)]
        public string UpdatedByUser { get; set; }
        [Write(false)]
        public string KYReqByName { get; set; }
        [Write(false)]
        public int IsBDS { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SpinnerList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnSubProgramList { get; set; }
        [Write(false)]
        public List<KYReqChild> Childs { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || KYReqMasterID > 0;

        [Write(false)]
        public List<KYReqBuyerTeam> KYReqBuyerTeams { get; set; }

        [Write(false)]
        public int RequisitionBy { get; set; }

        [Write(false)]
        public int CompanyId { get; set; }

        [Write(false)]
        public int DepartmentId { get; set; }

        [Write(false)]
        public int ConceptId { get; set; }

        [Write(false)]
        public int TrialNo { get; set; }

        [Write(false)]
        public int ConceptFor { get; set; }

        [Write(false)]
        public string ConceptForName { get; set; }

        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public string ItemSubGroup { get; set; }

        [Write(false)]
        public int KnittingTypeId { get; set; }

        [Write(false)]
        public int TechnicalNameId { get; set; }

        [Write(false)]
        public int CompositionId { get; set; }

        [Write(false)]
        public int FCMRMasterID { get; set; }

        [Write(false)]
        public int GSMId { get; set; }

        [Write(false)]
        public int GroupID { get; set; }

        [Write(false)]
        public string GroupIDs { get; set; }
        [Write(false)]
        public string RCompanyName { get; set; }

        [Write(false)]
        public DateTime ConceptDate { get; set; }
        [Write(false)]
        public string Buyer { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string GSM { get; set; }

        [Write(false)]
        public string ConceptStatus { get; set; }

        [Write(false)]
        public decimal Qty { get; set; }

        [Write(false)]
        public string RnDApproveBy { get; set; }

        [Write(false)]
        public string RnDAcknowledgeBy { get; set; }

        [Write(false)]
        public decimal TotalReqQty { get; set; }
        [Write(false)]
        public int YarnBrandID { get; set; }
        [Write(false)]
        public string YarnReqStatus { get; set; }
        [Write(false)]
        public List<FreeConceptMRMaster> FreeConceptMR { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> RCompanyList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnBrandList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> BookingList { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public string RnDReqByUser { get; set; }

        [Write(false)]
        public string RnDReqByName { get; set; }

        [Write(false)]
        public string Location { get; set; }


        [Write(false)]
        public DateTime ReqDate { get; set; }
        [Write(false)]
        public string YDStatus { get; set; }
        [Write(false)]
        public string IsIssue { get; set; }
        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        public string FCMRMasterIDs { get; set; }

        [Write(false)]
        public string FCIds { get; set; }

        [Write(false)]
        public string ParentKYReqNo { get; set; }
        [Write(false)]
        public int KPMasterID { get; set; } = 0;
        [Write(false)]
        public int RevisionPending { get; set; } = 0;
        [Write(false)]
        public int ConceptID { get; set; } = 0;
        [Write(false)]
        public decimal PlanQty { get; set; } = 0;
        [Write(false)]
        public int PlanNo { get; set; } = 0;
        [Write(false)]
        public bool Active { get; set; } = false;

        [Write(false)]
        public string UserName { get; set; } = "";
        [Write(false)]
        public string SubGroupName { get; set; } = "";
        //[Write(false)]
        //public int BuyerID { get; set; } = 0;
        [Write(false)]
        public int BuyerTeamID { get; set; } = 0;
        [Write(false)]
        public int ExportOrderID { get; set; } = 0;
        [Write(false)]
        public string BuyerTeam { get; set; } = "";
        [Write(false)]
        public string GroupConceptNo { get; set; } = "";
        [Write(false)]
        public decimal BookingQty { get; set; } = 0;
        [Write(false)]
        public string Contact { get; set; } = "";
        [Write(false)]
        public decimal ProduceKnittingQty { get; set; } = 0;
        [Write(false)]
        public decimal RemainingPlanQty { get; set; } = 0;
        [Write(false)]
        public string Uom { get; set; } = "";
        [Write(false)]
        public string Construction { get; set; } = "";
        [Write(false)]
        public string ColorName { get; set; } = "";
        [Write(false)]
        public string DyeingType { get; set; } = "";
        [Write(false)]
        public decimal Length { get; set; } = 0;
        [Write(false)]
        public decimal Width { get; set; } = 0;
        [Write(false)]
        public string Size { get; set; } = "";
        [Write(false)]
        public IEnumerable<Select2OptionModel> BuyerList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> BuyerTeamList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> LocationList { get; set; }

        [Write(false)]
        public IEnumerable<YarnProductSetupChildDTO> YarnProductSetupList { get; set; }

        [Write(false)]
        public IEnumerable<YarnProductSetupChildProgramDTO> ChildProgramList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> TechnicalParameterList { get; set; }

        [Write(false)]
        public IEnumerable<YarnProcessSetupMasterDTO> YarnProcessSetupList { get; set; }
        #endregion Additional
        
    }
    #region PendingWork
    //public class KYReqMasterValidator : AbstractValidator<KYReqMaster>
    //{
    //    public KYReqMasterValidator()
    //    {
    //        RuleFor(x => x.KYReqNo).NotEmpty();
    //        RuleFor(x => x.KYReqDate).NotEmpty();
    //        //RuleFor(x => x.BAnalysisID).NotEmpty();
    //        //RuleFor(x => x.BookingMasterID).NotEmpty();
    //        //RuleFor(x => x.SupplierID).NotEmpty();
    //        //RuleFor(x => x.SpinnerID).NotEmpty();
    //        RuleFor(x => x.Childs).Must(x => x.Count() > 0).WithMessage("You must add at least one Child Item.");
    //        When(x => x.Childs.Any(), () =>
    //        {
    //            RuleForEach(x => x.Childs).SetValidator(new KYReqChildValidator());
    //        });
    //    }
    //}

    #endregion

}
