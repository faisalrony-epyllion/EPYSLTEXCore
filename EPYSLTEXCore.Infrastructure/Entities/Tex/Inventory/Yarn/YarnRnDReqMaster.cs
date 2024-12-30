using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YARN_RnD_REQ_MASTER)]
    public class YarnRnDReqMaster: DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int RnDReqMasterID { get; set; } = 0;

        ///<summary>
        /// RnDReqNo (length: 50)
        ///</summary>
        public string RnDReqNo { get; set; } = AppConstants.NEW;

        ///<summary>
        /// RnDReqBy
        ///</summary>
        public int RnDReqBy { get; set; }=0;

        ///<summary>
        /// RnDReqDate
        ///</summary>
        public DateTime RnDReqDate { get; set; }= DateTime.Now;

        ///<summary>
        /// RnD for buyer
        ///</summary>
        public int BuyerId { get; set; }= 0;

        /// <summary>
        /// Remarks
        /// </summary>
        public string Remarks { get; set; } = "";

        ///<summary>
        /// IsApprove
        ///</summary>
        public bool IsApprove { get; set; }= false;

        ///<summary>
        /// ApproveDate
        ///</summary>
        public DateTime? ApproveDate { get; set; }=DateTime.Now;

        ///<summary>
        /// ApproveBy
        ///</summary>
        public int? ApproveBy { get; set; } = 0;

        ///<summary>
        /// IsAcknowledge
        ///</summary>
        public bool IsAcknowledge { get; set; }=false;

        ///<summary>
        /// AcknowledgeDate
        ///</summary>
        public DateTime? AcknowledgeDate { get; set; }=null;

        ///<summary>
        /// AcknowledgeBy
        ///</summary>
        public int? AcknowledgeBy { get; set; } = 0;

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; } = 0;

        ///<summary>
        /// DateAdded
        ///</summary>
        public DateTime DateAdded { get; set; } =  DateTime.Now;

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; } = 0;

        ///<summary>
        /// DateUpdated
        ///</summary>
        public DateTime? DateUpdated { get; set; } = DateTime.Now;

        public int RCompanyID { get; set; } = 0;
        public int OCompanyID { get; set; } = 0;
        public int LocationID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public bool IsReqForYD { get; set; }=false ;
        public string ConceptNo { get; set; } = "";
        public int FloorID { get; set; } = 0;
        public bool IsUnAcknowledge { get; set; } = false;
        public DateTime? UnAcknowledgeDate { get; set; } = DateTime.Now;
        public int? UnAcknowledgeBy { get; set; } = 0;
        public string UnAcknowledgeReason { get; set; } = "";
        public int PreProcessRevNo { get; set; } = 0;
        public int RevisionNo { get; set; } = 0;
        public DateTime? RevisionDate { get; set; }=DateTime.Now;
        public int RevisionBy { get; set; } = 0;
        public string RevisionReason { get; set; } = "";
        public bool IsAdditional { get; set; } = false;
        public bool IsWOKnittingInfo { get; set; }=false;
        #endregion Table Properties

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.RnDReqMasterID > 0;

        [Write(false)]
        public List<YarnRnDReqChild> Childs { get; set; }

        [Write(false)]
        public List<YarnRnDReqBuyerTeam> YarnRnDReqBuyerTeams { get; set; }

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
        public string Color { get; set; } = "";

        [Write(false)]
        public string ConceptStatus { get; set; }

        [Write(false)]
        public decimal Qty { get; set; }

        [Write(false)]
        public string RnDApproveBy { get; set; }

        [Write(false)]
        public string RnDAcknowledgeBy { get; set; }
        [Write(false)]
        public List<Select2OptionModel> StockTypeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public decimal TotalReqQty { get; set; }
        [Write(false)]
        public int YarnBrandID { get; set; }
        [Write(false)]
        public string YarnReqStatus { get; set; }
        [Write(false)]
        public List<FreeConceptMRMaster> FreeConceptMR { get; set; }= new List<FreeConceptMRMaster>();

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
        public string Supplier { get; set; }

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
        public string ParentRnDReqNo { get; set; }
        [Write(false)]
        public string RequisitionType { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> BuyerList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> BuyerTeamList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> SupplierList { get; set; }

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

        #endregion Additional Properties

        //public YarnRnDReqMaster()
        //{
        //    FCMRMasterIDs = "";
        //    RnDReqDate = DateTime.Now;
        //    RnDReqNo = AppConstants.NEW;
        //    DateAdded = DateTime.Now;
        //    IsApprove = false;
        //    IsAcknowledge = false;
        //    UnAcknowledgeBy = 0;
        //    IsAdditional = false;
        //    IsWOKnittingInfo = false;
        //    Childs = new List<YarnRnDReqChild>();
        //    YarnRnDReqBuyerTeams = new List<YarnRnDReqBuyerTeam>();
        //    GroupID = 0;
        //    GroupIDs = "";
        //    FreeConceptMR = new List<FreeConceptMRMaster>();
        //    YarnReqStatus = "";
        //}
    }
}
