using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DbNetSuiteCore.Utilities;
using DbNetSuiteCore.Attributes;
using DbNetSuiteCore.Constants;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Extensions;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.ViewModels.DbNetGrid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GridColumn = DbNetSuiteCore.Models.GridColumn;
using static DbNetSuiteCore.Utilities.DbNetDataCore;



namespace DbNetSuiteCore.Services
{
    internal class DbNetGrid : DbNetSuite
    {
        private Dictionary<string, object> _columnProperties = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, DataTable> _lookupTables = new Dictionary<string, DataTable>();
        private Dictionary<string, object> _resp = new Dictionary<string, object>();
        const string NullValueToken = "@@null@@";

        private GridColumnCollection Columns { get; set; } = new GridColumnCollection(new List<GridColumn>());
        private DbNetDataCore Database { get; set; }
        private DbNetGridRequest _dbNetGridRequest;
        private string _fromPart;
        private string _connectionString;
        private string _fixedFilterSql;
        private string _procedureName;
        public DbNetGrid(AspNetCoreServices services) : base(services)
        {
        }

        public BooleanDisplayMode BooleanDisplayMode { get; set; } = BooleanDisplayMode.TrueFalse;
        public Dictionary<string, object> ColumnFilterParams { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, string> ColumnFilters { get; set; } = new Dictionary<string, string>();
        public List<string> ColumnFilterSql { get; set; } = new List<string> { };
        public string ColumnName { get; set; } = String.Empty;
        public string ComponentId { get; set; } = String.Empty;
        public string ConnectionString
        {
            get => EncodingHelper.Decode(_connectionString);
            set => _connectionString = value;
        }
        public string Culture { get; set; } = String.Empty;
        public bool Copy { get; set; } = true;
        public int CurrentPage { get; set; } = 1;
        public GridColumn DefaultColumn { get; set; }
        public bool DeleteRow { get; set; } = false;
        public bool Export { get; set; } = true;
        public string Extension { get; set; } = string.Empty;
        public FilterColumnModeValues FilterColumnMode { get; set; } = FilterColumnModeValues.Simple;
        public string FilterSql { get; set; } = string.Empty;
        public Dictionary<string, object> FixedFilterParams { get; set; } = new Dictionary<string, object>();
        public string FixedFilterSql
        {
            get => EncodingHelper.Decode(_fixedFilterSql);
            set => _fixedFilterSql = value;
        }
        public string FixedOrderBy { get; set; } = string.Empty;
        public string FromPart
        {
            get => EncodingHelper.Decode(_fromPart);
            set => _fromPart = value;
        }
        public bool FrozenHeader { get; set; } = false;
        public bool GroupBy { get; set; } = false;
        public string Having { get; set; } = string.Empty;
        public string Id => ComponentId;
        public bool IgnorePrimaryKeys { get; set; } = false;
        public bool InsertRow { get; set; } = false;
        public bool MultiRowSelect { get; set; } = false;
        public MultiRowSelectLocation MultiRowSelectLocation { get; set; } = MultiRowSelectLocation.Left;
        public bool Navigation { get; set; } = true;
        public bool NestedGrid { get; set; } = false;
        public bool OptimizeForLargeDataset { get; set; } = false;
        public int? OrderBy { get; set; } = null;
        public OrderByDirection OrderByDirection { get; set; } = OrderByDirection.asc;
        public long PageSize { get; set; } = 20;
        public Dictionary<string, object> ParentFilterParams { get; set; } = new Dictionary<string, object>();
        public List<string> ParentFilterSql { get; set; } = new List<string> { };
        public string PrimaryKey { get; set; } = String.Empty;
        public string ProcedureName
        {
            get => EncodingHelper.Decode(_procedureName);
            set => _procedureName = value;
        }
        public Dictionary<string, object> ProcedureParams { get; set; }
        public bool QuickSearch { get; set; } = false;
        public string QuickSearchToken { get; set; } = string.Empty;
        public bool Search { get; set; } = true;
        public string SearchFilterJoin { get; set; } = "and";
        public List<SearchParameter> SearchParams { get; set; } = new List<SearchParameter>();
        public string SelectModifier { get; set; } = string.Empty;
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; } = ToolbarButtonStyle.Image;
        public ToolbarPosition ToolbarPosition { get; set; } = ToolbarPosition.Top;
        public int TotalPages { get; set; } = 0;
        public long TotalRows { get; set; } = 0;
        public bool UpdateRow { get; set; } = false;
        public bool View { get; set; } = false;
        public ResourceManager ResourceManager { get; set; }

