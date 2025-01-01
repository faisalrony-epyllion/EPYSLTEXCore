using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAFloorLeftOverReturnChild")]
    public class CDAFloorLeftOverReturnChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int LOReturnChildID { get; set; }

        public int LOReturnMasterID { get; set; }

        public int ReceiveQty { get; set; }

        public int ReturnQty { get; set; }

        public string Remarks { get; set; }

        public string BatchNo { get; set; }

        public decimal Rate { get; set; }

        #region Additional

        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string AgentName { get; set; }

        #endregion Additional
    }
}