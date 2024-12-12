using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table("ImportInvoicePaymentDetails")]
	public class ImportInvoicePaymentDetails : DapperBaseEntity
    {
		#region Table properties

		[ExplicitKey]
		public int IIPDetailsID { get; set; }

		public int IIPMasterID { get; set; }

		public int SGHeadID { get; set; }

		public int DHeadID { get; set; }

		public int SHeadID { get; set; }

		public int CalculationOn { get; set; }

		public decimal ValueInFC { get; set; }

		public decimal ValueInLC { get; set; }

		public decimal CurConvRate { get; set; }

		#endregion Table properties

		#region Additional Columns

		[Write(false)]
		public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || IIPDetailsID > 0;

		[Write(false)] 
		public int CTCategoryID { get; set; }
		[Write(false)] 
		public bool DHeadNeed { get; set; }
		[Write(false)] 
		public bool SHeadNeed { get; set; }
		[Write(false)] 
		public string SGHeadName { get; set; }

		[Write(false)]
		public List<ImportInvoicePaymentDetails> IPDetailSub { get; set; }

		#endregion Additional Columns

		public ImportInvoicePaymentDetails()
		{
			IPDetailSub = new List<ImportInvoicePaymentDetails>();
			CurConvRate = 0m;

		}

	}
}
