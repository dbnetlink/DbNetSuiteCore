namespace DbNetSuiteCore.Models
{
    public class DbNetSuiteResponse
    {
        public bool Error { get; set; } = false;
        public string Message { get; set; }
        public string Html { get; set; }
        public string Dialog { get; set; }
        public string Toolbar { get; set; }
    }
}