using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_DYEING_BATCH_WITH_BATCH_MASTER)]
    public class YDDyeingBatchWithBatchMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDDBBMID { get; set; }

        public int YDDBatchID { get; set; }

        public int YDBatchID { get; set; }

        public decimal BatchUseQtyKG { get; set; }

        public int BatchUseQtyPcs { get; set; }

        #region Addtional

        [Write(false)]
        public string YDBatchNo { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDDBBMID > 0;

        #endregion Addtional
    }
}
