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
    [Table(TableNames.YARN_CI_CHILD_PI)]
    public class YarnCIChildPI : DapperBaseEntity
    {
        [ExplicitKey]
        public int ChildPIID { get; set; }

        ///<summary>
        /// CIID
        ///</summary>
        public int CIID { get; set; }

        ///<summary>
        /// YPIMasterID
        ///</summary>
        public int YPIMasterID { get; set; }

        #region Additional Columns

        [Write(false)]
        public int Lcid { get; set; }

        [Write(false)]
        public int SupplierID { get; set; }

        [Write(false)]
        public string SupplierName { get; set; }

        [Write(false)]
        public string PiNo { get; set; }

        [Write(false)]
        public DateTime PIDate { get; set; }

        [Write(false)]
        public decimal TotalQty { get; set; }

        [Write(false)]
        public decimal TotalValue { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ChildPIID > 0;

        #endregion Additional Columns
    }
}
