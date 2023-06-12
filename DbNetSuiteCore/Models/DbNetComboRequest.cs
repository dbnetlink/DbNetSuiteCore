using System.Collections.Generic;

namespace DbNetSuiteCore.Models
{
    public class DbNetComboRequest
    {
        public string ComponentId { get; set; }
        public string ConnectionString { get; set; }
        public string ConnectionType { get; set; }
        public string Sql { get; set; }
        public bool AddEmptyOption { get; set; } = false;
        public string EmptyOptionText { get; set; } = string.Empty;
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
    }
}