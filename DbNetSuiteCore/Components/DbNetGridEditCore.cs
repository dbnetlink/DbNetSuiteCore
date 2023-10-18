using DbNetSuiteCore.Attributes;
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
    public class DbNetGridEditCore : DbNetSuiteCore
    {
        protected readonly string _fromPart;

        internal List<ColumnProperty> _columnProperties { get; set; } = new List<ColumnProperty>();
        internal string FromPart => _fromPart;

        /// <summary>
        /// Selects the columns to be displayed in the grid
        /// </summary>
        public List<string> Columns { get; set; } = new List<string>();
        /// <summary>
        /// Allow deletion of records
        /// </summary>
        public bool? Delete { get; set; } = null;
        /// <summary>
        /// Use to assign values for any parameter placeholders used in the SQL
        /// </summary>
        public Dictionary<string, object> FixedFilterParams { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// Applies an SQL filter the grid 
        /// </summary>
        public string FixedFilterSql { get; set; } = null;
        /// <summary>
        /// Allow insertion of new records
        /// </summary>
        public bool? Insert { get; set; } = null;
        /// <summary>
        /// Labels for the columns specified in the Columns property
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();
        /// <summary>
        /// Maximum height in pixels for preview image in grid or edit panel
        /// </summary>
        public int? MaxImageHeight { get; set; } = null;
        /// <summary>
        /// Sets initial ordering of the records e.g. last_updated desc 
        /// </summary>
        public string InitialOrderBy { get; set; } = null;
        /// <summary>
        /// Optimizes the performance for large datasets
        /// </summary>
        public bool? OptimizeForLargeDataset { get; set; } = null;
        /// <summary>
        /// Displays a search box in the toolbar that allows for searching against all the text based columns
        /// </summary>
        public bool? QuickSearch { get; set; } = null;
        /// <summary>
        /// Adds/removes a page navigation to/from the toolbar
        /// </summary>
        public bool? Navigation { get; set; } = null;
        /// <summary>
        /// Adds/removes a search dialog option to/from the toolbar
        /// </summary>
        public bool? Search { get; set; } = null;
        /// <summary>
        /// Controls the style of the toolbar button
        /// </summary>
        public ToolbarButtonStyle? ToolbarButtonStyle { get; set; } = null;
        /// <summary>
        /// Controls the position of the toolbar
        /// </summary>
        public ToolbarPosition? ToolbarPosition { get; set; } = null;

        public DbNetGridEditCore(string connection, string fromPart, string id = null) : base(connection, id)
        {
            _fromPart = fromPart;
        }
        /// <summary>
        /// Assigns a foreign key based lookup against a column to provide a descriptive value
        /// </summary>
        public void SetColumnLookup(string columnName, Lookup lookup)
        {
            SetColumnProperty(columnName, ColumnPropertyType.Lookup, lookup);

            if (string.IsNullOrEmpty(lookup.Parameter) == false)
            {
                SetColumnProperty(columnName, InternalColumnPropertyType.LookupParameter, lookup.Parameter);
            }
        }

        /// <summary>
        /// Assigns an enum based lookup against a column to provide a descriptive value
        /// </summary>
        public void SetColumnLookup(string columnName, Type lookup, bool useNameAsValue = false)
        {
            if (lookup.IsEnum)
            {
                DataTable dt = EnumToDataTable(lookup, useNameAsValue);
                SetColumnProperty(columnName, ColumnPropertyType.Lookup, JsonConvert.SerializeObject(dt));
            }
        }

        private DataTable EnumToDataTable(Type enumType, bool useNameAsValue)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("value", useNameAsValue ? typeof(string) : typeof(int));
            dataTable.Columns.Add("text", typeof(string));

            foreach (Enum value in Enum.GetValues(enumType))
            {
                string description = value.GetAttribute<DescriptionAttribute>()?.Description ?? Enum.GetName(enumType, value);
                dataTable.Rows.Add(useNameAsValue ? Enum.GetName(enumType, value) : Convert.ChangeType(value, value.GetTypeCode()), description);
            }

            return dataTable;
        }

        /// <summary>
        /// Creates a lookup based on a list of existing distinct values for the column
        /// </summary>
        public void SetColumnLookup(string columnName)
        {
            SetColumnLookup(columnName, new Lookup(_fromPart, columnName, null, true));
        }

        /// <summary>
        /// Shows/hides the specified column in the control
        /// </summary>
        public void SetColumnDisplay(string columnName, bool display = true)
        {
            SetColumnProperty(columnName, ColumnPropertyType.Display, display);
        }
        /// <summary>
        /// Shows/hides the specified columns in the control
        /// </summary>
        public void SetColumnDisplay(string[] columnNames, bool display = true)
        {
            foreach (var columnName in columnNames)
            {
                SetColumnDisplay(columnName, display);
            }
        }

        /// <summary>
        /// Hides the specified column in the control
        /// </summary>
        public void SetColumnHidden(string columnName, bool hide = true)
        {
            SetColumnDisplay(columnName, !hide);
        }
        /// <summary>
        /// Hides the specified columns in the control
        /// </summary>
        public void SetColumnHidden(string[] columnNames, bool hide = true)
        {
            SetColumnDisplay(columnNames, !hide);
        }

        /// <summary>
        /// Specifies the column to be shown in the Search dialog
        /// </summary>
        public void SetColumnSearch(string columnName)
        {
            SetColumnProperty(columnName, ColumnPropertyType.Search, true);
        }
        /// <summary>
        /// Specifies the columns to be shown in the Search dialog
        /// </summary>
        public void SetColumnSearch(string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                SetColumnSearch(columnName);
            }
        }

        /// <summary>
        /// Sets a CSS style for the specified column e.g. "background-color:gold; color:steelblue"
        /// </summary>
        public void SetColumnStyle(string columnName, string style)
        {
            SetColumnProperty(columnName, ColumnPropertyType.Style, style);
        }
        /// <summary>
        /// Sets a CSS style for the specified columns e.g. "background-color:gold; color:steelblue"
        /// </summary>
        public void SetColumnStyle(string[] columnNames, string style)
        {
            foreach (var columnName in columnNames)
            {
                SetColumnStyle(columnName, style);
            }
        }

        /// <summary>
        /// Overrides the default database type for the specified column
        /// </summary>
        public void SetColumnDataType(string columnName, Type type)
        {
            SetColumnProperty(columnName, ColumnPropertyType.DataType, type.ToString().Split(".").Last());
        }
        /// <summary>
        /// Overrides the default database type for the specified columns
        /// </summary>
        public void SetColumnDataType(string[] columnNames, Type type)
        {
            foreach (var columnName in columnNames)
            {
                SetColumnDataType(columnName, type);
            }
        }

        /// <summary>
        /// Sets the column as the foreign key in the linked control
        /// </summary>
        public void SetColumnAsForeignKey(string columnName)
        {
            SetColumnProperty(columnName, ColumnPropertyType.ForeignKey, true);
        }

        /// <summary>
        /// Sets the display format for the date/numeric column
        /// </summary>
        public void SetColumnFormat(string columnName, string format)
        {
            SetColumnProperty(columnName, ColumnPropertyType.Format, format);
        }
        /// <summary>
        /// Sets the display format for the list of date/numeric columns
        /// </summary>
        public void SetColumnFormat(string[] columnNames, string format)
        {
            foreach (var columnName in columnNames)
            {
                SetColumnFormat(columnName, format);
            }
        }
        /// <summary>
        /// Sets the label for the column
        /// </summary>
        public void SetColumnLabel(string columnName, string label)
        {
            SetColumnProperty(columnName, ColumnPropertyType.Label, label);
        }
        /// <summary>
        /// Indicates the contents of the column contains and should be rendered as a an image e.g. png, jpg
        /// </summary>
        public void SetColumnAsImage(string columnName, ImageConfiguration imageConfiguration)
        {
            SetColumnAsBlob(columnName, imageConfiguration, true);
        }

        /// <summary>
        /// Indicates the contents of the column contains should be rendered as a file e.g pdf, xlsx etc
        /// </summary>
        public void SetColumnAsFile(string columnName, FileConfiguration fileConfiguration)
        {
            SetColumnAsBlob(columnName, fileConfiguration, false);
        }

        private void SetColumnAsBlob(string columnName, BinaryConfiguration configuration, bool image)
        {
            ColumnPropertyType columnPropertyType = image ? ColumnPropertyType.Image : ColumnPropertyType.Download;
            SetColumnProperty(columnName, columnPropertyType, true);

            var extensionsWithMimeType = configuration.Extensions.Select(e => e = $"{e.Replace(".", string.Empty)}|{GetMimeTypeForFileExtension(e)}").ToList();

            SetColumnProperty(columnName, ColumnPropertyType.Extension, string.Join(",", extensionsWithMimeType));

            if (configuration.MetaDataColumns != null)
            {
                foreach (FileMetaData fileMetaData in configuration.MetaDataColumns.Keys)
                {
                    string metaDataColumnName = configuration.MetaDataColumns[fileMetaData];
                    SetColumnProperty(metaDataColumnName, ColumnPropertyType.UploadMetaData, fileMetaData);
                    SetColumnProperty(metaDataColumnName, ColumnPropertyType.UploadMetaDataColumn, columnName);
                }
            }
        }

        protected void SetColumnProperty(string columnName, Enum propertyType, object propertyValue)
        {
            columnName = FindColumnName(columnName);
            _columnProperties.Add(new ColumnProperty(columnName.ToLower(), propertyType, propertyValue));

            string FindColumnName(string name)
            {
                if (Columns.Any(c => c.ToLower() == name.ToLower()))
                {
                    return name;
                }

                var splitChars = new string[] { ".", " " };
                foreach (string splitChar in splitChars)
                {
                    var columnExpr = Columns.FirstOrDefault(c => c.Split(splitChar).Last().ToLower() == name.ToLower());

                    if (columnExpr != null)
                    {
                        return columnExpr;
                    }
                }

                if (propertyType is ColumnPropertyType)
                {
                    if ((ColumnPropertyType)propertyType == ColumnPropertyType.DataOnly)
                    {
                        if (Columns.Any())
                        {
                            Columns.Add(name);
                        }
                    }
                }
                return name;
            }
        }

        protected string ColumnExpressions()
        {
            if (Columns.Any() == false)
            {
                return string.Empty;
            }
            return $"setColumnExpressions(\"{string.Join("\",\"", Columns.Select(c => EncodingHelper.Encode(c)).ToList())}\");";
        }
        protected string ColumnKeys()
        {
            if (Columns.Any() == false)
            {
                return string.Empty;
            }
            return $"setColumnKeys(\"{string.Join("\",\"", Columns.Select(c => EncodingHelper.Encode(c.ToLower())).ToList())}\");";
        }
        protected string ColumnLabels()
        {
            if (Labels.Any() == false)
            {
                return string.Empty;
            }
            return $"setColumnLabels(\"{string.Join("\",\"", Labels)}\");";
        }

        protected string ColumnProperties()
        {
            var script = _columnProperties.Select(x => $"setColumnProperty(\"{EncodingHelper.Encode(x.ColumnName)}\",\"{LowerCaseFirstLetter(x.PropertyType.ToString())}\",{PropertyValue(x.PropertyValue, x.PropertyType)});").ToList();
            return string.Join(Environment.NewLine, script);

            string PropertyValue(object value, Enum propertyType)
            {
                if (value is bool)
                {
                    return value.ToString()!.ToLower();
                }

                switch (propertyType)
                {
                    case ColumnPropertyType.Lookup:
                        value = EncodingHelper.Encode(value.ToString());
                        break;
                    case ColumnPropertyType.InputValidation:
                        return Serialize(value);
                }

                return $"\"{value}\"";
            }
        }

        protected void AddProperties(List<string> properties)
        {
            AddProperty(ToolbarButtonStyle, nameof(ToolbarButtonStyle), properties);
            AddProperty(ToolbarPosition, nameof(ToolbarPosition), properties);
            AddProperty(Search, nameof(Search), properties);
            AddProperty(Culture, nameof(Culture), properties);
            AddProperty(QuickSearch, nameof(QuickSearch), properties);
            AddProperty(Navigation, nameof(Navigation), properties);
            AddProperty(Insert, nameof(Insert), properties);
            AddProperty(Delete, "_delete", properties);
            AddProperty(OptimizeForLargeDataset, nameof(OptimizeForLargeDataset), properties);
            AddProperty(ParentControlType, nameof(ParentControlType), properties);
            AddProperty(ParentChildRelationship, nameof(ParentChildRelationship), properties);
            AddProperty(MaxImageHeight, nameof(MaxImageHeight), properties);
            AddProperty(EncodingHelper.Encode(FixedFilterSql), nameof(FixedFilterSql), properties);
            AddProperty(EncodingHelper.Encode(InitialOrderBy), nameof(InitialOrderBy), properties);
            if (FixedFilterParams.Count > 0)
            {
                properties.Add($"fixedFilterParams = {Serialize(FixedFilterParams)};");
            }
        }
    }
}