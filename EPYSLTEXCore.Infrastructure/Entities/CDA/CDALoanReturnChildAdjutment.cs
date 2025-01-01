using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDALoanReturnChildAdjutment")]
	public class CDALoanReturnChildAdjutment : YarnItemMaster, IDapperBaseEntity
	{
		#region Table properties

		[ExplicitKey]
		public int CDALReturnAdjID { get; set; }
		public int CDALReturnChildID { get; set; }
		public int CDALRetuenMasterID { get; set; }
		public int CDALReceiveChildID { get; set; } 
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

		public CDALoanReturnChildAdjutment()
		{
			CDALReturnChildID = 0;
			CDALRetuenMasterID = 0;
			CDALReceiveChildID = 0; 
			AdjustQty = 0;
			Rate = 0;
			EntityState = EntityState.Added;
		} 
	}
}
