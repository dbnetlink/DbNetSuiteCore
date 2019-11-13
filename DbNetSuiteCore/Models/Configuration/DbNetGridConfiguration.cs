using DbNetSuiteCore.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

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
        public List<DbColumn> Columns { get; set; } = new List<DbColumn>();
        public ListDictionary Html { get; set; } = new ListDictionary();
        public string SearchToken { get; set; }
    }
}