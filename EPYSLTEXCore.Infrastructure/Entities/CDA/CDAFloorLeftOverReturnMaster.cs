using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAFloorLeftOverReturnMaster")]
    public class CDAFloorLeftOverReturnMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int LOReturnMasterID { get; set; }

        public string LOReturnNo { get; set; }

        public DateTime LOReturnDate { get; set; }

        public int LOReturnBy { get; set; }

        public int ReceiveMasterID { get; set; }

        public int CompanyID { get; set; }

        public int SupplierID { get; set; }

        public int SubGroupID { get; set; }

        public string Remarks { get; set; }

        public int AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        #region Additional PRoperty

        [Write(false)]
        public List<CDAFloorLeftOverReturnChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LOReturnMasterID > 0;

        [Write(false)]
        public string LOReturnByUser { get; set; }

        [Write(false)]
        public int ReceivedBy { get; set; }

        [Write(false)]
        public string ReceiveNo { get; set; }

        [Write(false)]
        public string ReceivedByUser { get; set; }

        [Write(false)]
        public DateTime ReceiveDate { get; set; }

        [Write(false)]
        public int CompanyId { get; set; }

        [Write(false)]
        public int SupplierId { get; set; }

        [Write(false)]
        public int SubGroupId { get; set; }

        #endregion Additional PRoperty

        public CDAFloorLeftOverReturnMaster()
        {
            LOReturnDate = DateTime.Now;
            LOReturnNo = AppConstants.NEW;
            Childs = new List<CDAFloorLeftOverReturnChild>();
        }
    }
}