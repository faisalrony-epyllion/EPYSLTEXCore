using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Admin
{
    [Table(TableNames.BankLimitChild)]
    public class BankLimitChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int BankLimitChildID { get; set; } = 0;
        public int BankLimitMasterID { get; set; } = 0;
        public int FormBankFacilityID { get; set; } = 0;
        public int LiabilityTypeID { get; set; } = 0;
        public int FromTenureDay { get; set; } = 0;
        public int ToTenureDay { get; set; } = 0;
        public decimal MaxLimit { get; set; } = 0;
        public decimal LCOpened { get; set; } = 0;
        public decimal LCAcceptenceGiven { get; set; } = 0;
        public decimal PaymentOnMaturity { get; set; } = 0;

        #region Additional Property

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.BankLimitChildID > 0;
        [Write(false)]
        public string FormBankFacilityName { get; set; } = "";
        [Write(false)]
        public string LiabilityTypeName { get; set; } = "";

        #endregion Additional Property
    }
}
