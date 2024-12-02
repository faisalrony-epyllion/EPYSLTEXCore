using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.CustomeAttribute;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities
{
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
