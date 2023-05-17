using DbNetSuiteCore.Enums;

namespace DbNetSuiteCore.ViewModels.DbNetGrid
{
    public class ToolbarViewModel : BaseViewModel
    {
        public bool QuickSearch { get; set; }
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; }
        public bool View { get; set; }
        public bool Search { get; set; }
        public bool Navigation { get; set; }
        public bool Export { get; set; }
        public bool Copy { get; set; }
    }
}
