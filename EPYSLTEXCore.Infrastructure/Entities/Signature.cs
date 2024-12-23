﻿using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    [Table("Signature")]
    public class Signatures : DapperBaseEntity
    {
        [ExplicitKey]
        public string Field { get; set; }

        [ExplicitKey]
        public DateTime Dates { get; set; }

        public decimal LastNumber { get; set; }

        [ExplicitKey]
        public string CompanyID { get; set; }

        [ExplicitKey]
        public string SiteID { get; set; }

        public Signatures()
        {
            LastNumber = 1m;
            CompanyID = "1";
            SiteID = "1";
        }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || Field != "";
        #endregion Additional Properties

    }
}
