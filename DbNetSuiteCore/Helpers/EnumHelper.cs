using System;
using System.ComponentModel;
using System.Data;

namespace DbNetSuiteCore.Helpers
{
    public static class EnumHelper
    {
        public static DataTable EnumToDataTable(Type enumType, bool useNameAsValue)
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
    }
}
