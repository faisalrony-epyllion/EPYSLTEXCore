using EPYSLTEXCore.Infrastructure.Entities.Gmt.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.DTOs
{
    public class FabricColorBookSetupDTO : FabricColorBookSetup
    {
        public string ColorName { get; set; }
        public IEnumerable<Select2OptionModel> ColorList { get; set; }
    }
}
