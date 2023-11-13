using DbNetSuiteCore.Enums.DbNetFile;

namespace DbNetSuiteCore.Models.DbNetFile
{
    public class FileColumn
    {
        private string _label;
        public FileInfoProperties Type { get; set; }
        public string Format { get; set; }
        public string Name => Type.ToString();
        public string Label { 
            get => string.IsNullOrEmpty(_label) ? Name : _label;
            set => _label = value;
        }
       
        public FileColumn()
        {
        }
        public FileColumn(FileInfoProperties type)
        {
            Type = type;
        }
    }
}