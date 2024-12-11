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
    [Table(TableNames.BondEntitlementChildItem)]
    public class BondEntitlementChildItem : DapperBaseEntity
    {
        [ExplicitKey]
        public int BondEntitlementChildItemID { get; set; } = 0;
        public int BondEntitlementChildID { get; set; } = 0;
        public int SegmentValueID { get; set; } = 0;
        public string HSCode { get; set; } = "";
        public decimal BankFacilityAmount { get; set; } = 0;

        #region Additional Property

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.BondEntitlementChildItemID > 0;
        [Write(false)]
        public string SegmentValue { get; set; } = "";

        #endregion Additional Property
    }
}
