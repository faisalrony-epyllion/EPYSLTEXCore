namespace EPYSLTEXCore.Infrastructure.Entities.General
{
    public class YarnProductSetupChildBindingModel : BaseModel
    {
        public YarnProductSetupChildBindingModel()
        {
            CountIDs = "";
        }
        public int SetupMasterID { get; set; }
        public int BlendTypeID { get; set; }
        public int YarnTypeID { get; set; }
        public int ProgramID { get; set; }
        public int SubProgramID { get; set; }
        public int CertificationsID { get; set; }
        public int TechnicalParameterID { get; set; }
        public int CompositionsID { get; set; }
        public int ShadeID { get; set; }
        public int ManufacturingLineID { get; set; }
        public int ManufacturingProcessID { get; set; }
        public int ManufacturingSubProcessID { get; set; }
        public int YarnColorID { get; set; }
        public int ColorGradeID { get; set; }
        public string CountIDs { get; set; }
        public string Counts { get; set; }
        public List<YarnProductSetupChildCountBindingModel> YarnProductSetupChildCounts { get; set; }
    }
}
