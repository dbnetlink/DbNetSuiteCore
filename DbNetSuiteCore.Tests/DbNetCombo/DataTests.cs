using AngleSharp.Html.Parser;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Helpers;
using DbNetSuiteCore.Components;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Tests.Extensions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DbNetSuiteCore.Constants.DbNetCombo;

namespace DbNetSuiteCore.Tests.DbNetCombo
{
    public class DataTests : DbNetComboTests
    {
        public DataTests() : base() { }

        [Fact]
        public async Task SimpleTest()
        {
            DbNetComboRequest request = GetRequest();
            request.ValueColumn = EncodingHelper.Encode("CustomerID");
            request.TextColumn = EncodingHelper.Encode("CompanyName");

            DbNetComboResponse? dbNetComboResponse = await GetResponse(request, RequestAction.Page);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetComboResponse?.Select.ToString() ?? string.Empty);
            var select = document.QuerySelectorAll("select");
            Assert.Equal(1, select?.Length);
            document = await parser.ParseDocumentAsync(dbNetComboResponse?.Options.ToString() ?? string.Empty);
            List<IHtmlOptionElement> options = document.QuerySelectorAll<IHtmlOptionElement>("option").ToList();
            Assert.Equal(91, options?.Count);

            Assert.Equal("ALFKI", options?.First().Value);
            Assert.Equal("Alfreds Futterkiste", options?.First().Text);
            Assert.Equal("WOLZA", options?.Last().Value);
            Assert.Equal("Wolski Zajazd", options?.Last().Text);
        }

        [Fact]
        public async Task ValueOnlyTest()
        {
            DbNetComboRequest request = GetRequest();
            request.ValueColumn = EncodingHelper.Encode("CompanyName");

            DbNetComboResponse? dbNetComboResponse = await GetResponse(request, RequestAction.Page);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetComboResponse?.Select.ToString() ?? string.Empty);
            var select = document.QuerySelectorAll("select");
            Assert.Equal(1, select?.Length);
            var input = document.QuerySelectorAll("input");
            Assert.Equal(0, input?.Length);
            document = await parser.ParseDocumentAsync(dbNetComboResponse?.Options.ToString() ?? string.Empty);
            List<IHtmlOptionElement> options = document.QuerySelectorAll<IHtmlOptionElement>("option").ToList();
            Assert.Equal(91, options?.Count);

