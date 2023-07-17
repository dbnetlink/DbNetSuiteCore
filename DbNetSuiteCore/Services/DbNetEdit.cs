using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using Microsoft.AspNetCore.Mvc;
using DbNetSuiteCore.Models.DbNetEdit;
using DbNetSuiteCore.Constants.DbNetEdit;
using DbNetSuiteCore.Enums;
using static DbNetSuiteCore.Utilities.DbNetDataCore;
using DbNetSuiteCore.ViewModels.DbNetEdit;
using System.Collections.Specialized;
using System;
using System.Linq;
using DbNetSuiteCore.Attributes;
using DbNetSuiteCore.Constants;
using DocumentFormat.OpenXml.Office.Word;
using System.Diagnostics.Metrics;

namespace DbNetSuiteCore.Services
{
    internal class DbNetEdit : DbNetGridEdit
    {
        private Dictionary<string, object> _resp = new Dictionary<string, object>();
        private string _foreignKeyColumn;
        private bool _foreignKeySupplied => string.IsNullOrEmpty(ForeignKeyColumn) == false && ForeignKeyValue != null;
        public List<EditColumn> Columns { get; set; } = new List<EditColumn>();
        public Dictionary<string, object> Changes { get; set; }
        public int CurrentRow { get; set; } = 1;
        public bool IsEditDialog { get; set; } = false;
        public int LayoutColumns { get; set; } = 1;
        public int TotalRows { get; set; }

        public DbNetEdit(AspNetCoreServices services) : base(services)
        {
        }
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
        public string ForeignKeyColumn
        {
            get => EncodingHelper.Decode(_foreignKeyColumn);
            set => _foreignKeyColumn = value;
        }
        public List<object> ForeignKeyValue { get; set; } = null;
        public async Task<object> Process()
        {
            var request = await DeserialiseRequest<DbNetEditRequest>();
            Columns = request.Columns;

            Initialise();

            DbNetEditResponse response = new DbNetEditResponse();

            switch (Action.ToLower())
            {
                case RequestAction.Initialize:
                    response.Toolbar = await Toolbar();
                    await CreateForm(response);
                    break;
                case RequestAction.SearchDialog:
                    await SearchDialog(response, Columns.Cast<DbColumn>().ToList());
                    break;
                case RequestAction.Lookup:
                    await LookupDialog(response, Columns.Cast<DbColumn>().ToList());
                    break;
                case RequestAction.GetRecord:
                    GetRecord(response);
                    break;
                case RequestAction.ApplyChanges:
                    ApplyChanges(response);
                    break;
                case RequestAction.Search:
                    SelectRecords(response);
                    break;
            }

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(response, serializeOptions);
        }


        private async Task<string> Toolbar()
        {
            var viewModel = new ViewModels.DbNetEdit.ToolbarViewModel();
            ReflectionHelper.CopyProperties(this, viewModel);
            var contents = await HttpContext.RenderToStringAsync("Views/DbNetEdit/Toolbar.cshtml", viewModel);
            return contents;
        }

        private async Task CreateForm(DbNetEditResponse response)
        {
            ConfigureEditColumns();

            string sql = $"select {BuildSelectPart(QueryBuildModes.Normal, Columns)} from {FromPart} where 1=2";
            QueryCommandConfig query = new QueryCommandConfig(sql);
            DataTable dataTable = LoadDataTable(query);

            var viewModel = new FormViewModel
            {
                DataColumns = dataTable.Columns,
                EditColumns = Columns,
                LookupTables = _lookupTables
            };

            ReflectionHelper.CopyProperties(this, viewModel);
            response.Form = await HttpContext.RenderToStringAsync($"Views/DbNetEdit/Form.cshtml", viewModel);

            if (IsEditDialog == false)
            {
                response.CurrentRow = 1;
                query = BuildSql();
                dataTable = LoadDataTable(query);
                response.TotalRows = dataTable.Rows.Count;
                response.Record = CreateRecord(dataTable, Columns.Cast<DbColumn>().ToList());
            }
            response.Columns = Columns;
        }
        private DataTable LoadDataTable(QueryCommandConfig query)
        {
            var dataTable = new DataTable();
            using (Database)
            {
                Database.Open();
                Database.ExecuteQuery(query);
                dataTable.Load(Database.Reader);
                Database.Close();
            }

            return dataTable;
        }

        private void SelectRecords(DbNetEditResponse response)
        {
            if (ValidateRequest(response, Columns) == false)
            {
                return;
            }

            ConfigureEditColumns();

            DataTable dataTable = GetDataTable();

            response.CurrentRow = CurrentRow;
            response.TotalRows = TotalRows;
            response.Columns = Columns;

            if (dataTable.Rows.Count > 0)
            {
                response.Record = CreateRecord(dataTable, Columns.Cast<DbColumn>().ToList());
                response.PrimaryKey = SerialisePrimaryKey(dataTable.Rows[0]);
            }
        }

