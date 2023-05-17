namespace DbNetSuiteCore.Models
{
    public class Column
    {
        public string ColumnKey => ColumnName.ToString();
        public string ColumnName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public Column()
        {
        }
    }
}
