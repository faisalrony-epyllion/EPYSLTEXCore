using Dapper.Contrib.Extensions;

namespace EPYSLTEX.Core.DTOs
{
    public class Select2OptionModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public string desc { get; set; }
        public string additionalValue { get; set; }
    }
}
