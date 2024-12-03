namespace EPYSLTEXCore.Infrastructure.Entities.General
{
    public class YarnProductSetupChildProgramBindingModel : BaseModel
    {
        public int SetupMasterID { get; set; }
        public int ProgramID { get; set; }
        public int SubProgramID { get; set; }
        public int CertificationsID { get; set; }
    }
}
