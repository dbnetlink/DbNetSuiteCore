using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models.DbNetEdit;
using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetGrid
{
    public class DbNetGridRequest : DbNetGridEditRequest
    {
        public BooleanDisplayMode BooleanDisplayMode { get; set; }
        public Dictionary<string, string> ColumnFilters { get; set; } = new Dictionary<string, string>();
        public List<GridColumn> Columns { get; set; } = new List<GridColumn>();
        public bool Copy { get; set; } = true;
        public int CurrentPage { get; set; } = 1;
        public GridColumn DefaultColumn { get; set; }
        public string Extension { get; set; }
        public bool Export { get; set; } = true;
        public Dictionary<string, object> FixedFilterParams { get; set; } = new Dictionary<string, object>();
        public string FixedFilterSql { get; set; }
        public bool FrozenHeader { get; set; } = false;
        public GridGenerationMode GridGenerationMode { get; set; } = GridGenerationMode.Display;
        public bool GroupBy { get; set; } = false;
        public bool MultiRowSelect { get; set; }
        public MultiRowSelectLocation MultiRowSelectLocation { get; set; }
        public bool NestedGrid { get; set; } = false;
        public int? OrderBy { get; set; }
        public OrderByDirection OrderByDirection { get; set; }
        public long PageSize { get; set; } = 20;
        public string ProcedureName { get; set; } = string.Empty;
        public Dictionary<string, object> ProcedureParams { get; set; } = new Dictionary<string, object>();
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; }
        public bool Update { get; set; } = false;
        public bool View { get; set; } = false;
        public int ViewLayoutColumns { get; set; }
    }
}