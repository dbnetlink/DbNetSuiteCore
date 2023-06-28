using System.Collections.ObjectModel;
using DbNetSuiteCore.Enums;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace DbNetSuiteCore.UI.Tests
{
    public class DbNetComboUI : DbNetSuiteUI, IClassFixture<WebApplicationFactoryFixture<Program>>
    {
        public DbNetComboUI(WebApplicationFactoryFixture<Program> factory) : base(factory, 7114)
        {
        }

        [Fact]
        public void SimpleTest()
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetcombo/simple");
                var select = GetSelect(driver);
                Assert.Equal(91, select.Options.Count);
            }
        }

        [Fact]
        public void SizeTest()
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetcombo/size");
                var select = GetSelect(driver);
                Assert.Equal(0, select.AllSelectedOptions.Count);
            }
        }

        [Fact]
        public void AutoRowSelectTest()
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetcombo/autorowselect");
                var select = GetSelect(driver);
                Assert.Equal(1, select.AllSelectedOptions.Count);
            }
        }

        [Fact]
        public void FilterTest()
        {
            List<KeyValuePair<string, int>> filterTests = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("the", 3),
                new KeyValuePair<string, int>("the%", 2),
                new KeyValuePair<string, int>("the*", 2),
                new KeyValuePair<string, int>("%es", 6)
          };

            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetcombo/FilteredCustomers");
                var input = GetFilter(driver);

                foreach (var filterTest in filterTests)
                {
                    input.Clear();
                    input.SendKeys(filterTest.Key);
                    input.SendKeys(Keys.Enter);
                    Thread.Sleep(1000);
                    WaitForLoad(driver);
                    var select = GetSelect(driver);
                    Assert.Equal(filterTest.Value, select.Options.Count);
                }
            }
        }

        [Fact]
        public void LinkedWithEmptyOption()
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetcombo/LinkedWithEmptyOption");
                Thread.Sleep(1000);
                var selects = GetSelects(driver);
                Assert.Equal(3, selects.Count);

                Assert.Equal(276, selects[0].Options.Count);
                Assert.Equal(0, selects[1].Options.Count);
                Assert.Equal(0, selects[2].Options.Count);

                selects[0].SelectByText("AC/DC");
                Thread.Sleep(1000);
                WaitForLoad(driver);
                selects = GetSelects(driver);
                Assert.Equal(3, selects[1].Options.Count);
                Assert.Equal(0, selects[2].Options.Count);

                selects[1].SelectByText("Let There Be Rock");
                Thread.Sleep(1000);
                WaitForLoad(driver);
                selects = GetSelects(driver);
                Assert.Equal(9, selects[2].Options.Count);
            }
        }

        [Fact]
        public void LinkedAutoRowSelect()
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetcombo/LinkedAutoRowSelect");
                Thread.Sleep(1000);
                var selects = GetSelects(driver);
                Assert.Equal(3, selects.Count);

                Assert.Equal(16, selects[0].Options.Count);
                Assert.Equal(64, selects[1].Options.Count);
                Assert.Equal(6, selects[2].Options.Count);
            }
        }

        [Fact]
        public void LinkedGrid()
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetcombo/LinkedGrid");
                Thread.Sleep(1000);
                var selects = GetSelects(driver);

                Assert.Equal(2, selects.Count);
                Assert.Equal(4, selects[0].Options.Count);
                Assert.Equal(19, selects[1].Options.Count);

                var table = GetTable(driver);
                var bodyRows = GetTableBodyRows(table);
                Assert.Equal(2, bodyRows.Count);

                selects[1].SelectByText("Bedford");
                selects[1].SelectByText("Boston");
                selects[1].SelectByText("Braintree");

                Thread.Sleep(1000);
                WaitForLoad(driver);
                table = GetTable(driver);
                bodyRows = GetTableBodyRows(table);
                Assert.Equal(6, bodyRows.Count);
            }
        }

        private SelectElement GetSelect(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            IWebElement element = wait.Until(driver => driver.FindElement(By.CssSelector("select.dbnetcombo")));
            return new SelectElement(element);
        }

        private List<SelectElement> GetSelects(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            ReadOnlyCollection<IWebElement> elements = wait.Until(driver => driver.FindElements(By.CssSelector("select.dbnetcombo")));
            return elements.Select(e => new SelectElement(e)).ToList();
        }

        private IWebElement GetFilter(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            IWebElement element = wait.Until(driver => driver.FindElement(By.CssSelector("input.dbnetcombo")));
            return element;
        }
    }
}