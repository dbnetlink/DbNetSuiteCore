using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetEdit
{
    public class DbNetEditRequest : DbNetGridEditRequest
    {
        public Dictionary<string,object> Changes { get; set; } = new Dictionary<string, object>();
        public List<EditColumn> Columns { get; set; } = new List<EditColumn>();
        public int TotalRows { get; set; }
        public int CurrentRow { get; set; } = 1;
        public string PrimaryKey { get; set; }
    }
}