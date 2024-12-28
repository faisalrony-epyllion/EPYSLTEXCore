using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{

    [Table(TableNames.DYEING_BATCH_ITEM)]
    public class DyeingBatchItem : DapperBaseEntity
    {
        [ExplicitKey]
        public int DBIID { get; set; }

        public int DBatchID { get; set; }

        public int BatchID { get; set; }

        public int ItemSubGroupID { get; set; }

        public int ItemMasterID { get; set; }

        public int RecipeID { get; set; }

        public int ConceptID { get; set; }

        public int BookingID { get; set; }

        public int ExportOrderID { get; set; }

        public int BuyerID { get; set; }

        public int BuyerTeamID { get; set; }

        public decimal Qty { get; set; }
        public decimal QtyPcs { get; set; }

        public bool IsFloorRequistion { get; set; }
        public bool PostProductionComplete { get; set; }
        public DateTime? ProductionDate { get; set; }

        #region Additional Property

        [Write(false)]
        public List<DyeingBatchItemRoll> DyeingBatchItemRolls { get; set; }

        [Write(false)]
        public List<DyeingBatchChildFinishingProcess> DyeingBatchChildFinishingProcesses { get; set; }
        
        [Write(false)]
        public string ItemSubGroup { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public string BookingNo { get; set; }

        [Write(false)]
        public string ExportOrderNo { get; set; }

        [Write(false)]
        public string BuyerName { get; set; }

        [Write(false)]
        public string BuyerTeamName { get; set; }

        [Write(false)]
        public string BatchNo { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string FabricConstruction { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string FabricComposition { get; set; }

        [Write(false)]
        public string FabricGsm { get; set; }

        [Write(false)]
        public int BItemReqID { get; set; }

        [Write(false)]
        public int FFSFRollID { get; set; }



        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || DBIID > 0;

        #endregion Additional Property

        public DyeingBatchItem()
        {
            DyeingBatchItemRolls = new List<DyeingBatchItemRoll>();
        }
    }
}