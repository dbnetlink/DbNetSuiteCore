namespace DbNetSuiteCore.Models
{
    public class DbNetSuiteCoreSettings
    {
        public string FontFamily { get; set; } = string.Empty;
        public string FontSize { get; set; } = string.Empty;
        public bool Debug { get; set; } = false;
        public bool ReadOnly { get; set; } = false;
        public string Culture { get; set; } = string.Empty;
    }
}