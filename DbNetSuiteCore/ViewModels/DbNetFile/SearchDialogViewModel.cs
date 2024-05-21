using DbNetSuiteCore.Models.DbNetFile;
using System.Collections.Generic;

namespace DbNetSuiteCore.ViewModels.DbNetFile
{
    public class SearchDialogViewModel : BaseViewModel
    {
        public List<FileColumn> Columns { get; set; }
    }
}
