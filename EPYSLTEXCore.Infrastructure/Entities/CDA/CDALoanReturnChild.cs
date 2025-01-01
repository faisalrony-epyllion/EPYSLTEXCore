using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDALoanReturnChild")]
	public class CDALoanReturnChild : YarnItemMaster, IDapperBaseEntity
	{
		#region Table properties

		[ExplicitKey]
		public int CDALReturnChildID { get; set; }

		public int CDALRetuenMasterID { get; set; } 

		public string BatchNo { get; set; }

		public DateTime ExpiryDate { get; set; } 

		public decimal ReturnQty { get; set; }

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
		public List<CDALoanReturnChildAdjutment> ChildAdjutment { get; set; }
		#endregion Additional Columns

		public CDALoanReturnChild()
		{  
			ReturnQty = 0;
			Rate = 0;
			EntityState = EntityState.Added;
		} 
	}

}
