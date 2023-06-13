namespace DbNetSuiteCore.Models
{
    public class DbNetComboResponse
    {
        public object Select { get; set; }
        public object Options { get; set; }
        public long TotalRows { get; set; }
        public bool Error { get; set; } = false;
        public string Message { get; set; }
    }
}