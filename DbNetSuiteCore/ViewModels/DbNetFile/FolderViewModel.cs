using DbNetSuiteCore.Models.DbNetFile;
using System.Collections.Generic;
using System.Data;

namespace DbNetSuiteCore.ViewModels.DbNetFile
{
    public class FolderViewModel : BaseViewModel
    {
        public string Folder { get; set; }
        public string RootFolder { get; set; }
        public FolderInformation Folders { get; set; }
    }
}