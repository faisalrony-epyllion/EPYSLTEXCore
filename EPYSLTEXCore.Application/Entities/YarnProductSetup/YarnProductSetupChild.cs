using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;


namespace EPYSLTEXCore.Application.Entities
{
    [Table("YarnProductSetupChild")]
    public class YarnProductSetupChild: DapperBaseEntity
    { 
        public string SetupMasterID { get; set; }
        public string? BlendTypeID { get; set; }  // Nullable as it's `null` in the JSON
        public string YarnTypeID { get; set; }
        public string ProgramID { get; set; }
        public string SubProgramID { get; set; }
        public string CertificationsID { get; set; }
        public string TechnicalParameterID { get; set; }
        public string CompositionsID { get; set; }
        public string ShadeID { get; set; }
        public string ManufacturingLineID { get; set; }
        public string ManufacturingProcessID { get; set; }
        public string ManufacturingSubProcessID { get; set; }
        public string YarnColorID { get; set; }
        public string ColorGradeID { get; set; }
        public string Text { get; set; }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified;
        #endregion Additional Properties
    }

    [Table("YarnProductSetup")]
    public class YarnProductSetup : DapperBaseEntity
    {
  
        public string SetupID { get; set; }
        public string YarnFiberTypeID { get; set; }
        public List<YarnProductSetupChild> Childs { get; set; }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified;
        #endregion Additional Properties
    }

 

 

     
}

