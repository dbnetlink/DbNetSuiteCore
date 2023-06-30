using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models.DbNetGrid;
using System.Collections.Generic;
using System.Data;
using System.Resources;

namespace DbNetSuiteCore.ViewModels.DbNetGrid
{
    public class SearchDialogViewModel : BaseViewModel
    {
        public BooleanDisplayMode BooleanDisplayMode { get; set; }
        public List<GridColumn> Columns { get; set; }
        public Dictionary<string, DataTable> LookupTables { get; set; }
    }
}
