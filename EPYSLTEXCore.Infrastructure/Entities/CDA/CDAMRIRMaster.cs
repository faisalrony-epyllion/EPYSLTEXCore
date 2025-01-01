using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAMRIRMaster")]
    public class CDAMRIRMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int MRIRMasterID { get; set; }

        public string MRIRNo { get; set; }

        public int MRIRBy { get; set; }

        public DateTime MRIRDate { get; set; }

        public int QCRemarksMasterID { get; set; }

        public int ReceiveID { get; set; }

        public int CompanyID { get; set; }

        public int RCompanyID { get; set; }

        public int SupplierID { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        #region Additional

        [Write(false)]
        public List<CDAMRIRChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || MRIRMasterID > 0;

        [Write(false)]
        public string MRIRByUser { get; set; }

        [Write(false)]
        public List<Select2OptionModel> ReceiveList { get; set; }

        #endregion Additional

        public CDAMRIRMaster()
        {
            Childs = new List<CDAMRIRChild>();
            MRIRDate = System.DateTime.Now;
            MRIRNo = AppConstants.NEW;
            Childs = new List<CDAMRIRChild>();
            ReceiveList = new List<Select2OptionModel>();
        }
    }
}