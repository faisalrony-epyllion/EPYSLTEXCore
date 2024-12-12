using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table("ImportInvoicePaymentMaster")]
    public class ImportInvoicePaymentMaster : DapperBaseEntity
    {
        #region Table properties

        [ExplicitKey]
        public int IIPMasterID { get; set; }

        public string IIPMasterNo { get; set; }

        public DateTime PaymentDate { get; set; }

        public int CompanyID { get; set; }

        public int BankBranchID { get; set; }

        public int? CurrencyID { get; set; }

        public decimal CurConvRate { get; set; }

        public decimal TotalInvoiceValue { get; set; }

        public decimal TotalPaymentValue { get; set; }

        public int BeneficiaryID { get; set; }

        public bool IsCDA { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        #endregion Table properties

        #region Additional Columns

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || IIPMasterID > 0;

        [Write(false)]
        public string SupplierName { get; set; }
        [Write(false)]
        public string CustomerName { get; set; }

        [Write(false)]
        public int SupplierID { get; set; }

        [Write(false)]
        public string BankRefNumber { get; set; }
        
        [Write(false)]
        public string PaymentBank { get; set; }

        [Write(false)]
        public decimal CIValue { get; set; }

        [Write(false)]
        public DateTime BankAcceptDate { get; set; }
        [Write(false)]
        public DateTime MaturityDate { get; set; }
        
        [Write(false)]
        public decimal TotalAcceptedValue { get; set; }        
        [Write(false)]
        public decimal TotalPaymentedValue { get; set; }
        [Write(false)]
        public decimal TotalPaidValue { get; set; }

        [Write(false)]
        public List<ImportInvoicePaymentChild> IPChilds { get; set; }

        [Write(false)]
        public List<ImportInvoicePaymentDetails> IPDetails { get; set; }

        [Write(false)]
        public List<Select2OptionModel> CompanyList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> HeadDescriptionList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> CalculationOnNameList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SHeadNameList { get; set; }
        [Write(false)]
        public bool Modify { get; set; }


        #endregion Additional Columns

        public ImportInvoicePaymentMaster()
        {
            PaymentDate = DateTime.Now;
            CurrencyID = 0;
            CurConvRate = 0;
            TotalInvoiceValue = 0m;
            TotalPaymentValue = 0m;
            BeneficiaryID = 0;
            TotalAcceptedValue = 0m;
            TotalPaymentedValue = 0m;
            TotalPaidValue = 0m;
            IsCDA = false;
            DateAdded = DateTime.Now;
            DateUpdated = DateTime.Now;

        }

    }
}
