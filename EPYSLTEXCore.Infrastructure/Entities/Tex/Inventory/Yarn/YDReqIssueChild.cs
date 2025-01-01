using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_REQ_ISSUE_CHILD)]
    public class YDReqIssueChild : YarnItemMaster, IDapperBaseEntity
    {
        public YDReqIssueChild()
        {
            YarnProgramID = 0;
            NoOfThread = 0;
            IssueQtyCone = 0;
            IssueQtyCarton = 0;
            YarnBrandID = 0;
            ChildRackBins = new List<YDReqIssueChildRackBinMapping>();
            EntityState = EntityState.Added;
        }

        #region Table Properties

        [ExplicitKey]
        public int YDReqIssueChildID { get; set; }

        public int YDReqIssueMasterID { get; set; }
        public int YDReqChildID { get; set; }
        public int YarnProgramID { get; set; }
        public decimal ReqQty { get; set; }
        public decimal IssueQty { get; set; }
        public int IssueQtyCone { get; set; }
        public int IssueQtyCarton { get; set; }
        public string Remarks { get; set; }
        public string YarnCategory { get; set; }
        public int NoOfThread { get; set; }
        public string LotNo { get; set; }
        public string PhysicalCount { get; set; }
        public int YarnBrandID { get; set; }
        public decimal Rate { get; set; }
        public string ShadeCode { get; set; }

        #endregion Table Properties

        #region Additional Properties
        [Write(false)]
        public string YarnBrand { get; set; }
        [Write(false)]
        public List<YDReqIssueChildRackBinMapping> ChildRackBins { get; set; }
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified || YDReqIssueChildID > 0;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        #endregion Additional Properties

    }
}
