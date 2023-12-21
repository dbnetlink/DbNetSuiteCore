using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace DbNetSuiteCore.Components
{
    public class DbNetGridEditCoreColumn
    {
        readonly List<ColumnProperty> _columnProperties;
        protected readonly string[] _columnNames;
        readonly string _fromPart;
        readonly List<string> _columns;

        internal DbNetGridEditCoreColumn(string[] columnNames, List<ColumnProperty> columnProperties, string fromPart, List<string> columns)
        {
            _columnProperties = columnProperties;
            _columnNames = columnNames;
            _fromPart = fromPart;
            _columns = columns;
        }

        protected void Lookup(Lookup lookup, string columnName = null)
        {
            SetColumnProperty(ColumnPropertyType.Lookup, lookup, columnName);

            if (string.IsNullOrEmpty(lookup.Parameter) == false)
            {
                SetColumnProperty(InternalColumnPropertyType.LookupParameter, lookup.Parameter, columnName);
            }
        }
        protected void Lookup(Type lookup, bool useNameAsValue = false)
        {
            if (lookup.IsEnum)
            {
                SetColumnProperty(ColumnPropertyType.LookupDataTable, EnumHelper.EnumToDataTable(lookup, useNameAsValue));
            }
        }

        protected void Lookup<T>(Dictionary<T,string> lookup)
        {
            SetColumnProperty(ColumnPropertyType.LookupDataTable, DictionaryToDataTable(lookup));
        }
        protected void Lookup()
        {
            foreach (string columnName in _columnNames)
            {
                Lookup(new Lookup(_fromPart, columnName, null, true), columnName);
            }
        }
        protected void Display(bool display = true)
        {
            SetColumnProperty(ColumnPropertyType.Display, display);
        }
        protected void Hidden(bool hide = true)
        {
            Display(!hide);
        }
        protected void Search()
        {
            SetColumnProperty(ColumnPropertyType.Search, true);
        }
        protected void Style(string style)
        {
            SetColumnProperty(ColumnPropertyType.Style, style);
        }
        /// <summary>
        protected void DataType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            SetColumnProperty(ColumnPropertyType.DataType, type.ToString().Split(".").Last());
        }
        protected void ForeignKey()
        {
            SetColumnProperty(ColumnPropertyType.ForeignKey, true);
        }
        protected void PrimaryKey(bool autoincrement = true)
        {
            SetColumnProperty(ColumnPropertyType.PrimaryKey, true);
            if (autoincrement)
            {
                SetColumnProperty(ColumnPropertyType.AutoIncrement, true);
            }
        }
        protected void Format(string format)
        {
            SetColumnProperty(ColumnPropertyType.Format, format);
        }
        protected void Label(string label)
        {
            SetColumnProperty(ColumnPropertyType.Label, label);
        }
        protected void Image(ImageConfiguration imageConfiguration)
        {
            SetColumnAsBlob(imageConfiguration, true);
        }
        protected void File(FileConfiguration fileConfiguration)
        {
            SetColumnAsBlob(fileConfiguration, false);
        }

        private DbNetGridEditCoreColumn SetColumnAsBlob(BinaryConfiguration configuration, bool image)
        {
            ColumnPropertyType columnPropertyType = image ? ColumnPropertyType.Image : ColumnPropertyType.Download;
            SetColumnProperty(columnPropertyType, true);

            var extensionsWithMimeType = configuration.Extensions.Select(e => e = $"{e.Replace(".", string.Empty)}|{TextHelper.GetMimeTypeForFileExtension(e)}").ToList();

            SetColumnProperty(ColumnPropertyType.Extension, string.Join(",", extensionsWithMimeType));

            if (configuration.MetaDataColumns != null)
            {
                foreach (FileMetaData fileMetaData in configuration.MetaDataColumns.Keys)
                {
                    string metaDataColumnName = configuration.MetaDataColumns[fileMetaData];
                    SetColumnProperty(ColumnPropertyType.UploadMetaData, fileMetaData, metaDataColumnName);
                    SetColumnProperty(ColumnPropertyType.UploadMetaDataColumn, _columnNames.First(), metaDataColumnName);
                }
            }
            return this;
        }

        public static DataTable DictionaryToDataTable<T>(Dictionary<T,string> lookup)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("value",typeof(string));
            dataTable.Columns.Add("text", typeof(string));

            foreach (T key in lookup.Keys)
            {
                dataTable.Rows.Add(key, lookup[key]);
            }

            return dataTable;
        }

        public bool ColumnPropertySet(Enum propertyType)
        {
            return _columnProperties.Any(c => c.ColumnName == _columnNames.First().ToLower() && c.PropertyType == propertyType);
        }
        protected void SetColumnProperty(Enum propertyType, object propertyValue, string columnName = null)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                foreach (string name in _columnNames)
                {
                    string colName = FindColumnName(name);
                    _columnProperties.Add(new ColumnProperty(colName.ToLower(), propertyType, propertyValue));
                }
            }
            else
            {
                _columnProperties.Add(new ColumnProperty(columnName.ToLower(), propertyType, propertyValue));
            }

            string FindColumnName(string name)
            {
                if (_columns.Any(c => c.ToLower() == name.ToLower()))
                {
                    return name;
                }

                var splitChars = new string[] { ".", " " };
                foreach (string splitChar in splitChars)
                {
                    var columnExpr = _columns.FirstOrDefault(c => c.Split(splitChar).Last().ToLower() == name.ToLower());

                    if (columnExpr != null)
                    {
                        return columnExpr;
                    }
                }

                if (propertyType is ColumnPropertyType)
                {
                    if ((ColumnPropertyType)propertyType == ColumnPropertyType.DataOnly)
                    {
                        if (_columns.Any())
                        {
                            _columns.Add(name);
                        }
                    }
                }
                return name;
            }
        }
    }
}