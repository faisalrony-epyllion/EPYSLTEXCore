using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.Knitting_Plan_Group)]
    public class KnittingPlanGroup : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int GroupID { get; set; } = 0;
        public int MachineGauge { get; set; } = 0;
        public int MachineDia { get; set; } = 0;
        public bool IsSubContact { get; set; } = false;
        public int BrandID { get; set; } = 0;
        public int KnittingTypeID { get; set; } = 0;
        public decimal Needle { get; set; } = 0;
        public int CPI { get; set; } = 0;
        public int SubGroupID { get; set; } = 0;
        public int BuyerID { get; set; } = 0;
        public int BuyerTeamID { get; set; } = 0;
        public decimal PlanQty { get; set; } = 0;
        public decimal TotalQty { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ColorName { get; set; } = "";
        public string GroupConceptNo { get; set; } = "";
        public int PreRevisionNo { get; set; } = 0;
        public int RevisionNo { get; set; } = 0;
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }
        public bool IsAdditional { get; set; } = false;
        public int ParentGroupID { get; set; } = 0;
        public int AdditionNo { get; set; } = 0;
        #endregion Table Properties

        #region Additional Fields

        [Write(false)]
        public List<KnittingPlanMaster> KnittingPlans { get; set; } = new List<KnittingPlanMaster>();
        [Write(false)]
        public List<KnittingPlanChild> KnittingPlanChilds { get; set; } = new List<KnittingPlanChild>();
        [Write(false)]
        public List<KnittingPlanChild> Childs { get; set; } = new List<KnittingPlanChild>();
        [Write(false)]
        public List<KnittingPlanYarn> Yarns { get; set; } = new List<KnittingPlanYarn>();
        [Write(false)]
        public List<Select2OptionModel> YarnBrandList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<KnittingMachine> KnittingMachines { get; set; } = new List<KnittingMachine>();
        [Write(false)]
        public List<KnittingMachine> KnittingSubContracts { get; set; } = new List<KnittingMachine>();
        [Write(false)]
        public List<Select2OptionModel> KnittingTypeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> MCSubClassList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> MachineTypeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<KJobCardMaster> KJobCardMasters { get; set; } = new List<KJobCardMaster>();
        [Write(false)]
        public List<KJobCardChild> KJobCardChilds { get; set; } = new List<KJobCardChild>();
        [Write(false)]
        public string ConceptNo { get; set; } = "";
        [Write(false)]
        public int MCSubClassID { get; set; } = 0;
        [Write(false)]
        public int ConsumptionID { get; set; } = 0;

        [Write(false)]
        public string SubGroupName { get; set; } = "";
        [Write(false)]
        public int CompanyID { get; set; } = 0;
        [Write(false)]
        public int ExportOrderID { get; set; } = 0;
        [Write(false)]
        public int ConstructionID { get; set; } = 0;

        [Write(false)]
        public int TechnicalNameId { get; set; } = 0;

        [Write(false)]
        public int CompositionID { get; set; } = 0;

        [Write(false)]
        public int GSMID { get; set; } = 0;

        [Write(false)]
        public decimal Qty { get; set; } = 0;

        [Write(false)]
        public decimal BookingQty { get; set; } = 0;

        [Write(false)]
        public string Remarks { get; set; } = "";

        [Write(false)]
        public string KnittingType { get; set; } = "";

        [Write(false)]
        public string Composition { get; set; } = "";
        [Write(false)]
        public string Construction { get; set; } = "";
        [Write(false)]
        public string TechnicalName { get; set; } = "";
        [Write(false)]
        public decimal Length { get; set; } = 0;
        [Write(false)]
        public decimal Width { get; set; } = 0;
        [Write(false)]
        public string MCSubClass { get; set; } = "";
        [Write(false)]
        public string GSM { get; set; } = "";
        [Write(false)]
        public string ConceptStatus { get; set; } = "";
        [Write(false)]
        public string ConceptForName { get; set; } = "";
        [Write(false)]
        public string BAnalysisNo { get; set; } = "";
        //[Write(false)]
        //public bool isModified { get; set; } = false;
        [Write(false)]
        public DateTime? BAnalysisDate { get; set; }
        [Write(false)]
        public string Buyer { get; set; } = "";
        [Write(false)]
        public string BuyerTeam { get; set; } = "";
        [Write(false)]
        public int ItemMasterID { get; set; } = 0;
        [Write(false)]
        public string Company { get; set; } = "";
        [Write(false)]
        public DateTime? ReqDeliveryDate { get; set; }
        [Write(false)]
        public bool NeedPreFinishingProcess { get; set; } = false;
        [Write(false)]
        public DateTime? ConceptDate { get; set; }
        [Write(false)]
        public decimal JobCardQty { get; set; } = 0;
        [Write(false)]
        public decimal BalanceQty { get; set; } = 0;
        [Write(false)]
        public int ContactID { get; set; } = 0;
        [Write(false)]
        public int MachineKnittingTypeID { get; set; } = 0;
        [Write(false)]
        public int KnittingMachineID { get; set; } = 0;
        [Write(false)]
        public string StyleNo { get; set; } = "";
        [Write(false)]
        public int SeasonID { get; set; } = 0;
        [Write(false)]
        public int BookingID { get; set; } = 0;
        [Write(false)]
        public int IsBDS { get; set; } = 0;
        [Write(false)]
        public bool IsRevision { get; set; } = false;
        [Write(false)]
        public int UserId { get; set; } = 0;
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || GroupID > 0;

        #endregion Additional Fields
    }
}
