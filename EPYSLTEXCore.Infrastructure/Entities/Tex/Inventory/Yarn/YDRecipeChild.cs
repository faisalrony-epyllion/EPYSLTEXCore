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
    [Table(TableNames.YARN_DYEING_RECIPE_CHILD)]
    public class YDRecipeChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDRecipeChildID { get; set; }

        ///<summary>
        /// YDRecipeMasterID
        ///</summary>
        public int YDRecipeMasterID { get; set; }

        ///<summary>
        /// ProcessID
        ///</summary>
        public int ProcessID { get; set; }

        ///<summary>
        /// ParticularsID
        ///</summary>
        public int ParticularsID { get; set; }

        ///<summary>
        /// RawItemID
        ///</summary>
        public int RawItemID { get; set; }

        ///<summary>
        /// TempIn
        ///</summary>
        public decimal TempIn { get; set; }

        ///<summary>
        /// TempOut
        ///</summary>
        public decimal TempOut { get; set; }

        ///<summary>
        /// ProcessTime
        ///</summary>
        public decimal ProcessTime { get; set; }

        ///<summary>
        /// IsPercentage
        ///</summary>
        public bool IsPercentage { get; set; }

        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// Qty
        ///</summary>
        public int Qty { get; set; }

        #region Additional Columns

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string ProcessName { get; set; }

        [Write(false)]
        public string ParticularsName { get; set; }

        [Write(false)]
        public string RawItemName { get; set; }

        [Write(false)]
        public string Unit { get; set; }

        [Write(false)]
        public string IsPercentageText => this.IsPercentage ? "Yes" : "No";

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDRecipeChildID > 0;

        #endregion Additional Columns

        public YDRecipeChild()
        {
            IsPercentage = false;
            Qty = 0;
            ProcessID = 0;
        }
    }
}
