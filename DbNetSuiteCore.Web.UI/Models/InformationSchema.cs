namespace DbNetSuiteCore.Web.UI.Models
{
    public class InformationSchema
    {
        public string? TABLE_CATALOG { get; set; }
        public string? TABLE_SCHEMA { get; set; }
        public string? TABLE_NAME { get; set; }
        public string? TABLE_TYPE { get; set; }
        public string FullQualifiedTableName => $"[{TABLE_SCHEMA}].[{TABLE_NAME}]";
        public string FullTableName => $"{TABLE_SCHEMA}.{TABLE_NAME}";
    }
}
