using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace DbNetSuiteCore.UI.Tests
{
    public class DbNetSuiteUI : IClassFixture<WebApplicationFactoryFixture<Program>>, IDisposable
    {
        protected readonly WebApplicationFactoryFixture<Program> _factory;
        protected IWebDriver WebDriver { get; }

        public DbNetSuiteUI(WebApplicationFactoryFixture<Program> factory)
        {
            _factory = factory;
            factory.CreateDefaultClient();

            var chromeOptions = new ChromeOptions();
           // chromeOptions.AddArguments("--headless"); // Run Chrome in headless mode (without UI)
            var chromeDriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            WebDriver = new ChromeDriver(chromeDriverPath, chromeOptions);
        }

        public void Dispose()
        {
            WebDriver.Dispose();
        }
    }
}