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
    [Table(TableNames.YD_DYEING_BATCH_MERGE_BATCH)]
    public class YDDyeingBatchMergeBatch : DapperBaseEntity
    {
        public YDDyeingBatchMergeBatch()
        {
            YDDBMID = 0;
            YDDBatchID = 0;
            MergeDBatchID = 0;
            BatchQty = 0;
        }
        [ExplicitKey]
        public int YDDBMID { get; set; }
        public int YDDBatchID { get; set; }
        public int MergeDBatchID { get; set; }
        public decimal BatchQty { get; set; }

        #region Additional Property
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDDBMID > 0;
        #endregion
    }
}
