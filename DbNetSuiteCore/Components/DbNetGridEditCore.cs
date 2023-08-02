using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Enums.DbNetGrid;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using System;
using System.Collections.Generic;
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
        /// Allow insertion of new records
        /// </summary>
        public bool? Insert { get; set; } = null;
        /// <summary>
        /// Labels for the columns specified in the Columns property
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();
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
        }

        /// <summary>
        /// Creates a lookup based on a list of existing distinct values for the column
        /// </summary>
        public void SetColumnLookup(string columnName)
        {
            SetColumnLookup(columnName, new Lookup(_fromPart, columnName, null, true));
        }

        /// <summary>
        /// Shows/hides the column in the control
        /// </summary>
        public void SetColumnDisplay(string columnName, bool display = false)
        {
            SetColumnProperty(columnName, ColumnPropertyType.Display, display);
        }
        /// <summary>
        /// Shows/hides the list of columns in the control
        /// </summary>
        public void SetColumnDisplay(string[] columnNames, bool display = false)
        {
            foreach (var columnName in columnNames)
            {
                SetColumnDisplay(columnName, display);
            }
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
        /// Indicates the contents of the column should be rendered as a an image
        /// </summary>
        public void SetColumnImage(string columnName, bool image = true)
        {
            SetColumnProperty(columnName, ColumnPropertyType.Image, image);
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
                        Columns.Add(name);
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
                if (propertyType.ToString() == ColumnPropertyType.Lookup.ToString())
                {
                    value = EncodingHelper.Encode(value.ToString());
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
        }
    }
}