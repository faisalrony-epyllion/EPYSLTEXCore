using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using FluentValidation;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.RND_KNITTING_PRODUCTION_QC)]
    public class KnittingProductionQC : BaseEntity
    {
        #region Table Properties

        [ExplicitKey]
        public int KQCID { get; set; }

        public int GRollID { get; set; }
        public int GQCParameterID { get; set; }
        public decimal ParameterValue { get; set; }
        public decimal AllowancePercentage { get; set; }
        public decimal QCValue { get; set; }

        #endregion Table Properties

        #region Additional Properties
        public int GRollId { get; set; }
        public int GQCParameterId { get; set; }
        public string QCParameterName { get; set; }
        #endregion

        public KnittingProductionQC()
        {
            ParameterValue = 0m;
            AllowancePercentage = 0m;
            QCValue = 0m;
        }
    }

    #region Validator
    public class KnittingProductionQCValidator : AbstractValidator<KnittingProductionQC>
    {
        public KnittingProductionQCValidator()
        {
            RuleFor(x => x.QCValue).NotEmpty();
        }
    }
    #endregion
}
