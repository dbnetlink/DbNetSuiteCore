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
using DocumentFormat.OpenXml.Drawing;
using System.Collections.Specialized;
using System.Collections;
using System;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace DbNetSuiteCore.Services
{
    internal class DbNetEdit : DbNetGridEdit
    {
        private Dictionary<string, object> _resp = new Dictionary<string, object>();
        private string _foreignKeyColumn;
        private string _primaryKey;
        private bool _foreignKeySupplied => string.IsNullOrEmpty(ForeignKeyColumn) == false && ForeignKeyValue != null;
        public List<EditColumn> Columns { get; set; } = new List<EditColumn>();
        public int TotalRows { get; set; }
        public Dictionary<string,object> Changes { get; set; }
        public int CurrentRow { get; set; } = 1;
        public string PrimaryKey
        {
            get => EncodingHelper.Decode(_primaryKey);
            set => _primaryKey = value;
        }

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
                    await Form(response);
                    break;
                case RequestAction.GetRecord:
                    GetRecord(response);
                    break;
                case RequestAction.ApplyChanges:
                    ApplyChanges(response);
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

        private async Task Form(DbNetEditResponse response)
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

            var viewModel = new FormViewModel
            {
                EditData = dataTable
            };

            ReflectionHelper.CopyProperties(this, viewModel);
            viewModel.Columns = Columns;
            viewModel.LookupTables = _lookupTables;

            response.Form = await HttpContext.RenderToStringAsync($"Views/DbNetEdit/Form.cshtml", viewModel);

            if (dataTable.Rows.Count > 0)
            {
                response.Record = CreateRecord(dataTable);
                response.PrimaryKey = SerialisePrimaryKey(dataTable.Rows[0]);
            }
        }

        private DataTable GetDataTable()
        {
            DataTable dataTable = InitialiseDataTable(Columns);

            TotalRows = -1;
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

                if (TotalRows == -1)
                {
                    TotalRows = counter;
                }

                Database.Close();
            }

            return dataTable;
        }
        private void GetRecord(DbNetEditResponse response)
        {
            ConfigureEditColumns();
            DataTable dataTable = GetDataTable();
            response.CurrentRow = CurrentRow;
            response.TotalRows = TotalRows;
            response.Columns = Columns;
            response.Record = CreateRecord(dataTable);
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
            string sql = $"select {BuildSelectPart(QueryBuildModes.Normal,Columns)} from {FromPart}";
            QueryCommandConfig query = new QueryCommandConfig(sql);
            return query;
        }

        private string SerialisePrimaryKey(DataRow dataRow)
        {
            Dictionary<string,object> primaryKeyValues = new Dictionary<string,object>();
            foreach(EditColumn editColumn in Columns)
            {
                if (editColumn.PrimaryKey)
                {
                    primaryKeyValues.Add(editColumn.ColumnName, dataRow.ItemArray[editColumn.Index]);
                }
            }
            return JsonSerializer.Serialize(primaryKeyValues);
        }
        private Dictionary<string, object> DeserialisePrimaryKey()
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(PrimaryKey);
        }

        private void ApplyChanges(DbNetEditResponse response)
        {
            List<string> updatePart = new List<string>();
            List<string> filterPart = new List<string>();
            CommandConfig updateCommand = new CommandConfig();

            foreach (string key in Changes.Keys)
            {
                DbColumn C = this.Columns.FirstOrDefault(c => c.IsMatch(key));

                updatePart.Add(Database.QualifiedDbObjectName(C.ColumnName) + " = " + Database.ParameterName(key));
                updateCommand.Params[Database.ParameterName(key)] = ConvertToDbParam(Changes[key]);
            }

            var primaryKeyValues = DeserialisePrimaryKey();

            foreach (string key in primaryKeyValues.Keys)
            {
                DbColumn C = this.Columns.FirstOrDefault(c => c.IsMatch(key));

                filterPart.Add(Database.QualifiedDbObjectName(C.ColumnName) + " = " + Database.ParameterName(key));
                updateCommand.Params[Database.ParameterName(key)] = ConvertToDbParam(primaryKeyValues[key]);
            }

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
