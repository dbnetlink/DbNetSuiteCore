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
using DbNetSuiteCore.Constants;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata.Ecma335;

namespace DbNetSuiteCore.Services
{
    internal class DbNetEdit : DbNetGridEdit
    {
        private Dictionary<string, object> _resp = new Dictionary<string, object>();

        public List<EditColumn> Columns { get; set; } = new List<EditColumn>();
        public Dictionary<string, object> Changes { get; set; }
        public long CurrentRow { get; set; } = 1;
        public bool IsEditDialog { get; set; } = false;
        public int LayoutColumns { get; set; } = 1;
        public long TotalRows { get; set; }
        public bool Browse => Columns.Any(c => c.Browse);
        public Guid? FormCacheKey { get; set; }
        public DateTime JavascriptDate { get; set; }
        public ToolbarPosition ToolbarPosition { get; set; } = ToolbarPosition.Bottom;
        public DbNetEdit(AspNetCoreServices services) : base(services)
        {
        }
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
        public new async Task<object> Process()
        {
            DbNetEditResponse response = new DbNetEditResponse();

            if (Action.ToLower() == RequestAction.SaveFiles)
            {
                await SaveFiles(response);
            }
            else
            {
                var request = await DeserialiseRequest<DbNetEditRequest>();
                Columns = request.Columns;
                await GridEditInitialise();

                switch (Action.ToLower())
                {
                    case RequestAction.Initialize:
                        await CreateForm(response);
                        break;
                    case RequestAction.SearchDialog:
                        await SearchDialog(response);
                        break;
                    case RequestAction.Lookup:
                        await LookupDialog(response);
                        break;
                    case RequestAction.GetRecord:
                        GetRecord(response);
                        break;
                    case RequestAction.UpdateRecord:
                        UpdateRecord(response);
                        break;
                    case RequestAction.InsertRecord:
                        InsertRecord(response);
                        break;
                    case RequestAction.DeleteRecord:
                        DeleteRecord(response);
                        break;
                    case RequestAction.Search:
                        SelectRecords(response);
                        break;
                    case RequestAction.DownloadColumnData:
                        return GetColumnData();
                    case RequestAction.UploadDialog:
                        await UploadDialog(response);
                        break;
                    case RequestAction.ConvertDate:
                        ConvertDate(response);
                        break;
                    case RequestAction.GetOptions:
                        await GetOptions(response);
                        break;
                    case RequestAction.ValidateRecord:
                        ValidateRecord(response);
                        break;
                }
            }

            Database.Close();

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(response, serializeOptions);
        }


        private async Task<string> Toolbar()
        {
            var viewModel = new ToolbarViewModel();
            ReflectionHelper.CopyProperties(this, viewModel);
            return await HttpContext.RenderToStringAsync("Views/DbNetEdit/Toolbar.cshtml", viewModel);
        }

        private async Task CreateForm(DbNetEditResponse response)
        {
            ConfigureEditColumns();

            response.Toolbar = await Toolbar();

            string sql = $"select {BuildSelectPart(QueryBuildModes.Normal, Columns)} from {FromPart} where 1=2";
            QueryCommandConfig query = new QueryCommandConfig(sql);
            DataTable dataTable = GetDataTable(query);

            var viewModel = new FormViewModel
            {
                DataColumns = dataTable.Columns,
                EditColumns = Columns,
                LookupTables = _lookupTables
            };

            ReflectionHelper.CopyProperties(this, viewModel);
            response.Form = await HttpContext.RenderToStringAsync($"Views/DbNetEdit/Form.cshtml", viewModel);

            if (OptimizeForLargeDataset)
            {
                SetTotalRows();
            }

            response.CurrentRow = 1;
            dataTable = GetCurrentRow(dataTable, OptimizeForLargeDataset);
            response.TotalRows = TotalRows;
            if (TotalRows > 0)
            {
                response.Record = CreateRecord(dataTable, DbColumns);
                response.PrimaryKey = SerialisePrimaryKey(dataTable.Rows[0]);
            }
            response.Columns = Columns;

            this.CheckForPrimaryKey(response);
        }

        private void SetTotalRows()
        {
            var query = BuildSql(QueryBuildModes.Count);

            using (Database)
            {
                Database.Open();
                TotalRows = Database.ExecuteScalar(query);
            }
        }

        private DataTable GetDataTable(QueryCommandConfig query)
        {
            DataTable dataTable = new DataTable();

            using (Database)
            {
                Database.Open();
                Database.ExecuteQuery(query);
                dataTable.Load(Database.Reader);
            }

            return dataTable;
        }

