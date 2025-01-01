using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Data.Entity;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("CDAFloorReceiveChild")]
    public class CDAFloorReceiveChild : YarnItemMaster, IDapperBaseEntity
    {
        public int FloorReceiveChildID { get; set; }

        public int FloorReceiveMasterID { get; set; }

        public string BatchNo { get; set; }

        public decimal Rate { get; set; }

        public int ReqQty { get; set; }

        public int IssueQty { get; set; }

        public int ReceiveQty { get; set; }

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
        public int FloorRemarksMasterId { get; set; }

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public string Supplier { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string AgentName { get; set; }

        #endregion Additional
    }
}

// </auto-generated>