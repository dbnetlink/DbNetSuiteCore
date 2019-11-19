namespace DbNetSuiteCore.Models
{
    public class Column
    {
        public string ColumnKey { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ColumnExpression { get; set; } = string.Empty;
        public string ColumnExpressionKey { get; set; } = string.Empty;

        public Column(string columnName)
        {
            ColumnName = columnName;
            ColumnExpression = columnName;
        }
    }
}
