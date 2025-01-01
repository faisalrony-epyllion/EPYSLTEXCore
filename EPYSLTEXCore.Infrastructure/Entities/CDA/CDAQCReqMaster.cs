using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAQCReqMaster")]
    public class CDAQCReqMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReqMasterID { get; set; }

        public string QCReqNo { get; set; }
        public int ReceiveID { get; set; }
        public int SupplierID { get; set; }
        public int CompanyID { get; set; }
        public int RCompanyID { get; set; }
        public int QCReqBy { get; set; }
        public DateTime QCReqDate { get; set; }
        public int QCForId { get; set; }
        public bool IsApprove { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int? ApproveBy { get; set; }
        public bool IsAcknowledge { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public int? AcknowledgeBy { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        #region Additional Columns

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || QCReqMasterID > 0;

        [Write(false)]
        public List<CDAQCReqChild> Childs { get; set; }

        [Write(false)]
        public List<Select2OptionModel> QCForList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> ReceiveList { get; set; }

        [Write(false)]
        public string[] ReceiveIds { get; set; }

        [Write(false)]
        public string QCReqFor { get; set; }

        [Write(false)]
        public string QCReqByUser { get; set; }

        [Write(false)]
        public string IsApproveStr => IsApprove ? "Yes" : "No";

        [Write(false)]
        public string IsAcknowledgeStr => IsAcknowledge ? "Yes" : "No";

        #endregion Additional Columns

        public CDAQCReqMaster()
        {
            QCReqDate = DateTime.Now;
            QCReqNo = AppConstants.NEW;
            DateAdded = DateTime.Now;
            IsApprove = false;
            IsAcknowledge = false;
            Childs = new List<CDAQCReqChild>();
        }
    }

    #region Validators

    

    #endregion Validators
}