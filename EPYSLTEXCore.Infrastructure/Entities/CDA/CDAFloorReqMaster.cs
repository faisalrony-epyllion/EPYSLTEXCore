using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAFloorReqMaster")]
    public class CDAFloorReqMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int FloorReqMasterID { get; set; }

        public string FloorReqNo { get; set; }

        public int FloorReqBy { get; set; }

        public DateTime FloorReqDate { get; set; }

        public int SubGroupID { get; set; }

        public bool IsApprove { get; set; }

        public DateTime? ApproveDate { get; set; }

        public int? ApproveBy { get; set; }

        public bool IsAcknowledge { get; set; }

        public DateTime? AcknowledgeDate { get; set; }

        public int? AcknowledgeBy { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public bool? Reject { get; set; }

        public int? RejectBy { get; set; }

        public DateTime? RejectDate { get; set; }

        public string RejectReason { get; set; }

        public int? UpdatedBy { get; set; }

        public System.DateTime? DateUpdated { get; set; }

        public string Remarks { get; set; }

        public int CompanyID { get; set; }

        public int BatchID { get; set; }

        public int ReqStatus { get; set; }

        public bool AdditionalApprove { get; set; }

        public string AdditionalReason { get; set; }

        public int? AdditionalApproveBy { get; set; }
        public DateTime ? AdditionalApproveDate { get; set; }

        #region Additional
        [Write(false)]
        public List<CDAFloorReqChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || FloorReqMasterID > 0;

        [Write(false)]
        public int RCompanyId { get; set; }

        [Write(false)]
        public bool FromBatch { get; set; }

        [Write(false)]
        public string RequisitionByUser { get; set; }

        [Write(false)]
        public string ApproveByUser { get; set; }

        [Write(false)]
        public string AcknowledgeByUser { get; set; }

        [Write(false)]
        public int TotalQty { get; set; }

        [Write(false)]
        public string BatchNo { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public DateTime BatchDate { get; set; }

        [Write(false)]
        public DateTime ConceptDate { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public string FloorReqByUser { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> ItemList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> AgentList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> FloorReqByList { get; set; }

        #endregion

        public CDAFloorReqMaster()
        {
            Reject = false;
            IsAcknowledge = false;
            IsApprove = false;
            Childs = new List<CDAFloorReqChild>();
            FloorReqDate = DateTime.Now;
            FloorReqNo = AppConstants.NEW;
        }
    }

}