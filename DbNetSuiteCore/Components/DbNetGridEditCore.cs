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
        internal List<ColumnProperty> _columnProperties { get; set; } = new List<ColumnProperty>();

        /// <summary>
        /// Selects the columns to be displayed in the grid
        /// </summary>
        public List<string> Columns { get; set; } = new List<string>();
        /// <summary>
        /// Labels for the columns specified in the Columns property
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();
        public DbNetGridEditCore(string connection, string id = null) : base(connection, id)
        {
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
    }
}