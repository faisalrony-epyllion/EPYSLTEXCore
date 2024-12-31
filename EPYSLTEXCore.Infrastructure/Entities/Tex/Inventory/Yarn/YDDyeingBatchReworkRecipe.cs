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
    [Table(TableNames.YD_DYEING_BATCH_REWORK_RECIPE)]
    public class YDDyeingBatchReworkRecipe : DapperBaseEntity
    {
        public YDDyeingBatchReworkRecipe()
        {
            YDDBRRID = 0;
            DBatchID = 0;
            DBRID = 0;
            YDRecipeReqMasterID = 0;
        }
        public int YDDBRRID { get; set; }
        public int DBatchID { get; set; }
        public int DBRID { get; set; }
        public int YDRecipeReqMasterID { get; set; }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDDBRRID > 0;
        #endregion
    }
}
