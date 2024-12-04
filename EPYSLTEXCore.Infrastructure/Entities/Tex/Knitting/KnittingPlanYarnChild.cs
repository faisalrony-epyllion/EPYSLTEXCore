using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.Knitting_Plan_Yarn_Child)]
    public class KnittingPlanYarnChild : DapperBaseEntity
    {
        public KnittingPlanYarnChild()
        {
            KPYarnChildID = 0;
            KPYarnID = 0;
            FCMRChildID = 0;
            ConceptID = 0;
            ItemMasterID = 0;
            ReqQty = 0;
        }
        [ExplicitKey]
        public int KPYarnChildID { get; set; }
        public int KPYarnID { get; set; }
        public int FCMRChildID { get; set; }
        public int ConceptID { get; set; }
        public int ItemMasterID { get; set; }
        public decimal ReqQty { get; set; }

        #region Addtional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || KPYarnChildID > 0;
        #endregion
    }
}
