using System.Collections.Generic;

namespace DbNetSuiteCore.Models.Configuration
{
    public class DbNetGridConfiguration
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public List<DbColumn> Columns { get; set; } = new List<DbColumn>();
        public string ToolbarHtml { get; set; } = string.Empty;
    }
}