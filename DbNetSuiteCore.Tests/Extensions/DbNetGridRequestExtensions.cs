using DbNetSuiteCore.Models.DbNetGrid;

namespace DbNetSuiteCore.Tests.Extensions
{
    public static class DbNetGridRequestExtensions
    {
        public static GridColumn GetColumn(this DbNetGridRequest request, string columnName)
        {
            if (request.Columns.FirstOrDefault(c => c.ColumnExpression == columnName) == null)
            {
                GridColumn gridColumn = new GridColumn(columnName) { Unmatched = true };
                request.Columns.Add(gridColumn);
            }
            return request.Columns.First(c => c.ColumnExpression == columnName);
        }
    }
}