        static public string GenerateLabel(string label)
        {
            label = Regex.Replace(label, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
            return Capitalise(label.Replace("_", " ").Replace(".", " "));
        }

        public static Type GetNullableType(Type type)
        {
            if (Nullable.GetUnderlyingType(type) == null)
                return type;
            else
                return Nullable.GetUnderlyingType(type);
        }

        public async Task<object> Process()
        {
            await DeserialiseRequest();
            Database = new DbNetDataCore(ConnectionString, Env, Configuration);
            DbNetGridResponse response = new DbNetGridResponse();

            ResourceManager = new ResourceManager("DbNetSuiteCore.Resources.Localization.default", typeof(DbNetGrid).Assembly);

            if (string.IsNullOrEmpty(this.Culture) == false)
            {
                CultureInfo ci = new CultureInfo(this.Culture);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }

            if (string.IsNullOrEmpty(this.ProcedureName) == false)
            {
                View = false;
                Search = false;
                QuickSearch = false;
            }

            switch (Action.ToLower())
            {
                case "initialize":
                    response.Toolbar = await Toolbar();
                    await Grid(response);
                    break;
                case "page":
                    await Grid(response);
                    break;
                case "generate-spreadsheet":
                    return GenerateSpreadsheet();
                case "html-export":
                    return await GenerateHtmlExport(response);
                case "download-column-data":
                    return GetColumnData();
                case "view-content":
                    await ViewDialog(response);
                    break;
                case "search-dialog":
                    await SearchDialog(response);
                    break;
                case "lookup":
                    await LookupDialog(response);
                    break;
                case "data-array":
                    return DataArray();
            }

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return System.Text.Json.JsonSerializer.Serialize(response, serializeOptions);
        }

        internal static string Capitalise(string text)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        internal static Dictionary<string, object> CaseInsensitiveDictionary(Dictionary<string, object> dictionary)
        {
            Dictionary<string, object> caseInsensitiveDictionary = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

            foreach (string k in dictionary.Keys)
                caseInsensitiveDictionary.Add(k, dictionary[k]);

            return caseInsensitiveDictionary;
        }

        internal string AggregateExpression(DbColumn c)
        {
            return Regex.Replace(c.ColumnExpression, @" as \w*$", "", RegexOptions.IgnoreCase);
        }

        internal GridColumn ColumnFromKey(string key)
        {
            foreach (GridColumn col in this.Columns)
                if (col.ColumnKey == key)
                    return col;

            foreach (GridColumn col in this.Columns)
                if (col.ColumnName.Split('.')[col.ColumnName.Split('.').Length - 1].ToLower() == key.ToLower())
                    return col;

            return null;
        }

        internal void ConfigureStoredProcedure()
        {
            this.InsertRow = false;
            this.UpdateRow = false;
            this.DeleteRow = false;
            this.View = false;

            QueryCommandConfig queryCommand = BuildSql();

            using (Database)
            {
                Database.Open();
                Database.ExecuteQuery(queryCommand);
                DataTable schema = Database.Reader.GetSchemaTable();

                var columns = Columns.ToList();

                Columns.Clear();

                var columnIndex = 0;

                foreach (DataRow row in schema.Rows)
                {
                    if (row.IsHidden())
                    {
                        continue;
                    }

                    GridColumn column = columns.Where(c => MatchingColumn(c, row.ColumnName(), row.BaseTableName())).FirstOrDefault();
                    if (column == null)
                    {
                        column = GenerateColumn(row);
                    }
                    column.Index = columnIndex++;

                    ConfigureColumn(column, row);

                    Columns.Add(column);
                }
            }
        }

        private void ConfigureColumn(GridColumn column, DataRow row)
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

        internal string DefaultOrderBy()
        {
            return DefaultOrderBy(false);
        }

        internal string DefaultOrderBy(bool useColumnName)
        {
            return $"{Columns.First().ColumnExpression} asc";
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

        internal string[] GetSelectColumns(string sql)
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

        internal int ParseBoolean(string boolString)
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

        internal QueryCommandConfig ProcedureCommandConfig()
        {
            QueryCommandConfig queryCommandConfig = new QueryCommandConfig(this.ProcedureName);

            using (Database)
            {
                Database.Open();
                try
                {
                    queryCommandConfig.Params = Database.DeriveParameters(queryCommandConfig.Sql);
                }
                catch (Exception) { }
            }

            foreach (string paramName in this.ProcedureParams.Keys)
            {
                Database.SetParamValue(queryCommandConfig.Params, paramName, ProcedureParams[paramName]);
            }

            return queryCommandConfig;
        }

        protected DataTable ArrayToDataTable(string jsonArray)
        {
            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.Converters.Add(new JsonStringEnumConverter());
            return ArrayToDataTable(System.Text.Json.JsonSerializer.Deserialize<object[]>(jsonArray, options));
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

        protected virtual string BuildFilterPart()
        {
            List<string> fp = new List<string>();

            GridColumn fkColumn = Columns.Where(c => c.ForeignKey && c.ForeignKeyValue != null).FirstOrDefault();

            if (fkColumn != null)
            {
                fp.Add($"{fkColumn.ColumnExpression} = {ParamName(fkColumn, false)}");
            }

            if (string.IsNullOrEmpty(FixedFilterSql) == false)
            {
                fp.Add(FixedFilterSql);
            }

            if (String.IsNullOrEmpty(QuickSearchToken) == false)
            {
                List<string> quickSearchFilter = new List<string>();
                foreach (GridColumn col in Columns.Where(c => c.QuickSearch))
                {
                    if (string.IsNullOrEmpty(col.Lookup))
                    {
                        quickSearchFilter.Add($"{col.ColumnExpression} like {ParamName(ParamNames.QuickSearchToken)}");
                    }
                    else
                    {
                        quickSearchFilter.Add($"{col.ColumnExpression} in ({SearchLookupSql(col, ParamNames.QuickSearchToken)})");
                    }
                }
                fp.Add($"({string.Join(" or ", quickSearchFilter.ToArray())})");
            }

            if (ColumnFilters.Keys.Any())
            {
                List<string> columnFilterPart = new List<string>();
                foreach (string columnName in ColumnFilters.Keys)
                {
                    GridColumn col = Columns[columnName];
                    if (string.IsNullOrEmpty(col.Lookup) || col.FilterMode == FilterColumnSelectMode.List)
                    {
                        var columnFilter = ParseFilterColumnValue(ColumnFilters[columnName], col);
                        if (columnFilter != null)
                        {
                            columnFilterPart.Add($"{RefineSearchExpression(col)} {columnFilter.Value.Key} {ParamName(col, ParamNames.ColumnFilter)}");
                        }
                    }
                    else
                    {
                        columnFilterPart.Add($"{col.ColumnName} in ({SearchLookupSql(col, ParamName(col, ParamNames.ColumnFilter))})");
                    }
                }
                fp.Add($"({string.Join(" and ", columnFilterPart.ToArray())})");
            }

            if (SearchParams.Any())
            {
                List<string> searchFilterPart = new List<string>();
                foreach (SearchParameter searchParameter in SearchParams)
                {
                    GridColumn gridColumn = Columns[searchParameter.ColumnIndex];

                    searchFilterPart.Add($"{RefineSearchExpression(gridColumn)} {FilterExpression(searchParameter, gridColumn)}");
                }
                fp.Add($"({string.Join($" {SearchFilterJoin} ", searchFilterPart.ToArray())})");
            }

            string filterPart = string.Join(" and ", fp.ToArray());

            return filterPart;
        }

        private string FilterExpression(SearchParameter searchParameter, GridColumn gridColumn)
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
                        parameters.Add(ParamName(gridColumn, $"{ParamNames.SearchFilter1}{i}"));
                    }
                    return template.Replace("{0}", string.Join(",", parameters));
                default:
                    string param1 = ParamName(gridColumn, ParamNames.SearchFilter1);
                    string param2 = ParamName(gridColumn, ParamNames.SearchFilter2);
                    return template.Replace("{0}", param1).Replace("{1}", param2);
            }
        }
        protected string BuildOrderPart(QueryBuildModes buildMode)
        {
            switch (buildMode)
            {
                case QueryBuildModes.Count:
                case QueryBuildModes.Totals:
                    return String.Empty;
            }

            List<string> orderPart = new List<string>();

            if (Columns.Any(c => c.FixedOrder))
            {
                foreach (GridColumn column in Columns.Where(c => c.FixedOrder))
                {
                    orderPart.Add((column.Index + 1).ToString());
                }
            }

            if (OrderBy.HasValue)
            {
                if (orderPart.Contains(OrderBy.Value.ToString()) == false)
                {
                    orderPart.Add($"{OrderBy.Value} {OrderByDirection}");
                }
            }

            if (orderPart.Any() == false)
            {
                orderPart.Add(DefaultOrderBy());
            }

            return string.Join(",", orderPart.ToArray());
        }

