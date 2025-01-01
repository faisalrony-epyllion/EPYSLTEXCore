using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASTSReceiveChild")]
    public class CDASTSReceiveChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int STSReceiveChildID { get; set; }
        public int STSReceiveMasterID { get; set; }
        public string BatchNo { get; set; } 
        public int ReqQty { get; set; }
        public int IssueQty { get; set; }
        public int ReceiveQty { get; set; }
        public decimal Rate { get; set; }

        #region Additional Columns
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified || STSReceiveChildID > 0;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

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
        public CDASTSReceiveChild()
        {
            IssueQty = 0;
            ReceiveQty = 0;
            Rate = 0; 
            UnitID = 28;
            DisplayUnitDesc = "Kg";
            EntityState = EntityState.Added;
        }
    }
    
}
// </auto-generated>
