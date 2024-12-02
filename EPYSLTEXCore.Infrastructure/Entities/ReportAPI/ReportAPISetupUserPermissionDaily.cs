using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    [Table("ReportAPISetupUserPermission")]
    public class ReportAPISetupUserPermissionDaily : DapperBaseEntity
    {
        [ExplicitKey]
        public int ReportID { get; set; }
        [ExplicitKey]
        public int UserCode { get; set; }
        [ExplicitKey]
        public DateTime UseDate { get; set; }
        public int LimitPerday { get; set; }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || UserCode == 0;
        #endregion Additional Properties
    }
}
