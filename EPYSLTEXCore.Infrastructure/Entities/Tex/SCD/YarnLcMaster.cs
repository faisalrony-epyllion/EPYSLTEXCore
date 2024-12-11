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
    [Table(TableNames.YarnLCMaster)]
    public class YarnLcMaster : DapperBaseEntity
    {
        public YarnLcMaster()
        {
            DocPresentationDays = 0;
            LCReceiveDate = DateTime.Now;
            LCExpiryDate = DateTime.Now;
            MaturityCalculationID = 0;
            //LcDate = DateTime.Now;
            //LCNo = AppConstants.NEW;
            MaturityCalculationID = 0;
            ProposalID = 0;
            PreRevisionNo = 0;
            RevisionNo = 0;
            LCValue = 0m;
            IssueBankID = 0;
            PaymentBankID = 0;
            LienBankID = 0;
            CalculationOfTenorID = 0;
            BankAcceptanceFrom = 0;
            Tolerance = 0m;
            CIDecID = 0;
            BCDecID = 0;
            IsContract = false;
            IsCDA = false;
            Proposed = true;
            Approve = false;
            NeedAmendent = false;
            IsConInsWith = false;
            NotifyPaymentBank = false;
            ApproveBy = 0;
            TTTypeID = 0;
            AvailableWithID = 0;
            TenureofLCID = 0;
            PartialShipmentID = 0;
            TransshipmentID = 0;
            LoadingPortID = 0;
            DischargePortID = 0;
            CashStatus = false;
            DateAdded = DateTime.Now;
            LcChilds = new List<YarnLcChild>();
            LcDocuments = new List<YarnLcDocument>();
        }

        [ExplicitKey]
        public int LCID { get; set; }
        public string LCNo { get; set; }
        public DateTime? LCDate { get; set; }
        public DateTime? LCExpiryDate { get; set; }
        public int ProposalID { get; set; }
        public int PreRevisionNo { get; set; }
        public int RevisionNo { get; set; }
        public int CompanyID { get; set; }
        public int SupplierID { get; set; }
        public int CurrencyID { get; set; }
        public decimal LCValue { get; set; }
        public decimal LCQty { get; set; }
        public int LCUnit { get; set; }
        public DateTime? LCReceiveDate { get; set; }
        public int IssueBankID { get; set; }
        public int PaymentBankID { get; set; }
        public int LienBankID { get; set; }
        public bool NotifyPaymentBank { get; set; }
        public string BBReportingNumber { get; set; }
        public int PaymentModeID { get; set; }
        public int DocPresentationDays { get; set; }
        public int BankAcceptanceFrom { get; set; }
        public int MaturityCalculationID { get; set; }
        public decimal Tolerance { get; set; }
        public int IncoTermsID { get; set; }
        public int CIDecID { get; set; }
        public int BCDecID { get; set; }
        public string HSCode { get; set; }
        public bool Proposed { get; set; }
        public DateTime? ProposedDate { get; set; }
        public bool Approve { get; set; }
        public int ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int AddedBy { get; set; }
        public int UpdatedBy { get; set; }
        public string LCFilePath { get; set; }
        public string AttachmentPreviewTemplate { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsCDA { get; set; }
        public bool IsContract { get; set; }
        public int CalculationOfTenorID { get; set; }
        public string BankRefNo { get; set; }
        public string AccountNo { get; set; }
        public string FormOfDC { get; set; }
        public int TTTypeID { get; set; }
        public int AvailableWithID { get; set; }
        public int TenureofLCID { get; set; }
        public int PartialShipmentID { get; set; }
        public int TransshipmentID { get; set; }
        public int LoadingPortID { get; set; }
        public int DischargePortID { get; set; }
        public bool IsConInsWith { get; set; }
        public bool NeedAmendent { get; set; }
        public bool CashStatus { get; set; }


        #region Additional

        [Write(false)]
        public List<YarnLcChild> LcChilds { get; set; }

        [Write(false)]
        public List<YarnLcDocument> LcDocuments { get; set; }

        [Write(false)]
        public int TExportLCID { get; set; }

        [Write(false)]
        public string TExportLCNo { get; set; }
        [Write(false)]
        public string ProposeContractNo { get; set; }

        [Write(false)]
        public string RetirementMode { get; set; }

        [Write(false)]
        public int YarnPIRevision { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> CustomerList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> RecCustomerList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> IncoTermsList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> PaymentTermsList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> CalculationOfTenorList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> CurrencyList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> CompanyList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> PaymentModeList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> LCUnitList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> IssueBankList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> LienBankList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> PaymentBankList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> BankAcceptanceFromList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> MaturityCalculationList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> CIDecList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> BCDecList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> CommercialAttachmentList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> ConsigneeList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> NotifyPartyList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> TTTypeList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> AvailableWithList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> TenureofLCList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> PartialShipmentList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> TransshipmentList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> LoadingPortList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> DischargePortList { get; set; }
        [Write(false)]
        public IList<Select2OptionModel> SupplierList { get; set; }

        [Write(false)]
        public string ProposalNo { get; set; }

        [Write(false)]
        public int BBLCRevisionNo { get; set; }

        [Write(false)]
        public DateTime ProposalDate { get; set; }

        [Write(false)]
        public string PiNo { get; set; }

        [Write(false)]
        public string SupplierName { get; set; }

        [Write(false)]
        public decimal TotalQty { get; set; }

        [Write(false)]
        public decimal TotalValue { get; set; }

        [Write(false)]
        public string CustomerName { get; set; }

        [Write(false)]
        public string CurrencyCode { get; set; }

        [Write(false)]
        public string IssueBank { get; set; }
        [Write(false)]
        public string PIStatus { get; set; }


        [Write(false)]
        public string PaymentBank { get; set; }

        [Write(false)]
        public string LCStatus { get; set; }

        [Write(false)]
        public string TenorOfCalculation { get; set; }

        [Write(false)]
        public string PaymentMethodName { get; set; }

        [Write(false)]
        public string Expression { get; set; }
        [Write(false)]
        public string CompanyName { get; set; }
        [Write(false)]
        public bool isAmendentValue { get; set; }

        [Write(false)]
        public bool isRevision { get; set; }

        [Write(false)]
        public int ProposeBankID { get; set; }
        [Write(false)]
        public string BranchName { get; set; }

        [Write(false)]

        public string BBLCStatus { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LCID > 0;

        #endregion Additional
    }
}
