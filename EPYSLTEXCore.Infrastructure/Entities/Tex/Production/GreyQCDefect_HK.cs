using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Production
{
    public class GreyQCDefect_HK : DapperBaseEntity
    {
        public GreyQCDefect_HK()
        {
            QCDefectID = 0;
            DefactName = "";
            DefactGroupName = "";
            DefectInputType = "";
            PointPerDefect = 0;
            ZeroTollarance = false;
        }
        [ExplicitKey]
        public int QCDefectID { get; set; }
        public string DefactName { get; set; }
        public string DefactGroupName { get; set; }
        public string DefectInputType { get; set; }
        public int PointPerDefect { get; set; }
        public bool ZeroTollarance { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCDefectID > 0;
    }
}
