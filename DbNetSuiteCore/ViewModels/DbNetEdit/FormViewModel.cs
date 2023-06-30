using DbNetSuiteCore.Models.DbNetEdit;
using System.Collections.Generic;
using System.Data;

namespace DbNetSuiteCore.ViewModels.DbNetEdit
{
    public class FormViewModel : BaseViewModel
    {
        public DataTable EditData { get; set; }
        public List<EditColumn> Columns { get; set; }
        public Dictionary<string, DataTable> LookupTables { get; set; }
    }
}
