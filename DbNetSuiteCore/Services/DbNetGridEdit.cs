using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DbNetSuiteCore.Models.DbNetGrid;
using System.Collections.Specialized;
using System.Data;
using System;
using System.Linq;
using DbNetSuiteCore.Extensions;
using System.Threading;

namespace DbNetSuiteCore.Services
{
    public class DbNetGridEdit : DbNetSuite
    {
        private string _fromPart;
        public string FromPart
        {
            get => EncodingHelper.Decode(_fromPart);
            set => _fromPart = value;
        }
        public bool Navigation { get; set; }
        public bool QuickSearch { get; set; }
        public string QuickSearchToken { get; set; } = string.Empty;
        public bool Search { get; set; }

        public DbNetGridEdit(AspNetCoreServices services) : base(services)
        {
        }


        protected List<T> ConfigureColumns<T>(List<T> columns, System.Data.DataTable dataTable, bool groupBy)
        {
            int columnIndex = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                if (row.IsHidden())
                {
                    continue;
                }

                DbColumn column = columns.FirstOrDefault(c => MatchingColumn(c, row.ColumnName(), row.BaseTableName())) as DbColumn;

                column.Index = columnIndex++;
                ConfigureColumn(column, row);

                if (column.DataType == nameof(String))
                {
                    column.QuickSearch = true;
                }

                if (string.IsNullOrEmpty(column.Lookup) == false)
                {
                    column.DataType = nameof(String);
                }

                if (column.PrimaryKey == false)
                {
                    column.PrimaryKey = row.IsKey() || row.IsAutoIncrement();
                    column.AutoIncrement = row.IsAutoIncrement();
                }

                if (column is GridColumn)
                {
                    var gridColumn = (GridColumn)column;

                    if (gridColumn.GroupHeader)
                    {
                        gridColumn.Display = false;
                    }

                    if (gridColumn.TotalBreak)
                    {
                        if (gridColumn.ClearDuplicateValue.HasValue == false)
                        {
                            gridColumn.ClearDuplicateValue = true;
                        }
                    }

                    if (gridColumn.ClearDuplicateValue.HasValue == false || groupBy)
                    {
                        gridColumn.ClearDuplicateValue = false;
                    }

                    if (gridColumn.DataOnly)
                    {
                        column.Display = false;
                    }

                    if (gridColumn.Filter && gridColumn.FilterMode == FilterColumnSelectMode.List)
                    {
                        if (string.IsNullOrEmpty(column.Lookup))
                        {
                            column.Lookup = EncodingHelper.Encode($"select distinct {column.ColumnExpression} from {FromPart}");
                        }
                    }
                }
            }

            columns = columns.OrderBy(c => (c as DbColumn).Index).ToList();

            using (Database)
            {
                Database.Open();
                foreach (object o in columns.Where(c => string.IsNullOrEmpty((c as DbColumn).Lookup) == false))
                {
                    DbColumn col = (DbColumn)o;
                    string sql = Database.UpdateConcatenationOperator(col.Lookup);

                    ListDictionary @params = Database.ParseParameters(sql);

                    if (@params.Count > 0)
                        sql = Regex.Replace(sql, " where .*", " where 1=2", RegexOptions.IgnoreCase);

                    System.Data.DataTable lookupDataTable;
                    try
                    {
                        lookupDataTable = Database.GetSchemaTable(sql);
                    }
                    catch (Exception)
                    {
                        throw new Exception($"Error in column lookup sql {sql}");
                    }

                    int textRowIndex = (lookupDataTable.Rows.Count == 1) ? 0 : 1;

                    try
                    {
                        DataRow textColumnRow = lookupDataTable.Rows[textRowIndex];
                        col.LookupDataType = textColumnRow.DataType().Name;
                        col.LookupTable = textColumnRow.BaseTableName();
                        col.LookupTextField = textColumnRow.ColumnName();
                        col.LookupTextExpression = col.LookupTextField;

                        if (col.LookupDataType == nameof(String))
                        {
                            col.QuickSearch = true;
                        }

                        if (textRowIndex > 0)
                        {
                            string[] cols = GetSelectColumns(col.Lookup);
                            if (cols.Length > 1)
                                col.LookupTextExpression = cols[1];
                        }

                        col.LookupValueField = lookupDataTable.Rows[0].ColumnName();
                    }
                    catch (Exception ex)
                    {
                        ThrowException("Error [<b>" + ex.Message + "</b>] in column <b>Lookup</b> property<br /><br />The SQL is not in the expected format.  Expected format is  \"SELECT IdField, TextDescriptionField FROM Table\"");
                        return null;
                    }
                }
            }

            foreach (object o in columns.Where(c => (c as DbColumn).Unmatched && (c as DbColumn).PrimaryKey))
            {
                DbColumn column = (DbColumn)o;
                column.Unmatched = false;
                column.Display = false;
            }

            while (columns.Any(c => (c as DbColumn).Unmatched))
            {
                columns.Remove(columns.First(c => (c as DbColumn).Unmatched));
            }

            return columns;
        }

