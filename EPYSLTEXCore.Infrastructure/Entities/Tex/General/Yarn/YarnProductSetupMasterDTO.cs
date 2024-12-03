using EPYSLTEXCore.Infrastructure.Entities.General;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn
{
    public class YarnProductSetupMasterDTO : YarnProductSetupMasterBindingModel
    {
        public string FiberType { get; set; }

        public IEnumerable<Select2OptionModel> FiberTypeList { get; set; }
        public IEnumerable<Select2OptionModel> BlendTypeList { get; set; }
        public IEnumerable<Select2OptionModel> YarnTypeList { get; set; }
        public IEnumerable<Select2OptionModel> YarnProgramList { get; set; }
        public IEnumerable<Select2OptionModel> YarnSubProgramList { get; set; }
        public IEnumerable<Select2OptionModel> CertificationsList { get; set; }
        public IEnumerable<Select2OptionModel> TechnicalParameterList { get; set; }

        public IEnumerable<Select2OptionModel> CompositionsList { get; set; }
        public IEnumerable<Select2OptionModel> ShadeList { get; set; }
        public IEnumerable<Select2OptionModel> ManufacturingLineList { get; set; }
        public IEnumerable<Select2OptionModel> ManufacturingProcessList { get; set; }
        public IEnumerable<Select2OptionModel> ManufacturingSubProcessList { get; set; }
        public IEnumerable<Select2OptionModel> YarnColorList { get; set; }
        public IEnumerable<Select2OptionModel> ColorGradeList { get; set; }
        public IEnumerable<Select2OptionModel> CountList { get; set; }

        public new List<YarnProductSetupChildDTO> Childs { get; set; }
        public new List<YarnProductSetupChildProgramDTO> ChildPrograms { get; set; }
        public new List<YarnProductSetupChildTechnicalParameterDTO> ChildTechnicalParameters { get; set; }
        public List<YarnProcessSetupMasterDTO> ProcessSetupList { get; set; }
    }

    public class YarnProductSetupChildDTO : YarnProductSetupChildBindingModel
    {
        public string BlendType { get; set; }
        public string YarnType { get; set; }
        public string Program { get; set; }
        public string SubProgram { get; set; }
        public string Certifications { get; set; }
        public string TechnicalParameter { get; set; }
        public string Compositions { get; set; }
        public string Shade { get; set; }
        public string ManufacturingLine { get; set; }
        public string ManufacturingProcess { get; set; }
        public string ManufacturingSubProcess { get; set; }
        public string YarnColor { get; set; }
        public string ColorGrade { get; set; }
        public string CountNames { get; set; }

    }

    public class YarnProductSetupChildProgramDTO : YarnProductSetupChildProgramBindingModel
    {
        public string Program { get; set; }
        public string SubProgram { get; set; }
        public string Certifications { get; set; }
    }

    public class YarnProductSetupChildTechnicalParameterDTO : YarnProductSetupChildTechnicalParameterBindingModel
    {
        public string TechnicalParameter { get; set; }
    }
}
