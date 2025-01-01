using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAPOChild")]
    public class CDAPOChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int CDAPOChildID { get; set; }
        public int CDAPOMasterID { get; set; } 
        public decimal POQty { get; set; }
        public decimal Rate { get; set; }
        public decimal POValue { get; set; }
        public string Remarks { get; set; }
        public string HSCode { get; set; }
        public string CDAShade { get; set; }
        public int? CDAProgramID { get; set; }
        public int? NoOfThread { get; set; }
        public int POForID { get; set; }
        public decimal ReqQty { get; set; }
        public int PRMasterID { get; set; }
        public int PRChildID { get; set; }
        public string QuotationRefNo { get; set; } 
        public DateTime? QuotationRefDate { get; set; }

        #region Additional Columns
        [Write(false)]
        public int POUnitID { get; set; }
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified || CDAPOChildID > 0;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added; 
        
        [Write(false)]
        public string PRNo { get; set; }
        [Write(false)]
        public int CDAPRChildID { get; set; }

        [Write(false)]
        public string DisplayUnitDesc { get; set; } 
        [Write(false)]
        public string POFor { get; set; }
        [Write(false)]
        public DateTime PRRequiredDate { get; set; }

        #endregion Additional Columns 
        public CDAPOChild()
        {
            UnitID = 28;
            DisplayUnitDesc = "Kg";
            EntityState = EntityState.Added;
            ReqQty = 0m;
        }
    }

}