        protected string BuildGroupByPart(QueryBuildModes buildMode)
        {
            List<string> groupByParts = new List<string>();

            switch (buildMode)
            {
                case QueryBuildModes.Totals:
                    int ordinal = 1;
                    foreach (GridColumn column in Columns)
                    {
                        if (column.Aggregate == AggregateType.None && column.TotalBreak)
                        {
                            groupByParts.Add(column.ColumnExpression);
                            ordinal++;
                        }
                    }
                    break;
                case QueryBuildModes.Normal:
                case QueryBuildModes.Spreadsheet:
                    if (GroupBy)
                    {
                        foreach (GridColumn column in Columns)
                        {
                            if (column.Aggregate == AggregateType.None)
                            {
                                groupByParts.Add(column.ColumnExpression);
                            }
                        }
                    }

                    break;
            }

            return string.Join(",", groupByParts.ToArray());
        }

        protected virtual ListDictionary BuildParams()
        {
            return BuildParams(QueryBuildModes.Normal);
        }

        protected virtual ListDictionary BuildParams(QueryBuildModes buildMode)
        {
            ListDictionary parameters = new ListDictionary();

            GridColumn fkColumn = Columns.Where(c => c.ForeignKey && c.ForeignKeyValue != null).FirstOrDefault();

            if (fkColumn != null)
            {
                parameters.Add(ParamName(fkColumn, true), ConvertToDbParam(fkColumn.ForeignKeyValue, fkColumn));
            }

            if (FixedFilterParams.Keys.Any())
            {
                foreach (string paramName in FixedFilterParams.Keys)
                {
                    parameters.Add(ParamName(paramName), ConvertToDbParam(FixedFilterParams[paramName]));
                }
            }

            if (String.IsNullOrEmpty(QuickSearchToken) == false)
            {
                parameters.Add(ParamName(ParamNames.QuickSearchToken, true), ConvertToDbParam($"%{QuickSearchToken}%"));
            }

            if (ColumnFilters.Keys.Any())
            {
                foreach (string columnName in ColumnFilters.Keys)
                {
                    GridColumn gridColumn = Columns[columnName];
                    var columnFilter = ParseFilterColumnValue(ColumnFilters[columnName], gridColumn);
                    if (columnFilter != null)
                    {
                        parameters.Add(ParamName(gridColumn, ParamNames.ColumnFilter, true), ConvertToDbParam(columnFilter.Value.Value));
                    }
                }
            }

            if (SearchParams.Any())
            {
                foreach (SearchParameter searchParameter in SearchParams)
                {
                    GridColumn gridColumn = Columns[searchParameter.ColumnIndex];

                    string expression = searchParameter.SearchOperator.GetAttribute<FilterExpressionAttribute>()?.Expression ?? "{0}";

                    if (expression.Contains("{0}"))
                    {
                        AddSearchFilterParams(parameters, gridColumn, searchParameter, ParamNames.SearchFilter1, searchParameter.Value1);
                    }
                    if (expression.Contains("{1}"))
                    {
                        AddSearchFilterParams(parameters, gridColumn, searchParameter, ParamNames.SearchFilter2, searchParameter.Value2);
                    }
                }
            }

            return parameters;
        }

