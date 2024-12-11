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
    [Table(TableNames.YarnLCDocument)]
    public class YarnLcDocument : DapperBaseEntity
    {
        [ExplicitKey]
        public int LCDocID { get; set; }

        public int Lcid { get; set; }
        public int DocId { get; set; }

        #region Additional

        [Write(false)]
        public string CDTypeName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LCDocID > 0;

        #endregion Additional
    }
}
