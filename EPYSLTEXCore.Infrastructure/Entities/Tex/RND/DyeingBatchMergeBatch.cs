using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Statics;
using System;
using System.Collections.Generic;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("DyeingBatchMergeBatch")]
    public class DyeingBatchMergeBatch : DapperBaseEntity
    {
        public DyeingBatchMergeBatch()
        {
            DBMID = 0;
            DBatchID = 0;
            MergeDBatchID = 0;
            BatchQty = 0;
        }
        [ExplicitKey]
        public int DBMID { get; set; }
        public int DBatchID { get; set; }
        public int MergeDBatchID { get; set; }
        public decimal BatchQty { get; set; }

        #region Additional Property
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || DBMID > 0;
        #endregion
    }
}
