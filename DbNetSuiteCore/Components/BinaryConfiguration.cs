using DbNetSuiteCore.Enums;
using System.Collections.Generic;
using System.Linq;

namespace DbNetSuiteCore.Components
{
    public class BinaryConfiguration
    {
	    private readonly List<string> _extensions = new List<string>();
	    private readonly Dictionary<FileMetaData, string> _metaDataColumns;

        public List<string> Extensions => _extensions.Select(e => e.Split(".").Last()).ToList();
        public Dictionary<FileMetaData, string> MetaDataColumns => _metaDataColumns;

        public BinaryConfiguration(List<string> extensions, Dictionary<FileMetaData, string> metaDataColumns)
        {
            _extensions = extensions;
            _metaDataColumns = metaDataColumns; 
        }
        public BinaryConfiguration(string extension, Dictionary<FileMetaData, string> metaDataColumns) : this(new List<string> { extension }, metaDataColumns) { }
    }
}