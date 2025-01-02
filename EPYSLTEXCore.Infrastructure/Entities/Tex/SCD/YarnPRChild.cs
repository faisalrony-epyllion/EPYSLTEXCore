using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Static;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YARN_PR_CHILD)]
    public class YarnPRChild : YarnItemMaster, IDapperBaseEntity
    {

        [ExplicitKey]
        public int YarnPRChildID { get; set; } = 0;
        public int YarnPRMasterID { get; set; } = 0;
        public string YarnCategory { get; set; } = "";
        public decimal ReqQty { get; set; } = 0;
        public int ReqCone { get; set; } = 0;
        public int ConceptID { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public string ShadeCode { get; set; } = "";
        public int SetupChildID { get; set; } = 0;
        public int FPRCompanyID { get; set; } = 0;
        public int FCMRChildID { get; set; } = 0;
        public int PYBBookingChildID { get; set; } = 0;
        public int YDMaterialRequirementChildItemID { get; set; } = 0;
        public string HSCode { get; set; } = "";
        public int PreProcessRevNo { get; set; } = 0;
        public string RefLotNo { get; set; } = "";
        public int RefSpinnerID { get; set; } = 0;
        public int AllocationID { get; set; } = 0;
        public decimal PurchaseQty { get; set; } = 0;
        public decimal AllocationQty { get; set; } = 0;
        public int BaseTypeId { get; set; } = (int)EnumBaseType.None;
        public int AllocationChildID { get; set; } = 0;
        public int DayValidDurationId { get; set; } = 0;

        #region Additional Property
        [Write(false)]
        public string RefSpinner { get; set; } = "";
        [Write(false)]
        public string Source { get; set; } = "";
        [Write(false)]
        public EntityState EntityState { get; set; } = EntityState.Added;
        [Write(false)]
        public int TotalRows { get; set; } = 0;
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public List<YarnPRCompany> YarnPRCompanies { get; set; } = new List<YarnPRCompany>();
        [Write(false)]
        public string DisplayUnitDesc { get; set; } = "";
        [Write(false)]
        public string CompanyNames { get; set; } = "";
        [Write(false)]
        public string CPRCompanyNames { get; set; } = "";
        [Write(false)]
        public string Companys { get; set; } = "";
        [Write(false)]
        public string FPRCompanyName { get; set; } = "";
        [Write(false)]
        public string ConceptNo { get; set; } = "";
        [Write(false)]
        public string BookingID { get; set; } = "";
        [Write(false)]
        public string BookingNo { get; set; } = "";
        [Write(false)]
        public int[] CompanyIDs { get; set; } = Array.Empty<int>();
        [Write(false)]
        public int[] CPRCompanyIDs { get; set; } = Array.Empty<int>();
        [Write(false)]
        public string GroupConceptNo { get; set; } = "";
        [Write(false)]
        public int DayDuration { get; set; } = 0;
        [Write(false)]
        public string DayValidDurationName { get; set; } = "";
        [Write(false)]
        public decimal StockQty { get; set; } = 0; 
        [Write(false)]
        public decimal PRQty { get; set; } = 0;
        [Write(false)]
        public decimal ROLLocalPurchase { get; set; } = 0;
        [Write(false)]
        public decimal ROLForeignPurchase { get; set; } = 0;
        [Write(false)]
        public decimal ReOrderQty { get; set; } = 0;
        [Write(false)]
        public decimal MaximumPRQtyLP { get; set; } = 0;
        [Write(false)]
        public decimal MaximumPRQtyFP { get; set; } = 0;
        [Write(false)]
        public decimal MOQ { get; set; } = 0;
        #endregion Additional Property
    }
}
