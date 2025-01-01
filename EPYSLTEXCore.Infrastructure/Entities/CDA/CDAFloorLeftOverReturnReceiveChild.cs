using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Data.Entity;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("CDAFloorLeftOverReturnReceiveChild")]
    public class CDAFloorLeftOverReturnReceiveChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int LeftOverReturnReceiveChildID { get; set; }

        public int LeftOverReturnReceiveMasterID { get; set; }

        public string BatchNo { get; set; }

        public int ReceiveQty { get; set; }

        public int ReturnQty { get; set; }

        public string Remarks { get; set; }

        public decimal Rate { get; set; }

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
        public int LOReturnReceiveMasterID { get; set; }

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string AgentName { get; set; }

        #endregion Additional
    }
}