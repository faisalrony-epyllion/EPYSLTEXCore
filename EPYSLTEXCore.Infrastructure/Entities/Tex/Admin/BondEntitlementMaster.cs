using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;


namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Admin
{
    [Table(TableNames.BondEntitlementMaster)]
    public class BondEntitlementMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int BondEntitlementMasterID { get; set; } = 0;
        public int CompanyID { get; set; } = 0;
        public string BondLicenceNo { get; set; } = "";
        public string EBINNo { get; set; } = "";
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }

        #region Additional Property

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.BondEntitlementMasterID > 0;
        [Write(false)]
        public List<BondEntitlementChild> Childs { get; set; } = new List<BondEntitlementChild>();
        [Write(false)]
        public string CompanyName { get; set; } = "";
        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> CurrencyList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> UnitList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> SubGroups { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> Dyes { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> Chemicals { get; set; } = Enumerable.Empty<Select2OptionModel>();

        #endregion Additional Property
    }
}
