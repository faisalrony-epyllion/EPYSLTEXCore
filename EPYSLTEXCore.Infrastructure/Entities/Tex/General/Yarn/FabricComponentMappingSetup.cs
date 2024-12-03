using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn
{
    [Table("T_FabricComponentMappingSetup")]
    public class FabricComponentMappingSetup : DapperBaseEntity
    {
        [ExplicitKey]
        public int SetupID { get; set; }
        public int FiberID { get; set; }
        public int SubProgramID { get; set; }
        public int CertificationsID { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || SetupID > 0;
        [Write(false)]
        public string Fiber { get; set; }
        [Write(false)]
        public string SubProgram { get; set; }
        [Write(false)]
        public string Certifications { get; set; }

        public FabricComponentMappingSetup()
        {
            SetupID = 0;
        }
    }
}
