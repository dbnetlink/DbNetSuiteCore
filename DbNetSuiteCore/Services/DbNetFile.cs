using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Constants.DbNetFile;
using DbNetSuiteCore.Models.DbNetFile;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc;
using DbNetSuiteCore.ViewModels.DbNetFile;
using DbNetSuiteCore.Enums.DbNetFile;
using System.IO;
using System.Linq;
using DbNetSuiteCore.Enums;
using Microsoft.AspNetCore.Http;
using DbNetSuiteCore.Attributes;
using System.Globalization;
using System.Text.Json.Serialization;
using DbNetSuiteCore.Constants;
using DbNetSuiteCore.Utilities;

namespace DbNetSuiteCore.Services
{
    internal class DbNetFile : DbNetSuite
    {
        private Dictionary<string, object> _resp = new Dictionary<string, object>();

        private string _folder;
        private string _fileName;
        private string _rootFolder;

        private readonly IFileProvider _fileProvider;
        public List<FileColumn> Columns { get; set; } = new List<FileColumn>();

        public DbNetFile(AspNetCoreServices services) : base(services)
        {
            _fileProvider = services.webHostEnvironment.WebRootFileProvider;
        }

        public string Folder
        {
            get => EncodingHelper.Decode(_folder);
            set => _folder = value;
        }

