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
        public bool Copy { get; set; } = true;
        public string Culture { get; set; }
        public int CurrentPage { get; set; } = 1;
        public GridColumn DefaultColumn { get; set; }
        public string Extension { get; set; }
        public bool Export { get; set; } = true;
        public Dictionary<string, object> FixedFilterParams { get; set; } = new Dictionary<string, object>();
        public string FixedFilterSql { get; set; }
        public string FromPart { get; set; }
        public bool FrozenHeader { get; set; } = false;
        public GridGenerationMode GridGenerationMode { get; set; } = GridGenerationMode.Display;
        public bool GroupBy { get; set; } = false;
        public int LookupColumnIndex { get; set; }
        public bool MultiRowSelect { get; set; }
        public MultiRowSelectLocation MultiRowSelectLocation { get; set; }
        public bool Navigation { get; set; } = true;
        public bool NestedGrid { get; set; } = false;
        public bool OptimizeForLargeDataset { get; set; }
        public int? OrderBy { get; set; }
        public OrderByDirection OrderByDirection { get; set; }
        public long PageSize { get; set; } = 20;
        public string PrimaryKey { get; set; }
        public string ProcedureName { get; set; } = string.Empty;
        public Dictionary<string, object> ProcedureParams { get; set; } = new Dictionary<string, object>();
        public bool QuickSearch { get; set; } = false;
        public string QuickSearchToken { get; set; }
        public bool Search { get; set; } = true;
        public string SearchFilterJoin { get; set; }
        public List<SearchParameter> SearchParams { get; set; } = new List<SearchParameter>();
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; }
        public bool View { get; set; } = false;
    }
}