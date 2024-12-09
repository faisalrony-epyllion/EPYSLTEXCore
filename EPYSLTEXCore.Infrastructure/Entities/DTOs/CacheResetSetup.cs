using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.DTOs
{
    public class CacheResetSetup
    {
        public int CacheID { get; set; }
        public string CacheKey { get; set; } = "";
        public string CacheDetails { get; set; } = "";
        public string ApiName { get; set; } = "";
        public string ParameterValue { get; set; } = "";
        public bool AutoRefresh { get; set; } = false;
        public string Interval { get; set; } = "";
        public int IntervalValue { get; set; } = 0;
    }
}
