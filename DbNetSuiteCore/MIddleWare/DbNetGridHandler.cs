using DbNetSuiteCore.Services;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Models.Configuration;
using DbNetSuiteCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace DbNetSuiteCore.Middleware
{
    public class DbNetGridHandler
    {
        protected IDbNetData Db = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private DbNetGridConfiguration _DbNetGridConfiguration;
        private readonly IConfiguration _configuration;
        public const string Extension = ".dbnetgrid";
        public IViewRenderService _viewRenderService;
        private readonly RequestDelegate _next;

        public DbNetGridHandler(RequestDelegate next,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment hostingEnvironment,
            IViewRenderService viewRenderService,
            IDbNetData dbNetData
            )
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _viewRenderService = viewRenderService;
            _next = next;
            Db = dbNetData;
        }

        public async Task Invoke(HttpContext context)
        {
            _DbNetGridConfiguration = SerialisationHelper.DeserialiseJson<DbNetGridConfiguration>(context.Request.Body);

   //         Db = new DbNetData(_DbNetGridConfiguration.ConnectionString, _DbNetGridConfiguration.DataProvider, _httpContextAccessor, _hostingEnvironment, _configuration);
            Db.Open(_DbNetGridConfiguration.ConnectionString, _DbNetGridConfiguration.DataProvider);

            var handler = (string)context.Request.Query["handler"] ?? string.Empty;

            switch (handler.ToLower())
            {
                case "init":
                    ConfigureColumns();
                    _DbNetGridConfiguration.Html["toolbar"] = await _viewRenderService.RenderToStringAsync("DbNetGrid", _DbNetGridConfiguration);
                    GetPage();
                    _DbNetGridConfiguration.Html["page"] = await _viewRenderService.RenderToStringAsync("DbNetGrid_Page", _DbNetGridConfiguration);
                    break;
                case "page":
                    GetPage();
                    _DbNetGridConfiguration.Html["page"] = await _viewRenderService.RenderToStringAsync("DbNetGrid_Page", _DbNetGridConfiguration);
                    break;
            }

            Db.Close();
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(SerialisationHelper.SerialiseToJson(_DbNetGridConfiguration));
         //   await _next.Invoke(context);
        }

        private void GetPage()
        {
            string Sql = $"select {ColumnList()} from {_DbNetGridConfiguration.TableName} where 1=2";
            DataTable dt = Db.GetDataTable(Sql);

            QueryCommandConfig query = new QueryCommandConfig();
            query.Sql = $"select {ColumnList()} from {_DbNetGridConfiguration.TableName}";
            AddSearchTokenFilter(query);
            AddDropDownFilter(query);
            AddOrderBy(query);
            AddLookupData();

            int firstRecord = (_DbNetGridConfiguration.PageSize * (_DbNetGridConfiguration.CurrentPage - 1)) + 1;
            int lastRecord = (firstRecord + (_DbNetGridConfiguration.PageSize - 1));
            int recordCounter = 0;
            Db.ExecuteQuery(query);

            while (Db.GetDataReader().Read())
            {
                recordCounter++;
                if (recordCounter >= firstRecord && recordCounter <= lastRecord)
                {
                    DataRow row = dt.NewRow();

                    for (var i = 0; i < Db.GetDataReader().FieldCount; i++)
                    {
                        row[i] = Db.ReaderValue(i);
                    }


                    dt.Rows.Add(row);
                }
            }

            _DbNetGridConfiguration.TotalRows = recordCounter;
            _DbNetGridConfiguration.PageData = dt;
        }

        private void ConfigureColumns()
        {
            Dictionary<string, bool> BaseTables = new Dictionary<string, bool>();

            List<GridColumn> userDefinedColumns = new List<GridColumn>(_DbNetGridConfiguration.Columns.ToArray());
            string Sql = $"select {ColumnList()} from {_DbNetGridConfiguration.TableName} where 1=2";
            _DbNetGridConfiguration.Columns.Clear();

            DataTable DT = Db.GetSchemaTable(Sql);

            for (var idx = 0; idx < DT.Rows.Count; idx++)
            {
                DataRow row = DT.Rows[idx];

                if (row.Table.Columns.Contains("IsHidden"))
                    if (Convert.ToBoolean(row["IsHidden"]))
                        continue;

                GridColumn gridColumn = (userDefinedColumns.Count() > idx) ? userDefinedColumns[idx] : null;
                _DbNetGridConfiguration.Columns.Add(GenerateColumn(row, gridColumn));
            }

            string EditableBaseTable = "";

            int rowCounter = 0;

            foreach (DataRow row in DT.Rows)
            {
                if (rowCounter >= _DbNetGridConfiguration.Columns.Count)
                    break;

                DbColumn C = _DbNetGridConfiguration.Columns[rowCounter];

                if (C.BaseTableName == "")
                    C.BaseTableName = Convert.ToString(row["BaseTableName"]);

                C.ColumnName = Convert.ToString(row["ColumnName"]);
                C.ColumnSize = Convert.ToInt32(row["ColumnSize"]);

                if (C.IsBoolean)
                    C.DataType = "Boolean";
                else
                    C.DataType = ((Type)row["DataType"]).Name;

                if (row.Table.Columns.Contains("ProviderType"))
                    C.DbDataType = row["ProviderType"].ToString();

                switch (C.DataType)
                {
                    case "Byte[]":
                        C.Search = false;
                        C.SimpleSearch = false;
                        break;
                    case "Guid":
                        if (!C.Required)
                            if (String.IsNullOrEmpty(C.Lookup))
                                C.ReadOnly = true;
                        break;
                    default:
                        if (!Convert.ToBoolean(row["AllowDBNull"]))
                            C.Required = true;
                        break;
                }

                if (C.PrimaryKey)
                    EditableBaseTable = C.BaseTableName;

                if (Convert.ToBoolean(row["IsKey"]))
                {
                    if (EditableBaseTable == "")
                        EditableBaseTable = C.BaseTableName;
                }

                if (Db.Database != DatabaseType.Oracle)
                    C.SequenceName = "";

                if (row.Table.Columns.Contains("IsAutoIncrement"))
                    if (!C.AutoIncrement)
                        C.AutoIncrement = Convert.ToBoolean(row["IsAutoIncrement"]);

                if (C.AutoIncrement || C.SequenceName != "")
                {
                    if (EditableBaseTable == "")
                        EditableBaseTable = C.BaseTableName;

                    if (EditableBaseTable == C.BaseTableName)
                        C.PrimaryKey = true;
                    C.AutoIncrement = true;
                    C.ReadOnly = true;
                    C.Required = false;
                }
                else if (C.PrimaryKey)
                    C.UpdateReadOnly = true;

                if (C.BulkInsert)
                    if (C.Lookup == "")
                        C.BulkInsert = false;

                if (C.Unique)
                    C.Required = true;

                if (C.Format == "")
                {
                    switch (C.DataType)
                    {
                        case "DateTime":
                            C.Format = "d";
                            break;
                        case "TimeSpan":
                            C.Format = "t";
                            break;
                    }
                }

                C.UploadRootFolder = C.UploadRootFolder.Replace("~", _hostingEnvironment.WebRootPath);

                if (C.IsBoolean)
                    C.EditControlType = ControlType.CheckBox;

                if (C.ReadOnly)
                {
                    C.InsertReadOnly = true;
                    C.UpdateReadOnly = true;
                }

                rowCounter++;
            }


            if (EditableBaseTable != "")
                BaseTables[EditableBaseTable.ToLower()] = true;

            int StandardSearchColumns = 0;
            int SimpleSearchColumns = 0;
            int ColumnOrder = 0;

            foreach (DbColumn Col in _DbNetGridConfiguration.Columns)
            {
                ColumnOrder++;

                if (Col.IsBoolean)
                    Col.EditControlType = ControlType.CheckBox;

                if (Col.BaseTableName == "" || Col.ForeignKey)
                {
                    Col.InsertReadOnly = true;
                    Col.UpdateReadOnly = true;
                }

                if (Col.Search)
                    StandardSearchColumns++;

                if (Col.DataType != "String" && !Col.Lookup.ToLower().StartsWith("select"))
                    Col.SimpleSearch = false;

                if (Col.SimpleSearch)
                    SimpleSearchColumns++;

                if (Col.Encryption != HashTypes.None)
                    Col.EditControlType = ControlType.Password;
            }

            foreach (DbColumn Col in _DbNetGridConfiguration.Columns)
            {
                if (Col.Lookup.Equals(string.Empty))
                    continue;

                if (Col.Lookup.StartsWith("["))
                {
                    Col.LookupTable = "";
                    Col.LookupTextField = "text";
                    Col.LookupValueField = "value";
                    continue;
                }

                Sql = Col.Lookup;
                ListDictionary Params = Db.ParseParameters(Sql);

                if (Params.Count > 0)
                    Sql = Regex.Replace(Sql, " where .*", " where 1=2", RegexOptions.IgnoreCase);

                try
                {
                    DT = Db.GetSchemaTable(Sql);
                }
                catch (Exception ex)
                {
                    throw new Exception($"{ex.Message} - {Db.CommandErrorInfo()}");
                }

            }
        }

        private void AddSearchTokenFilter(QueryCommandConfig query)
        {
            if (string.IsNullOrWhiteSpace(_DbNetGridConfiguration.SearchToken))
            {
                return;
            }

            var filter = new List<string>();

            foreach (DbColumn column in _DbNetGridConfiguration.Columns)
            {
                if (column.SimpleSearch)
                {
                    filter.Add($"{StripColumnRename(column.ColumnExpression)} like {Db.ParameterName("simplesearchtoken")}");
                }
            }

            if (filter.Any() == false)
            {
                return;
            }

            query.Sql += $" where ({string.Join(" or ", filter)})";
            query.Params["simplesearchtoken"] = $"%{_DbNetGridConfiguration.SearchToken}%";
        }

        private void AddDropDownFilter(QueryCommandConfig query)
        {
            if (string.IsNullOrWhiteSpace(_DbNetGridConfiguration.DropDownFilterValue))
            {
                return;
            }

            query.Sql += $"{(query.Sql.Contains(" where ") ? " and " : " where ")}({_DbNetGridConfiguration.DropDownFilterColumn} = {Db.ParameterName("DropDownFilterValue")})";
            query.Params["DropDownFilterValue"] = $"{_DbNetGridConfiguration.DropDownFilterValue}";
        }

        private void AddOrderBy(QueryCommandConfig query)
        {
            if (string.IsNullOrEmpty(_DbNetGridConfiguration.OrderByColumn))
            {
                _DbNetGridConfiguration.OrderByColumn = _DbNetGridConfiguration.Columns.First().ColumnName;
                _DbNetGridConfiguration.OrderBySequence = "asc";
            }
            query.Sql += $" order by {_DbNetGridConfiguration.OrderByColumn} {_DbNetGridConfiguration.OrderBySequence}";
        }

        private GridColumn GenerateColumn(DataRow row, GridColumn column)
        {
            GridColumn C;

            if (column == null)
            {
                C = new GridColumn(Convert.ToString(row["ColumnName"]));
            }
            else
            {
                C = column;
                C.ColumnName = Convert.ToString(row["ColumnName"]);
            }

            if (string.IsNullOrEmpty(C.Label))
            {
                C.Label = Regex.Replace(C.ColumnName, "(\\B[A-Z])", " $1");
            }

            C.BaseTableName = Convert.ToString(row["BaseTableName"]);

            GetBaseSchemaName(C, row);

            if (C.BaseTableName != string.Empty)
                C.ColumnExpression = $"{Db.QualifiedDbObjectName(C.BaseTableName)}.{C.ColumnExpression}";

            C.AddedByUser = false;

            return C;
        }

        private void GetBaseSchemaName(DbColumn C, DataRow Row)
        {
            if (C.BaseSchemaName == "")
                if (Row.Table.Columns.Contains("BaseSchemaName"))
                    if (Row["BaseSchemaName"] != System.DBNull.Value)
                        C.BaseSchemaName = Convert.ToString(Row["BaseSchemaName"]);
        }

        private string ColumnList()
        {
            var columns = "*";

            if (_DbNetGridConfiguration.Columns.Any())
            {
                columns = string.Join(",", _DbNetGridConfiguration.Columns.Select(c => c.ColumnExpression).ToArray());
            }
            return columns;
        }

        protected string StripColumnRename(string ColumnExpression)
        {
            string[] ColumnParts = ColumnExpression.Split(')');
            ColumnParts[ColumnParts.Length - 1] = Regex.Replace(ColumnParts[ColumnParts.Length - 1], " as .*", "", RegexOptions.IgnoreCase);
            ColumnParts[0] = Regex.Replace(ColumnParts[0], "unique |distinct ", "", RegexOptions.IgnoreCase);

            return String.Join(")", ColumnParts);
        }

        private void AddLookupData()
        {
            foreach (GridColumn gc in _DbNetGridConfiguration.Columns)
            {
                if (string.IsNullOrEmpty(gc.Lookup))
                {
                    continue;
                }
                QueryCommandConfig query = new QueryCommandConfig();
                query.Sql = gc.Lookup;
                Db.ExecuteQuery(query);

                while (Db.GetDataReader().Read())
                {
                    gc.LookupData[Db.ReaderString(0)] = Db.ReaderString(1);
                }
            }
        }
    }
}
