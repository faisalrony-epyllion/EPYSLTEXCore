using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn.Knitting
{
    [Table(TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING)]
    public class KnittingSubContractIssueChildRackBinMapping : DapperBaseEntity
    {
        public KnittingSubContractIssueChildRackBinMapping()
        {
            KSCICRBId = 0;
            KSCIssueChildID = 0;
            ChildRackBinID = 0;
            IssueQtyCone = 0;
            IssueCartoon = 0;
            IssueQtyKg = 0;
            YarnStockSetId = 0;
        }
        [ExplicitKey]
        public int KSCICRBId { get; set; }
        public int KSCIssueChildID { get; set; }
        public int ChildRackBinID { get; set; }
        public int IssueQtyCone { get; set; }
        public int IssueCartoon { get; set; }
        public decimal IssueQtyKg { get; set; }
        public int YarnStockSetId { get; set; }

        #region Additional properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || KSCICRBId > 0;
        #endregion
    }
}
