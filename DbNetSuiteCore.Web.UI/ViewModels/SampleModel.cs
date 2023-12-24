using DbNetSuiteCore.Web.UI.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using DbNetSuiteCore.Enums;

namespace DbNetSuiteCore.Web.UI.ViewModels
{
    public class SampleModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
		private DataProvider _dataProvider;
		public DataProvider DataProvider => _dataProvider;
        private string _connectionString => _configuration?.GetConnectionString("northwind")?.
            Replace("~", _webHostEnvironment.WebRootPath) ?? string.Empty;

        public SampleModel(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration; 
        }

        public void OnGet(DataProvider dataProvider = DataProvider.SqlClient)
        {
            _dataProvider = dataProvider;
		}

        public async Task<IEnumerable<T>> GetList<T>(string tableName)
        {
            IEnumerable<T> data = new List<T>();
            using (var conn = new SqliteConnection(_connectionString))
            {
                SqlMapper.AddTypeHandler(new BooleanTypeHandler());
                SqlMapper.AddTypeHandler(new DecimalTypeHandler());
                data = await conn.QueryAsync<T>($"select * from {tableName};");
            }

            return data;
        }

        public async Task<Dictionary<int, string>> GetLookup(string sql) 
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("value", typeof(int));
            dataTable.Columns.Add("text", typeof(string));

            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                IDataReader reader = await conn.ExecuteReaderAsync(sql);

                while (reader.Read())
                {
                    dataTable.Rows.Add(reader.GetValue(0), reader.GetValue(1));
                }

                await conn.CloseAsync();
            }

            Dictionary<int, string> lookup = new Dictionary<int, string>();

            foreach( DataRow row in dataTable.Rows)
            {
                lookup.Add((int)row[0], (string)row[1]);
            }

            return lookup;
        }
    }
}
