using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class DbNetEditRequest : DbNetSuiteRequest
    {
        public string FromPart { get; set; }
        public bool Navigation { get; set; }
        public bool QuickSearch { get; set; }
        public bool Search { get; set; }
    }
}