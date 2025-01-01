using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace EPYSLTEX.Core.Entities.Tex
{ 
    [Table("CDAIndentChild")]
    public class CDAIndentChild : YarnItemMaster, IDapperBaseEntity
    {
        public CDAIndentChild()
        {
            IndentQty = 0;
            CheckQty = 0;
            ApprovQty = 0;
            ReqQty = 0;
            UnitID = 28;
            DisplayUnitDesc = "Kg";
            Remarks = "";
            HSCode = "";
            CompanyID = 0;
            EntityState = EntityState.Added;
            ChildItems = new List<CDAIndentChildDetails>();
            CDAIndentCompanies = new List<CDAIndentChildCompany>(); 
        }

        [ExplicitKey]
        public int CDAIndentChildID { get; set; }
        public int CDAIndentMasterID { get; set; }
        public string HSCode { get; set; }
        public int CompanyID { get; set; }
        public decimal IndentQty { get; set; }
        public decimal CheckQty { get; set; }
        public decimal ApprovQty { get; set; }
        public decimal ReqQty { get; set; }
        public string Remarks { get; set; }

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
        public string ItemIDs { get; set; }

        [Write(false)]
        public int CDAPRChildID { get; set; }
        [Write(false)]
        public int SubGroupID { get; set; }
        [Write(false)]
        public string SubGroupName { get; set; }
        [Write(false)]
        public string IndentNo { get; set; }
        [Write(false)]
        public DateTime IndentDate { get; set; }
        [Write(false)]
        public string TriggerPoint { get; set; }
        [Write(false)]
        public int CIndentBy { get; set; }
        [Write(false)]
        public string CDAIndentByUser { get; set; }
        [Write(false)]
        public DateTime AcknowledgeDate { get; set; }
        [Write(false)]
        public DateTime TexAcknowledgeDate { get; set; }
        [Write(false)]
        public string CompanyName { get; set; } 

        [Write(false)]
        public virtual List<CDAIndentChildCompany> CDAIndentCompanies { get; set; }
        [Write(false)]
        public List<CDAIndentChildDetails> ChildItems { get; set; }
        #endregion Additional Columns
    }
}