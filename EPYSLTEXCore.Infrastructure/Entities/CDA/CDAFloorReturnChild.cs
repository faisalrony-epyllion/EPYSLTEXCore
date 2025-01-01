using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAFloorReturnChild")]
    public class CDAFloorReturnChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int FloorReturnChildID { get; set; }
        
        public int FloorReturnMasterID { get; set; }

        public string BatchNo { get; set; }

        public string Remarks { get; set; }

        public decimal Rate { get; set; }

        public int ReceiveQty { get; set; }

        public int ReturnQty { get; set; }

        #region AdditionaL
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public int FloorRemarksMasterID { get; set; }

        [Write(false)]
        public int ReqQty { get; set; }

        [Write(false)]
        public int FloorReceiveMasterId { get; set; }

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string AgentName { get; set; }

        #endregion
    }

   

}
