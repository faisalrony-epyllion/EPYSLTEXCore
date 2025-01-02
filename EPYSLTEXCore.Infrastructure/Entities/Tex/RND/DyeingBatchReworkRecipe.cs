using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.DYEING_BATCH_REWORK_RECIPE)]
    public class DyeingBatchReworkRecipe : DapperBaseEntity
    {
        public DyeingBatchReworkRecipe()
        {
            DBRRID = 0;
            DBatchID = 0;
            DBRID = 0;
            RecipeReqMasterID = 0;
        }
        public int DBRRID { get; set; }
        public int DBatchID { get; set; }
        public int DBRID { get; set; }
        public int RecipeReqMasterID { get; set; }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || DBRRID > 0;
        #endregion
    }
}