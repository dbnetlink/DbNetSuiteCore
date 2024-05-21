using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using System.Text.RegularExpressions;
using DbNetSuiteCore.Utilities;
using DbNetSuiteCore.Web.UI.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DbNetSuiteCore.Enums;
using System.Configuration;

namespace DbNetSuiteCore.Web.UI.Pages.Samples.DbNetGrid
{
    public class BrowseDbModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public string? Db { get; set; }
        public string? Table { get; set; }
        public string? View { get; set; }
        public string? ErrorMessage { get; set; }

        public string? FromPart => QualifiedObjectName();
        public bool? IsTable => string.IsNullOrEmpty(View);
        public DatabaseType? DatabaseType { get; set; }
        public List<DbObject> Tables { get; set; } = new List<DbObject>();
        public List<DbObject> Views { get; set; } = new List<DbObject>();
        public Dictionary<string, DatabaseType> Connections { get; set; } = new Dictionary<string, DatabaseType>();

        public BrowseDbModel(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public void OnGet(string? db = null, string? table = null, string? view = null, DatabaseType? databaseType = null)
        {
            Table = table;
            View = view;
            BrowseDbPopulate(db, databaseType);
        }

        public void BrowseDbPopulate(string? db = null, DatabaseType? databaseType = null)
        {
            var connectonStrings = _configuration.GetSection("ConnectionStrings").GetChildren();
            connectonStrings = connectonStrings.AsEnumerable().Where(c => FilterConnectionString(c)).ToList();

            var connectionAlias = db ?? connectonStrings.AsEnumerable().First().Key;
            var connectionString = _configuration.GetConnectionString(connectionAlias);

            Connections = connectonStrings.AsEnumerable().Select(c => c.Key).ToDictionary(c => c, c => DbNetDataCore.DeriveDatabaseType(_configuration.GetConnectionString(c)));
            Db = connectionAlias;
            DatabaseType = databaseType.HasValue ? databaseType : Connections[connectionAlias];

            try
            {
                using (var connection = new DbNetDataCore(connectionAlias, _webHostEnvironment, _configuration, DatabaseType.Value))
                {
                    connection.Open();
                    Tables = connection.InformationSchema(DbNetDataCore.MetaDataType.Tables);
                    Views = connection.InformationSchema(DbNetDataCore.MetaDataType.Views);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unable to connect to database => {ex.Message}";
                return;
            }

            if (string.IsNullOrEmpty(Table) && string.IsNullOrEmpty(View))
            {
                Table = Tables.First().QualifiedTableName;
            }
        }
        private bool FilterConnectionString(IConfigurationSection configSection)
        {
            string path = _configuration.GetConnectionString(configSection.Key) ?? string.Empty;
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


        private string QualifiedObjectName()
        {
            string fromPart = string.IsNullOrEmpty(View) ? Table : View;
            if (fromPart.Contains(" "))
            {
                if (fromPart.StartsWith("[") == false)
                {
                    fromPart = $"[{string.Join("].[", fromPart.Split("."))}]";
                }
            }
            return fromPart;
        }
    }
}
