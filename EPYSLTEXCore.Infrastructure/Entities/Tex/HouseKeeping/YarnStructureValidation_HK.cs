using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System.Collections.Generic;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.HouseKeeping
{
    [Table("YarnStructureValidation_HK")]
    public class YarnStructureValidation_HK : DapperBaseEntity
    {
        #region Table Properties
        [ExplicitKey]
        public int SetupID { get; set; }
        public int YarnTypeID { get; set; }
        public int MProcessID { get; set; }
        public int MSubProcessID { get; set; }
        public int QualityParamID { get; set; }
        public string CountUnit { get; set; }
        #endregion

        #region Additional Properties
        [Write(false)]
        public string YarnType { get; set; }
        [Write(false)]
        public string MProcessName { get; set; }
        [Write(false)]
        public string MSubProcessName { get; set; }
        [Write(false)]
        public string QualityParamName { get; set; }
        [Write(false)]
        public List<YarnStructureValidation_HK> Childs { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || SetupID > 0;
        #endregion
        public YarnStructureValidation_HK()
        {
            SetupID = 0;
            YarnTypeID = 0;
            MProcessID = 0;
            MSubProcessID = 0;
            QualityParamID = 0;
            CountUnit = "";
            Childs = new List<YarnStructureValidation_HK>();
        }
    }
}
