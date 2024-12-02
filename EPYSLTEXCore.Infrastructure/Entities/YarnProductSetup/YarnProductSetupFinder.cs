using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    public class YarnProductSetupFinder : DapperBaseEntity
    {
        public int SetupMasterID { get; set; }
        public int FiberTypeID { get; set; }
        public string FiberType { get; set; }

        public override bool IsModified => false;
    }
}
