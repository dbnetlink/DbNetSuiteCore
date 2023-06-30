using System.Data;
using System.Linq;
using DbNetSuiteCore.Models.DbNetGrid;

namespace DbNetSuiteCore.Extensions
{
    public static class DataTableExtension
    {
        public static int? ColumnIndex(this DataTable dataTable, GridColumn gridColumn)
        {
            return dataTable.Columns.Cast<DataColumn>()
                .FirstOrDefault(c => c.ColumnName.ToLower() == gridColumn.ColumnName.ToLower())?.Ordinal;
        }

        public static object ColumnValue(this DataTable dataTable, int rowIdx, int colIdx)
        {
            return dataTable.Rows[rowIdx].ItemArray[colIdx];
        }

        public static object ColumnValue(this DataTable dataTable, int rowIdx, GridColumn gridColumn)
        {
            return dataTable.Rows[rowIdx].ItemArray[dataTable.ColumnIndex(gridColumn) ?? 0];
        }
    }
}