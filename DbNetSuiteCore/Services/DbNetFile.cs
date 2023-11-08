using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Constants.DbNetCombo;
using DbNetSuiteCore.Models.DbNetFile;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc;
using DbNetSuiteCore.ViewModels.DbNetFile;
using DbNetSuiteCore.Enums.DbNetFile;
using System.IO;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace DbNetSuiteCore.Services
{
    internal class DbNetFile : DbNetSuite
    {
        private Dictionary<string, object> _resp = new Dictionary<string, object>();

        private string _folder;
        private string _rootFolder;

        private readonly IFileProvider _fileProvider;
        public List<FileColumn> Columns { get; set; } = new List<FileColumn>();

        public DbNetFile(AspNetCoreServices services) : base(services)
        {
            _fileProvider = services.webHostEnvironment.WebRootFileProvider;
        }

        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
        public string Folder
        {
            get => EncodingHelper.Decode(_folder);
            set => _folder = value;
        }
        public string RootFolder
        {
            get => EncodingHelper.Decode(_rootFolder);
            set => _rootFolder = value;
        }
        public new async Task<object> Process()
        {
            await DeserialiseRequest<DbNetFileRequest>();
            Initialise();

            DbNetFileResponse response = new DbNetFileResponse();

            ConfigureColumns();

            switch (Action.ToLower())
            {
                case RequestAction.Page:
                    await Page(response);
                    break;
            }

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(response, serializeOptions);
        }


        private async Task Page(DbNetFileResponse response)
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

            IDirectoryContents folderContents = _fileProvider.GetDirectoryContents(Folder);

            foreach (IFileInfo fileInfo in folderContents)
            {
                var row = dataTable.NewRow();
                row[FileInfoProperties.Name.ToString()] = fileInfo.Name;
                row[FileInfoProperties.IsDirectory.ToString()] = fileInfo.IsDirectory;
                row[FileInfoProperties.LastModified.ToString()] = fileInfo.LastModified;
                row[FileInfoProperties.Length.ToString()] = fileInfo.Length;
                row[FileInfoProperties.Exists.ToString()] = fileInfo.Exists;

                if (fileInfo.IsDirectory)
                {
                    /*
                    DirectoryInfo systemDirectoryInfo = new DirectoryInfo(fileInfo.PhysicalPath);
                    row[FileInfoProperties.Created.ToString()] = systemDirectoryInfo.CreationTime;
                    row[FileInfoProperties.LastAccessed.ToString()] = systemDirectoryInfo.LastAccessTime;
                    row[FileInfoProperties.Extension.ToString()] = systemDirectoryInfo.Extension;
                    */
                }
                else
                {
                    FileInfo systemfileInfo = new FileInfo(fileInfo.PhysicalPath);
                    row[FileInfoProperties.Created.ToString()] = systemfileInfo.CreationTime;
                    row[FileInfoProperties.LastAccessed.ToString()] = systemfileInfo.LastAccessTime;
                    row[FileInfoProperties.Extension.ToString()] = systemfileInfo.Extension;
                }

                dataTable.Rows.Add(row);
            }

            var viewModel = new FileViewModel
            {
                DataView = new DataView(dataTable),
                Columns = Columns,
                RootFolder = RootFolder
            };

            ReflectionHelper.CopyProperties(this, viewModel);
            response.Html = await HttpContext.RenderToStringAsync($"Views/DbNetFile/Page.cshtml", viewModel);
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
        }
    }
}