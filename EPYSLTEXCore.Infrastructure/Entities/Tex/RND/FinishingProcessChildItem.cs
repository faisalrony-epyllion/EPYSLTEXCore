using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
  
    [Table(TableNames.FINISHING_PROCESS_CHILD_ITEM)]
    public class FinishingProcessChildItem : DapperBaseEntity
    {
        [ExplicitKey] 
        public int FPChildItemID { get; set; }
        public int FPChildID { get; set; }
        public int FPMasterID { get; set; }
        public int SegmentNo { get; set; }
        public int ItemMasterID { get; set; }
        public decimal Qty { get; set; }
        public decimal ActualQty { get; set; }
        public bool IsPreProcess { get; set; }
        #region Additional
        [Write(false)]
        public string text { get; set; }
        [Write(false)]
        public string ItemName { get; set; }


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FPChildItemID > 0;

        #endregion Additional
    }
}
