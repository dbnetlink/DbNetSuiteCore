using DbNetSuiteCore.Enums;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class DbNetGridEditRequest : DbNetSuiteRequest
    {
        public string ColumnName { get; set; }
        public bool Delete { get; set; } = false;
        public Dictionary<string, object> FixedFilterParams { get; set; } = new Dictionary<string, object>();
        public string FixedFilterSql { get; set; }
        public string FromPart { get; set; }
        public int LookupColumnIndex { get; set; }
        public bool Insert { get; set; } = false;
        public int MaxImageHeight { get; set; }
        public bool Navigation { get; set; } = true;
        public string InitialOrderBy { get; set; }
        public bool OptimizeForLargeDataset { get; set; }
        public ParentChildRelationship? ParentChildRelationship { get; set; }
        public string PrimaryKey { get; set; }
        public bool QuickSearch { get; set; } = false;
        public bool Search { get; set; } = true;
        public string SearchFilterJoin { get; set; }
        public List<SearchParameter> SearchParams { get; set; } = new List<SearchParameter>();
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; }
        public string JsonKey { get; set; } = string.Empty;
        public JArray Json { get; set; }
    }
}