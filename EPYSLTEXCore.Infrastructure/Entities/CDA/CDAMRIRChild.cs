using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAMRIRChild")]
    public class CDAMRIRChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int MRIRChildID { get; set; }
        
        public int MRIRMasterID { get; set; }

        public string LotNo { get; set; }

        public int ReceiveQty { get; set; }

        public decimal Rate { get; set; }

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
        public int ReceiveID { get; set; }

        [Write(false)]
        public int RCompanyId { get; set; }

        [Write(false)]
        public int CompanyId { get; set; }

        [Write(false)]
        public int SupplierId { get; set; }

        [Write(false)]
        public int QCRemarksMasterId { get; set; }

        [Write(false)]
        public int ReqQty { get; set; }

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string AgentName { get; set; }
        #endregion
    }

}

