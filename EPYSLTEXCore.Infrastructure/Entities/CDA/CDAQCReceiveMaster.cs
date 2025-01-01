using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAQCReceiveMaster")]
    public class CDAQCReceiveMaster : DapperBaseEntity
    {
        public CDAQCReceiveMaster()
        {
            QCReceiveDate = DateTime.Now;
            QCReceiveNo = AppConstants.NEW;
            DateAdded = DateTime.Now;
            Childs = new List<CDAQCReceiveChild>();
        }

        [ExplicitKey]
        public int QCReceiveMasterID { get; set; }

        public string QCReceiveNo { get; set; }

        public int PhysicalReceiveID { get; set; }

        public int SupplierID { get; set; }

        public int CompanyID { get; set; }

        public int RCompanyID { get; set; }

        public int QCReceivedBy { get; set; }

        public DateTime QCReceiveDate { get; set; }

        public int QCReqMasterId { get; set; }

        public int QCIssueMasterId { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }


        #region Additional Columns
        [Write(false)]
        public List<CDAQCReceiveChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || QCReceiveMasterID > 0;

        [Write(false)]
        public int ReceiveID { get; set; }

        [Write(false)]
        public string QCReceivedByUser { get; set; }

        [Write(false)]
        public string QCIssueNo { get; set; }

        [Write(false)]
        public DateTime QCIssueDate { get; set; }

        [Write(false)]
        public string QCIssueByUser { get; set; }

        [Write(false)]
        public string QCReqByUser { get; set; }

        [Write(false)]
        public string QCReqNo { get; set; }

        [Write(false)]
        public DateTime QCReqDate { get; set; }

        [Write(false)]
        public string QCReqFor { get; set; }

        [Write(false)]
        public int ReqQty { get; set; }

        [Write(false)]
        public int IssueQty { get; set; }

        [Write(false)]
        public int ReceiveQty { get; set; }

        #endregion

    }
}