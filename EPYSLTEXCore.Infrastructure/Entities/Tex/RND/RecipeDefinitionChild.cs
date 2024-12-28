using Dapper.Contrib.Extensions;
using FluentValidation;
using System.Collections.Generic;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("RecipeDefinitionChild")]
    public class RecipeDefinitionChild : DapperBaseEntity
    {
        public RecipeDefinitionChild()
        {
            Unit = "";
            DefChilds = new List<RecipeDefinitionChild>();
            RawItemId = 0;
        }

        [ExplicitKey]
        public int RecipeChildID { get; set; }

        public int RecipeId { get; set; }

        public int ProcessId { get; set; }

        public int ParticularsId { get; set; }

        public int RawItemId { get; set; }

        public decimal Qty { get; set; }

        public int UnitID { get; set; }

        public decimal TempIn { get; set; }

        public decimal TempOut { get; set; }

        public decimal ProcessTime { get; set; }

        public int RecipeDInfoID { get; set; }

        public decimal Temperature { get; set; }

        public bool IsPercentage { get; set; }

        #region Additional
        [Write(false)]
        public int DBRID { get; set; }

        [Write(false)]
        public int DBatchID { get; set; }

        [Write(false)]
        public string IsPercentageText => this.IsPercentage ? "Yes" : "No";

        [Write(false)]
        public string Unit { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || RecipeChildID > 0;

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
        public List<RecipeDefinitionChild> DefChilds { get; set; }

        #endregion Additional


    }

    public class RecipeDefinitionChildValidator : AbstractValidator<RecipeDefinitionChild>
    {
        public RecipeDefinitionChildValidator()
        {
            RuleFor(x => x.ParticularsId).NotEmpty().WithMessage("Particulars must not be empty!"); 
            //RuleFor(x => x.Qty).NotEmpty().When(x => x.ParticularsName != "Water").WithMessage("Qty must not be empty!");
            //RuleFor(x => x.RawItemId).NotEmpty().When(x => x.ParticularsName != "Water").WithMessage("Please select item");
            //RuleFor(x => x.UnitID).NotEmpty().When(x => x.IsPercentage == false).WithMessage("Please select unit");
        }
    }

}