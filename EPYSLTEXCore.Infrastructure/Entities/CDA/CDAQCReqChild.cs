using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAQCReqChild")]
    public class CDAQCReqChild : YarnItemMaster, IDapperBaseEntity
    {
        public CDAQCReqChild()
        {
            EntityState = EntityState.Added;
            UnitID = 1;
            Uom = "Pcs";
        }

        [ExplicitKey]
        public int QCReqChildID { get; set; }

        public int QCReqMasterId { get; set; }

        public string LotNo { get; set; }

        public int ReqQty { get; set; }

        #region Additional Fields

        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int SupplierId { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public string Supplier { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string AgentName { get; set; }

        [Write(false)]
        public int RCompanyId { get; set; }

        [Write(false)]
        public int CompanyId { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        #endregion Additional Fields
    }
}