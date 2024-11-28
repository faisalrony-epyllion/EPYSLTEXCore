using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Entities
{
    public class YarnProductSetupFinder   
    {
        public int SetupMasterID { get; set; }
        public int FiberTypeID { get; set; }
        public string FiberType { get; set; }
 
 

    }
}
