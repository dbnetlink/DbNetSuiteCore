using DbNetSuiteCore.Enums;
using System.Collections.Generic;

namespace DbNetSuiteCore.Components
{
    public class FileConfiguration : BinaryConfiguration
    {
        public FileConfiguration(List<string> extensions, Dictionary<FileMetaData, string> metaDataColumns = null) : base(extensions, metaDataColumns)
        { }
        public FileConfiguration( string extension, Dictionary<FileMetaData, string> metaDataColumns = null) : base(extension, metaDataColumns)
        { }
    }
}