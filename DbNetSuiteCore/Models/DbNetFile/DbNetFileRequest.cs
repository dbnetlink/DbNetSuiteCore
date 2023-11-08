using System.Collections.Generic;

namespace DbNetSuiteCore.Models.DbNetFile
{
    public class DbNetFileRequest : DbNetSuiteRequest
    {
        public string Folder { get; set; }
        public string RootFolder { get; set; }
        public List<FileColumn> Columns { get; set; } = new List<FileColumn>();
    }
}