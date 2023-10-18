﻿using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DbNetSuiteCore.Components
{
    public class DbNetGridEditCoreColumn
    {
        readonly List<ColumnProperty> _columnProperties;
        readonly string[] _columnNames;
        readonly string _fromPart;
        readonly List<string> _columns;

        internal DbNetGridEditCoreColumn(string[] columnNames, List<ColumnProperty> columnProperties, string fromPart, List<string> columns)
        {
            _columnProperties = columnProperties;
            _columnNames = columnNames;
            _fromPart = fromPart;
            _columns = columns;
        }
        protected void Lookup(Lookup lookup)
        {
            SetColumnProperty(ColumnPropertyType.Lookup, lookup);

            if (string.IsNullOrEmpty(lookup.Parameter) == false)
            {
                SetColumnProperty(InternalColumnPropertyType.LookupParameter, lookup.Parameter);
            }
        }
        protected void Lookup(Type lookup, bool useNameAsValue = false)
        {
            if (lookup.IsEnum)
            {
                DataTable dt = EnumHelper.EnumToDataTable(lookup, useNameAsValue);
                SetColumnProperty(ColumnPropertyType.Lookup, JsonConvert.SerializeObject(dt));
            }
        }
        protected void Lookup()
        {
            Lookup(new Lookup(_fromPart, _columnNames.First(), null, true));
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
            SetColumnProperty(ColumnPropertyType.DataType, type.ToString().Split(".").Last());
        }
        protected void ForeignKey()
        {
            SetColumnProperty(ColumnPropertyType.ForeignKey, true);
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