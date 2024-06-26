﻿using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DbNetSuiteCore.Models
{
    public class DbColumn : Column
    {
        private List<string> _numericDataTypes = new List<string>() { nameof(Decimal), nameof(Double), nameof(Single), nameof(Int64), nameof(Int32), nameof(Int16) };
        private bool _search = false;
        private bool _userAssignedSearch = false;
        private string _lookup;
        private string _columnExpression;
        private object _foreignKeyValue;

        public string BaseTableName { get; set; }
        public string BaseSchemaName { get; set; }
        public bool Browse { get; set; } = false;
        public string DataType { get; set; }
        public DbColumn DependentLookup { get; set; }
        public bool IsDependentLookup => string.IsNullOrEmpty(LookupParameter) == false;

        public string OriginalDataType { get; set; }

        public bool Search
        {
            get => _search && Binary == false && ForeignKey == false;
            set => _search = SetSearch(value);
        }

        public bool UserAssignedSearch
        {
            get => _userAssignedSearch && Search;
        }
        public bool AddedByUser { get; set; } = true;
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
        public int LookupColumns => LookupIsDataTable ? 2 : DbNetGridEdit.GetSelectColumns(Lookup).Length;
        public bool LookupIsDataTable
        {
            get => LookupDataTable != null && LookupDataTable.HasValues;
        }
        public bool HasLookup
        {
            get => string.IsNullOrEmpty(Lookup) == false || LookupIsDataTable;
        }
        public JArray LookupDataTable { get; set; }
        public string LookupDataType { get; set; }
        public string LookupParameter { get; set; }

        public string DbDataType { get; set; }
        public string LookupTable { get; set; }
        public string LookupValueField { get; set; }
        public string LookupTextField { get; set; }
        public string LookupTextExpression { get; set; }
        public string Culture { get; set; }
        public bool IsBoolean => DataType == nameof(Boolean);
        public bool Unmatched { get; set; }
        public bool Binary => DataType == "Byte[]";
        public int Index { get; set; } = -1;
        public bool? Display { get; set; } = null;
        public bool Show => Display.HasValue ? Display.Value : false;
        public bool QuickSearch { get; set; } = false;
        public bool IsNumeric => _numericDataTypes.Contains(DataType);
        public bool AllowsNull { get; set; }
        public bool Download { get; set; } = false;
        public bool Image { get; set; } = false;
        public string Extension { get; set; }
        public bool IsImageExtension => IsImageFileType();
        public FileMetaData? UploadMetaData { get; set; }
        public string UploadMetaDataColumn { get; set; }
        public string Style { get; set; }
        public bool IsKey => PrimaryKey || ForeignKey;


        public DbColumn()
        {
        }

        public DbColumn(string columnExpression)
        {
            ColumnExpression = EncodingHelper.Encode(columnExpression);
        }

        public void EncodeClientProperties()
        {
            _columnExpression = EncodingHelper.Encode(_columnExpression);
            _lookup = EncodingHelper.Encode(_lookup);
        }
        public bool IsMatch(string columnName)
        {
            if (this.ColumnExpression.ToLower() == columnName.ToLower())
                return true;
            else if (this.ColumnName.ToLower() == columnName.ToLower())
                return true;

            return false;
        }

        private bool IsImageFileType()
        {
            if (string.IsNullOrEmpty(Extension) == false)
            {
                string[] extensions = Extension.ToLower().Replace(".", string.Empty).Split(',');
                foreach (string extension in extensions)
                {
                    if (extension.Split("|").Last().StartsWith("image/"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private object ConvertForeignKeyValue(object foreignKeyValue)
        {
            if (foreignKeyValue != null)
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
                try
                {
                    var fk = EncodingHelper.Decode(foreignKeyValue.ToString());
                    var foreignKeyValues = JsonSerializer.Deserialize<Dictionary<string, object>>(fk);
                    return foreignKeyValues.Values.First();
                }
                catch (Exception)
                {
                }
            }

            return foreignKeyValue;
        }

        private bool SetSearch(bool searchable)
        {
            if (searchable)
            {
                _userAssignedSearch = true;
            }
            return searchable;
        }
    }
}