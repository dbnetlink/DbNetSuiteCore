using DbNetSuiteCore.Enums;
using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetFile
{
    public class DbNetFileRequest : DbNetSuiteRequest
    {
        public string Folder { get; set; }
        public string FileName { get; set; }
        public string RootFolder { get; set; }
        public List<FileColumn> Columns { get; set; } = new List<FileColumn>();
        public bool QuickSearch { get; set; }
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; }
        public bool Search { get; set; }
        public bool Navigation { get; set; }
        public bool Export { get; set; }
        public bool Copy { get; set; }
        public bool Upload { get; set; }
        public int CurrentPage { get; set; }
        public string Caption { get; set; }
        public bool Nested { get; set; }
        public string OrderBy { get; set; }
        public OrderByDirection? OrderByDirection { get; set; }
        public List<SearchParameter> SearchParams { get; set; }
        public string SearchFilterJoin { get; set; } = "and";
        public bool IsSearchResults { get; set; }
        public bool IncludeSubfolders { get; set; }

    }
}