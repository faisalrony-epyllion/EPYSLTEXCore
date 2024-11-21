using System.Collections.Generic;

namespace EPYSLTEX.Core.DTOs
{
    public class FilterByExpressionModel
    {
        public FilterByExpressionModel()
        {
            Parameters = new List<string>();
        }
        public string Expression { get; set; }
        public List<string> Parameters { get; set; }
    }
}
