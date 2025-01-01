using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAAllocationChild")]
    public class CDAAllocationChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int AllocationChildID { get; set; }
        
        public int AllocationID { get; set; }

        public string LotNo { get; set; }

        public int CCID1 { get; set; }

        public int CCID2 { get; set; }

        public int CCID3 { get; set; }

        public int CCID4 { get; set; }

        public int CCID5 { get; set; }

        public int RCCID1 { get; set; }

        public int RCCID2 { get; set; }

        public int RCCID3 { get; set; }

        public int RCCID4 { get; set; }

        public int RCCID5 { get; set; }

        public decimal RelativeFactor { get; set; }

        public int SUnitID { get; set; }

        public decimal ReqQty { get; set; }

        public decimal ApproveStockQty { get; set; }

        public decimal DiagnosticStockQty { get; set; }

        public decimal PipelineStockQty { get; set; }

        public decimal TotalAllocatedQty { get; set; }

        #region Additional
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public int SUom { get; set; }

        [Write(false)]
        public string CDAType { get; set; }

        [Write(false)]
        public string CDACount { get; set; }

        [Write(false)]
        public string CDAComposition { get; set; }

        [Write(false)]
        public string CDAShade { get; set; }

        [Write(false)]
        public string CDAColor { get; set; }
        #endregion


        public CDAAllocationChild()
        {
            RelativeFactor = 0m;
            SUnitID = 0;
            ApproveStockQty = 0m;
            DiagnosticStockQty = 0m;
            PipelineStockQty = 0m;
        }
    }

}

