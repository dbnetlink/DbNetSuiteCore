using DbNetSuiteCore.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;

namespace DbNetSuiteCore.Models.Configuration
{
    public class DbNetGridConfiguration
    {
        public string Id { get; set; }
        public string ConnectionString { get; set; }
        public DataProvider DataProvider { get; set; } = DataProvider.SqlClient;
        public string TableName { get; set; }
        public int PageSize { get; set; } = 20;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages => (int)Math.Ceiling(TotalRows / (double)PageSize);
        public int TotalRows { get; set; } = -1;
        public List<GridColumn> Columns { get; set; } = new List<GridColumn>();
        public ListDictionary Html { get; set; } = new ListDictionary();
        public string SearchToken { get; set; }
        public DataTable PageData { get; set; }
        public string OrderByColumn { get; set; }
        public string OrderBySequence { get; set; }
        public string DropDownFilterValue { get; set; }
        public string DropDownFilterColumn { get; set; }
    }
}