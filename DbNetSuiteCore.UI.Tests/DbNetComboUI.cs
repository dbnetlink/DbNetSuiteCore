using System.Collections.ObjectModel;
using DbNetSuiteCore.Enums;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace DbNetSuiteCore.UI.Tests
{
    public class DbNetComboUI : DbNetSuiteUI, IClassFixture<WebApplicationFactoryFixture<Program>>
    {
        public DbNetComboUI(WebApplicationFactoryFixture<Program> factory) : base(factory)
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
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetcombo/FilteredCustomers");
                var input = GetFilter(driver);
                input.SendKeys("the");
                input.SendKeys(Keys.Enter);
                Thread.Sleep(1000);
                WaitForLoad(driver);
                var select = GetSelect(driver);
                Assert.Equal(3, select.Options.Count);
            }
        }

        private void WaitForLoad(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(driver =>
            {
                bool result = (bool)((IJavaScriptExecutor)driver).
                ExecuteScript($"return $('.dbnetsuite-loading:first').is(':visible') == false");
                return result;
            });
        }

        private SelectElement GetSelect(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            IWebElement element = wait.Until(driver => driver.FindElement(By.CssSelector("select.dbnetcombo")));
            return new SelectElement(element);
        }

        private IWebElement GetFilter(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            IWebElement element = wait.Until(driver => driver.FindElement(By.CssSelector("input.dbnetcombo")));
            return element;
        }
    }
}