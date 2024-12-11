using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YARN_CI_CHILD_YARN_SUBPROGRAM)]
    public class YarnCIChildYarnSubProgram : DapperBaseEntity
    {
        [ExplicitKey]
        public int YarnCIChildSubProgramID { get; set; }

        ///<summary>
        /// CIChildID
        ///</summary>
        public int ChildID { get; set; }

        ///<summary>
        /// CIID
        ///</summary>
        public int CIID { get; set; }

        ///<summary>
        /// SubProgramID
        ///</summary>
        public int SubProgramID { get; set; }

        #region AdditionalFields

        [Write(false)]
        public string SubProgramName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YarnCIChildSubProgramID > 0;

        #endregion AdditionalFields
    }
}
