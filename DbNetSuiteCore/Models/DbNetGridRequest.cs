using DbNetSuiteCore.Enums;
using System.Collections.Generic;

namespace DbNetSuiteCore.Models
{
    public class DbNetGridRequest
    {
        public BooleanDisplayMode BooleanDisplayMode { get; set; }
        public Dictionary<string, string> ColumnFilters { get; set; } = new Dictionary<string, string>();
        public GridColumnCollection Columns { get; set; } = new GridColumnCollection();   
        public string ColumnName { get; set; }
        public string ComponentId { get; set; }
        public string ConnectionString { get; set; }
        public string ConnectionType { get; set; }
        public bool Copy { get; set; }
        public string Culture { get; set; }
        public int CurrentPage { get; set; }
        public GridColumn DefaultColumn { get; set; }
        public string Extension { get; set; }
        public bool Export { get; set; }
        public Dictionary<string, object> FixedFilterParams { get; set; } = new Dictionary<string, object>();
        public string FixedFilterSql { get; set; }
        public string FromPart { get; set; }
        public bool FrozenHeader { get; set; }
        public GridGenerationMode GridGenerationMode { get; set; } = GridGenerationMode.Display;
        public bool GroupBy { get; set; }
        public int LookupColumnIndex { get; set; }
        public bool MultiRowSelect { get; set; }
        public MultiRowSelectLocation MultiRowSelectLocation { get; set; }
        public bool Navigation { get; set; }
        public bool NestedGrid { get; set; }
        public bool OptimizeForLargeDataset { get; set; }
        public int? OrderBy { get; set; }
        public OrderByDirection OrderByDirection { get; set; }
        public long PageSize { get; set; }
        public string PrimaryKey { get; set; }
        public string ProcedureName { get; set; } = string.Empty;
        public Dictionary<string, object> ProcedureParams { get; set; } = new Dictionary<string, object>();
        public bool QuickSearch { get; set; }
        public string QuickSearchToken { get; set; }
        public bool Search { get; set; }
        public string SearchFilterJoin { get; set; }
        public List<SearchParameter> SearchParams { get; set; } = new List<SearchParameter>();

        public ToolbarButtonStyle ToolbarButtonStyle { get; set; }
        public bool View { get; set; }
    }
}