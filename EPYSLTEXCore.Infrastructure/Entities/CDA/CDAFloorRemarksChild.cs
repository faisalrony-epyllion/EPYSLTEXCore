using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Data.Entity;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("CDAFloorRemarksChild")]
    public class CDAFloorRemarksChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int FloorRemarksChildID { get; set; }

        public int FloorRemarksMasterID { get; set; }

        public string BatchNo { get; set; }

        public decimal Rate { get; set; }

        public string Remarks { get; set; }

        public bool Approve { get; set; }

        public int ApproveBy { get; set; }

        public DateTime? ApproveDate { get; set; }

        public bool Reject { get; set; }

        public int RejectBy { get; set; }

        public DateTime? RejectDate { get; set; }

        public bool? ReTest { get; set; }

        public int? ReTestBy { get; set; }

        public DateTime? ReTestDate { get; set; }

        public int ReqQty { get; set; }

        public int IssueQty { get; set; }

        public int ReceiveQty { get; set; }

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
        public int CompanyId { get; set; }

        [Write(false)]
        public int SupplierId { get; set; }

        [Write(false)]
        public DateTime FloorRemarksDate { get; set; }

        [Write(false)]
        public int FloorReqMasterId { get; set; }

        [Write(false)]
        public int FloorIssueMasterId { get; set; }

        [Write(false)]
        public int FloorReceiveMasterId { get; set; }

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public string Supplier { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string AgentName { get; set; }

        #endregion Additional

        public CDAFloorRemarksChild()
        {
            Approve = false;
            Reject = false;
            ReTest = false;
        }
    }
}