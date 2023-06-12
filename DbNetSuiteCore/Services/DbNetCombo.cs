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
using DbNetSuiteCore.ViewModels.DbNetCombo;

namespace DbNetSuiteCore.Services
{
    internal class DbNetCombo : DbNetSuite
    {
        private Dictionary<string, object> _resp = new Dictionary<string, object>();
        const string NullValueToken = "@@null@@";

        private DbNetDataCore Database { get; set; }
        private DbNetComboRequest _dbNetComboRequest;
        private string _sql;


        public DbNetCombo(AspNetCoreServices services) : base(services)
        {
        }

        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
        public string Sql
        {
            get => EncodingHelper.Decode(_sql);
            set => _sql = value;
        }

        public bool AddEmptyOption { get; set; } = false;
        public string EmptyOptionText { get; set; } = String.Empty;
      
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
                    await Combo(response);
                    break;
                case "page":
                    await Combo(response);
                    break;
            }

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return System.Text.Json.JsonSerializer.Serialize(response, serializeOptions);
        }

        protected virtual QueryCommandConfig BuildSql()
        {
            QueryCommandConfig query = new QueryCommandConfig(Sql);
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

            var viewModel = new ComboViewModel
            {
                DataTable = dataTable
            };

            response.Data = await HttpContext.RenderToStringAsync($"Views/DbNetCombo/Combo.cshtml", viewModel);
        }
    }
}