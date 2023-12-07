using DbNetSuiteCore.Web.UI.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using Microsoft.Data.Sqlite;

namespace DbNetSuiteCore.Web.UI.Pages.Samples.DbNetGrid
{
    public class GenericListModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private string _connectionString => _configuration?.GetConnectionString("northwind")?.
            Replace("~", _webHostEnvironment.WebRootPath) ?? string.Empty;

        public IEnumerable<Product>? Products { get; set; }
        public Dictionary<int,string>? SupplierLookup { get; set; }
        public Dictionary<int, string>? CategoryLookup { get; set; }

        public GenericListModel(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration; 
        }

        public async Task OnGet()
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                SqlMapper.AddTypeHandler(new BooleanTypeHandler());
                SqlMapper.AddTypeHandler(new DecimalTypeHandler());
                this.Products = await conn.QueryAsync<Product>("select * from products;");

                this.SupplierLookup = await GetLookup(conn,"select SupplierID as Key, CompanyName as Value from Suppliers order by 2");
                this.CategoryLookup = await GetLookup(conn, "select CategoryID as Key, CategoryName as Value from Categories order by 2");
            }
        }

        private async Task<Dictionary<int, string>> GetLookup(SqliteConnection conn, string sql) 
        {
            Dictionary<int, string> lookup = (await conn.QueryAsync<KeyValuePair<int, string>>(sql)).ToDictionary(row => row.Key, row => row.Value);
            return lookup;
        }
    }
}
