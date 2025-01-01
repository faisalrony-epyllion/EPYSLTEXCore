using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAIndentChildDetails")]  
	public class CDAIndentChildDetails : DapperBaseEntity
	{
		public CDAIndentChildDetails()
		{
			IndentQty = 0;
			CheckQty = 0;
			ApprovQty = 0;
			DetailsQTY = 0;
		}
		[ExplicitKey]
		public int CDAIndentChildDetailsID { get; set; }
		public int CDAIndentChildID { get; set; }
		public int CDAIndentMasterID { get; set; }
		public DateTime BookingDate { get; set; }
		public decimal IndentQty { get; set; }
		public decimal CheckQty { get; set; }
		public decimal ApprovQty { get; set; }
		public decimal DetailsQTY { get; set; } 

		#region Additional Columns 

		[Write(false)]
		public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.CDAIndentChildDetailsID > 0;

		#endregion Additional Columns 
	}
}
