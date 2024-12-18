﻿using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Admin
{
    [Table(TableNames.BankLimitMaster)]
    public class BankLimitMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int BankLimitMasterID { get; set; } = 0;
        public int CompanyID { get; set; } = 0;
        public int CurrencyID { get; set; } = 0;
        public int BankID { get; set; } = 0;
        public int BankFacilityTypeID { get; set; } = 0;
        public decimal AccumulatedLimit { get; set; } = 0;
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }

        #region Additional Property

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.BankLimitMasterID > 0;
        [Write(false)]
        public List<BankLimitChild> Childs { get; set; } = new List<BankLimitChild>();
        [Write(false)]
        public string CompanyName { get; set; } = "";
        [Write(false)]
        public string CurrencyName { get; set; } = "";
        [Write(false)]
        public string BankName { get; set; } = "";
        [Write(false)]
        public string BankFacilityTypeName { get; set; } = "";
        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> CurrencyList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> BankList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> BankFacilityTypeList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> FormBankFacilityList { get; set; } = Enumerable.Empty<Select2OptionModel>();
        [Write(false)]
        public IEnumerable<Select2OptionModel> LiabilityTypeList { get; set; } = Enumerable.Empty<Select2OptionModel>();

        [Write(false)]
        public int BankLimitChildID { get; set; } = 0;
        [Write(false)]
        public int FromTenureDay { get; set; } = 0;
        [Write(false)]
        public int ToTenureDay { get; set; } = 0;
        [Write(false)]
        public decimal MaxLimit { get; set; } = 0;
        [Write(false)]
        public decimal LCOpened { get; set; } = 0;
        [Write(false)]
        public decimal LCAcceptenceGiven { get; set; } = 0;
        [Write(false)]
        public decimal PaymentOnMaturity { get; set; } = 0;

        [Write(false)]
        public string FormBankFacilityName { get; set; } = "";
        [Write(false)]
        public string LiabilityTypeName { get; set; } = "";


        #endregion Additional Property
    }
}
