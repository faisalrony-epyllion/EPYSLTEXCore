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
    [Table(TableNames.YarnLCChild)]
    public class YarnLcChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int ChildID { get; set; }

        public int LCID { get; set; }
        public int YPIReceiveMasterID { get; set; }
        public int RevisionNo { get; set; }

        #region Additional

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ChildID > 0;

        [Write(false)]
        public int SupplierID { get; set; }

        [Write(false)]
        public string SupplierName { get; set; }

        [Write(false)]
        public string PINo { get; set; }

        [Write(false)]
        public DateTime PIDate { get; set; }

        [Write(false)]
        public decimal TotalQty { get; set; }

        [Write(false)]
        public decimal TotalValue { get; set; }
        [Write(false)]
        public string PIFilePath { get; set; }

        [Write(false)]
        public string BBLCStatus { get; set; }

        #endregion Additional
    }
}
