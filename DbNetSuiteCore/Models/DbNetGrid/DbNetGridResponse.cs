using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetGrid
{
    public class DbNetGridResponse : DbNetGridEditResponse
    {
        private List<GridColumn> columns;
        public object Data { get; set; }
        public int TotalPages { get; set; }
        public long TotalRows { get; set; }
        public int CurrentPage { get; set; }
        public List<GridColumn> Columns
        {
            get { return columns; }
            set { columns = value; columns.ForEach(c => c.EncodeClientProperties()); }
        }
    }
}