        private DataTable GetDataTable()
        {
            DataTable dataTable = InitialiseDataTable(Columns);

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query;

                query = BuildSql();

                Database.Open();
                Database.ExecuteQuery(query);

                int counter = 0;

                while (Database.Reader.Read())
                {
                    counter++;

                    if (counter == CurrentRow)
                    {
                        object[] values = new object[Database.Reader.FieldCount];
                        Database.Reader.GetValues(values);
                        dataTable.Rows.Add(values);
                    }
                }
                Database.Close();
                TotalRows = counter;
            }

            return dataTable;
        }
        private void GetRecord(DbNetEditResponse response)
        {
            DataTable dataTable;

            if (IsEditDialog)
            {
                response.TotalRows = 1;
                response.CurrentRow = 1;
                QueryCommandConfig query = BuildSql();
                dataTable = LoadDataTable(query);
            }
            else
            {
                ConfigureEditColumns();
                dataTable = GetDataTable();
                response.CurrentRow = CurrentRow;
                response.TotalRows = TotalRows;
                response.Columns = Columns;
            }
            response.Record = CreateRecord(dataTable, Columns.Cast<DbColumn>().ToList());
            response.PrimaryKey = SerialisePrimaryKey(dataTable.Rows[0]);
        }
        private void ConfigureEditColumns()
        {
            Columns = ConfigureColumns(Columns);
        }
        protected override void AddColumn(DataRow row)
        {
            Columns.Add(InitialiseColumn(new EditColumn(), row) as EditColumn);
        }
        protected virtual QueryCommandConfig BuildSql()
        {
            string sql = $"select {BuildSelectPart(QueryBuildModes.Normal, Columns)} from {FromPart}";
            ListDictionary parameters = new ListDictionary();
            List<string> filterPart = new List<string>();

            if (string.IsNullOrEmpty(PrimaryKey) == false)
            {
                filterPart.Add(PrimaryKeyFilter(parameters));
            }
            else
            {
                if (SearchParams.Any())
                {
                    List<string> searchFilterPart = new List<string>();
                    foreach (SearchParameter searchParameter in SearchParams)
                    {
                        EditColumn editColumn = Columns[searchParameter.ColumnIndex];
                        searchFilterPart.Add($"{RefineSearchExpression(editColumn)} {FilterExpression(searchParameter, editColumn)}");
                    }

                    filterPart.Add($"{string.Join($" {SearchFilterJoin} ", searchFilterPart)}");
                    AddSearchDialogParameters(Columns.Cast<DbColumn>().ToList(), parameters);
                }

                if (String.IsNullOrEmpty(QuickSearchToken) == false)
                {
                    filterPart.Add($"({string.Join(" or ", QuickSearchFilter(Columns.Cast<DbColumn>().ToList()).ToArray())})");
                    parameters.Add(ParamName(ParamNames.QuickSearchToken, true), ConvertToDbParam($"%{QuickSearchToken}%"));
                }
            }

            if (filterPart.Any())
            {
                sql += $" where {string.Join(" or ", filterPart)}";
            }

            return new QueryCommandConfig(sql, parameters);
        }

        private void ApplyChanges(DbNetEditResponse response)
        {
            List<string> updatePart = new List<string>();
            List<string> filterPart = new List<string>();
            CommandConfig updateCommand = new CommandConfig();

            object convertedValue = new object();
            foreach (string key in Changes.Keys)
            {
                DbColumn dbColumn = this.Columns.FirstOrDefault(c => c.IsMatch(key));

                if (dbColumn.Required && Changes[key].ToString() == string.Empty)
                {
                    response.ValidationMessage = new KeyValuePair<string, string>(dbColumn.ColumnName, $"{dbColumn.Label} is required.");
                    break;
                }
               
                bool isValid = ConvertUserValue(dbColumn.DataType, Changes[key].ToString(), ref convertedValue);

                if (!isValid)
                {
                    response.ValidationMessage = new KeyValuePair<string, string>(dbColumn.ColumnName, $"Value for {dbColumn.Label} is not in the correct format.");
                     break;
                }
                updateCommand.Params[Database.ParameterName(key)] = ConvertToDbParam(Changes[key]);
            }

            if (response.ValidationMessage != null)
            {
                response.Error = true;
                return;
            }

            foreach (string key in Changes.Keys)
            {
                DbColumn C = this.Columns.FirstOrDefault(c => c.IsMatch(key));

                updatePart.Add(Database.QualifiedDbObjectName(C.ColumnName) + " = " + Database.ParameterName(key));
                updateCommand.Params[Database.ParameterName(key)] = ConvertToDbParam(Changes[key]);
            }

            filterPart.Add(PrimaryKeyFilter(updateCommand.Params));
            updateCommand.Sql = $"update {FromPart} set {string.Join(", ", updatePart)} where {string.Join(" and ", filterPart)}";

            try
            {
                using (Database)
                {
                    Database.Open();
                    Database.ExecuteNonQuery(updateCommand);
                }

                response.Message = "Record updated";
            }
            catch (Exception ex)
            {
                response.Message = $"An error has occurred ({ex.Message})";
            }

            GetRecord(response);
        }
    }
}
