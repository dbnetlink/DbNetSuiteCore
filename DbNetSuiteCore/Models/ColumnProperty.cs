using DbNetSuiteCore.Enums;

namespace DbNetSuiteCore.Models
{
    internal class ColumnProperty
    {
        public string ColumnName { get; set; }
        public ColumnPropertyType PropertyType { get; set; }
        public object PropertyValue { get; set; }

        public ColumnProperty(string columnName, ColumnPropertyType propertyType, object propertyValue) 
        { 
            ColumnName = columnName;
            PropertyType = propertyType;
            PropertyValue = propertyValue;
        }
    }
}
