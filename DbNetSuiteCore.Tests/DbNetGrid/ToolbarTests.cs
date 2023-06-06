using AngleSharp.Html.Parser;
using DbNetSuiteCore.Models;
using DbNetSuiteCore.Helpers;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using DbNetSuiteCore.Tests.Extensions;
using DbNetSuiteCore.Enums;
using AngleSharp.Text;

namespace DbNetSuiteCore.Tests.DbNetGrid
{
    public class ToolbarTests : DbNetGridTests
    {
        public ToolbarTests() : base() { }

        [Fact]
        public async Task UnconfiguredToolbarTest()
        {
            DbNetGridRequest request = GetRequest();

            DbNetGridResponse? dbNetGridResponse = await GetResponse(request, "initialize");

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetGridResponse!.Toolbar);
            var buttons = document.QuerySelectorAll("button");

            Assert.Equal(7, buttons.Length);
        }
        [Fact]
        public async Task ConfiguredToolbarTest()
        {
            DbNetGridRequest request = GetRequest();

            request.View = true;
            request.QuickSearch = true;
            request.Export = false;
            request.Copy = false;
            request.Navigation = false;

            DbNetGridResponse? dbNetGridResponse = await GetResponse(request, "initialize");

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetGridResponse!.Toolbar);
            var buttons = document.QuerySelectorAll("button");

            Assert.Equal(2, buttons.Length);
            Assert.Equal("_SearchBtn", buttons[0].Id);
            Assert.Equal("_ViewBtn", buttons[1].Id);

            var inputs = document.QuerySelectorAll("input");
            Assert.Equal(2, inputs.Length);
            Assert.Equal("QuickSearch", inputs[0].NameAttr());
            Assert.Equal("Rows", inputs[1].NameAttr());

            Assert.Equal(91, dbNetGridResponse.TotalRows);
            Assert.Equal(1, dbNetGridResponse.TotalPages);
            Assert.Equal(1, dbNetGridResponse.CurrentPage);
        }

        [Fact]
        public async Task ToolbarNavigationTest()
        {
            DbNetGridRequest request = GetRequest();
            DbNetGridResponse? dbNetGridResponse = await GetResponse(request, "initialize");

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(dbNetGridResponse!.Toolbar);
            var buttons = document.QuerySelectorAll("button");

            var inputs = document.QuerySelectorAll("input");
            Assert.Equal(3, inputs.Length);
            Assert.Equal("PageNumber", inputs[0].NameAttr());
            Assert.Equal("PageCount", inputs[1].NameAttr());
            Assert.Equal("Rows", inputs[2].NameAttr());
        }

        [Fact]
        public async Task ToolbarButtonStyleTest()
        {
            foreach (ToolbarButtonStyle toolbarButtonStyle in Enum.GetValues(typeof(ToolbarButtonStyle)))
            {
                DbNetGridRequest request = GetRequest();
                request.ToolbarButtonStyle = toolbarButtonStyle;

                DbNetGridResponse? dbNetGridResponse = await GetResponse(request, "initialize");

                var parser = new HtmlParser();
                var document = await parser.ParseDocumentAsync(dbNetGridResponse!.Toolbar);
                var buttons = document.QuerySelectorAll("button");
                
                foreach(var button in buttons)
                {
                    var classNames = button?.ClassName?.Split(' ');
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
    }
}


