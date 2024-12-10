using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table("YarnQCIssueChildRackBinMapping")]
    public class YarnQCIssueChildRackBinMapping : DapperBaseEntity
    {
        public YarnQCIssueChildRackBinMapping()
        {
            YQCICRBId = 0;
            QCIssueChildID = 0;
            ChildRackBinID = 0;
            IssueQtyCone = 0;
            IssueCartoon = 0;
            IssueQtyKg = 0;
        }
        [ExplicitKey]
        public int YQCICRBId { get; set; }
        public int QCIssueChildID { get; set; }
        public int ChildRackBinID { get; set; }
        public int IssueQtyCone { get; set; }
        public int IssueCartoon { get; set; }
        public decimal IssueQtyKg { get; set; }

        #region Additional properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YQCICRBId > 0;
        #endregion
    }
}
