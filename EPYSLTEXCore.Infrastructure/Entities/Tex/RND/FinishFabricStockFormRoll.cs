using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table("T_FinishFabricStockFormRoll")]
    public class FinishFabricStockFormRoll : DapperBaseEntity
    {
        #region Table Properties
        [ExplicitKey]
        public int FFSFRollID { get; set; }
        public int FFSFromID { get; set; }
        public int DBIRollID { get; set; }
        public decimal RollQty { get; set; }
        public int RollQtyPcs { get; set; }
        public int GRollID { get; set; }

        #endregion Table Properties

        #region MyRegion
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.FFSFRollID > 0;

        [Write(false)]
        public int FormID { get; set; }

        [Write(false)]
        public List<LiveProductForm> LiveProductForm { get; set; }
        [Write(false)]
        public List<DyeingBatchItemRoll> DyeingBatchItemRolls { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }


        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string Gsm { get; set; }
        [Write(false)]
        public int LPFormID { get; set; }
        [Write(false)]
        public string RollNo { get; set; }

        [Write(false)]
        public int QtyInPcs { get; set; }

        [Write(false)]
        public decimal QtyinKG { get; set; }

        [Write(false)]
        public int DBIID { get; set; }
        //[Write(false)] 
        //public int DBatchID { get; set; }
        //[Write(false)]
        //public int BatchID { get; set; }


        #endregion MyRegion
        public FinishFabricStockFormRoll()
        {
            DyeingBatchItemRolls = new List<DyeingBatchItemRoll>();
            LiveProductForm = new List<LiveProductForm>();
        }
    }
}
