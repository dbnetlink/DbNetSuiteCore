using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Resources;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DbNetSuiteCore.Utilities;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using Microsoft.AspNetCore.Mvc;
using static DbNetSuiteCore.Utilities.DbNetDataCore;
using DbNetSuiteCore.ViewModels.DbNetCombo;
using System.Linq;

namespace DbNetSuiteCore.Services
{
    internal class DbNetCombo : DbNetSuite
    {
        private Dictionary<string, object> _resp = new Dictionary<string, object>();

        private DbNetDataCore Database { get; set; }
        private DbNetComboRequest _dbNetComboRequest;
        private string _fromPart;
        private string _foreignKeyColumn;
        private string _valueColumn;
        private string _textColumn;
        private string _procedureName;
        private bool _foreignKeySupplied => string.IsNullOrEmpty(ForeignKeyColumn) == false && ForeignKeyValue != null;

        public DbNetCombo(AspNetCoreServices services) : base(services)
        {
        }

        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
        public string FromPart
        {
            get => EncodingHelper.Decode(_fromPart);
            set => _fromPart = value;
        }

        public bool AddEmptyOption { get; set; } = false;
        public bool AddFilter { get; set; } = false;
        public List<string> DataOnlyColumns { get; set; } = null;
        public bool Distinct { get; set; } = false;
        public string EmptyOptionText { get; set; } = String.Empty;
        public string FilterToken { get; set; } = String.Empty;
        public string ForeignKeyColumn
        {
            get => EncodingHelper.Decode(_foreignKeyColumn);
            set => _foreignKeyColumn = value;
        }
        public List<object> ForeignKeyValue { get; set; } = null;

        public string TextColumn
        {
            get => EncodingHelper.Decode(_textColumn);
            set => _textColumn = value;
        }
        public string ValueColumn
        {
            get => EncodingHelper.Decode(_valueColumn);
            set => _valueColumn = value;
        }
        public int Size { get; set; } = 1;
        public bool MultipleSelect { get; set; } = false;

        public string ProcedureName
        {
            get => EncodingHelper.Decode(_procedureName);
            set => _procedureName = value;
        }
        public Dictionary<string, object> ProcedureParams { get; set; } = new Dictionary<string, object>();
        public async Task<object> Process()
        {
            await DeserialiseRequest();
            Database = new DbNetDataCore(ConnectionString, Env, Configuration);
            DbNetComboResponse response = new DbNetComboResponse();

            ResourceManager = new ResourceManager("DbNetSuiteCore.Resources.Localization.default", typeof(DbNetGrid).Assembly);

            if (string.IsNullOrEmpty(this.Culture) == false)
            {
                CultureInfo ci = new CultureInfo(this.Culture);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }

            switch (Action.ToLower())
            {
                case "initialize":
                case "page":
                case "filter":
                    await Combo(response);
                    break;
            }

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(response, serializeOptions);
        }

        protected virtual QueryCommandConfig BuildSql()
        {
            List<string> columns = new List<string>() { ValueColumn };
            if (string.IsNullOrEmpty(TextColumn) == false)
            {
                columns.Add(TextColumn);
            }

            foreach(string c in DataOnlyColumns)
            {
                columns.Add(EncodingHelper.Decode(c));
            }

            string sql = $"select{(Distinct ? " distinct" : string.Empty)} {string.Join(",", columns)} from {FromPart}";

            var paramNames = new List<string>();
            if (_foreignKeySupplied)
            {
                if (ForeignKeyValue.Count == 0)
                {
                    sql += $" where {ForeignKeyColumn} is null";
                }
                else
                {
                    for (var i= 0; i < ForeignKeyValue.Count; i++)
                    {
                        paramNames.Add(Database.ParameterName($"foreignKey{i}"));
                    }
                   
                    sql += $" where {ForeignKeyColumn} in ({string.Join(",",paramNames)})";
                }
            }

            sql += $" order by {(string.IsNullOrEmpty(TextColumn) ? 1 : 2)}";

            QueryCommandConfig query = new QueryCommandConfig(sql);

            if (_foreignKeySupplied)
            {
                if (ForeignKeyValue.Count > 0)
                {
                    for (var i = 0; i < ForeignKeyValue.Count; i++)
                    {
                        query.Params[paramNames[i]] = ForeignKeyValue[i];
                    }
                }
            }
            return query;
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

        private async Task DeserialiseRequest()
        {
            _dbNetComboRequest = await GetRequest<DbNetComboRequest>();
            ReflectionHelper.CopyProperties(_dbNetComboRequest, this);
        }

        private async Task Combo(DbNetComboResponse response)
        {
            DataTable dataTable = new DataTable();

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query;

                if (string.IsNullOrEmpty(ProcedureName)){
                    query = BuildSql();
                }
                else
                {
                    query = ProcedureCommandConfig();
                }

                Database.Open();
                Database.ExecuteQuery(query);
                dataTable.Load(Database.Reader);
                Database.Close();
            }

            response.TotalRows = dataTable.Rows.Count;

            DataView dataView = new DataView(dataTable);

            if (Action == "filter" && string.IsNullOrEmpty(FilterToken) == false)
            {
                FilterToken = FilterToken.Replace("%", "*");
                if (FilterToken.Contains("*") == false)
                {
                    FilterToken = $"*{FilterToken}*";
                }

                var textIndex = dataTable.Columns.Count > 1 ? 1 : 0;
                dataView.RowFilter = $"[{dataTable.Columns[textIndex].ColumnName}] LIKE '{FilterToken}'";
            }

            var viewModel = new ComboViewModel
            {
                DataView = dataView
            };

            ReflectionHelper.CopyProperties(this, viewModel);

            if (Action != "filter")
            {
                response.Select = await HttpContext.RenderToStringAsync($"Views/DbNetCombo/Combo.cshtml", viewModel);
            }

            response.Options = await HttpContext.RenderToStringAsync($"Views/DbNetCombo/Options.cshtml", viewModel);
        }
    }
}