using Microsoft.AspNetCore.SignalR;

namespace DbNetSuiteCore.Models
{
    public class DbNetSuiteCoreSettings
    {
        public string FontFamily { get; set; }
        public string FontSize { get; set; }
        public bool Debug { get; set; } = false;
    }
}