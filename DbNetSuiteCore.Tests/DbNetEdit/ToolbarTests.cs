using AngleSharp.Html.Parser;
using DbNetSuiteCore.Tests.Extensions;
using DbNetSuiteCore.Enums;
using DbNetSuiteCore.Constants.DbNetEdit;
using DbNetSuiteCore.Models.DbNetEdit;
using DbNetSuiteCore.Helpers;

namespace DbNetSuiteCore.Tests.DbNetEdit
{
    public class ToolbarTests : DbNetEditTests
    {
        public ToolbarTests() : base() { }

        [Fact]
        public async Task UnconfiguredToolbarTest()
        {
            DbNetEditRequest request = GetRequest();

            DbNetEditResponse? DbNetEditResponse = await GetResponse(request, RequestAction.Initialize);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(DbNetEditResponse!.Toolbar);
            var buttons = document.QuerySelectorAll("button");

            Assert.Equal(7, buttons.Length);
        }
        [Fact]
        public async Task ConfiguredToolbarTest()
        {
            DbNetEditRequest request = GetRequest();
            request.QuickSearch = true;

            DbNetEditResponse? DbNetEditResponse = await GetResponse(request, RequestAction.Initialize);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(DbNetEditResponse!.Toolbar);
            var buttons = document.QuerySelectorAll("button");

            Assert.Equal(7, buttons.Length);
            Assert.Equal("_SearchBtn", buttons[0].Id);

            var inputs = document.QuerySelectorAll("input");
            Assert.Equal(3, inputs.Length);
            Assert.Equal("QuickSearch", inputs[0].NameAttr());
            Assert.Equal("RowNumber", inputs[1].NameAttr());

            Assert.Equal(91, DbNetEditResponse.TotalRows);
            Assert.Equal(1, DbNetEditResponse.CurrentRow);
        }

        [Fact]
        public async Task ToolbarNavigationTest()
        {
            DbNetEditRequest request = GetRequest();
            DbNetEditResponse? DbNetEditResponse = await GetResponse(request, RequestAction.Initialize);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(DbNetEditResponse!.Toolbar);

            var inputs = document.QuerySelectorAll("input");
            Assert.Equal(2, inputs.Length);
            Assert.Equal("RowNumber", inputs[0].NameAttr());
            Assert.Equal("RowCount", inputs[1].NameAttr());
        }

        [Fact]
        public async Task ToolbarButtonStyleTest()
        {
            foreach (ToolbarButtonStyle toolbarButtonStyle in Enum.GetValues(typeof(ToolbarButtonStyle)))
            {
                DbNetEditRequest request = GetRequest();
                request.ToolbarButtonStyle = toolbarButtonStyle;

                DbNetEditResponse? DbNetEditResponse = await GetResponse(request, RequestAction.Initialize);

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(DbNetEditResponse!.Toolbar);
                var buttons = document.QuerySelectorAll("button");
                
                foreach(var button in buttons)
                {
                    var classNames = button?.ClassList;
                    switch (toolbarButtonStyle)
                    {
                        case ToolbarButtonStyle.Image:
                            Assert.True(classNames?.Contains("toolbar-button-image"));
                            break;
                        case ToolbarButtonStyle.ImageAndText:
                            Assert.True(classNames?.Contains("toolbar-button-imageandtext"));
                            break;
                        case ToolbarButtonStyle.Text:
                            Assert.True(classNames?.Contains("toolbar-button-text"));
                            break;
                    }
                }
            }
        }

        [Fact]
        public async Task FixedFilterToolbarTest()
        {
            DbNetEditRequest request = GetRequest("products");
            request.FixedFilterSql = EncodingHelper.Encode("discontinued = @discontinued");
            request.FixedFilterParams["discontinued"] = true;

            DbNetEditResponse? DbNetEditResponse = await GetResponse(request, RequestAction.Initialize);

            Assert.Equal(8, DbNetEditResponse?.TotalRows);
            Assert.Equal(1, DbNetEditResponse?.CurrentRow);
        }

        [Fact]
        public async Task BrowseToolbarTest()
        {
            DbNetEditRequest request = GetRequest("products");

            request.Columns.Add(new EditColumn("ProductName") { Browse = true });
            request.Columns.Add(new EditColumn("UnitPrice"));
            request.Columns.Add(new EditColumn("UnitsInStock"));

            DbNetEditResponse? DbNetEditResponse = await GetResponse(request, RequestAction.Initialize);
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(DbNetEditResponse!.Toolbar);
            var buttons = document.QuerySelectorAll("button");
            Assert.Equal(8, buttons.Length);
            Assert.Equal("_BrowseBtn", buttons[5].Id);
        }
    }
}