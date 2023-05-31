using System.Net;

namespace DbNetSuiteCore.Tests
{
    public class ResourceTests : DbNetSuiteCoreTests
    {
        public ResourceTests() : base() {}

        [Fact]
        public async Task TestGetCssStylesheet()
        {
            var response = await _client.GetAsync("/resource.dbnetsuite?action=css");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task TestGetCssStylesheetWithFont()
        {
            string fontFamily = "Helvetica";
            string fontSize = "Large";
            var response = await _client.GetAsync($"/resource.dbnetsuite?action=css&font-family={fontFamily}&font-size={fontSize}");
            string css = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains($"--main-font-size: {fontSize}", css);
            Assert.Contains($"--main-font-family: {fontFamily}", css);
        }
        [Fact]
        public async Task TestGetResourceNotFound()
        {
            var response = await _client.GetAsync("/resource.dbnetsuite?action=xxxxxx");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task TestGetClientScript()
        {
            var response = await _client.GetAsync("/resource.dbnetsuite?action=script");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task TestGetFont()
        {
            var response = await _client.GetAsync("/resource.dbnetsuite?action=font&name=material-icons");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task TestGetFontNotFound()
        {
            var response = await _client.GetAsync("/resource.dbnetsuite?action=font&name=xxxxxx");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task TestGetImage()
        {
            var response = await _client.GetAsync("/resource.dbnetsuite?action=image&name=ui-icons_444444_256x240.png");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task TestGetImageNotFound()
        {
            var response = await _client.GetAsync("/resource.dbnetsuite?action=image&name=xxxxxx.png");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
