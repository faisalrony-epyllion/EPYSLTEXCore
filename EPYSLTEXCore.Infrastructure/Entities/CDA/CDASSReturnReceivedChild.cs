using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASSReturnReceivedChild")]
    public class CDASSReturnReceivedChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int SSReturnReceivedChildID { get; set; }
        public int SSReturnMasterID { get; set; }
        public int SSReturnReceivedMasterID { get; set; } 
        public string Remarks { get; set; } 
        public int ReceiveQty { get; set; }
        public int ReturnQty { get; set; }
        public string BatchNo { get; set; }
        public decimal Rate { get; set; } 


        #region Additional Columns
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified || SSReturnReceivedChildID > 0;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public int SSReceiveMasterId { get; set; }
        [Write(false)]
        public int SSRemarksMasterID { get; set; }
        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string AgentName { get; set; }
        [Write(false)]
        public string DisplayUnitDesc { get; set; }

        #endregion Additional Columns
        public CDASSReturnReceivedChild()
        {
            ReceiveQty = 0;
            ReturnQty = 0;
            Rate = 0;
            UnitID = 28;
            DisplayUnitDesc = "Kg";
            EntityState = EntityState.Added;
        } 
    }
    
}
 
