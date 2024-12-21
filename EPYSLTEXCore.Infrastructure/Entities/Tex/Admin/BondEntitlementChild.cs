using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Admin
{
    [Table(TableNames.BondEntitlementChild)]
    public class BondEntitlementChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int BondEntitlementChildID { get; set; } = 0;
        public int BondEntitlementMasterID { get; set; } = 0;
        public int SegmentNameID { get; set; } = 0;
        public string ItemName { get; set; } = "";
        public int UnitID { get; set; } = 0;
        public decimal EntitlementQty { get; set; } = 0;

        #region Additional Property

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.BondEntitlementChildID > 0;
        [Write(false)]
        public List<BondEntitlementChildItem> ChildItems { get; set; } = new List<BondEntitlementChildItem>();
        [Write(false)]
        public string SegmentName { get; set; } = "";

        #endregion Additional Property
    }
}
