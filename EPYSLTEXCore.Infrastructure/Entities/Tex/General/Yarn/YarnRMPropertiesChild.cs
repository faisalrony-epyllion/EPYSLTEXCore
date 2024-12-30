using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn
{
    [Table(TableNames.YarnRMPropertiesChild)]
    public class YarnRMPropertiesChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int YRMPChildID { get; set; }
        public int YRMPID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public int SpinnerID { get; set; } = 0;

        #region Additional Properties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YRMPChildID > 0;
        [Write(false)]
        public string Supplier { get; set; } = "";
        [Write(false)]
        public string Spinner { get; set; } = "";

        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> SpinnerList { get; set; } = new List<Select2OptionModel>();

        #endregion
    }
}
