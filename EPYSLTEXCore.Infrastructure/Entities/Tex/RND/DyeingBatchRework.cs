using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{

    [Table(TableNames.DYEING_BATCH_REWORK)]

    public class DyeingBatchRework : DapperBaseEntity
    {
        public DyeingBatchRework()
        {
            DBRID = 0;
            DBatchID = 0;
            ShiftID = 0;
            UnloadShiftID = 0;
            OperatorID = 0;
            UnloadOperatorID = 0;
            PreProductionComplete = false;
            PostProductionComplete = false;
            BatchStatus = 0;
            IsNewBatch = false;
            IsNewRecipe = false;
            IsRedyeingBatch = false;
            ExistingDBatchID = 0;

            ExistingDBatchNo = "";
        }
        [ExplicitKey]
        public int DBRID { get; set; }
        public int DBatchID { get; set; }
        public int ShiftID { get; set; }
        public int UnloadShiftID { get; set; }
        public int OperatorID { get; set; }
        public int UnloadOperatorID { get; set; }
        public DateTime? BatchStartTime { get; set; }
        public DateTime? BatchEndTime { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? PlanBatchStartTime { get; set; }
        public DateTime? PlanBatchEndTime { get; set; }
        public bool PreProductionComplete { get; set; }
        public bool PostProductionComplete { get; set; }
        public int BatchStatus { get; set; }
        public bool IsNewBatch { get; set; }
        public bool IsNewRecipe { get; set; }
        public bool IsRedyeingBatch { get; set; }
        public int ExistingDBatchID { get; set; }
        #region Additional Fields
        [Write(false)]
        public string ExistingDBatchNo { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || DBRID > 0;
        #endregion
    }
}
