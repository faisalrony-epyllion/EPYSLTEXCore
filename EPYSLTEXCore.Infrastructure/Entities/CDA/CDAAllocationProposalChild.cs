using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Data.Entity;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("CDAAllocationProposalChild")]
    public class CDAAllocationProposalChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int YAPChildID { get; set; }

        public int YAPMasterID { get; set; }

        public int AllocationID { get; set; }

        public int AllocationChildID { get; set; }

        public decimal DiagnosticStockQty { get; set; }

        public bool Approve { get; set; }

        public int? ApproveBy { get; set; }

        public DateTime? ApproveDate { get; set; }

        public bool Reject { get; set; }

        public int? RejectBy { get; set; }

        public DateTime? RejectDate { get; set; }

        public bool Acknowledge { get; set; }

        public int? AcknowledgeBy { get; set; }

        public DateTime? AcknowledgeDate { get; set; }

        public string Remarks { get; set; }


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
        public string CDAType { get; set; }

        [Write(false)]
        public string CDACount { get; set; }

        [Write(false)]
        public string CDAComposition { get; set; }

        [Write(false)]
        public string CDAShade { get; set; }

        [Write(false)]
        public string CDAColor { get; set; }
        EntityState IDapperBaseEntity.EntityState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion

        public CDAAllocationProposalChild()
        {
            DiagnosticStockQty = 0m;
            Approve = false;
            ApproveBy = 0;
            Reject = false;
            RejectBy = 0;
        }
    }

}

