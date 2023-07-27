using DbNetSuiteCore.Enums;
using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class DbNetGridEditRequest : DbNetSuiteRequest
    {
        public bool Delete { get; set; } = false;
        public string FromPart { get; set; }
        public int LookupColumnIndex { get; set; }
        public bool Insert { get; set; } = false;
        public bool Navigation { get; set; } = true;
        public bool OptimizeForLargeDataset { get; set; }
        public ParentChildRelationship? ParentChildRelationship { get; set; }
        public bool QuickSearch { get; set; } = false;
        public string QuickSearchToken { get; set; }
        public bool Search { get; set; } = true;
        public string SearchFilterJoin { get; set; }
        public List<SearchParameter> SearchParams { get; set; } = new List<SearchParameter>();
    }
}