        public string FileName
        {
            get => EncodingHelper.Decode(_fileName);
            set => _fileName = value;
        }
        public string RootFolder
        {
            get => EncodingHelper.Decode(_rootFolder);
            set => _rootFolder = value;
        }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; } = 20;
        public int TotalRows { get; set; }
        public int TotalPages { get; set; }
        public bool QuickSearch { get; set; }
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; }
        public bool Search { get; set; }
        public bool Navigation { get; set; }
        public bool Export { get; set; }
        public bool Copy { get; set; }
        public bool Upload { get; set; }
        public string Caption { get; set; }
        public bool Nested { get; set; }
        public string QuickSearchToken { get; set; }
        public string OrderBy { get; set; }
        public OrderByDirection? OrderByDirection { get; set; }
        public string SearchFilterJoin { get; set; } = "and";
        public List<SearchParameter> SearchParams { get; set; } = new List<SearchParameter>();

        public new async Task<object> Process()
        {
            await DeserialiseRequest<DbNetFileRequest>();
            Initialise();

            DbNetFileResponse response = new DbNetFileResponse();

            ConfigureColumns();

            switch (Action.ToLower())
            {
                case RequestAction.Initialise:
                    await Page(response, true);
                    break;
                case RequestAction.Page:
                    await Page(response);
                    break;
                case RequestAction.DownloadFile:
                    return DownloadBinaryFile();
                case RequestAction.SearchDialog:
                    await SearchDialog(response);
                    break;
            }

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =    {
                    new JsonStringEnumConverter()
                }
            };
            return JsonSerializer.Serialize(response, serializeOptions);
        }

        private async Task Page(DbNetFileResponse response, bool inititialise = false)
        {
            if (ValidateRequest(response) == false)
            {
                return;
            }

            if (inititialise)
            {
                response.Toolbar = await Toolbar();
            }
            SqlDataTable sqlDataTable = new SqlDataTable();
            DataTable dataTable;

            using (sqlDataTable)
            {
                IDirectoryContents folderContents = _fileProvider.GetDirectoryContents(Folder);
                Dictionary<string,object> values = new Dictionary<string, object>();

                foreach (IFileInfo fileInfo in folderContents)
                {
                    FileInformation fileInformation = new FileInformation(fileInfo);
                    await sqlDataTable.AddRow(fileInformation);
                }

                string filter = string.Empty;
                Dictionary<string,object> filterParameters = new Dictionary<string,object>();

                if (SearchParams.Any())
                {
                    filter = string.Join($" {SearchFilterJoin} ", SearchDialogFilter());
                    filterParameters = SearchDialogParameters();
                }

                dataTable = await sqlDataTable.Query(filter, filterParameters);
            }

            DataView dataView = new DataView(dataTable);
            if (string.IsNullOrEmpty(OrderBy))
            {
                OrderBy = FileInfoProperties.Name.ToString();
            }
            if (OrderByDirection.HasValue == false)
            {
                OrderByDirection = Enums.OrderByDirection.asc;
            }

            dataView.Sort = $"IsDirectory DESC, {OrderBy} {OrderByDirection}";

            FileColumn orderByColumn = Columns.FirstOrDefault(c => c.Type.ToString() == OrderBy) ?? Columns.First();
            orderByColumn.OrderBy = OrderByDirection;

            if (string.IsNullOrEmpty(QuickSearchToken) == false)
            {
                QuickSearchToken = QuickSearchToken.Replace("%", "*");
                if (QuickSearchToken.Contains("*") == false)
                {
                    QuickSearchToken = $"*{QuickSearchToken}*";
                }
                dataView.RowFilter = $"{FileInfoProperties.Name} LIKE '{QuickSearchToken}'";
            }

            TotalRows = dataView.Count;
            if (PageSize <= 0)
            {
                PageSize = TotalRows;
                TotalPages = 1;
                CurrentPage = 1;
            }
            else
            {
                TotalPages = (int)Math.Ceiling((double)TotalRows / (double)Math.Abs(PageSize));
            }

            response.CurrentPage = CurrentPage;
            response.TotalPages = TotalPages;
            response.TotalRows = TotalRows;

            var viewModel = new FileViewModel
            {
                DataView = dataView,
                Columns = Columns,
                RootFolder = RootFolder,
                FirstRow = (CurrentPage - 1) * PageSize,
                LastRow = CurrentPage * PageSize
            };

            ReflectionHelper.CopyProperties(this, viewModel);
            response.Html = await HttpContext.RenderToStringAsync($"Views/DbNetFile/Page.cshtml", viewModel);
        }

        private List<string> SearchDialogFilter()
        {
            List<string> searchFilterPart = new List<string>();
            foreach (SearchParameter searchParameter in SearchParams)
            {
                FileColumn fileColumn = Columns.FirstOrDefault(c => c.Type == searchParameter.ColumnType);
                searchFilterPart.Add($"{fileColumn.Type} {FilterExpression(searchParameter, fileColumn)}");
            }

            return searchFilterPart;
        }

        protected string FilterExpression(SearchParameter searchParameter, FileColumn fileColumn)
        {
            string template = searchParameter.SearchOperator.GetAttribute<FilterExpressionAttribute>().Expression;
            string param1 = ParamName(fileColumn, ParamNames.SearchFilter1);
            string param2 = ParamName(fileColumn, ParamNames.SearchFilter2);
            return template.Replace("{0}", param1).Replace("{1}", param2);
        }

        protected string ParamName(FileColumn fileColumn, string suffix = "", bool parameterValue = false)
        {
            return Database.ParameterName($"{fileColumn.Type}{suffix}");
        }

        protected Dictionary<string, object> SearchDialogParameters()
        {
            Dictionary<string,object> parameters  = new Dictionary<string,object>();
            foreach (SearchParameter searchParameter in SearchParams)
            {
                FileColumn fileColumn = Columns.FirstOrDefault(c => c.Type == searchParameter.ColumnType);

                string expression = searchParameter.SearchOperator.GetAttribute<FilterExpressionAttribute>()?.Expression ?? "{0}";

                if (expression.Contains("{0}"))
                {
                    AddSearchFilterParams(parameters, fileColumn, searchParameter, ParamNames.SearchFilter1, searchParameter.Value1);
                }
                if (expression.Contains("{1}"))
                {
                    AddSearchFilterParams(parameters, fileColumn, searchParameter, ParamNames.SearchFilter2, searchParameter.Value2);
                }
            }

            return parameters;
        }

        protected void AddSearchFilterParams(Dictionary<string, object> parameters, FileColumn fileColumn, SearchParameter searchParameter, string prefix, string value)
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
                        parameters.Add(ParamName(fileColumn, $"{prefix}{i}", true), ConvertToType(template.Replace("{0}", values[i]), fileColumn));
                    }
                    break;
                case SearchOperator.True:
                case SearchOperator.False:
                    parameters.Add(ParamName(fileColumn, prefix, true), ConvertToType(template.Replace("{0}", searchParameter.SearchOperator.ToString().ToLower()), fileColumn));
                    break;
                default:
                    parameters.Add(ParamName(fileColumn, prefix, true), ConvertToType(template.Replace("{0}", value), fileColumn));
                    break;
            }
        }

        private async Task<string> Toolbar()
        {
            var viewModel = new ToolbarViewModel();
            ReflectionHelper.CopyProperties(this, viewModel);
            var contents = await HttpContext.RenderToStringAsync("Views/DbNetFile/Toolbar.cshtml", viewModel);
            return contents;
        }

        protected byte[] DownloadBinaryFile()
        {
            string url = $"{Folder}/{FileName}";
            IFileInfo fileInfo = _fileProvider.GetFileInfo(url);
            byte[] fileBytes = File.ReadAllBytes(fileInfo.PhysicalPath);
            HttpContext.Response.ContentType = GetMimeTypeForFileExtension($".{FileName.Split(".").Last()}");
            return fileBytes;
        }

        private DataTable CreateDataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(FileInfoProperties.Name.ToString(), typeof(string));
            dataTable.Columns.Add(FileInfoProperties.IsDirectory.ToString(), typeof(bool));
            dataTable.Columns.Add(FileInfoProperties.Exists.ToString(), typeof(bool));
            dataTable.Columns.Add(FileInfoProperties.LastModified.ToString(), typeof(DateTimeOffset));
            dataTable.Columns.Add(FileInfoProperties.Length.ToString(), typeof(long));
            dataTable.Columns.Add(FileInfoProperties.Created.ToString(), typeof(DateTime));
            dataTable.Columns.Add(FileInfoProperties.LastAccessed.ToString(), typeof(DateTime));
            dataTable.Columns.Add(FileInfoProperties.Extension.ToString(), typeof(string));

            return dataTable;
        }
        private void ConfigureColumns()
        {
            if (Columns.Any() == false)
            {
                Columns = new List<FileColumn>()
                {
                    new FileColumn(FileInfoProperties.Name),
                    new FileColumn(FileInfoProperties.Created),
                    new FileColumn(FileInfoProperties.LastModified),
                    new FileColumn(FileInfoProperties.LastAccessed),
                    new FileColumn(FileInfoProperties.Length)
                };
            }

            if (Columns.Any(c => c.Type == FileInfoProperties.Name) == false)
            {
                Columns.Insert(0, new FileColumn(FileInfoProperties.Name));
            }

            foreach (var column in Columns)
            {
                if (string.IsNullOrEmpty(column.Label))
                {
                    if (column.Type == FileInfoProperties.Length)
                    {
                        column.Label = "Size";
                    }
                    else
                    {
                        column.Label = GenerateLabel(column.Type.ToString());
                    }
                }
            }
        }

        private bool ValidateRequest(DbNetFileResponse response)
        {
            response.Message = String.Empty;

            if (SearchParams.Any())
            {
                foreach (SearchParameter searchParameter in SearchParams)
                {
                    FileColumn fileColumn = Columns.First(c => c.Type == searchParameter.ColumnType);

                    string expression = searchParameter.SearchOperator.GetAttribute<FilterExpressionAttribute>()?.Expression ?? "{0}";

                    if (expression.Contains("{0}"))
                    {
                        searchParameter.Value1Valid = ValidateUserValue(fileColumn, searchParameter.Value1);
                    }
                    if (expression.Contains("{1}"))
                    {
                        searchParameter.Value2Valid = ValidateUserValue(fileColumn, searchParameter.Value2);
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

        private bool ValidateUserValue(FileColumn fileColumn, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            return ConvertToType(value, fileColumn) == null ? false : true;
        }

        private object ConvertToType(object value, FileColumn column)
        {
            string dataType = nameof(String);

            switch (column.Type)
            {
                case FileInfoProperties.LastAccessed:
                case FileInfoProperties.LastModified:
                case FileInfoProperties.Created:
                    dataType = nameof(DateTime);
                    break;
                case FileInfoProperties.Length:
                    dataType = nameof(Decimal);
                    break;
            }

            object typedValue = string.Empty;
            try
            {
                switch (dataType)
                {
                    case nameof(DateTime):
                        if (string.IsNullOrEmpty(column.Format))
                        {
                            typedValue = Convert.ChangeType(value, Type.GetType($"System.{nameof(DateTime)}"));
                        }
                        else
                        {
                            try
                            {
                                typedValue = DateTime.ParseExact(value.ToString(), column.Format, CultureInfo.CurrentCulture);
                            }
                            catch
                            {
                                typedValue = DateTime.Parse(value.ToString(), CultureInfo.CurrentCulture);
                            }
                        }
                        break;
                    case nameof(Decimal):
                        typedValue = Convert.ChangeType(value, Type.GetType($"System.{dataType}"));
                        break;
                    default:
                        typedValue = value;
                        break;
                }
            }
            catch (Exception e)
            {
                ThrowException(e.Message, "ConvertToType: Value: " + value.ToString() + " DataType:" + dataType);
                return null;
            }

            return typedValue;
        }

        private async Task SearchDialog(DbNetSuiteResponse response)
        {
            var searchDialogViewModel = new SearchDialogViewModel();
            ReflectionHelper.CopyProperties(this, searchDialogViewModel);
            searchDialogViewModel.Columns = Columns;
            response.Dialog = await HttpContext.RenderToStringAsync("Views/DbNetFile/SearchDialog.cshtml", searchDialogViewModel);
        }
    }
}