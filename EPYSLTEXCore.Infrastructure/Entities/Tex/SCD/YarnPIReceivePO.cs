using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnPIReceivePO)]
    public class YarnPIReceivePO : DapperBaseEntity
    {
        [ExplicitKey]
        public int YPIReceivePOID { get; set; }
        public int YPIReceiveMasterID { get; set; }
        public int YPOMasterID { get; set; }
        public int RevisionNo { get; set; }
        public int ItemMasterID { get; set; }
        public decimal PIQty { get; set; }

        public int YPOChildID { get; set; }

        #region Additional

        [Write(false)]
        public string PONo { get; set; }

        [Write(false)]
        public int YPIReceiveChildID { get; set; }
        [Write(false)]
        public decimal Rate { get; set; }
        [Write(false)]
        public decimal POQty { get; set; }
        [Write(false)]
        public decimal POValue { get; set; }
        [Write(false)]
        public decimal BalancePOQty { get; set; }
        [Write(false)]
        public int MasterRevisioNo { get; set; }

        
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YPIReceivePOID > 0;

        #endregion Additional
    }
}