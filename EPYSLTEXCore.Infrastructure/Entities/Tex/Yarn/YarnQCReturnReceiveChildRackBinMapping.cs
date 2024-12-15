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
    [Table(TableNames.YARN_QC_RETURN_RECEIVE_CHILD_RACK_BIN_MAPPING)]
    public class YarnQCReturnReceiveChildRackBinMapping : DapperBaseEntity
    {
        public YarnQCReturnReceiveChildRackBinMapping()
        {
            YQCRRId = 0;
            QCReturnReceivedChildID = 0;
            ChildRackBinID = 0;
            ReceiveQtyCone = 0;
            ReceiveCartoon = 0;
            ReceiveQtyKg = 0;
        }
        [ExplicitKey]
        public int YQCRRId { get; set; }
        public int QCReturnReceivedChildID { get; set; }
        public int ChildRackBinID { get; set; }
        public int ReceiveQtyCone { get; set; }
        public int ReceiveCartoon { get; set; }
        public decimal ReceiveQtyKg { get; set; }

        #region Additional properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YQCRRId > 0;
        #endregion
    }
}
