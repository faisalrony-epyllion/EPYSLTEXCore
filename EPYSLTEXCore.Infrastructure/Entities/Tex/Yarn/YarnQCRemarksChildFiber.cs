using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table(TableNames.YARN_QC_REMARKS_CHILDFIBER)]
    public class YarnQCRemarksChildFiber : DapperBaseEntity
    {
        [ExplicitKey]

        public int QCRemarksChildFiberID { get; set; }
        public int QCRemarksChildID { get; set; }
        public int ComponentID { get; set; }
        public decimal PercentageValue { get; set; }

        #region Additional Columns
        [Write(false)]
        public string ComponentName { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || QCRemarksChildFiberID > 0;

        #endregion Additional Columns


    }
}
