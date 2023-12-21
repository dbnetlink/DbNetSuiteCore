using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DbNetSuiteCore.Constants;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Extensions;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.ViewModels.DbNetGrid;
using Microsoft.AspNetCore.Mvc;
using GridColumn = DbNetSuiteCore.Models.DbNetGrid.GridColumn;
using static DbNetSuiteCore.Utilities.DbNetDataCore;
using DbNetSuiteCore.Constants.DbNetGrid;
using DbNetSuiteCore.Models.DbNetGrid;
using Microsoft.AspNetCore.Http;

namespace DbNetSuiteCore.Services
{
    public class DbNetGrid : DbNetGridEdit
    {
        private Dictionary<string, object> _columnProperties = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, object> _resp = new Dictionary<string, object>();
        const string NullValueToken = "@@null@@";
        public List<GridColumn> Columns { get; set; } = new List<GridColumn>();
        private string _procedureName;
        private long _pageSize = 20;

        public DbNetGrid(AspNetCoreServices services) : base(services)
        {
        }

        public BooleanDisplayMode BooleanDisplayMode { get; set; } = BooleanDisplayMode.TrueFalse;
        public Dictionary<string, object> ColumnFilterParams { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, string> ColumnFilters { get; set; } = new Dictionary<string, string>();
        public List<string> ColumnFilterSql { get; set; } = new List<string> { };
        public bool Copy { get; set; } = true;
        public int CurrentPage { get; set; } = 1;
        public GridColumn DefaultColumn { get; set; }
        public bool Export { get; set; } = true;
        public FilterColumnModeValues FilterColumnMode { get; set; } = FilterColumnModeValues.Simple;
        public string FilterSql { get; set; } = string.Empty;
        public string FixedOrderBy { get; set; } = string.Empty;
        public bool FrozenHeader { get; set; } = false;
        public GridGenerationMode GridGenerationMode { get; set; } = GridGenerationMode.Display;
        public bool GroupBy { get; set; } = false;
        public string Having { get; set; } = string.Empty;
        public bool IgnorePrimaryKeys { get; set; } = false;
        public bool MultiRowSelect { get; set; } = false;
        public MultiRowSelectLocation MultiRowSelectLocation { get; set; } = MultiRowSelectLocation.Left;
        public bool NestedGrid { get; set; } = false;
        public int? OrderBy { get; set; } = null;
        public OrderByDirection OrderByDirection { get; set; } = OrderByDirection.asc;
        public long PageSize
        {
            get => Navigation == false ? -1 : _pageSize;
            set => _pageSize = value;
        }
        public Dictionary<string, object> ParentFilterParams { get; set; } = new Dictionary<string, object>();
        public List<string> ParentFilterSql { get; set; } = new List<string> { };
        public string ProcedureName
        {
            get => EncodingHelper.Decode(_procedureName);
            set => _procedureName = value;
        }
        public Dictionary<string, object> ProcedureParams { get; set; }
        public string SelectModifier { get; set; } = string.Empty;
        public int TotalPages { get; set; } = 0;
        public long TotalRows { get; set; } = 0;
        public bool Update { get; set; } = false;
        public bool View { get; set; } = false;
        public int ViewLayoutColumns { get; set; }
        public string ExportExtension { get; set; } = string.Empty;

        public static Type GetNullableType(Type type)
        {
            if (Nullable.GetUnderlyingType(type) == null)
                return type;
            else
                return Nullable.GetUnderlyingType(type);
        }

        public new async Task<object> Process()
        {
            var request = await DeserialiseRequest<DbNetGridRequest>();
            Columns = request.Columns;

            await GridEditInitialise();

            DbNetGridResponse response = new DbNetGridResponse();

            if (string.IsNullOrEmpty(this.ProcedureName) == false)
            {
                View = false;
                Search = false;
                QuickSearch = false;
            }

            switch (Action.ToLower())
            {
                case RequestAction.Initialize:
                    await Grid(response, true);
                    break;
                case RequestAction.Page:
                    await Grid(response);
                    break;
                case RequestAction.Export:
                    return ExportGrid(response);
                case RequestAction.DownloadColumnData:
                    return GetColumnData();
                case RequestAction.ViewContent:
                    await ViewDialog(response);
                    break;
                case RequestAction.SearchDialog:
                    await SearchDialog(response);
                    break;
                case RequestAction.Lookup:
                    await LookupDialog(response);
                    break;
                case RequestAction.DataArray:
                    return DataArray();
                case RequestAction.DataTable:
                    response.Toolbar = await Toolbar();
                    GridGenerationMode = GridGenerationMode.DataTable;
                    await Grid(response);
                    break;
                case RequestAction.GridRow:
                    await GridRow(response);
                    break;
                case RequestAction.DeleteRecord:
                    DeleteRecord(response);
                    break;
            }

            Database.Close();

            foreach (GridColumn gridColumn in response.Columns ?? new List<GridColumn>())
            {
                gridColumn.LookupDataTable = null;
            }

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(response, serializeOptions);
        }

        internal static Dictionary<string, object> CaseInsensitiveDictionary(Dictionary<string, object> dictionary)
        {
            Dictionary<string, object> caseInsensitiveDictionary = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

            foreach (string k in dictionary.Keys)
                caseInsensitiveDictionary.Add(k, dictionary[k]);

            return caseInsensitiveDictionary;
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
            this.Insert = false;
            this.Update = false;
            this.Delete = false;
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
                        column = InitialiseColumn(new GridColumn(), row) as GridColumn;
                    }
                    column.Index = columnIndex++;

                    ConfigureColumn(column, row);

                    Columns.Add(column);
                }
            }
        }

        internal string DefaultOrderBy()
        {
            return string.IsNullOrEmpty(this.InitialOrderBy) ? $"{Columns.First().ColumnExpression} asc" : this.InitialOrderBy;
        }
        internal QueryCommandConfig ProcedureCommandConfig()
        {
            QueryCommandConfig queryCommandConfig = new QueryCommandConfig(ProcedureName);

            List<string> parameterNames = new List<string>();

            using (Database)
            {
                Database.Open();
                try
                {
                    queryCommandConfig.Params = Database.DeriveParameters(queryCommandConfig.Sql);
                    parameterNames = queryCommandConfig.Params.Keys.Cast<string>().Select(k => k.ToLower()).ToList();
                }
                catch (Exception) { }
            }

            foreach (string paramName in this.ProcedureParams.Keys)
            {
                if (parameterNames.Any())
                {
                    if (parameterNames.Contains(paramName.ToLower()) == false)
                    {
                        throw new Exception($"Parameter [{paramName}] is not a valid parameter name for stored procedure [{ProcedureName}]");
                    }
                }
                Database.SetParamValue(queryCommandConfig.Params, paramName, ProcedureParams[paramName]);
            }

            return queryCommandConfig;
        }
        protected virtual string BuildFilterPart()
        {
            List<string> fp = new List<string>();

            string foreignKeyFilter = ForeignKeyFilter();

            if (string.IsNullOrEmpty(foreignKeyFilter) == false)
            {
                fp.Add(foreignKeyFilter);
            }

            if (string.IsNullOrEmpty(FixedFilterSql) == false)
            {
                fp.Add(FixedFilterSql);
            }

            if (String.IsNullOrEmpty(QuickSearchToken) == false)
            {
                fp.Add($"({string.Join(" or ", QuickSearchFilter(Columns.Cast<DbColumn>().ToList()).ToArray())})");
            }

            if (ColumnFilters.Keys.Any())
            {
                List<string> columnFilterPart = new List<string>();
                foreach (string columnName in ColumnFilters.Keys)
                {
                    GridColumn col = Columns.First(c => c.IsMatch(columnName));
                    if (col.HasLookup == false || col.FilterMode == FilterColumnSelectMode.List || col.LookupColumns < 2)
                    {
                        var columnFilter = ParseFilterColumnValue(ColumnFilters[columnName], col);
                        if (columnFilter != null)
                        {
                            columnFilterPart.Add($"{RefineSearchExpression(col)} {columnFilter.Value.Key} {ParamName(col, ParamNames.ColumnFilter)}");
                        }
                    }
                    else
                    {
                        if (col.LookupIsDataTable)
                        {
                            var columnFilter = ParseFilterColumnValue(ColumnFilters[columnName], col);
                            string filterValue = columnFilter.Value.Value.ToString().Replace("%", string.Empty).ToLower();
                            List<object> values = _lookupTables[col.ColumnKey].AsEnumerable().Where(r => r.ItemArray[1].ToString().ToLower().Contains(filterValue)).Select(r => r[0]).ToList();
                            columnFilterPart.Add($"{col.ColumnName} in ({string.Join(",",values)})");
                        }
                        else
                        {
                            columnFilterPart.Add($"{col.ColumnName} in ({SearchLookupSql(col, ParamName(col, ParamNames.ColumnFilter))})");
                        }
                    }
                }
                fp.Add($"({string.Join(" and ", columnFilterPart.ToArray())})");
            }

            if (SearchParams.Any())
            {
                fp.Add($"({string.Join($" {SearchFilterJoin} ", SearchDialogFilter(Columns.Cast<DbColumn>().ToList()).ToArray())})");
            }

            string filterPart = string.Join(" and ", fp.ToArray());

            return filterPart;
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
                ForeignKeyFilter(parameters);
                /*
                if (fkColumn.ForeignKeyValue.ToString() != nameof(System.DBNull))
                {
                    if (fkColumn.ForeignKeyValue is List<object>)
                    {
                        List<object> foreignKeyValues = fkColumn.ForeignKeyValue as List<object>;
                        for (var i = 0; i < foreignKeyValues.Count; i++)
                        {
                            parameters.Add(Database.ParameterName($"{fkColumn.ColumnName}{i}"), ConvertToDbParam(foreignKeyValues[i], fkColumn));
                        }
                    }
                    else
                    {
                        parameters.Add(ParamName(fkColumn, true), ConvertToDbParam(fkColumn.ForeignKeyValue, fkColumn));
                    }
                }
                */
            }

            if (FixedFilterParams.Keys.Any())
            {
                foreach (string paramName in FixedFilterParams.Keys)
                {
                    parameters.Add(ParamName(paramName), ConvertToDbParam(FixedFilterParams[paramName], null));
                }
            }

            if (String.IsNullOrEmpty(QuickSearchToken) == false)
            {
                parameters.Add(ParamName(ParamNames.QuickSearchToken, true), ConvertToDbParam($"%{QuickSearchToken}%", null));
            }

            if (ColumnFilters.Keys.Any())
            {
                foreach (string columnName in ColumnFilters.Keys)
                {
                    GridColumn gridColumn = Columns.First(x => x.IsMatch(columnName));
                    var columnFilter = ParseFilterColumnValue(ColumnFilters[columnName], gridColumn);
                    if (columnFilter != null)
                    {
                        parameters.Add(ParamName(gridColumn, ParamNames.ColumnFilter, true), ConvertToDbParam(columnFilter.Value.Value, gridColumn));
                    }
                }
            }

            if (SearchParams.Any())
            {
                AddSearchDialogParameters(Columns.Cast<DbColumn>().ToList(), parameters);
            }

            return parameters;
        }

        protected QueryCommandConfig BuildSql()
        {
            return BuildSql(QueryBuildModes.Normal);
        }
        protected QueryCommandConfig BuildSql(QueryBuildModes buildMode)
        {
            if (ProcedureName != "")
            {
                return ProcedureCommandConfig();
            }

            return BuildSql(BuildSelectPart<GridColumn>(buildMode, Columns, GroupBy), BuildFilterPart(), BuildOrderPart(buildMode), BuildGroupByPart(buildMode), buildMode);
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

        private void ConfigureGridColumns()
        {
            if (!String.IsNullOrEmpty(ProcedureName))
            {
                ConfigureStoredProcedure();
                return;
            }

            Columns = ConfigureColumns(Columns, GroupBy);

            if (DefaultColumn != null)
            {
                Columns.ToList().ForEach(c => c.Filter = DefaultColumn.Filter);
                Columns.ToList().ForEach(c => c.View = DefaultColumn.View);
                Columns.ToList().ForEach(c => c.Style = DefaultColumn.Style);
            }

            if (OrderBy.HasValue == false)
            {
                if (string.IsNullOrEmpty(InitialOrderBy))
                {
                    if (Columns.Any(c => c.Display.Value))
                    {
                        var orderByColumn = Columns.FirstOrDefault(c => c.Display.Value);
                        if (orderByColumn.DbDataType != "ntext")
                        {
                            OrderBy = Columns.FirstOrDefault(c => c.Display.Value).Index + 1;
                        }
                    }
                }
                else
                {
                    string orderBy = InitialOrderBy.Split(",").First();
                    GridColumn orderByColumn = Columns.FirstOrDefault(c => c.IsMatch(InitialOrderBy.Split(" ").First()));
                    if (orderByColumn != null)
                    {
                        OrderBy = orderByColumn.Index + 1;
                        OrderByDirection = InitialOrderBy.ToLower().Split(" ").Contains("desc") ? OrderByDirection.desc : OrderByDirection.asc;
                    }
                }
            }

            if (OrderBy.HasValue)
            {
                if (Columns[OrderBy.Value - 1].OrderBy.HasValue == false)
                {
                    Columns[OrderBy.Value - 1].OrderBy = OrderByDirection;
                }
            }

            if (Columns.Any(c => c.View) == false)
            {
                Columns.ToList().ForEach(c => c.View = true);
            }
        }

        protected override void AddColumn(DataRow row)
        {
            Columns.Add(InitialiseColumn(new GridColumn(), row) as GridColumn);
        }
        private async Task<byte[]> GenerateHtmlExport(DbNetGridResponse response)
        {
            GridGenerationMode = GridGenerationMode.Export;
            await Grid(response);
            var cssFiles = new List<string>() { "dbnetsuite", "dbnetgrid" };
            string css = string.Empty;
            foreach (var cssFile in cssFiles)
            {
                css += TextHelper.StripBOM(await GetResourceString($"CSS.{cssFile}.css"));
            }
            var viewModel = new HtmlExportViewModel
            {
                GridHtml = response.Data.ToString(),
                GridCss = css
            };
            string html = await HttpContext.RenderToStringAsync("Views/DbNetGrid/HtmlExport.cshtml", viewModel);
            HttpContext.Response.ContentType = GetMimeTypeForFileExtension($".html"); 
            return Encoding.UTF8.GetBytes(html);
        }

        private string DataArray()
        {
            ConfigureGridColumns();

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
                        else if (_lookupTables.Keys.Contains(column.ColumnKey))
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

        private string Serialize(object data, bool indent = false)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (indent) {
                serializeOptions.WriteIndented = true;
            }
            return JsonSerializer.Serialize(data, serializeOptions);
        }

        private byte[] ExportGrid(DbNetGridResponse response)
        {
            switch (ExportExtension)
            {
                case "xlsx":
                    return GenerateSpreadsheet();
                case "json":
                    return SerializeGrid();
                default:
                    return GenerateHtmlExport(response).Result;
            }
        }

        public byte[] SerializeGrid()
        {
            var query = BuildSql();

            Database.Open();
            Database.ExecuteQuery(query);

            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < Database.Reader.FieldCount; i++)
                cols.Add(Database.Reader.GetName(i));

            while (Database.Reader.Read())
                results.Add(ConvertToDictionary(cols, Database.Reader));

            string json = Serialize(results, true);
            HttpContext.Response.ContentType = GetMimeTypeForFileExtension($".json");
            return Encoding.UTF8.GetBytes(json);
        }
        private Dictionary<string, object> ConvertToDictionary(IEnumerable<string> cols, IDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }

        private byte[] GenerateSpreadsheet()
        {
            ConfigureGridColumns();

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

                        if (column.IsNumeric)
                        {
                            worksheet.Column(colIdx).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                        }
                        else
                        {
                            switch (column.DataType)
                            {
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
                                if (_lookupTables.Keys.Contains(column.ColumnKey))
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

        private async Task Grid(DbNetGridResponse response, bool initialise = false)
        {
            if (ValidateRequest(response, Columns) == false)
            {
                return;
            }
            ConfigureGridColumns();

            if (initialise)
            {
                response.Toolbar = await Toolbar();
            }

            switch (GridGenerationMode)
            {
                case GridGenerationMode.DataTable:
                case GridGenerationMode.Export:
                    PageSize = -1;
                    break;
            }

            DataTable dataTable = InitialiseDataTable();

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
                    TotalPages = 1;
                }
                else
                {
                    TotalPages = (int)Math.Ceiling((double)TotalRows / (double)Math.Abs(PageSize));
                }
            }

            DataTable totalsDataTable = GetTotalsDataTable();

            response.CurrentPage = CurrentPage;
            response.TotalPages = TotalPages;
            response.TotalRows = TotalRows;
            response.Columns = Columns;

            var viewModel = new GridViewModel
            {
                GridData = dataTable,
                GridTotals = totalsDataTable
            };

            ReflectionHelper.CopyProperties(this, viewModel);
            viewModel.Columns = Columns;
            viewModel.LookupTables = _lookupTables;

            string viewName = this.GridGenerationMode == GridGenerationMode.DataTable ? "DataTable" : "Grid";

            response.Data = await HttpContext.RenderToStringAsync($"Views/DbNetGrid/{viewName}.cshtml", viewModel);
        }

        private async Task GridRow(DbNetGridResponse response)
        {
            ConfigureGridColumns();

            DataTable dataTable = InitialiseDataTable();

            using (Database)
            {
                Database.Open();

                QueryCommandConfig query = new QueryCommandConfig();
                query.Sql = $"select {BuildSelectPart<GridColumn>(QueryBuildModes.Normal, Columns)} from {FromPart} where {PrimaryKeyFilter(query.Params)}"; ;

                Database.Open();
                Database.ExecuteQuery(query);
                Database.Reader.Read();
                object[] values = new object[Database.Reader.FieldCount];
                Database.Reader.GetValues(values);
                dataTable.Rows.Add(values);
                PageSize = 1;
                TotalPages = 1;
            }
            var viewModel = new GridViewModel
            {
                GridData = dataTable
            };
            ReflectionHelper.CopyProperties(this, viewModel);
            viewModel.Columns = Columns;
            viewModel.LookupTables = _lookupTables;
            response.Html = await HttpContext.RenderToStringAsync($"Views/DbNetGrid/Grid.cshtml", viewModel);
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

            if (gridColumn.IsNumeric)
            {
                return new KeyValuePair<string, object>(comparisionOperator, filterColumnValue);
            }
            switch (gridColumn.DataType)
            {
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

            response.Dialog = await HttpContext.RenderToStringAsync("Views/DbNetGrid/ViewDialog.cshtml", viewModel);

            viewModel.ViewData = GetViewData();
            viewModel.Columns = Columns;
            viewModel.LookupTables = _lookupTables;
            viewModel.LayoutColumns = ViewLayoutColumns;

            response.Record = CreateRecord(viewModel.ViewData, Columns.Cast<DbColumn>().ToList());
            response.Data = await HttpContext.RenderToStringAsync("Views/DbNetGrid/ViewDialogContent.cshtml", viewModel);
        }

        private DataTable GetViewData()
        {
            DataTable dataTable = new DataTable();

            ConfigureGridColumns();

            foreach (GridColumn gridColumn in Columns)
            {
                DataColumn dataColumn = new DataColumn(gridColumn.ColumnName);
                dataTable.Columns.Add(dataColumn);
            }

            if (string.IsNullOrEmpty(this.PrimaryKey))
            {
                throw new Exception("A primary key column must be specified");
            }

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query = new QueryCommandConfig();

                query.Sql = $"select {BuildSelectPart(QueryBuildModes.View, Columns)} from {FromPart} where {PrimaryKeyFilter(query.Params)}";
                Database.ExecuteQuery(query);
                Database.Reader.Read();
                object[] values = new object[Database.Reader.FieldCount];
                Database.Reader.GetValues(values);
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
        private string GetLookupValue(GridColumn column, object dataValue)
        {
            if (column.LookupColumns < 2)
            {
                return dataValue?.ToString() ?? string.Empty;
            }
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
    }
}