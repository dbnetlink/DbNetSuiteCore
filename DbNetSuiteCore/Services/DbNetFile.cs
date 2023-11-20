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
            }

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(response, serializeOptions);
        }

        private async Task Page(DbNetFileResponse response, bool inititialise = false)
        {
            if (inititialise)
            {
                response.Toolbar = await Toolbar();
            }
            DataTable dataTable = CreateDataTable();
            IDirectoryContents folderContents = _fileProvider.GetDirectoryContents(Folder);

            foreach (IFileInfo fileInfo in folderContents)
            {
                var row = dataTable.NewRow();
                row[FileInfoProperties.Name.ToString()] = fileInfo.Name;
                row[FileInfoProperties.IsDirectory.ToString()] = fileInfo.IsDirectory;
                row[FileInfoProperties.LastModified.ToString()] = fileInfo.LastModified;
                row[FileInfoProperties.Length.ToString()] = fileInfo.Length;
                row[FileInfoProperties.Exists.ToString()] = fileInfo.Exists;

                if (!fileInfo.IsDirectory)
                {
                    FileInfo systemfileInfo = new FileInfo(fileInfo.PhysicalPath);
                    row[FileInfoProperties.Created.ToString()] = systemfileInfo.CreationTime;
                    row[FileInfoProperties.LastAccessed.ToString()] = systemfileInfo.LastAccessTime;
                    row[FileInfoProperties.Extension.ToString()] = systemfileInfo.Extension;
                }

                dataTable.Rows.Add(row);
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

            if (Columns.Any( c => c.Type == FileInfoProperties.Name) == false)
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
    }
}