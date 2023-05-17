using System;
using System.Data;

namespace DbNetSuiteCore.Extensions
{
    public static class DataRowExtension
    {
        public static short NumericScale(this DataRow dataRow)
        {
            return Convert.ToInt16(dataRow["NumericScale"]);
        }
        public static int ColumnSize(this DataRow dataRow)
        {
            return Convert.ToInt32(dataRow["ColumnSize"]);
        }
        public static string ColumnName(this DataRow dataRow)
        {
            return dataRow["ColumnName"].ToString();
        }
        public static string BaseTableName(this DataRow dataRow)
        {
            return dataRow["BaseTableName"].ToString();
        }
        public static object BaseSchemaName(this DataRow dataRow)
        {
            return dataRow["BaseSchemaName"];
        }
        public static bool IsHidden(this DataRow dataRow)
        {
            return BoolValue(dataRow, "IsHidden", false) || dataRow.UnsupportedDataType();
        }
        public static Type DataType(this DataRow dataRow)
        {
            return  (dataRow["DataType"] is Type) ? (Type)dataRow["DataType"] : null;
        }
        public static bool IsKey(this DataRow dataRow)
        {
            return BoolValue(dataRow, "IsKey", false);
        }
        public static bool IsAutoIncrement(this DataRow dataRow)
        {
            return BoolValue(dataRow, "IsAutoIncrement", false);
        }

        public static string DataTypeName(this DataRow dataRow) => dataRow["DataTypeName"]?.ToString() ?? string.Empty;

        private static bool BoolValue(DataRow dataRow, string propertyName, bool defaultValue)
        {
            if (dataRow.Table.Columns.Contains(propertyName))
            {
                bool value;
                if (bool.TryParse(dataRow[propertyName]?.ToString(), out value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        private static bool UnsupportedDataType(this DataRow dataRow)
        {
            string[] unsupportedDataTypes = new[] { "hierarchyid", "geography" };
            foreach (var unsupportedDataType in unsupportedDataTypes)
            {
                if (dataRow.DataTypeName().ToLower().EndsWith($"sys.{unsupportedDataType}"))
                    return true;
            }
            return false;
        }
    }
}