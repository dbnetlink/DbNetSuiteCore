using DbNetSuiteCore.Enums;

namespace DbNetSuiteCore.ViewModels.DbNetFile
{
    public class ToolbarViewModel : ToolbarBaseViewModel
    {
        public bool Search { get; set; }
        public bool Navigation { get; set; }
        public bool Export { get; set; }
        public bool Copy { get; set; }
        public bool Upload { get; set; }
    }
}
