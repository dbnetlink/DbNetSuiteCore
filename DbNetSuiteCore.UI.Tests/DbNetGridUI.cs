﻿using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using DbNetSuiteCore.Enums;
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
        public void SimpleTestDb()
        {
            SimpleTest(false);
        }
        [Fact]
        public void SimpleTestJson()
        {
            SimpleTest(true);
        }

        private void SimpleTest(bool json = false)
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetgrid/customers?jsonmode={json}");

                var table = GetTable(driver);
                var rows = GetTableBodyRows(table);

                Assert.Equal(20, rows.Count);
            }
        }

        [Fact]
        public void HeadingSortTestDb()
        {
            HeadingSortTest(false);
        }

        [Fact]
        public void HeadingSortTestJson()
        {
            HeadingSortTest(true);
        }
        private void HeadingSortTest(bool json = false)
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetgrid/customers?jsonmode={json}");

                var table = GetTable(driver);
                var headerRows = GetTableHeaderRows(table);
                var bodyRows = GetTableBodyRows(table);

                var firstCell = bodyRows.First().FindElements(By.TagName("td")).First();
                Assert.Equal("ALFKI", firstCell.Text);

                headerRows.First().FindElements(By.TagName("th")).First().Click();

                WaitForLoad(driver);

                table = GetTable(driver);
                bodyRows = GetTableBodyRows(table);

                firstCell = bodyRows.First().FindElements(By.TagName("td")).First();
                Assert.Equal("WOLZA", firstCell.Text);
            }
        }

        [Fact]
        public void StringSearchDialogTestSQLite()
        {
            StringSearchDialogTest(DataProvider.SQLite);
        }

        [Fact]
        public void StringSearchDialogTestMSSqlServer()
        {
            StringSearchDialogTest(DataProvider.SqlClient);
        }

        [Fact]
        public void StringSearchDialogTestJson()
        {
            StringSearchDialogTest(DataProvider.DataTable);
        }
        private void StringSearchDialogTest(DataProvider dataProvider)
        {
            List<SearchTemplate> searchTemplates = new List<SearchTemplate>
            {
                new SearchTemplate(SearchOperator.EqualTo, 1, "BOTTM"),
                new SearchTemplate(SearchOperator.NotEqualTo, 90, "BOTTM"),
                new SearchTemplate(SearchOperator.Contains, 4, "ER"),
                new SearchTemplate(SearchOperator.DoesNotContain, 87, "ER"),
                new SearchTemplate(SearchOperator.StartsWith, 3, "BO"),
                new SearchTemplate(SearchOperator.DoesNotStartWith, 88, "BO"),
                new SearchTemplate(SearchOperator.EndsWith, 2, "SH"),
                new SearchTemplate(SearchOperator.DoesNotEndWith, 89, "SH"),
                new SearchTemplate(SearchOperator.In, 3, "COMMI,EASTC,DRACD"),
                new SearchTemplate(SearchOperator.NotIn, 88, "COMMI,EASTC,DRACD"),
                new SearchTemplate(SearchOperator.GreaterThan, 9, "V"),
                new SearchTemplate(SearchOperator.LessThan, 82, "V"),
                new SearchTemplate(SearchOperator.Between, 6, "BLONP","CACTU"),
                new SearchTemplate(SearchOperator.NotBetween, 85, "BLONP","CACTU"),
                new SearchTemplate(SearchOperator.NotLessThan, 5, "WARTH"),
                new SearchTemplate(SearchOperator.NotGreaterThan, 87, "WARTH"),
                new SearchTemplate(SearchOperator.IsNull, 0),
                new SearchTemplate(SearchOperator.IsNotNull, 91)
            };

            ApplySearchTemplates(searchTemplates, "customers", "CustomerID", dataProvider);
        }

        [Fact]
        public void DateSearchDialogTestSQLite()
        {
            DateSearchDialogTest(DataProvider.SQLite);
        }

        [Fact]
        public void DateSearchDialogTestMSSQLServer()
        {
            DateSearchDialogTest(DataProvider.SqlClient);
        }

        [Fact]
        public void DateSearchDialogTestJson()
        {
            DateSearchDialogTest(DataProvider.DataTable);
        }

        private void DateSearchDialogTest(DataProvider dataProvider)
        {
            List<SearchTemplate> searchTemplates = new List<SearchTemplate>
            {
                new SearchTemplate(SearchOperator.EqualTo, 2, "12/2/2017"),
                new SearchTemplate(SearchOperator.NotEqualTo, 828, "12/2/2017"),
                new SearchTemplate(SearchOperator.In, 5, "6/5/2018,6/5/2017"),
                new SearchTemplate(SearchOperator.NotIn, 825, "6/5/2018,6/5/2017"),
                new SearchTemplate(SearchOperator.GreaterThan, 267, "1/1/2018"),
                new SearchTemplate(SearchOperator.LessThan, 560, "1/1/2018"),
                new SearchTemplate(SearchOperator.Between, 270, "1/1/2018","31/12/2018"),
                new SearchTemplate(SearchOperator.NotBetween, 560, "1/1/2018","31/12/2018"),
                new SearchTemplate(SearchOperator.NotLessThan, 678, "1/1/2017"),
                new SearchTemplate(SearchOperator.NotGreaterThan, 154, "1/1/2017"),
                new SearchTemplate(SearchOperator.IsNull, 0),
                new SearchTemplate(SearchOperator.IsNotNull, 830)
            };

            ApplySearchTemplates(searchTemplates, "orders", "OrderDate", dataProvider);
        }

        [Fact]
        public void DecimalSearchDialogTestSQLite()
        {
            DecimalSearchDialogTest(DataProvider.SQLite);
        }
        [Fact]
        public void DecimalSearchDialogTestMSSqlServer()
        {
            DecimalSearchDialogTest(DataProvider.SqlClient);
        }
        [Fact]
        public void DecimalSearchDialogTestJson()
        {
            DecimalSearchDialogTest(DataProvider.DataTable);
        }

        private void DecimalSearchDialogTest(DataProvider dataProvider)
        {
            List<SearchTemplate> searchTemplates = new List<SearchTemplate>
            {
                new SearchTemplate(SearchOperator.EqualTo, 16, "263.5"),
                new SearchTemplate(SearchOperator.NotEqualTo, 2139, "263.5"),
                new SearchTemplate(SearchOperator.In, 25, "81,64.8,36.8"),
                new SearchTemplate(SearchOperator.NotIn, 2130, "81,64.8,36.8"),
                new SearchTemplate(SearchOperator.GreaterThan, 46, "100"),
                new SearchTemplate(SearchOperator.LessThan, 2109, "100"),
                new SearchTemplate(SearchOperator.Between, 145, "50","150"),
                new SearchTemplate(SearchOperator.NotBetween, 2010, "50","150"),
                new SearchTemplate(SearchOperator.NotLessThan, 24, "200"),
                new SearchTemplate(SearchOperator.NotGreaterThan, 2131, "200"),
                new SearchTemplate(SearchOperator.IsNull, 0),
                new SearchTemplate(SearchOperator.IsNotNull, 2155)
            };

            ApplySearchTemplates(searchTemplates, "orderdetails", "UnitPrice", dataProvider);
        }

        [Fact]
        public void BooleanSearchDialogTestSQLite()
        {
            BooleanSearchDialogTest(DataProvider.SQLite);
        }

        [Fact]
        public void BooleanSearchDialogTestMSSqlServer()
        {
            BooleanSearchDialogTest(DataProvider.SqlClient);
        }

        [Fact]
        public void BooleanSearchDialogTestJson()
        {
            BooleanSearchDialogTest(DataProvider.DataTable);
        }

        private void BooleanSearchDialogTest(DataProvider dataProvider)
        {
            List<SearchTemplate> searchTemplates = new List<SearchTemplate>
            {
                new SearchTemplate(SearchOperator.True, 8),
                new SearchTemplate(SearchOperator.False, 69),
            };

            ApplySearchTemplates(searchTemplates, $"products", "Discontinued", dataProvider);
        }

        [Fact]
        public void NullSearchDialogTestSQLite()
        {
            NullSearchDialogTest(DataProvider.SQLite);
        }

        [Fact]
        public void NullSearchDialogTestMSSqlServer()
        {
            NullSearchDialogTest(DataProvider.SqlClient);
        }

        [Fact]
        public void NullSearchDialogTestJson()
        {
            NullSearchDialogTest(DataProvider.DataTable);
        }

        private void NullSearchDialogTest(DataProvider dataProvider)
        {
            List<SearchTemplate> searchTemplates = new List<SearchTemplate>
            {
                new SearchTemplate(SearchOperator.IsNull, 21),
                new SearchTemplate(SearchOperator.IsNotNull, 809)
            };

            ApplySearchTemplates(searchTemplates, "orders", "ShippedDate", dataProvider);
        }

        [Fact]
        public void NoToolbarTest()
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetgrid/notoolbar");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var element = wait.Until(driver => driver.FindElement(By.ClassName("dbnetgrid-table")));

                Assert.Equal(91, element.FindElements(By.CssSelector("tr.data-row")).Count);
            }
        }

        [Fact]
        public void ViewTestDb()
        {
            ViewTest(false);
        }

        [Fact]
        public void ViewTestJson()
        {
            ViewTest(true);
        }

        private void ViewTest(bool json = false)
        {
            using (var driver = WebDriver)
            {
                driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetgrid/view?jsonmode={json}");

                var toolbar = GetToolbar(driver);
                var viewButton = GetButton(toolbar, "view");
                viewButton.Click();
                var viewDialog = WaitForViewDialog(driver);
                Assert.NotNull(viewDialog);

                IWebElement div = viewDialog.FindElements(By.CssSelector($"div[data-columnname='lastname']")).First();
                //               IWebElement div = row.FindElement(By.CssSelector("div.view-dialog-value"));
                Assert.Equal("Davolio", div.Text);
                var parentCell = div.FindElement(By.XPath("./.."));
                Assert.Equal("Davolio", parentCell.GetAttribute("data-value"));
            }
        }

        private IWebElement GetToolbar(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            return wait.Until(driver => driver.FindElement(By.ClassName("dbnetsuite-toolbar")));
        }

        private IWebElement WaitForSearchDialog(IWebDriver driver)
        {
            return WaitForDialog(driver, "search");
        }

        private IWebElement WaitForViewDialog(IWebDriver driver)
        {
            return WaitForDialog(driver, "view");
        }

        private IWebElement WaitForDialog(IWebDriver driver, string dialogName)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(driver =>
            {
                bool result = (bool)((IJavaScriptExecutor)driver).
                ExecuteScript($"return $('.{dialogName}-dialog:first').is(':visible') == true");
                return result;
            });

            return driver.FindElement(By.CssSelector($".{dialogName}-dialog"));
        }


        private IWebElement? FindElementByAttributeName(ReadOnlyCollection<IWebElement> elements, string attributeName, string attributeValue)
        {
            foreach (IWebElement element in elements)
            {
                if ((element.GetAttribute(attributeName) ?? string.Empty).ToLower() == attributeValue.ToLower())
                { 
                    return element; 
                }
            }

            return null;
        }
        private IWebElement GetButton(IWebElement container, string buttonType)
        {
            return container.FindElement(By.CssSelector($"button[button-type='{buttonType}']"));
        }

        private IWebElement GetInput(IWebElement container, string name)
        {
            return container.FindElement(By.CssSelector($"input[name='{name}']"));
        }

        private void ApplySearchTemplates(List<SearchTemplate> searchTemplates, string page, string columnName, DataProvider dataProvider = DataProvider.SQLite)
        {
            page = page + $"?DataProvider={dataProvider}";

            using (var driver = WebDriver)
            {
                var searchDialog = GetSearchDialog(driver, page);
                var applyButton = GetButton(searchDialog, "apply");
                var clearButton = GetButton(searchDialog, "clear");

                ReadOnlyCollection<IWebElement> rows = searchDialog.FindElements(By.TagName("tr"));

                var colName = rows.First().GetAttribute("columnname");
                IWebElement? row = FindElementByAttributeName(rows, "columnname", columnName); 

                if (row == null)
                {
                    throw new Exception($"Search row for {columnName} not found");
                }
                SelectElement select = new SelectElement(row.FindElement(By.TagName("select")));
                ReadOnlyCollection<IWebElement> inputs = row.FindElements(By.TagName("input"));

                foreach (var searchTemplate in searchTemplates)
                {
                    clearButton.Click();
                    select.SelectByValue(searchTemplate.SearchOperator.ToString());

                    switch (searchTemplate.SearchOperator)
                    {
                        case SearchOperator.Between:
                        case SearchOperator.NotBetween:
                            inputs[0].SendKeys(searchTemplate.Token1);
                            inputs[1].SendKeys(searchTemplate.Token2);
                            break;
                        case SearchOperator.IsNull:
                        case SearchOperator.IsNotNull:
                        case SearchOperator.True:
                        case SearchOperator.False:
                            break;
                        default:
                            inputs.First().SendKeys(searchTemplate.Token1);
                            break;
                    }
                    applyButton.Click();
                    WaitForLoad(driver);
                    var toolbar = GetToolbar(driver);
                    var rowsInput = GetInput(toolbar, "Rows");
                    Assert.Equal(searchTemplate.ExpectedResult, Convert.ToInt32(rowsInput.GetAttribute("value")));
                }
            }
        }

        private IWebElement GetSearchDialog(IWebDriver driver, string page)
        {
            driver.Navigate().GoToUrl($"{_factory.HostUrl}/dbnetgrid/{page}");
            var toolbar = GetToolbar(driver);
            var searchButton = GetButton(toolbar, "search");
            searchButton.Click();
            return WaitForSearchDialog(driver);
        }
    }

    public class SearchTemplate
    {
        public SearchOperator SearchOperator { get; set; }
        public int ExpectedResult { get; set; }
        public string? Token1 { get; set; }
        public string? Token2 { get; set; }

        public SearchTemplate(SearchOperator searchOperator, int expectedResult, string? token1 = null, string? token2 = null)
        {
            SearchOperator = searchOperator;
            ExpectedResult = expectedResult;
            Token1 = token1;
            Token2 = token2;
        }
    }
}