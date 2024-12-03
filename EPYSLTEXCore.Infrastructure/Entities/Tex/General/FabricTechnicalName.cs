using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table("FabricTechnicalName")]
    public class FabricTechnicalName : DapperBaseEntity
    {
        [ExplicitKey]
        public int TechnicalNameId { get; set; }

        public string TechnicalName { get; set; }
       public int ConstructionId { get; set; }

        public string ConstructionName { get; set; }


        #region Additional Columns
        [Write(false)] 
        public int SubClassID { get; set; }
        [Write(false)]
        public string SubClassName { get; set; }
        [Write(false)] 
        public int StructureTypeID { get; set; }
        [Write(false)]
        public string StructureTypeName { get; set; }
        [Write(false)]
        public bool isModified { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> ConstructionList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> SubClassList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> StructureTypeList { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.TechnicalNameId > 0;
       
        #endregion Additional Columns
        //public FabricTechnicalName()
        //{
        //    EntityState = EntityState.Added;
        //}
    }
}
