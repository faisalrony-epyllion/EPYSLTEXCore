using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{

    [Table(TableNames.LAB_TEST_REQUISITION_EXPORT_COUNTRY)]
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
