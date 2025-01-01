using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASTSReqChild")]
    public class CDASTSReqChild : YarnItemMaster, IDapperBaseEntity //: DapperBaseEntity
    {
        [ExplicitKey]
        public int STSReqChildID { get; set; }
        public int STSReqMasterID { get; set; } 
        public int ReqQty { get; set; }
        public string Remarks { get; set; }


        #region Additional Columns
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified || STSReqChildID > 0;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string AgentName { get; set; }
        [Write(false)]
        public int CompanyID { get; set; } 
        [Write(false)]
        public string DisplayUnitDesc { get; set; } 

        #endregion Additional Columns
        public CDASTSReqChild()
        {
            ReqQty = 0;
            UnitID = 28;
            DisplayUnitDesc = "Kg";
            EntityState = EntityState.Added;
        }
    }
 
}
 
