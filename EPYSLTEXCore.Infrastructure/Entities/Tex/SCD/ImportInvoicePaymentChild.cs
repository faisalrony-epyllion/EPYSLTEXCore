using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table("ImportInvoicePaymentChild")]
	public class ImportInvoicePaymentChild : DapperBaseEntity
    {
		#region Table properties

		[ExplicitKey]
		public int IIPChildID { get; set; }

		public int IIPMasterID { get; set; }

		public int InvoiceID { get; set; }

		public string BankRefNo { get; set; }

		public DateTime BankRefDate { get; set; }

		public decimal InvoiceValue { get; set; }

		public decimal PaymentValue { get; set; }

		public decimal CurConvRate { get; set; }

		#endregion Table properties

		#region Additional Columns

		[Write(false)]
		public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || IIPChildID > 0;

		[Write(false)]
		public string InvoiceNo { get; set; }
		[Write(false)]
		public DateTime InvoiceDate { get; set; }
		
		[Write(false)]
		public decimal PaymentedValue { get; set; }	
		
		[Write(false)]
		public decimal BalanceAmount { get; set; }

		#endregion Additional Columns

		public ImportInvoicePaymentChild()
		{
			BankRefDate = DateTime.Now;
			InvoiceValue = 0m;
			PaymentValue = 0m;
			CurConvRate = 0m;
			PaymentedValue = 0m;
			BalanceAmount = 0m;
		}

	}
}
