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
        public string EmptyOptionText { get; set; } = String.Empty;
        public string FilterToken { get; set; } = String.Empty;
        public string ForeignKeyColumn
        {
            get => EncodingHelper.Decode(_foreignKeyColumn);
            set => _foreignKeyColumn = value;
        }
        public object ForeignKeyValue { get; set; } = null;

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

            string sql = $"select {string.Join(",", columns)} from {FromPart}";

            var fkParamName = Database.ParameterName("foreignKey");

            if (_foreignKeySupplied)
            {
                sql += $" where {ForeignKeyColumn} = {fkParamName}";
            }

            sql += $" order by {columns.Count}";

            QueryCommandConfig query = new QueryCommandConfig(sql);

            if (_foreignKeySupplied)
            {
                query.Params[fkParamName] = ForeignKeyValue;
            }
            return query;
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

                query = BuildSql();

                Database.Open();
                Database.ExecuteQuery(query);
                dataTable.Load(Database.Reader);
                Database.Close();
            }

            response.TotalRows = dataTable.Rows.Count;

            DataView dataView = new DataView(dataTable);

            if (Action == "filter" && string.IsNullOrEmpty(FilterToken) == false)
            {
                FilterToken = FilterToken.Replace("%","*");
                if (FilterToken.Contains("*") == false)
                {
                    FilterToken = $"*{FilterToken}*";
                }

                var textIndex = dataTable.Columns.Count > 1 ? 1 : 0;
                dataView.RowFilter = $"[{dataTable.Columns[textIndex].ColumnName}] LIKE '{FilterToken}'";
            }

            var viewModel = new ComboViewModel
            {
                DataView = dataView,
                AddFilter = this.AddFilter
            };

            if (Action != "filter")
            {
                response.Select = await HttpContext.RenderToStringAsync($"Views/DbNetCombo/Combo.cshtml", viewModel);
            }

            response.Options = await HttpContext.RenderToStringAsync($"Views/DbNetCombo/Options.cshtml", viewModel);
        }
    }
}