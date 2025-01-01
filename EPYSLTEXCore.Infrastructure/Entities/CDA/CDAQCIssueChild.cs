using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAQCIssueChild")]
    public class CDAQCIssueChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int QCIssueChildID { get; set; }

        public int QCIssueMasterID { get; set; }

        public string LotNo { get; set; }

        public int ReqQty { get; set; }

        public int IssueQty { get; set; }
        
        public decimal Rate { get; set; }

        #region Additional Column
        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;

        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public int SupplierId { get; set; }

        [Write(false)]
        public int SpinnerId { get; set; }
        #endregion
    }

}

