using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnPOMaster)]
    public class YarnPOMaster : DapperBaseEntity
    {
        public YarnPOMaster()
        {
            YPOMasterID = 0;
            DateAdded = DateTime.Now;
            RevisionNo = 0;
            //CurrencyId = 0;
            QuotationRefNo = "";
            TypeOfLcId = 0;
            CalculationofTenure = 0;
            CreditDays = 0;
            CountryOfOriginId = 0;
            ShippingTolerance = 0m;
            PortofLoadingID = 0;
            PortofDischargeID = 0;
            ShipmentModeId = 0;
            Proposed = false;
            ProposedBy = 0;
            UnApprove = false;
            UnApproveBy = 0;
            Approved = false;
            ApprovedBy = 0;
            QuotationRefDate = DateTime.Now;
            YarnPOChilds = new List<YarnPOChild>();
            PoNo = AppConstants.NEW;
            PoDate = DateTime.Now;
            DeliveryStartDate = DateTime.Now;
            DeliveryEndDate = DateTime.Now;
            CurrencyId = 2;
            CurrencyCode = "USD";
            ReImbursementCurrencyId = 2;
            ReImbursmentCurrency = "USD";
            ConceptNo = "";
            BookingNo = "";
            YarnChildPoBuyerIds = "";
            SubGroupID = AppConstants.ITEM_SUB_GROUP_YARN;
            IsRevise = false;
            YarnPOChildOrders = new List<YarnPOChildOrder>();
            YarnPOForBuyers = new List<YarnPOChildBuyer>();
            ExportOrderList = new List<Select2OptionModel>();
            BaseTypes = new List<Select2OptionModel>();
            CompanyId = 0;
            SupplierId = 0;
            PoForId = 0;
            Remarks = "";
            IncoTermsId = 0;
            PaymentTermsId = 0;
            Charges = "";
            UnapproveReason = "";
            OfferValidity = 0;
            SignIn = false;
            SignInBy = 0;
            SupplierAcknowledge = false;
            SupplierAcknowledgeBy = 0;
            SupplierReject = false;
            SupplierRejectBy = 0;
            SupplierRejectReason = "";
            PRMasterID = 0;
            SubGroupID = 0;
            AddedBy = 0;
            UpdatedBy = 0;
            IsRevision = false;
            RevisionBy = 0;
            RevisionReason = "";
            IsCancel = false;
            CancelBy = 0;
            CancelReason = "";
            IgnoreValidationPOIds = new List<int>();
        }

        #region Table Properties

        [ExplicitKey]
        public int YPOMasterID { get; set; }
        public string PoNo { get; set; }
        public DateTime PoDate { get; set; }
        public int RevisionNo { get; set; }
        public DateTime? RevisionDate { get; set; }
        public int CompanyId { get; set; }
        public int SupplierId { get; set; }
        public int PoForId { get; set; }
        public int CurrencyId { get; set; }
        public string QuotationRefNo { get; set; }
        public DateTime? QuotationRefDate { get; set; }
        public DateTime DeliveryStartDate { get; set; }
        public DateTime DeliveryEndDate { get; set; }
        public string Remarks { get; set; }
        public string InternalNotes { get; set; }
        public int IncoTermsId { get; set; }
        public int PaymentTermsId { get; set; }
        public int? TypeOfLcId { get; set; }
        public int? TenureofLc { get; set; }
        public int? CalculationofTenure { get; set; }
        public int? CreditDays { get; set; }
        public int ReImbursementCurrencyId { get; set; }
        public string Charges { get; set; }
        public int CountryOfOriginId { get; set; }
        public bool TransShipmentAllow { get; set; }
        public decimal ShippingTolerance { get; set; }
        public int? PortofLoadingID { get; set; }
        public int? PortofDischargeID { get; set; }
        public int? ShipmentModeId { get; set; }
        public bool Proposed { get; set; }
        public int ProposedBy { get; set; }
        public DateTime? ProposedDate { get; set; }
        public bool UnApprove { get; set; }
        public string UnapproveReason { get; set; }
        public bool Approved { get; set; }
        public int ApprovedBy { get; set; }
        public int UnApproveBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? UnApproveDate { get; set; }
        public int OfferValidity { get; set; }
        public int? QualityApprovalProcedureId { get; set; }
        public bool SignIn { get; set; }
        public int SignInBy { get; set; }
        public DateTime? SignInDate { get; set; }
        public bool SupplierAcknowledge { get; set; }
        public int SupplierAcknowledgeBy { get; set; }
        public DateTime? SupplierAcknowledgeDate { get; set; }
        public bool SupplierReject { get; set; }
        public int SupplierRejectBy { get; set; }
        public string SupplierRejectReason { get; set; }
        public int PRMasterID { get; set; }
        public int SubGroupID { get; set; }
        public int AddedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string ConceptNo { get; set; }
        public string BookingNo { get; set; }
        public bool IsRevision { get; set; }
        public int RevisionBy { get; set; }
        public string RevisionReason { get; set; }
        public bool IsCancel { get; set; }
        public int CancelBy { get; set; }
        public DateTime? CancelDate { get; set; }
        public string CancelReason { get; set; }

        #endregion Table Properties

        #region Additional Columns

        [Write(false)]
        public List<YarnPOChild> YarnPOChilds { get; set; }
        [Write(false)]
        public string StatusPIPO { get; set; } = "";
        [Write(false)]
        public string[] BuyerIds { get; set; }
        [Write(false)]
        public string PortOfLoadingName { get; set; }

        [Write(false)]
        public string PortOfDischargeName { get; set; }

        [Write(false)]
        public string POStatus { get; set; }
        [Write(false)]
        public string AddedByName { get; set; } = "";
        [Write(false)]
        public bool ReceivedCompleted { get; set; }

        [Write(false)]
        public string PIStatus { get; set; }

        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public string SupplierName { get; set; }

        [Write(false)]
        public string POFor { get; set; }

        [Write(false)]
        public decimal TotalQty { get; set; }
        [Write(false)]
        public decimal BalanceQTY { get; set; } = 0;
        [Write(false)]
        public decimal POQty { get; set; } = 0;

        [Write(false)]
        public decimal TotalValue { get; set; }

        [Write(false)]
        public string CurrencyCode { get; set; }

        [Write(false)]
        public string BranchName { get; set; }

        [Write(false)]
        public string IncoTermsName { get; set; }

        [Write(false)]
        public string PaymentTermsName { get; set; }

        [Write(false)]
        public bool IsItemGenerate { get; set; }

        [Write(false)]
        public string TypeOfLC { get; set; }

        [Write(false)]
        public string LCTenure { get; set; }

        [Write(false)]
        public string ReImbursmentCurrency { get; set; }

        [Write(false)]
        public string CountyOfOrigin { get; set; }

        [Write(false)]
        public string ShipmentMode { get; set; }

        [Write(false)]
        public DateTime? InHouseDate { get; set; }

        [Write(false)]
        public DateTime? SFToPLDate { get; set; }

        [Write(false)]
        public DateTime? PLToPDDate { get; set; }

        [Write(false)]
        public DateTime? PCFDate { get; set; }

        [Write(false)]
        public int SFToPLDays { get; set; }

        [Write(false)]
        public int PLToPDDays { get; set; }

        [Write(false)]
        public int PCFDays { get; set; }

        [Write(false)]
        public int InHouseDays { get; set; }

        [Write(false)]
        public string UserName { get; set; }

        [Write(false)]
        public DateTime PRRequiredDate { get; set; }

        [Write(false)]
        public DateTime PRDate { get; set; }
        [Write(false)]
        public string PRNO { get; set; }
        [Write(false)]
        public string PRByUser { get; set; }
        [Write(false)]
        public string ApproveBy { get; set; }
        [Write(false)]
        public string RejectBy { get; set; }
        [Write(false)]
        public string AcknowledgeBy { get; set; }
        [Write(false)]
        public bool InLand { get; set; }
        [Write(false)]
        public string CountryOfOriginName { get; set; }
        [Write(false)]
        public int YarnPRChildID { get; set; }
        [Write(false)]
        public string YarnChildPoBuyerIds { get; set; }
        [Write(false)]
        public string BuyerName { get; set; } = "";
        [Write(false)]
        public string YarnPRNo { get; set; } = "";
        [Write(false)]
        public int ConceptID { get; set; } = 0;
        [Write(false)]
        public int BaseTypeId { get; set; } = 0;
        [Write(false)]
        public int DayValidDurationId { get; set; } = 0;
        [Write(false)]
        public string DayValidDurationName { get; set; } = "";
        [Write(false)]
        public bool IsReceivedPO { get; set; } = false;
        [Write(false)]
        public bool IsCheckDVD { get; set; } = true;
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YPOMasterID > 0;
        [Write(false)]
        public IEnumerable<YarnPOChildYarnSubProgram> YarnPOChildYarnSubPrograms { get; set; }

        [Write(false)]
        public IEnumerable<YarnPOChildOrder> YarnPOChildOrders { get; set; }

        [Write(false)]
        public IEnumerable<YarnPOChildBuyer> YarnPOForBuyers { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> BuyerList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ExportOrderList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> SupplierList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> IncoTermsList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> PaymentTermsList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> PIForList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> PortofLoadingList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> PortofDischargeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CalculationofTenureList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CreditDaysList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CurrencyTypeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> BankBranchList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ShipmentModeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CountryOfOriginList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> TypeOfLcList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> OfferValidityList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> QualityApprovalProcedureList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnUnitList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> FiberTypeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> BlendTypeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnTypeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnProgramList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnSubProgramList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ManufacturingLineList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ManufacturingProcessList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ManufacturingSubProcessList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnCompositionList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnCountList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnColorList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ColorGradeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ShadeList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> BaseTypes { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> DayValidDurations { get; set; }
        [Write(false)]
        public List<int> IgnoreValidationPOIds { get; set; }

        [Write(false)]
        public string YarnPRBy { get; set; }
        [Write(false)]
        public string ShadeCode { get; set; }

        [Write(false)]
        public decimal ReqQty { get; set; }
        [Write(false)]
        public string Segment1ValueDesc { get; set; }
        [Write(false)]
        public string Segment2ValueDesc { get; set; }
        [Write(false)]
        public string Segment3ValueDesc { get; set; }
        [Write(false)]
        public string Segment4ValueDesc { get; set; }
        [Write(false)]
        public string Segment5ValueDesc { get; set; }
        [Write(false)]
        public string Segment6ValueDesc { get; set; }
        [Write(false)]
        public string Segment7ValueDesc { get; set; }
        [Write(false)]
        public string Segment8ValueDesc { get; set; }
        [Write(false)]
        public bool IsRevise { get; set; }
        [Write(false)]
        public bool IsYarnReceived { get; set; } = false;
        #endregion Additional Columns
    }

    #region Validator

    //public class YarnPOMasterValidator : AbstractValidator<YarnPOMaster>
    //{
    //    public YarnPOMasterValidator()
    //    {
    //        RuleFor(x => x.PoDate).NotEmpty();
    //        RuleFor(x => x.CompanyId).NotEmpty();
    //        RuleFor(x => x.SupplierId).NotEmpty().WithMessage("Supplier field is required.");
    //        RuleFor(x => x.CurrencyId).NotEmpty();
    //        RuleFor(x => x.QuotationRefNo).MaximumLength(100);
    //        RuleFor(x => x.DeliveryStartDate).NotEmpty();
    //        RuleFor(x => x.DeliveryEndDate).NotEmpty();
    //        RuleFor(x => x.Remarks).MaximumLength(500);
    //        RuleFor(x => x.InternalNotes).MaximumLength(500);
    //        RuleFor(x => x.IncoTermsId).NotEmpty().WithMessage("Inco Terms field is required.");
    //        RuleFor(x => x.PaymentTermsId).NotEmpty().WithMessage("Payment Terms field is required.");
    //        RuleFor(x => x.ReImbursementCurrencyId).NotEmpty();
    //        RuleFor(x => x.Charges).MaximumLength(500);
    //        RuleFor(x => x.ShippingTolerance).Must(x => x >= 0 && x <= 10);
    //        RuleFor(x => x.CountryOfOriginId).NotEmpty();
    //        RuleFor(x => x.UnapproveReason).MaximumLength(300);
    //        RuleFor(x => x.OfferValidity).NotEmpty();

    //        RuleFor(x => x.YarnPOChilds).Must(x => x.Count() > 0).WithMessage("You must add at least one Yarn Child Item.");
    //        When(x => x.YarnPOChilds.Any(), () =>
    //        {
    //            RuleForEach(x => x.YarnPOChilds).SetValidator(new YarnPOChildValidator());
    //        });
    //    }
    //}

    #endregion Validator
}
