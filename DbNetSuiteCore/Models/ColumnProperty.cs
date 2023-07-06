using DbNetSuiteCore.Enums.DbNetGrid;
using System;

namespace DbNetSuiteCore.Models
{
    internal class ColumnProperty
    {
        public string ColumnName { get; set; }
        public Enum PropertyType { get; set; }
        public object PropertyValue { get; set; }

        internal ColumnProperty(string columnName, Enum propertyType, object propertyValue) 
        { 
            ColumnName = columnName;
            PropertyType = propertyType;
            PropertyValue = propertyValue;
        }
    }
}
