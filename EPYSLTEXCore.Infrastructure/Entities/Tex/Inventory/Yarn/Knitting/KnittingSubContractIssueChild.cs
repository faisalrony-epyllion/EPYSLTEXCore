using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn.Knitting
{
    [Table(TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD)]
    public class KnittingSubContractIssueChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int KSCIssueChildID { get; set; }
        public int KSCIssueMasterID { get; set; } = 0;
        public int KSCReqChildID { get; set; } = 0;
        public decimal IssueQty { get; set; } = 0;
        public int IssueQtyCarton { get; set; } = 0;
        public int IssueQtyCone { get; set; } = 0;
        public string Remarks { get; set; } = "";


        #region Additional Columns
        [Write(false)]
        public int YarnProgramID { get; set; } = 0;
        [Write(false)]
        public decimal ReqQty { get; set; } = 0;
        [Write(false)]
        public string YarnCategory { get; set; } = "";
        [Write(false)]
        public int NoOfThread { get; set; } = 0;
        [Write(false)]
        public int YarnTypeID { get; set; } = 0;
        [Write(false)]
        public int YarnCountID { get; set; } = 0;
        [Write(false)]
        public string YarnLotNo { get; set; } = "";
        [Write(false)]
        public string PhysicalCount { get; set; } = "";
        [Write(false)]
        public int SpinnerID { get; set; } = 0;
        [Write(false)]
        public decimal StitchLength { get; set; } = 0;
        [Write(false)]
        public int SCQty { get; set; } = 0;
        [Write(false)]
        public decimal ReqCone { get; set; } = 0;
        [Write(false)]
        public int StockQty { get; set; } = 0;
        [Write(false)]
        public string YarnProgram { get; set; } = "";
        [Write(false)]
        public string Uom { get; set; } = "Kg";
        [Write(false)]
        public string YarnType { get; set; } = "";
        [Write(false)]
        public string YarnCount { get; set; } = "";
        [Write(false)]
        public string YarnComposition { get; set; } = "";
        [Write(false)]
        public string Shade { get; set; } = "";
        [Write(false)]
        public string YarnColor { get; set; } = "";
        [Write(false)]
        public string SpinnerName { get; set; } = "";
        [Write(false)]
        public int NoOfCarton { get; set; } = 0;
        [Write(false)]
        public int NoOfCone { get; set; } = 0;
        [Write(false)]
        public string YarnSubProgramIds { get; set; } = "";
        [Write(false)]
        public decimal AllocatedQty { get; set; } = 0;
        [Write(false)]
        public int YBChildItemID { get; set; } = 0;
        [Write(false)]
        public int StockFromTableId { get; set; } = 0;
        [Write(false)]
        public int StockFromPKId { get; set; } = 0;
        [Write(false)]
        public int StockTypeId { get; set; } = 0;
        [Write(false)]
        public List<KnittingSubContractIssueChildSubProgram> ChildSubPrograms { get; set; }
        [Write(false)]
        public List<KnittingSubContractIssueChildRackBinMapping> ChildRackBins { get; set; }
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified || KSCIssueChildID > 0;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        #endregion Additional Columns
        public KnittingSubContractIssueChild()
        {
            Uom = "Kg";
            StockQty = 0;
            ReqQty = 0;
            NoOfThread = 0;
            SpinnerID = 0;
            IssueQty = 0;
            IssueQtyCarton = 0;
            IssueQtyCone = 0;
            EntityState = EntityState.Added;
            ChildSubPrograms = new List<KnittingSubContractIssueChildSubProgram>();
            ChildRackBins = new List<KnittingSubContractIssueChildRackBinMapping>();
        }
    }
}
