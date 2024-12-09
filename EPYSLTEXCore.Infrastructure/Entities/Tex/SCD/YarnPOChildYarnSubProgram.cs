using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnPOChildYarnSubProgram)]
    public class YarnPOChildYarnSubProgram : DapperBaseEntity
    {
        [ExplicitKey]
        public int YPOChildYSPID { get; set; } = 0;

        public int YPOChildID { get; set; } = 0;
        public int YPOMasterID { get; set; } = 0;
        public int YarnSubProgramId { get; set; } = 0;

        #region Addtional Properties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YPOChildYSPID > 0;

        #endregion Addtional Properties
    }
}
