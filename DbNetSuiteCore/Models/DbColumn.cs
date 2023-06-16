﻿using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using System.Collections.Generic;

namespace DbNetSuiteCore.Models
{
    public class DbColumn : Column
    {
        private bool _search = true;
        private string _lookup;
        private string _columnExpression;
        private object _foreignKeyValue;
        public string BaseTableName { get; set; }
        public string BaseSchemaName { get; set; }
        public string DataType { get; set; }
        public string OriginalDataType { get; set; }

        public bool Search
        {
            get => _search && Binary == false && ForeignKey == false;
            set => _search = value;
        }
        public bool AddedByUser { get; set; }
        public string Format { get; set; } = string.Empty;
        public string ColumnExpression
        {
            get => EncodingHelper.Decode(_columnExpression);
            set => _columnExpression = value;
        }
        public bool EditDisplay { get; set; }
        public bool PrimaryKey { get; set; } = false;
        public bool ForeignKey { get; set; }
        public object ForeignKeyValue
        {
            get => ConvertForeignKeyValue(_foreignKeyValue);
            set => _foreignKeyValue = value;
        }
        public bool AutoIncrement { get; set; }
        public int ColumnSize { get; set; }
        public string Lookup
        {
            get => EncodingHelper.Decode(_lookup);
            set => _lookup = value;
        }
        public string LookupDataType { get; set; }
        public string DbDataType { get; set; }
        public string LookupTable { get; set; }
        public string LookupValueField { get; set; }
        public string LookupTextField { get; set; }
        public string LookupTextExpression { get; set; }
        public string Culture { get; set; }
        public bool IsBoolean { get; set; }
        public EditControlType EditControlType { get; set; }
        public bool Unmatched { get; set; }
        public bool Binary => DataType == "Byte[]";


        public DbColumn()
        {
        }

        public void EncodeClientProperties()
        {
            _columnExpression = EncodingHelper.Encode(_columnExpression);
            _lookup = EncodingHelper.Encode(_lookup);
        }

        private object ConvertForeignKeyValue(object foreignKeyValue)
        {
            if (foreignKeyValue is Newtonsoft.Json.Linq.JArray)
            {
                List<object> fkArray = (foreignKeyValue as Newtonsoft.Json.Linq.JArray).ToObject<List<object>>();
                if (fkArray.Count > 0)
                {
                    return fkArray;
                }
                return nameof(System.DBNull);
            }
            return foreignKeyValue;
        }
    }
}
