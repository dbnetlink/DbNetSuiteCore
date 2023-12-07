using DbNetSuiteCore.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using Microsoft.Data.Sqlite;
using DbNetSuiteCore.Models.DbNetEdit;

namespace DbNetSuiteCore.Web.UI.Pages.Samples.DbNetEdit
{
    public class GenericListEditModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private string _connectionString => _configuration?.GetConnectionString("northwind")?.
            Replace("~", _webHostEnvironment.WebRootPath) ?? string.Empty;

        public IEnumerable<Product>? Products { get; set; }
        public Dictionary<int, string>? SupplierLookup { get; set; }
        public Dictionary<int, string>? CategoryLookup { get; set; }

        public GenericListEditModel(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
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

                this.SupplierLookup = await GetLookup(conn, "select SupplierID as Key, CompanyName as Value from Suppliers order by 2");
                this.CategoryLookup = await GetLookup(conn, "select CategoryID as Key, CategoryName as Value from Categories order by 2");
            }
        }

        private async Task<Dictionary<int, string>> GetLookup(SqliteConnection conn, string sql)
        {
            Dictionary<int, string> lookup = (await conn.QueryAsync<KeyValuePair<int, string>>(sql)).ToDictionary(row => row.Key, row => row.Value);
            return lookup;
        }

        public async Task<IActionResult> OnPost([FromBody] JsonUpdateRequest jsonUpdateRequest)
        {
            string message = "Product updated";
            bool success = true;

            using (var conn = new SqliteConnection(_connectionString))
            {
                string set = string.Join(",", jsonUpdateRequest.Changes.Keys.Select(k => $"{k} = @{k}"));
                var paramValues = new DynamicParameters();

                paramValues.Add($"@{jsonUpdateRequest.PrimaryKeyName}", jsonUpdateRequest.PrimaryKeyValue);
                foreach (string key in jsonUpdateRequest.Changes.Keys)
                {
                    paramValues.Add($"@{key}", jsonUpdateRequest.Changes[key]);
                }

                try
                {
                    conn.Execute($"update products set {set} where {jsonUpdateRequest.PrimaryKeyName} = @{jsonUpdateRequest.PrimaryKeyName}", paramValues);
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    success = false;
                }

                this.Products = await conn.QueryAsync<Product>("select * from products;");
            }
            return new JsonResult(new JsonUpdateResponse { Success = success, Message = message, DataSet = this.Products });
        }
    }
}
