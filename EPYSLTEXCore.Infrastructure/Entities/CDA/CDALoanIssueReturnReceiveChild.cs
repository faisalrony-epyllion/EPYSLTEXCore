using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDALoanIssueReturnReceiveChild")]
	public class CDALoanIssueReturnReceiveChild : YarnItemMaster, IDapperBaseEntity
	{
		#region Table properties

		[ExplicitKey]
		public int CDALIssueReturnChildID { get; set; }

		public int CDALIssueReturnID { get; set; } 

		public string BatchNo { get; set; }

		public DateTime ExpiryDate { get; set; } 

		public decimal ChallanQty { get; set; }

		public decimal ReceiveQty { get; set; }

		public decimal Rate { get; set; }

		public string Remarks { get; set; }

		#endregion Table properties

		#region Additional Columns

		[Write(false)]
		public EntityState EntityState { get; set; }
		[Write(false)]
		public int TotalRows { get; set; }
		[Write(false)]
		public bool IsModified => EntityState == EntityState.Modified;
		[Write(false)]
		public bool IsNew => EntityState == EntityState.Added;
		[Write(false)]
		public string DisplayUnitDesc { get; set; }
		[Write(false)]
		public List<CDALoanIssueReturnReceiveChildAdjutment> ChildAdjutment { get; set; }
		#endregion Additional Columns

		public CDALoanIssueReturnReceiveChild()
		{
			ChallanQty = 0;
			ReceiveQty = 0;
			Rate = 0;
			EntityState = EntityState.Added;
		} 
	}
	
}
