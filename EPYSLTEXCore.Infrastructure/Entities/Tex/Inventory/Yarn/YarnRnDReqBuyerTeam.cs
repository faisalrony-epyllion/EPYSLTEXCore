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
    [Table(TableNames.YarnRNDReqBuyerTeam)]
    public class YarnRnDReqBuyerTeam: DapperBaseEntity
    {
        [ExplicitKey]
        public int YarnRnDReqBuyerTeamID { get; set; } = 0;

        public int RnDReqMasterID { get; set; }= 0;
        public int BuyerTeamId { get; set; } = 0;

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YarnRnDReqBuyerTeamID > 0;

    }
}
