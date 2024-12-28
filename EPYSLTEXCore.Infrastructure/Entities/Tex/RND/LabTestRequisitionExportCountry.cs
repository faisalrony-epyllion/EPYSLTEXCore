using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Statics;
using FluentValidation;
using System;
using System.Collections.Generic;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("LabTestRequisitionExportCountry")]
    public class LabTestRequisitionExportCountry : DapperBaseEntity
    {
        public LabTestRequisitionExportCountry()
        {
            LTRECountryID = 0;
            LTReqBuyerID = 0;
            LTReqMasterID = 0;
            CountryRegionID = 0;
            SeqNo = 0;

            RegionName = "";
        }

        [ExplicitKey]
        public int LTRECountryID { get; set; }
        public int LTReqBuyerID { get; set; }
        public int LTReqMasterID { get; set; }
        public int CountryRegionID { get; set; }
        public int SeqNo { get; set; }

        #region Additional Props
        [Write(false)]
        public string RegionName { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LTRECountryID > 0;
        #endregion
    }
}
