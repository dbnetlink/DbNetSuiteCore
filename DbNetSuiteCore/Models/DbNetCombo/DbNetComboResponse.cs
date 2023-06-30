namespace DbNetSuiteCore.Models.DbNetCombo
{
    public class DbNetComboResponse : DbNetSuiteResponse
    {
        public object Select { get; set; }
        public object Options { get; set; }
        public long TotalRows { get; set; }
    }
}