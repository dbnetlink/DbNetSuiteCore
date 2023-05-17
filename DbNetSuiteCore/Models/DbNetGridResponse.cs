using System.Collections.Generic;

namespace DbNetSuiteCore.Models
{
    public class DbNetGridResponse
    {
        private List<GridColumn> columns;
        public string Toolbar { get; set; }
        public object Data { get; set; }
        public int TotalPages { get; set; }
        public long TotalRows { get; set; }
        public int CurrentPage { get; set; }
        public List<GridColumn> Columns
        {
            get { return columns; }
            set { columns = value; columns.ForEach(c => c.EncodeClientProperties()); }
        }
        public Dictionary<string, object> Record { get; set; }
        public List<SearchParameter> SearchParams { get; set; }
        public bool Error { get; set; } = false;
        public string Message { get; set; }
    }
}
