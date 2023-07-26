using DbNetSuiteCore.Enums;

namespace DbNetSuiteCore.Models
{
    public class DbNetSuiteRequest
    {
        public string ComponentId { get; set; }
        public string ConnectionString { get; set; }
        public string ConnectionType { get; set; }
        public string Culture { get; set; }
        public ComponentType? ParentControlType { get; set; }
    }
}