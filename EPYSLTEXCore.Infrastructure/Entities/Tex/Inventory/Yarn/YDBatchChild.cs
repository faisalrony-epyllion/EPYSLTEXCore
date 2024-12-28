using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_BATCH_CHILD)]
    public class YDBatchChild : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int YDBatchChildID { get; set; }

        public int BItemReqID { get; set; } = 0;

        public int YDBatchID { get; set; } = 0;

        public int GRollID { get; set; } = 0;

        public decimal RollQty { get; set; } = 0;

        public int RollQtyPcs { get; set; } = 0;

        public int ItemMasterID { get; set; } = 0;

        #endregion Table Properties

        #region Additional Propeties

        [Write(false)]
        public string RollNo { get; set; }

        [Write(false)]
        public string ShiftName { get; set; }

        [Write(false)]
        public string OperatorName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDBatchChildID > 0;

        #endregion Additional Propeties
        public YDBatchChild()
        {
            YDBatchID = 0;
        }
    }
}
