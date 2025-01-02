using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.BATCH_CHILD)]
    public class BatchChild : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int BChildID { get; set; }

        public int BItemReqID { get; set; }

        public int BatchID { get; set; }

        public int GRollID { get; set; }

        public decimal RollQty { get; set; }

        public int RollQtyPcs { get; set; }

        public int ItemMasterID { get; set; }

        #endregion Table Properties

        #region Additional Propeties

        [Write(false)]
        public string RollNo { get; set; }

        [Write(false)]
        public string ShiftName { get; set; }

        [Write(false)]
        public string OperatorName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || BChildID > 0;

        #endregion Additional Propeties
        public BatchChild()
        {
            BatchID = 0;
        }
    }

}