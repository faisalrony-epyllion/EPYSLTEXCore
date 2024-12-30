using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.KYReqBuyerTeam)]
    public class KYReqBuyerTeam: DapperBaseEntity
    {
        [ExplicitKey]
        public int KYReqBuyerTeamID { get; set; }

        public int KYReqMasterID { get; set; }
        public int BuyerTeamId { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.KYReqBuyerTeamID > 0;
    }
}
