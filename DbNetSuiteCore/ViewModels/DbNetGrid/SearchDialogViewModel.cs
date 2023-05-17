using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Models;
using System.Collections.Generic;
using System.Data;
using System.Resources;

namespace DbNetSuiteCore.ViewModels.DbNetGrid
{
    public class SearchDialogViewModel : BaseViewModel
    {
        public BooleanDisplayMode BooleanDisplayMode { get; set; }
        public GridColumnCollection Columns { get; set; }
        public Dictionary<string, DataTable> LookupTables { get; set; }
    }
}
