namespace EPYSLTEXCore.Infrastructure.DTOs
{
    public class Select2MappingOptionModel
    {
        public int SegmentValueMappingID { get; set; }
        public int YarnTypeSVID { get; set; }
        public int ManufacturingProcessSVID { get; set; }
        public int SubProcessSVID { get; set; }
        public int QualityParameterSVID { get; set; }
        public int CountSVID { get; set; }
        public bool IsInactive { get; set; }
        public string YarnType { get; set; }
        public string ManufacturingProcess { get; set; }
        public string SubProcess { get; set; }
        public string QualityParameter { get; set; }
        public string Count { get; set; }
        public int FiberID { get; set; }
        public int SubProgramID { get; set; }
        public int CertificationsID { get; set; }
        public string Fiber { get; set; }
        public string SubProgram { get; set; }
        public string Certifications { get; set; }
        public string id { get; set; }
        public string text { get; set; }
        public string desc { get; set; }
    }
}
