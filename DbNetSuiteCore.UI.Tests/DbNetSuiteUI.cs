using System.Collections.ObjectModel;
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

        public DbNetSuiteUI(WebApplicationFactoryFixture<Program> factory, int? port = null)
        {
            if (port.HasValue)
            {
                factory.HostUrl = $"https://localhost:{port.Value}";
            }
            _factory = factory;
            factory.CreateDefaultClient();

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless"); // Run Chrome in headless mode (without UI)
            var chromeDriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            WebDriver = new ChromeDriver(chromeDriverPath, chromeOptions);
        }

        public void Dispose()
        {
            WebDriver.Dispose();
        }

        protected void WaitForLoad(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(driver =>
            {
                bool result = (bool)((IJavaScriptExecutor)driver).
                ExecuteScript($"return $('.dbnetsuite-loading:first').is(':visible') == false");
                return result;
            });
        }
        protected IWebElement GetTable(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            return wait.Until(driver => driver.FindElement(By.ClassName("dbnetgrid-table")));
        }
        protected ReadOnlyCollection<IWebElement> GetTableBodyRows(IWebElement table)
        {
            return GetTableSectionRows(table, "tbody");
        }
        protected ReadOnlyCollection<IWebElement> GetTableHeaderRows(IWebElement table)
        {
            return GetTableSectionRows(table, "thead");
        }
        protected ReadOnlyCollection<IWebElement> GetTableSectionRows(IWebElement table, string tagName)
        {
            return table.FindElement(By.TagName(tagName)).FindElements(By.TagName("tr"));
        }
    }
}