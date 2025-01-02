using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAFloorReqChild")]
    public class CDAFloorReqChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int FloorReqChildID { get; set; }

        public int FloorReqMasterID { get; set; }

        public int ReqQty { get; set; }

        public string Remarks { get; set; }

        public decimal AdditionalReqQty { get; set; }


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
        public string DisplayUnitDesc { get; set; }

        [Write(false)]
        public int BItemReqID { get; set; }

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string AgentName { get; set; }

        [Write(false)]
        public int CompanyId { get; set; }

        #endregion

        public CDAFloorReqChild()
        {
            DisplayUnitDesc = "Kg";
            ReqQty = 0;
        }
    }

}
// </auto-generated>
