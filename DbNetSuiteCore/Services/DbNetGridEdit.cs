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
using DbNetSuiteCore.Attributes;
using static DbNetSuiteCore.Utilities.DbNetDataCore;
using System.Text.Json.Serialization;
using System.Text.Json;
using DbNetSuiteCore.Models.DbNetEdit;
using DbNetSuiteCore.ViewModels.DbNetGrid;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DbNetSuiteCore.Constants;
using DbNetSuiteCore.Enums.DbNetEdit;

namespace DbNetSuiteCore.Services
{
    public class DbNetGridEdit : DbNetSuite
    {
        private string _fromPart;
        private string _primaryKey;

        protected Dictionary<string, DataTable> _lookupTables = new Dictionary<string, DataTable>();
        protected List<DbColumn> DbColumns
        {
            get
            {
                if (this is DbNetGrid)
                {
                    return (this as DbNetGrid).Columns.Cast<DbColumn>().ToList();
                }
                else if (this is DbNetEdit)
                {
                    return (this as DbNetEdit).Columns.Cast<DbColumn>().ToList();
                }
                return null;
            }
        }
        public string ColumnName { get; set; } = String.Empty;

        public bool Delete { get; set; } = false;
        public string Extension { get; set; } = string.Empty;

        public string FromPart
        {
            get => EncodingHelper.Decode(_fromPart);
            set => _fromPart = value;
        }
        public bool Insert { get; set; } = false;
        public int LookupColumnIndex { get; set; }
        public int MaxImageHeight { get; set; } = 40;
        public bool Navigation { get; set; }
        public bool OptimizeForLargeDataset { get; set; } = false;

        public string PrimaryKey
        {
            get => EncodingHelper.Decode(_primaryKey);
            set => _primaryKey = value;
        }
        public Dictionary<string, object> PrimaryKeyValues
        {
            get => JsonSerializer.Deserialize<Dictionary<string, object>>(PrimaryKey);
        }
        public bool QuickSearch { get; set; }
        public string QuickSearchToken { get; set; } = string.Empty;
        public ParentChildRelationship? ParentChildRelationship { get; set; } = null;
        public bool Search { get; set; }
        public string SearchFilterJoin { get; set; } = "and";
        public List<SearchParameter> SearchParams { get; set; } = new List<SearchParameter>();


        public DbNetGridEdit(AspNetCoreServices services) : base(services)
        {
        }


