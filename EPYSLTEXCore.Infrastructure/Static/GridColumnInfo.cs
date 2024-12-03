using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Static
{
    public class GridColumnInfo
    {
        public String field { get; set; } = "";
        public String headerText { get; set; } = "";
        public bool visible { get; set; } = false;
        public bool allowEditing { get; set; } = false;
        public String editType { get; set; } = "";
        public int width { get; set; } = 0;
        public bool displayAsCheckBox { get; set; } = false;
        public string textAlign { get; set; } = "";
    }
}
