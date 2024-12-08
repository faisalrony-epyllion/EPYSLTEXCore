using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex
{
    [Table(TableNames.YARN_RECEIVE_CHILD_SUB_PROGRAM)]
    public class YarnReceiveChildSubProgram : DapperBaseEntity
    {
        [ExplicitKey]
        public int ReceiveChildSubProgramID { get; set; }

        /// <summary>
        /// ReceiveID
        /// </summary>
        public int ReceiveID { get; set; }

        ///<summary>
        /// ChildID
        ///</summary>
        public int ChildID { get; set; }

        ///<summary>
        /// SubProgramID
        ///</summary>
        public int SubProgramID { get; set; }

        #region AdditionalFields

        [Write(false)]
        public string SubProgramName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ReceiveChildSubProgramID > 0;

        #endregion AdditionalFields
    }
}
