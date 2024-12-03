using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    public class YarnProductSetupFinder : DapperBaseEntity
    {
        public int SetupMasterID { get; set; }
        public int FiberTypeID { get; set; }
        public string FiberType { get; set; }

        public override bool IsModified => false;
    }
}
