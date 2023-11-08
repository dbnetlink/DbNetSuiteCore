using DbNetSuiteCore.Models.DbNetFile;
using System.Collections.Generic;
using System.Data;

namespace DbNetSuiteCore.ViewModels.DbNetFile
{
    public class FileViewModel : BaseViewModel
    {
        public string Folder { get; set; }
        public string RootFolder { get; set; }
        public DataView DataView { get; set; }
        public List<FileColumn> Columns { get; set; }
    }
}