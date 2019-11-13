using DbNetSuiteCore.Data;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Models.Configuration;
using DbNetSuiteCore.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DbNetSuiteCore.Middleware
{
    public class DbNetGridHandler
    {
        protected DbNetData Db = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private DbNetGridConfiguration _DbNetGridConfiguration;
        private readonly IConfiguration _configuration;
        public const string Extension = ".dbnetgrid";
        public IViewRenderService _viewRenderService;

        public DbNetGridHandler(RequestDelegate next,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment hostingEnvironment,
            IViewRenderService viewRenderService
            )
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _viewRenderService = viewRenderService;
        }

        public async Task Invoke(HttpContext context)
        {
            _DbNetGridConfiguration = SerialisationHelper.DeserialiseJson<DbNetGridConfiguration>(context.Request.Body);
            Db = new DbNetData(_DbNetGridConfiguration.ConnectionString, _DbNetGridConfiguration.DataProvider, _httpContextAccessor, _hostingEnvironment, _configuration);
            Db.Open();

            var handler = (string)context.Request.Query["handler"] ?? string.Empty;

            switch (handler.ToLower())
            {
                case "init":
                    ConfigureColumns();
                    _DbNetGridConfiguration.Html["toolbar"] = await _viewRenderService.RenderToStringAsync("DbNetGrid", _DbNetGridConfiguration);
                    _DbNetGridConfiguration.Html["page"] = await _viewRenderService.RenderToStringAsync("DbNetGrid_Page", GetPage());
                    break;
                case "page":
                    GetPage();
                    _DbNetGridConfiguration.Html["page"] = await _viewRenderService.RenderToStringAsync("DbNetGrid_Page", GetPage());
                    break;
            }

            Db.Close();
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(SerialisationHelper.SerialiseToJson(_DbNetGridConfiguration));
        }

        private DataTable GetPage()
        {
            string Sql = $"select {ColumnList()} from {_DbNetGridConfiguration.TableName} where 1=2";
            DataTable dt = Db.GetDataTable(Sql);

            QueryCommandConfig query = new QueryCommandConfig();
            query.Sql = $"select {ColumnList()} from {_DbNetGridConfiguration.TableName}";
            AddFilter(query);

            int firstRecord = (_DbNetGridConfiguration.PageSize * (_DbNetGridConfiguration.CurrentPage - 1)) + 1;
            int lastRecord = (firstRecord + (_DbNetGridConfiguration.PageSize - 1));
            int recordCounter = 0;
            Db.ExecuteQuery(query);

            while (Db.Reader.Read())
            {
                recordCounter++;
                if (recordCounter >= firstRecord && recordCounter <= lastRecord)
                {
                    DataRow row = dt.NewRow();

                    for (var i = 0; i < Db.Reader.FieldCount; i++)
                    {
                        row[i] = Db.ReaderValue(i);
                    }

                    dt.Rows.Add(row);
                }
            }

            _DbNetGridConfiguration.TotalRows = recordCounter;

            return dt;
        }

        private void ConfigureColumns()
        {
            Dictionary<string, bool> BaseTables = new Dictionary<string, bool>();

            List<DbColumn> userDefinedColumns = _DbNetGridConfiguration.Columns;
            string Sql = $"select {ColumnList()} from {_DbNetGridConfiguration.TableName} where 1=2";
            _DbNetGridConfiguration.Columns.Clear();

            DataTable DT = Db.GetSchemaTable(Sql);

            for (var idx = 0; idx < DT.Rows.Count; idx++ )
            {
                DataRow row = DT.Rows[idx];

                if (row.Table.Columns.Contains("IsHidden"))
                    if (Convert.ToBoolean(row["IsHidden"]))
                        continue;
                _DbNetGridConfiguration.Columns.Add(GenerateColumn(row));

                if (userDefinedColumns.Any())
                {
                    _DbNetGridConfiguration.Columns[idx].ColumnExpression = userDefinedColumns[idx].ColumnExpression;
                }
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

        private void AddFilter(QueryCommandConfig query)
        {
            if (string.IsNullOrWhiteSpace(_DbNetGridConfiguration.SearchToken))
            {
                return;
            }

            var filter = new List<string>();

            foreach(DbColumn column in _DbNetGridConfiguration.Columns)
            {
                if (column.SimpleSearch)
                {
                    filter.Add(StripColumnRename(column.ColumnExpression) + " like " + Db.ParameterName("simplesearchtoken"));
                }
            }

            if (filter.Any() == false)
            {
                return;
            }

            query.Sql += $" where {string.Join(" or ", filter)}";
            query.Params["simplesearchtoken"] = $"%{_DbNetGridConfiguration.SearchToken}%";
        }

        private DbColumn GenerateColumn(DataRow row)
        {
            DbColumn C = new DbColumn();

            C.ColumnExpression = Db.QualifiedDbObjectName(Convert.ToString(row["ColumnName"]));
            C.ColumnName = Convert.ToString(row["ColumnName"]);
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
    }
}
