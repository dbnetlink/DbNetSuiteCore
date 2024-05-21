using DbNetSuiteCore.Enums;
using DbNetSuiteCore.ViewModels.DbNetFile;

namespace DbNetSuiteCore.ViewModels.DbNetGrid
{
    public class ToolbarViewModel : ToolbarBaseViewModel
    {
        public bool View { get; set; }
        public bool Search { get; set; }
        public bool Navigation { get; set; }
        public bool Export { get; set; }
        public bool Copy { get; set; }
        public bool Insert { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
    }
}
