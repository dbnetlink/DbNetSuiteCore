using DbNetSuiteCore.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using Microsoft.Data.Sqlite;
using DbNetSuiteCore.Models.DbNetEdit;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Components;
using DbNetSuiteCore.Enums.DbNetEdit;

namespace DbNetSuiteCore.Web.UI.Pages.Samples.DbNetGrid
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
        public DbNetGridCore? ProductsGrid { get; set; }

        public GenericListEditModel(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public async Task OnGet()
        {
            IEnumerable<Product>? products;
            Dictionary<int, string>? supplierLookup;
            Dictionary<int, string>? categoryLookup;

            using (var conn = new SqliteConnection(_connectionString))
            {
                SqlMapper.AddTypeHandler(new BooleanTypeHandler());
                SqlMapper.AddTypeHandler(new DecimalTypeHandler());
                products = await conn.QueryAsync<Product>("select * from products;");

                supplierLookup = await GetLookup(conn, "select SupplierID as Key, CompanyName as Value from Suppliers order by 2");
                categoryLookup = await GetLookup(conn, "select CategoryID as Key, CategoryName as Value from Categories order by 2");
            }

            ProductsGrid = new DbNetGridCore(DataSourceType.List);
            ProductsGrid.AddList(products);
            ProductsGrid.Column(nameof(Product.ProductID)).PrimaryKey();
            ProductsGrid.EditControl.Column(nameof(Product.ProductID)).PrimaryKey();
            ProductsGrid.Column(nameof(Product.UnitPrice)).Format("c");
            ProductsGrid.Column(nameof(Product.Discontinued)).DataType(typeof(bool));
            ProductsGrid.Column(nameof(Product.SupplierID)).Lookup(supplierLookup);
            ProductsGrid.Column(nameof(Product.CategoryID)).Lookup(categoryLookup);
            ProductsGrid.EditControl.Column(new string[] { nameof(Product.UnitPrice), nameof(Product.ProductName), nameof(Product.CategoryID), nameof(Product.SupplierID), nameof(Product.QuantityPerUnit), nameof(Product.ReorderLevel), nameof(Product.UnitsInStock), nameof(Product.UnitsOnOrder) }).Required();
            ProductsGrid.EditControl.Column(nameof(Product.ReorderLevel)).DefaultValue("0");
            ProductsGrid.EditControl.Column(nameof(Product.UnitsInStock)).DefaultValue("0");

            // ProductsEdit.LayoutColumns = 2;
            ProductsGrid.Insert = true;
            ProductsGrid.Update = true;
            ProductsGrid.Delete = true;

            ProductsGrid.Bind(DbNetSuiteCore.Enums.DbNetGrid.EventType.onJsonUpdated, "applyJsonChanges");
            ProductsGrid.EditControl.Bind(EventType.onJsonUpdated, "applyJsonChanges");
        }

        private async Task<Dictionary<int, string>> GetLookup(SqliteConnection conn, string sql)
        {
            Dictionary<int, string> lookup = (await conn.QueryAsync<KeyValuePair<int, string>>(sql)).ToDictionary(row => row.Key, row => row.Value);
            return lookup;
        }

        public async Task<IActionResult> OnPost([FromBody] JsonUpdateRequest jsonUpdateRequest)
        {
            string message = "";
            bool success = true;

            using (var conn = new SqliteConnection(_connectionString))
            {
                string sql = string.Empty;
                var paramValues = new DynamicParameters();
                switch (jsonUpdateRequest.EditMode)
                {
                    case EditMode.Update:
                        string set = string.Join(",", jsonUpdateRequest.Changes.Keys.Select(k => $"{k} = @{k}"));
                        paramValues.Add($"@{jsonUpdateRequest.PrimaryKeyName}", jsonUpdateRequest.PrimaryKeyValue);
                        foreach (string key in jsonUpdateRequest.Changes.Keys)
                        {
                            paramValues.Add($"@{key}", jsonUpdateRequest.Changes[key]);
                        }
                        sql = $"update products set {set} where {jsonUpdateRequest.PrimaryKeyName} = @{jsonUpdateRequest.PrimaryKeyName}";
                        message = "Product updated";
                        break;
                    case EditMode.Insert:
                        string columns = string.Join(",", jsonUpdateRequest.Changes.Keys.Select(k => $"{k}"));
                        string values = string.Join(",", jsonUpdateRequest.Changes.Keys.Select(k => $"@{k}"));

                        foreach (string key in jsonUpdateRequest.Changes.Keys)
                        {
                            paramValues.Add($"@{key}", jsonUpdateRequest.Changes[key]);
                        }
                        sql = $"insert into products({columns}) values({values})";
                        message = "Product added";
                        break;
                    case EditMode.Delete:
                        paramValues.Add($"@{jsonUpdateRequest.PrimaryKeyName}", jsonUpdateRequest.PrimaryKeyValue);
                        sql = $"delete from products where {jsonUpdateRequest.PrimaryKeyName} = @{jsonUpdateRequest.PrimaryKeyName}";
                        message = "Product deleted";
                        break;
                }

                try
                {
                    conn.Execute(sql, paramValues);
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    success = false;
                }

                var products = await conn.QueryAsync<Product>("select * from products;");
                return new JsonResult(new JsonUpdateResponse { Success = success, Message = message, DataSet = products });
            }

        }
    }
}
