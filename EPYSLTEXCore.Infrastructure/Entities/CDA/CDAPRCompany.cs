using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAPRCompany")]
	public class CDAPRCompany : DapperBaseEntity
	{
		[ExplicitKey]
		public int CDAPRCompanyID { get; set; }
		public int CompanyID { get; set; }
		public int CDAPRChildID { get; set; } 
		public int CDAPRMasterID { get; set; } 
		/// <summary>
		/// Identifies if commercial PR companies or PR companies.
		/// If 'true' commercial PR if 'false' PR  
		/// </summary>
		public bool IsCPR { get; set; } 
		//public virtual CDAPRChild CDAPRChild { get; set; }

		#region Additional Columns 
		[Write(false)]
		public string CompanyName { get; set; }
		[Write(false)]
		public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.CDAPRCompanyID > 0;

		#endregion Additional Columns 
	}
}
