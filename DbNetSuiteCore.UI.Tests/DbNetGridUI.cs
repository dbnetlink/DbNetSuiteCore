using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace DbNetSuiteCore.UI.Tests
{
    public class DbNetGridUI : DbNetSuiteUI, IClassFixture<WebApplicationFactoryFixture<Program>>
    {
        public DbNetGridUI(WebApplicationFactoryFixture<Program> factory) : base(factory)
        {
        }

        [Fact]
        public void SimpleTest()
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/simple");

                var table = GetTable(driver);
                var rows = GetTableBodyRows(table);

                Assert.Equal(20, rows.Count);

                var firstCell = rows.First().FindElements(By.TagName("td")).First();
                Assert.Equal("ALFKI", firstCell.Text);

                table.FindElements(By.TagName("th")).First().Click();

                WaitForLoad(driver);

                table = GetTable(driver);
                rows = GetTableBodyRows(table);

                firstCell = rows.First().FindElements(By.TagName("td")).First();
                Assert.Equal("WOLZA", firstCell.Text);

                var toolbar = GetToolbar(driver);
                var searchButton = GetButton(toolbar,"search");
                searchButton.Click();
                var searchDialog = WaitForSearchDialog(driver);
                var applyButton = GetButton(searchDialog, "apply");

                SelectElement select = new SelectElement(searchDialog.FindElements(By.TagName("select")).First());
            }
        }

        [Fact]
        public void NoToolbarTest()
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/notoolbar");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var element = wait.Until(driver => driver.FindElement(By.ClassName("dbnetgrid-table")));

                Assert.Equal(91, element.FindElements(By.CssSelector("tr.data-row")).Count);
            }
        }
        private IWebElement GetTable(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            return wait.Until(driver => driver.FindElement(By.ClassName("dbnetgrid-table")));
        }

        private IWebElement GetToolbar(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            return wait.Until(driver => driver.FindElement(By.ClassName("dbnetgrid-toolbar")));
        }

        private ReadOnlyCollection<IWebElement> GetTableBodyRows(IWebElement table)
        {
            return GetTableSectionRows(table,"tbody");
        }
        private ReadOnlyCollection<IWebElement> GetTableHeaderRows(IWebElement table)
        {
            return GetTableSectionRows(table, "thead");
        }
        private ReadOnlyCollection<IWebElement> GetTableSectionRows(IWebElement table, string tagName)
        {
            return table.FindElement(By.TagName(tagName)).FindElements(By.CssSelector("tr.data-row"));
        }

        private void WaitForLoad(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(driver =>
            {
                bool result = (bool)((IJavaScriptExecutor)driver).
                ExecuteScript("return $('.dbnetgrid-loading:first').is(':visible') == false");
                return result;
            });
        }

        private IWebElement WaitForSearchDialog(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(driver =>
            {
                bool result = (bool)((IJavaScriptExecutor)driver).
                ExecuteScript("return $('.search-dialog:first').is(':visible') == true");
                return result;
            });

            return driver.FindElement(By.CssSelector(".search-dialog"));
        }

        private IWebElement GetButton(IWebElement container, string buttonType)
        {
            return container.FindElement(By.CssSelector($"button[button-type='{buttonType}']"));
        }
    }
}