            Assert.Equal("Alfreds Futterkiste", options?.First().Value);
            Assert.Equal("Alfreds Futterkiste", options?.First().Text);
            Assert.Equal("Wolski  Zajazd", options?.Last().Value);
            Assert.Equal("Wolski Zajazd", options?.Last().Text);
        }

        [Fact]
        public async Task FilterTest()
        {
            DbNetComboRequest request = GetRequest();
            request.ValueColumn = EncodingHelper.Encode("CustomerID");
            request.TextColumn = EncodingHelper.Encode("CompanyName");
            request.AddFilter = true;
            request.FilterToken = "the";

            DbNetComboResponse? dbNetComboResponse = await GetResponse(request, RequestAction.Filter);

            var parser = new HtmlParser();
            Assert.Null(dbNetComboResponse?.Select);

            var document = await parser.ParseDocumentAsync(dbNetComboResponse?.Options.ToString() ?? string.Empty);
            List<IHtmlOptionElement> options = document.QuerySelectorAll<IHtmlOptionElement>("option").ToList();
            Assert.Equal(3, options?.Count);

            request.FilterToken = "the%";
            dbNetComboResponse = await GetResponse(request, RequestAction.Filter);
            document = await parser.ParseDocumentAsync(dbNetComboResponse?.Options.ToString() ?? string.Empty);
            options = document.QuerySelectorAll<IHtmlOptionElement>("option").ToList();
            Assert.Equal(2, options?.Count);
            Assert.True(options?.First().Text.StartsWith("The"));

            request.FilterToken = "%holdings";
            dbNetComboResponse = await GetResponse(request, RequestAction.Filter);
            document = await parser.ParseDocumentAsync(dbNetComboResponse?.Options.ToString() ?? string.Empty);
            options = document.QuerySelectorAll<IHtmlOptionElement>("option").ToList();
            Assert.Equal(1, options?.Count);
            Assert.True(options?.First().Text.EndsWith("Holdings"));
        }

        [Fact]
        public async Task EmptyOptionTextTest()
        {
            DbNetComboRequest request = GetRequest();
            request.ValueColumn = EncodingHelper.Encode("CustomerId");
            request.TextColumn = EncodingHelper.Encode("CompanyName");
            request.EmptyOptionText = "Please select an customer ...";

            DbNetComboResponse? dbNetComboResponse = await GetResponse(request, RequestAction.Page);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetComboResponse?.Options.ToString() ?? string.Empty);
            List<IHtmlOptionElement> options = document.QuerySelectorAll<IHtmlOptionElement>("option").ToList();

            Assert.Equal(string.Empty, options?.First().Value);
            Assert.Equal(request.EmptyOptionText, options?.First().Text);
        }

        [Fact]
        public async Task ForeignKeyTest()
        {
            DbNetComboRequest request = GetRequest();
            request.ValueColumn = EncodingHelper.Encode("CustomerId");
            request.TextColumn = EncodingHelper.Encode("CompanyName");
            request.ForeignKeyColumn = EncodingHelper.Encode("region");
            request.ForeignKeyValue = new List<object>() { "North America" };

            DbNetComboResponse? dbNetComboResponse = await GetResponse(request, RequestAction.Page);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetComboResponse?.Options.ToString() ?? string.Empty);
            List<IHtmlOptionElement> options = document.QuerySelectorAll<IHtmlOptionElement>("option").ToList();

            Assert.Equal(16, options?.Count);
            Assert.Equal("BOTTM", options?.First().Value);
            Assert.Equal("Bottom-Dollar Markets", options?.First().Text);
            Assert.Equal("WHITC", options?.Last().Value);
            Assert.Equal("White Clover Markets", options?.Last().Text);
        }

        [Fact]
        public async Task DistinctTest()
        {
            DbNetComboRequest request = GetRequest();
            request.ValueColumn = EncodingHelper.Encode("Country");
            request.Distinct = true;

            DbNetComboResponse? dbNetComboResponse = await GetResponse(request, RequestAction.Page);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetComboResponse?.Options.ToString() ?? string.Empty);
            List<IHtmlOptionElement> options = document.QuerySelectorAll<IHtmlOptionElement>("option").ToList();

            Assert.Equal(21, options?.Count);
            Assert.Equal("Argentina", options?.First().Value);
            Assert.Equal("Argentina", options?.First().Text);
            Assert.Equal("Venezuela", options?.Last().Value);
            Assert.Equal("Venezuela", options?.Last().Text);
        }

        [Fact]
        public async Task SizeTest()
        {
            DbNetComboRequest request = GetRequest();
            request.ValueColumn = EncodingHelper.Encode("CustomerId");
            request.TextColumn = EncodingHelper.Encode("CompanyName");
            request.Size = 10;

            DbNetComboResponse? dbNetComboResponse = await GetResponse(request, RequestAction.Page);

            var parser = new HtmlParser();

            var document = await parser.ParseDocumentAsync(dbNetComboResponse?.Select.ToString() ?? string.Empty);
            IHtmlSelectElement? select = document.QuerySelector<IHtmlSelectElement>("select");

            Assert.Equal(request.Size, select?.Size);
            Assert.Equal(-1, select?.SelectedIndex);
            Assert.Equal(false, select?.IsMultiple);
        }

        [Fact]
        public async Task MultipleSelectTest()
        {
            DbNetComboRequest request = GetRequest();
            request.ValueColumn = EncodingHelper.Encode("CustomerId");
            request.TextColumn = EncodingHelper.Encode("CompanyName");
            request.Size = 10;
            request.MultipleSelect = true;

            DbNetComboResponse? dbNetComboResponse = await GetResponse(request, RequestAction.Page);

            var parser = new HtmlParser();

            var document = await parser.ParseDocumentAsync(dbNetComboResponse?.Select.ToString() ?? string.Empty);
            IHtmlSelectElement? select = document.QuerySelector<IHtmlSelectElement>("select");

            Assert.Equal(request.Size, select?.Size);
            Assert.Equal(true, select?.IsMultiple);
        }

        [Fact]
        public async Task DataOnlyColumnsTest()
        {
            var dataOnlyColumns = new List<string>() { "ReportsTo", "Title", "TitleOfCourtesy", "BirthDate", "HireDate", "Address", "City", "Region", "PostalCode", "Country", "HomePhone", "Extension" };

            DbNetComboRequest request = GetRequest("employees");
            request.ValueColumn = EncodingHelper.Encode("employeeid");
            request.TextColumn = EncodingHelper.Encode("lastname || ', ' || firstname");
            request.DataOnlyColumns = dataOnlyColumns.Select(c => EncodingHelper.Encode(c)).ToList();

            DbNetComboResponse? dbNetComboResponse = await GetResponse(request, RequestAction.Page);

            var parser = new HtmlParser();

            var document = await parser.ParseDocumentAsync(dbNetComboResponse?.Options.ToString() ?? string.Empty);
            List<IHtmlOptionElement> options = document.QuerySelectorAll<IHtmlOptionElement>("option").ToList();
            var dataset = options?.First()?.Dataset;

            foreach (var column in dataOnlyColumns) 
            {
                Assert.NotNull(dataset?[column.ToLower()]);
            }
        }
    }

}