using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("CDAFloorIssueMaster")]
    public class CDAFloorIssueMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int FloorIssueMasterID { get; set; }

        public int SupplierID { get; set; }

        public string FloorIssueNo { get; set; }

        public DateTime FloorIssueDate { get; set; }

        public int FloorIssueBy { get; set; }

        public int FloorReqMasterID { get; set; }

        public int CompanyID { get; set; }

        public int SubGroupID { get; set; }

        public bool Approve { get; set; }

        public DateTime? ApproveDate { get; set; }

        public int? ApproveBy { get; set; }

        public bool Reject { get; set; }

        public DateTime? RejectDate { get; set; }

        public int? RejectBy { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        public string Remarks { get; set; }

        #region Additional
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FloorIssueMasterID > 0;

        [Write(false)]
        public List<CDAFloorIssueChild> Childs { get; set; }

        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; }

        [Write(false)]
        public string FloorIssueByUser { get; set; }

        [Write(false)]
        public string FloorReqNo { get; set; }

        [Write(false)]
        public DateTime FloorReqDate { get; set; }

        [Write(false)]
        public string ReqByUser { get; set; }

        [Write(false)]
        public int ReqQty { get; set; }

        [Write(false)]
        public int IssueQty { get; set; }

        #endregion

        public CDAFloorIssueMaster()
        {
            Childs = new List<CDAFloorIssueChild>();
        }
    }

}
