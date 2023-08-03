using DbNetSuiteCore.Enums;
using System.Collections.Generic;

namespace DbNetSuiteCore.Components
{
    public class ImageConfiguration : BinaryConfiguration
    {
        public ImageConfiguration(List<string> extensions, Dictionary<FileMetaData, string> metaDataColumns = null) : base(extensions, metaDataColumns)
        {
        }
        public ImageConfiguration(string extension, Dictionary<FileMetaData, string> metaDataColumns = null) : base(extension, metaDataColumns)
        {
        }
    }
}