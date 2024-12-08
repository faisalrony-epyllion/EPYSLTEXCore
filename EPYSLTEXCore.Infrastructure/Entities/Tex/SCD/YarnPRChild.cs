using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Static;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YARN_PR_CHILD)]
    public class YarnPRChild : YarnItemMaster, IDapperBaseEntity
    {
        public YarnPRChild()
        {
            YDMaterialRequirementChildItemID = 0;
            Remarks = "";
            YarnPRCompanies = new List<YarnPRCompany>();
            CompanyIDs = new int[] { CompnayIDConstants.EFL };
            CPRCompanyIDs = new int[] { CompnayIDConstants.EFL };
            //CompanyNames = "EFL";
            //CPRCompanyNames = "EFL";
            BookingNo = "";
            ConceptNo = "";
            YarnPRChildID = 0;
            YarnPRMasterID = 0;
            YarnCategory = "";
            ReqQty = 0;
            ReqCone = 0;
            ConceptID = 0;
            Remarks = "";
            ShadeCode = "";
            SetupChildID = 0;
            FPRCompanyID = 0;
            FCMRChildID = 0;
            PYBBookingChildID = 0;
            YDMaterialRequirementChildItemID = 0;
            HSCode = "";
            PreProcessRevNo = 0;
            RefLotNo = "";
            RefSpinnerID = 0;
            AllocationID = 0;
            PurchaseQty = 0;
            AllocationQty = 0;
            BaseTypeId = (int)EnumBaseType.None;
            AllocationChildID = 0;
        }

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
        public EntityState EntityState { get; set; }
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
        public int[] CompanyIDs { get; set; }
        [Write(false)]
        public int[] CPRCompanyIDs { get; set; }
        [Write(false)]
        public string GroupConceptNo { get; set; } = "";
        [Write(false)]
        public int DayDuration { get; set; } = 0;
        [Write(false)]
        public string DayValidDurationName { get; set; } = "";
        #endregion Additional Property
    }

    //public class YarnPRChildValidator : AbstractValidator<YarnPRChild>
    //{
    //    public YarnPRChildValidator()
    //    {
    //        //RuleFor(x => x.Segment1ValueId).NotEmpty().WithMessage("Composition is required!");
    //        //RuleFor(x => x.Segment2ValueId).NotEmpty().WithMessage("Yarn type is required!");
    //        //RuleFor(x => x.Segment3ValueId).NotEmpty().WithMessage("Manufacturing process is required!");
    //        //RuleFor(x => x.Segment5ValueId).NotEmpty().WithMessage("Yarn count is required!");
    //        //RuleFor(x => x.Segment7ValueId).NotEmpty().WithMessage("Yarn count is required!");
    //        RuleFor(x => x.CompanyIDs).NotEmpty().WithMessage("Company field is required!");
    //        RuleFor(x => x.ReqQty).NotEmpty().WithMessage("Req Qty is required!");
    //        //RuleFor(x => x.HSCode).NotEmpty().WithMessage("HS Code is required!").MaximumLength(20).WithMessage("HS code can not be more than 20 characters.");
    //    }
    //}
}
