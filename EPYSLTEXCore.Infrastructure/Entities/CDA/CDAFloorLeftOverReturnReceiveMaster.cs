using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAFloorLeftOverReturnReceiveMaster")]
    public class CDAFloorLeftOverReturnReceiveMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int LeftOverReturnReceiveMasterID { get; set; }

        public string LeftOverReturnReceiveNo { get; set; }

        public DateTime LeftOverReturnReceiveDate { get; set; }

        public int LeftOverReturnReceiveBy { get; set; }

        public int LOReturnMasterID { get; set; }

        public int RReceiveMasterID { get; set; }

        public int CompanyID { get; set; }

        public int SupplierID { get; set; }

        public int SubGroupID { get; set; }

        public string Remarks { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        #region Additional

        public List<CDAFloorLeftOverReturnReceiveChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LOReturnMasterID > 0;

        [Write(false)]
        public string LOReturnReceiveNo { get; set; }

        [Write(false)]
        public DateTime LOReturnReceiveDate { get; set; }

        [Write(false)]
        public int LOReturnReceiveBy { get; set; }

        [Write(false)]
        public string LOReturnReceiveByUser { get; set; }

        [Write(false)]
        public string LOReturnNo { get; set; }

        [Write(false)]
        public DateTime LOReturnDate { get; set; }

        [Write(false)]
        public string LOReturnByUser { get; set; }

        [Write(false)]
        public string ReceiveNo { get; set; }

        #endregion Additional

        public CDAFloorLeftOverReturnReceiveMaster()
        {
            LOReturnReceiveDate = DateTime.Now;
            LOReturnReceiveNo = AppConstants.NEW;
            Childs = new List<CDAFloorLeftOverReturnReceiveChild>();
            DateAdded = DateTime.Now;
        }
    }
}