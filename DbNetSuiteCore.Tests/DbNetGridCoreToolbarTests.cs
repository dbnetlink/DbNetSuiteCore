using AngleSharp.Html.Parser;
using System.Net;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Helpers;
using System.Text;
using System.Text.Json;

namespace DbNetSuiteCore.Tests
{
    public class DbNetGridCoreToolbarTests : DbNetSuiteCoreTests
    {
        public DbNetGridCoreToolbarTests() : base() {}

        [Fact]
        public async Task UnconfiguredToolbarTest()
        {
            DbNetGridRequest request = new DbNetGridRequest();
            request.FromPart = EncodingHelper.Encode("customers");
            request.ConnectionString = EncodingHelper.Encode("northwind");

            var json = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/dbnetgrid.dbnetsuite?action=initialize", json);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            /*
            var parser = new HtmlParser();
            var content = await response.Content.ReadAsStringAsync();
            var document = await parser.ParseDocumentAsync(content);

            // Assert specific elements or content in the HTML response
            var titleElement = document.QuerySelector("title");
            Assert.NotNull(titleElement);
            Assert.Equal("Expected Title", titleElement.TextContent);

            var headingElement = document.QuerySelector("h1");
            Assert.NotNull(headingElement);
            Assert.Equal("Expected Heading", headingElement.TextContent);
            */
        }
    }
}


