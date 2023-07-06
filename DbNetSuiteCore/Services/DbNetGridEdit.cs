﻿using DbNetSuiteCore.Enums;
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

namespace DbNetSuiteCore.Services
{
    public class DbNetGridEdit : DbNetSuite
    {
        private string _fromPart;
        protected Dictionary<string, DataTable> _lookupTables = new Dictionary<string, DataTable>();

        public string FromPart
        {
            get => EncodingHelper.Decode(_fromPart);
            set => _fromPart = value;
        }
        public bool Navigation { get; set; }
        public bool QuickSearch { get; set; }
        public string QuickSearchToken { get; set; } = string.Empty;
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
                        editColumn.Display = false;
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

                foreach (Group g in match.Groups)
                    if (!String.IsNullOrEmpty(g.Value))
                        columnList = columnList.Replace(g.Value, g.Value.Replace(",", "~"));
            }

            string[] columnExpressions = columnList.Split(',');

            for (int I = 0; I < columnExpressions.Length; I++)
                columnExpressions[I] = columnExpressions[I].Replace("~", ",");

            return columnExpressions;
        }

        protected bool ValidateRequest<T>(DbNetGridEditResponse response, List<T> columns)
        {
            response.Message = String.Empty;

            if (SearchParams.Any())
            {
                object convertedValue = new object();

                foreach (SearchParameter searchParameter in SearchParams)
                {
                    DbColumn gridColumn = columns[searchParameter.ColumnIndex] as DbColumn;

                    string expression = searchParameter.SearchOperator.GetAttribute<FilterExpressionAttribute>()?.Expression ?? "{0}";

                    switch (searchParameter.SearchOperator)
                    {
                        case SearchOperator.In:
                        case SearchOperator.NotIn:
                            foreach (string value in searchParameter.Value1.Split(','))
                            {
                                searchParameter.Value1Valid = ConvertSearchValue(gridColumn.DataType, value, ref convertedValue);
                                if (searchParameter.Value1Valid == false)
                                {
                                    break;
                                }
                            }
                            break;
                        default:
                            if (expression.Contains("{0}"))
                            {
                                searchParameter.Value1Valid = ConvertSearchValue(gridColumn.DataType, searchParameter.Value1, ref convertedValue);
                            }
                            if (expression.Contains("{1}"))
                            {
                                searchParameter.Value2Valid = ConvertSearchValue(gridColumn.DataType, searchParameter.Value2, ref convertedValue);
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

        private bool ConvertSearchValue(string dataType, string value, ref object convertedValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            try
            {
                switch (dataType)
                {
                    case nameof(Double):
                        convertedValue = Convert.ToDouble(value);
                        break;
                    case nameof(Decimal):
                        convertedValue = Convert.ToDecimal(value);
                        break;
                    case nameof(Int16):
                    case nameof(Int32):
                    case nameof(Int64):
                        convertedValue = Convert.ToInt64(value);
                        break;
                    case nameof(DateTime):
                        convertedValue = Convert.ToDateTime(value);
                        break;
                    case nameof(Boolean):
                        convertedValue = Convert.ToBoolean(value);
                        break;
                    default:
                        convertedValue = value.ToString();
                        break;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        protected DataTable InitialiseDataTable<T>(List<T> columns)
        {
            DataTable dataTable = new DataTable();
            foreach (object o in columns)
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
        protected Dictionary<string, object> CreateRecord(DataTable dataTable)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.DataType == typeof(Byte[]))
                {
                    continue;
                }
                var columnValue = dataTable.Rows[0][column.ColumnName];
                dictionary[column.ColumnName.ToLower()] = (columnValue == System.DBNull.Value ? string.Empty : columnValue);
            }

            return dictionary;
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
        protected object ConvertToDbParam(object value, DbColumn column = null)
        {
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
                        paramValue = TimeSpan.Parse(DateTime.Parse(value.ToString()).ToString("t"));
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
                        paramValue = Convert.ChangeType(value, GetColumnType(dataType));
                        break;
                    default:
                        paramValue = Convert.ChangeType(value, GetColumnType(dataType));
                        break;
                }
            }
            catch (Exception e)
            {
                ThrowException(e.Message, "ConvertToDbParam: Value: " + value.ToString() + " DataType:" + dataType);
                return DBNull.Value;
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
    }
}

