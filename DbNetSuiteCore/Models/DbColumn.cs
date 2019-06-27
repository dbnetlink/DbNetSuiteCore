using DbNetSuiteCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbNetSuiteCore.Models
{
    /////////////////////////////////////////////// 
    public class DbColumn : Column
    ///////////////////////////////////////////////
    {
        public enum LookupSearchModeValues
        {
            SearchValue,
            SearchText
        }

        internal int MaxTextLength = 10;
        internal bool AddedByUser = true;

        public AuditModes Audit { get; set; } = AuditModes.None;
        public bool AutoIncrement { get; set; } = false;
        public bool IsBoolean { get; set; } = false;
        public string BaseSchemaName { get; set; } = string.Empty;
        public string BaseTableName { get; set; } = string.Empty;
        public bool BulkInsert { get; set; } = false;
        public string ColumnExpression { get; set; } = string.Empty;
        public string ColumnExpressionKey { get; set; } = string.Empty;
        public int ColumnSize { get; set; } = 0;
        public string Culture { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string DbDataType { get; set; } = string.Empty;
        public Dictionary<string, object> EditControlProperties { get; set; } = new Dictionary<string, object>();
        public bool Display { get; set; } = true;
        public ControlType EditControlType { get; set; } = ControlType.Auto;
        public bool EditDisplay { get; set; } = true;
        public HashTypes Encryption { get; set; } = HashTypes.None;
        public bool UpdateReadOnly { get; set; } = false;
        public bool ForeignKey { get; set; } = false;
        public string Format { get; set; } = string.Empty;
        public string SearchFormat { get; set; } = string.Empty;
        public string InitialValue { get; set; } = string.Empty;
        public bool InsertReadOnly { get; set; } = false;
        public string Lookup { get; set; } = string.Empty;
        public string LookupDataType { get; set; } = string.Empty;
        public LookupSearchModeValues LookupSearchMode { get; set; } = LookupSearchModeValues.SearchValue;
        public string LookupTable { get; set; } = string.Empty;
        public string LookupTextField { get; set; } = string.Empty;
        public string LookupTextExpression { get; set; } = string.Empty;
        public string LookupValueField { get; set; } = string.Empty;
        public int MaxThumbnailHeight { get; set; } = 30;
        public string PlaceHolder { get; set; } = string.Empty;
        public bool PrimaryKey { get; set; } = false;
        public bool ReadOnly { get; set; } = false;
        public bool Required { get; set; } = false;
        public bool SimpleSearch { get; set; } = true;
        public bool Search { get; set; } = true;
        public string SearchLookup { get; set; } = string.Empty;
        public int SearchColumnOrderSearch { get; set; } = 0;
        public string SequenceName { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public string ToolTip { get; set; } = string.Empty;
        public bool Unique { get; set; } = false;
        public string UploadDataColumn { get; set; } = string.Empty;
        public string UploadExtFilter { get; set; } = string.Empty;
        public string UploadFileNameColumn { get; set; } = string.Empty;
        public int UploadMaxFileSize { get; set; } = 0;
        public bool UploadOverwrite { get; set; } = false;
        public bool UploadRename { get; set; } = false;
        public string UploadRootFolder { get; set; } = string.Empty;
        public string UploadSubFolder { get; set; } = string.Empty;
        public string XmlAttributeName { get; set; } = string.Empty;
        public string XmlElementName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public int ParentColumnIndex { get; set; } = -1;
    }
}
