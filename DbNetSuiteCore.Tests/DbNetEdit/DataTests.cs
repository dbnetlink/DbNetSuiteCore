using AngleSharp.Html.Parser;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Tests.Extensions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DbNetSuiteCore.Constants.DbNetEdit;
using DbNetSuiteCore.Models.DbNetEdit;
using DbNetSuiteCore.Components;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DbNetSuiteCore.Tests.DbNetEdit
{
    public class DataTests : DbNetEditTests
    {
        public DataTests() : base() { }

        [Fact]
        public async Task SimpleTest()
        {
            DbNetEditRequest request = GetRequest();

            DbNetEditResponse? dbNetEditResponse = await GetResponse(request, "initialize");

            Assert.Equal(11, dbNetEditResponse?.Columns.Count);
            Assert.Equal("CustomerID", dbNetEditResponse?.Columns.First().ColumnName);
            Assert.Equal("Customer ID", dbNetEditResponse?.Columns.First().Label);
            Assert.Equal(true, dbNetEditResponse?.Columns.First().PrimaryKey);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetEditResponse?.Form ?? string.Empty);

            var fields = document.QuerySelectorAll("input");
            Assert.Equal(11, fields.Count());
        }
        /*
        [Fact]
        public async Task ColumnsTest()
        {
            DbNetEditRequest request = GetRequest();

            request.Columns.Add(new EditColumn("CompanyName") { Label = "Name" });
            request.Columns.Add(new EditColumn("Address + ', ' + City as Addr") { Label = "Address" });
            request.Columns.Add(new EditColumn("Country"));
            request.Columns.Add(new EditColumn("Phone + '/' + Fax as PhoneFax") { Label = "Phone/Fax" });

            DbNetEditResponse? dbNetEditResponse = await GetResponse(request, "initialize");
            Assert.Equal(4, dbNetEditResponse?.Columns.Count);
            Assert.Equal("CompanyName", dbNetEditResponse?.Columns.First().ColumnName);
            Assert.Equal("Name", dbNetEditResponse?.Columns.First().Label);
            Assert.Equal(false, dbNetEditResponse?.Columns.First().PrimaryKey);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetEditResponse?.Data.ToString() ?? string.Empty);

            var thead = document.QuerySelector("thead");
            Assert.Equal(1, thead?.Children.Length);
            var tbody = document.QuerySelector("tbody");
            Assert.Equal((int)request.PageSize, tbody?.Children.Length);
            var tr = tbody?.Children.First();
            Assert.Equal(4, tr?.Children.Length);
        }

        [Fact]
        public async Task JoinTest()
        {
            DbNetEditRequest request = GetRequest("Customers join Orders on Customers.CustomerID = Orders.CustomerID join [Order Details] on Orders.OrderID = [Order Details].OrderID");

            request.Columns.Add(new EditColumn("CompanyName"));
            request.Columns.Add(new EditColumn("OrderDate"));
            request.Columns.Add(new EditColumn("[Order Details].OrderID"));
            request.Columns.Add(new EditColumn("ProductID") { Lookup = EncodingHelper.Encode("select ProductId, ProductName from Products") });
            request.Columns.Add(new EditColumn("UnitPrice") { Format = "c" });
            request.Columns.Add(new EditColumn("Quantity"));

            DbNetEditResponse? dbNetEditResponse = await GetResponse(request, "initialize");
            Assert.Equal(6, dbNetEditResponse?.Columns.Count);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetEditResponse?.Data.ToString() ?? string.Empty);

            var tbody = document.QuerySelector("tbody");
            Assert.Equal((int)request.PageSize, tbody?.Children.Length);
            var tr = tbody?.Children.First();
            Assert.Equal(6, tr?.Children.Length);
            Assert.Equal("Rössle Sauerkraut", tr?.Children[CellIndex(document, "ProductID")].TextContent);
            Assert.StartsWith("£", tr?.Children[4].TextContent);
        }

        [Fact]
        public async Task StylingTest()
        {
            DbNetEditRequest request = GetRequest("Products");

            string style = "background-color:gold; color:steelblue";

            request.Columns.Add(new EditColumn("ProductName"));
            request.Columns.Add(new EditColumn("UnitPrice") { Style = style });
            request.Columns.Add(new EditColumn("UnitsInStock"));

            DbNetEditResponse? dbNetEditResponse = await GetResponse(request, RequestAction.Initialize);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetEditResponse?.Data.ToString() ?? string.Empty);

            var tbody = document.QuerySelector("tbody");
            Assert.Equal((int)request.PageSize, tbody?.Children.Length);
            var tr = tbody?.Children.First();
            Assert.Equal(style, tr?.Children[1].StyleAttr());
        }

        [Fact]
        public async Task MultiRowSelectTest()
        {
            foreach (MultiRowSelectLocation multiRowSelectLocation in Enum.GetValues(typeof(MultiRowSelectLocation)))
            {
                DbNetEditRequest request = GetRequest();

                request.MultiRowSelect = true;
                request.MultiRowSelectLocation = multiRowSelectLocation;

                DbNetEditResponse? dbNetEditResponse = await GetResponse(request, RequestAction.Initialize);

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(dbNetEditResponse?.Data.ToString() ?? string.Empty);

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

        [Fact]
        public async Task NestedTest()
        {
            DbNetEditRequest request = GetRequest();

            request.NestedEdit = true;

            DbNetEditResponse? dbNetEditResponse = await GetResponse(request, RequestAction.Initialize);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetEditResponse?.Data.ToString() ?? string.Empty);

            var tbody = document.QuerySelector("tbody");
            IElement tr = tbody!.Children.First();
            IElement td = tr!.Children.First();
            Assert.Equal(dbNetEditResponse!.Columns.Count + 1, tr!.Children.Count());
            Assert.Equal("BUTTON", td!.Children.First().TagName);
        }

        [Fact]
        public async Task ColumnFiltersTest()
        {
            DbNetEditRequest request = GetRequest("products");

            request.BooleanDisplayMode = BooleanDisplayMode.Checkbox;

            var columns = new List<string>() { "ProductID", "ProductName", "SupplierID", "CategoryID", "QuantityPerUnit", "UnitPrice", "UnitsInStock", "UnitsOnOrder", "ReorderLevel", "Discontinued" };

            foreach( var column in columns)
            {
                var gridColumn = new EditColumn(column) { Filter = true };

                switch(column)
                {
                    case "SupplierID":
                        gridColumn.Lookup = EncodingHelper.Encode("select SupplierId, CompanyName from Suppliers");
                        break;
                    case "CategoryID":
                        gridColumn.Lookup = EncodingHelper.Encode("select CategoryId, CategoryName from Categories");
                        gridColumn.FilterMode = FilterColumnSelectMode.List;
                        break;
                }
                request.Columns.Add(gridColumn);
            }

            DbNetEditResponse? dbNetEditResponse = await GetResponse(request, "initialize");

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetEditResponse?.Data.ToString() ?? string.Empty);

            var thead = document.QuerySelector("thead");
            Assert.Equal(2, thead!.Children.Count());

            IHtmlTableRowElement? filterRow = thead!.Children[1] as IHtmlTableRowElement;

            Assert.Equal("INPUT", filterRow!.Children[CellIndex(document, "ProductID")].Children.First().TagName);
            Assert.Equal("SELECT", filterRow!.Children[CellIndex(document, "CategoryID")].Children.First().TagName);
            Assert.Equal("INPUT", filterRow!.Children[CellIndex(document, "SupplierID")].Children.First().TagName);
        }

        [Fact]
        public async Task FrozenHeaderTest()
        {
            DbNetEditRequest request = GetRequest("products");
            request.FrozenHeader = true;

            var columns = new List<string>() { "ProductID", "ProductName", "SupplierID", "CategoryID", "QuantityPerUnit", "UnitPrice", "UnitsInStock", "UnitsOnOrder", "ReorderLevel", "Discontinued" };

            foreach (var column in columns)
            {
                var gridColumn = new EditColumn(column) { Filter = true };
                request.Columns.Add(gridColumn);
            }

            DbNetEditResponse? dbNetEditResponse = await GetResponse(request, "initialize");

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetEditResponse?.Data.ToString() ?? string.Empty);

            var thead = document.QuerySelector("thead");
            foreach (IElement th in thead!.QuerySelectorAll("th"))
            {
                Assert.True(th.ClassList.Contains("sticky"));
            }
        }

        [Fact]
        public async Task ViewTest()
        {
            DbNetEditRequest request = GetRequest("employees");
            request.View = true;

            var columns = new List<string>() { "firstname", "lastname", "photo", "notes", "photopath" };

            foreach (var column in columns)
            {
                var gridColumn = new EditColumn(column);

                switch(column)
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

            request.Columns.Add(new EditColumn("reportsto") { Lookup = EncodingHelper.Encode("select EmployeeId, lastname + ',' + firstname from employees") } );

            DbNetEditResponse? dbNetEditResponse = await GetResponse(request, "initialize");

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetEditResponse!.Toolbar);
            var viewButton = document.QuerySelector("button[button-type=\"view\"]");
            Assert.NotNull(viewButton);
            document = await parser.ParseDocumentAsync(dbNetEditResponse?.Data.ToString() ?? string.Empty);
            var tbody = document.QuerySelector("tbody");
            Assert.Equal(9, tbody?.Children.Length);
        }
        */
    }
}