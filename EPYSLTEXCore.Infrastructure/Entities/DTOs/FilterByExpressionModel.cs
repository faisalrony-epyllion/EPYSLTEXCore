using System.Collections.Generic;

namespace EPYSLTEXCore.Infrastructure.DTOs
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
