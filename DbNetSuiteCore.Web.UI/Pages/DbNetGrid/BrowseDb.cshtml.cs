using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using System.Text.RegularExpressions;
using DbNetSuiteCore.Utilities;
using DbNetSuiteCore.Web.UI.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DbNetSuiteCore.Web.UI.Pages.Samples.DbNetGrid
{
    public class BrowseDbModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public string? Db { get; set; }
        public string? Table { get; set; }
        public string? View { get; set; }
        public List<DbObject> Tables { get; set; } = new List<DbObject>();
        public List<DbObject> Views { get; set; } = new List<DbObject>();
        public List<string> Connections { get; set; } = new List<string>();

        public BrowseDbModel(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public void OnGet(string? db = null,string? table = null,string? view = null)   
        {
            Table = table;
            View = view;
            BrowseDbPopulate(db);
        }

        public void BrowseDbPopulate(string? db = null)
        {
            var connectonStrings = _configuration.GetSection("ConnectionStrings").GetChildren();
            connectonStrings = connectonStrings.AsEnumerable().Where(c => FilterConnectionString(_configuration.GetConnectionString(c.Key))).ToList();

            var connectionAlias = db ?? connectonStrings.AsEnumerable().First().Key;
            var connectionString = _configuration.GetConnectionString(connectionAlias);

            Connections = connectonStrings.AsEnumerable().Select(c => c.Key).ToList();

            using (var connection = new DbNetDataCore(connectionString, _webHostEnvironment))
            {
                connection.Open();
                Tables = connection.InformationSchema(DbNetDataCore.MetaDataType.Tables);
                Views = connection.InformationSchema(DbNetDataCore.MetaDataType.Views);
            }

            Db = connectionAlias;

            if (string.IsNullOrEmpty(Table) && string.IsNullOrEmpty(View))
            {
                Table = Tables.First().QualifiedTableName;
            }
        }
        private bool FilterConnectionString(string path)
        {
            if (_webHostEnvironment.IsDevelopment())
            {
                return true;
            }

            return IsSqliteConnectionString(path);
        }

        private bool IsSqliteConnectionString(string path)
        {
            return Regex.IsMatch(path, @"Data Source=(.*)\.db;", RegexOptions.IgnoreCase);
        }

        private string MapDatabasePath(string ConnectionString)
        {
            if (!ConnectionString.EndsWith(";"))
                ConnectionString += ";";

            ConnectionString = Regex.Replace(ConnectionString, @"DataProvider=(.*?);", "", RegexOptions.IgnoreCase);
            string CurrentPath = this._webHostEnvironment.WebRootPath;
            string DataSourcePropertyName = "data source";
            ConnectionString = Regex.Replace(ConnectionString, DataSourcePropertyName + "=~", DataSourcePropertyName + "=" + CurrentPath, RegexOptions.IgnoreCase).Replace("=//", "=/");
            return ConnectionString;
        }

        private List<DbObject> InformationSchemaQuery(SqlConnection conn, string tableType)
        {
            var sql = $"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='{tableType}' order by TABLE_SCHEMA, TABLE_NAME";
            return conn.Query<InformationSchema>(sql).Select(s => new DbObject() { QualifiedTableName = s.FullQualifiedTableName, TableName = s.FullTableName }).ToList();
        }

        private string SqliteMasterQuery(string tableType)
        {
            return $"SELECT name as QualifiedTableName,name as TableName FROM sqlite_master WHERE type='{tableType}' and name not like 'sqlite_%' order by name";
        }
    }
}
