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

        [Fact]
        public async Task ColumnsTest()
        {
            DbNetEditRequest request = GetRequest();

            request.Columns.Add(new EditColumn("CustomerID"));
            request.Columns.Add(new EditColumn("CompanyName"));
            request.Columns.Add(new EditColumn("Address"));
            request.Columns.Add(new EditColumn("Country") { Lookup = EncodingHelper.Encode("select distinct country from customers")});
            request.Columns.Add(new EditColumn("Phone"));

            DbNetEditResponse? dbNetEditResponse = await GetResponse(request, "initialize");
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetEditResponse?.Form.ToString() ?? string.Empty);
            var fields = document.QuerySelectorAll("input");
            Assert.Equal(4, fields.Count());
            var select = document.QuerySelectorAll("select");
            Assert.Single(select);
        }
    
        [Fact]
        public async Task StylingTest()
        {
            DbNetEditRequest request = GetRequest("Products");

            string style = "background-color:gold; color:steelblue";

            request.Columns.Add(new EditColumn("ProductId"));
            request.Columns.Add(new EditColumn("ProductName"));
            request.Columns.Add(new EditColumn("UnitPrice") { Style = style });
            request.Columns.Add(new EditColumn("UnitsInStock"));

            DbNetEditResponse? dbNetEditResponse = await GetResponse(request, RequestAction.Initialize);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetEditResponse?.Form.ToString() ?? string.Empty);
            var field = document.QuerySelector("input[name='unitprice']");
            Assert.Equal(style, field?.StyleAttr());
        }
    }
}