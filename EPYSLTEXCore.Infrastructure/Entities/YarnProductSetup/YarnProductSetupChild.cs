using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.CustomeAttribute;
using EPYSLTEXCore.Infrastructure.Data;


namespace EPYSLTEXCore.Infrastructure.Entities
{
    [Table(TableNames.YARN_PRODUCT_SETUP_CHILD)]
    public class YarnProductSetupChild : DapperBaseEntity
    {
        [ExplicitKey]

        public int SetupChildID { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("SetupMasterID")] // This indicates that UserId is a foreign key referring to the User table
        public int SetupMasterID { get; set; }

        public int? BlendTypeID { get; set; }  // Nullable as it's `null` in the JSON
        public int? YarnTypeID { get; set; }
        public int? ProgramID { get; set; }
        public int? SubProgramID { get; set; }
        public int? CertificationsID { get; set; }
        public int? TechnicalParameterID { get; set; }
        public int? CompositionsID { get; set; }
        public int? ShadeID { get; set; }
        public int? ManufacturingLineID { get; set; }
        public int? ManufacturingProcessID { get; set; }
        public int? ManufacturingSubProcessID { get; set; }
        public int? YarnColorID { get; set; }
        public int? ColorGradeID { get; set; }
         

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified;
        #endregion Additional Properties
    }
 






}

