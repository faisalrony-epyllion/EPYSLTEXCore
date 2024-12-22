using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn
{
    [Table(TableNames.YarnPropertiesMapping)]
    public class YarnPropertiesMapping : DapperBaseEntity
    {
        [ExplicitKey]
        public int YarnPropertiesMappingID { get; set; } = 0;
        public int FiberTypeID { get; set; } = 0;
        public int BlendTypeID { get; set; } = 0;
        public int YarnTypeID { get; set; } = 0;
        public int ProgramID { get; set; } = 0;
        public int SubProgramID { get; set; } = 0;
        public int CertificationID { get; set; } = 0;
        public int TechnicalParameterID { get; set; } = 0;
        public int YarnCompositionID { get; set; } = 0;
        public int ShadeReferenceID { get; set; } = 0;
        public int ManufacturingLineID { get; set; } = 0;
        public int ManufacturingProcessID { get; set; } = 0;
        public int ManufacturingSubProcessID { get; set; } = 0;
        public int ColorID { get; set; } = 0;
        public int ColorGradeID { get; set; } = 0;
        public int YarnCountID { get; set; } = 0;
        public int AddedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; } = 0;
        public DateTime? DateUpdated { get; set; }

        #region Additional Properties

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YarnPropertiesMappingID > 0;
        [Write(false)]
        public string FiberType { get; set; } = "";
        [Write(false)]
        public string BlendType { get; set; } = "";
        [Write(false)]
        public string YarnType { get; set; } = "";
        [Write(false)]
        public string Program { get; set; } = "";
        [Write(false)]
        public string SubProgram { get; set; } = "";
        [Write(false)]
        public string Certification { get; set; } = "";
        [Write(false)]
        public string Certifications { get; set; } = "";
        [Write(false)]
        public string TechnicalParameter { get; set; } = "";
        [Write(false)]
        public string YarnComposition { get; set; } = "";
        [Write(false)]
        public string ShadeReference { get; set; } = "";
        [Write(false)]
        public string ManufacturingLine { get; set; } = "";
        [Write(false)]
        public string ManufacturingProcess { get; set; } = "";
        [Write(false)]
        public string ManufacturingSubProcess { get; set; } = "";
        [Write(false)]
        public string Color { get; set; } = "";
        [Write(false)]
        public string ColorGrade { get; set; } = "";
        [Write(false)]
        public string YarnCount { get; set; } = "";

        [Write(false)]
        public List<Select2OptionModel> FiberTypeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> BlendTypeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> YarnTypeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> ProgramList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> SubProgramList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> CertificationList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> TechnicalParameterList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> YarnCompositionList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> ShadeReferenceList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> ManufacturingLineList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> ManufacturingProcessList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> ManufacturingSubProcessList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> ColorList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> ColorGradeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> YarnCountList { get; set; } = new List<Select2OptionModel>();

        #endregion
    }
}