        protected string BuildSelectPart<T>(QueryBuildModes buildMode, List<T> columns, bool groupBy = false)
        {
            if (buildMode == QueryBuildModes.Count)
            {
                return "count(*)";
            }

            List<string> selectParts = new List<string>();

            foreach (object c in columns)
            {
                DbColumn column = (DbColumn)c;
                GridColumn gridColumn = (GridColumn)c;

                column.ColumnExpression = EncodingHelper.Encode(Database.UpdateConcatenationOperator(column.ColumnExpression));

                if (buildMode == QueryBuildModes.PrimaryKeysOnly)
                    if (!column.PrimaryKey)
                        continue;

                if (column.Unmatched || column.ColumnExpression == "*")
                {
                    continue;
                }

                string columnExpression = column.ColumnExpression;
                switch (buildMode)
                {
                    case QueryBuildModes.Configuration:
                    case QueryBuildModes.View:
                        break;
                    case QueryBuildModes.Totals:
                        if (gridColumn.Aggregate == AggregateType.None)
                        {
                            if (gridColumn.TotalBreak == false)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            columnExpression = $"{gridColumn.Aggregate}({AggregateExpression(gridColumn)}) as {gridColumn.ColumnName}";
                        }
                        break;
                    case QueryBuildModes.Normal:
                    case QueryBuildModes.Spreadsheet:
                        if (column.Binary && buildMode == QueryBuildModes.Spreadsheet)
                        {
                            continue;
                        }
                        if (groupBy)
                        {
                            if (gridColumn.Aggregate != AggregateType.None)
                            {
                                columnExpression = $"{gridColumn.Aggregate}({AggregateExpression(column)}) as {column.ColumnName}";
                            }
                        }
                        break;
                    default:
                        break;
                }

                selectParts.Add(columnExpression);
            }

            if (selectParts.Count == 0)
            {
                switch (buildMode)
                {
                    case QueryBuildModes.PrimaryKeysOnly:
                        ThrowException("Unable to find any primary key columns.");
                        break;
                    case QueryBuildModes.Totals:
                        return string.Empty;
                }
            }

            return string.Join(", ", selectParts.ToArray());
        }

        private string AggregateExpression(DbColumn c)
        {
            return Regex.Replace(c.ColumnExpression, @" as \w*$", "", RegexOptions.IgnoreCase);
        }
        protected bool MatchingColumn(object o, string columnName, string tableName)
        {
            DbColumn c = o as DbColumn;
            if (c.ColumnExpression.Contains("."))
            {
                if (Database.UnqualifiedDbObjectName(c.ColumnExpression).ToLower() ==
                    $"{tableName}.{columnName}".ToLower())
                {
                    return true;
                }
            }

            if (c.ColumnExpression.Split(" ").Last().ToLower() == columnName.ToLower())
            {
                return true;
            }

            return c.ColumnName.ToLower() == columnName.ToLower() || c.ColumnExpression.ToLower() == columnName.Split('.').Last().ToLower();
        }


        protected void ConfigureColumn(DbColumn column, DataRow row)
        {
            column.Unmatched = false;

            if (column.BaseTableName == string.Empty)
            {
                column.BaseTableName = row.BaseTableName();
            }

            GetBaseSchemaName(column, row);

            column.ColumnName = row.ColumnName();
            column.ColumnSize = row.ColumnSize();

            if (column.Label == "")
            {
                column.Label = GenerateLabel(column.ColumnName);
            }

            column.OriginalDataType = row.DataType()?.Name ?? nameof(String);

            if (String.IsNullOrEmpty(column.DataType))
            {
                switch (row.DataTypeName().ToLower())
                {
                    case "datetime":
                        column.DataType = nameof(DateTime);
                        break;
                    default:
                        if (row.DataType() == null)
                        {
                            column.ColumnExpression = $"'' as {column.ColumnName}";
                            column.DataType = nameof(String);
                        }
                        else
                        {
                            column.DataType = row.DataType().Name;
                        }
                        break;
                }
            }

            column.DbDataType = row.DataTypeName();

            if (string.IsNullOrEmpty(column.Format))
            {
                switch (column.DataType)
                {
                    case nameof(DateTime):
                        column.Format = "d";
                        break;
                    case nameof(TimeSpan):
                        column.Format = "t";
                        break;
                }
            }

        }

        internal void GetBaseSchemaName(DbColumn column, DataRow row)
        {
            if (column.BaseSchemaName == "")
                if (row.Table.Columns.Contains("BaseSchemaName"))
                    if (row.BaseSchemaName() != DBNull.Value)
                        column.BaseSchemaName = Convert.ToString(row.BaseSchemaName());
        }

        internal string GetBaseTableName(object tableName)
        {
            string baseTableName = "";

            if (Regex.IsMatch(this.FromPart, @"\b" + tableName.ToString() + @"\b", RegexOptions.IgnoreCase))
                baseTableName = tableName.ToString();

            return baseTableName;
        }

        static public string GenerateLabel(string label)
        {
            label = Regex.Replace(label, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
            return Capitalise(label.Replace("_", " ").Replace(".", " "));
        }

        internal static string Capitalise(string text)
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        protected string[] GetSelectColumns(string sql)
        {
            Match match = Regex.Match(sql, @"select (.*?) from", RegexOptions.IgnoreCase);
            string columnList = match.Groups[1].ToString();

            string[] patterns = { @"(\'.*\')", @"(\(.*\))" };
            foreach (string p in patterns)
            {
                match = Regex.Match(columnList, p);

                foreach (System.Text.RegularExpressions.Group g in match.Groups)
                    if (!String.IsNullOrEmpty(g.Value))
                        columnList = columnList.Replace(g.Value, g.Value.Replace(",", "~"));
            }

            string[] columnExpressions = columnList.Split(',');

            for (int I = 0; I < columnExpressions.Length; I++)
                columnExpressions[I] = columnExpressions[I].Replace("~", ",");

            return columnExpressions;
        }
    }
}