        protected List<T> ConfigureColumns<T>(List<T> columns, bool groupBy = false)
        {
            DataTable dataTable;

            using (Database)
            {
                Database.Open();

                string selectPart = columns.Any(c => (c as DbColumn).Unmatched == false) == false ? "*" : BuildSelectPart(QueryBuildModes.Configuration, columns);
                string sql = $"select {selectPart} from {FromPart} where 1=2";

                dataTable = Database.GetSchemaTable(sql);

                foreach (DataRow row in dataTable.Rows)
                {
                    if (row.IsHidden())
                    {
                        continue;
                    }

                    DbColumn column = columns.Where(c => MatchingColumn((c as DbColumn), row.ColumnName(), row.BaseTableName())).FirstOrDefault() as DbColumn;

                    if (column == null)
                    {
                        AddColumn(row);
                    }
                }
            }
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

                column.AllowsNull = row.AllowsNull();

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

                if (column is EditColumn)
                {
                    var editColumn = (EditColumn)column;

                    if (editColumn.DataType == "Byte[]")
                    {
                        editColumn.EditControlType = EditControlType.Upload;
                    }

                    if (editColumn.DataType == nameof(String))
                    {
                        if (editColumn.ColumnSize > 100)
                        {
                            if (editColumn.EditControlType == EditControlType.Auto)
                            {
                                editColumn.EditControlType = EditControlType.TextArea;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(editColumn.Lookup) == false)
                    {
                        if (editColumn.EditControlType == EditControlType.Auto)
                        {
                            editColumn.EditControlType = EditControlType.DropDownList;
                        }
                    }

                    if (editColumn.DataType == nameof(Boolean))
                    {
                        editColumn.EditControlType = EditControlType.CheckBox;
                        editColumn.ColumnSize = 0;
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

                    DataTable lookupDataTable;
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

            if (this is DbNetGrid)
            {
                if (columns.Any(c => (c as DbColumn).Browse))
                {

                    foreach (object o in columns.Where(c => (c as DbColumn).Browse == false))
                    {
                        DbColumn column = (DbColumn)o;
                        column.Display = false;
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

            if (Search)
            {
                if (columns.Any(c => (c as DbColumn).UserAssignedSearch) == false)
                {
                    foreach (object o in columns)
                    {
                        DbColumn column = (DbColumn)o;
                        column.Search = true;
                    }
                }
            }

            GetLookupTables(columns);

            return columns;
        }

        protected virtual void AddColumn(DataRow row)
        {
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
                        if ((column as GridColumn).Aggregate == AggregateType.None)
                        {
                            if ((column as GridColumn).TotalBreak == false)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            columnExpression = $"{(column as GridColumn).Aggregate}({AggregateExpression(column)}) as {column.ColumnName}";
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
                            if ((column as GridColumn).Aggregate != AggregateType.None)
                            {
                                columnExpression = $"{(column as GridColumn).Aggregate}({AggregateExpression(column)}) as {column.ColumnName}";
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

        protected List<string> QuickSearchFilter(List<DbColumn> columns)
        {
            List<string> quickSearchFilter = new List<string>();
            foreach (DbColumn dbColumn in columns.Where(c => c.QuickSearch))
            {
                if (string.IsNullOrEmpty(dbColumn.Lookup) || dbColumn.LookupColumns < 2)
                {
                    quickSearchFilter.Add($"{dbColumn.ColumnExpression} like {ParamName(ParamNames.QuickSearchToken)}");
                }
                else
                {
                    quickSearchFilter.Add($"{dbColumn.ColumnExpression} in ({SearchLookupSql(dbColumn, ParamNames.QuickSearchToken)})");
                }
            }

            return quickSearchFilter;
        }

        protected string SearchLookupSql(DbColumn col, string paramName)
        {
            Match m = Regex.Match(col.Lookup, @"select (.*?) from", RegexOptions.IgnoreCase);
            string columnList = m.Groups[1].ToString();

            string valueColumn = Database.QualifiedDbObjectName(col.LookupValueField);

            if (valueColumn == String.Empty)
            {
                valueColumn = GetSelectColumns(col.Lookup).First();
            }

            string sql = col.Lookup.Replace(columnList, valueColumn);
            sql = Regex.Replace(sql, " order by .*", "", RegexOptions.IgnoreCase);

            string filter = $"({StripColumnRename(col.LookupTextExpression)} like {ParamName(paramName)})";

            if (Regex.IsMatch(sql, " where ", RegexOptions.IgnoreCase))
                sql = Regex.Replace(sql, " where ", " where " + filter + " and ", RegexOptions.IgnoreCase);
            else
                sql += " where " + filter;

            return sql;
        }

        protected List<string> SearchDialogFilter(List<DbColumn> columns)
        {
            List<string> searchFilterPart = new List<string>();
            foreach (SearchParameter searchParameter in SearchParams)
            {
                DbColumn dbColumn = columns[searchParameter.ColumnIndex];

                searchFilterPart.Add($"{RefineSearchExpression(dbColumn)} {FilterExpression(searchParameter, dbColumn)}");
            }

            return searchFilterPart;
        }

        protected void AddSearchDialogParameters(List<DbColumn> columns, ListDictionary parameters)
        {
            foreach (SearchParameter searchParameter in SearchParams)
            {
                DbColumn dbColumn = columns[searchParameter.ColumnIndex];

                string expression = searchParameter.SearchOperator.GetAttribute<FilterExpressionAttribute>()?.Expression ?? "{0}";

                if (expression.Contains("{0}"))
                {
                    AddSearchFilterParams(parameters, dbColumn, searchParameter, ParamNames.SearchFilter1, searchParameter.Value1);
                }
                if (expression.Contains("{1}"))
                {
                    AddSearchFilterParams(parameters, dbColumn, searchParameter, ParamNames.SearchFilter2, searchParameter.Value2);
                }
            }
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
                    case "date":
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

            if (column.ColumnSize == 0)
            {
                column.ColumnSize = row.ColumnSize();
            }

            switch (column.DataType)
            {
                case nameof(DateTime):
                    column.ColumnSize = 8;
                    break;
                case nameof(Guid):
                    column.ColumnSize = 36;
                    break;
                default:
                    if (column.ColumnSize < 10)
                    {
                        column.ColumnSize = column.IsNumeric ? 10 : 20;
                    }
                    break;
            }
        }

        protected void DeleteRecord(DbNetSuiteResponse response)
        {
            if (IsReadOnly(response))
            {
                return;
            }

            List<string> filterPart = new List<string>();
            CommandConfig deleteCommand = new CommandConfig();

            filterPart.Add(PrimaryKeyFilter(deleteCommand.Params));
            deleteCommand.Sql = $"delete from {FromPart} where {string.Join(" and ", filterPart)}";

            try
            {
                using (Database)
                {
                    Database.Open();
                    Database.ExecuteNonQuery(deleteCommand);
                }

                response.Message = "Record deleted";
            }
            catch (Exception ex)
            {
                response.Message = $"Delete failed ({ex.Message})";
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

        internal static string[] GetSelectColumns(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return new string[] { };
            }

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
        protected async Task SearchDialog(DbNetSuiteResponse response)
        {
            var searchDialogViewModel = new SearchDialogViewModel();
            ReflectionHelper.CopyProperties(this, searchDialogViewModel);
            searchDialogViewModel.Columns = DbColumns;
            searchDialogViewModel.LookupTables = _lookupTables;
            response.Dialog = await HttpContext.RenderToStringAsync("Views/DbNetGrid/SearchDialog.cshtml", searchDialogViewModel);
        }
        protected async Task LookupDialog(DbNetSuiteResponse response)
        {
            var lookupDialogViewModel = new LookupDialogViewModel();
            ReflectionHelper.CopyProperties(this, lookupDialogViewModel);
            response.Dialog = await HttpContext.RenderToStringAsync("Views/DbNetGrid/LookupDialog.cshtml", lookupDialogViewModel);

            var dataTable = GetLookupData(this.LookupColumnIndex);
            string sortColumnName = dataTable.Columns[dataTable.Columns.Count - 1].ColumnName;
            var sortedDataTable = dataTable.AsEnumerable().OrderBy(x => x.Field<string>(sortColumnName)).ToList();
            // var dataView = new DataView(dataTable);
            // dataView.Sort = $"{sortColumnName} ASC";
            lookupDialogViewModel.LookupData = sortedDataTable;

            response.Html = await HttpContext.RenderToStringAsync("Views/DbNetGrid/LookupDialogContent.cshtml", lookupDialogViewModel);
        }

        private DataTable GetLookupData(int columnIndex)
        {
            DbColumn dbColumn = DbColumns.FirstOrDefault(c => c.Index == columnIndex);
            DataTable dataTable = new DataTable();

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query = new QueryCommandConfig();
                query.Sql = Database.UpdateConcatenationOperator(dbColumn!.Lookup);
                Database.ExecuteQuery(query);
                dataTable.Load(Database.Reader);
                Database.Close();
            }

            return dataTable;
        }

        protected bool ValidateRequest<T>(DbNetGridEditResponse response, List<T> columns)
        {
            response.Message = String.Empty;

            if (SearchParams.Any())
            {
                foreach (SearchParameter searchParameter in SearchParams)
                {
                    DbColumn dbColumn = columns[searchParameter.ColumnIndex] as DbColumn;

                    string expression = searchParameter.SearchOperator.GetAttribute<FilterExpressionAttribute>()?.Expression ?? "{0}";

                    switch (searchParameter.SearchOperator)
                    {
                        case SearchOperator.In:
                        case SearchOperator.NotIn:
                            foreach (string value in searchParameter.Value1.Split(','))
                            {
                                searchParameter.Value1Valid = ValidateUserValue(dbColumn, value);
                                if (searchParameter.Value1Valid == false)
                                {
                                    break;
                                }
                            }
                            break;
                        default:
                            if (expression.Contains("{0}"))
                            {
                                searchParameter.Value1Valid = ValidateUserValue(dbColumn, searchParameter.Value1);
                            }
                            if (expression.Contains("{1}"))
                            {
                                searchParameter.Value2Valid = ValidateUserValue(dbColumn, searchParameter.Value2);
                            }
                            break;
                    }
                }

                response.SearchParams = SearchParams;
                var invalid = SearchParams.Any(s => s.Value1Valid == false || s.Value2Valid == false);

                if (invalid)
                {
                    response.Error = true;
                    response.Message = Translate("HighlightedFormatInvalid");
                }
            }

            return true;
        }

        protected bool ValidateUserValue(DbColumn dbColumn, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            return ConvertToDbParam(value, dbColumn) == null ? false : true;
        }

        protected DataTable InitialiseDataTable()
        {
            DataTable dataTable = new DataTable();
            foreach (object o in DbColumns)
            {
                DbColumn dbColumn = (DbColumn)o;
                DataColumn dataColumn = new DataColumn(dbColumn.ColumnName);
                dataColumn.DataType = GetColumnType(dbColumn.OriginalDataType == "Byte[]" ? nameof(String) : dbColumn.OriginalDataType);
                dataTable.Columns.Add(dataColumn);
            }

            return dataTable;
        }

        protected Type GetColumnType(string typeName)
        {
            return Type.GetType("System." + typeName);
        }

        protected DbColumn InitialiseColumn(DbColumn column, DataRow row)
        {
            column.ColumnExpression = EncodingHelper.Encode(Database.QualifiedDbObjectName(row.ColumnName(), false));
            column.ColumnName = row.ColumnName();
            column.BaseTableName = row.BaseTableName();

            GetBaseSchemaName(column, row);

            column.AddedByUser = false;

            return column;
        }

        protected void GetLookupTables<T>(List<T> columns)
        {
            _lookupTables.Clear();

            using (Database)
            {
                foreach (object o in columns)
                {
                    DbColumn column = (DbColumn)o;
                    if (string.IsNullOrEmpty(column.Lookup))
                    {
                        continue;
                    }

                    if (column.Lookup.StartsWith("["))
                    {
                        _lookupTables.Add(column.ColumnKey, ArrayToDataTable(column.Lookup));
                    }
                    else
                    {
                        Database.Open();
                        string sql = Database.UpdateConcatenationOperator(column.Lookup);

                        ListDictionary @params = Database.ParseParameters(sql);

                        if (@params.Count > 0)
                            sql = Regex.Replace(sql, " where .*", " where 1=2", RegexOptions.IgnoreCase);

                        sql = AddLookupOrder(sql);
                        try
                        {
                            _lookupTables.Add(column.ColumnKey, Database.GetDataTable(new QueryCommandConfig(sql)));
                        }
                        catch (Exception ex)
                        {
                            ThrowException(ex.Message, sql);
                        }
                    }
                    Database.Close();
                }
            }
        }

        private string AddLookupOrder(string sql)
        {
            if (sql.IndexOf("order by", StringComparison.CurrentCultureIgnoreCase) > -1)
                return sql;

            if (sql.IndexOf("group by", StringComparison.CurrentCultureIgnoreCase) > -1)
                return sql;

            string[] columns = new string[0];

            Match m = Regex.Match(sql, "select (.*?) from ", RegexOptions.IgnoreCase);

            if (m.Success)
                columns = Regex.Replace(m.Groups[1].Value, @",(?=[^\']*\'([^\']*\'[^\']*\')*$)", "~").Split(',');

            if (columns.Length == 0)
                return sql;

            sql += $" order by {((columns.Length == 1) ? "1" : "2")}";
            return sql;
        }

        protected DataTable ArrayToDataTable(string jsonArray)
        {
            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.Converters.Add(new JsonStringEnumConverter());
            return ArrayToDataTable(JsonSerializer.Deserialize<object[]>(jsonArray, options));
        }

        protected DataTable ArrayToDataTable(object[] lookupObject)
        {
            DataTable dataTable = new DataTable();

            try
            {
                dataTable.Columns.Add("value", typeof(string));
                dataTable.Columns.Add("text", typeof(string));

                foreach (object r in lookupObject)
                {
                    object[] item;
                    if (r is string)
                        item = new object[] { r.ToString() };
                    else
                        item = (object[])r;

                    DataRow dataRow = dataTable.NewRow();
                    dataRow[0] = item[0].ToString();
                    if (item.Length > 1)
                        dataRow[1] = item[1].ToString();
                    else
                        dataRow[1] = dataRow[0];

                    dataTable.Rows.Add(dataRow);
                }
            }
            catch (Exception) { }

            return dataTable;
        }
        protected Dictionary<string, object> CreateRecord(DataTable dataTable, List<DbColumn> columns)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.DataType == typeof(Byte[]))
                {
                    continue;
                }
                DbColumn dbColumn = columns[column.Ordinal];
                var columnValue = dataTable.Rows[0][column.ColumnName];

                columnValue = columnValue == System.DBNull.Value ? string.Empty : columnValue;

                try
                {
                    if (dbColumn.IsNumeric)
                    {
                        columnValue = Convert.ToDecimal(columnValue).ToString(dbColumn.Format);
                    }
                    else if (dbColumn.DataType == nameof(DateTime))
                    {
                        columnValue = Convert.ToDateTime(columnValue).ToString(dbColumn.Format);
                    }
                }
                catch { }

                dictionary[column.ColumnName.ToLower()] = columnValue;
            }

            return dictionary;
        }
        public string SerialisePrimaryKey(DataRow dataRow)
        {
            Dictionary<string, object> primaryKeyValues = new Dictionary<string, object>();
            foreach (DbColumn dbColumn in DbColumns)
            {
                if (dbColumn.PrimaryKey)
                {
                    primaryKeyValues.Add(dbColumn.ColumnName, dataRow.ItemArray[dbColumn.Index]);
                }
            }
            return JsonSerializer.Serialize(primaryKeyValues);
        }
        protected string ParamName(DbColumn column, string suffix = "", bool parameterValue = false)
        {
            return Database.ParameterName($"{column.ColumnName}{suffix}");
        }
        protected string ParamName(DbColumn column, bool parameterValue = false)
        {
            return ParamName(column, string.Empty, parameterValue);
        }
        protected string ParamName(string paramName, bool parameterValue = false)
        {
            return Database.ParameterName(paramName, parameterValue);
        }
        protected object ConvertToDbParam(object value, DbColumn column)
        {
            if (column == null)
            {
                column = new DbColumn();
            };

            string dataType = column?.DataType ?? string.Empty;
            if (value == null)
            {
                if (dataType == "Byte[]")
                    return new byte[0];
                else
                    return DBNull.Value;
            }

            if (dataType == string.Empty)
                dataType = value.GetType().Name;

            if (value is string)
            {
                string valueString = (string)value;
                if (valueString.Equals("") || valueString.Equals(string.Empty))
                    return DBNull.Value;
            }

            if (value is JsonElement)
            {
                JsonElement jsonElement = (JsonElement)value;
                switch (jsonElement.ValueKind)
                {
                    case JsonValueKind.String:
                        value = jsonElement.GetString();
                        break;
                    case JsonValueKind.Number:
                        value = jsonElement.GetUInt64();
                        break;
                    default:
                        throw new Exception($"jsonElement.ValueKind => {jsonElement.ValueKind} not supported");
                }
                dataType = value.GetType().Name;
            }

            object paramValue = string.Empty;
            try
            {
                switch (dataType)
                {
                    case nameof(Boolean):
                        if (value.ToString() == String.Empty)
                            paramValue = DBNull.Value;
                        else
                            paramValue = ParseBoolean(value.ToString());
                        break;
                    case nameof(TimeSpan):
                        paramValue = TimeSpan.Parse(DateTime.Parse(value.ToString()).ToString(column.Format));
                        break;
                    case nameof(DateTime):
                        paramValue = Convert.ChangeType(value, Type.GetType($"System.{nameof(DateTime)}"));
                        //paramValue = DateTime.ParseExact(value.ToString(), column.Format, CultureInfo.InvariantCulture);
                        break;
                    case nameof(Byte):
                        paramValue = value;
                        break;
                    case nameof(Guid):
                        paramValue = new Guid(value.ToString());
                        break;
                    case nameof(Int16):
                    case nameof(Int32):
                    case nameof(Int64):
                    case nameof(Decimal):
                    case nameof(Single):
                    case nameof(Double):
                        if (string.IsNullOrEmpty(column.Format) == false)
                        {
                            var cultureInfo = Thread.CurrentThread.CurrentCulture;
                            value = value.ToString().Replace(cultureInfo.NumberFormat.CurrencySymbol, "");
                        }
                        paramValue = Convert.ChangeType(value, GetColumnType(dataType));
                        break;
                    case nameof(UInt16):
                    case nameof(UInt32):
                    case nameof(UInt64):
                        paramValue = Convert.ChangeType(value, GetColumnType(dataType.Replace("U", string.Empty)));
                        break;
                    default:
                        paramValue = Convert.ChangeType(value, GetColumnType(dataType));
                        break;
                }
            }
            catch (Exception e)
            {
                ThrowException(e.Message, "ConvertToDbParam: Value: " + value.ToString() + " DataType:" + dataType);
                return null;
            }

            switch (dataType)
            {
                case nameof(DateTime):
                    switch (Database.Database)
                    {
                        case DatabaseType.SQLite:
                            paramValue = Convert.ToDateTime(paramValue).ToString("yyyy-MM-dd");
                            break;
                    }
                    break;
            }

            return paramValue;
        }

        protected void AddSearchFilterParams(ListDictionary parameters, DbColumn dbColumn, SearchParameter searchParameter, string prefix, string value)
        {
            string template = "{0}";
            switch (searchParameter.SearchOperator)
            {
                case SearchOperator.Contains:
                case SearchOperator.DoesNotContain:
                    template = "%{0}%";
                    break;
                case SearchOperator.StartsWith:
                case SearchOperator.DoesNotStartWith:
                    template = "{0}%";
                    break;
                case SearchOperator.EndsWith:
                case SearchOperator.DoesNotEndWith:
                    template = "%{0}";
                    break;
            }

            switch (searchParameter.SearchOperator)
            {
                case SearchOperator.In:
                case SearchOperator.NotIn:
                    string[] values = value.Split(",");
                    for (var i = 0; i < values.Length; i++)
                    {
                        parameters.Add(ParamName(dbColumn, $"{prefix}{i}", true), ConvertToDbParam(template.Replace("{0}", values[i]), dbColumn));
                    }
                    break;
                case SearchOperator.True:
                case SearchOperator.False:
                    parameters.Add(ParamName(dbColumn, prefix, true), ConvertToDbParam(template.Replace("{0}", searchParameter.SearchOperator.ToString().ToLower()), dbColumn));
                    break;
                default:
                    parameters.Add(ParamName(dbColumn, prefix, true), ConvertToDbParam(template.Replace("{0}", value), dbColumn));
                    break;
            }
        }

        protected string ForeignKeyFilter(ListDictionary parameters = null)
        {
            DbColumn fkColumn = DbColumns.Where(c => c.ForeignKey).FirstOrDefault();

            if (fkColumn == null)
            {
                return string.Empty;
            }
            string filter = string.Empty;
            object foreignKeyValue = fkColumn.ForeignKeyValue;

            if ((foreignKeyValue?.ToString() ?? nameof(System.DBNull)) == nameof(System.DBNull))
            {
                filter = $"{fkColumn.ColumnExpression} is null";
            }
            else
            {
                /*
                if (ParentControlType == ComponentType.DbNetEdit)
                {
                    var fk = GetForeignKeyValue(foreignKeyValue);
                    var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(fk);
                    foreignKeyValue = dictionary.Values.FirstOrDefault();
                }
                */
                if (foreignKeyValue is List<object>)
                {
                    List<string> paramNames = new List<string>();
                    List<object> foreignKeyValues = foreignKeyValue as List<object>;
                    for (var i = 0; i < foreignKeyValues.Count; i++)
                    {
                        string paramName = Database.ParameterName($"{fkColumn.ColumnName}{i}");
                        paramNames.Add(paramName);
                        if (parameters != null)
                        {
                            parameters[paramName] = foreignKeyValues[i];
                        }
                    }

                    filter = $"{fkColumn.ColumnExpression} in ({string.Join(",", paramNames)})";
                }
                else
                {
                    string paramName = ParamName(fkColumn, false);
                    filter = $"{fkColumn.ColumnExpression} = {paramName}";
                    if (parameters != null)
                    {
                        parameters[paramName] = ConvertToDbParam(foreignKeyValue, null);
                    }
                }
            }

            return filter;
        }
        protected byte[] GetColumnData()
        {
            byte[] byteData = new byte[0];

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query = new QueryCommandConfig();

                query.Sql = $"select {this.ColumnName} from {FromPart} where {PrimaryKeyFilter(query.Params)}";
                Database.ExecuteQuery(query);

                if (Database.Reader.Read())
                {
                    object value = Database.Reader[0];
                    if (value is Byte[])
                    {
                        byteData = (byte[])value;
                    }
                }
                Database.Close();
            }

            if (string.IsNullOrEmpty(Extension) == false)
            {
                HttpContext.Response.ContentType = GetMimeTypeForFileExtension($".{Extension}");
            }
            return byteData;
        }


        protected object GetForeignKeyValue(object foreignKeyValue)
        {
            var fk = EncodingHelper.Decode(foreignKeyValue.ToString());
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(fk);
            return dictionary.Values.FirstOrDefault();
        }

        protected string PrimaryKeyFilter(ListDictionary parameters, Dictionary<string, object> primaryKeyValues  = null)
        {
            if (primaryKeyValues == null)
            {
                primaryKeyValues = PrimaryKeyValues;
            }
            List<string> primaryKeyFilterPart = new List<string>();
            foreach (string key in primaryKeyValues.Keys)
            {
                DbColumn dbColumn = DbColumns.FirstOrDefault(c => c.IsMatch(key));
                primaryKeyFilterPart.Add($"{key} = {Database.ParameterName(key)}");
                parameters.Add(Database.ParameterName(key), ConvertToDbParam(primaryKeyValues[key], dbColumn));
            }
            return $"{string.Join($" and ", primaryKeyFilterPart)}";
        }
        protected string RefineSearchExpression(DbColumn col)
        {
            string columnExpression = StripColumnRename(col.ColumnExpression);

            if (col.DataType != typeof(DateTime).Name)
            {
                return columnExpression;
            }

            switch (Database.Database)
            {
                case DatabaseType.SqlServer:
                    if (col.DbDataType != "31") // "Date"
                        columnExpression = $"CONVERT(DATE,{columnExpression})";
                    break;
                case DatabaseType.Oracle:
                    columnExpression = $"trunc({columnExpression})";
                    break;
                case DatabaseType.PostgreSql:
                    columnExpression = $"date_trunc('day',{columnExpression})";
                    break;
            }

            return columnExpression;
        }

        protected string FilterExpression(SearchParameter searchParameter, DbColumn dbColumn)
        {
            string template = searchParameter.SearchOperator.GetAttribute<FilterExpressionAttribute>().Expression;

            switch (searchParameter.SearchOperator)
            {
                case SearchOperator.In:
                case SearchOperator.NotIn:
                    List<string> parameters = new List<string>();
                    string[] values = searchParameter.Value1.Split(",");
                    for (var i = 0; i < values.Length; i++)
                    {
                        parameters.Add(ParamName(dbColumn, $"{ParamNames.SearchFilter1}{i}"));
                    }
                    return template.Replace("{0}", string.Join(",", parameters));
                default:
                    string param1 = ParamName(dbColumn, ParamNames.SearchFilter1);
                    string param2 = ParamName(dbColumn, ParamNames.SearchFilter2);
                    return template.Replace("{0}", param1).Replace("{1}", param2);
            }
        }

        protected string StripColumnRename(string columnExpression)
        {
            string[] columnParts = columnExpression.Split(')');
            columnParts[columnParts.Length - 1] = Regex.Replace(columnParts[columnParts.Length - 1], " as .*", "", RegexOptions.IgnoreCase);
            columnParts[0] = Regex.Replace(columnParts[0], "unique |distinct ", "", RegexOptions.IgnoreCase);

            return String.Join(")", columnParts);
        }
        private int ParseBoolean(string boolString)
        {
            switch (boolString.ToLower())
            {
                case "true":
                case "1":
                    return 1;
                default:
                    return 0;
            }
        }

        protected bool IsReadOnly(DbNetSuiteResponse response)
        {
            if (Settings.ReadOnly)
            {
                response.Error = true;
                response.Message = "Database modification has been disabled";
            }

            return Settings.ReadOnly;
        }
    }
}

