using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_RECIPE_DEFINITION_CHILD)]
    public class YDRecipeDefinitionChild : DapperBaseEntity
    {
        public YDRecipeDefinitionChild()
        {
            Unit = "";
            DefChilds = new List<YDRecipeDefinitionChild>();
            RawItemID = 0;
        }

        [ExplicitKey]
        public int YDRecipeChildID { get; set; }

        public int YDRecipeId { get; set; }

        public int ProcessID { get; set; }

        public int ParticularsID { get; set; }

        public int RawItemID { get; set; }

        public decimal Qty { get; set; }

        public int UnitID { get; set; }

        public decimal TempIn { get; set; }

        public decimal TempOut { get; set; }

        public decimal ProcessTime { get; set; }

        public int YDRecipeDInfoID { get; set; }

        public decimal Temperature { get; set; }

        public bool IsPercentage { get; set; }

        #region Additional
        [Write(false)]
        public int DBRID { get; set; }

        [Write(false)]
        public int YDDBatchID { get; set; }

        [Write(false)]
        public string IsPercentageText => this.IsPercentage ? "Yes" : "No";

        [Write(false)]
        public string Unit { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDRecipeChildID > 0;

        [Write(false)]
        public string ItemName { get; set; }

        [Write(false)]
        public string ParticularsName { get; set; }

        [Write(false)]
        public string ProcessName { get; set; }

        [Write(false)]
        public string RawItemName { get; set; }

        [Write(false)]
        public string Uom { get; set; }

        [Write(false)]
        public string FiberPart { get; set; }

        [Write(false)]
        public string ColorName { get; set; }

        [Write(false)]
        public int YDBRecipeChildID { get; set; } = 0;
        [Write(false)]
        public int YDBatchID { get; set; } = 0;
        [Write(false)]
        public int YDDBRID { get; set; } = 0;
        [Write(false)]
        public List<YDRecipeDefinitionChild> DefChilds { get; set; }

        #endregion Additional


    }
}
