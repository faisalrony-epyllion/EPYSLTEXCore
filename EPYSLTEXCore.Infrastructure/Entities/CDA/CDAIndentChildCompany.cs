using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAIndentChildCompany")]
    public class CDAIndentChildCompany : DapperBaseEntity
    {
        [ExplicitKey]
        public int CDAIndentChildCompanyID { get; set; }

        public int CDAIndentChildID { get; set; }

        public int CDAIndentMasterID { get; set; }

        public int CompanyID { get; set; }

        public int ItemMasterID { get; set; }

        public int UnitID { get; set; }

        public decimal IndentQty { get; set; }

        public decimal ReqQty { get; set; }

        #region Additional Columns

        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.CDAIndentChildCompanyID > 0;

        #endregion Additional Columns

    }
}