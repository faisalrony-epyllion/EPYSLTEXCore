using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn
{
    [Table(TableNames.FIBER_SUBPROGRAM_CERTIFICATIONS_FILTER_SETUP)]
    public class FabricComponentMappingSetup : DapperBaseEntity
    {
        [ExplicitKey]
        public int SetupID { get; set; } = 0;
        public int FiberID { get; set; } = 0;
        public int SubProgramID { get; set; } = 0;
        public int CertificationsID { get; set; } = 0;
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; } = 0;
        public DateTime DateUpdated { get; set; }


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || SetupID > 0;
        [Write(false)]
        public string Fiber { get; set; } = "";
        [Write(false)]
        public string SubProgram { get; set; } = "";
        [Write(false)]
        public string Certifications { get; set; } = "";

        public FabricComponentMappingSetup()
        {
            SetupID = 0;
        }
    }
}
