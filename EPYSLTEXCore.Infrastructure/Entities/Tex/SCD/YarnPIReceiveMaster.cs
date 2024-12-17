using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnPIReceiveMaster)]
    public class YarnPIReceiveMaster : DapperBaseEntity
    {
        public YarnPIReceiveMaster()
        {
            IncoTermsID = 0;
            PaymentTermsID = 0;
            TransShipmentAllow = false;
            ShippingTolerance = 0m;
            PIDate = DateTime.Now;
            DateAdded = DateTime.Now;
            Accept = false;
            AcceptBy = 0;
            UnAcknowledgeBy = 0;
            Reject = false;
            RejectBy = 0;
            YarnPIReceiveAdditionalValueList = new List<YarnPIReceiveAdditionalValue>();
            Childs = new List<YarnPIReceiveChild>();
            YarnPIReceiveDeductionValueList = new List<YarnPIReceiveDeductionValue>();
            YarnPIReceivePOList = new List<YarnPIReceivePO>();
            AvailablePOForPIList = new List<AvailablePOForPI>();
            YarnPO = new YarnPOMaster();
            YarnPOChilds = new List<YarnPOChild>();
            TypeOfLCID = 0;
            IsRevise = false;
            POIds = "";
         
            RevisionBy = 0;
        }

        [ExplicitKey]
        public int YPIReceiveMasterID { get; set; }
        public DateTime PIDate { get; set; }
        public int RevisionNo { get; set; }
        public DateTime? RevisionDate { get; set; }
        public int SupplierID { get; set; }
        public string Remarks { get; set; }
        public bool ReceivePI { get; set; }
        public int ReceivePIBy { get; set; }
        public DateTime? ReceivePIDate { get; set; }
        public string PIFilePath { get; set; }
        public string AttachmentPreviewTemplate { get; set; }
        public decimal NetPIValue { get; set; }
        public int AddedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int IncoTermsID { get; set; }
        public int PaymentTermsID { get; set; }
        public int? TypeOfLCID { get; set; }
        public int? TenureofLC { get; set; }
        public int? CalculationofTenure { get; set; }
        public int? CreditDays { get; set; }
        public int? OfferValidity { get; set; }
        public int? ReImbursementCurrencyID { get; set; }
        public string Charges { get; set; }
        public int? CountryOfOriginID { get; set; }
        public bool TransShipmentAllow { get; set; }
        public decimal ShippingTolerance { get; set; }
        public int? PortofLoadingID { get; set; }
        public int? PortofDischargeID { get; set; }
        public int? ShipmentModeID { get; set; }
        public string YPINo { get; set; }
        public int CompanyID { get; set; }
        public bool Accept { get; set; }
        public int? AcceptBy { get; set; }
        public DateTime? AcceptDate { get; set; }
        public bool Reject { get; set; }
        public int? RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; }
        public bool NeedsReview { get; set; }
        public string PONo { get; set; }
        public int CurrencyID { get; set; }
        public bool UploadAcknowledge { get; set; }
        public bool Acknowledge { get; set; }
        public int AcknowledgeBy { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsCDA { get; set; }
        public bool UnAcknowledge { get; set; }
        public int UnAcknowledgeBy { get; set; }
        public DateTime? UnAcknowledgeDate { get; set; }
        public string UnAcknowledgeReason { get; set; }
        public int RevisionBy { get; set; }
        public int PreProcessRevNo { get; set; }

        #region Additional

        [Write(false)]
        public List<YarnPIReceiveChild> Childs { get; set; }

        [Write(false)]
        public List<YarnPIReceiveAdditionalValue> YarnPIReceiveAdditionalValueList { get; set; }

        [Write(false)]
        public List<YarnPIReceiveDeductionValue> YarnPIReceiveDeductionValueList { get; set; }

        [Write(false)]
        public List<YarnPIReceivePO> YarnPIReceivePOList { get; set; }

        [Write(false)]
        public List<YarnPOChild> YarnPOChildList { get; set; }

        [Write(false)]
        public int YarnPOMasterRevision { get; set; }
        [Write(false)]
        public bool IsRevise { get; set; }
        [Write(false)]
        public string SupplierName { get; set; }

        [Write(false)]
        public int YPOMasterID { get; set; }

        [Write(false)]
        public string PODate { get; set; }
        [Write(false)]
        public string QuotationRefNo { get; set; }
        [Write(false)]
        public string POAddedByName { get; set; } = "";

        [Write(false)]
        public string POFor { get; set; }

        [Write(false)]
        public string DeliveryStartDate { get; set; }

        [Write(false)]
        public string DeliveryEndDate { get; set; }

        [Write(false)]
        public decimal POQty { get; set; }
        [Write(false)]
        public decimal POValue { get; set; }

        [Write(false)]
        public decimal PIQty { get; set; }

        [Write(false)]
        public decimal PIValue { get; set; }

        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public string TypeOfLc { get; set; }

        [Write(false)]
        public decimal TotalAddValue { get; set; }

        [Write(false)]
        public decimal TotalDeductionValue { get; set; }

        [Write(false)]
        public string POCreatedBy { get; set; }

        [Write(false)]
        public int SubGroupID { get; set; }
        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        public string POIds { get; set; }
        [Write(false)]
        public string isFileExist { get; set; }

        [Write(false)]
        public int Approved { get; set; }

        

        [Write(false)]
        public List<AvailablePOForPI> AvailablePOForPIList { get; set; }

        [Write(false)]
        public YarnPOMaster YarnPO { get; set; }
        [Write(false)]
        public List<YarnPOChild> YarnPOChilds { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnAdditionalValueSetupList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnDeductionValueSetupList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> IncoTermsList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> PaymentTermsList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CountryOfOriginList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CalculationofTenureList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> ShipmentModeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> LCTenureList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> PortofLoadingList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> PortofDischargeList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> TypeOfLCList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> OfferValidityList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> AdditionalValueSetupList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> DeductionValueSetupList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CreditDaysList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnSubProgramList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YPIReceiveMasterID > 0;

        #endregion Additional
    }


    
}