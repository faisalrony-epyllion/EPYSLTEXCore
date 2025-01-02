using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASTSIssueChild")]
    public class CDASTSIssueChild : YarnItemMaster, IDapperBaseEntity
    {
        public int STSIssueChildID { get; set; }
        public int STSIssueMasterID { get; set; }
        public string BatchNo { get; set; }
        public int ReqQty { get; set; }
        public int IssueQty { get; set; }
        public string Remarks { get; set; }
        public decimal Rate { get; set; } 


        #region Additional Columns
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified || STSIssueChildID > 0;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string AgentName { get; set; }        
        [Write(false)]
        public string DisplayUnitDesc { get; set; }

        #endregion Additional Columns
        public CDASTSIssueChild()
        {
            ReqQty = 0;
            IssueQty = 0;
            UnitID = 28;
            DisplayUnitDesc = "Kg";
            EntityState = EntityState.Added;
        }
    }
   
}
 
