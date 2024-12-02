using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.CustomeAttribute;
using EPYSLTEXCore.Infrastructure.Data;


namespace EPYSLTEXCore.Infrastructure.Entities
{
    [Table("YarnProductSetupChild")]
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
 

    [Table("YarnProductSetupMaster")]
    public class YarnProductSetup : DapperBaseEntity
    {

        [ExplicitKey]
        public int SetupMasterID { get; set; }
        public int? FiberTypeID { get; set; }
        public int? AddedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        


        [Write(false)]
        [ChildEntity]
        public List<YarnProductSetupChild> Childs { get; set; }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified;
        #endregion Additional Properties
    }






}

