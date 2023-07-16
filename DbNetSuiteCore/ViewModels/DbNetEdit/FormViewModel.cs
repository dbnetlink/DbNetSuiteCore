using DbNetSuiteCore.Models.DbNetEdit;
using System.Collections.Generic;
using System.Data;

namespace DbNetSuiteCore.ViewModels.DbNetEdit
{
    public class FormViewModel : BaseViewModel
    {
        public DataColumnCollection DataColumns { get; set; }
        public List<EditColumn> EditColumns { get; set; }
        public Dictionary<string, DataTable> LookupTables { get; set; }
        public int LayoutColumns { get; set; }
    }
}
