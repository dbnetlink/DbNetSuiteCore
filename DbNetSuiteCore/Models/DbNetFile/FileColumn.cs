using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Enums.DbNetFile;

namespace DbNetSuiteCore.Models.DbNetFile
{
    public class FileColumn
    {
        public FileInfoProperties Type { get; set; }
        public string Format { get; set; }
        public string Name => Type.ToString();
        public string Label { get; set; }
        public OrderByDirection? OrderBy { get; set; } = null;
        public FileColumn()
        {
        }
        public FileColumn(FileInfoProperties type)
        {
            Type = type;
        }
    }
}