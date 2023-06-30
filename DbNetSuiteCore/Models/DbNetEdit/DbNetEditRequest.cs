using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class DbNetEditRequest : DbNetSuiteRequest
    {
        public List<EditColumn> Columns { get; set; } = new List<EditColumn>();
        public string FromPart { get; set; }
        public bool Navigation { get; set; }
        public bool QuickSearch { get; set; }
        public bool Search { get; set; }
    }
}