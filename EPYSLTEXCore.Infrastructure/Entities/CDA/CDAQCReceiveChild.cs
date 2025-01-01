using Dapper.Contrib.Extensions;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAQCReceiveChild")]
    public class CDAQCReceiveChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int QCReceiveChildID { get; set; }
        
        public int QCReceiveMasterID { get; set; }
        
        public string LotNo { get; set; }

        public int ReqQty { get; set; }

        public int IssueQty { get; set; }

        public int ReceiveQty { get; set; }

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
        public int QCRemarksMasterId { get; set; }
        
        #endregion

    }

}

