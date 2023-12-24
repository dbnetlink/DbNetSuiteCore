using AngleSharp.Html.Parser;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Tests.Extensions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DbNetSuiteCore.Constants.DbNetGrid;
using DbNetSuiteCore.Models.DbNetGrid;
using DbNetSuiteCore.Tests.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DbNetSuiteCore.Tests.DbNetGrid
{
    public class DataTests : DbNetGridTests
    {
        private List<DataProvider> _dataProviders = new List<DataProvider>() { DataProvider.SQLite, DataProvider.SqlClient, DataProvider.DataTable };
        public DataTests() : base() { }

        [Fact]
        public async Task SimpleTest()
        {
            DbNetGridRequest request;
            foreach (DataProvider dataProvider in _dataProviders)
            {
                switch (dataProvider)
                {
                    case DataProvider.SQLite:
                    case DataProvider.SqlClient:
                        request = GetRequest("Customers", dataProvider);
                        break;
                    default:
                        request = GetRequest<Customer>();
                        request.GetColumn(nameof(Customer.CustomerID)).PrimaryKey = true;
                        break;
                }

                request.PageSize = 10;

                DbNetGridResponse? dbNetGridResponse = await GetResponse(request, RequestAction.Initialize);

                Assert.Equal(11, dbNetGridResponse?.Columns.Count(c => c.Show));
                Assert.Equal("CustomerID", dbNetGridResponse?.Columns.First(c => c.Show).ColumnName);
                Assert.Equal("Customer ID", dbNetGridResponse?.Columns.First(c => c.Show).Label);
                Assert.Equal(true, dbNetGridResponse?.Columns.First().PrimaryKey);

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(dbNetGridResponse?.Data.ToString() ?? string.Empty);

                var thead = document.QuerySelector("thead");
                Assert.Equal(1, thead?.Children.Length);
                var tbody = document.QuerySelector("tbody");
                Assert.Equal((int)request.PageSize, tbody?.Children.Length);
                var tr = tbody?.Children.First();
                Assert.Equal(11, tr?.Children.Length);

                request.CurrentPage = 5;
                request.Columns = dbNetGridResponse!.Columns;

                dbNetGridResponse = await GetResponse(request, RequestAction.Page);

                document = await parser.ParseDocumentAsync(dbNetGridResponse?.Data.ToString() ?? string.Empty);
                tbody = document.QuerySelector("tbody");
                Assert.Equal((int)request.PageSize, tbody?.Children.Length);
            }
        }

        [Fact]
        public async Task ColumnsTest()
        {
            DbNetGridRequest request;

            foreach (DataProvider dataProvider in _dataProviders)
            {
                switch (dataProvider)
                {
                    case DataProvider.SQLite:
                    case DataProvider.SqlClient:
                        request = GetRequest("Customers", dataProvider);
                        request.Columns.Add(new GridColumn("CompanyName") { Label = "Name" });
                        request.Columns.Add(new GridColumn("Address + ', ' + City as Addr") { Label = "Address" });
                        request.Columns.Add(new GridColumn("Country"));
                        request.Columns.Add(new GridColumn("Phone + '/' + Fax as PhoneFax") { Label = "Phone/Fax" });
                        break;
                    default:
                        request = GetRequest<Customer>();
                        request.Columns.Add(new GridColumn(nameof(Customer.CompanyName)) { Label = "Name" });
                        request.Columns.Add(new GridColumn("Address + ', ' + City as Addr") { Label = "Address" });
                        request.Columns.Add(new GridColumn(nameof(Customer.Country)));
                        request.Columns.Add(new GridColumn("Phone + '/' + Fax as PhoneFax") { Label = "Phone/Fax" });
                        break;
                }

                DbNetGridResponse? response = await GetResponse(request, RequestAction.Initialize);
                Assert.Equal(4, response?.Columns.Count);
                Assert.Equal("CompanyName", response?.Columns.First().ColumnName);
                Assert.Equal("Name", response?.Columns.First().Label);
                Assert.Equal(false, response?.Columns.First().PrimaryKey);

                HtmlParser parser = new HtmlParser();
                IHtmlDocument document = await parser.ParseDocumentAsync(response?.Data.ToString() ?? string.Empty);

                IElement? thead = document.QuerySelector("thead");
                Assert.Equal(1, thead?.Children.Length);
                var tbody = document.QuerySelector("tbody");
                Assert.Equal((int)request.PageSize, tbody?.Children.Length);
                var tr = tbody?.Children.First();
                Assert.Equal(4, tr?.Children.Length);
            }
        }

        [Fact]
        public async Task JoinTest()
        {
            foreach (DataProvider dataProvider in _dataProviders)
            {
                DbNetGridRequest request;
                switch (dataProvider)
                {
                    case DataProvider.SQLite:
                    case DataProvider.SqlClient:
                        request = GetRequest("Customers join Orders on Customers.CustomerID = Orders.CustomerID join [Order Details] on Orders.OrderID = [Order Details].OrderID", dataProvider);
                        break;
                    default:
                        continue;
                }

                request.Columns.Add(new GridColumn("CompanyName"));
                request.Columns.Add(new GridColumn("OrderDate"));
                request.Columns.Add(new GridColumn("[Order Details].OrderID"));
                request.Columns.Add(new GridColumn("ProductID") { Lookup = EncodingHelper.Encode("select ProductId, ProductName from Products") });
                request.Columns.Add(new GridColumn("UnitPrice") { Format = "c" });
                request.Columns.Add(new GridColumn("Quantity"));

                DbNetGridResponse? dbNetGridResponse = await GetResponse(request, RequestAction.Initialize);
                Assert.Equal(6, dbNetGridResponse?.Columns.Count);

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(dbNetGridResponse?.Data.ToString() ?? string.Empty);

                var tbody = document.QuerySelector("tbody");
                Assert.Equal((int)request.PageSize, tbody?.Children.Length);
                var tr = tbody?.Children.First();
                Assert.Equal(6, tr?.Children.Length);
                Assert.Equal("Rössle Sauerkraut", tr?.Children[CellIndex(document, "ProductID")].TextContent);
                Assert.StartsWith("£", tr?.Children[4].TextContent);
            }
        }

        [Fact]
        public async Task StylingTest()
        {
            foreach (DataProvider dataProvider in _dataProviders)
            {
                DbNetGridRequest request;
                switch (dataProvider)
                {
                    case DataProvider.SQLite:
                    case DataProvider.SqlClient:
                        request = GetRequest("Products", dataProvider);
                        break;
                    default:
                        request = GetRequest<Product>("products");
                        break;
                }

                string style = "background-color:gold; color:steelblue";

                request.Columns.Add(new GridColumn("ProductName"));
                request.Columns.Add(new GridColumn("UnitPrice") { Style = style });
                request.Columns.Add(new GridColumn("UnitsInStock"));

                DbNetGridResponse? dbNetGridResponse = await GetResponse(request, RequestAction.Initialize);

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(dbNetGridResponse?.Data.ToString() ?? string.Empty);

                var tbody = document.QuerySelector("tbody");
                Assert.Equal((int)request.PageSize, tbody?.Children.Length);
                var tr = tbody?.Children.First();
                Assert.Equal(style, tr?.Children[1].StyleAttr());
            }
        }

        [Fact]
        public async Task MultiRowSelectTest()
        {
            foreach (MultiRowSelectLocation multiRowSelectLocation in Enum.GetValues(typeof(MultiRowSelectLocation)))
            {
                foreach (DataProvider dataProvider in _dataProviders)
                {
                    DbNetGridRequest request;
                    switch (dataProvider)
                    {
                        case DataProvider.SQLite:
                        case DataProvider.SqlClient:
                            request = GetRequest("customers", dataProvider);
                            break;
                        default:
                            request = GetRequest<Customer>("customers");
                            break;
                    }
                   
                    request.MultiRowSelect = true;
                    request.MultiRowSelectLocation = multiRowSelectLocation;

                    DbNetGridResponse? dbNetGridResponse = await GetResponse(request, RequestAction.Initialize);

                    var parser = new HtmlParser();
                    var document = await parser.ParseDocumentAsync(dbNetGridResponse?.Data.ToString() ?? string.Empty);

                    string[] selectors = { "thead", "tbody" };
                    foreach (string selector in selectors)
                    {
                        var thead = document.QuerySelector(selector);
                        IElement tr = thead!.Children.First();
                        IElement cell;
                        IElement checkbox;
                        if (multiRowSelectLocation == MultiRowSelectLocation.Left)
                            cell = tr!.Children.First();
                        else
                            cell = tr!.Children.Last();

                        checkbox = cell.Children.First();

                        Assert.Equal("INPUT", checkbox?.TagName);
                        Assert.Equal("checkbox", checkbox?.TypeAttr());
                    }
                }
            }
        }

        [Fact]
        public async Task NestedTest()
        {
            DbNetGridRequest request = GetRequest();

            request.NestedGrid = true;

            DbNetGridResponse? dbNetGridResponse = await GetResponse(request, RequestAction.Initialize);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetGridResponse?.Data.ToString() ?? string.Empty);

            var tbody = document.QuerySelector("tbody");
            IElement tr = tbody!.Children.First();
            IElement td = tr!.Children.First();
            Assert.Equal(dbNetGridResponse!.Columns.Count(c => c.Show) + 1, tr!.Children.Count());
            Assert.Equal("BUTTON", td!.Children.First().TagName);
        }

        [Fact]
        public async Task ColumnFiltersTest()
        {
            foreach (DataProvider dataProvider in _dataProviders)
            {
                DbNetGridRequest request;
                switch (dataProvider)
                {
                    case DataProvider.SQLite:
                    case DataProvider.SqlClient:
                        request = GetRequest("Products", dataProvider);
                        break;
                    default:
                        request = GetRequest<Product>("products");
                        break;
                }

                request.BooleanDisplayMode = BooleanDisplayMode.Checkbox;

                var columns = new List<string>() { "ProductID", "ProductName", "SupplierID", "CategoryID", "QuantityPerUnit", "UnitPrice", "UnitsInStock", "UnitsOnOrder", "ReorderLevel", "Discontinued" };

                foreach (var column in columns)
                {
                    var gridColumn = new GridColumn(column) { Filter = true };

                    switch (column)
                    {
                        case "ProductID":
                            gridColumn.Display = true;
                            break;
                        case "SupplierID":
                            switch (dataProvider)
                            {
                                case DataProvider.SQLite:
                                case DataProvider.SqlClient:
                                    gridColumn.Lookup = EncodingHelper.Encode("select SupplierId, CompanyName from Suppliers");
                                    break;
                                default:
                                    gridColumn.LookupDataTable = await GetLookup("select SupplierId, CompanyName from Suppliers order by 2");
                                    break;
                            }
                            break;
                        case "CategoryID":
                            switch (dataProvider)
                            {
                                case DataProvider.SQLite:
                                case DataProvider.SqlClient:
                                    gridColumn.Lookup = EncodingHelper.Encode("select CategoryId, CategoryName from Categories");
                                    break;
                                default:
                                    gridColumn.LookupDataTable = await GetLookup("select CategoryId, CategoryName from Categories order by 2");
                                    break;
                            }
                            gridColumn.FilterMode = FilterColumnSelectMode.List;
                            break;
                    }
                    request.Columns.Add(gridColumn);
                }

                DbNetGridResponse? dbNetGridResponse = await GetResponse(request, RequestAction.Initialize);

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(dbNetGridResponse?.Data.ToString() ?? string.Empty);

                var thead = document.QuerySelector("thead");
                Assert.Equal(2, thead!.Children.Count());

                IHtmlTableRowElement? filterRow = thead!.Children[1] as IHtmlTableRowElement;

                Assert.Equal("INPUT", filterRow!.Children[CellIndex(document, "ProductID")].Children.First().TagName);
                Assert.Equal("SELECT", filterRow!.Children[CellIndex(document, "CategoryID")].Children.First().TagName);
                Assert.Equal("INPUT", filterRow!.Children[CellIndex(document, "SupplierID")].Children.First().TagName);
            }
        }

        [Fact]
        public async Task FrozenHeaderTest()
        {
            foreach (DataProvider dataProvider in _dataProviders)
            {
                DbNetGridRequest request;
                switch (dataProvider)
                {
                    case DataProvider.SQLite:
                    case DataProvider.SqlClient:
                        request = GetRequest("Products", dataProvider);
                        break;
                    default:
                        request = GetRequest<Product>("products");
                        break;
                }
                request.FrozenHeader = true;

                var columns = new List<string>() { "ProductID", "ProductName", "SupplierID", "CategoryID", "QuantityPerUnit", "UnitPrice", "UnitsInStock", "UnitsOnOrder", "ReorderLevel", "Discontinued" };

                foreach (var column in columns)
                {
                    var gridColumn = new GridColumn(column) { Filter = true };
                    request.Columns.Add(gridColumn);
                }

                DbNetGridResponse? dbNetGridResponse = await GetResponse(request, RequestAction.Initialize);

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(dbNetGridResponse?.Data.ToString() ?? string.Empty);

                var thead = document.QuerySelector("thead");
                foreach (IElement th in thead!.QuerySelectorAll("th"))
                {
                    Assert.True(th.ClassList.Contains("sticky"));
                }
            }
        }

        [Fact]
        public async Task ViewTest()
        {
            foreach (DataProvider dataProvider in _dataProviders)
            {
                DbNetGridRequest request;
                switch (dataProvider)
                {
                    case DataProvider.SQLite:
                    case DataProvider.SqlClient:
                        request = GetRequest("employees", dataProvider);
                        break;
                    default:
                        request = GetRequest<Employee>("employees");
                        break;
                }
                request.View = true;

                var columns = new List<string>() { "firstname", "lastname", "photo", "notes", "photopath" };
                GridColumn gridColumn;

                foreach (var column in columns)
                {
                    gridColumn = new GridColumn(column);

                    switch (column)
                    {
                        case "photo":
                        case "notes":
                        case "photopath":
                            gridColumn.Display = false;
                            break;
                    }

                    switch (column)
                    {
                        case "image":
                            gridColumn.Image = true;
                            break;
                    }

                    gridColumn.View = true;
                    request.Columns.Add(gridColumn);
                }

                gridColumn = new GridColumn("reportsto");

                switch (dataProvider)
                {
                    case DataProvider.SQLite:
                    case DataProvider.SqlClient:
                        gridColumn.Lookup = EncodingHelper.Encode("select EmployeeId, lastname + ',' + firstname from employees");
                        break;
                    default:
                        gridColumn.LookupDataTable = await GetLookup("select EmployeeId, lastname + ',' + firstname from employees order by 2");
                        break;
                }

                request.Columns.Add(gridColumn);

                DbNetGridResponse? dbNetGridResponse = await GetResponse(request, RequestAction.Initialize);

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(dbNetGridResponse!.Toolbar);
                var viewButton = document.QuerySelector("button[button-type=\"view\"]");
                Assert.NotNull(viewButton);
                document = await parser.ParseDocumentAsync(dbNetGridResponse?.Data.ToString() ?? string.Empty);
                var tbody = document.QuerySelector("tbody");
                Assert.Equal(9, tbody?.Children.Length);
            }
        }
    }
}