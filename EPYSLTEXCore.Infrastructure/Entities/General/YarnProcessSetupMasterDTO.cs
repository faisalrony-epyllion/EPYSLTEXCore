namespace EPYSLTEXCore.Infrastructure.Entities.General
{
    public class YarnProcessSetupMasterDTO : YarnProcessSetupMasterBindingModel
    {
        public string ManufacturingLine { get; set; }
        public string ManufacturingProcess { get; set; }
        public string ManufacturingSubProcess { get; set; }
        public string YarnColor { get; set; }
        public string ColorGrade { get; set; }

        public IEnumerable<Select2OptionModel> ManufacturingLineList { get; set; }
        public IEnumerable<Select2OptionModel> ManufacturingProcessList { get; set; }
        public IEnumerable<Select2OptionModel> ManufacturingSubProcessList { get; set; }
        public IEnumerable<Select2OptionModel> YarnCompositionList { get; set; }
        public IEnumerable<Select2OptionModel> YarnColorList { get; set; }
        public IEnumerable<Select2OptionModel> ColorGradeList { get; set; }
        public IEnumerable<Select2OptionModel> CountList { get; set; }
        public IEnumerable<Select2OptionModel> FiberTypeList { get; set; }
    }
}
