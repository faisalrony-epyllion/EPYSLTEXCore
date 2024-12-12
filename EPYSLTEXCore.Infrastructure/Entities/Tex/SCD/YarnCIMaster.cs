using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YARN_CI_MASTER)]
    public class YarnCIMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int CIID { get; set; }
        public int LCID { get; set; }
        public string CINo { get; set; }
        public DateTime CIDate { get; set; }
        public decimal CIValue { get; set; }
        public string ExpNo { get; set; }
        public DateTime? ExpDate { get; set; }
        public string BOENo { get; set; }
        public DateTime? BOEDate { get; set; }
        public int ConsigneeId { get; set; }
        public int NotifyPartyId { get; set; }
        public int NotiryParty1Id { get; set; }
        public bool BOE { get; set; }
        public DateTime? DateAddedBoe { get; set; }
        public bool DC { get; set; }
        public System.DateTime? DateAddedDc { get; set; }
        public bool Pl { get; set; }
        public DateTime? DateAddedPl { get; set; }
        public bool BC { get; set; }
        public DateTime? DateAddedBc { get; set; }
        public bool CO { get; set; }
        public System.DateTime? DateAddedCo { get; set; }
        public bool Wml { get; set; }
        public DateTime? DateAddedWml { get; set; }
        public bool Psic { get; set; }
        public DateTime? DateAddedPsic { get; set; }
        public bool CusSubmit { get; set; }
        public int? CusSubmitTo { get; set; }
        public DateTime? CusSubmitDate { get; set; }
        public int? CusSubmitBy { get; set; }
        public DateTime? CusSubmitUpdateDate { get; set; }
        public bool CustReceipt { get; set; }
        public string CustReceiptNo { get; set; }
        public DateTime? CustReceiptDate { get; set; }
        public int? CustReceiptBy { get; set; }
        public DateTime? CustReceiptUpdateDate { get; set; }
        public bool CustAccept { get; set; }
        public DateTime? CustAcceptDate { get; set; }
        public int? CustAcceptBy { get; set; }
        public DateTime? CustAcceptUpdateDate { get; set; }
        public bool CommSubmit { get; set; }
        public int? CommSubmitTo { get; set; }
        public DateTime? CommSubmitDate { get; set; }
        public int? CommSubmitBy { get; set; }
        public DateTime? CommSubmitUpdateDate { get; set; }
        public bool CommAcknowledge { get; set; }
        public DateTime? CommAckDate { get; set; }
        public int? CommAckBy { get; set; }
        public DateTime? CommAckUpdateDate { get; set; }
        public bool BankSubmit { get; set; }
        public string BankRefNumber { get; set; }
        public DateTime? BankSubmitDate { get; set; }
        public int? BankSubmitBy { get; set; }
        public DateTime? BankSubmitUpdateDate { get; set; }
        public bool BankAccept { get; set; }
        public string BankAcceptNumber { get; set; }
        public DateTime? BankAcceptDate { get; set; }
        public int? BankAcceptBy { get; set; }
        public DateTime? BankAcceptUpdateDate { get; set; }
        public bool BankAcknowledge { get; set; }
        public DateTime? BankAckDate { get; set; }
        public int? BankAckBy { get; set; }
        public DateTime? BankAckUpdateDate { get; set; }
        public bool Acceptance { get; set; }
        public int? AcceptanceBy { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public bool Reject { get; set; }
        public int? RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public bool IsCDA { get; set; }
        public bool SG { get; set; }
        public bool IsComplete { get; set; }
        public int CompanyId { get; set; }
        public int SupplierId { get; set; }
        public string BLNo { get; set; }
        public string ContainerStatus { get; set; }
        public DateTime? BLDate { get; set; }
        public string CIFilePath { get; set; }
        public string AttachmentPreviewTemplate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        #region Additional Columns

        [Write(false)]
        public List<YarnCIChild> CIChilds { get; set; }

        [Write(false)]
        public List<YarnCIChildPI> CIChildPIs { get; set; }

        [Write(false)]
        public List<Select2OptionModel> CustomerList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> IncoTermsList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> PaymentTermsList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> LCTenureList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> CurrencyTypeList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> CompanyList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> LCTypeList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> YarnUnitList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> LCIssuingBankList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> LienBankList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> PaymentBankList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> BankAcceptanceFromList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> MaturityCalculationList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> CIDeclarationList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> BIDeclarationList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> CommercialAttachmentList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> ConsigneeList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> NotifyPartyList { get; set; }
        [Write(false)]
        public List<YarnCIDoc> AllDocTypes { get; set; } //Use for only interface
        [Write(false)]
        public List<YarnCIDoc> YarnCIDocs { get; set; } //Use for DB records

        [Write(false)]
        public string LcNo { get; set; }

        [Write(false)]
        public DateTime LcDate { get; set; }

        [Write(false)]
        public DateTime LcExpiryDate { get; set; }

        [Write(false)]
        public int RevisionNo { get; set; }

        [Write(false)]
        public int CustomerId { get; set; }

        [Write(false)]
        public int RecCustomerId { get; set; }

        [Write(false)]
        public int CurrencyId { get; set; }

        [Write(false)]
        public decimal LcValue { get; set; }

        [Write(false)]
        public decimal LcQty { get; set; }

        [Write(false)]
        public int LcUnit { get; set; }

        [Write(false)]
        public DateTime LcReceiveDate { get; set; }

        [Write(false)]
        public int IssueBankId { get; set; }

        [Write(false)]
        public int PaymentBankId { get; set; }

        [Write(false)]
        public int LienBankId { get; set; }

        [Write(false)]
        public bool NotifyPaymentBank { get; set; }

        [Write(false)]
        public string BbReportingNumber { get; set; }

        [Write(false)]
        public int PaymentModeId { get; set; }

        [Write(false)]
        public string HsCode { get; set; }

        [Write(false)]
        public int CalculationOfTenor { get; set; }

        [Write(false)]
        public DateTime DocPresentationDate { get; set; }

        [Write(false)]
        public int BankAcceptanceFrom { get; set; }

        [Write(false)]
        public int MaturityCalculationId { get; set; }

        [Write(false)]
        public decimal Tolerance { get; set; }

        [Write(false)]
        public string LcDateStr { get; set; }

        [Write(false)]
        public string LcReceiveDateStr { get; set; }

        [Write(false)]
        public string DocPresentationDateStr { get; set; }

        [Write(false)]
        public string TenorOfCalculation { get; set; }

        [Write(false)]
        public string PaymentMethodName { get; set; }

        [Write(false)]
        public string CustomerName { get; set; }

        [Write(false)]
        public string IssueBank { get; set; }

        [Write(false)]
        public string PaymentBank { get; set; }

        [Write(false)]
        public string CurrencyCode { get; set; }

        [Write(false)]
        public string LCStatus { get; set; }

        [Write(false)]
        public string LcFilePath { get; set; }

        //[Write(false)]
        //public string AttachmentPreviewTemplate { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || CIID > 0;
        [Write(false)]
        public string SupplierName { get; set; }
        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public decimal MaxLCValue { get; set; }
        [Write(false)]
        public decimal MaxTolerance { get; set; }
        [Write(false)]
        public decimal TotalCI { get; set; }

        [Write(false)]
        public List<YarnCIMaster> YarnCIList { get; set; } //Use for DB records

        [Write(false)]
        public List<ImportDocumentAcceptanceChargeDetails> IDACDetails { get; set; }

        [Write(false)]
        public List<Select2OptionModel> HeadDescriptionList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> CalculationOnNameList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SHeadNameList { get; set; }
        [Write(false)]
        public string LCNo { get; set; }
        [Write(false)]
        public DateTime LCDate { get; set; }
        [Write(false)]
        public decimal LCValue { get; set; }

        #endregion Additional Columns
        public YarnCIMaster()
        {
            IDACDetails = new List<ImportDocumentAcceptanceChargeDetails>();
            AddedBy = 0;
            DateAdded = DateTime.Now;
            CIValue = 0m;
            NotiryParty1Id = 0;
            BOE = false;
            DC = false;
            Pl = false;
            BC = false;
            CO = false;
            Wml = false;
            Psic = false;
            CusSubmit = false;
            CustReceipt = false;
            CustAccept = false;
            CommSubmit = false;
            CommAcknowledge = false;
            BankSubmit = false;
            BankAccept = false;
            BankAcknowledge = false;
            Acceptance = false;
            CIDate = DateTime.Now;
            ExpDate = DateTime.Now;
            BOEDate = DateTime.Now;
            CIChilds = new List<YarnCIChild>();
            CIChildPIs = new List<YarnCIChildPI>();
            CustomerList = new List<Select2OptionModel>();
            IncoTermsList = new List<Select2OptionModel>();
            PaymentTermsList = new List<Select2OptionModel>();
            LCTenureList = new List<Select2OptionModel>();
            CurrencyTypeList = new List<Select2OptionModel>();
            CompanyList = new List<Select2OptionModel>();
            LCTypeList = new List<Select2OptionModel>();
            YarnUnitList = new List<Select2OptionModel>();
            LCIssuingBankList = new List<Select2OptionModel>();
            LienBankList = new List<Select2OptionModel>();
            PaymentBankList = new List<Select2OptionModel>();
            BankAcceptanceFromList = new List<Select2OptionModel>();
            MaturityCalculationList = new List<Select2OptionModel>();
            CIDeclarationList = new List<Select2OptionModel>();
            BIDeclarationList = new List<Select2OptionModel>();
            CommercialAttachmentList = new List<Select2OptionModel>();
            ConsigneeList = new List<Select2OptionModel>();
            NotifyPartyList = new List<Select2OptionModel>();
            AllDocTypes = new List<YarnCIDoc>();
            YarnCIDocs = new List<YarnCIDoc>();
        }
    }
}
