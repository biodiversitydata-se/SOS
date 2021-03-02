using System.Collections.Generic;

namespace SOS.Lib.Models.ApplicationInsights
{
    public class ApplicationInsightsQueryResponse
    {
        public class Table
        {
            public string Name { get; set; }
            public IList<IList<object>> Rows { get; set; }
        }
        public Table[] Tables { get; set; }
    }
}
