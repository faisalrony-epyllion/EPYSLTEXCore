using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.KY_Req_Child_SubProgram)]
    public class KYReqChildSubProgram : DapperBaseEntity
    {
        [ExplicitKey]
        public int KYReqChildSubProgramID { get; set; }

        ///<summary>
        /// KYReqChildID
        ///</summary>
        public int KYReqChildID { get; set; }
        ///<summary>
        /// KYReqMasterID
        ///</summary>
        public int KYReqMasterID { get; set; }
        ///<summary>
        /// SubProgramID
        ///</summary>
        public int SubProgramID { get; set; }


        #region AdditionalFields

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || KYReqChildSubProgramID > 0;
        [Write(false)]
        public string SubProgramName { get; set; }

        #endregion
    }
}
