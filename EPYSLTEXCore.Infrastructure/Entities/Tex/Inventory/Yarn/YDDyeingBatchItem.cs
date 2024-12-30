using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_DYEING_BATCH_ITEM)]
    public class YDDyeingBatchItem : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDDBIID { get; set; }

        public int YDDBatchID { get; set; }

        public int YDBatchID { get; set; }

        public int YDBItemReqID { get; set; }

        public int ItemSubGroupID { get; set; }

        public int ItemMasterID { get; set; }

        public int YDRecipeID { get; set; }

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
        public int SoftWindingChildID { get; set; } = 0;
        public int YDRICRBId { get; set; } = 0;

        #region Additional Property

        [Write(false)]
        public List<YDDyeingBatchItemRoll> YDDyeingBatchItemRolls { get; set; }

        //[Write(false)]
        //public List<DyeingBatchChildFinishingProcess> DyeingBatchChildFinishingProcesses { get; set; }

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
        public string YDBatchNo { get; set; }

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
        public int YDRecipeItemInfoID { get; set; } = 0;
        [Write(false)]
        public string YarnCategory { get; set; }


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDDBIID > 0;

        #endregion Additional Property

        public YDDyeingBatchItem()
        {
            YDDyeingBatchItemRolls = new List<YDDyeingBatchItemRoll>();
        }
    }
}
