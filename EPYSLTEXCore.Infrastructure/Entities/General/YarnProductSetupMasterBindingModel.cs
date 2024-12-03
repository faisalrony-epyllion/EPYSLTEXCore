namespace EPYSLTEXCore.Infrastructure.Entities.General
{
    public class YarnProductSetupMasterBindingModel : BaseModel
    {
        public int FiberTypeID { get; set; }
        public List<YarnProductSetupChildBindingModel> Childs { get; set; }
        public List<YarnProductSetupChildProgramBindingModel> ChildPrograms { get; set; }
        public List<YarnProductSetupChildTechnicalParameterBindingModel> ChildTechnicalParameters { get; set; }

        public YarnProductSetupMasterBindingModel()
        {
            Childs = new List<YarnProductSetupChildBindingModel>();
            ChildPrograms = new List<YarnProductSetupChildProgramBindingModel>();
            ChildTechnicalParameters = new List<YarnProductSetupChildTechnicalParameterBindingModel>();
        }
    }
}
