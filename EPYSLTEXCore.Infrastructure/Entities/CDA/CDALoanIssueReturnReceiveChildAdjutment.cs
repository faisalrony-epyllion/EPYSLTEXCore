using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDALoanIssueReturnReceiveChildAdjutment")]
	public class CDALoanIssueReturnReceiveChildAdjutment : YarnItemMaster, IDapperBaseEntity
	{
		#region Table properties

		[ExplicitKey]
		public int CDALIRRAdjID { get; set; }

		public int CDALIssueReturnChildID { get; set; }

		public int CDALIssueReturnID { get; set; }

		public int CDALIssueChildID { get; set; } 

		public string BatchNo { get; set; }

		public DateTime ExpiryDate { get; set; } 

		public decimal AdjustQty { get; set; }

		public decimal Rate { get; set; }

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
		public decimal ReceiveQty { get; set; }
		[Write(false)]
		public decimal TotalValue { get; set; }
		#endregion Additional Columns

		public CDALoanIssueReturnReceiveChildAdjutment()
		{
			//CDALIssueReturnChildID = 0;
            //CDALIssueReturnID = 0;
            CDALIssueChildID = 0;
            ItemMasterID = 0;
			UnitID = 0;
			AdjustQty = 0;
			Rate = 0;
            EntityState = EntityState.Added;
        }

	}
}