        private void AddSearchFilterParams(ListDictionary parameters, GridColumn gridColumn, SearchParameter searchParameter, string prefix, string value)
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
                        parameters.Add(ParamName(gridColumn, $"{prefix}{i}", true), ConvertToDbParam(template.Replace("{0}", values[i]), gridColumn));
                    }
                    break;
                default:
                    parameters.Add(ParamName(gridColumn, prefix, true), ConvertToDbParam(template.Replace("{0}", value), gridColumn));
                    break;
            }
        }

        private string ParamName(GridColumn column, string suffix = "", bool parameterValue = false)
        {
            return Database.ParameterName($"{column.ColumnName}{suffix}");
        }

        private string ParamName(GridColumn column, bool parameterValue = false)
        {
            return ParamName(column, string.Empty, parameterValue);
        }

        private string ParamName(string paramName, bool parameterValue = false)
        {
            return Database.ParameterName(paramName, parameterValue);
        }

        protected virtual string BuildSelectPart(QueryBuildModes buildMode)
        {
            return BuildSelectPart(buildMode, false);
        }

        protected virtual string BuildSelectPart(QueryBuildModes buildMode = QueryBuildModes.Normal, bool unqualifiedColumnNames = false)
        {
            if (buildMode == QueryBuildModes.Count)
                return "count(*)";

            List<string> selectParts = new List<string>();

            foreach (GridColumn column in Columns)
            {
                column.ColumnExpression = EncodingHelper.Encode(Database.UpdateConcatenationOperator(column.ColumnExpression));

                if (buildMode == QueryBuildModes.PrimaryKeysOnly)
                    if (!column.PrimaryKey)
                        continue;

                if (column.Unmatched || column.ColumnExpression == "*")
                {
                    continue;
                }

                string columnExpression = column.ColumnExpression;

                if (unqualifiedColumnNames)
                    columnExpression = columnExpression.Split('.')[columnExpression.Split('.').Length - 1];

                switch (buildMode)
                {
                    case QueryBuildModes.Configuration:
                    case QueryBuildModes.View:
                        break;
                    case QueryBuildModes.Totals:
                        if (column.Aggregate == AggregateType.None)
                        {
                            if (column.TotalBreak == false)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            columnExpression = $"{column.Aggregate}({AggregateExpression(column)}) as {column.ColumnName}";
                        }
                        break;
                    case QueryBuildModes.Normal:
                    case QueryBuildModes.Spreadsheet:
                        if (column.Binary && buildMode == QueryBuildModes.Spreadsheet)
                        {
                            continue;
                        }
                        if (GroupBy)
                        {
                            if (column.Aggregate != AggregateType.None)
                            {
                                columnExpression =
                                    $"{column.Aggregate}({AggregateExpression(column)}) as {column.ColumnName}";
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

            string columnList = string.Join(", ", selectParts.ToArray());

            return columnList;
        }

        protected QueryCommandConfig BuildSql()
        {
            return BuildSql(QueryBuildModes.Normal);
        }

        protected QueryCommandConfig BuildSql(QueryBuildModes buildMode)
        {
            if (ProcedureName != "")
                return ProcedureCommandConfig();

            return BuildSql(BuildSelectPart(buildMode), BuildFilterPart(), BuildOrderPart(buildMode), BuildGroupByPart(buildMode), buildMode);
        }

        protected virtual QueryCommandConfig BuildSql(string selectPart, string filterPart, string orderPart, string groupByPart, QueryBuildModes buildMode)
        {
            if (string.IsNullOrEmpty(selectPart))
            {
                return null;
            }

            string sql = $"select {selectPart} from {FromPart}";
            if (!String.IsNullOrEmpty(filterPart))
                sql += $" where {filterPart}";

            if (groupByPart != string.Empty)
                sql += $" group by {groupByPart}";

            if (buildMode != QueryBuildModes.Totals)
                if (orderPart != string.Empty)
                    sql += $" order by {orderPart}";

            ListDictionary p = BuildParams(buildMode);
            sql = ReplaceWithCorrectParams(sql, p);

            QueryCommandConfig query = new QueryCommandConfig(sql);
            query.Params = p;
            return query;
        }

        protected bool ColumnIncluded(DataRow r)
        {
            string columnName = r.ColumnName().ToLower();
            foreach (DbColumn c in Columns)
            {
                string[] columnParts = c.ColumnExpression.Split('.');
                string ce = columnParts[columnParts.Length - 1].ToLower();
                if (Database.UnqualifiedDbObjectName(ce) == columnName)
                    return true;
            }
            return false;
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

        protected Type GetColumnType(string typeName)
        {
            return Type.GetType("System." + typeName);
        }

        protected void GetLookupTables()
        {
            _lookupTables.Clear();

            using (Database)
            {
                foreach (DbColumn column in Columns)
                {
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

        protected string ReplaceWithCorrectParams(string sql, ListDictionary p)
        {
            Regex paramPattern = new Regex(@"Col\d{1,3}Param\d{1,3}");

            foreach (string key in p.Keys)
            {
                if (paramPattern.IsMatch(key))
                    sql = Regex.Replace(sql, @"([\(, ])" + key + @"\b", "$1" + Database.ParameterName(key));
            }

            return sql;
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

            string filter = $"({StripColumnRename(col.LookupTextExpression)} like {paramName})";

            if (Regex.IsMatch(sql, " where ", RegexOptions.IgnoreCase))
                sql = Regex.Replace(sql, " where ", " where " + filter + " and ", RegexOptions.IgnoreCase);
            else
                sql += " where " + filter;

            return sql;
        }

        protected string StripColumnRename(string columnExpression)
        {
            string[] columnParts = columnExpression.Split(')');
            columnParts[columnParts.Length - 1] = Regex.Replace(columnParts[columnParts.Length - 1], " as .*", "", RegexOptions.IgnoreCase);
            columnParts[0] = Regex.Replace(columnParts[0], "unique |distinct ", "", RegexOptions.IgnoreCase);

            return String.Join(")", columnParts);
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

        private void ConfigureColumns()
        {
            if (!String.IsNullOrEmpty(ProcedureName))
            {
                ConfigureStoredProcedure();
                return;
            }

            using (Database)
            {
                Database.Open();

                string selectPart = Columns.Any(c => c.Unmatched == false) == false ? "*" : BuildSelectPart(QueryBuildModes.Configuration);
                string sql = $"select {selectPart} from {FromPart} where 1=2";

                DataTable dataTable = Database.GetSchemaTable(sql);

                int columnIndex = 0;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row.IsHidden())
                    {
                        continue;
                    }

                    GridColumn column = Columns.Where(c => MatchingColumn(c, row.ColumnName(), row.BaseTableName())).FirstOrDefault();

                    if (column == null)
                    {
                        column = GenerateColumn(row);
                        Columns.Add(column);
                    }

                    column.Index = columnIndex++;
                    ConfigureColumn(column, row);

                    if (column.GroupHeader)
                    {
                        column.Display = false;
                    }
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

                    if (column.TotalBreak)
                    {
                        if (column.ClearDuplicateValue.HasValue == false)
                        {
                            column.ClearDuplicateValue = true;
                        }
                    }

                    if (column.ClearDuplicateValue.HasValue == false || GroupBy)
                    {
                        column.ClearDuplicateValue = false;
                    }

                    if (DefaultColumn != null)
                    {
                        column.Filter = DefaultColumn.Filter;
                        column.View = DefaultColumn.View;
                        column.Style = DefaultColumn.Style;
                    }

                    if (column.DataOnly)
                    {
                        column.Display = false;
                    }

                    if (column.Filter && column.FilterMode == FilterColumnSelectMode.List)
                    {
                        if (string.IsNullOrEmpty(column.Lookup))
                        {
                            column.Lookup = EncodingHelper.Encode($"select distinct {column.ColumnExpression} from {FromPart}");
                        }
                    }
                }
            }

            Columns = new GridColumnCollection(Columns.OrderBy(c => c.Index).ToList());

            using (Database)
            {
                Database.Open();
                foreach (GridColumn col in Columns.Where(c => string.IsNullOrEmpty(c.Lookup) == false))
                {
                    string sql = Database.UpdateConcatenationOperator(col.Lookup);

                    ListDictionary @params = Database.ParseParameters(sql);

                    if (@params.Count > 0)
                        sql = Regex.Replace(sql, " where .*", " where 1=2", RegexOptions.IgnoreCase);

                    DataTable dataTable;
                    try
                    {
                        dataTable = Database.GetSchemaTable(sql);
                    }
                    catch (Exception)
                    {
                        throw new Exception($"Error in column lookup sql {sql}");
                    }

                    int textRowIndex = (dataTable.Rows.Count == 1) ? 0 : 1;

                    try
                    {
                        DataRow textColumnRow = dataTable.Rows[textRowIndex];
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

                        col.LookupValueField = dataTable.Rows[0].ColumnName();
                    }
                    catch (Exception ex)
                    {
                        ThrowException("Error [<b>" + ex.Message + "</b>] in column <b>Lookup</b> property<br /><br />The SQL is not in the expected format.  Expected format is  \"SELECT IdField, TextDescriptionField FROM Table\"");
                        return;
                    }
                }
            }

            foreach (var gridColumn in Columns.Where(c => c.Unmatched && c.PrimaryKey))
            {
                gridColumn.Unmatched = false;
                gridColumn.Display = false;
            }

            while (Columns.Any(c => c.Unmatched))
            {
                Columns.Remove(Columns.First(c => c.Unmatched));
            }

            if (OrderBy.HasValue == false)
            {
                OrderBy = Columns.FirstOrDefault(c => c.Display).Index + 1;
            }

            if (Columns.Any(c => c.View) == false)
            {
                Columns.ToList().ForEach(c => c.View = true);
            }

            Columns[OrderBy.Value - 1].OrderBy = OrderByDirection;

            GetLookupTables();
        }

        private bool MatchingColumn(GridColumn c, string columnName, string tableName)
        {
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
        private async Task DeserialiseRequest()
        {
            _dbNetGridRequest = await GetRequest<DbNetGridRequest>();
            ReflectionHelper.CopyProperties(_dbNetGridRequest, this);
            Columns = _dbNetGridRequest.Columns;
        }

        private GridColumn GenerateColumn(DataRow row)
        {
            GridColumn c = new GridColumn();

            c.ColumnExpression = EncodingHelper.Encode(Database.QualifiedDbObjectName(row.ColumnName(), false));
            c.ColumnName = row.ColumnName();
            c.BaseTableName = row.BaseTableName();

            GetBaseSchemaName(c, row);

            c.AddedByUser = false;

            return c;
        }

        private async Task<byte[]> GenerateHtmlExport(DbNetGridResponse response)
        {
            PageSize = -1;
            await Grid(response);
            var viewModel = new HtmlExportViewModel
            {
                GridHtml = response.Data.ToString(),
                GridCss = await GetResourceString($"CSS.dbnetsuite.css")
            };
            response.Data = await HttpContext.RenderToStringAsync("Views/DbNetGrid/HtmlExport.cshtml", viewModel);
            HttpContext.Response.ContentType = GetMimeTypeForFileExtension($".html"); ;
            return Encoding.UTF8.GetBytes(response.Data.ToString());
        }

        /*
        private string GoogleChartDatatable()
        {
            ConfigureColumns();

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query = BuildSql(QueryBuildModes.Spreadsheet);
                Database.ExecuteQuery(query);

                Components.DataTable dataTable = new Models.GoogleCharts.DataTable();

                foreach (GridColumn column in Columns)
                {
                    if (column.Binary)
                    {
                        continue;
                    }

                    var col = new Models.GoogleCharts.Col
                    {
                        Id = $"c{column.Index}",
                        Label = column.Label,
                        Type = JavaScriptTypeName(column)
                    };

                    dataTable.Cols.Add(col);
                }

                while (Database.Reader.Read())
                {
                    object[] values = new object[Database.Reader.FieldCount];
                    Database.Reader.GetValues(values);
                    var row = new Models.GoogleCharts.Row();
                    foreach (GridColumn column in Columns)
                    {
                        if (column.Binary)
                        {
                            continue;
                        }

                        var cell = new Models.GoogleCharts.Cell();

                        object value = values[column.Index];

                        if (value == DBNull.Value)
                        {
                            cell.V = null;
                        }
                        else if (string.IsNullOrEmpty(column.Lookup) == false)
                        {
                            cell.V = GetLookupValue(column, value);
                        }
                        else
                        {
                            cell.V = value;
                        }

                        row.C.Add(cell);
                    }

                    dataTable.Rows.Add(row);
                }

                return Serialize(dataTable);
            }
        }
        */

        private string DataArray()
        {
            ConfigureColumns();

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query = BuildSql(QueryBuildModes.Spreadsheet);
                Database.ExecuteQuery(query);

                List<object> dataTable = new List<object>();
                List<string> headings = new List<string>();

                foreach (GridColumn column in Columns)
                {
                    if (column.Binary)
                    {
                        continue;
                    }
                    headings.Add(column.Label);
                }
                dataTable.Add(headings);

                while (Database.Reader.Read())
                {
                    object[] values = new object[Database.Reader.FieldCount];
                    Database.Reader.GetValues(values);
                    var row = new List<object>();
                    foreach (GridColumn column in Columns)
                    {
                        if (column.Binary)
                        {
                            continue;
                        }

                        object value = values[column.Index];

                        if (value == DBNull.Value)
                        {
                            value = null;
                        }
                        else if (string.IsNullOrEmpty(column.Lookup) == false)
                        {
                            value = GetLookupValue(column, value);
                        }

                        row.Add(value);
                    }

                    dataTable.Add(row);
                }

                return Serialize(dataTable);
            }
        }

        private string Serialize(object data)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return System.Text.Json.JsonSerializer.Serialize(data, serializeOptions);
        }

        private byte[] GenerateSpreadsheet()
        {
            ConfigureColumns();

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query = BuildSql(QueryBuildModes.Spreadsheet);
                Database.ExecuteQuery(query);

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    Dictionary<string, int> columnWidths = new Dictionary<string, int>();

                    var worksheet = workbook.Worksheets.Add(Id);

                    var rowIdx = 1;
                    var colIdx = 1;
                    foreach (GridColumn column in Columns)
                    {
                        if (column.Binary)
                        {
                            continue;
                        }
                        var cell = worksheet.Cell(rowIdx, colIdx);
                        cell.Value = column.Label;
                        cell.Style.Font.Bold = true;
                        worksheet.Column(colIdx).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                        switch (column.DataType)
                        {
                            case nameof(Double):
                            case nameof(Decimal):
                            case nameof(Int32):
                            case nameof(Int64):
                                worksheet.Column(colIdx).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                                break;
                            case nameof(DateTime):
                            case nameof(TimeSpan):
                                worksheet.Column(colIdx).Width = 10;
                                break;
                            case nameof(Boolean):
                                break;
                            default:
                                columnWidths[column.ColumnName] = 0;
                                break;
                        }
                        colIdx++;
                    }

                    while (Database.Reader.Read())
                    {
                        rowIdx++;
                        colIdx = 1;
                        object[] values = new object[Database.Reader.FieldCount];
                        Database.Reader.GetValues(values);

                        foreach (GridColumn column in Columns)
                        {
                            if (column.Binary)
                            {
                                continue;
                            }
                            object value = values[colIdx - 1];

                            if (value == null || value == DBNull.Value)
                            {
                                worksheet.Cell(rowIdx, colIdx).Value = string.Empty;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(column.Lookup) == false)
                                {
                                    value = GetLookupValue(column, value);
                                }

                                var cell = worksheet.Cell(rowIdx, colIdx);

                                switch (column.DataType)
                                {
                                    case nameof(Double):
                                        cell.Value = Convert.ToDouble(value);
                                        break;
                                    case nameof(Decimal):
                                        cell.Value = Convert.ToDecimal(value);
                                        break;
                                    case nameof(Int16):
                                    case nameof(Int32):
                                    case nameof(Int64):
                                        cell.Value = Convert.ToInt64(value);
                                        break;
                                    case nameof(DateTime):
                                        cell.Value = Convert.ToDateTime(value);
                                        break;
                                    case nameof(TimeSpan):
                                        cell.Value = TimeSpan.Parse(value?.ToString() ?? string.Empty);
                                        break;
                                    case nameof(Boolean):
                                        cell.Value = Convert.ToBoolean(value);
                                        break;
                                    default:
                                        cell.Value = value.ToString();
                                        break;
                                }

                                if (columnWidths.ContainsKey(column.ColumnName))
                                {
                                    if (value.ToString().Length > columnWidths[column.ColumnName])
                                    {
                                        columnWidths[column.ColumnName] = value.ToString().Length;
                                    }
                                }
                            }

                            colIdx++;
                        }
                    }

                    colIdx = 0;
                    foreach (GridColumn column in Columns)
                    {
                        colIdx++;

                        if (columnWidths.ContainsKey(column.ColumnName))
                        {
                            var width = columnWidths[column.ColumnName];

                            if (width < 10)
                            {
                                continue;
                            }

                            if (width > 50)
                            {
                                width = 50;
                            }
                            worksheet.Column(colIdx).Width = width * 0.8;
                        }
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        workbook.SaveAs(memoryStream);
                        HttpContext.Response.ContentType = GetMimeTypeForFileExtension($".xlsx"); ;
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        private async Task<T> GetRequest<T>()
        {
            var request = HttpContext.Request;
            using (var streamReader = new HttpRequestStreamReader(request.Body, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var json = await JObject.LoadAsync(jsonReader);
                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                options.Converters.Add(new JsonStringEnumConverter());
                return json.ToObject<T>();
            }
        }

        private string JavaScriptTypeName(GridColumn column)
        {
            switch (column.DataType)
            {
                case nameof(Single):
                case nameof(Double):
                case nameof(Decimal):
                case nameof(Int16):
                case nameof(Int32):
                case nameof(Int64):
                    return "number";
                case nameof(DateTime):
                case nameof(TimeSpan):
                    return "date";
                case nameof(Boolean):
                    return "boolean";
                default:
                    return "string";
            }
        }
        private byte[] GetColumnData()
        {
            byte[] byteData = new byte[0];

            GridColumn primaryKeyColumn = this.Columns.FirstOrDefault(c => c.PrimaryKey);

            if (primaryKeyColumn == null)
            {
                throw new Exception("A primary key column must be specified");
            }

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query = new QueryCommandConfig();

                query.Sql = $"select {this.ColumnName} from {FromPart} where {primaryKeyColumn.ColumnExpression} = {ParamName(primaryKeyColumn, false)}";
                query.Params.Add(ParamName(primaryKeyColumn, true), ConvertToDbParam(this.PrimaryKey, primaryKeyColumn));
                Database.ExecuteQuery(query);

                if (Database.Reader.Read())
                {
                    byteData = (byte[])Database.Reader[0];
                }
                Database.Close();
            }

            HttpContext.Response.ContentType = GetMimeTypeForFileExtension($".{Extension}"); ;
            return byteData;
        }

        private string GetMimeTypeForFileExtension(string extension)
        {
            const string defaultContentType = "application/octet-stream";

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(extension, out string contentType))
            {
                contentType = defaultContentType;
            }

            return contentType;
        }

        private async Task Grid(DbNetGridResponse response)
        {
            if (ValidateRequest(response) == false)
            {
                return;
            }
            ConfigureColumns();

            DataTable dataTable = new DataTable();

            foreach (GridColumn gridColumn in Columns)
            {
                DataColumn dataColumn = new DataColumn(gridColumn.ColumnName);
                dataColumn.DataType = GetColumnType(gridColumn.OriginalDataType == "Byte[]" ? nameof(String) : gridColumn.OriginalDataType);
                dataTable.Columns.Add(dataColumn);
            }

            TotalRows = -1;
            using (Database)
            {
                Database.Open();
                QueryCommandConfig query;
                if (OptimizeForLargeDataset)
                {
                    query = BuildSql(QueryBuildModes.Count);
                    Database.ExecuteQuery(query);
                    Database.Reader.Read();
                    TotalRows = Convert.ToInt64(Database.Reader.GetValue(0));
                }

                query = BuildSql();

                Database.Open();
                Database.ExecuteQuery(query);

                int counter = 0;
                long startRow = (CurrentPage - 1) * PageSize;
                long endRow = CurrentPage * PageSize;

                while (Database.Reader.Read())
                {
                    counter++;

                    if ((counter > startRow && counter <= endRow) || PageSize <= 0)
                    {
                        object[] values = new object[Database.Reader.FieldCount];
                        Database.Reader.GetValues(values);
                        dataTable.Rows.Add(values);
                    }

                    if (TotalRows > -1 && PageSize > 0 && counter > endRow)
                    {
                        break;
                    }
                }

                if (TotalRows == -1)
                {
                    TotalRows = counter;
                }

                if (PageSize <= 0)
                {
                    PageSize = TotalRows;
                }
                TotalPages = (int)Math.Ceiling((double)TotalRows / (double)PageSize);

                Database.Close();
            }

            DataTable totalsDataTable = GetTotalsDataTable();

            response.CurrentPage = CurrentPage;
            response.TotalPages = TotalPages;
            response.TotalRows = TotalRows;
            response.Columns = Columns.ToList<GridColumn>().ToList();

            var viewModel = new GridViewModel
            {
                GridData = dataTable,
                GridTotals = totalsDataTable
            };

            ReflectionHelper.CopyProperties(this, viewModel);
            viewModel.Columns = Columns;
            viewModel.LookupTables = _lookupTables;

            response.Data = await HttpContext.RenderToStringAsync("Views/DbNetGrid/Grid.cshtml", viewModel);
        }

        private DataTable GetTotalsDataTable()
        {
            DataTable totalsDataTable = new DataTable();

            using (Database)
            {
                QueryCommandConfig query = BuildSql(QueryBuildModes.Totals);

                if (query != null)
                {
                    Database.Open();
                    Database.ExecuteQuery(query);

                    for (var i = 0; i < Database.Reader.FieldCount; i++)
                    {
                        DataColumn dataColumn = new DataColumn(Database.Reader.GetName(i));
                        totalsDataTable.Columns.Add(dataColumn);
                    }

                    while (Database.Reader.Read())
                    {
                        object[] values = new object[Database.Reader.FieldCount];
                        Database.Reader.GetValues(values);
                        totalsDataTable.Rows.Add(values);
                    }

                    Database.Close();
                }
            }

            return totalsDataTable;
        }
        private KeyValuePair<string, object>? ParseFilterColumnValue(string filterColumnValue, GridColumn gridColumn)
        {
            string comparisionOperator = "=";

            if (filterColumnValue.StartsWith(">=") || filterColumnValue.StartsWith("<="))
            {
                comparisionOperator = filterColumnValue.Substring(0, 2);
            }
            else if (filterColumnValue.StartsWith(">") || filterColumnValue.StartsWith("<"))
            {
                comparisionOperator = filterColumnValue.Substring(0, 1);
            }

            if (comparisionOperator != "=")
            {
                filterColumnValue = filterColumnValue.Substring(comparisionOperator.Length);
            }

            switch (gridColumn.DataType)
            {
                case nameof(Int16):
                case nameof(Int32):
                case nameof(Int64):
                case nameof(Decimal):
                case nameof(Single):
                case nameof(Double):
                    return new KeyValuePair<string, object>(comparisionOperator, filterColumnValue);
                case nameof(Boolean):
                    return new KeyValuePair<string, object>("=", Convert.ToBoolean(filterColumnValue));
                case nameof(DateTime):
                    try
                    {
                        return new KeyValuePair<string, object>(comparisionOperator, Convert.ToDateTime(filterColumnValue));
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                case nameof(TimeSpan):
                    try
                    {
                        return new KeyValuePair<string, object>(comparisionOperator, TimeSpan.Parse(filterColumnValue ?? string.Empty));
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                default:
                    return new KeyValuePair<string, object>("like", $"%{filterColumnValue}%");
            }
        }

        private string RefineSearchExpression(DbColumn col)
        {
            string columnExpression = StripColumnRename(col.ColumnExpression);

            if (col.DataType != typeof(DateTime).Name)
            {
                return columnExpression;
            }

            switch (Database.Database)
            {
                case DatabaseType.SqlServer:
                case DatabaseType.SqlServerCE:
                    if (col.DbDataType != "31") // "Date"
                        columnExpression = $"CONVERT(DATE,{columnExpression})";
                    break;
                case DatabaseType.Oracle:
                    columnExpression = $"trunc({columnExpression})";
                    break;
                case DatabaseType.PostgreSql:
                    columnExpression = $"date_trunc('day',{columnExpression})";
                    break;
                case DatabaseType.Firebird:
                    columnExpression = $"cast({columnExpression} AS DATE)";
                    break;
                case DatabaseType.Sybase:
                    columnExpression = $"convert(datetime, convert(binary(4), {columnExpression}))";
                    break;
            }

            return columnExpression;
        }

        private bool PrimaryKeySupplied()
        {
            foreach (DbColumn col in Columns)
                if (col.PrimaryKey)
                    return true;

            return false;
        }

        private async Task<string> Toolbar()
        {
            var viewModel = new ToolbarViewModel();

            ReflectionHelper.CopyProperties(this, viewModel);

            var contents = await HttpContext.RenderToStringAsync("Views/DbNetGrid/Toolbar.cshtml", viewModel);
            return contents;
        }

        private async Task ViewDialog(DbNetGridResponse response)
        {
            var viewModel = new ViewDialogViewModel();

            ReflectionHelper.CopyProperties(this, viewModel);

            response.Toolbar = await HttpContext.RenderToStringAsync("Views/DbNetGrid/ViewDialog.cshtml", viewModel);

            viewModel.ViewData = GetViewData();
            viewModel.Columns = Columns;
            viewModel.LookupTables = _lookupTables;

            response.Record = CreateRecord(viewModel.ViewData);
            response.Data = await HttpContext.RenderToStringAsync("Views/DbNetGrid/ViewDialogContent.cshtml", viewModel);
        }

        private async Task SearchDialog(DbNetGridResponse response)
        {
            var searchDialogViewModel = new SearchDialogViewModel();
            ReflectionHelper.CopyProperties(this, searchDialogViewModel);
            searchDialogViewModel.Columns = Columns;
            searchDialogViewModel.LookupTables = _lookupTables;
            response.Data = await HttpContext.RenderToStringAsync("Views/DbNetGrid/SearchDialog.cshtml", searchDialogViewModel);
        }
        private async Task LookupDialog(DbNetGridResponse response)
        {
            var lookupDialogViewModel = new LookupDialogViewModel();
            ReflectionHelper.CopyProperties(this, lookupDialogViewModel);
            response.Toolbar = await HttpContext.RenderToStringAsync("Views/DbNetGrid/LookupDialog.cshtml", lookupDialogViewModel);

            var dataTable = GetLookupData(_dbNetGridRequest.LookupColumnIndex);
            var dataView = new DataView(dataTable);
            dataView.Sort = $"{dataTable.Columns[dataTable.Columns.Count - 1].ColumnName} ASC";
            lookupDialogViewModel.LookupData = dataView.ToTable();

            response.Data = await HttpContext.RenderToStringAsync("Views/DbNetGrid/LookupDialogContent.cshtml", lookupDialogViewModel);
        }

        private Dictionary<string, object> CreateRecord(DataTable dataTable)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.DataType == typeof(Byte[]))
                {
                    continue;
                }
                dictionary[column.ColumnName.ToLower()] = dataTable.Rows[0][column.ColumnName];
            }

            return dictionary;
        }

        private DataTable GetViewData()
        {
            DataTable dataTable = new DataTable();

            ConfigureColumns();

            foreach (GridColumn gridColumn in Columns)
            {
                DataColumn dataColumn = new DataColumn(gridColumn.ColumnName);
                dataTable.Columns.Add(dataColumn);
            }

            GridColumn primaryKeyColumn = this.Columns.Where(c => c.PrimaryKey).FirstOrDefault();

            if (primaryKeyColumn == null)
            {
                throw new Exception("A primary key column must be specified");
            }

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query = new QueryCommandConfig();

                query.Sql = $"select {BuildSelectPart(QueryBuildModes.View)} from {FromPart} where {primaryKeyColumn.ColumnExpression} = {ParamName(primaryKeyColumn, false)}";
                query.Params.Add(ParamName(primaryKeyColumn, true), ConvertToDbParam(this.PrimaryKey, primaryKeyColumn));
                Database.ExecuteQuery(query);
                Database.Reader.Read();
                object[] values = new object[Database.Reader.FieldCount];
                Database.Reader.GetValues(values);
                dataTable.Rows.Add(values);

                Database.Close();
            }

            return dataTable;
        }

        private DataTable GetLookupData(int columnIndex)
        {
            GridColumn gridColumn = Columns.FirstOrDefault(c => c.Index == columnIndex);
            DataTable dataTable = new DataTable();

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query = new QueryCommandConfig();
                query.Sql = Database.UpdateConcatenationOperator(gridColumn!.Lookup);
                Database.ExecuteQuery(query);
                dataTable.Load(Database.Reader);
                Database.Close();
            }

            return dataTable;
        }

        private string GetLookupValue(GridColumn column, object dataValue)
        {
            DataTable lookupTable = _lookupTables[column.ColumnKey];
            foreach (DataRow row in lookupTable.Rows)
            {
                if (row[0].ToString() == dataValue.ToString())
                {
                    return row[1].ToString();
                }
            }
            return dataValue.ToString();
        }

        private bool ValidateRequest(DbNetGridResponse response)
        {
            response.Message = String.Empty;

            if (SearchParams.Any())
            {
                object convertedValue = new object();

                foreach (SearchParameter searchParameter in SearchParams)
                {
                    GridColumn gridColumn = Columns[searchParameter.ColumnIndex];

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

        public string Translate(string key)
        {
            return ResourceManager.GetString(key) ?? $"*{key}*";
        }
    }
}