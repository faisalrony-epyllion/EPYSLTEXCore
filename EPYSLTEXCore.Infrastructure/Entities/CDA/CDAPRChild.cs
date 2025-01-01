using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDAPRChild")]
    public class CDAPRChild : YarnItemMaster, IDapperBaseEntity
    {
        public CDAPRChild()
        {
            ReqQty = 0;
            UnitID = 28;
            DisplayUnitDesc = "Kg";
            Remarks = "";
            EntityState = EntityState.Added;
            CDAPRCompanies = new List<CDAPRCompany>();
            SuggestedQty = 0;
            CDAIndentChildID = 0;
            HSCode = "";
        }

        [ExplicitKey]
        public int CDAPRChildID { get; set; }
        public int CDAPRMasterID { get; set; }
        public int ReqQty { get; set; }
        public string Remarks { get; set; }
        public string YarnCategory { get; set; }
        public int SetupChildID { get; set; }
        public int FPRCompanyID { get; set; }
        public int SuggestedQty { get; set; }
        public int CDAIndentChildID { get; set; }
        public string HSCode { get; set; }

        #region Additional Columns 
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public string CompanyName { get; set; }
        [Write(false)]
        public string FPRCompanyName { get; set; }
        [Write(false)]
        public string CPRCompanyNames { get; set; }
        [Write(false)]
        public int[] CompanyIDs { get; set; }
        [Write(false)]
        public int CompanyID { get; set; }
        [Write(false)]
        public int[] CPRCompanyIDs { get; set; }
        [Write(false)]
        public string ItemIDs { get; set; }
        [Write(false)]
        public virtual List<CDAPRCompany> CDAPRCompanies { get; set; }
        #endregion Additional Columns  
    }
    

}

