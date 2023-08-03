using DbNetSuiteCore.Enums;
using System.Collections.Generic;

namespace DbNetSuiteCore.Components
{
    public class BinaryConfiguration
    {
	    private readonly List<string> _extensions;
	    private readonly Dictionary<FileMetaData, string> _metaDataColumns;

        public List<string> Extensions => _extensions;
        public Dictionary<FileMetaData, string> MetaDataColumns => _metaDataColumns;

        public BinaryConfiguration(List<string> extensions, Dictionary<FileMetaData, string> metaDataColumns)
        {
            _extensions = extensions;
            _metaDataColumns = metaDataColumns; 
        }
        public BinaryConfiguration(string extension, Dictionary<FileMetaData, string> metaDataColumns) : this(new List<string> { extension }, metaDataColumns) { }
    }
}