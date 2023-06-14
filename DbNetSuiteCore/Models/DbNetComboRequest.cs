using System.Collections.Generic;

namespace DbNetSuiteCore.Models
{
    public class DbNetComboRequest
    {
        public string ComponentId { get; set; }
        public string ConnectionString { get; set; }
        public string ConnectionType { get; set; }
        public string FromPart { get; set; }
        public string ValueColumn { get; set; }
        public string TextColumn { get; set; }
        public bool AddEmptyOption { get; set; } = false;
        public bool AddFilter { get; set; } = false;
        public string EmptyOptionText { get; set; } = string.Empty;
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
        public string FilterToken { get; set; } = string.Empty;
        public string ForeignKeyColumn { get; set; } = null;
        public object ForeignKeyValue { get; set; } = null;
    }
}