using DbNetSuiteCore.Enums;
using System.Collections.Generic;

namespace DbNetSuiteCore.Components
{
    public class FileConfiguration : BinaryConfiguration
    {
        public FileConfiguration( string extension, Dictionary<FileMetaData, string> metaDataColumns = null) : base(extension, metaDataColumns)
        { }
    }
}