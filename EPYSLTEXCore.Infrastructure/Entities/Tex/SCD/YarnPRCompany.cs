using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YARN_PR_COMPANY)]
    public class YarnPRCompany : DapperBaseEntity
    {
        [ExplicitKey]
        public int YarnPRCompanyID { get; set; }

        public int YarnPRChildID { get; set; }

        public int YarnPRMasterID { get; set; }

        public int CompanyID { get; set; }

        /// <summary>
        /// Identifies if commercial PR companies or PR companies.
        /// If 'true' commercial PR if 'false' PR
        /// </summary>
        public bool IsCPR { get; set; }

        #region Additional

        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YarnPRCompanyID > 0;

        #endregion Additional
    }
}
