using DbNetSuiteCore.Enums.DbNetGrid;
using System;
using System.Collections.Generic;

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

        public override bool Equals(object obj)
        {
            return Equals(obj as ColumnProperty);
        }

        public bool Equals(ColumnProperty obj)
        {
            return ColumnName == obj.ColumnName && PropertyType.ToString() == obj.PropertyType.ToString();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
