using DbNetSuiteCore.Enums.DbNetFile;
using DbNetSuiteCore.Models.DbNetFile;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DbNetSuiteCore.ViewModels.DbNetFile
{
    public class FileViewModel : BaseViewModel
    {
        public string Folder { get; set; }
        public string RootFolder { get; set; }
        public DataView DataView { get; set; }
        public List<FileColumn> Columns { get; set; }
        public int FirstRow { get; set; }
        public int LastRow { get; set; }
        public string Caption { get; set; }
        public bool Nested { get; set; }
        public bool IsSearchResults { get; set; }
        public List<FileColumn> FilteredColumns => Columns.Where(c => c.Name != FileInfoProperties.IsDirectory.ToString()).ToList();
    }
}