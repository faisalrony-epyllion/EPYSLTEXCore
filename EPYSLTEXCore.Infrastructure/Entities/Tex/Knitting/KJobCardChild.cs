using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.KNITTING_JOB_CARD_Child)]
    public class KJobCardChild : DapperBaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int KJobCardChildID { get; set; }
        public int KJobCardMasterID { get; set; }
        public int KPChildID { get; set; }
        public decimal ProdQty { get; set; }
        public int ProdQtyPcs { get; set; }
        #endregion

        #region Additonal Fields
        [Write(false)]
        public string MachineType { get; set; }
        [Write(false)]
        public string MCSubClassName { get; set; }
        [Write(false)]
        public int MCSubClassID { get; set; }
        [Write(false)]
        public string Composition { get; set; }
        [Write(false)]
        public string Size { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string GSM { get; set; }
        [Write(false)]
        public decimal MaxQty { get; set; }
        [Write(false)]
        public decimal MaxQtyKg { get; set; }
        [Write(false)]
        public decimal MaxQtyPcs { get; set; }
        [Write(false)]
        public decimal BookingQty { get; set; }
        [Write(false)]
        public override bool IsModified => KJobCardChildID > 0 || EntityState == System.Data.Entity.EntityState.Modified;
        #endregion
        public KJobCardChild()
        {
            KJobCardChildID = 0;
            KJobCardMasterID = 0;
            KPChildID = 0;
            ProdQty = 0;
            ProdQtyPcs = 0;
            MachineType = "";
            MCSubClassName = "";
            MCSubClassID = 0;
            MaxQty = 0;
            MaxQtyKg = 0;
            MaxQtyPcs = 0;
            BookingQty = 0;
        }
    }
}
