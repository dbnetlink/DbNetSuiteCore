using System.Data;

namespace DbNetSuiteCore.ViewModels.DbNetCombo
{
    public class ComboViewModel : BaseViewModel
    {
        public DataView DataView { get; set; }
        public bool AddFilter { get; set; }
    }
}