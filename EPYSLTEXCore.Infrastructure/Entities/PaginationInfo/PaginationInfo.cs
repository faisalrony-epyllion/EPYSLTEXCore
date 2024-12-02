using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Entities.PaginationInfo
{
    public class PaginationInfo
    {
        public string GridType { get; set; }
        public string PageBy { get; set; }
        public string FilterBy { get; set; }
        public string OrderBy { get; set; }
        public string PageByNew { get; set; }
        public PaginationInfo()
        {
            PageBy = "";
            FilterBy = "";
            OrderBy = "";
            PageByNew = "";
        }
    }
}
