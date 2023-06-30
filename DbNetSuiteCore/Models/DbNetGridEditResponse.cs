using System.Collections.Generic;

namespace DbNetSuiteCore.Models
{
    public class DbNetGridEditResponse : DbNetSuiteResponse
    {
        public Dictionary<string, object> Record { get; set; }
        public List<SearchParameter> SearchParams { get; set; }
    }
}