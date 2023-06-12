namespace DbNetSuiteCore.Models
{
    public class DbNetComboResponse
    {
        public object Data { get; set; }
        public long TotalRows { get; set; }
        public bool Error { get; set; } = false;
        public string Message { get; set; }
    }
}