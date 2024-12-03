using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Static;
using FluentValidation;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnPOChild)]
    public class YarnPOChild : YarnItemMaster, IDapperBaseEntity
    {
        public YarnPOChild()
        {
            EntityState = EntityState.Added;
            PoQty = 0m;
            Rate = 0m;
            ItemMasterID = 0;
            PoForId = 0;
            BaseTypeId = (int)EnumBaseType.None;
            BuyerID = 0;
            BaseTypeName = "";
            YarnPOChildBuyers = new List<YarnPOChildBuyer>();
            YarnPOChildOrders = new List<YarnPOChildOrder>();
            YarnPOChildYarnSubPrograms = new List<YarnPOChildYarnSubProgram>();
            UnitID = 28;
            DisplayUnitDesc = "Kg";
            YarnCategory = "";
            YarnChildPoBuyerIds = "";
            YarnChildPoExportIds = "";
            YarnChildPoEWOs = "";
            ReceivedCompleted = false;
            EntityState = EntityState.Added;
            YarnChildPoBuyerIdArray = new int[] { };
            YarnChildPoExportIdArray = new int[] { };
            IsYarnReceive = false;
            ReceiveQty = 0;
        }

        [ExplicitKey]
        public int YPOChildID { get; set; }
        public int YPOMasterID { get; set; }
        public string YarnCategory { get; set; }
        public decimal PoQty { get; set; }
        public decimal Rate { get; set; }
        public decimal PIValue { get; set; }
        public string Remarks { get; set; }
        public string YarnLotNo { get; set; }
        public string HSCode { get; set; }
        public string YarnShade { get; set; }
        public int YarnProgramId { get; set; }
        public int NoOfThread { get; set; }
        public int PoForId { get; set; } //2161,2162,2163,2164
        public int PRMasterID { get; set; }
        public int PRChildID { get; set; }
        public string ShadeCode { get; set; }
        public int ConceptID { get; set; }
        public string EWOOthers { get; set; }
        public int POCone { get; set; }
        public string QuotationRefNo { get; set; }
        public DateTime? QuotationRefDate { get; set; }
        public string BookingNo { get; set; }
        public int BuyerID { get; set; }
        public bool ReceivedCompleted { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public int YarnStockSetId { get; set; } = 0;
        public int DayValidDurationId { get; set; } = 0;

        #region Additional Property
        [Write(false)]
        public decimal BalanceQTY { get; set; } = 0;
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public int ReqCone { get; set; }
        [Write(false)]
        public DateTime YarnPRRequiredDate { get; set; }
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public int TotalRows { get; set; }
        [Write(false)]
        public bool IsYarnReceive { get; set; }
        [Write(false)]
        public bool IsYarnReceiveByPI { get; set; }
        [Write(false)]
        public decimal ReceiveQty { get; set; }
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public List<YarnPOChildYarnSubProgram> YarnPOChildYarnSubPrograms { get; set; }
        [Write(false)]
        public List<YarnPOChildBuyer> YarnPOChildBuyers { get; set; }
        [Write(false)]
        public List<YarnPOChildOrder> YarnPOChildOrders { get; set; }
        [Write(false)]
        public string YarnSubProgramIds { get; set; }
        [Write(false)]
        public string YarnSubProgramNames { get; set; }
        [Write(false)]
        public int[] YarnSubProgramIdArray { get; set; }
        [Write(false)]
        public string YarnChildPoBuyerIds { get; set; }
        [Write(false)]
        public int[] YarnChildPoBuyerIdArray { get; set; }
        [Write(false)]
        public string YarnChildPoExportIds { get; set; }
        [Write(false)]
        public int[] YarnChildPoExportIdArray { get; set; }
        [Write(false)]
        public string POFor { get; set; }
        [Write(false)]
        public string YarnPRNo { get; set; }
        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public decimal PIQtyN { get; set; }
        [Write(false)]
        public decimal PIRateN { get; set; }
        [Write(false)]
        public decimal PIValueN { get; set; }
        [Write(false)]
        public decimal StockQty { get; set; }
        [Write(false)]
        public decimal ReqQty { get; set; }
        [Write(false)]
        public string YarnProgram { get; set; }
        [Write(false)]
        public string YarnChildPoEWOs { get; set; }
        [Write(false)]
        public string BuyerNames { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public decimal TotalPOQty { get; set; }
        [Write(false)]
        public decimal TotalPOValue { get; set; }
        [Write(false)]
        public int BaseTypeId { get; set; }
        [Write(false)]
        public string BaseTypeName { get; set; }
        [Write(false)]
        public int DayDuration { get; set; } = 0;
        [Write(false)]
        public string DayValidDurationName { get; set; } = "";
        [Write(false)]
        public int DayValidDurationIdPR { get; set; } = 0;
        [Write(false)]
        public string DayValidDurationIdPRName { get; set; } = "";
        [Write(false)]
        public int DayDurationPR { get; set; } = 0;
        [Write(false)]
        public decimal MaxPOQty { get; set; } = 0;
        [Write(false)]
        public bool IsInvalidItem { get; set; } = false;
        #endregion Additional Property
    }

    #region Validators

    public class YarnPOChildValidator : AbstractValidator<YarnPOChild>
    {
        public YarnPOChildValidator()
        {
            RuleFor(x => x.UnitID).NotEmpty();
            //RuleFor(x => x.PoForId).NotEmpty().WithMessage("PO for Field is required.");
            RuleFor(x => x.PoQty).NotEmpty().WithMessage("Yarn PO Qty must be greater than zero");
            RuleFor(x => x.Rate).NotEmpty().WithMessage("Yarn PO Rate must be greater than zero");
            RuleFor(x => x.Remarks).MaximumLength(200);
            RuleFor(x => x.YarnLotNo).MaximumLength(50);
            //RuleFor(x => x.HSCode).NotEmpty().WithMessage("HS Code is required.");
            RuleFor(x => x.Segment4ValueDesc).MaximumLength(50);
        }
    }

    #endregion Validators
}