        private void SelectRecords(DbNetEditResponse response)
        {
            if (ValidateRequest(response, Columns) == false)
            {
                return;
            }

            CurrentRow = 1;

            ConfigureEditColumns();

            DataTable dataTable = GetCurrentRow();

            response.CurrentRow = CurrentRow;
            response.TotalRows = TotalRows;
            response.Columns = Columns;

            if (dataTable.Rows.Count > 0)
            {
                response.Record = CreateRecord(dataTable, Columns.Cast<DbColumn>().ToList());
                response.PrimaryKey = SerialisePrimaryKey(dataTable.Rows[0]);
            }
        }

        private DataTable GetCurrentRow(DataTable dataTable = null, bool rowsCounted = false)
        {
            if (dataTable == null)
            {
                dataTable = InitialiseDataTable();
            }

            using (Database)
            {
                Database.Open();
                QueryCommandConfig query = BuildSql();

                Database.Open();
                Database.ExecuteQuery(query);

                int counter = 0;
                object[] values = new object[0];

                while (Database.Reader.Read())
                {
                    counter++;
                    values = new object[Database.Reader.FieldCount];
                    Database.Reader.GetValues(values);

                    if (counter == CurrentRow)
                    {
                        dataTable.Rows.Add(values);
                        if (rowsCounted)
                        {
                            break;
                        }
                    }
                }

                if (rowsCounted == false)
                {
                    TotalRows = counter;

                    if (TotalRows < CurrentRow && TotalRows > 0)
                    {
                        CurrentRow = TotalRows;
                        dataTable.Rows.Add(values);
                    }
                }
            }

            return dataTable;
        }
        private void GetRecord(DbNetEditResponse response)
        {
            DataTable dataTable;

            if (ParentControlType.HasValue && ParentChildRelationship == Enums.ParentChildRelationship.OneToOne)
            {
                response.TotalRows = 1;
                response.CurrentRow = 1;
                QueryCommandConfig query = BuildSql();
                dataTable = GetDataTable(query);
            }
            else
            {
                ConfigureEditColumns();
                if (OptimizeForLargeDataset)
                {
                    SetTotalRows();
                }

                dataTable = GetCurrentRow(null, OptimizeForLargeDataset);
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

            foreach (EditColumn editColumn in Columns)
            {
                switch (editColumn.EditControlType)
                {
                    case Enums.DbNetEdit.EditControlType.Date:
                        editColumn.Format = "yyyy-MM-dd";
                        break;
                    case Enums.DbNetEdit.EditControlType.DateTime:
                        editColumn.Format = "yyyy-MM-ddTHH:mm:ss";
                        break;
                }
            }
        }
        protected override void AddColumn(DataRow row)
        {
            Columns.Add(InitialiseColumn(new EditColumn(), row) as EditColumn);
        }
        protected virtual QueryCommandConfig BuildSql(QueryBuildModes queryBuildMode = QueryBuildModes.Normal)
        {
            string selectPart = queryBuildMode == QueryBuildModes.Count ? "count(*)" : BuildSelectPart(QueryBuildModes.Normal, Columns);
            string sql = $"select {selectPart} from {FromPart}";
            ListDictionary parameters = new ListDictionary();
            List<string> filterPart = new List<string>();
            List<string> searchFilterPart = new List<string>();

            if (string.IsNullOrEmpty(PrimaryKey) == false && ParentControlType.HasValue && ParentChildRelationship == Enums.ParentChildRelationship.OneToOne)
            {
                filterPart.Add(PrimaryKeyFilter(parameters));
            }
            else
            {
                string foreignKeyFilter = ForeignKeyFilter(parameters);

                if (string.IsNullOrEmpty(foreignKeyFilter) == false)
                {
                    filterPart.Add(foreignKeyFilter);
                }

                if (string.IsNullOrEmpty(FixedFilterSql) == false)
                {
                    filterPart.Add(FixedFilterSql);
                    if (FixedFilterParams.Keys.Any())
                    {
                        foreach (string paramName in FixedFilterParams.Keys)
                        {
                            parameters.Add(ParamName(paramName), ConvertToDbParam(FixedFilterParams[paramName], null));
                        }
                    }
                }

                if (SearchParams.Any())
                {
                    foreach (SearchParameter searchParameter in SearchParams)
                    {
                        EditColumn editColumn = Columns[searchParameter.ColumnIndex];
                        searchFilterPart.Add($"{RefineSearchExpression(editColumn)} {FilterExpression(searchParameter, editColumn)}");
                    }

                    filterPart.Add($"({string.Join($" {SearchFilterJoin} ", searchFilterPart)})");
                    AddSearchDialogParameters(Columns.Cast<DbColumn>().ToList(), parameters);
                }

                if (String.IsNullOrEmpty(QuickSearchToken) == false)
                {
                    filterPart.Add($"({string.Join(" or ", QuickSearchFilter(Columns.Cast<DbColumn>().ToList()).ToArray())})");
                    parameters.Add(ParamName(ParamNames.QuickSearchToken, true), ConvertToDbParam($"%{QuickSearchToken}%", null));
                }
            }

            if (filterPart.Any())
            {
                sql += $" where {string.Join(" and ", filterPart)}";
            }

            sql += $" order by {Columns.FirstOrDefault(c => c.Browse)?.ColumnExpression ?? "1"}";

            return new QueryCommandConfig(sql, parameters);
        }

        private void ValidateRecord(DbNetEditResponse response)
        {
            ConfigureEditColumns();

            if (CheckForPrimaryKey(response) == false)
            {
                return;
            }
            ValidateUserInput(response);
        }

        private void UpdateRecord(DbNetEditResponse response)
        {
            ConfigureEditColumns();

            if (CheckForPrimaryKey(response) == false)
            {
                return;
            }
            List<string> updatePart = new List<string>();
            List<string> filterPart = new List<string>();
            CommandConfig updateCommand = new CommandConfig();
            ValidateUserInput(response);
            if (response.Error)
            {
                return;
            }

            if (IsReadOnly(response))
            {
                return;
            }

            UploadedFiles uploadedFiles = null;

            if (FormCacheKey.HasValue)
            {
                uploadedFiles = Cache.Get(FormCacheKey) as UploadedFiles;
            }

            foreach (string key in Changes.Keys)
            {
                DbColumn c = this.Columns.FirstOrDefault(c => c.IsMatch(key));

                updatePart.Add(Database.QualifiedDbObjectName(c.ColumnName) + " = " + Database.ParameterName(key));
                updateCommand.Params[Database.ParameterName(key)] = ConvertToDbParam(Changes[key], c);
            }

            if (uploadedFiles != null)
            {
                foreach (IFormFile file in uploadedFiles.Files)
                {
                    DbColumn c = this.Columns.FirstOrDefault(c => c.IsMatch(file.Name));

                    var bytes = uploadedFiles.FileBytes[file.Name];
                    updatePart.Add(Database.QualifiedDbObjectName(file.Name) + " = " + Database.ParameterName(file.Name));
                    updateCommand.Params[Database.ParameterName(file.Name)] = ConvertToDbParam(bytes, c);
                }
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
                response.Error = true;
                response.Message = $"Update failed ({ex.Message})";
            }

            GetRecord(response);
        }

        private void InsertRecord(DbNetEditResponse response)
        {
            ConfigureEditColumns();

            List<string> columnNames = new List<string>();
            List<string> paramNames = new List<string>();
            CommandConfig insertCommand = new CommandConfig();

            ValidateUserInput(response);

            if (response.Error)
            {
                return;
            }

            if (DbColumns.Any(c => c.PrimaryKey && c.AutoIncrement == false))
            {
                if (CheckPrimaryKeyIsUnique(response) == false)
                {
                    return;
                };
            }

            if (IsReadOnly(response))
            {
                return;
            }

            UploadedFiles uploadedFiles = null;

            if (FormCacheKey.HasValue)
            {
                uploadedFiles = Cache.Get(FormCacheKey) as UploadedFiles;
            }

            foreach (string key in Changes.Keys)
            {
                DbColumn c = this.Columns.FirstOrDefault(c => c.IsMatch(key));

                columnNames.Add(Database.QualifiedDbObjectName(c.ColumnName));
                paramNames.Add(Database.ParameterName(key));
                insertCommand.Params[Database.ParameterName(key)] = ConvertToDbParam(Changes[key], c);
            }

            if (uploadedFiles != null)
            {
                foreach (IFormFile file in uploadedFiles.Files)
                {
                    DbColumn c = this.Columns.FirstOrDefault(c => c.IsMatch(file.Name));

                    var bytes = uploadedFiles.FileBytes[file.Name];
                    columnNames.Add(Database.QualifiedDbObjectName(c.ColumnName));
                    paramNames.Add(Database.ParameterName(file.Name));
                    insertCommand.Params[Database.ParameterName(file.Name)] = ConvertToDbParam(bytes, c);
                }
            }

            DbColumn foreignKey = Columns.FirstOrDefault(c => c.ForeignKey);

            if (foreignKey != null)
            {
                columnNames.Add(Database.QualifiedDbObjectName(foreignKey.ColumnName));
                paramNames.Add(Database.ParameterName(foreignKey.ColumnName));
                insertCommand.Params[Database.ParameterName(foreignKey.ColumnName)] = ConvertToDbParam(foreignKey.ForeignKeyValue, foreignKey);
            }

            insertCommand.Sql = $"insert into {FromPart} ({string.Join(",", columnNames)}) values ({string.Join(",", paramNames)})";

            string autoIncrementColumnName = this.Columns.FirstOrDefault(c => c.AutoIncrement)?.ColumnName;

            try
            {
                using (Database)
                {
                    Database.Open();
                    response.PrimaryKey = Database.ExecuteInsert(insertCommand, autoIncrementColumnName).ToString();
                }

                response.Message = "Record added";
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Message = $"Insert failed ({ex.Message})";
            }
        }

        private void ValidateUserInput(DbNetEditResponse response)
        {
            foreach (string key in Changes.Keys)
            {
                EditColumn dbColumn = this.Columns.FirstOrDefault(c => c.IsMatch(key));

                if (dbColumn.Required && Changes[key].ToString() == string.Empty)
                {
                    response.ValidationMessage = new KeyValuePair<string, string>(dbColumn.ColumnName, $"{dbColumn.Label} is required.");
                    break;
                }

                bool isValid = ValidateUserValue(dbColumn, Changes[key].ToString());

                if (!isValid)
                {
                    response.ValidationMessage = new KeyValuePair<string, string>(dbColumn.ColumnName, $"Value for <b>{dbColumn.Label}</b> is not in the correct format.");
                    break;
                }
            }

            response.Error = response.ValidationMessage != null;
        }

        private bool CheckPrimaryKeyIsUnique(DbNetEditResponse response)
        {
            QueryCommandConfig query = new QueryCommandConfig();

            Dictionary<string, object> primaryKeyValues = new Dictionary<string, object>();

            foreach (string key in Changes.Keys)
            {
                DbColumn dbColumn = this.Columns.FirstOrDefault(c => c.IsMatch(key));

                if (dbColumn.PrimaryKey == false || dbColumn.AutoIncrement)
                {
                    continue;
                }

                primaryKeyValues[dbColumn.ColumnName] = Changes[key];
            }

            query.Sql = $"select 1 from {this.FromPart} where {this.PrimaryKeyFilter(query.Params, primaryKeyValues)}";

            using (Database)
            {
                Database.Open();
                Database.ExecuteQuery(query);
                response.Error = Database.Reader.Read();
            }

            if (response.Error)
            {
                response.ValidationMessage = new KeyValuePair<string, string>(primaryKeyValues.Keys.First(), "Supplied primary key value is not unique");
            }

            return !response.Error;
        }

        private async Task SaveFiles(DbNetEditResponse response)
        {
            var formCollection = await HttpContext.Request.ReadFormAsync();

            UploadedFiles uploadedFiles = new UploadedFiles();
            uploadedFiles.FormCollection = formCollection;

            if (FormCacheKey.HasValue)
            {
                try
                {
                    Cache.Remove(FormCacheKey.Value);
                }
                catch { }
            }

            response.FormCacheKey = Guid.NewGuid();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                    .SetPriority(CacheItemPriority.Normal);

            Cache.Set(response.FormCacheKey, uploadedFiles, cacheEntryOptions);
        }

        protected async Task UploadDialog(DbNetSuiteResponse response)
        {
            var uploadDialogViewModel = new UploadDialogViewModel();
            ReflectionHelper.CopyProperties(this, uploadDialogViewModel);
            response.Dialog = await HttpContext.RenderToStringAsync("Views/DbNetEdit/UploadDialog.cshtml", uploadDialogViewModel);
        }

        private void ConvertDate(DbNetEditResponse response)
        {
            DbColumn column = DbColumns.FirstOrDefault(c => c.IsMatch(this.ColumnName));

            string format = "d";
            if (column != null)
            {
                format = string.IsNullOrEmpty(column.Format) ? "d" : column.Format;
            }

            response.ConvertedDate = this.JavascriptDate.ToString(format);
        }

        private bool CheckForPrimaryKey(DbNetEditResponse response)
        {
            if (Columns.Any(c => c.PrimaryKey) == false)
            {
                response.Message = "A primary key has not been defined";
                return false;
            }

            return true;
        }
    }
}