


using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.CustomeAttribute;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    [Table(TableNames.YARN_PRODUCT_SETUP_MASTER)]
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
        public string FiberType { get; set; }

        [Write(false)]
        [ChildEntity]
        public List<YarnProductSetupChild> Childs { get; set; }

        #region Additional Properties
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified;
        #endregion Additional Properties
    }
}
